import { Link } from "react-router-dom";

const Sidebar = () => {
    return (
        <nav style={{ width: "200px", background: "#f3f3f3", padding: "20px", height: "100vh" }}>
            <h2>DevHabit</h2>
            <ul style={{ listStyle: "none", padding: 0 }}>
                <li><Link to="/dashboard">Dashboard</Link></li>
                <li><Link to="/habits">My Habits</Link></li>
                <li><Link to="/entries">My Entries</Link></li>
                <li><Link to="/tags">My Tags</Link></li>
            </ul>
        </nav>
    );
};

export default Sidebar;
