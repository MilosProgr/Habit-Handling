// src/generics/useResourceService.ts
import { API_BASE_URL } from "../api/config";
import type { Link } from "../types/models/link";
import { useCallback } from "react";

export function useResourceService<TResource>(token?: string) {

    const fetchWithAuth = useCallback(
        async <T>(url: string, options: RequestInit = {}, acceptHeader = "application/json"): Promise<T> => {
            if (!token) throw new Error("No token available");

            const headers: HeadersInit = {
                ...options.headers,
                Authorization: `Bearer ${token}`,
                Accept: acceptHeader,
            };

            const response = await fetch(url, { ...options, headers });
            if (!response.ok) throw new Error(`Request failed: ${response.status}`);
            return response.json();
        },
        [token]
    );

    const getResource = useCallback(
        async (path: string, acceptHeader: string = "application/json"): Promise<TResource | null> => {
            if (!token) return null;

            try {
                const fullPath = path.startsWith("http") ? path : `${API_BASE_URL}${path}`;
                return await fetchWithAuth<TResource>(fullPath, {}, acceptHeader);
            } catch (error) {
                console.error("[useResourceService] Failed to fetch resource:", error);
                return null;
            }
        },
        [fetchWithAuth, token]
    );

    const updateResource = useCallback(
        async (data: Partial<TResource>, link: Link): Promise<boolean> => {
            if (!token) return false;
            try {
                if (!link?.href || link.method.toUpperCase() !== "PUT") throw new Error("Invalid link");

                const href = link.href.startsWith("http") ? link.href : `${API_BASE_URL}${link.href}`;
                await fetchWithAuth<TResource>(href, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(data),
                });
                return true;
            } catch (error) {
                console.error("[useResourceService] Failed to update resource:", error);
                return false;
            }
        },
        [fetchWithAuth, token]
    );

    const addResource = useCallback(
        async (
            path: string,
            data: Partial<TResource>,
            contentType = "application/json",
            acceptType = "application/json",
            idempotencyKey?: string
        ): Promise<TResource | null> => {
            if (!token) return null;

            try {
                const headers: Record<string, string> = {
                    "Content-Type": contentType,
                    Accept: acceptType,
                    Authorization: `Bearer ${token}`,
                };
                if (idempotencyKey) headers["Idempotency-Key"] = idempotencyKey;

                const fullPath = path.startsWith("http") ? path : `${API_BASE_URL}${path}`;
                return await fetchWithAuth<TResource>(fullPath, {
                    method: "POST",
                    headers,
                    body: JSON.stringify(data),
                });
            } catch (error) {
                console.error("[useResourceService] Failed to create resource:", error);
                return null;
            }
        },
        [fetchWithAuth, token]
    );

    const deleteResource = useCallback(
        async (linkOrPath: Link | string): Promise<boolean> => {
            if (!token) return false;

            try {
                let href: string;
                let method = "DELETE";

                if (typeof linkOrPath === "string") {
                    href = linkOrPath.startsWith("http") ? linkOrPath : `${API_BASE_URL}${linkOrPath}`;
                } else {
                    if (!linkOrPath?.href || linkOrPath.method.toUpperCase() !== "DELETE") {
                        throw new Error("Invalid delete link");
                    }
                    href = linkOrPath.href.startsWith("http") ? linkOrPath.href : `${API_BASE_URL}${linkOrPath.href}`;
                    method = linkOrPath.method;
                }

                await fetchWithAuth(href, { method });
                return true;
            } catch (error) {
                console.error("[useResourceService] Failed to delete resource:", error);
                return false;
            }
        },
        [fetchWithAuth, token]
    );

    return { getResource, updateResource, addResource, deleteResource };
}
