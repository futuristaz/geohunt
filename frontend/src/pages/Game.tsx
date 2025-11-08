// src/pages/Game.tsx (StreetViewApp)
declare global {
  interface Window {
    google: any;
  }
}

import { useEffect, useRef, useState } from 'react';
import MiniMap from '../components/Minimap';
import { useNavigate } from 'react-router-dom';

interface Coordinates {
  lat: number;
  lng: number;
}

interface GeocodingApiResponse {
  id: number;
  modifiedCoordinates: Coordinates;
  panoID: string;
}

interface GameApiResponse {
  id: string;
  userId: string;
  totalScore: number;
  startedAt: string;
  finishedAt: string | null;
}

const StreetViewApp = () => {
  const streetViewRef = useRef<HTMLDivElement>(null);
  const panoRef = useRef<any>(null);

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const isInitializedRef = useRef(false);

  const [selectedCoords, setSelectedCoords] = useState<Coordinates | null>(null);
  const [initialCoords, setInitialCoords] = useState<Coordinates | null>(null);
  const [gameId, setGameId] = useState<string | null>(null);
  const [locationId, setLocationId] = useState<number | null>(null);

  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (isInitializedRef.current) return;
    isInitializedRef.current = true;

    const initializeStreetView = async () => {
      try {
        // Wait for Google Maps (max ~15s)
        await new Promise<void>((resolve, reject) => {
          const start = Date.now();
          const wait = () => {
            if (window.google?.maps) return resolve();
            if (Date.now() - start > 15000)
              return reject(new Error('Google Maps API not loaded'));
            setTimeout(wait, 100);
          };
          wait();
        });

        if (!streetViewRef.current) {
          throw new Error('Street View container not mounted');
        }

        // 1) Fetch coordinates
        const coordsRes = await fetch('/api/geocoding/valid_coords');
        if (!coordsRes.ok) throw new Error(`API request failed: ${coordsRes.status}`);
        const coordsData: GeocodingApiResponse = await coordsRes.json();

        const lat = parseFloat(String(coordsData.modifiedCoordinates.lat));
        const lng = parseFloat(String(coordsData.modifiedCoordinates.lng));
        if (Number.isNaN(lat) || Number.isNaN(lng)) {
          throw new Error('Invalid coordinates received from API');
        }
        const position: Coordinates = { lat, lng };
        setInitialCoords(position);

        // 2) Start a new game (check .ok before json)
        const gameRes = await fetch('/api/Game', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ userId: 'f5f925f5-4345-474e-a068-5169ab8bcb15' }),
        });
        if (!gameRes.ok) throw new Error(`Game API request failed: ${gameRes.status}`);
        const gameData: GameApiResponse = await gameRes.json();
        setGameId(gameData.id);

        // 3) Init Street View with the freshly fetched position
        panoRef.current = new window.google.maps.StreetViewPanorama(streetViewRef.current, {
          position,
          pov: { heading: 0, pitch: 0 },
          zoom: 1,
          addressControl: false,
          fullscreenControl: false,
          imageDateControl: false,
          showRoadLabels: false,
        });

        // 4) Persist the location (so guesses can reference it)
        const locRes = await fetch('/api/Locations', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ latitude: lat, longitude: lng, panoId: coordsData.panoID }),
        });
        if (!locRes.ok) throw new Error(`Location API request failed: ${locRes.status}`);
        const locData = await locRes.json();
        setLocationId(locData.id);

        setLoading(false);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load Street View';
        setError(errorMessage);
        setLoading(false);
        console.error('Street View initialization error:', err);
      }
    };

    initializeStreetView();
  }, []);

  const handleSubmitGuess = async () => {
    if (!selectedCoords || !initialCoords || !gameId || !locationId) {
      console.error('Missing required data:', { selectedCoords, initialCoords, gameId, locationId });
      return;
    }

    try {
      setSubmitting(true);

      // 1) Score this guess
      const resultResponse = await fetch('/api/result', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ guessedCoords: selectedCoords, initialCoords }),
      });
      if (!resultResponse.ok) throw new Error('Failed to submit guess');
      const resultData = await resultResponse.json(); // { score, distanceKm }

      // 2) Update total score on game
      const updateScoreResponse = await fetch(`/api/Game/${gameId}/score`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(resultData.score),
      });
      if (!updateScoreResponse.ok) throw new Error('Failed to update score');

      // 3) Persist guess
      const guessRes = await fetch('/api/Guess', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          gameId,
          locationId,
          guessedLatitude: selectedCoords.lat,
          guessedLongitude: selectedCoords.lng,
          score: resultData.score,
          distanceKm: resultData.distance,
        }),
      });
      if (!guessRes.ok) throw new Error('Failed to store guess');

      // 4) Finish game
      const finishRes = await fetch(`/api/Game/${gameId}/finish`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
      });
      if (!finishRes.ok) throw new Error('Failed to finish game');

      // 5) Navigate to Results with helpful state
      navigate(`/results/${gameId}`, {
        state: {
          gameId,
          lastScore: resultData.score,
          lastDistanceKm: resultData.distance,
          selectedCoords,
          initialCoords,
        },
        replace: true,
      });
    } catch (e) {
      console.error(e);
      alert((e as Error).message || 'Failed to finish the game');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="relative h-screen w-screen">
      {/* Street View container */}
      <div ref={streetViewRef} className="h-full w-full" />

      {/* Loading overlay */}
      {loading && !error && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/30 backdrop-blur-sm">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
            <p className="text-white">Loading Street View...</p>
            <p className="text-white/70 text-sm mt-2">Check console for details</p>
          </div>
        </div>
      )}

      {/* Error overlay */}
      {error && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="bg-white p-8 rounded-lg shadow-lg max-w-md">
            <h2 className="text-red-600 text-xl font-bold mb-2">Error</h2>
            <p className="text-gray-700">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Retry
            </button>
          </div>
        </div>
      )}

      {/* Mini map + submit */}
      <div className="absolute bottom-4 right-4 flex flex-col gap-2" style={{ zIndex: 9999 }}>
        <MiniMap
          initialZoom={1}
          onSelect={(coords) => {
            setSelectedCoords(coords);
            console.log('Selected coords from MiniMap:', coords);
          }}
          className="overflow-hidden shadow-lg rounded-xl"
          style={{ width: 300, height: 200 }}
        />
        <button
          className="px-3 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition disabled:opacity-60 disabled:cursor-not-allowed"
          onClick={handleSubmitGuess}
          disabled={!selectedCoords || submitting || !!error || loading}
        >
          {submitting ? 'Submitting…' : selectedCoords ? 'Submit Guess' : 'Select a location on the map'}
        </button>
      </div>
    </div>
  );
};

export default StreetViewApp;
