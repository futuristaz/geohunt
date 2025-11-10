// src/pages/Start.tsx
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

type CreateGameDto = {
  userId: string;
  totalRounds: number;
};

type GameResponseDto = {
  id: string;
  userId: string;
  totalScore: number;
  startedAt: string;
  finishedAt: string | null;
  currentRound: number;
  totalRounds: number;
};

const DEFAULT_USER_ID = "019a6a1e-d51a-731d-917a-171fdcf9d9b7"; // replace with real user id

export default function Start() {
  const [rounds, setRounds] = useState(3);     // default selection
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const [userId, setUserId] = useState<string>('');
  const navigate = useNavigate();

  useEffect(() => {getUser();}, []);

  const getUser = async () => {
    try {
      const res = await fetch('/api/User/me', {
        method: 'GET',
        credentials: 'include' // include cookies for authentication
      });
      if (res.ok) {
        const data = await res.json();
        setUserId(data.id);
      } else {
        navigate('/login', { replace: true });
      }
    } catch (error) {
      console.error('Error checking login status:', error);
      navigate('/login', { replace: true });
    }
  };

  const handleStart = async () => {
    setErr(null);
    setLoading(true);
    getUser();
    try {
      const payload: CreateGameDto = {
        userId: userId,
        totalRounds: rounds,
      };

      const res = await fetch("/api/Game", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `Failed to start game (HTTP ${res.status})`);
      }

      const game: GameResponseDto = await res.json();

      // Navigate to /game with the newly created game id and selected rounds
      navigate("/game", {
        state: {
          gameId: game.id,
          totalRounds: game.totalRounds,   // the server may clamp the value (e.g., min/max)
          currentRound: game.currentRound, // should be 1
        },
        replace: true,
      });
    } catch (e) {
      setErr((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 max-w-md">
      <h1 className="text-2xl font-semibold mb-4">Start a Game</h1>

      <label className="block mb-2 font-medium">How many rounds?</label>
      <select
        className="border rounded px-3 py-2 mb-4 w-40"
        value={rounds}
        onChange={(e) => setRounds(Number(e.target.value))}
        disabled={loading}
      >
        {[1,2,3,4,5].map(n => (
          <option key={n} value={n}>{n}</option>
        ))}
      </select>

      <div className="flex items-center gap-3">
        <button
          className="px-4 py-2 bg-blue-600 text-white rounded disabled:opacity-60 disabled:cursor-not-allowed"
          onClick={handleStart}
          disabled={loading}
        >
          {loading ? "Starting…" : "Start Game"}
        </button>
        {err && <span className="text-red-600 text-sm">{err}</span>}
      </div>
    </div>
  );
}
