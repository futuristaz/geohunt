// Scripts/streetView.js
const LAT = 40.748817;   // Latitude
const LNG = -73.985428;  // Longitude
const HEADING = 165;    
const PITCH = 0;     
const ZOOM = 1;          

function initMap() {
  const position = { lat: LAT, lng: LNG };

  const panorama = new google.maps.StreetViewPanorama(
    document.getElementById("street-view"),
    {
      position: position,
      pov: { heading: HEADING, pitch: PITCH },
      zoom: ZOOM,
    }
  );
}
