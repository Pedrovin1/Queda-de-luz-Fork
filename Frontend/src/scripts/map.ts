/// <reference types="google.maps" />

export async function initMap(elementId: string) {
  const portoAlegre = {
    north: -29.930786,
    south: -30.269359,
    west: -51.299773,
    east: -51.01142,
  }

  const { Map } = (await google.maps.importLibrary('maps')) as google.maps.MapsLibrary

  await google.maps.importLibrary('geometry')

  const map = document.getElementById(elementId)

  const darkMode : google.maps.MapTypeStyle[] = [
    {
        elementType: "geometry", stylers: [{ color: "#242f3e"}]
    },
    {
        elementType: "labels.text.stroke", stylers: [{ color: "#242f3e"}]
    },
    {
        elementType: "labels.text.fill", stylers: [{ color: "#eeeae4"}]
    },
    {
        featureType: "administrative.locality", elementType: "labels.text.fill", stylers: [{ color: "#eeeae4"}]
    },
    {
        featureType: "poi", elementType: "labels.text.fill", stylers: [{ color: "#d59563"}]
    },
    {
        featureType: "poi.park", elementType: "geometry", stylers: [{ color: "#263c3f"}]
    },
    {
        featureType: "poi.park", elementType: "labels.text.fill", stylers: [{ color: "#6b9a76"}]
    },
    {
        featureType: "road", elementType: "geometry", stylers: [{ color: "#38414e"}]
    },
    {
        featureType: "road", elementType: "geometry.stroke", stylers: [{ color: "#212a37"}]
    },
    {
        featureType: "road", elementType: "labels.text.fill", stylers: [{ color: "#9ca5b3"}]
    },
    {
        featureType: "road.highway", elementType: "geometry", stylers: [{ color: "#38414e"}]
    },
    {
        featureType: "road.highway", elementType: "geometry.stroke", stylers: [{ color: "#212a37"}]
    },
    {
        featureType: "road.highway", elementType: "labels.text.fill", stylers: [{ color: "#9ca5b3"}]
    },
    {
        featureType: "water", elementType: "geometry", stylers: [{ color: "#17263c"}]
    },
    {
        featureType: "water", elementType: "labels.text.fill", stylers: [{ color: "#515c6d"}]
    },
    {
        featureType: "water", elementType: "labels.text.stroke", stylers: [{ color: "#17263c"}]
    },
    {
        featureType: "poi", stylers: [{ visibility: "off" }]
    },
    {
        featureType: "transit", stylers: [{ visibility: "off" }]
    }
  ]


  if (map) {
    const mapOutput = new Map(map, {
      center: { lat: -30.0351, lng: -51.17195 },
      zoom: 14,
      minZoom: 12,
      maxZoom: 16,
      restriction: {
        latLngBounds: portoAlegre,
        strictBounds: false,
      },
      styles: darkMode,
      mapTypeControl: false,
      streetViewControl: false,
      fullscreenControl: false,
    })

    try {
      const response = await fetch('portoalegre.geojson')
      const data = await response.json()

      const worldCoords: google.maps.LatLngLiteral[] = [
        { lat: -27, lng: -57 },
        { lat: -27, lng: -49 },
        { lat: -33, lng: -49 },
        { lat: -33, lng: -57 },
        { lat: -27, lng: -57 },
      ]

      const poaPaths: google.maps.LatLngLiteral[][] = [worldCoords]

      const processRing = (ring: [number, number][]): google.maps.LatLngLiteral[] => {
        let path = ring.map(([lng, lat]) => ({ lat, lng }))

        const area = google.maps.geometry.spherical.computeSignedArea(path)
        if (area < 0) {
          path.reverse()
        }
        return path
      }

      data.features.forEach((feature: any) => {
        const geom = feature.geometry
        if (geom?.type === 'MultiPolygon') {
          geom.coordinates.forEach((polygon: [number, number][][]) => {
            polygon.forEach((ring) => {
              if (ring.length >= 3) {
                poaPaths.push(processRing(ring))
              }
            })
          })
        } else if (geom?.type === 'Polygon') {
          geom.coordinates.forEach((ring: [number, number][]) => {
            if (ring.length >= 3) poaPaths.push(processRing(ring))
          })
        }
      })

      new google.maps.Polygon({
        paths: poaPaths,
        strokeColor: '#00d4ff',
        strokeOpacity: 0.2,
        strokeWeight: 1,
        fillColor: '#000000',
        fillOpacity: 0.85,
        map: mapOutput,
        clickable: false,
        zIndex: 999,
      })
    } catch (error) {
      console.error('Erro ao carregar dados GeoJSON', error)
    }

    return mapOutput
  }
}
