import DailyActivityHeatmap from "../features/entries/DailyActivityHeatmap";
// import QuickEntry from "../features/entries/QuickEntry";
// import { useState } from "react";
import { useLatestEntries } from "../features/entries/LatestEntries";

const Dashboard = () => {
    // const [sidebarOpen, setSidebarOpen] = useState(true);
    const { entries, isLoading, error } = useLatestEntries(10); // poziv hooka

    return (
        <div style={{ display: "flex" }}>
            <main style={{ padding: "20px", flex: 1 }}>
                <h1>Dashboard</h1>
                <div style={{ display: "flex", gap: "20px" }}>
                    <div style={{ flex: 2 }}>
                        <DailyActivityHeatmap />

                        {/* Latest Entries prikaz */}
                        <div style={{ marginTop: "20px" }}>
                            <h3>Latest Entries</h3>
                            {isLoading && <p>Loading entries...</p>}
                            {error && <p style={{ color: "red" }}>{error}</p>}
                            {!isLoading && !error && entries.length === 0 && <p>No entries found</p>}
                            {!isLoading && !error && entries.length > 0 && (
                                <div>
                                    {entries.map((entry) => (
                                        <div
                                            key={entry.id}
                                            style={{
                                                padding: "10px",
                                                borderBottom: "1px solid #ddd",
                                            }}
                                        >
                                            <strong>{entry.value}</strong>
                                            <div>
                                                {entry.createdAtUtc
                                                    ? new Date(entry.createdAtUtc).toLocaleDateString()
                                                    : "No date"}
                                            </div>
                                            {entry.notes && <div>{entry.notes}</div>}
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Sidebar / QuickEntry može da ide ovde */}
                    {/* <div style={{ flex: 1 }}>
              <QuickEntry />
          </div> */}
                </div>
            </main>
        </div>
    );
};


export default Dashboard;
