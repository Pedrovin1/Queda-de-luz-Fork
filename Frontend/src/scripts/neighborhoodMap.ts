export const fetchAllNeighborhoods = async(cityName: string):Promise<string[]> => {
  const query = `
    [out:json][timeout:25];
    area["name"="${cityName}"]["admin_level"="8"]->.searchArea;
    (
      node["place"~"suburb|neighbourhood"](area.searchArea);
    );
    out tags;
  `;

  const url = `https://overpass-api.de/api/interpreter?data=${encodeURIComponent(query)}`

  try{

    const response = await fetch(url);
    const data = await response.json()

    if(!data.elements) return [];

    console.log(data.elements)

    const names = data.elements
      .map((el: any) => el.tags.name)
      .filter((name: string | undefined): name is string => !!name);

    return Array.from<string>(new Set(names)).sort();

  }catch(e){
    console.error("Erro ao pegar bairros com Overpass: ", e);
    return [];
  }

}

const fetchNeighborhoodOutline = async (
  neighborhoodName: string,
): Promise<google.maps.LatLngLiteral[][]> => {
  const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(neighborhoodName)}&format=json&polygon_geojson=1&limit=1`

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

export const neighborhoodOutlines = async (
  map: google.maps.Map,
  neighborhoodNames: string[],
  cityName: string,
): Promise<google.maps.Polygon[]> => {
  const polygonsMap: google.maps.Polygon[] = []
  const allBounds = new google.maps.LatLngBounds()

  const fetchPromises = neighborhoodNames.map(async (name) => {
    const fullSearchName = `${name}, ${cityName}`
    const paths = await fetchNeighborhoodOutline(fullSearchName)

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
      polygonsMap.push(polygon)

      paths.forEach((path) => {
        path.forEach((point) => allBounds.extend(point))
      })
    }
  })

  await Promise.all(fetchPromises)

  if (polygonsMap.length > 0) {
    map.fitBounds(allBounds)
    map.setZoom((map.getZoom() || 14) - 0.5)
  }
  return polygonsMap
}
