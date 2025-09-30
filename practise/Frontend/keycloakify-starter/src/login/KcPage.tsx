import { Suspense, lazy } from "react";
import type { ClassKey } from "keycloakify/login";
import type { KcContext } from "./KcContext";
import { useI18n } from "./i18n";
import DefaultPage from "keycloakify/login/DefaultPage";
import Template from "./Template";
import { tss } from "tss-react/mui";
import { createTheme, ThemeProvider } from '@mui/material/styles';
// import { colors } from "@mui/material";
import "../assets/font/main.css"
import backgroundUrl from "../assets/img/background.avif"
const UserProfileFormFields = lazy(
    () => import("./UserProfileFormFields")

);
const Login = lazy(() => import("./pages/Login"));
const doMakeUserConfirmPassword = true;

const theme = createTheme({
    palette: {
        mode: 'dark',
        background: {
            default: "000000",
            paper: "#111111",

        },
        text: {
            primary: "#EDEDED",
            secondary: "#A1A1A1",
        },
    },
    typography: {
        fontFamily: 'Geist',
    },
});

export default function KcPage(props: { kcContext: KcContext }) {
    return (
        <ThemeProvider theme={theme}>
            <KcPageContexualised {...props} />
        </ThemeProvider>
    );
}

function KcPageContexualised(props: { kcContext: KcContext }) {
    const { kcContext } = props;

    const { i18n } = useI18n({ kcContext });

    const { classes } = useStyles();

    return (
        <Suspense>
            {(() => {
                switch (kcContext.pageId) {
                    case "login.ftl": return (
                        <Login
                            // {...{ kcContext, i18n }}

                            {...{ kcContext, i18n, classes }}
                            Template={Template}
                            doUseDefaultCss={true}
                        />
                    );
                    default:
                        return (

                            <DefaultPage
                                kcContext={kcContext}
                                i18n={i18n}
                                classes={classes}
                                Template={Template}
                                doUseDefaultCss={true}
                                UserProfileFormFields={UserProfileFormFields}
                                doMakeUserConfirmPassword={doMakeUserConfirmPassword}
                            />
                        );
                }
            })()}
        </Suspense>
    );
}

const useStyles = tss.create(({ theme }) => ({
    kcHtmlClass: {
        ":root  ": {
            colorScheme: "dark",
        },
    },
    kcBodyClass: {
        // backgroundColor: theme.palette.background.default,
        color: theme.palette.text.primary,
        background: `url(${backgroundUrl}) no-repeat center center fixed`,
        backgroundSize: "cover",
    },
} satisfies { [key in ClassKey]?: unknown }));
// const classes = {

// } satisfies { [key in ClassKey]?: unknown };
