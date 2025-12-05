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
  const [roomHubConnected, setRoomHubConnected] = useState(false);
  const [gameHubConnected, setGameHubConnected] = useState(false);

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

    // Connection state handlers
    hub.onclose(() => {
      console.log("RoomHub disconnected");
      setRoomHubConnected(false);
    });

    hub.onreconnecting(() => {
      console.log("RoomHub reconnecting...");
      setRoomHubConnected(false);
    });

    hub.onreconnected(() => {
      console.log("RoomHub reconnected");
      setRoomHubConnected(true);
    });

    // Events
    hub.on("PlayerJoined", (player: Player) => {
      console.log("Player joined:", player);
      setPlayers((prev) => [...prev, player]);
    });

    hub.on("PlayerLeft", (playerId: string) => {
      console.log("Player left:", playerId);
      setPlayers((prev) => prev.filter((p) => p.id !== playerId));
    });

    hub.on("PlayerListUpdated", (list: Player[]) => {
      console.log("Player list updated:", list);
      setPlayers(list);
    });

    hub.on("GameStarted", (gameId: string) => {
      console.log("RoomHub: GameStarted received, navigating to:", gameId);
      navigate(`/multiplayer/${roomCode}/${gameId}`);
    });

    const start = async () => {
      try {
        await hub.start();
        setRoomHubConnected(true);
        console.log("Connected to RoomHub");

        // Join room after connection is established
        const me = players.find((p) => p.id === myPlayerId);
        await hub.invoke(
          "JoinRoom",
          roomCode,
          myPlayerId,
          me?.displayName ?? "Player"
        );
        console.log("Joined room successfully");
      } catch (err) {
        console.error("RoomHub connection error:", err);
        setRoomHubConnected(false);
      }
    };

    start();

    // Cleanup only on unmount
    return () => {
      console.log("Cleaning up RoomHub connection");
      if (hub.state === signalR.HubConnectionState.Connected) {
        hub.invoke("LeaveRoom", roomCode).catch(console.error);
      }
      hub.stop();
    };
  }, [roomCode, myPlayerId]); // Remove players and navigate from dependencies

  // -----------------------------------------------------------
  // 4. Connect to GameHub IMMEDIATELY
  // -----------------------------------------------------------
  useEffect(() => {
    if (!roomCode || !myPlayerId) return;
    if (gameHubRef.current) return;

    const hub = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/gameHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    gameHubRef.current = hub;

    // Connection state handlers
    hub.onclose(() => {
      console.log("GameHub disconnected");
      setGameHubConnected(false);
    });

    hub.onreconnecting(() => {
      console.log("GameHub reconnecting...");
      setGameHubConnected(false);
    });

    hub.onreconnected(() => {
      console.log("GameHub reconnected");
      setGameHubConnected(true);
    });

    // Listen for game start event
    hub.on("GameStarted", (gameId: string) => {
      console.log("GameHub: GameStarted received, navigating to:", gameId);
      navigate(`/multiplayer/${roomCode}/${gameId}`);
    });

    hub.on("RoundStarted", (data: any) => {
      console.log("RoundStarted:", data);
    });

    const start = async () => {
      try {
        await hub.start();
        setGameHubConnected(true);
        console.log("Connected to GameHub");
      } catch (err) {
        console.error("GameHub error:", err);
        setGameHubConnected(false);
      }
    };

    start();

    // Cleanup only on unmount
    return () => {
      console.log("Cleaning up GameHub connection");
      hub.stop();
    };
  }, [roomCode, myPlayerId]); // Remove navigate from dependencies

  // -----------------------------------------------------------
  // TOGGLE READY
  // -----------------------------------------------------------
  const handleToggleReady = async () => {
    if (!myPlayerId) return;
    if (!roomHubConnected) {
      setError("Not connected to server. Please wait...");
      return;
    }

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
    if (!gameHubConnected) {
      setError("Not connected to game server. Please wait...");
      return;
    }

    try {
      console.log("Starting game for room:", roomCode);
      await gameHubRef.current?.invoke("StartGame", roomCode);
    } catch (err) {
      console.error("StartGame failed:", err);
      setError("Failed to start game");
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

      if (roomHubRef.current?.state === signalR.HubConnectionState.Connected) {
        await roomHubRef.current?.invoke("LeaveRoom", roomCode);
      }
    } catch (err) {
      console.error("Failed to leave room:", err);
    } finally {
      roomHubRef.current?.stop();
      gameHubRef.current?.stop();
      navigate("/joinroom");
    }
  };

  const allReady = players.length > 0 && players.every((p) => p.isReady);

  if (loading) return <p className="text-white">Loading room...</p>;
  if (error) return <p className="text-red-400">{error}</p>;

  return (
    <main className="text-white flex flex-col items-center p-8">
      <h1 className="text-3xl font-bold mb-6">Room {roomCode}</h1>

      {/* Connection Status */}
      <div className="mb-4 text-sm">
        <span className={roomHubConnected ? "text-green-400" : "text-yellow-400"}>
          RoomHub: {roomHubConnected ? "Connected" : "Connecting..."}
        </span>
        {" | "}
        <span className={gameHubConnected ? "text-green-400" : "text-yellow-400"}>
          GameHub: {gameHubConnected ? "Connected" : "Connecting..."}
        </span>
      </div>

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
          disabled={!roomHubConnected}
          className={`px-6 py-3 rounded-xl transition ${
            roomHubConnected
              ? "bg-green-600 hover:bg-green-700"
              : "bg-gray-500 cursor-not-allowed"
          }`}
        >
          {players.find((p) => p.id === myPlayerId)?.isReady
            ? "Unready"
            : "Get Ready"}
        </button>

        <button
          onClick={handleStartGame}
          disabled={!allReady || !gameHubConnected}
          className={`px-6 py-3 rounded-xl transition ${
            allReady && gameHubConnected
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