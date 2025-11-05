/// <reference types="google.maps" />

import { useEffect, useState, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Loader } from '@googlemaps/js-api-loader';

declare global {
    interface Window {
        google: any;
        initMap: () => void;
    }
}

type LatLng = { lat: number; lng: number; };
type Coords = LatLng;

type GuessData = {
    initialCoords: LatLng
    guessedCoords: LatLng
}

type ResultResponse = {
    distance: number;
    score: number;
}

type LocationRequest = {
    latitude: number;
    longitude: number;
    panoId: string;
}

type LocationResponse = {id: string;}
type GameCreateRequest = {UserId: string;}
type GameResponse = {id: string;}

type ScorePatchRequest = {score: number;}
type GuessRequest = {
    gameId: string;
    locationId: string;
    guessedLatitude: number;
    guessedLongitude: number;
    distanceKm: number;
    score: number;
}


export default function Game() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  
  const totalRounds = parseInt(searchParams.get('rounds') ?? '1', 10);
  const [currentRound, setCurrentRound] = useState(() => 
    parseInt(sessionStorage.getItem('currentRound') ?? '1', 10)
  );
  const [message, setMessage] = useState<{ text: string; type: 'error' | 'success' | '' }>({
    text: '',
    type: '',
  })
  const [selectedCoords, setSelectedCoords] = useState<Coords | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Typed refs
  const streetViewRef = useRef<HTMLDivElement | null>(null)
  const miniMapRef = useRef<HTMLDivElement | null>(null)
  const selectionMarkerRef = useRef<google.maps.Marker | null>(null)

  const initialPositionRef = useRef<LatLng | null>(null)
  const gameIdRef = useRef<string | null>(sessionStorage.getItem('gameId'))
  const locationIdRef = useRef<string | null>(null)

  const showMessage = (text: string, type: 'error' | 'success' | '') => {
    setMessage({ text, type })
    setTimeout(() => setMessage({ text: '', type: '' }), 5000)
  }

 useEffect(() => {
  const loader = new Loader({
    apiKey: import.meta.env.VITE_GOOGLE_MAPS_API_KEY, // put your key in .env
    version: 'weekly',
    libraries: [] // add 'places' etc. if you need them
  })

  loader.load()
    .then(() => initializeMap()) // formerly initMap()
    .catch((e) => {
      console.error('Failed to load Google Maps:', e)
      showMessage('Failed to load Google Maps.', 'error')
    })
  }, [])

  const initializeMap = async () => {
    try {
      // 1) Ensure game exists
      if (!gameIdRef.current) {
        const responseGame = await postGame({ 
          UserId: "f5f925f5-4345-474e-a068-5169ab8bcb15" 
        });
        gameIdRef.current = responseGame.id;
        sessionStorage.setItem('gameId', responseGame.id);
      }

      // 2) Get location coordinates
      const response = await fetch('/api/geocoding/valid_coords');
      if (!response.ok) throw new Error('Failed to fetch coordinates');
      const data = await response.json();

      const lat = Number(data.modifiedCoordinates.lat);
      const lng = Number(data.modifiedCoordinates.lng);
      const initialPosition: LatLng = { lat, lng };
      initialPositionRef.current = initialPosition;

      // 3) Street View
      if (!streetViewRef.current) {
        showMessage('Street View container not found.', 'error');
        return;
      }

      new google.maps.StreetViewPanorama(streetViewRef.current, {
        position: initialPositionRef.current,
        pov: { heading: 0, pitch: 0 },
        zoom: 1,
      });

      // 4) Save location in backend
       const responseLocation = await postLocation({ 
        latitude: lat, 
        longitude: lng, 
        panoId: data.panoID 
      });

      if (!responseLocation) {
        showMessage('Error initializing game. Please try again later.', 'error');
        return;
      }
      locationIdRef.current = responseLocation.id;


      // 5) Mini map
      if (!miniMapRef.current) {
        showMessage('Mini map container not found.', 'error');
        return;
      }

      const miniMap = new google.maps.Map(miniMapRef.current, {
        center: { lat: 0.0, lng: 0.0 },
        zoom: 1,
        streetViewControl: false,
        fullscreenControl: false,
        mapTypeControl: false,
      });

      const selectionMarker = new google.maps.Marker({
        map: miniMap,
        visible: false,
      });

      selectionMarkerRef.current = selectionMarker;

      // Add click listener to mini map
      miniMap.addListener('click', (event: google.maps.MapMouseEvent) => {
        const e = event.latLng;
        if (!e) return;
        const position: LatLng = { lat: e.lat(), lng: e.lng() };
        setSelectedCoords({ lat, lng });

        if (selectionMarkerRef.current) {
            selectionMarkerRef.current.setPosition(position);
            selectionMarkerRef.current.setVisible(true);
        }

        console.log('Selected coordinates:', {
          lat: lat.toFixed(5),
          lng: lng.toFixed(5),
        });
      });
    } catch (error) {
      console.error('Error initializing map:', error);
      showMessage('Error initializing game. Please try again later.', 'error');
    }
  };

  const handleSubmit = async () => {
    if (!selectedCoords) {
        showMessage('No coordinates selected.', 'error');
        return;
    }

    if (!initialPositionRef.current) {
      showMessage('Initial position not set.', 'error');
      return;
    }

    if (!gameIdRef.current || !locationIdRef.current) {
        showMessage('Game or location ID not set.', 'error');
        return;
    }
    setIsSubmitting(true);

    try {
      console.log('Submitting coordinates:', selectedCoords);
    
      const guessData: GuessData = {
        initialCoords: initialPositionRef.current,
        guessedCoords: selectedCoords
      };

      // 1) Calculate result
      const result = await getResult(guessData); //ResultResponse
      
      // 2) Update game score
      await updateScore(gameIdRef.current, result.score);

      // 3) Post guess
      const guessPayload: GuessRequest = {
        gameId: gameIdRef.current,
        locationId: locationIdRef.current,
        guessedLatitude: selectedCoords.lat,
        guessedLongitude: selectedCoords.lng,
        distanceKm: result.distance,
        score: result.score
      };

      await postGuess(guessPayload);

      // 4) Get all results
      const raw = sessionStorage.getItem('allResults')
      const allResults: ResultResponse[] = raw ? JSON.parse(raw) : []      
      allResults.push(result);
      sessionStorage.setItem('allResults', JSON.stringify(allResults));
      
      // 5) Next round or finish
      if (currentRound < totalRounds) {
        // Move to next round
        const nextRound = currentRound + 1;
        sessionStorage.setItem('currentRound', nextRound.toString());
        window.location.reload();
      } else {
        // Finish game
        sessionStorage.removeItem('currentRound');
        await finishGame();
        const totalScore = await fetchTotalScore(gameIdRef.current);
        sessionStorage.setItem('totalScore', totalScore.toString());
        sessionStorage.removeItem('gameId');
        navigate('/result');
      }
    } catch (error) {
      console.error('Error submitting guess:', error);
      showMessage('Error submitting guess. Please try again.', 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  // API functions
  async function getResult(data: GuessData): Promise<ResultResponse> {
    const r = await fetch('/api/result', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
    if (!r.ok) throw new Error('Failed to get result');
    return await r.json();
  }

  async function postLocation(data: LocationRequest): Promise<LocationResponse> {
    const r = await fetch('/api/Locations', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
    if (!r.ok) throw new Error('Failed to post location');
    return await r.json();
  }

  async function postGame(data: GameCreateRequest): Promise<GameResponse> {
    const r = await fetch('/api/Game', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
    if (!r.ok) throw new Error('Failed to create game');
    return await r.json();
  }

  async function updateScore(gameId: string, score: number): Promise<void> {
    // Many ASP.NET endpoints expect a JSON object { score }, not a raw number
    const r = await fetch(`/api/Game/${gameId}/score`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ score } as ScorePatchRequest),
    })
    if (!r.ok) throw new Error('updateScore failed')
  }

  async function postGuess(data: GuessRequest): Promise<void> {
    const r = await fetch('/api/Guess', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    })
    if (!r.ok) throw new Error('postGuess failed')
  }

  async function finishGame(): Promise<void> {
    const r = await fetch(`/api/Game/${gameIdRef.current}/finish`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
    })
    if (!r.ok) throw new Error('finishGame failed')
  }

  async function fetchTotalScore(gameId: string): Promise<number> {
    const r = await fetch(`/api/Game/${gameId}/total-score`)
    if (!r.ok) return 0
    return r.json()
  }

  return (
    <div style={{ height: '100vh', width: '100%', margin: 0, position: 'relative' }}>
      {/* Street View */}
      <div ref={streetViewRef} style={{ height: '100vh', width: '100%' }} />

      {/* Mini Map Container */}
      <div
        style={{
          position: 'fixed',
          bottom: '24px',
          right: '24px',
          width: '280px',
          borderRadius: '12px',
          overflow: 'hidden',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.3)',
          backgroundColor: 'rgba(255, 255, 255, 0.9)',
          backdropFilter: 'blur(6px)',
          zIndex: 10,
        }}
      >
        {/* Message Container */}
        {message.text && (
          <div
            style={{
              padding: '8px',
              textAlign: 'center',
              backgroundColor: message.type === 'error' ? '#dc3545' : '#28a745',
              color: 'white',
              fontSize: '14px',
            }}
          >
            {message.text}
          </div>
        )}

        {/* Mini Map */}
        <div ref={miniMapRef} style={{ height: '180px', width: '100%' }} />

        {/* Submit Button Container */}
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', width: '100%' }}>
          <button
            onClick={handleSubmit}
            disabled={isSubmitting || !selectedCoords}
            style={{
              padding: '8px 16px',
              border: 'none',
              borderRadius: '4px',
              backgroundColor: isSubmitting || !selectedCoords ? '#6c757d' : '#007bff',
              color: 'white',
              cursor: isSubmitting || !selectedCoords ? 'not-allowed' : 'pointer',
              width: '100%',
              fontSize: '14px',
            }}
          >
            {isSubmitting ? 'Submitting...' : 'Submit guess'}
          </button>
        </div>
      </div>
    </div>
  );
}