// src/interfaces/habits.ts

// export type FrequencyType = 'None' | 'Daily' | 'Weekly' | 'Monthly';
// export type HabitStatus = 'Active' | 'Paused' | 'Completed';
// export type HabitType = 'Positive' | 'Negative';
export type AutomationSource = 0 | 1; // None=0, GitHub=1
export type HabitType = 0 | 1 | 2; // None=0, Binary=1, Measurable=2
export type FrequencyType = 0 | 1 | 2 | 3; // None=0, Daily=1, Weekly=2, Monthly=3

export interface Frequency {
    type: FrequencyType;
    timesPerPeriod: number;
}

export interface Target {
    value: number;
    unit: string;
}

export interface Milestone {
    target: number;
    current: number;
}

export interface CreateHabitDto {
    name: string;
    description?: string;
    type: HabitType;
    frequency: Frequency;
    target: Target;
    endDate?: string;        // ISO date string yyyy-MM-dd
    milestone?: Milestone;
    automationSource?: AutomationSource;
}

export interface UpdateHabitDto {
    id?: string;
    name: string;
    description?: string;
    type: HabitType;
    frequency: Frequency;
    target: Target;
    endDate?: string;
    milestone?: { target: number };
    automationSource?: AutomationSource;
}
export interface Habit {
    id: string;
    name: string;
    description?: string;
    type: HabitType;
    frequency: Frequency;
    target: Target;
    // status: HabitStatus;
    isArchived: boolean;
    endDate?: string; // ISO format (yyyy-MM-dd)
    milestone?: Milestone;
    createdAtUtc: string;
    updatedAtUtc?: string;
    lastCompletedAtUtc?: string;
    automationSource?: AutomationSource;

    links?: [
        {
            href: string;
            rel: string;
            method: string;
        }
    ];
}
