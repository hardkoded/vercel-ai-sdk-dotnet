#!/usr/bin/env python3
"""Generates docs/articles/models/*.md, one page per provider.

Each page lists the model kinds the provider implements, the model ids the
repo's tests use, a link to the upstream Vercel AI Gateway model catalog, and a
C# example. The kinds and ids come from tests/ (a call such as
`provider.ImageModel("fal-ai/flux/dev")`), so a page only names models that the
port exercises.

Every example is compiled against src/Vercel.AI before it is written. An
example that does not compile falls back to the next candidate factory method
and finally to a snippet that only creates the model.

Usage: python3 build/generate-model-pages.py [--no-compile]
"""
import collections
import glob
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
SRC = os.path.join(ROOT, "src", "Vercel.AI")
OUT = os.path.join(ROOT, "docs", "articles", "models")
CATALOG = "https://vercel.com/ai-gateway/models"

DISPLAY = {
    "AmazonBedrock": "Amazon Bedrock",
    "AssemblyAI": "AssemblyAI",
    "Azure": "Azure OpenAI",
    "BlackForestLabs": "Black Forest Labs",
    "ByteDance": "ByteDance",
    "Deepgram": "Deepgram",
    "DeepInfra": "DeepInfra",
    "DeepSeek": "DeepSeek",
    "ElevenLabs": "ElevenLabs",
    "Fal": "fal",
    "FishAudio": "Fish Audio",
    "Gateway": "Vercel AI Gateway",
    "GmiCloud": "GMI Cloud",
    "Google": "Google Generative AI",
    "HeyGen": "HeyGen",
    "HuggingFace": "Hugging Face",
    "KlingAI": "Kling AI",
    "MiniMax": "MiniMax",
    "OpenAI": "OpenAI",
    "OpenAICompatible": "OpenAI-compatible",
    "OpenResponses": "Open Responses",
    "QuiverAI": "QuiverAI",
    "RevAI": "Rev.ai",
    "ScaleDown": "ScaleDown",
    "Tencent": "Tencent Cloud",
    "TogetherAI": "Together AI",
    "TypeSafeAi": "TypeSafe",
    "Xai": "xAI",
    "Zai": "Z.ai",
}

# Factory method names seen in tests, per model kind, in the order a page prefers them.
KINDS = collections.OrderedDict(
    [
        ("language", ["LanguageModel", "ChatModel", "Chat", "ResponsesModel", "CreateChatModel"]),
        ("embedding", ["EmbeddingModel"]),
        ("image", ["ImageModel", "Image"]),
        ("speech", ["SpeechModel", "Speech"]),
        ("transcription", ["TranscriptionModel", "Transcription"]),
        ("video", ["VideoModel", "Video"]),
        ("reranking", ["RerankingModel", "Reranking"]),
        ("decision", ["DecisionModel"]),
    ]
)
KIND_TITLE = {
    "language": "Language",
    "embedding": "Embedding",
    "image": "Image",
    "speech": "Speech",
    "transcription": "Transcription",
    "video": "Video",
    "reranking": "Reranking",
    "decision": "Decision",
}
PLACEHOLDER = re.compile(
    r"no-such|some-model|unknown|nonexistent|invalid|bogus|fake|dummy|mock|^test|^my-|custom|placeholder|^model$|^id$|^x$|^m$|^embed$",
    re.I,
)

METHOD_TO_KIND = {m: k for k, ms in KINDS.items() for m in ms}
CALL = re.compile(r"\.(" + "|".join(sorted(METHOD_TO_KIND, key=len, reverse=True)) + r')\("([^"{}$\\]+)"\)')


def display(dir_name):
    if dir_name in DISPLAY:
        return DISPLAY[dir_name]
    return re.sub(r"(?<=[a-z])(?=[A-Z])", " ", dir_name)


def slug(dir_name):
    return re.sub(r"[^a-z0-9]+", "-", display(dir_name).lower()).strip("-")


def find_providers():
    providers = {}
    for directory in sorted(glob.glob(os.path.join(SRC, "*/"))):
        name = os.path.basename(directory.rstrip("/"))
        candidates = []
        for path in sorted(glob.glob(os.path.join(directory, "*Provider.cs"))):
            text = open(path, encoding="utf-8-sig").read()
            for match in re.finditer(r"public (?:sealed |partial )*class (\w+Provider)\b", text):
                cls = match.group(1)
                if re.search(r"public static \w+ Create\(", text) and f"class {cls}" in text:
                    candidates.append(cls)
        if not candidates:
            continue
        preferred = [c for c in candidates if c == f"{name}Provider"] or candidates
        providers[name] = {"dir": name, "cls": preferred[0], "ns": f"Vercel.AI.{name}"}
    return providers


def provider_for_test(path, names):
    parts = re.split(r"[\\/]", os.path.relpath(path, os.path.join(ROOT, "tests")))
    best = None
    for part in parts:
        stem = re.sub(r"\.cs$", "", part)
        for name in names:
            if re.match(re.escape(name) + r"(?=[A-Z]|$)", stem, re.I) or stem.lower() == name.lower():
                if best is None or len(name) > len(best):
                    best = name
    return best


