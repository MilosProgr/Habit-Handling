export interface EntryDailyStatDto {
    date: string; // možeš i Date, ali string je lakše parsirati iz JSON-a
    count: number;
}

export interface EntryStatsDto {
    dailyStats: EntryDailyStatDto[];
    totalEntries: number;
    currentStreak: number;
    longestStreak: number;
}
