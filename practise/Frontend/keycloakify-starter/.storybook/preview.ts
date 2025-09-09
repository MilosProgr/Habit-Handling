import type { Preview } from "@storybook/react";
import "../src/index.css" // Import your global styles including Tailwind CSS

const preview: Preview = {
    parameters: {
        controls: {
            matchers: {
                color: /(background|color)$/i,
                date: /Date$/i
            }
        }
    }
};

export default preview;
