// src/features/entries/useEntryStats.ts
import { useState, useCallback } from "react";
import { useResourceService } from "../../generics/useGenericResourse";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";
import { EntryStatsDto } from "./model/EntryStats";

export function useEntryStats() {
    const token = useKeycloakToken(); // uzimamo token iz Keycloak hooka
    const { getResource } = useResourceService<EntryStatsDto>(token ?? undefined);

    const [stats, setStats] = useState<EntryStatsDto | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchStats = useCallback(async () => {
        if (!token) {
            setError("No authentication token available");
            setStats(null);
            return null;
        }

        try {
            setIsLoading(true);
            setError(null);

            const data = await getResource("/api/entries/stats", "application/json");

            if (!data) {
                setError("No data received from server");
                setStats(null);
                return null;
            }

            setStats(data);
            return data;
        } catch (e) {
            console.error(e);
            setError("Failed to load entry statistics");
            setStats(null);
            return null;
        } finally {
            setIsLoading(false);
        }
    }, [getResource, token]);

    return {
        stats,
        fetchStats,
        isLoading,
        error,
    };
}
