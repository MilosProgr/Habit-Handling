import { useEffect, useMemo } from "react";
import { format, parseISO, eachDayOfInterval, startOfYear } from "date-fns";
import { useEntryStats } from "./useEntryStats";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";

const colorLevels = [
    "#ebf4ff",
    "#90cdf4",
    "#4299e1",
    "#3182ce",
    "#2b6cb0"
];

const DailyActivityHeatmap = () => {
    const token = useKeycloakToken();
    const { stats, fetchStats, isLoading, error } = useEntryStats();

    useEffect(() => {
        if (token) fetchStats();
    }, [token, fetchStats]);

    const dailyStats = stats?.dailyStats ?? [];

    // Popuni sve dane od početka godine
    const allDays = useMemo(() => {
        if (!dailyStats.length) return [];

        const startDate = startOfYear(new Date());
        const endDate = new Date();
        const daysArray = eachDayOfInterval({ start: startDate, end: endDate });

        return daysArray.map(date => {
            const stat = dailyStats.find(d => d.date === format(date, "yyyy-MM-dd"));
            return {
                date: format(date, "yyyy-MM-dd"),
                count: stat?.count ?? 0
            };
        });
    }, [dailyStats]);

    const maxCount = useMemo(() => {
        if (!allDays.length) return 1;
        return Math.max(...allDays.map(d => d.count));
    }, [allDays]);

    const daysWithColors = useMemo(() => {
        return allDays.map(d => ({
            ...d,
            color: d.count === 0
                ? colorLevels[0]
                : colorLevels[Math.min(Math.ceil((d.count / maxCount) * (colorLevels.length - 1)), colorLevels.length - 1)]
        }));
    }, [allDays, maxCount]);

    if (isLoading) return <div>Loading activity heatmap...</div>;
    if (error) return <div>Error loading activity heatmap: {error}</div>;
    if (!stats || !stats.dailyStats) return null;

    return (
        <div style={{ display: "flex", gap: "20px", alignItems: "flex-start" }}>
            <div>
                <h3><strong>Daily Activity</strong></h3>
                <div
                    style={{
                        display: "grid",
                        gridTemplateRows: "repeat(7, 20px)",
                        gridAutoFlow: "column",
                        gap: "4px"
                    }}
                >
                    {daysWithColors.map(day => (
                        <div
                            key={day.date}
                            title={`${format(parseISO(day.date), "EEE, MMM d, yyyy")}: ${day.count} entries`}
                            style={{
                                width: "20px",
                                height: "20px",
                                backgroundColor: day.color,
                                borderRadius: "4px",
                            }}
                        />
                    ))}
                </div>

                {/* Legenda */}
                <div style={{ display: "flex", alignItems: "center", marginTop: "10px", gap: "5px" }}>
                    <span style={{ fontSize: "0.8rem" }}>Less</span>
                    {colorLevels.map((color, idx) => (
                        <div
                            key={idx}
                            style={{
                                width: "14px",
                                height: "14px",
                                backgroundColor: color,
                                borderRadius: "3px"
                            }}
                        />
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
