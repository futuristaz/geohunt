// src/pages/Results.tsx
import { Link, useLocation } from "react-router-dom";

type ResultsState = {
  gameId: string;
  lastScore: number;
  lastDistanceKm: number;
  selectedCoords: { lat: number; lng: number }; // your guess
  initialCoords: { lat: number; lng: number };  // actual
};

export default function Results() {
  const location = useLocation();
  const state = location.state as ResultsState | undefined;

  // If someone navigates here directly (no state), show a gentle fallback
  if (!state) {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-semibold mb-4">Results</h1>
        <p className="mb-4 text-gray-700">
          There’s no results data to display (this page expects to be opened from the game).
        </p>
        <div className="flex gap-3">
          <Link to="/start" className="px-4 py-2 bg-blue-600 text-white rounded">Play</Link>
          <Link to="/" className="px-4 py-2 bg-gray-200 rounded">Home</Link>
        </div>
      </div>
    );
  }

  const { gameId, lastScore, lastDistanceKm, selectedCoords, initialCoords } = state;

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold mb-4">Results</h1>

      <div className="space-y-3">
        <div>Game ID: <b className="font-mono">{gameId}</b></div>
        <div>Last round score: <b>{lastScore}</b> points</div>
        <div>Distance: <b>{lastDistanceKm} km</b></div>

        <div className="text-sm text-gray-700">
          <div>
            Actual:&nbsp;
            <b>{initialCoords.lat.toFixed(5)}, {initialCoords.lng.toFixed(5)}</b>
          </div>
          <div>
            Your guess:&nbsp;
            <b>{selectedCoords.lat.toFixed(5)}, {selectedCoords.lng.toFixed(5)}</b>
          </div>
        </div>
      </div>

      <div className="mt-6 flex gap-3">
        <Link to="/start" className="px-4 py-2 bg-blue-600 text-white rounded">Play Again</Link>
        <Link to="/" className="px-4 py-2 bg-gray-200 rounded text-blue-600">Home</Link>
      </div>
    </div>
  );
}
