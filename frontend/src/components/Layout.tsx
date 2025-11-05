import { Link, Outlet } from 'react-router-dom';

export default function Layout() {
    return (
        <div>
            <nav className='flex gap-10 p-4 bg-blue-950'>
                <Link to="/">Home</Link>
                <Link to="/about">About</Link>
                <Link to="/users">Users</Link>
            </nav>
            <Outlet />
        </div>
    )
}