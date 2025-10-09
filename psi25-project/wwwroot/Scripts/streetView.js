async function initMap() {
  const response = await fetch('/api/geocoding/default-streetview');
  const data = await response.json();

  const initialPosition = { lat: data.lat, lng: data.lng };

  new google.maps.StreetViewPanorama(
    document.getElementById('street-view'),
    {
      position: initialPosition,
      pov: { heading: 0, pitch: 0 },
      zoom: 1,
    }
  );

  const miniMap = new google.maps.Map(document.getElementById('mini-map'), {
    center: initialPosition,
    zoom: 3,
    streetViewControl: false,
    fullscreenControl: false,
    mapTypeControl: false,
  });

  const selectionMarker = new google.maps.Marker({
    map: miniMap,
    visible: false,
  });

  miniMap.addListener('click', (event) => {
    const lat = event.latLng.lat();
    const lng = event.latLng.lng();
    const position = { lat, lng };

    selectionMarker.setPosition(position);
    selectionMarker.setVisible(true);

    console.log('Selected coordinates:', {
      lat: lat.toFixed(5),
      lng: lng.toFixed(5),
    });
  });
}

window.onload = initMap;
