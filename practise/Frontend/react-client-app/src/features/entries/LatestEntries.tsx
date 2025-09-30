// src/features/entries/useLatestEntries.ts
import { useState, useEffect, useCallback } from "react";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";
import { Entry } from "./model/Entry";
import { useResourceService } from "../../generics/useGenericResourse";

export function useLatestEntries(pageSize = 10) {
    const token = useKeycloakToken();
    const { getResource } = useResourceService<{ data: Entry[] }>(token ?? undefined);

    const [entries, setEntries] = useState<Entry[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchEntries = useCallback(async () => {
        if (!token) {
            setError("No authentication token");
            return;
        }

        try {
            setIsLoading(true);
            setError(null);

            const query = new URLSearchParams({
                page_size: pageSize.toString(),
                fields: "id,value,createdAtUtc,notes",
            });

            const result = await getResource(`/api/entries?${query.toString()}`, "application/json");

            if (!result?.data || !Array.isArray(result.data)) {
                setError("No entries received");
                setEntries([]);
                return;
            }

            const sorted = [...result.data].sort((a, b) => {
                const dateA = a.createdAtUtc ? new Date(a.createdAtUtc).getTime() : 0;
                const dateB = b.createdAtUtc ? new Date(b.createdAtUtc).getTime() : 0;
                return dateB - dateA;
            });

            setEntries(sorted);
        } catch (e) {
            console.error(e);
            setError("Failed to load entries");
            setEntries([]);
        } finally {
            setIsLoading(false);
        }
    }, [getResource, pageSize, token]);

    useEffect(() => {
        fetchEntries();
    }, [fetchEntries]);

    return { entries, isLoading, error, refresh: fetchEntries };
}
