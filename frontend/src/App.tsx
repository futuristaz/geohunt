import { Routes, Route } from 'react-router-dom'
import Home from './pages/Home'
import Start from './pages/Start'
import Game from './pages/Game'
import Results from './pages/Results'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/start" element={<Start />} />
      <Route path="/game" element={<Game />} />
      <Route path="/results/:gameId" element={<Results />} />
    </Routes>
  )
}