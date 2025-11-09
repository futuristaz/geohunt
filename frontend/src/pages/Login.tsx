
import { User, Lock } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function Login() {
    const navigate = useNavigate();
    
    return (
      <div>
        <section className='text-center pt-8'>
          <h1 className='text-5xl font-black'>GeoHunt</h1>
        </section>
        <section className='text-center pt-8'>
          <h2 className='text-2xl font-semibold'>Please log in to start exploring the world</h2>
        </section>
        <section className='flex flex-col items-center justify-center pt-8'>
          <div className="relative mb-4 w-64">
            <User className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input type="text" placeholder="Username" className="w-full pl-10 pr-4 py-2 border rounded-lg" />
          </div>
          <div className="relative mb-4 w-64">
            <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input type="password" placeholder="Password" className="w-full pl-10 pr-4 py-2 border rounded-lg" />
          </div>
          <button
            onClick={() => console.log('Log in')}
            className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition"
          >
            Log In
          </button>
          <p className='pt-4'>
            Don't have an account?{' '}
            <button 
              onClick={() => navigate('/signup')} 
              className="text-blue-600 hover:text-blue-700 underline"
            >
              Sign up
            </button>
          </p>
        </section>
      </div>
    )
  }

