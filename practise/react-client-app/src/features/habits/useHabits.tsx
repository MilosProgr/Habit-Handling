// src/features/habits/useHabits.ts
import { useState, useEffect, useCallback } from "react";
import { Habit } from "./model/habit";
import { PagedHateoasResponse } from "../../types/models/PagedHaiteoasResponse";
import { useResourceService } from "../../generics/useGenericResourse";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";

export function useHabits(pageSize = 6) {
    const token = useKeycloakToken();
    const { getResource, addResource, deleteResource } =
        useResourceService<PagedHateoasResponse<Habit> | Habit>(token ?? undefined);

    const [habits, setHabits] = useState<Habit[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Fetch habits
    const fetchHabits = useCallback(async () => {
        if (!token) return;

        setIsLoading(true);
        setError(null);

        try {
            const result = await getResource(
                `/habits?page_size=${pageSize}&fields=id,name,description,target,frequency,endDate,milestone`,
                "application/vnd.dev-habit.hateoas+json"
            );



            // Ako vraća HATEOAS wrapper, izvuci samo data
            if (result && "data" in result) {
                setHabits(result.data ?? []);
                console.log("Fetched habits:", result);
            } else if (result) {
                // Ako vraća direktno Habit
                setHabits([result as Habit]);
            } else {
                setHabits([]);
            }
        } catch (err) {
            console.error(err);
            setError("Failed to load habits");
        } finally {
            setIsLoading(false);
        }
    }, [getResource, token, pageSize]);

    // Add habit
    const createHabit = useCallback(
        async (newHabit: Partial<Habit>) => {
            if (!token) return null;
            setIsLoading(true);
            try {
                const created = await addResource("/habits", newHabit);
                const habitToAdd: Habit =
                    "data" in created!
                        ? (Array.isArray(created.data) ? created.data[0] : created.data) // uzmi prvi ako je array
                        : (created as Habit);

                if (habitToAdd) setHabits((prev) => [...prev, habitToAdd]);
                return habitToAdd;
            } catch (err) {
                console.error(err);
                setError("Failed to create habit");
                return null;
            } finally {
                setIsLoading(false);
            }
        },
        [addResource, token]
    );

    // Delete habit
    const removeHabit = useCallback(
        async (habitId: string) => {
            if (!token) return false;
            setIsLoading(true);
            try {
                const success = await deleteResource(`/habits/${habitId}`);
                if (success) setHabits((prev) => prev.filter((h) => h.id !== habitId));
                return success;
            } catch (err) {
                console.error(err);
                setError("Failed to delete habit");
                return false;
            } finally {
                setIsLoading(false);
            }
        },
        [deleteResource, token]
    );

    useEffect(() => {
        fetchHabits();
    }, [fetchHabits]);

    return { habits, isLoading, error, fetchHabits, createHabit, removeHabit };
}
