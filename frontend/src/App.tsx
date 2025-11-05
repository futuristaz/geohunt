import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import Home from './pages/Home'
import Start from './pages/Start'
import Game from './pages/Game'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/start" element={<Start />} />
        <Route path="/game" element={<Game />} />
      </Route>
    </Routes>
  )
}