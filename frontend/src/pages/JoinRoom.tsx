import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function JoinRoom() {
  const navigate = useNavigate();
  const [roomCode, setRoomCode] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [rounds, setRounds] = useState(1); // for creating a room
  const [error, setError] = useState('');
  const [userId, setUserId] = useState<string | null>(null);

  // Fetch logged-in user
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const res = await fetch('/api/User/me', { credentials: 'include' });
        if (!res.ok) throw new Error('Not logged in');
        const data = await res.json();
        setUserId(data.id);
      } catch (err) {
        console.error(err);
        navigate('/login');
      }
    };
    fetchUser();
  }, [navigate]);

  const handleJoinRoom = async () => {
    if (!roomCode || !displayName || !userId) {
      setError('Please fill in all fields');
      return;
    }

    try {
      const res = await fetch('/api/Rooms/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ roomCode, userId, displayName })
      });

      if (!res.ok) throw new Error('Room not found or join failed');

      // Redirect to RoomLobby
      navigate(`/room/${roomCode}`);
    } catch (err) {
      console.error(err);
      setError('Failed to join room. Please check the code.');
    }
  };

  const handleCreateRoom = async () => {
    if (!displayName || !userId) {
      setError('Please enter your display name');
      return;
    }

    try {
      const res = await fetch('/api/Rooms/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ totalRounds: rounds })
      });

      if (!res.ok) throw new Error('Failed to create room');

      const room = await res.json();

      // Automatically join the creator as a player
      const joinRes = await fetch('/api/Rooms/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ roomCode: room.roomCode, userId, displayName })
      });

      if (!joinRes.ok) throw new Error('Failed to join newly created room');

      navigate(`/room/${room.roomCode}`);
    } catch (err) {
      console.error(err);
      setError('Could not create or join room');
    }
  };

  return (
    <main className="text-white flex flex-col items-center justify-center p-8">
      <h1 className="text-4xl font-bold mb-6">Join or Create a Room</h1>

      <div className="flex flex-col gap-4 w-full max-w-sm">
        {/* Join room */}
        <input
          type="text"
          value={roomCode}
          onChange={(e) => setRoomCode(e.target.value)}
          placeholder="Room Code (to join)"
          className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <input
          type="text"
          value={displayName}
          onChange={(e) => setDisplayName(e.target.value)}
          placeholder="Your Display Name"
          className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
        />

        <input
          type="number"
          value={rounds}
          onChange={(e) => setRounds(Number(e.target.value))}
          min={1}
          placeholder="Number of Rounds (for new room)"
          className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
        />

        {error && <p className="text-red-500 text-sm">{error}</p>}

        <div className="flex gap-4">
          <button
            onClick={handleJoinRoom}
            className="flex-1 px-6 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition"
          >
            Join Room
          </button>
          <button
            onClick={handleCreateRoom}
            className="flex-1 px-6 py-3 bg-green-600 text-white font-semibold rounded-xl shadow hover:bg-green-700 transition"
          >
            Create Room
          </button>
        </div>

        <button
          onClick={() => navigate('/')}
          className="mt-4 px-6 py-3 bg-red-600 text-white font-semibold rounded-xl shadow hover:bg-red-700 transition"
        >
          Back
        </button>
      </div>
    </main>
  );
}
