document.addEventListener('DOMContentLoaded', () => {
    const gameData = JSON.parse(sessionStorage.getItem('guessData'));
    const distance = sessionStorage.getItem('distance');
    console.log('Distance from sessionStorage:', distance);

    if (!gameData) {
        console.error('No game data found in sessionstorage');
        return;
    };

    document.getElementById('init-lat').textContent = `Initial Latitude: ${gameData.initialCoords.lat.toFixed(5)}`;
    document.getElementById('init-lng').textContent = `Initial Longitude: ${gameData.initialCoords.lng.toFixed(5)}`;
    document.getElementById('guessed-lat').textContent = `Guessed Latitude: ${gameData.guessedCoords.lat.toFixed(5)}`;
    document.getElementById('guessed-lng').textContent = `Guessed Longitude: ${gameData.guessedCoords.lng.toFixed(5)}`;
    document.getElementById('distance').textContent = `Distance: ${distance} kilometers`;
});