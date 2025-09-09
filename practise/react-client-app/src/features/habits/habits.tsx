// src/features/habits/HabitPage.tsx
import React, { useEffect } from "react";
import { Link as RouterLink } from "react-router-dom";
import { useHabits } from "./useHabits";

export const HabitPage: React.FC = () => {
  const { habits, isLoading, error, removeHabit } = useHabits();

  useEffect(() => {
    console.log("Current habits in HabitPage:", habits);
  }, [habits]);

  const handleDelete = async (habitId: string, habitName: string) => {
    const confirmed = window.confirm(`Are you sure you want to delete "${habitName}"?`);
    if (!confirmed) return;
    await removeHabit(habitId);
  };

  if (error) return <div className="text-red-600 p-4">{error}</div>;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-semibold">My Habits</h1>
        <RouterLink
          to="/habits/create"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Create Habit
        </RouterLink>
      </div>

      {isLoading ? (
        <div>Loading...</div>
      ) : habits.length === 0 ? (
        <div className="text-gray-500">No habits yet. Create your first habit!</div>
      ) : (
        <div className="space-y-4">
          {habits.map((habit) => (
            <div
              key={habit.id}
              className="flex justify-between items-center p-4 bg-white rounded shadow"
            >
              <div>
                <h2 className="font-medium">{habit.name}</h2>
                {habit.description && <p className="text-sm text-gray-600">{habit.description}</p>}
              </div>
              <button
                onClick={() => handleDelete(habit.id, habit.name)}
                className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600"
                disabled={isLoading}
              >
                Delete
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default HabitPage;
