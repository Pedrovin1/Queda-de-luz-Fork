/// <reference types="google.maps" />
import { createCityMask, fetchCityBounds, fetchCityOutline } from './cityMap'
import { neighborhoodOutlines } from './neighborhoodMap'
import { addUserlocationMarker } from './userLocation'

export async function initMap(elementId: string, city: string, neighborhoods:string[]) {
  const { Map } = (await google.maps.importLibrary('maps')) as google.maps.MapsLibrary

  await google.maps.importLibrary('geometry')

  const map = document.getElementById(elementId)

  const [boundsCity, outlineCity] = await Promise.all([
    fetchCityBounds(city),
    fetchCityOutline(city),
  ])

  if (map) {
    const mapOutput = new Map(map, {
      mapId: 'f469ad8968c9354b9e051197',
      center: { lat: -30.0351, lng: -51.17195 },
      zoom: 14,
      minZoom: 12,
      maxZoom: 16,
      restriction: {
        latLngBounds: boundsCity,
        strictBounds: false,
      },
      mapTypeControl: false,
      streetViewControl: false,
      fullscreenControl: false,
      clickableIcons: false,
    })

    createCityMask(mapOutput, outlineCity)

    await addUserlocationMarker(mapOutput, outlineCity)

    await neighborhoodOutlines(mapOutput, neighborhoods, city)

    return mapOutput
  }
}
