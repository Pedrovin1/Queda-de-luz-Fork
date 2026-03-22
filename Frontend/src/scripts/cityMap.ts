const worldAmericaCoords: google.maps.LatLngLiteral[] = [
  { lat: 15, lng: -95 },
  { lat: 15, lng: -30 },
  { lat: -58, lng: -30 },
  { lat: -58, lng: -95 },
  { lat: 15, lng: -95 },
]

export const fetchCityBounds = async (cityName: string): Promise<google.maps.LatLngBounds> => {
  const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(cityName)}&format=json&limit=1`

  try {
    const response = await fetch(url)
    const data = await response.json()

    if (!data.length) throw new Error('Local não encontrado')

    const [south, north, west, east] = data[0].boundingbox.map(Number)

    return new google.maps.LatLngBounds({ lat: south, lng: west }, { lat: north, lng: east })
  } catch (e) {
    console.error('Erro ao puxar dados da cidade: ', e)

    return new google.maps.LatLngBounds(
      { lat: -30.269359, lng: -51.299773 },
      { lat: -29.930786, lng: -51.01142 },
    )
  }
}

export const fetchCityOutline = async (
  cityName: string,
): Promise<google.maps.LatLngLiteral[][]> => {
  const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(cityName)}&format=json&polygon_geojson=1&limit=1`

  try {
    const response = await fetch(url)
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
    return paths
  } catch (e) {
    console.error('Erro ao contornar cidade: ', e)
    return []
  }
}

export const createCityMask = (map: google.maps.Map, cityPath: google.maps.LatLngLiteral[][]) => {
  const maskPaths: google.maps.LatLngLiteral[][] = [worldAmericaCoords]

  const processRing = (path: google.maps.LatLngLiteral[]): google.maps.LatLngLiteral[] => {
    const area = google.maps.geometry.spherical.computeSignedArea(path)
    if (area < 0) path.reverse()
    return path
  }

  cityPath.forEach((path) => {
    if (path.length >= 3) {
      maskPaths.push(processRing(path))
    }
  })

  return new google.maps.Polygon({
    paths: maskPaths,
    strokeColor: '#00d4ff',
    strokeOpacity: 0.3,
    strokeWeight: 2,
    fillColor: '#000000',
    fillOpacity: 0.85,
    map: map,
    clickable: false,
    zIndex: 10,
  })
}
