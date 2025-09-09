// CreateEntriesPage.tsx
import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { EntrySource, CreateEntry } from "./model/Entry";
import { Habit } from "../habits/model/habit";
import { useResourceService } from "../../generics/useGenericResourse";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";
import { useEntries } from "./useEntries";

export const CreateEntriesPage: React.FC = () => {
    const token = useKeycloakToken();
    const navigate = useNavigate();
    const { createEntry } = useEntries();
    const { getResource } = useResourceService<{ data: Habit[] }>(token ?? undefined);

    const [habits, setHabits] = useState<Habit[]>([]);
    const [habitId, setHabitId] = useState<string>("");
    const [value, setValue] = useState<number>(1);
    const [notes, setNotes] = useState<string>("");
    const [date, setDate] = useState<string>("");
    const [source, setSource] = useState<EntrySource>(EntrySource.manual);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Fetch habits for dropdown
    const fetchHabits = useCallback(async () => {
        if (!token) return;
        try {
            const result = await getResource(
                "/api/habits?page_size=15&fields=id,name",
                "application/vnd.dev-habit.hateoas+json"
            );
            if (result?.data) {
                setHabits(result.data);
                if (result.data.length > 0) setHabitId(result.data[0].id);
            }
        } catch (err) {
            console.error("Failed to load habits", err);
            setError("Failed to load habits");
        }
    }, [getResource, token]);

    useEffect(() => {
        fetchHabits();
    }, [fetchHabits]);

    if (!token) return <p className="text-gray-600">Please login first.</p>;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError(null);

        // const dateUtc = new Date(date).toISOString();
        // const dateOnly = date; // jer je već "yyyy-MM-dd" iz <input type="date" />

        const payload: CreateEntry = {
            habitId,
            value,
            notes: notes || undefined,
            date: date

        };

        const success = await createEntry(payload);
        setIsLoading(false);

        if (success) {
            navigate("/entries");
        } else {
            setError("Failed to create entry");
        }
    };

    return (
        <div className="max-w-xl mx-auto p-6">
            <h1 className="text-2xl font-bold mb-4">Create New Entry</h1>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block font-medium mb-1">Habit</label>
                    <select
                        value={habitId}
                        onChange={e => setHabitId(e.target.value)}
                        className="border rounded w-full p-2"
                        required
                    >
                        {habits.map(habit => (
                            <option key={habit.id} value={habit.id}>
                                {habit.name}
                            </option>
                        ))}
                    </select>
                </div>

                <div>
                    <label className="block font-medium mb-1">Value</label>
                    <input
                        type="number"
                        min="1"
                        value={value}
                        onChange={e => setValue(Number(e.target.value))}
                        className="border rounded w-full p-2"
                        required
                    />
                </div>

                <div>
                    <label className="block font-medium mb-1">Notes</label>
                    <textarea
                        value={notes}
                        onChange={e => setNotes(e.target.value)}
                        className="border rounded w-full p-2"
                    />
                </div>

                <div>
                    <label className="block font-medium mb-1">Date</label>
                    <input
                        type="date"
                        value={date}
                        onChange={e => setDate(e.target.value)}
                        className="border rounded w-full p-2"
                        required
                    />
                </div>

                <div>
                    <label className="block font-medium mb-1">Source</label>
                    <select
                        value={source}
                        onChange={e => setSource(Number(e.target.value))}
                        className="border rounded w-full p-2"
                    >
                        <option value={EntrySource.manual}>Manual</option>
                        <option value={EntrySource.automation}>Automation</option>
                        <option value={EntrySource.FileImport}>File Import</option>
                    </select>
                </div>

                {error && <p className="text-red-600">{error}</p>}

                <button
                    type="submit"
                    className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
                    disabled={isLoading}
                >
                    {isLoading ? "Saving..." : "Save Entry"}
                </button>
            </form>
        </div>
    );
};

export default CreateEntriesPage;
