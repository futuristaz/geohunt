import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';

interface Player {
  id: string;
  userId: string;
  displayName: string;
  isReady: boolean;
}

export default function RoomLobby() {
  const { roomCode } = useParams<{ roomCode: string }>();
  const navigate = useNavigate();

  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userId, setUserId] = useState<string>('');
  const [myPlayerId, setMyPlayerId] = useState<string>('');

  // 1️⃣ Fetch current logged-in user
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const res = await fetch('/api/User/me', { credentials: 'include' });
        if (!res.ok) throw new Error('User not logged in');
        const data = await res.json();
        setUserId(data.id);
      } catch (err) {
        console.error(err);
        setError('Failed to fetch user');
        setLoading(false);
      }
    };
    fetchUser();
  }, []);

  // 2️⃣ Fetch players in the room every 3 seconds
  useEffect(() => {
    if (!roomCode || !userId) return;

    const fetchPlayers = async () => {
      try {
        const res = await fetch(`/api/Rooms/${roomCode}/players`, { credentials: 'include' });
        if (!res.ok) throw new Error('Failed to load players');
        const data: Player[] = await res.json();
        setPlayers(data);

        // Find my player
        const me = data.find(p => p.userId === userId);
        if (me) setMyPlayerId(me.id);

        setLoading(false);
      } catch (err) {
        console.error(err);
        setError('Failed to load players');
        setLoading(false);
      }
    };

    fetchPlayers();
    const interval = setInterval(fetchPlayers, 3000);
    return () => clearInterval(interval);
  }, [roomCode, userId]);

  // 3️⃣ Toggle ready/unready
  const handleToggleReady = async () => {
    if (!myPlayerId) return;

    try {
      const res = await fetch(`/api/Players/${myPlayerId}/toggle-ready`, {
        method: 'POST',
        credentials: 'include',
      });
      if (!res.ok) throw new Error('Failed to toggle ready');

      const updatedPlayer: Player = await res.json();
      setPlayers(prev => prev.map(p => (p.id === myPlayerId ? updatedPlayer : p)));
    } catch (err) {
      console.error(err);
      setError('Failed to toggle ready');
    }
  };

  // 4️⃣ Leave room
  const handleLeaveRoom = async () => {
    if (!myPlayerId) return;

    try {
      const res = await fetch(`/api/Players/${myPlayerId}/leave-room`, {
        method: 'POST',
        credentials: 'include',
      });
      if (!res.ok) throw new Error('Failed to leave room');

      navigate('/joinroom');
    } catch (err) {
      console.error(err);
      setError('Failed to leave room');
    }
  };

  if (loading) return <p className="text-white">Loading room...</p>;
  if (error) return <p className="text-red-500">{error}</p>;

  return (
    <main className="text-white flex flex-col items-center p-8">
      <h1 className="text-3xl font-bold mb-6">Room {roomCode}</h1>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        {players.map(player => (
          <div
            key={player.id}
            className="flex flex-col items-center p-4 bg-white/10 rounded-xl"
          >
            <span className="text-4xl">🙂</span>
            <span className="mt-2">{player.displayName}</span>
            <span className="mt-1 text-sm">
              {player.isReady ? '✅ Ready' : '❌ Not Ready'}
            </span>
          </div>
        ))}
      </div>

      {/* Only show buttons if myPlayerId is known */}
      {myPlayerId ? (
        <div className="flex gap-4">
          <button
            onClick={handleToggleReady}
            className="px-6 py-3 bg-green-600 text-white font-semibold rounded-xl shadow hover:bg-green-700 transition"
          >
            {players.find(p => p.id === myPlayerId)?.isReady ? 'Unready' : 'Get Ready'}
          </button>

          <button
            onClick={handleLeaveRoom}
            className="px-6 py-3 bg-red-600 text-white font-semibold rounded-xl shadow hover:bg-red-700 transition"
          >
            Leave Room
          </button>
        </div>
      ) : (
        <p className="text-gray-400">Loading player info...</p>
      )}
    </main>
  );
}