def collect_models(providers):
    found = {name: collections.defaultdict(collections.Counter) for name in providers}
    methods = {name: collections.defaultdict(collections.Counter) for name in providers}
    for path in glob.glob(os.path.join(ROOT, "tests", "**", "*.cs"), recursive=True):
        name = provider_for_test(path, providers)
        if name is None:
            continue
        for method, model_id in CALL.findall(open(path, encoding="utf-8-sig").read()):
            if PLACEHOLDER.search(model_id):
                continue
            kind = METHOD_TO_KIND[method]
            found[name][kind][model_id] += 1
            methods[name][kind][method] += 1
    return found, methods


PROMPTS = {
    "language": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await Ai.GenerateTextAsync(",
        "    new GenerateTextOptions",
        "    {",
        "        Model = model,",
        "        Prompt = \"Invent a new holiday and describe its traditions.\",",
        "    }",
        ");",
        "",
        "Console.WriteLine(result.Text);",
    ],
    "embedding": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await Embed.EmbedAsync(",
        "    new EmbedRequest { Model = model, Value = \"sunny day at the beach\" }",
        ");",
        "",
        "Console.WriteLine(result.Embedding.Length);",
    ],
    "image": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await GenerateImage.GenerateImageAsync(",
        "    new GenerateImageRequest { Model = model, Text = \"A red apple sticker\" }",
        ");",
        "",
        "await File.WriteAllBytesAsync(\"image.png\", result.Image.Data!);",
    ],
    "speech": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await GenerateSpeech.GenerateSpeechAsync(",
        "    new GenerateSpeechRequest { Model = model, Text = \"Hello from the Vercel AI SDK for .NET.\" }",
        ");",
        "",
        "await File.WriteAllBytesAsync(\"speech.mp3\", result.Audio.Data);",
    ],
    "transcription": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await Transcribe.TranscribeAsync(",
        "    new TranscribeRequest",
        "    {",
        "        Model = model,",
        "        Audio = await File.ReadAllBytesAsync(\"audio.mp3\"),",
        "    }",
        ");",
        "",
        "Console.WriteLine(result.Text);",
    ],
    "video": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await GenerateVideo.GenerateVideoAsync(",
        "    new GenerateVideoRequest",
        "    {",
        "        Model = model,",
        "        Prompt = new VideoPrompt(\"A yorkie among dandelions at Crissy Field.\"),",
        "    }",
        ");",
        "",
        "await File.WriteAllBytesAsync(\"video.mp4\", result.Video.Data);",
    ],
    "reranking": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await Rerank.RerankAsync(",
        "    new RerankRequest",
        "    {",
        "        Model = model,",
        "        Query = \"What is an agent?\",",
        "        Documents = new object?[] { \"An agent calls tools in a loop.\", \"Bananas are yellow.\" },",
        "        TopN = 1,",
        "    }",
        ");",
        "",
        "foreach (var ranked in result.Ranking)",
        "{",
        "    Console.WriteLine($\"{ranked.OriginalIndex}: {ranked.Score}\");",
        "}",
    ],
}
CREATE_ONLY = [
    "var model = {P}.{M}(\"{ID}\");",
    "",
    "Console.WriteLine(model.ModelId);",
]
USINGS = [
    "using Vercel.AI;",
    "using Vercel.AI.GenerateText;",
    "using Vercel.AI.Operations;",
    "using Vercel.AI.Provider;",
]


def render(lines, provider, method, model_id):
    return "\n".join(lines).replace("{P}", provider).replace("{M}", method).replace("{ID}", model_id)


def example_variants(info, kind, method_order, model_id):
    """Yields (snippet, uses_operation) candidates for one kind, most useful first."""
    provider = f"{info['cls']}.Create()"
    for method in method_order:
        if kind in PROMPTS:
            yield render(PROMPTS[kind], provider, method, model_id), True
    for method in method_order:
        yield render(CREATE_ONLY, provider, method, model_id), False


