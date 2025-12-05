import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';

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
  const [userId, setUserId] = useState('');
  const [myPlayerId, setMyPlayerId] = useState('');

  const roomHubRef = useRef<signalR.HubConnection | null>(null);
  const gameHubRef = useRef<signalR.HubConnection | null>(null);

  // -----------------------------------------------------------
  // 1. Fetch current user
  // -----------------------------------------------------------
  useEffect(() => {
    const loadUser = async () => {
      try {
        const res = await fetch("/api/User/me", { credentials: "include" });
        if (!res.ok) throw new Error("User not logged in");

        const data = await res.json();
        setUserId(data.id);
      } catch (err) {
        console.error(err);
        setError("Failed to fetch user");
      } finally {
        setLoading(false);
      }
    };
    loadUser();
  }, []);

  // -----------------------------------------------------------
  // 2. Fetch initial players
  // -----------------------------------------------------------
  useEffect(() => {
    if (!userId || !roomCode) return;

    const loadPlayers = async () => {
      try {
        const res = await fetch(`/api/Rooms/${roomCode}/players`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load players");

        const list: Player[] = await res.json();
        setPlayers(list);

        const me = list.find((p) => p.userId === userId);
        if (me) setMyPlayerId(me.id);
      } catch (err) {
        console.error(err);
        setError("Failed to load players");
      }
    };

    loadPlayers();
  }, [userId, roomCode]);

  // -----------------------------------------------------------
  // 3. Connect to RoomHub (only once)
  // -----------------------------------------------------------
  useEffect(() => {
    if (!roomCode || !myPlayerId) return;
    if (roomHubRef.current) return;

    const hub = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/roomHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    roomHubRef.current = hub;

    // events
    hub.on("PlayerJoined", (player: Player) => {
      setPlayers((prev) => [...prev, player]);
    });

    hub.on("PlayerLeft", (playerId: string) => {
      setPlayers((prev) => prev.filter((p) => p.id !== playerId));
    });

    hub.on("PlayerListUpdated", (list: Player[]) => {
      setPlayers(list);
    });

    hub.on("GameStarted", (gameId: string) => {
      navigate(`/multiplayer/${roomCode}/${gameId}`);
    });

    const start = async () => {
      try {
        await hub.start();
        const me = players.find((p) => p.id === myPlayerId);
        await hub.invoke(
          "JoinRoom",
          roomCode,
          myPlayerId,
          me?.displayName ?? "Player"
        );
        console.log("Connected to RoomHub");
      } catch (err) {
        console.error("RoomHub connection error:", err);
      }
    };

    start();
  }, [roomCode, myPlayerId, players, navigate]);

  // -----------------------------------------------------------
  // 4. Create GameHub ONLY when needed
  // -----------------------------------------------------------
  const ensureGameHub = async () => {
    if (gameHubRef.current) return;

    const hub = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/gameHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    hub.on("RoundStarted", (data: { gameId: string }) => {
      navigate(`/multiplayer/${roomCode}/${data.gameId}`);
    });

    gameHubRef.current = hub;

    try {
      await hub.start();
      console.log("Connected to GameHub");
    } catch (err) {
      console.error("GameHub error:", err);
    }
  };

  // -----------------------------------------------------------
  // TOGGLE READY
  // -----------------------------------------------------------
  const handleToggleReady = async () => {
    if (!myPlayerId) return;

    try {
      const res = await fetch(`/api/Players/${myPlayerId}/toggle-ready`, {
        method: "POST",
        credentials: "include",
      });

      if (!res.ok) throw new Error("Failed to toggle ready");

      const updated: Player = await res.json();
      setPlayers((prev) =>
        prev.map((p) => (p.id === myPlayerId ? updated : p))
      );

      await roomHubRef.current?.invoke(
        "UpdateReadyState",
        roomCode,
        myPlayerId,
        updated.isReady
      );
    } catch (err) {
      console.error(err);
      setError("Failed to toggle ready");
    }
  };

  // -----------------------------------------------------------
  // START GAME
  // -----------------------------------------------------------
  const handleStartGame = async () => {
    await ensureGameHub();
    try {
      await gameHubRef.current?.invoke("StartGame", roomCode);
    } catch (err) {
      console.error("StartGame failed:", err);
    }
  };

  // -----------------------------------------------------------
  // LEAVE ROOM
  // -----------------------------------------------------------
  const handleLeaveRoom = async () => {
    try {
      await fetch(`/api/Players/${myPlayerId}/leave-room`, {
        method: "POST",
        credentials: "include",
      });

      await roomHubRef.current?.invoke("LeaveRoom", roomCode);
    } catch (err) {
      console.error("Failed to leave room:", err);
    } finally {
      roomHubRef.current?.stop();
      gameHubRef.current?.stop();
      navigate("/joinroom");
    }
  };

  const allReady =
    players.length > 0 && players.every((p) => p.isReady);

  if (loading) return <p className="text-white">Loading room...</p>;
  if (error) return <p className="text-red-400">{error}</p>;

  return (
    <main className="text-white flex flex-col items-center p-8">
      <h1 className="text-3xl font-bold mb-6">Room {roomCode}</h1>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        {players.map((player) => (
          <div
            key={player.id}
            className="flex flex-col items-center p-4 bg-white/10 rounded-xl"
          >
            <span className="text-4xl">🙂</span>
            <span className="mt-2">{player.displayName}</span>
            <span className="text-sm mt-1">
              {player.isReady ? "✅ Ready" : "❌ Not Ready"}
            </span>
          </div>
        ))}
      </div>

      <div className="flex gap-4">
        <button
          onClick={handleToggleReady}
          className="px-6 py-3 bg-green-600 rounded-xl hover:bg-green-700 transition"
        >
          {players.find((p) => p.id === myPlayerId)?.isReady
            ? "Unready"
            : "Get Ready"}
        </button>

        <button
          onClick={handleStartGame}
          disabled={!allReady}
          className={`px-6 py-3 rounded-xl transition ${
            allReady
              ? "bg-blue-600 hover:bg-blue-700"
              : "bg-gray-500 cursor-not-allowed"
          }`}
        >
          Start Game
        </button>

        <button
          onClick={handleLeaveRoom}
          className="px-6 py-3 bg-red-600 rounded-xl hover:bg-red-700 transition"
        >
          Leave Room
        </button>
      </div>
    </main>
  );
}
