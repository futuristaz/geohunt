import { useNavigate } from "react-router-dom"

export default function Start() {
    const navigate = useNavigate();

    return (
    <main className="text-white flex flex-col items-center justify-center">
      <section className="text-center p-8">
        <h1 className="text-3xl font-bold mb-4">
          Choose round amount:
        </h1>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button
                onClick={() => navigate('/game')}
                className="px-6 py-3 bg-transparent border border-white font-semibold rounded-xl hover:bg-white/10 transition"
            >
                1 round
            </button>
        </div>
      </section>
    </main>
  )
}