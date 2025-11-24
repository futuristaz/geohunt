import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function JoinRoom() {
  const navigate = useNavigate();
  const [roomCode, setRoomCode] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [error, setError] = useState('');
  const [userId, setUserId] = useState<string | null>(null);

  // Fetch logged-in user ID
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
    if (!roomCode) {
      setError('Please enter a room code');
      return;
    }
    if (!displayName) {
      setError('Please enter a display name');
      return;
    }
    if (!userId) {
      setError('User not loaded yet');
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

      // Navigate to room lobby
      navigate(`/room/${roomCode}`);
    } catch (err) {
      console.error(err);
      setError('Failed to join room. Please check the code.');
    }
  };

  return (
    <main className="text-white flex flex-col items-center justify-center p-8">
      <h1 className="text-4xl font-bold mb-6">Join a Room</h1>

      <div className="flex flex-col gap-4 w-full max-w-sm">
        <input
          type="text"
          value={roomCode}
          onChange={(e) => setRoomCode(e.target.value)}
          placeholder="Enter Room Code"
          className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
        />

        <input
          type="text"
          value={displayName}
          onChange={(e) => setDisplayName(e.target.value)}
          placeholder="Enter Your Name"
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
            onClick={() => navigate('/')}
            className="flex-1 px-6 py-3 bg-transparent border border-white font-semibold rounded-xl hover:bg-white/10 transition"
          >
            Back
          </button>
        </div>
      </div>
    </main>
  );
}
