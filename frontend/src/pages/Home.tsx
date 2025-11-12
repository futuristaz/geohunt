import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

export default function Home() {
  const navigate = useNavigate();
  const [loggedIn, setLoggedIn] = useState<boolean | null>(null);
  const [username, setUsername] = useState('');

  useEffect(() => {
    const checkLogin = async () => {
      try {
        const res = await fetch('/api/User/me', {
          method: 'GET',
          credentials: 'include' // include cookies for authentication
        });
        if (res.ok) {
          const data = await res.json();
          setLoggedIn(true);
          setUsername(data.username);
        } else {
          setLoggedIn(false);
          navigate('/login', { replace: true });
        }
      } catch (error) {
        console.error('Error checking login status:', error);
        setLoggedIn(false);
        navigate('/login', { replace: true });
      }
    };
    checkLogin();
  }, [navigate])

  const handleLogout = async () => {
    try {
      const res = await fetch('/api/Account/logout', {
        method: 'POST',
        credentials: 'include' // include cookies for authentication
      });
      if (res.ok) {
        setLoggedIn(false);
        navigate('/login', { replace: true });
      } else {
        console.error('Logout failed');
        alert('Logout failed. Please try again.');
      }
    } catch (error) {
      console.error('Error during logout:', error);
      alert('An error occurred during logout. Please try again.');
    }
  };

  return (
    <main className="text-white flex flex-col items-center justify-center">
      <h1>{username}</h1>
      <button
        onClick={handleLogout}
        className="absolute top-4 right-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition"
      >
        Logout
      </button>
      <section className="text-center p-8">
        <h1 className="text-5xl font-extrabold mb-4">
          Welcome to GeoHunt 🌍
        </h1>

        <p className="text-lg text-blue-100 mb-8">
          Explore the world, find hidden treasures, and track your progress with our interactive map-based adventure game.
        </p>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <button
            onClick={() => navigate('/start')}
            className="px-6 py-3 bg-white text-blue-700 font-semibold rounded-xl shadow hover:bg-blue-50 transition"
          >
            Start Playing
          </button>

          <button
            onClick={() => navigate('/about')}
            className="px-6 py-3 bg-transparent border border-white font-semibold rounded-xl hover:bg-white/10 transition"
          >
            Learn More
          </button>
        </div>
      </section>
    </main>
  )
}
