/// <reference types="google.maps" />
import { createCityMask, fetchCityBounds, fetchCityOutline } from './cityMap'
import { neighborhoodOutlines } from './neighborhoodMap'
import { addUserlocationMarker, fetchAllLocation } from '../user/userLocation'

//Funções para inicialização e customização do mapa

export async function initMap(elementId: string, city: string, neighborhoods: string[]) {
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

    mapOutput.addListener('click', async (e: google.maps.MapMouseEvent) => {
      if (e.latLng) {
        const lat = e.latLng.lat()
        const lng = e.latLng.lng()

        const neighborhoodClicked = await fetchAllLocation(lat, lng)

        window.dispatchEvent(
          new CustomEvent('map-neighborhood-clicked', {
            detail: { name: neighborhoodClicked?.neighborhood },
          }),
        )
      }
    })

    createCityMask(mapOutput, outlineCity)

    await neighborhoodOutlines(mapOutput, neighborhoods, city)

    await addUserlocationMarker(mapOutput, outlineCity)

    return mapOutput
  }
}
