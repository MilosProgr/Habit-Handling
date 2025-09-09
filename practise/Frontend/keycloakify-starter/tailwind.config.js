/** @type {import('tailwindcss').Config} */
module.exports = {
    mode: ['jit'],
    content: [
        "./src/**/*.{js,jsx,ts,tsx}",
        "./.storybook/**/*.{js,ts,jsx,tsx}"
    ],
    theme: {
        extend: {
            backgroundImage: {
                'back-img': "url('/assets/background.avif')",
            },
        },
        plugins: [],
    }
}