export const supportedLanguages = {
    en: "English",
    sr: "Srpski",
    de: "Deutsch",
    fr: "Français",
    es: "Español",
    ru: "Русский",
} as const;

export type SupportedLanguageTag = keyof typeof supportedLanguages;