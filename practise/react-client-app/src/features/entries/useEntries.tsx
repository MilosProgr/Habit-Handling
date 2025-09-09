// src/features/entries/useEntries.ts
import { useState, useCallback } from "react";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";
import { useResourceService } from "../../generics/useGenericResourse";
import { CreateEntry, Entry } from "./model/Entry";
import { v4 as uuidv4 } from "uuid";


export function useEntries() {
    const token = useKeycloakToken();

    // GET operacije: { data: Entry[] }
    const { getResource } = useResourceService<{ data: Entry[] }>(token ?? undefined);
    // POST/PUT/DELETE operacije: Entry
    const { addResource, updateResource, deleteResource } = useResourceService<Entry>(token ?? undefined);

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    /** Lista entry-ja */
    const listEntries = useCallback(
        async (params?: { pageSize?: number; fields?: string; url?: string }): Promise<Entry[] | null> => {
            if (!token) return null;

            try {
                setIsLoading(true);
                setError(null);

                const url = params?.url ?? "/api/entries";
                const fields = params?.fields ?? "id,value,createdAtUtc,updatedAtUtc,notes,isArchived,source";
                const pageSize = params?.pageSize ?? 10;

                const query = new URLSearchParams({ page_size: pageSize.toString(), fields });
                const fullUrl = `${url}?${query.toString()}`;

                const result = await getResource(fullUrl, "application/json");

                if (!result?.data || !Array.isArray(result.data)) {
                    setError("No data received from server");
                    return [];
                }
                console.log("Ispisi mi rezultat: ", result)

                // sortiranje po datumu silazno
                const sorted = [...result.data].sort((a, b) => {
                    const dateA = a.createdAtUtc ? new Date(a.createdAtUtc).getTime() : 0;
                    const dateB = b.createdAtUtc ? new Date(b.createdAtUtc).getTime() : 0;
                    return dateB - dateA;
                });

                return sorted;
            } catch (e) {
                console.error("[useEntries] Failed to list entries:", e);
                setError("Failed to load entries");
                return [];
            } finally {
                setIsLoading(false);
            }
        },
        [getResource, token]
    );

    /** Dobijanje jednog entry */
    const getEntry = useCallback(
        async (idOrUrl: string): Promise<Entry | null> => {
            if (!token) return null;

            try {
                setIsLoading(true);
                setError(null);

                const url = idOrUrl.startsWith("/api") ? idOrUrl : `/api/entries/${idOrUrl}`;
                const result = await getResource(url, "application/json");
                return result?.data?.[0] ?? null;
            } catch (e) {
                console.error("[useEntries] Failed to fetch entry:", e);
                setError("Failed to fetch entry");
                return null;
            } finally {
                setIsLoading(false);
            }
        },
        [getResource, token]
    );

    /** Kreiranje entry */
    const createEntry = useCallback(
        async (entryData: Partial<CreateEntry>): Promise<Entry | null> => {
            if (!token) return null;

            try {
                setIsLoading(true);
                setError(null);
                const idempotencyKey = uuidv4();
                const result = await addResource(
                    "/api/entries",
                    entryData,
                    "application/json",
                    "application/json",
                    idempotencyKey
                );
                return result ?? null;
            } catch (e) {
                console.error("[useEntries] Failed to create entry:", e);
                setError("Failed to create entry");
                return null;
            } finally {
                setIsLoading(false);
            }
        },
        [addResource, token]
    );

    /** Update entry */
    const updateEntry = useCallback(
        async (id: string, entryData: Partial<Entry>): Promise<boolean> => {
            if (!token) return false;

            try {
                setIsLoading(true);
                setError(null);
                return await updateResource(entryData, { href: `/api/entries/${id}`, method: "PUT", rel: "update" });
            } catch (e) {
                console.error("[useEntries] Failed to update entry:", e);
                setError("Failed to update entry");
                return false;
            } finally {
                setIsLoading(false);
            }
        },
        [updateResource, token]
    );

    /** Delete entry */
    const deleteEntry = useCallback(
        async (id: string): Promise<boolean> => {
            if (!token) return false;

            try {
                setIsLoading(true);
                setError(null);
                return await deleteResource({ href: `/api/entries/${id}`, method: "DELETE", rel: "delete" });
            } catch (e) {
                console.error("[useEntries] Failed to delete entry:", e);
                setError("Failed to delete entry");
                return false;
            } finally {
                setIsLoading(false);
            }
        },
        [deleteResource, token]
    );

    return {
        listEntries,
        getEntry,
        createEntry,
        updateEntry,
        deleteEntry,
        isLoading,
        error,
    };
}
