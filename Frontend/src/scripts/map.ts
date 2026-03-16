/// <reference types="google.maps" />
import { createCityMask, fetchCityBounds, fetchCityOutline } from './cityMap'

const darkMode: google.maps.MapTypeStyle[] = [
  {
    elementType: 'geometry',
    stylers: [{ color: '#242f3e' }],
  },
  {
    elementType: 'labels.text.stroke',
    stylers: [{ color: '#242f3e' }],
  },
  {
    elementType: 'labels.text.fill',
    stylers: [{ color: '#eeeae4' }],
  },
  {
    featureType: 'administrative.locality',
    elementType: 'labels.text.fill',
    stylers: [{ color: '#eeeae4' }],
  },
  {
    featureType: 'poi',
    elementType: 'labels.text.fill',
    stylers: [{ color: '#d59563' }],
  },
  {
    featureType: 'poi.park',
    elementType: 'geometry',
    stylers: [{ color: '#263c3f' }],
  },
  {
    featureType: 'poi.park',
    elementType: 'labels.text.fill',
    stylers: [{ color: '#6b9a76' }],
  },
  {
    featureType: 'road',
    elementType: 'geometry',
    stylers: [{ color: '#38414e' }],
  },
  {
    featureType: 'road',
    elementType: 'geometry.stroke',
    stylers: [{ color: '#212a37' }],
  },
  {
    featureType: 'road',
    elementType: 'labels.text.fill',
    stylers: [{ color: '#9ca5b3' }],
  },
  {
    featureType: 'road.highway',
    elementType: 'geometry',
    stylers: [{ color: '#38414e' }],
  },
  {
    featureType: 'road.highway',
    elementType: 'geometry.stroke',
    stylers: [{ color: '#212a37' }],
  },
  {
    featureType: 'road.highway',
    elementType: 'labels.text.fill',
    stylers: [{ color: '#9ca5b3' }],
  },
  {
    featureType: 'water',
    elementType: 'geometry',
    stylers: [{ color: '#17263c' }],
  },
  {
    featureType: 'water',
    elementType: 'labels.text.fill',
    stylers: [{ color: '#515c6d' }],
  },
  {
    featureType: 'water',
    elementType: 'labels.text.stroke',
    stylers: [{ color: '#17263c' }],
  },
  {
    featureType: 'poi',
    stylers: [{ visibility: 'off' }],
  },
  {
    featureType: 'transit',
    stylers: [{ visibility: 'off' }],
  },
]

export async function initMap(elementId: string) {
  const { Map } = (await google.maps.importLibrary('maps')) as google.maps.MapsLibrary

  await google.maps.importLibrary('geometry')

  const map = document.getElementById(elementId)

  const city = 'Porto Alegre'

  const [boundsCity, outlineCity] = await Promise.all([
    fetchCityBounds(city),
    fetchCityOutline(city),
  ])

  if (map) {
    const mapOutput = new Map(map, {
      center: { lat: -30.0351, lng: -51.17195 },
      zoom: 14,
      minZoom: 12,
      maxZoom: 16,
      restriction: {
        latLngBounds: boundsCity,
        strictBounds: false,
      },
      styles: darkMode,
      mapTypeControl: false,
      streetViewControl: false,
      fullscreenControl: false,
    })

    createCityMask(mapOutput, outlineCity)

    return mapOutput
  }
}
