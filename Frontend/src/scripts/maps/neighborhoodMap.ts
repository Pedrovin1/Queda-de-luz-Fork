//Funções de gerenciamento de parametros de bairros

import { cacheManager } from '../utils/cacheManager'
import { safeFetch } from '../utils/clientApi'

let polygonsCleaner: Map<string, google.maps.Polygon> = new Map()

export interface NeighborhoodInfo {
  id: number;
  name: string;
} 

export const clearAllPolygons = () => {
  polygonsCleaner.forEach((p) => p.setMap(null))
  polygonsCleaner.clear()
}

export const fetchAllNeighborhoods = async (cityName: string): Promise<NeighborhoodInfo[]> => {
  if (!cityName) return []

  const cacheNeighborhoods = `${cityName}-neighborhoods`

  try {
    const cached = cacheManager.get<NeighborhoodInfo[]>(cacheNeighborhoods)
    if (cached) return cached
  } catch (e) {
    console.warn('Erro ao ler cache da lista de bairros.')
  }

  const query = `
    [out:json];
    area["name"="${cityName}"]["admin_level"="8"]->.searchArea;
    (
      relation["admin_level"="10"](area.searchArea);
      way["admin_level"="10"](area.searchArea);
    );
    out tags;
  `

  const url = `https://overpass-api.de/api/interpreter?data=${encodeURIComponent(query)}`

  //OBS: Overpass é uma api muito instavel, porem é a unica opção para dar get em uma lista de bairros
  //Nominatim apenas retorna a latitude e longitude, porem não seus nomes.

  try {
    const response = await safeFetch(url)
    const data = await response.json()

    if (!data.elements) return []

    console.log(data.elements)

    const neighborhoodPackage = data.elements
      .map((el: any) => ({
        id: el.id,
        name: el.tags.name
      }))
      .filter((n: NeighborhoodInfo) => n.name !== '')

    const sendNeighborhoods = [...new Map<string, NeighborhoodInfo>(neighborhoodPackage.map((n: NeighborhoodInfo) => [n.name, n])
      ).values()
    ].sort((a: NeighborhoodInfo, b:NeighborhoodInfo) => a.name.localeCompare(b.name))

    cacheManager.set(cacheNeighborhoods, sendNeighborhoods, 7)

    return sendNeighborhoods as NeighborhoodInfo[]
  } catch (e) {
    console.error('Erro ao pegar bairros com Overpass: ', e)
    return []
  }
}

const fetchNeighborhoodOutline = async (
  neighborhoodName: string,
): Promise<google.maps.LatLngLiteral[][]> => {
  const cacheOutlines = `outline-${neighborhoodName}`
  try {
    const cached = cacheManager.get<google.maps.LatLngLiteral[][]>(cacheOutlines)
    if (cached) return cached
  } catch (e) {
    console.warn('Erro ao pegar cache das bordas de bairro')
  }

  const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(neighborhoodName)}&format=json&polygon_geojson=1&limit=1`

  try {
    const response = await safeFetch(url)
    const data = await response.json()

    if (!data.length || !data[0].geojson) return []

    const geojson = data[0].geojson
    const paths: google.maps.LatLngLiteral[][] = []

    if (geojson.type === 'Polygon') {
      geojson.coordinates.forEach((ring: any) => {
        paths.push(ring.map(([lng, lat]: [number, number]) => ({ lat, lng })))
      })
    } else if (geojson.type === 'MultiPolygon') {
      geojson.coordinates.forEach((polygon: any) => {
        polygon.forEach((ring: any) => {
          paths.push(ring.map(([lng, lat]: [number, number]) => ({ lat, lng })))
        })
      })
    }
    cacheManager.set(cacheOutlines, paths, 7)
    return paths
  } catch (e) {
    console.error('Erro ao contornar cidade: ', e)
    return []
  }
}

export const neighborhoodOutlines = async (
  map: google.maps.Map,
  neighborhoodNames: string[],
  cityName: string,
  fixedCamera: boolean = true,
): Promise<void> => {
  const currentNameSet = new Set(neighborhoodNames)

  for (const [name, polygon] of polygonsCleaner.entries()) {
    if (!currentNameSet.has(name)) {
      polygon.setMap(null)
      polygonsCleaner.delete(name)
    }
  }

  const fetchPromises = neighborhoodNames
    .filter((name) => !polygonsCleaner.has(name))
    .map(async (name) => {
      const fullSearchName = `${name}, ${cityName}`
      return { name, paths: await fetchNeighborhoodOutline(fullSearchName) }
    })

  const result = await Promise.all(fetchPromises)

  const allBounds = new google.maps.LatLngBounds()

  result.forEach(({ name, paths }) => {
    if (paths.length <= 0) return
    if (paths.length > 0) {
      const polygon = new google.maps.Polygon({
        paths: paths,
        strokeColor: '#FF4500',
        strokeOpacity: 0.5,
        strokeWeight: 2,
        fillColor: '#FF4500',
        fillOpacity: 0.35,
        map: map,
        zIndex: 15,
      })

      polygonsCleaner.set(name, polygon)

      paths.forEach((path) => {
        path.forEach((point) => allBounds.extend(point))
      })
    }
  })

  if (polygonsCleaner.size > 0 && fixedCamera) {
    map.fitBounds(allBounds, 50)
  }
}