def compile_check(candidates):
    """candidates: {key: snippet}. Returns the set of keys that compile."""
    work = tempfile.mkdtemp(prefix="model-pages-")
    try:
        namespaces = sorted({f"Vercel.AI.{k.split('|')[0]}" for k in candidates})
        proj = os.path.join(work, "Check.csproj")
        open(proj, "w").write(
            f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="{os.path.join(SRC, 'Vercel.AI.csproj')}" />
  </ItemGroup>
</Project>
"""
        )
        keys = sorted(candidates)
        body = []
        for i, key in enumerate(keys):
            snippet = candidates[key]
            ns = f"Vercel.AI.{key.split('|')[0]}"
            indented = "\n".join("        " + line if line else "" for line in snippet.splitlines())
            body.append(f"    // {key}\n    public static async Task M{i}()\n    {{\n{indented}\n    }}\n")
        usings = "\n".join(USINGS + [f"using {n};" for n in namespaces])
        cs = f"{usings}\n\nnamespace Check;\n\npublic static class Snippets\n{{\n" + "\n".join(body) + "}\n"
        open(os.path.join(work, "Snippets.cs"), "w").write(cs)
        result = subprocess.run(
            ["dotnet", "build", proj, "-nologo", "-v", "q", "-clp:NoSummary"],
            capture_output=True,
            text=True,
        )
        lines = cs.splitlines()
        failed = set()
        for match in re.finditer(r"Snippets\.cs\((\d+),\d+\): error (CS\d+)", result.stdout):
            line_no = int(match.group(1))
            for j in range(line_no - 1, -1, -1):
                if lines[j].startswith("    // "):
                    failed.add(lines[j][7:].strip())
                    break
        if result.returncode != 0 and not failed:
            sys.stderr.write(result.stdout[-4000:])
            raise SystemExit("compile check failed without a snippet error")
        return set(keys) - failed
    finally:
        shutil.rmtree(work, ignore_errors=True)


def main():
    compile_enabled = "--no-compile" not in sys.argv
    providers = find_providers()
    found, methods = collect_models(providers)

    plan = {}
    candidates = {}
    for name, info in providers.items():
        kinds = {k: v for k, v in found[name].items() if v}
        if not kinds:
            continue
        plan[name] = {}
        for kind, counter in kinds.items():
            ids = [i for i, _ in counter.most_common(6)]
            method_order = [m for m, _ in methods[name][kind].most_common()]
            plan[name][kind] = {"ids": ids, "methods": method_order}
            for index, (snippet, op) in enumerate(example_variants(info, kind, method_order, ids[0])):
                candidates[f"{name}|{kind}|{index}"] = snippet

    ok = compile_check(candidates) if compile_enabled else set(candidates)

    shutil.rmtree(OUT, ignore_errors=True)
    os.makedirs(OUT)
    index_rows = []
    for name, kinds in sorted(plan.items(), key=lambda kv: display(kv[0]).lower()):
        info = providers[name]
        chosen = None
        fallback = None
        for kind in KINDS:
            if kind not in kinds:
                continue
            rich = len(kinds[kind]["methods"]) if kind in PROMPTS else 0
            for index in range(len(kinds[kind]["methods"]) * 2):
                key = f"{name}|{kind}|{index}"
                if key not in ok:
                    continue
                if index < rich:
                    chosen = (kind, candidates[key])
                elif fallback is None:
                    fallback = (kind, candidates[key])
                break
            if chosen:
                break
        chosen = chosen or fallback
        write_page(name, info, kinds, chosen)
        index_rows.append((name, kinds))
    write_index(index_rows)
    print(f"Wrote {len(index_rows)} provider pages to {os.path.relpath(OUT, ROOT)}")


def write_page(name, info, kinds, chosen):
    title = display(name)
    cls = info["cls"]
    lines = [f"# {title}", ""]
    lines.append(
        f"`{cls}` in the `{info['ns']}` namespace creates the {title} models that this port implements. "
        f"The API reference lists every option: [{cls}](/api/{info['ns']}/{cls}/)."
    )
    lines += [
        "",
        f"Browse the upstream catalog of {title} models and their pricing on the [Vercel AI Gateway models page]({CATALOG}).",
        "",
        "## Models",
        "",
        "| Kind | Factory method | Model ids used in this port's tests |",
        "| --- | --- | --- |",
    ]
    for kind in KINDS:
        if kind not in kinds:
            continue
        entry = kinds[kind]
        methods = ", ".join(f"`{m}`" for m in sorted(entry["methods"]))
        ids = ", ".join(f"`{i}`" for i in entry["ids"])
        lines.append(f"| {KIND_TITLE[kind]} | {methods} | {ids} |")
    lines += [
        "",
        "A model id in the table is one that the tests exercise. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.",
        "",
        "## Example",
        "",
    ]
    if chosen:
        kind, snippet = chosen
        lines += [f"{KIND_TITLE[kind]} model:", "", "```csharp", f"using {info['ns']};", ""]
        lines += [snippet, "```", ""]
    lines += ["See the [Providers](providers.md) guide for authentication and provider options.", ""]
    with open(os.path.join(OUT, slug(name) + ".md"), "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines))


def write_index(rows):
    lines = [
        "# Models",
        "",
        "One page per provider, with the model kinds this port implements, the model ids its tests use, a link to the upstream model catalog, and a C# example.",
        "",
        f"The upstream catalog is the [Vercel AI Gateway models page]({CATALOG}).",
        "",
        "| Provider | Kinds |",
        "| --- | --- |",
    ]
    for name, kinds in sorted(rows, key=lambda r: display(r[0]).lower()):
        kind_list = ", ".join(KIND_TITLE[k] for k in KINDS if k in kinds)
        lines.append(f"| [{display(name)}]({slug(name)}.md) | {kind_list} |")
    lines.append("")
    with open(os.path.join(OUT, "index.md"), "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines))


if __name__ == "__main__":
    main()
