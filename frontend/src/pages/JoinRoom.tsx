import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function JoinRoom() {
  const navigate = useNavigate();
  const [userId, setUserId] = useState<string | null>(null);
  const [displayName, setDisplayName] = useState('');
  const [error, setError] = useState('');

  const [joinRoomCode, setJoinRoomCode] = useState('');
  const [createRounds, setCreateRounds] = useState(1);

  const [activeTab, setActiveTab] = useState<'join' | 'create'>('join');

  // Fetch current user
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

  // Join room handler
  const handleJoinRoom = async () => {
    if (!joinRoomCode || !displayName || !userId) {
      setError('Please fill all fields');
      return;
    }
    try {
      const res = await fetch('/api/Rooms/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          roomCode: joinRoomCode,
          userId,
          displayName
        })
      });
      if (!res.ok) throw new Error('Failed to join room');
      navigate(`/room/${joinRoomCode}`);
    } catch (err) {
      console.error(err);
      setError('Could not join room. Check the code.');
    }
  };

  // Create room handler
  const handleCreateRoom = async () => {
    if (!displayName || !userId) {
      setError('Please enter your name');
      return;
    }
    try {
      const res = await fetch('/api/Rooms/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ totalRounds: createRounds })
      });
      if (!res.ok) throw new Error('Failed to create room');
      const room = await res.json();

      // Auto-join creator as player
      const joinRes = await fetch('/api/Rooms/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          roomCode: room.roomCode,
          userId,
          displayName
        })
      });
      if (!joinRes.ok) throw new Error('Failed to join created room');

      navigate(`/room/${room.roomCode}`);
    } catch (err) {
      console.error(err);
      setError('Could not create room');
    }
  };

  return (
    <main className="text-white flex flex-col items-center justify-center p-8">
      <h1 className="text-4xl font-bold mb-6">Rooms</h1>

      <div className="flex gap-4 mb-6">
        <button
          onClick={() => setActiveTab('join')}
          className={`px-6 py-3 rounded-xl font-semibold ${
            activeTab === 'join'
              ? 'bg-blue-600'
              : 'bg-white/10 hover:bg-white/20'
          }`}
        >
          Join Room
        </button>
        <button
          onClick={() => setActiveTab('create')}
          className={`px-6 py-3 rounded-xl font-semibold ${
            activeTab === 'create'
              ? 'bg-green-600'
              : 'bg-white/10 hover:bg-white/20'
          }`}
        >
          Create Room
        </button>
      </div>

      <div className="w-full max-w-sm flex flex-col gap-4">
        {activeTab === 'join' && (
          <>
            <input
              type="text"
              value={joinRoomCode}
              onChange={(e) => setJoinRoomCode(e.target.value)}
              placeholder="Enter Room Code"
              className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Your Name"
              className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              onClick={handleJoinRoom}
              className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition"
            >
              Join Room
            </button>
          </>
        )}

        {activeTab === 'create' && (
          <>
            <input
              type="number"
              value={createRounds}
              onChange={(e) => setCreateRounds(Number(e.target.value))}
              min={1}
              placeholder="Number of Rounds"
              className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-green-500"
            />
            <input
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Your Name"
              className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-green-500"
            />
            <button
              onClick={handleCreateRoom}
              className="px-6 py-3 bg-green-600 text-white font-semibold rounded-xl shadow hover:bg-green-700 transition"
            >
              Create Room
            </button>
          </>
        )}

        {error && <p className="text-red-500 text-sm">{error}</p>}

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
