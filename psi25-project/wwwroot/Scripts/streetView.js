async function initMap() {
  
  const response = await fetch('/api/geocoding/default-streetview');

  const data = await response.json();

  new google.maps.StreetViewPanorama(
    document.getElementById("street-view"),
        {
          position: { lat: data.lat, lng: data.lng },
          pov: { heading: 0, pitch: 0 },
          zoom: 1
        }
  );
}

window.onload = initMap;
