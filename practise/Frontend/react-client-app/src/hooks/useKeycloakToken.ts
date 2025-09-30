// src/hooks/useKeycloakToken.ts
import { useKeycloak } from "@react-keycloak/web";
import { useState, useEffect, useCallback } from "react";

/**
 * Custom hook koji vraća trenutni Keycloak token ili null
 * i automatski refreshuje token pre nego što istekne.
 */
export function useKeycloakToken(): string | null {
    const { keycloak } = useKeycloak();
    const [token, setToken] = useState<string | null>(null);

    const refreshToken = useCallback(async () => {
        if (keycloak?.authenticated) {
            try {
                // updateToken(30) -> osveži token ako ističe za manje od 30 sekundi
                const refreshed = await keycloak.updateToken(30);
                if (refreshed) {
                    console.log("Token refreshed");
                }
                setToken(keycloak.token ?? null);
            } catch (err) {
                console.error("Failed to refresh token", err);
                setToken(null);
            }
        } else {
            setToken(null);
        }
    }, [keycloak]);

    useEffect(() => {
        // postavi token odmah
        if (keycloak?.authenticated) {
            setToken(keycloak.token ?? null);
        }

        // interval za automatsko osvežavanje tokena svakih 20 sekundi
        const interval = setInterval(() => {
            refreshToken();
        }, 20000);

        return () => clearInterval(interval);
    }, [keycloak, refreshToken]);

    return token;
}
