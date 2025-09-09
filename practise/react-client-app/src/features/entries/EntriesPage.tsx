import React, { useEffect, useState } from "react";
import { Link as RouterLink } from "react-router-dom";
import { useEntries } from "./useEntries";
import type { Entry } from "./model/Entry";
import type { Link as HypermediaLink } from "../../types/models/link";

export const EntriesPage: React.FC = () => {
    const {
        listEntries,
        isLoading,
        error,
        deleteEntry,
        // archiveEntry, unArchiveEntry // dodaj ako ih implementiraš u useEntries
    } = useEntries();

    const [entries, setEntries] = useState<Entry[]>([]);
    const [, setCreateLink] = useState<HypermediaLink | null>(null);

    useEffect(() => {
        loadEntries();
    }, []);

    const loadEntries = async () => {
        try {
            const result = await listEntries({
                pageSize: 6,
                fields: "id,value,notes,date,source,isArchived,createdAtUtc,updatedAtUtc"
            });
            console.log("Entries API result:", result);

            if (result) {
                // result je Entry[], jer listEntries vraća array
                setEntries(result);

                // Ako želiš HATEOAS link, moraš promeniti listEntries da vraća ceo server response
                setCreateLink(null);
            } else {
                setEntries([]);
                setCreateLink(null);
            }
        } catch (e) {
            console.error("[EntriesPage] Failed to load entries", e);
            setEntries([]);
        }
    };

    const handleDelete = async (entry: Entry) => {
        const confirmed = window.confirm(`Delete entry with value "${entry.value}"?`);
        if (!confirmed) return;

        const success = await deleteEntry(entry.id);
        if (success) setEntries(prev => prev.filter(e => e.id !== entry.id));
    };

    const handleArchiveToggle = async (entry: Entry) => {
        // ako si implementirao archive/unArchive funkcionalnost u useEntries
        console.warn("Archive toggle not implemented yet", entry);
    };

    return (
        <div className="max-w-4xl mx-auto p-6">
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-2xl font-semibold">My Entries</h1>

                <RouterLink
                    to="/entries/create"
                    className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                    Create Entry
                </RouterLink>
            </div>

            {isLoading ? (
                <p>Loading...</p>
            ) : error ? (
                <p className="text-red-500">{error}</p>
            ) : entries.length === 0 ? (
                <p>No entries found</p>
            ) : (
                <div className="space-y-4">
                    {entries
                        .slice()
                        .sort((a, b) => {
                            const dateA = a.createdAtUtc ? new Date(a.createdAtUtc).getTime() : 0;
                            const dateB = b.createdAtUtc ? new Date(b.createdAtUtc).getTime() : 0;
                            return dateB - dateA;
                        })
                        .map(entry => (
                            <div key={entry.id} className="flex justify-between p-4 bg-white rounded shadow">
                                <div>
                                    <strong>{entry.value}</strong>
                                    <p>{entry.notes}</p>
                                    <div className="text-sm text-gray-500 mt-1">
                                        <span>Date: {entry.date}</span> | <span>Source: {entry.source}</span>
                                    </div>
                                </div>
                                <div className="flex gap-2">
                                    <button
                                        onClick={() => handleArchiveToggle(entry)}
                                        className={`px-3 py-1 rounded text-white ${entry.isArchived
                                            ? "bg-yellow-500 hover:bg-yellow-600"
                                            : "bg-gray-500 hover:bg-gray-600"
                                            }`}
                                    >
                                        {entry.isArchived ? "Unarchive" : "Archive"}
                                    </button>
                                    <button
                                        onClick={() => handleDelete(entry)}
                                        className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600"
                                    >
                                        Delete
                                    </button>
                                </div>
                            </div>
                        ))}
                </div>
            )}
        </div>
    );
};

export default EntriesPage;
