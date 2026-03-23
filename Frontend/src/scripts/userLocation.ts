//Funções de gerenciamento de parametros da cidade

export const fetchAllLocation = async (lat: number, lng: number) => {
  const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`
  try {
    const response = await fetch(url)
    const data = await response.json()

    const detectCity = data.address.city || data.address.town || data.address.village
    const detectNeighborhood = data.address.suburb || data.address.neighborhood

    return { city: detectCity, neighborhood: detectNeighborhood }
  } catch (e) {
    console.error('Erro ao capturar geolocalização:', e)
  }
}

export const fetchNeighborhoodLocation = async (lat: number, lng: number): Promise<string> => {
  const fixedLat = lat.toFixed(4);
  const fixedLng = lng.toFixed(4);
  const cachelocation = `location-${fixedLat}-${fixedLng}`
  
  try{
    const cached = localStorage.getItem(cachelocation)
    if(cached) return JSON.parse(cached)
  }catch(e){
    console.warn("Erro ao pegar cache das bordas de bairro")
  }
  
  const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`

  try {
    const response = await fetch(url)
    const data = await response.json()

    const neighborhoodName = data.address.suburb || data.address.neighborhood

    try{  
      localStorage.setItem(cachelocation, JSON.stringify(neighborhoodName));
    }catch(e){
      console.error("Não foi possivel guardar em cache o local do bairro.", e)
    }

    return neighborhoodName
  } catch (e) {
    console.error('Erro ao pegar local de usuario para o marcador: ', e)
    return 'Local atual'
  }
}

const userLocationContainer = (neighborhoodName: string) => {
  const container = document.createElement('div')
  container.className = 'user-location-container'

  const balloon = document.createElement('div')
  balloon.className = 'user-location-balloon'
  balloon.textContent = `Você está em: ${neighborhoodName}`

  const dot = document.createElement('div')
  dot.className = 'user-location-dot'

  const pulse = document.createElement('div')
  pulse.className = 'user-location-pulse'

  container.appendChild(balloon)
  container.appendChild(pulse)
  container.appendChild(dot)

  return container
}

export const addUserlocationMarker = async (
  map: google.maps.Map,
  cityBounds: google.maps.LatLngLiteral[][],
)  => {
  const { AdvancedMarkerElement } = (await google.maps.importLibrary(
    'marker',
  )) as google.maps.MarkerLibrary

  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (position: GeolocationPosition) => {
        const userPos = new google.maps.LatLng(position.coords.latitude, position.coords.longitude)

        const cityPolygon = new google.maps.Polygon({ paths: cityBounds })

        const locationData = await fetchAllLocation(userPos.lat(), userPos.lng())

        if (locationData) {
          window.dispatchEvent(
            new CustomEvent('location-detected', {
              detail: { city: locationData.city, neighborhood: locationData.neighborhood },
            }),
          )
        }

        if (google.maps.geometry.poly.containsLocation(userPos, cityPolygon)) {
          const neightborhoodName = await fetchNeighborhoodLocation(userPos.lat(), userPos.lng())

          new AdvancedMarkerElement({
            map: map,
            position: userPos,
            content: userLocationContainer(neightborhoodName),
            title: 'Sua Localização',
            zIndex: 30,
          })
          window.dispatchEvent(
            new CustomEvent('neighborhood-detected', {
              detail: { name: neightborhoodName },
            }),
          )
        } else {
          console.log('Usuario localizado fora dos limites da cidade.')
        }
      },
      (error) => {
        console.warn('Erro ao obter geolocalização ou permissão negada: ', error.message)
      },
      { enableHighAccuracy: true, timeout: 5000, maximumAge: 0 },
    )
  } else {
    console.error('Geolocalização não suportada pelo navegador.')
  }
}
