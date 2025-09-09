import { useEffect } from "react";
import { useEntryStats } from "./useEntryStats";
import { format, parseISO } from "date-fns";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";

const DailyActivityHeatmap = () => {
    const token = useKeycloakToken();
    const { stats, fetchStats, isLoading, error } = useEntryStats();

    useEffect(() => {
        if (token) {
            fetchStats();
        }
    }, [token, fetchStats]); // čekamo da token postoji pre fetch-a

    if (isLoading) return <div>Loading activity heatmap...</div>;
    if (error) return <div>Error loading activity heatmap: {error}</div>;
    if (!stats || !stats.dailyStats) return null;

    const dailyStats = stats.dailyStats ?? [];
    const maxCount = dailyStats.length > 0 ? Math.max(...dailyStats.map(d => d.count)) : 1;

    const colorLevels = [
        "#ebf4ff",
        "#90cdf4",
        "#4299e1",
        "#3182ce",
        "#2b6cb0"
    ];

    const getColorForCount = (count: number) => {
        if (count === 0) return colorLevels[0];
        const index = Math.ceil((count / maxCount) * (colorLevels.length - 1));
        return colorLevels[index];
    };

    const weeks: { date: string, count: number }[][] = [];
    dailyStats.forEach((day, i) => {
        const weekIndex = Math.floor(i / 7);
        if (!weeks[weekIndex]) weeks[weekIndex] = [];
        weeks[weekIndex].push(day);
    });

    return (
        <div style={{ display: "flex", alignItems: "flex-start", gap: "20px" }}>
            <div>
                <h3><strong>Daily Activity</strong></h3>
                <div style={{ display: "flex", gap: "4px" }}>
                    {weeks.map((week, weekIdx) => (
                        <div key={weekIdx} style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                            {week.map(day => (
                                <div
                                    key={day.date}
                                    title={`${format(parseISO(day.date), "MMM d, yyyy")}: ${day.count} entries`}
                                    style={{
                                        width: "20px",
                                        height: "20px",
                                        backgroundColor: getColorForCount(day.count),
                                        borderRadius: "4px",
                                    }}
                                />
                            ))}
                        </div>
                    ))}
                </div>
                <div style={{ display: "flex", alignItems: "center", marginTop: "10px", gap: "5px" }}>
                    <span style={{ fontSize: "0.8rem" }}>Less</span>
                    {colorLevels.map((color, idx) => (
                        <div key={idx} style={{
                            width: "14px",
                            height: "14px",
                            backgroundColor: color,
                            borderRadius: "3px"
                        }} />
                    ))}
                    <span style={{ fontSize: "0.8rem" }}>More</span>
                </div>
            </div>
            <div style={{ alignSelf: "center", fontSize: "0.9rem", color: "#555" }}>
                Current Streak: <strong>{stats.currentStreak}</strong> days
            </div>
        </div>
    );
};

export default DailyActivityHeatmap;
