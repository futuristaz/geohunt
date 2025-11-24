import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

interface Player {
  id: string;
  userId: string;
  displayName: string;
  isReady: boolean;
}

export default function RoomLobby() {
  const { roomCode } = useParams<{ roomCode: string }>();
  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [myPlayerId, setMyPlayerId] = useState<string>('');
  const [userId, setUserId] = useState<string>('');

  // Get current user
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const res = await fetch('/api/User/me', { credentials: 'include' });
        if (!res.ok) throw new Error('User not logged in');
        const data = await res.json();
        setUserId(data.id);
      } catch (err) {
        console.error(err);
      }
    };
    fetchUser();
  }, []);

  // Fetch players in room
  useEffect(() => {
    if (!roomCode) return;

    const fetchPlayers = async () => {
      try {
        const res = await fetch(`/api/Rooms/${roomCode}/players`, { credentials: 'include' });
        if (!res.ok) throw new Error('Failed to load players');
        const data: Player[] = await res.json();
        setPlayers(data);

        // Set current player's ID
        const me = data.find(p => p.userId === userId);
        if (me) setMyPlayerId(me.id);

        setLoading(false);
      } catch (err) {
        console.error(err);
        setError('Could not load players');
        setLoading(false);
      }
    };

    fetchPlayers();
    const interval = setInterval(fetchPlayers, 3000); // Refresh every 3s
    return () => clearInterval(interval);
  }, [roomCode, userId]);

  const handleReady = async () => {
    if (!myPlayerId) return;

    try {
      const res = await fetch(`/api/Players/${myPlayerId}/ready`, {
        method: 'POST',
        credentials: 'include'
      });
      if (!res.ok) throw new Error('Failed to set ready');

      setPlayers(players.map(p =>
        p.id === myPlayerId ? { ...p, isReady: true } : p
      ));
    } catch (err) {
      console.error(err);
      setError('Failed to set ready');
    }
  };

  if (loading) return <p className="text-white">Loading room...</p>;
  if (error) return <p className="text-red-500">{error}</p>;

  return (
    <main className="text-white flex flex-col items-center p-8">
      <h1 className="text-3xl font-bold mb-6">Room {roomCode}</h1>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        {players.map(player => (
          <div key={player.id} className="flex flex-col items-center p-4 bg-white/10 rounded-xl">
            <span className="text-4xl">🙂</span>
            <span className="mt-2">{player.displayName}</span>
            <span className="mt-1 text-sm">{player.isReady ? '✅ Ready' : '❌ Not Ready'}</span>
          </div>
        ))}
      </div>

      <button
        onClick={handleReady}
        className="px-6 py-3 bg-green-600 text-white font-semibold rounded-xl shadow hover:bg-green-700 transition"
      >
        Get Ready
      </button>
    </main>
  );
}
