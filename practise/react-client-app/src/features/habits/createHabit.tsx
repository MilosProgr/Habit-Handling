// HabitCreatePage.tsx
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useResourceService } from "../../generics/useGenericResourse";
import { useKeycloakToken } from "../../hooks/useKeycloakToken";
import { Habit, FrequencyType, HabitType } from "./model/habit";

export const HabitCreatePage: React.FC = () => {
    const token = useKeycloakToken();
    const { addResource } = useResourceService<Habit>(token ?? undefined);
    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [type, setType] = useState<HabitType>(1);
    const [frequencyType, setFrequencyType] = useState<FrequencyType>(1);
    const [timesPerPeriod, setTimesPerPeriod] = useState(1);
    const [targetValue, setTargetValue] = useState(1);
    const [unit, setUnit] = useState("sessions");
    const [endDate, setEndDate] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    if (!token) return <p className="text-gray-600">Please login first.</p>;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError(null);

        const payload = {
            name,
            description,
            type,
            frequency: { type: frequencyType, timesPerPeriod },
            target: { value: targetValue, unit },
            endDate: endDate || undefined,
        };

        const success = await addResource("/api/habits", payload);
        setIsLoading(false);

        if (success) {
            navigate("/habits");
        } else {
            setError("Failed to create habit");
        }
    };

    return (
        <div className="max-w-xl mx-auto p-6">
            <h1 className="text-2xl font-bold mb-4">Create New Habit</h1>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block">Name</label>
                    <input value={name} onChange={e => setName(e.target.value)} className="border rounded w-full" required />
                </div>
                <div>
                    <label className="block">Description</label>
                    <textarea value={description} onChange={e => setDescription(e.target.value)} className="border rounded w-full" />
                </div>
                <div>
                    <label className="block">Type</label>
                    <select value={type} onChange={e => setType(Number(e.target.value) as HabitType)} className="border rounded w-full">
                        <option value={1}>Binary</option>
                        <option value={2}>Measurable</option>
                    </select>
                </div>
                <div className="flex gap-4">
                    <div className="flex-1">
                        <label className="block">Frequency Type</label>
                        <select value={frequencyType} onChange={e => setFrequencyType(Number(e.target.value) as FrequencyType)} className="border rounded w-full">
                            <option value={1}>Daily</option>
                            <option value={2}>Weekly</option>
                            <option value={3}>Monthly</option>
                        </select>
                    </div>
                    <div className="flex-1">
                        <label className="block">Times Per Period</label>
                        <input type="number" min="1" value={timesPerPeriod} onChange={e => setTimesPerPeriod(Number(e.target.value))} className="border rounded w-full" required />
                    </div>
                </div>
                <div className="flex gap-4">
                    <div className="flex-1">
                        <label className="block">Target Value</label>
                        <input type="number" min="1" value={targetValue} onChange={e => setTargetValue(Number(e.target.value))} className="border rounded w-full" required />
                    </div>
                    <div className="flex-1">
                        <label className="block">Unit</label>
                        <select value={unit} onChange={e => setUnit(e.target.value)} className="border rounded w-full">
                            <option value="sessions">Sessions</option>
                            <option value="minutes">Minutes</option>
                            <option value="reps">Reps</option>
                        </select>
                    </div>
                </div>
                <div>
                    <label className="block">End Date (optional)</label>
                    <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)} className="border rounded w-full" />
                </div>

                {error && <p className="text-red-600">{error}</p>}

                <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded" disabled={isLoading}>
                    {isLoading ? "Saving..." : "Save Habit"}
                </button>
            </form>
        </div>
    );
};

export default HabitCreatePage;
