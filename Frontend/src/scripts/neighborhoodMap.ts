//Funções de gerenciamento de parametros de bairros

import { cacheManager } from "./cacheManager"
import { safeFetch } from "./clientApi"

let polygonsCleaner: google.maps.Polygon[] = []

const clearNeighborhoodPolygons = () => {
  polygonsCleaner.forEach((p) => p.setMap(null))
  polygonsCleaner = []
}

export const fetchAllNeighborhoods = async (cityName: string): Promise<string[]> => {
  if (!cityName) return []

  const cacheNeighborhoods = `${cityName}-neighborhoods`

  try {
    const cached = cacheManager.get<string[]>(cacheNeighborhoods)
    if (cached) return cached
  } catch (e) {
    console.warn('Erro ao ler cache da lista de bairros.')
  }

  const query = `
    [out:json][timeout:25];
    area["name"="${cityName}"]["admin_level"="8"]->.searchArea;
    (
      node["place"~"suburb|neighbourhood"](area.searchArea);
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

    const names = data.elements
      .map((el: any) => el.tags.name)
      .filter((name: string | undefined): name is string => !!name)

    const sendNeighborhoods = Array.from<string>(new Set(names)).sort()

    cacheManager.set(cacheNeighborhoods, sendNeighborhoods, 7);

    return sendNeighborhoods
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
): Promise<google.maps.Polygon[]> => {
  const polygonsMap: google.maps.Polygon[] = []
  const allBounds = new google.maps.LatLngBounds()

  const fetchPromises = neighborhoodNames.map(async (name) => {
    const fullSearchName = `${name}, ${cityName}`
    return { name, paths: await fetchNeighborhoodOutline(fullSearchName) }
  })

  const result = await Promise.all(fetchPromises)

  clearNeighborhoodPolygons()

  result.forEach(({ paths }) => {
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
      polygonsCleaner.push(polygon)
      polygonsMap.push(polygon)

      paths.forEach((path) => {
        path.forEach((point) => allBounds.extend(point))
      })
    }
  })

  if (polygonsMap.length > 0 && fixedCamera) {
    map.fitBounds(allBounds)
  }
  return polygonsMap
}
