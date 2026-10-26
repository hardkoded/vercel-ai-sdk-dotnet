// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import { satteri } from "@astrojs/markdown-satteri";
import { baseLinksPlugin } from "./src/base-links.mjs";
import { siteBase, siteOrigin } from "./site.mjs";
import apiSidebar from "./src/generated/api-sidebar.json";

// https://astro.build/config
export default defineConfig({
    site: siteOrigin,
    base: siteBase,
    trailingSlash: "always",
    markdown: {
        processor: satteri({
            mdastPlugins: [baseLinksPlugin()],
        }),
    },
    integrations: [
        starlight({
            title: "Vercel AI SDK for .NET",
            description:
                "Unofficial community port of the Vercel AI SDK for server applications. Not an official Vercel product.",
            customCss: [
                "@fontsource/inter/400.css",
                "@fontsource/inter/500.css",
                "@fontsource/inter/600.css",
                "@fontsource/inter/700.css",
                "@fontsource/dm-mono/400.css",
                "./src/styles/custom.css",
            ],
            expressiveCode: {
                themes: ["github-light", "github-dark"],
                useStarlightDarkModeSwitch: true,
                useStarlightUiThemeColors: true,
            },
            social: [
                { icon: "github", label: "GitHub", href: "https://github.com/hardkoded/vercel-ai-sdk-dotnet" },
            ],
            editLink: {
                baseUrl: "https://github.com/hardkoded/vercel-ai-sdk-dotnet/edit/main/docs-site/src/content/docs/",
            },
            credits: false,
            components: {
                Footer: "./src/components/Footer.astro",
            },
            sidebar: [
                { label: "Home", slug: "index" },
                {
                    label: "Guides",
                    items: [
                        { label: "Quickstart", slug: "quickstart" },
                        { label: "Tools", slug: "tools" },
                        { label: "Speech", slug: "speech" },
                        { label: "Streaming", slug: "streaming" },
                        { label: "Providers", slug: "providers" },
                        { label: "TypeSafe", slug: "typesafe-ai" },
                        { label: "Compatibility", slug: "compatibility" },
                        { label: "Warnings", slug: "warnings" },
                        { label: "Secure URL fetching", slug: "secure-url-fetching" },
                    ],
                },
                {
                    label: "API",
                    collapsed: true,
                    items: apiSidebar,
                },
            ],
        }),
    ],
});
