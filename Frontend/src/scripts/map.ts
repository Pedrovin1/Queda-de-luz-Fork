/// <reference types="google.maps" />  

export async function initMap(elementId: string) {

    const portoAlegre = {
        north: -29.930786,
        south: -30.269359,
        west: -51.299773,
        east: -51.011420,
    }

    const {Map} = await google.maps.importLibrary("maps") as google.maps.MapsLibrary;

    await google.maps.importLibrary("geometry");

    const map = document.getElementById(elementId);

    if(map) {
        const mapOutput = new Map(map, {
            center: { lat: -30.03510, lng: -51.17195 },
            zoom: 14,
            minZoom: 12,
            maxZoom: 16,
            restriction: {
                latLngBounds: portoAlegre,
                strictBounds: false,
            },
            styles: [
                {
                    featureType: "poi", stylers: [{ visibility: "off" }]
                },
                {
                    featureType: "transit", stylers: [{ visibility: "off" }]   
                }
            ],
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: true,
        });

        try{
            const response = await fetch("portoalegre.geojson");
            const data = await response.json();

            const worldCoords: google.maps.LatLngLiteral[] = [
                { lat: -27, lng: -57 },
                { lat: -27, lng: -49 },
                { lat: -33, lng: -49 },
                { lat: -33, lng: -57 },
                { lat: -27, lng: -57 },
            ]

            const poaPaths: google.maps.LatLngLiteral[][] = [worldCoords];

            const processRing = (ring: [number, number][]): google.maps.LatLngLiteral[] => {
                let path = ring.map(([lng, lat]) => ({ lat, lng }));

                const area = google.maps.geometry.spherical.computeSignedArea(path);
                if(area < 0) {
                    path.reverse();
                }
                return path;
            }

            
            data.features.forEach((feature: any) => {
                const geom = feature.geometry;
                if(geom?.type === "MultiPolygon") {
                    geom.coordinates.forEach((polygon: [number, number][][]) => {
                        polygon.forEach((ring) => {
                            if(ring.length >= 3) {
                                poaPaths.push(processRing(ring));
                            }
                        })
                    })
                } else if(geom?.type === "Polygon") {
                    geom.coordinates.forEach((ring: [number, number][]) => {
                        if (ring.length >= 3) poaPaths.push(processRing(ring));
                    });
                }
            });

            new google.maps.Polygon({
                paths: poaPaths,
                strokeColor: "darkgray",
                strokeOpacity: 0.2,
                strokeWeight: 1,
                fillColor: "gray",
                fillOpacity: 0.8,
                map: mapOutput,
                clickable: false,
                zIndex: 999
            });
            
        }catch(error) {
            console.error("Erro ao carregar dados GeoJSON", error);
        }

        return mapOutput;
    }
}