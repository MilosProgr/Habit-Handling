import { Habit } from "../../habits/model/habit";

export interface Entry {
    id: string; // Unique identifier for the entry
    habit?: Habit;
    value: number; // The value of the entry, e.g., "5" for a habit with a numeric value
    notes?: string
    source?: EntrySource; // Source of the entry (manual, automation, file import)
    externalId?: string; // External ID if the entry is from an external source
    isArchived?: boolean; // Indicates if the entry is archived
    date: string; // ISO date string
    createdAtUtc?: string; // ISO date string for when the entry was created
    updatedAtUtc?: string; // ISO date string for when the entry was last updated

    links?: [
        {
            href: string;
            rel: string;
            method: string;
        }
    ];

}

export interface CreateEntry {
    habitId: string;
    value: number; // The value of the entry, e.g., "5" for a habit with a numeric value
    notes?: string
    date: string; // ISO date string

}

export enum EntrySource {
    manual = 0,
    automation = 1,
    FileImport = 2,
}