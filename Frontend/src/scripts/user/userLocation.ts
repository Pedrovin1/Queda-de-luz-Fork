//Funções de gerenciamento de parametros da cidade

import { cacheManager } from './utils/cacheManager'
import { safeFetch } from './utils/clientApi'

interface UserLocation {
  city: string
  neighborhood: string
}

export const fetchAllLocation = async (lat: number, lng: number) => {
  const fixedLat = lat.toFixed(4)
  const fixedLng = lng.toFixed(4)

  const cacheLocation = `location-${fixedLat}-${fixedLng}`

  try {
    const cached = cacheManager.get<{ city: string; neighborhood: string }>(cacheLocation)
    if (cached) return cached
  } catch (e) {
    console.warn('Erro ao pegar cache das bordas de bairro')
  }

  const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`

  try {
    const response = await safeFetch(url)
    const data = await response.json()

    const locationData: UserLocation = {
      city: data.address.city || data.address.town || data.address.village,
      neighborhood: data.address.suburb || data.address.neighborhood,
    }

    cacheManager.set(cacheLocation, locationData, 2)
    return locationData
  } catch (e) {
    console.error('Erro ao capturar geolocalização:', e)
    return null
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
) => {
  const { AdvancedMarkerElement } = (await google.maps.importLibrary(
    'marker',
  )) as google.maps.MarkerLibrary

  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (position: GeolocationPosition) => {
        const userPos = new google.maps.LatLng(position.coords.latitude, position.coords.longitude)

        const cityPolygon = new google.maps.Polygon({ paths: cityBounds })

        const locationData = await fetchAllLocation(userPos.lat(), userPos.lng())

        if (!locationData) {
          console.warn('Não foi possivel carregar os dados de localização para o marcador')
          return
        } else {
          window.dispatchEvent(
            new CustomEvent('location-detected', {
              detail: { city: locationData.city, neighborhood: locationData.neighborhood },
            }),
          )
        }

        if (google.maps.geometry.poly.containsLocation(userPos, cityPolygon)) {
          new AdvancedMarkerElement({
            map: map,
            position: userPos,
            content: userLocationContainer(locationData.neighborhood),
            title: 'Sua Localização',
            zIndex: 30,
          })
          window.dispatchEvent(
            new CustomEvent('neighborhood-detected', {
              detail: { name: locationData.neighborhood },
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
