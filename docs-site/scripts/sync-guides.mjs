import { mkdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { redirectHtml } from "./redirect.mjs";

const docsSite = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(docsSite, "..");
const contentDir = path.join(docsSite, "src/content/docs");
const repo = "https://github.com/hardkoded/vercel-ai-sdk-dotnet";

const pages = [
    {
        source: "docs/articles/quickstart.md",
        output: "quickstart.md",
        title: "Quickstart",
        label: "Quickstart",
        legacy: "articles/quickstart.html",
        target: "/quickstart/",
    },
    {
        source: "docs/articles/tools.md",
        output: "tools.md",
        title: "Tools",
        label: "Tools",
        legacy: "articles/tools.html",
        target: "/tools/",
    },
    {
        source: "docs/articles/speech.md",
        output: "speech.md",
        title: "Speech",
        label: "Speech",
        legacy: "articles/speech.html",
        target: "/speech/",
    },
    {
        source: "docs/articles/streaming.md",
        output: "streaming.md",
        title: "Streaming",
        label: "Streaming",
        legacy: "articles/streaming.html",
        target: "/streaming/",
    },
    {
        source: "docs/articles/providers.md",
        output: "providers.md",
        title: "Providers",
        label: "Providers",
        legacy: "articles/providers.html",
        target: "/providers/",
    },
    {
        source: "docs/articles/typesafe-ai.md",
        output: "typesafe-ai.md",
        title: "TypeSafe",
        label: "TypeSafe",
        legacy: "articles/typesafe-ai.html",
        target: "/typesafe-ai/",
    },
    {
        source: "docs/articles/compatibility.md",
        output: "compatibility.md",
        title: "Compatibility",
        label: "Compatibility",
        legacy: "articles/compatibility.html",
        target: "/compatibility/",
    },
    {
        source: "docs/articles/warnings.md",
        output: "warnings.md",
        title: "Warnings",
        label: "Warnings",
        legacy: "articles/warnings.html",
        target: "/warnings/",
    },
    {
        source: "docs/articles/secure-url-fetching.md",
        output: "secure-url-fetching.md",
        title: "Secure URL fetching",
        label: "Secure URL fetching",
        legacy: "articles/secure-url-fetching.html",
        target: "/secure-url-fetching/",
    },
];

const pageTargets = new Map(pages.map((page) => [path.posix.basename(page.source), page.target]));

const sectionRedirects = [
    // The old DocFX table of contents. The site root is now the marketing page.
    ["toc.html", "/docs/"],
    ["articles/toc.html", "/docs/"],
];

await mkdir(contentDir, { recursive: true });

for (const page of pages) {
    const raw = await readFile(path.join(repoRoot, page.source), "utf8");
    const body = rewriteLinks(stripTitle(raw.replace(/^\uFEFF/, "")));
    const description = describe(body);
    const frontmatter = [
        "---",
        `title: ${JSON.stringify(page.title)}`,
        `description: ${JSON.stringify(description)}`,
        `editUrl: ${JSON.stringify(`${repo}/edit/main/${page.source}`)}`,
        "sidebar:",
        `  label: ${JSON.stringify(page.label)}`,
        "---",
        "",
    ].join("\n");
    await writeFile(path.join(contentDir, page.output), `${frontmatter}${body.trim()}\n`);
    await writeLegacy(page.legacy, page.target);
}

for (const [legacy, target] of sectionRedirects) {
    await writeLegacy(legacy, target);
}

console.log(`Synced ${pages.length} guide pages into ${path.relative(repoRoot, contentDir)}`);

function stripTitle(markdown) {
    return markdown.replace(/^#\s+[^\n]*\n+/, "");
}

function describe(markdown) {
    const paragraph = markdown
        .split(/\n\s*\n/)
        .map((block) => block.trim())
        .find((block) => block && !block.startsWith("#") && !block.startsWith("```") && !block.startsWith("|") && !block.startsWith("<") && !block.startsWith("<!--"));
    const text = (paragraph ?? "Vercel AI SDK for .NET")
        .replace(/\[([^\]]+)\]\([^)]+\)/g, "$1")
        .replace(/[`*_]/g, "")
        .replace(/\s+/g, " ")
        .trim();
    return text.length > 160 ? `${text.slice(0, 157).trimEnd()}...` : text;
}

function rewriteLinks(markdown) {
    const withGuides = markdown.replace(/\[([^\]]+)\]\(([^)\s]+)\)/g, (full, text, href) => {
        if (/^[a-z][a-z0-9+.-]*:/i.test(href) || href.startsWith("#") || href.startsWith("/")) return full;
        const hashAt = href.indexOf("#");
        const file = hashAt >= 0 ? href.slice(0, hashAt) : href;
        const hash = hashAt >= 0 ? href.slice(hashAt) : "";
        const target = pageTargets.get(path.posix.basename(file));
        if (!target) return full;
        return `[${text}](${target}${hash})`;
    });
    return withGuides.replace(
        /href=(['"])\/examples\//g,
        `href=$1${repo}/blob/main/examples/`,
    );
}

async function writeLegacy(relativePath, target) {
    const destination = path.join(docsSite, "public", relativePath);
    await mkdir(path.dirname(destination), { recursive: true });
    await writeFile(destination, redirectHtml(target));
}
