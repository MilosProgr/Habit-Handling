import { useEffect, useState } from "react";
import { assert } from 'keycloakify/tools/assert';
import { clsx } from "keycloakify/tools/clsx";
import { kcSanitize } from "keycloakify/lib/kcSanitize";
import type { TemplateProps } from "keycloakify/login/TemplateProps";
import { getKcClsx } from "keycloakify/login/lib/kcClsx";
import { useInsertScriptTags } from "keycloakify/tools/UseInsertScriptTags";
import { useSetClassName } from "keycloakify/tools/useSetClassName";
import { useInitialize } from "keycloakify/login/Template.useInitialize";
import type { I18n } from "./i18n";
import type { KcContext } from "./KcContext";
import { useStyles } from "tss-react/mui";
import { Typography } from "@mui/material";

import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';

export default function Template(props: TemplateProps<KcContext, I18n>) {
    const {
        displayInfo = false,
        displayMessage = true,
        displayRequiredFields = false,
        headerNode,
        socialProvidersNode = null,
        infoNode = null,
        documentTitle,
        bodyClassName,
        kcContext,
        i18n,
        doUseDefaultCss,
        classes,
        children
    } = props;

    const { kcClsx } = getKcClsx({ doUseDefaultCss, classes });
    // const { msg, msgStr, currentLanguage, enabledLanguages } = i18n;
    const { msg, msgStr } = i18n;
    const { realm, locale, auth, url, message, isAppInitiatedAction, authenticationSession, scripts } = kcContext;

    useEffect(() => {
        document.title = documentTitle ?? msgStr("loginTitle", realm.displayName);
    }, []);

    useSetClassName({
        qualifiedName: "html",
        className: kcClsx("kcHtmlClass")
    });

    useSetClassName({
        qualifiedName: "body",
        className: bodyClassName ?? kcClsx("kcBodyClass")
    });

    useEffect(() => {
        const { currentLanguageTag } = locale ?? {};
        if (currentLanguageTag === undefined) return;
        const html = document.querySelector("html");
        assert(html !== null);
        html.lang = currentLanguageTag;
    }, []);

    // Browser-safe useInsertLinkTags
    const { areAllStylesLoaded } = useInsertLinkTags({
        scripts: !doUseDefaultCss
            ? []
            : [
                `${url.resourcesCommonPath}/nodeModules/@patternfly/patternfly.min.css`,
                `${url.resourcesCommonPath}/nodeModules/patternfly/dist/css/patternfly.min.css`,
                `${url.resourcesCommonPath}/nodeModules/patternfly/dist/css/patternfly-additions.min.css`,
                `${url.resourcesCommonPath}/css/login.css`
            ],
        position: "head"
    });

    const { insertScriptTags } = useInsertScriptTags({
        scriptTags: [
            { type: "module", src: `${url.resourcesPath}/js/menu-button-links.js` },
            ...(authenticationSession
                ? [{ type: "module", textContent: [].join("\n") } as const]
                : []),
            ...(scripts?.map(script => ({ type: "text/javascript", src: script }) as const) ?? [])
        ],
        componentOrHookName: ""
    });

    useEffect(() => {
        if (areAllStylesLoaded) insertScriptTags();
    }, [areAllStylesLoaded]);

    const { css, theme, cx } = useStyles();


    const { isReadyToRender } = useInitialize({ kcContext, doUseDefaultCss });

    if (!isReadyToRender) return null;

    return (
        <div className={
            cx(
                kcClsx("kcLoginClass"),
                css({
                    height: '100vh',
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                }),
            )
        }>

            {/* <div className={kcClsx("kcFormCardClass")}> */}
            <div className=
                {css({
                    backgroundColor: theme.palette.background.paper,
                    padding: theme.spacing(4),
                    borderRadius: theme.shape.borderRadius,
                    boxShadow: theme.shadows[5],
                    width: '100%',
                    maxWidth: 400,
                })
                }>


                <header className={kcClsx("kcFormHeaderClass")}>
                    {/* {enabledLanguages.length > 1 && (
                        <div className={kcClsx("kcLocaleMainClass")} id="kc-locale">
                            <div id="kc-locale-wrapper" className={kcClsx("kcLocaleWrapperClass")}>
                                <div id="kc-locale-dropdown" className={clsx("menu-button-links", kcClsx("kcLocaleDropDownClass"))}>
                                    <button
                                        tabIndex={1}
                                        id="kc-current-locale-link"
                                        aria-label={msgStr("languages")}
                                        aria-haspopup="true"
                                        aria-expanded="false"
                                        aria-controls="language-switch1"
                                    >
                                        {currentLanguage.label}
                                    </button>
                                    <ul
                                        role="menu"
                                        tabIndex={-1}
                                        aria-labelledby="kc-current-locale-link"
                                        aria-activedescendant=""
                                        id="language-switch1"
                                        className={kcClsx("kcLocaleListClass")}
                                    >
                                        {enabledLanguages.map(({ languageTag, label, href }, i) => (
                                            <li key={languageTag} className={kcClsx("kcLocaleListItemClass")} role="none">
                                                <a role="menuitem" id={`language-${i + 1}`} className={kcClsx("kcLocaleItemClass")} href={href}>
                                                    {label}
                                                </a>
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            </div>
                        </div>
                    )} */}
                    {(() => {
                        const node = !(auth !== undefined && auth.showUsername && !auth.showResetCredentials) ? (
                            <Typography
                                variant="h1"
                                sx={{
                                    pb: 4
                                }}
                            >{headerNode}</Typography>
                        ) : (
                            <div id="kc-username" className={kcClsx("kcFormGroupClass")}>
                                <label id="kc-attempted-username">{auth.attemptedUsername}</label>
                                <a id="reset-login" href={url.loginRestartFlowUrl} aria-label={msgStr("restartLoginTooltip")}>
                                    <div className="kc-login-tooltip">
                                        <i className={kcClsx("kcResetFlowIcon")}></i>
                                        <span className="kc-tooltip-text">{msg("restartLoginTooltip")}</span>
                                    </div>
                                </a>
                            </div>
                        );

                        if (displayRequiredFields) {
                            return (
                                <div className={kcClsx("kcContentWrapperClass")}>
                                    <div className={clsx(kcClsx("kcLabelWrapperClass"), "subtitle")}>
                                        <span className="subtitle">
                                            <span className="required">*</span>
                                            {msg("requiredFields")}
                                        </span>
                                    </div>
                                    <div className="col-md-10">{node}</div>
                                </div>
                            );
                        }

                        return node;
                    })()}
                </header>

                <div id="kc-content">
                    <div id="kc-content-wrapper">
                        {displayMessage && message !== undefined && (message.type !== "warning" || !isAppInitiatedAction) && (
                            // <div
                            //     className={clsx(
                            //         `alert-${message.type}`,
                            //         kcClsx("kcAlertClass"),
                            //         `pf-m-${message?.type === "error" ? "danger" : message.type}`
                            //     )}
                            // >
                            //     <div className="pf-c-alert__icon">
                            //         {message.type === "success" && <span className={kcClsx("kcFeedbackSuccessIcon")}></span>}
                            //         {message.type === "warning" && <span className={kcClsx("kcFeedbackWarningIcon")}></span>}
                            //         {message.type === "error" && <span className={kcClsx("kcFeedbackErrorIcon")}></span>}
                            //         {message.type === "info" && <span className={kcClsx("kcFeedbackInfoIcon")}></span>}
                            //     </div>
                            //     <span
                            //         className={kcClsx("kcAlertTitleClass")}
                            //         dangerouslySetInnerHTML={{ __html: kcSanitize(message.summary) }}
                            //     />
                            // </div>
                            <div>
                                <Stack sx={{ width: '100%' }} spacing={2}>
                                    <Alert severity={message.type === "error" ? "error" : message.type === "warning" ? "warning" : message.type === "info" ? "info" : "success"}>
                                        <span
                                            dangerouslySetInnerHTML={{ __html: kcSanitize(message.summary) }}
                                        />
                                    </Alert>
                                </Stack>
                            </div>
                        )}

                        {children}

                        {auth !== undefined && auth.showTryAnotherWayLink && (
                            <form id="kc-select-try-another-way-form" action={url.loginAction} method="post">
                                <div className={kcClsx("kcFormGroupClass")}>
                                    <input type="hidden" name="tryAnotherWay" value="on" />
                                    <a
                                        href="#"
                                        id="try-another-way"
                                        onClick={() => {
                                            document.forms["kc-select-try-another-way-form" as never].requestSubmit();
                                            return false;
                                        }}
                                    >
                                        {msg("doTryAnotherWay")}
                                    </a>
                                </div>
                            </form>
                        )}

                        {socialProvidersNode}

                        {displayInfo && (
                            // <div id="kc-info" className={kcClsx("kcSignUpClass")}>
                            //     <div id="kc-info-wrapper" className={kcClsx("kcInfoAreaWrapperClass")}>
                            //         {infoNode}
                            //     </div>
                            // </div>
                            <div
                                className={css({
                                    display: 'flex',
                                    justifyContent: 'center',
                                    alignItems: 'center',
                                    marginTop: theme.spacing(3)
                                })}>
                                {infoNode}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}

/**
 * Browser-safe implementation of useInsertLinkTags
 */
function useInsertLinkTags({ scripts, position }: { scripts: string[]; position: "head" | "body" }) {
    const [areAllStylesLoaded, setAreAllStylesLoaded] = useState(false);

    useEffect(() => {
        if (typeof document === "undefined") return; // SSR/Storybook safe
        if (!scripts || scripts.length === 0) {
            setAreAllStylesLoaded(true);
            return;
        }

        let loadedCount = 0;

        scripts.forEach((href) => {
            const link = document.createElement("link");
            link.rel = "stylesheet";
            link.href = href;

            link.onload = link.onerror = () => {
                loadedCount += 1;
                if (loadedCount === scripts.length) setAreAllStylesLoaded(true);
            };

            // Append to proper position
            if (position === "head") {
                document.head.appendChild(link);
            } else if (position === "body") {
                document.body.appendChild(link);
            }
        });
    }, [scripts, position]);

    return { areAllStylesLoaded };
}
