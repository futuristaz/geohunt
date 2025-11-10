import { User, Lock } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function Login() {
    const navigate = useNavigate();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleLogin = async () => {
      try {
        setError(''); // Clear any previous errors
        
        const res = await fetch('/api/Account/login', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ username, password }),
        });

        if (res.ok) {
          // Login successful, redirect to home
          navigate('/');
        } else {
          // Login failed
          const data = await res.json();
          setError(data.message || 'Login failed. Please check your credentials.');
        }
      } catch (err) {
        console.error('Login error:', err);
        setError('An error occurred. Please try again.');
      }
    };

    const handleSubmit = (e: React.FormEvent) => {
      e.preventDefault();
      handleLogin();
    };

    return (
      <div>
        <section className='text-center pt-8'>
          <h1 className='text-5xl font-black'>GeoHunt</h1>
        </section>
        <section className='text-center pt-8'>
          <h2 className='text-2xl font-semibold'>Please log in to start exploring the world</h2>
        </section>
        <form onSubmit={handleSubmit} className='flex flex-col items-center justify-center pt-8'>
          {error && (
            <div className="mb-4 w-64 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg">
              {error}
            </div>
          )}
          <div className="relative mb-4 w-64">
            <User className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-100" size={20} />
            <input 
              type="text" 
              placeholder="Username" 
              className="w-full pl-10 pr-4 py-2 border rounded-lg text-gray-100"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>
          <div className="relative mb-4 w-64">
            <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-100" size={20} />
            <input 
              type="password" 
              placeholder="Password" 
              className="w-full pl-10 pr-4 py-2 border rounded-lg text-gray-100"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <button
            type="submit"
            className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition"
          >
            Log In
          </button>
          <p className='pt-4'>
            Don't have an account?{' '}
            <button 
              type="button"
              onClick={() => navigate('/signup')} 
              className="text-blue-600 hover:text-blue-700 underline"
            >
              Sign up
            </button>
          </p>
        </form>
      </div>
    )
  }