import { useEffect, useRef, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import * as signalR from "@microsoft/signalr";
import MiniMap from "../components/MiniMap";

interface Coordinates {
  lat: number;
  lng: number;
}

interface RoundData {
  roundNumber: number;
  totalRounds: number;
  latitude: number;
  longitude: number;
  panoId: string;
}

interface PlayerGuess {
  playerId: string;
  score: number;
  distanceMeters: number;
}

interface MultiplayerGameState {
  gameId: string;
  currentRound: number;
  totalRounds: number;
}

export default function MultiplayerGame() {
  const { roomCode, gameId } = useParams<{ roomCode: string; gameId: string }>();
  const navigate = useNavigate();

  const streetViewRef = useRef<HTMLDivElement>(null);
  const panoRef = useRef<any>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [roundData, setRoundData] = useState<RoundData | null>(null);
  const [selectedCoords, setSelectedCoords] = useState<Coordinates | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [gameState, setGameState] = useState<MultiplayerGameState | null>(null);
  const [playersGuesses, setPlayersGuesses] = useState<PlayerGuess[]>([]);

  // --- initialize SignalR connection ---
  useEffect(() => {
    if (!roomCode || !gameId) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/gameHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // --- listen for round start ---
    connection.on("RoundStarted", (data: RoundData & { currentRound: number; totalRounds: number }) => {
      setRoundData(data);
      setGameState({ gameId, currentRound: data.currentRound, totalRounds: data.totalRounds });
      setSelectedCoords(null);

      // Init/retarget StreetView
      if (!panoRef.current) {
        panoRef.current = new window.google.maps.StreetViewPanorama(streetViewRef.current!, {
          position: { lat: data.latitude, lng: data.longitude },
          pov: { heading: 0, pitch: 0 },
          zoom: 1,
          addressControl: false,
          fullscreenControl: false,
          imageDateControl: false,
          showRoadLabels: false,
        });
      } else {
        panoRef.current.setPosition({ lat: data.latitude, lng: data.longitude });
        panoRef.current.setPov({ heading: 0, pitch: 0 });
        panoRef.current.setZoom(1);
      }

      setLoading(false);
    });

    // --- listen for guess results (optional for leaderboard) ---
    connection.on("PlayerGuessed", (guess: PlayerGuess) => {
      setPlayersGuesses((prev) => [...prev.filter(g => g.playerId !== guess.playerId), guess]);
    });

    // --- listen for game finished ---
    connection.on("GameFinished", () => {
      navigate(`/multiplayer/${roomCode}/results/${gameId}`);
    });

    const startConnection = async () => {
      try {
        await connection.start();
        console.log("Connected to GameHub");

        // Notify hub that this player joined the game
        await connection.invoke("JoinGame", roomCode, gameId);
      } catch (err) {
        console.error(err);
        setError("Failed to connect to game server");
      }
    };

    startConnection();

    return () => {
      connection.stop();
    };
  }, [roomCode, gameId, navigate]);

  const handleSubmitGuess = async () => {
    if (!selectedCoords || !roundData || !connectionRef.current) return;

    try {
      setSubmitting(true);

      // Notify server with the guess
      await connectionRef.current.invoke("SubmitGuess", gameId, selectedCoords);

      setSelectedCoords(null);
    } catch (err) {
      console.error(err);
      setError("Failed to submit guess");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <p className="text-white">Loading round...</p>;
  if (error) return <p className="text-red-500">{error}</p>;
  if (!roundData || !gameState) return null;

  return (
    <div className="relative h-screen w-screen">
      <div className="absolute top-3 left-3 z-50 rounded bg-black/60 text-white px-3 py-2 text-sm">
        Round {roundData.roundNumber} / {roundData.totalRounds}
      </div>

      <div ref={streetViewRef} className="h-full w-full" />

      <div className="absolute bottom-4 right-4 flex flex-col gap-2" style={{ zIndex: 9999 }}>
        <MiniMap
          initialZoom={1}
          onSelect={(coords) => setSelectedCoords(coords)}
          className="overflow-hidden shadow-lg rounded-xl"
          style={{ width: 300, height: 200 }}
        />
        <button
          onClick={handleSubmitGuess}
          disabled={!selectedCoords || submitting}
          className="px-3 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition disabled:opacity-60 disabled:cursor-not-allowed"
        >
          {submitting ? "Submitting…" : selectedCoords ? "Submit Guess" : "Select a location on the map"}
        </button>
      </div>
    </div>
  );
}
