// src/pages/Results.tsx
import { Link, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";

type ResultsState = {
  gameId: string;
};

type GameResult = {
    id: number;
    gameId: string;
    locationId: number;
    roundNumber: number;
    guessedLatitude: number;
    guessedLongitude: number;
    distanceKm: number;
    score: number;
    actualLatitude: number;
    actualLongitude: number;
};

export default function Results() {
  const location = useLocation();
  const state = location.state as ResultsState | undefined;
  
  const [results, setResults] = useState<GameResult[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // If someone navigates here directly (no state), show a gentle fallback
  if (!state) {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-semibold mb-4">Results</h1>
        <p className="mb-4 text-gray-700">
          There's no results data to display (this page expects to be opened from the game).
        </p>
        <div className="flex gap-3">
          <Link to="/start" className="px-4 py-2 bg-blue-600 text-white rounded">Play</Link>
          <Link to="/" className="px-4 py-2 bg-gray-200 rounded">Home</Link>
        </div>
      </div>
    );
  }

  const { gameId } = state;

  useEffect(() => {
    const fetchGameResults = async () => {
      try {
        setLoading(true);
        const res = await fetch(`/api/Guess/${gameId}`, {
          method: "GET",
          headers: { "Content-Type": "application/json" }
        });
        if (!res.ok) throw new Error('Failed to fetch game results');
        const data = await res.json();
        setResults(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchGameResults();
  }, [gameId]);

  if (loading) {
    return <div className="p-6">Loading results...</div>;
  }

  if (error) {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-semibold mb-4">Error</h1>
        <p className="text-red-600 mb-4">{error}</p>
        <Link to="/start" className="px-4 py-2 bg-blue-600 text-white rounded">
          Try Again
        </Link>
      </div>
    );
  }

  // Calculate total score if needed
  const totalScore = results.reduce((sum, result) => sum + result.score, 0);

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold mb-4">Results</h1>

      <div className="space-y-3 mb-6">
        <div>Game ID: <b className="font-mono">{gameId}</b></div>
        <div>Total Score: <b>{totalScore}</b> points</div>
      </div>

      {/* Display each round's results */}
      <div className="space-y-4">
        {results.map((result, index) => (
          <div key={index} className="border rounded-lg p-4 bg-gray-50">
            <h3 className="font-semibold mb-2 text-blue-900">Round {index + 1}</h3>
            <div className="space-y-1 text-sm text-blue-800">
              <div>Score: <b>{result.score}</b> points</div>
              <div>Distance: <b>{result.distanceKm}</b> km</div>
              <div className="text-gray-700">
                <div>Actual: {result.actualLatitude.toFixed(5)}, {result.actualLongitude.toFixed(5)}</div>
                <div>Your guess: {result.guessedLatitude.toFixed(5)}, {result.guessedLongitude.toFixed(5)}</div>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="mt-6 flex gap-3">
        <Link to="/start" className="px-4 py-2 bg-blue-600 text-white rounded">
          Play Again
        </Link>
        <Link to="/" className="px-4 py-2 bg-gray-200 rounded text-blue-600">
          Home
        </Link>
      </div>
    </div>
  );
}