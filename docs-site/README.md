# Vercel AI SDK for .NET docs

The documentation site is [Astro Starlight](https://starlight.astro.build/). The published site is [https://www.hardkoded.com/vercel-ai-sdk-dotnet/](https://www.hardkoded.com/vercel-ai-sdk-dotnet/).

## Preview locally

From this directory:

```bash
npm ci
npm run api
npm run dev
```

Open http://localhost:4321/vercel-ai-sdk-dotnet/ . The path includes `/vercel-ai-sdk-dotnet/` because that is the base path on hardkoded.com (`https://www.hardkoded.com/vercel-ai-sdk-dotnet/`).

The site root is a marketing page (`src/pages/index.astro`), not the Starlight home. Documentation starts at `/docs/`. Quickstart, the other guides, and the API stay on their existing paths.

`npm run dev` and `npm run build` copy `docs/articles/` into the site before Astro starts. Edit the articles in `docs/articles/`. The copies under `src/content/docs/` are generated. The documentation index at `src/content/docs/docs/index.mdx` is edited by hand.

Search is built with the production output:

```bash
npm run build
npm run preview
```

## Regenerate the API reference

The API pages are generated. They are not edited by hand.

```bash
npm run api
```

That needs the .NET 10 SDK. It restores the local DocFX tool (`dotnet-tools.json`, DocFX 2.80.1), runs `docfx metadata` on `src/Vercel.AI` (`net10.0`), and writes Markdown into `src/content/docs/api/`. Commit the generated pages and `src/generated/api-sidebar.json`.

`npm run api -- --skip-metadata` rebuilds the Markdown from `docs-site/.api-yaml` without running DocFX again.

## GitHub Pages

GitHub Pages is retired. `.github/workflows/docs.yml` only builds and checks this site. It runs on every pull request and on every push to `main`, and uploads the build as a workflow artifact. It does not deploy. Vercel is the host. The Vercel Git integration publishes the site on push to `main`.

Old DocFX `.html` URLs redirect to the new paths. Member anchors from DocFX (`#Vercel_AI_AiClient_...`) are kept on the generated pages. A redirect preserves the hash when JavaScript runs.
