import { useNavigate } from 'react-router-dom'

export default function Home() {
  const navigate = useNavigate()

  return (
    <main className="text-white flex flex-col items-center justify-center">
      <section className="text-center p-8">
        <h1 className="text-5xl font-extrabold mb-4">
          Welcome to GeoHunt 🌍
        </h1>

        <p className="text-lg text-blue-100 mb-8">
          Explore the world, find hidden treasures, and track your progress with our interactive map-based adventure game.
        </p>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <button
            onClick={() => navigate('/game')}
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
