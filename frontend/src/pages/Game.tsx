// src/pages/Game.tsx (StreetViewApp)
declare global {
  interface Window {
    google: any;
  }
}

import { useEffect, useRef, useState } from 'react';
import MiniMap from '../components/MiniMap';
import { useNavigate, useLocation } from 'react-router-dom';

interface Coordinates {
  lat: number;
  lng: number;
}

interface GeocodingApiResponse {
  id: number;
  modifiedCoordinates: Coordinates;
  panoID: string;
}

type GameStateFromStart = {
  gameId: string;
  totalRounds: number;
  currentRound: number; // starts at 1
};

type GuessPostResponse = {
  guess: {
    id: string;
    gameId: string;
    locationId: number;
    roundNumber: number;
    guessedLatitude: number;
    guessedLongitude: number;
    distanceKm: number;
    score: number;
  };
  finished: boolean;
  currentRound: number; // new current round (advanced or same if finished)
  totalScore: number;
};

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

  const [round, setRound] = useState<number>(1);
  const [totalRounds, setTotalRounds] = useState<number>(3);
  const [submitting, setSubmitting] = useState(false);

  const navigate = useNavigate();
  const location = useLocation();

  // ---- utility: handle API errors consistently ----
  const handleApiError = async (response: Response, context: string) => {
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      if (errorData.code === 'MAPS_UNAVAILABLE') {
        throw new Error(`Couldn't ${context}. The map service is temporarily unavailable. Please retry in a moment.`);
      }
      throw new Error(`${context} failed: ${response.status}`);
    }
  };

  // ---- read state passed from Start.tsx ----
  useEffect(() => {
    const s = (location.state || {}) as Partial<GameStateFromStart>;
    if (!s.gameId) {
      navigate('/start', { replace: true });
      return;
    }
    setGameId(s.gameId);
    setRound(s.currentRound ?? 1);
    setTotalRounds(s.totalRounds ?? 3);
  }, [location.state, navigate]);

  // ---- utility: wait for Google Maps ----
  const waitGoogleMaps = () =>
    new Promise<void>((resolve, reject) => {
      const start = Date.now();
      const tick = () => {
        if (window.google?.maps) return resolve();
        if (Date.now() - start > 15000)
          return reject(new Error('Google Maps API not loaded'));
        setTimeout(tick, 100);
      };
      tick();
    });

  // ---- load a new location: fetch coords, init/retarget pano, persist Location ----
  const loadRoundLocation = async () => {
    // 1) Fetch coordinates for this round
    const coordsRes = await fetch('/api/geocoding/valid_coords');
    await handleApiError(coordsRes, 'load Street View');
    const coordsData: GeocodingApiResponse = await coordsRes.json();

    const lat = parseFloat(String(coordsData.modifiedCoordinates.lat));
    const lng = parseFloat(String(coordsData.modifiedCoordinates.lng));
    if (Number.isNaN(lat) || Number.isNaN(lng)) {
      throw new Error('Invalid coordinates received from API');
    }
    const position: Coordinates = { lat, lng };

    // 2) Init/retarget Street View
    if (!panoRef.current) {
      panoRef.current = new window.google.maps.StreetViewPanorama(streetViewRef.current!, {
        position,
        pov: { heading: 0, pitch: 0 },
        zoom: 1,
        addressControl: false,
        fullscreenControl: false,
        imageDateControl: false,
        showRoadLabels: false,
      });
    } else {
      panoRef.current.setPosition(position);
      panoRef.current.setPov({ heading: 0, pitch: 0 });
      panoRef.current.setZoom(1);
    }

    // 3) Persist Location for this round
    const locRes = await fetch('/api/Locations', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ latitude: lat, longitude: lng, panoId: coordsData.panoID }),
    });
    await handleApiError(locRes, 'save location');
    const locData = await locRes.json();

    // 4) Update UI state
    setInitialCoords(position);
    setLocationId(locData.id);
    setSelectedCoords(null);
  };

  // ---- initial mount: wait for maps, mount pano and first round location ----
  useEffect(() => {
    if (isInitializedRef.current) return;
    isInitializedRef.current = true;

    (async () => {
      try {
        await waitGoogleMaps();
        if (!streetViewRef.current) throw new Error('Street View container not mounted');
        await loadRoundLocation();
        setLoading(false);
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Failed to load Street View';
        setError(msg);
        setLoading(false);
        console.error('Street View init error:', err);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ---- submit guess: score, post Guess, advance or finish ----
  const handleSubmitGuess = async () => {
    if (!selectedCoords || !initialCoords || !gameId || !locationId) {
      console.error('Missing required data:', { selectedCoords, initialCoords, gameId, locationId });
      return;
    }

    try {
      setSubmitting(true);

      // A) score this guess
      const resultResponse = await fetch('/api/result', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ guessedCoords: selectedCoords, initialCoords }),
      });
      await handleApiError(resultResponse, 'calculate score');
      const resultData: { score: number; distance: number } = await resultResponse.json();

      // B) create guess (server updates total score, advances round, sets finished)
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
      await handleApiError(guessRes, 'save guess');
      const guessData: GuessPostResponse = await guessRes.json();

      if (guessData.finished) {
        // FINISHED → go to results
        navigate(`/results/${gameId}`, {
          state: {
            gameId,
          },
          replace: true,
        });
        return;
      }

      // NOT FINISHED → advance to next round
      setRound(guessData.currentRound);
      await loadRoundLocation();
    } catch (e) {
      console.error(e);
      alert((e as Error).message || 'Failed to submit this round');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="relative h-screen w-screen">
      {/* Round/Score badge */}
      <div className="absolute top-3 left-3 z-50 rounded bg-black/60 text-white px-3 py-2 text-sm">
        Round {round} / {totalRounds}
      </div>

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
            // console.log('Selected coords from MiniMap:', coords);
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
