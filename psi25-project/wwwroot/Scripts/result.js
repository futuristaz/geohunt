document.addEventListener('DOMContentLoaded', () => {
    const gameData = JSON.parse(sessionStorage.getItem('guessData'));
    const result = JSON.parse(sessionStorage.getItem('result'));

    document.getElementById('init-lat').textContent = `Initial latitude: ${gameData.initialCoords.lat.toFixed(5)}`;
    document.getElementById('init-lng').textContent = `Initial longitude: ${gameData.initialCoords.lng.toFixed(5)}`;
    document.getElementById('guessed-lat').textContent = `Guessed latitude: ${gameData.guessedCoords.lat.toFixed(5)}`;
    document.getElementById('guessed-lng').textContent = `Guessed longitude: ${gameData.guessedCoords.lng.toFixed(5)}`;
    document.getElementById('distance').textContent = `Distance: ${result.distance} kilometers`;
    document.getElementById('score').textContent = `Score: ${result.score} points`;
});