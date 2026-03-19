const fetchNeighborhoodLocation = async(lat: number, lng: number): Promise<string> => {
    const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`;

    try{
        const response = await fetch(url);
        const data = await response.json();

        return data.address.suburb || data.address.neighborhood;
    }catch(e){
        console.error("Erro ao pegar local de usuario para o marcador: ", e);
        return "Local atual";
    }
}

const userLocationContainer = (neighborhoodName: string) => {
  const container = document.createElement('div')
  container.className = 'user-location-container'

  const balloon = document.createElement('dov');
  balloon.className = 'user-location-balloon';
  balloon.textContent = `Você está em: ${neighborhoodName}`;

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
  cityBounds: google.maps.LatLngBounds,
) => {
  const { AdvancedMarkerElement } = (await google.maps.importLibrary(
    'marker',
  )) as google.maps.MarkerLibrary

  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition( 
      async (position: GeolocationPosition) => {
        const userPos = {
          lat: position.coords.latitude,
          lng: position.coords.longitude,
        }

        if (cityBounds.contains(userPos)) {

            const neightborhoodName = await fetchNeighborhoodLocation(userPos.lat, userPos.lng);

            new AdvancedMarkerElement({
                map: map,
                position: userPos,
                content: userLocationContainer(neightborhoodName),
                title: 'Sua Localização',
                zIndex: 30,
            })
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
