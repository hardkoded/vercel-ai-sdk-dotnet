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
    r"any-model|model-id|no-such|some-model|unknown|nonexistent|invalid|bogus|fake|dummy|mock|^test|^my-|custom|placeholder|^model$|^id$|^x$|^m$|^embed$",
    re.I,
)

# Providers whose Create() needs arguments.
PROVIDER_EXPR = {
    "OpenAICompatible": 'OpenAICompatibleProvider.Create(new OpenAICompatibleOptions { ProviderName = "example", BaseUrl = "https://api.example.com/v1" })',
}
CALLERS = {
    "embedding": "IEmbeddingCaller",
    "image": "IImageCaller",
    "speech": "ISpeechCaller",
    "transcription": "ITranscriptionCaller",
    "video": "IVideoCaller",
    "reranking": "IRerankCaller",
}

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
            for match in re.finditer(r"public (?:sealed |abstract |partial )*class (\w+Provider)\b", text):
                candidates.append(match.group(1))
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


# Evidence the tests do not carry: the quickstart (docs/articles/quickstart.md) calls the Gateway with this id.
# The same goes for providers whose tests never call a factory method: the ids come from upstream's provider docs
# (content/providers/01-ai-sdk-providers/*.mdx).
SEED = {
    ("Gateway", "language", "LanguageModel"): ["openai/gpt-6-astra"],
    ("ByteDance", "image", "ImageModel"): ["seedream-5-0-260128", "seedream-4-5-251128"],
    ("ByteDance", "video", "VideoModel"): ["seedance-1-0-pro-250528", "seedance-1-5-pro-251215"],
    ("Hume", "speech", "SpeechModel"): ["default"],
    ("KlingAI", "video", "VideoModel"): ["kling-v3.0-t2v", "kling-v3.0-i2v", "kling-v3.0-motion-control"],
    ("OpenResponses", "language", "LanguageModel"): ["openai/gpt-6-astra"],
}


def collect_models(providers):
    found = {name: collections.defaultdict(collections.Counter) for name in providers}
    methods = {name: collections.defaultdict(collections.Counter) for name in providers}
    for (name, kind, method), ids in SEED.items():
        if name in providers:
            for model_id in ids:
                found[name][kind][model_id] += 1
                methods[name][kind][method] += 1
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
    "decision": [
        "var model = {P}.{M}(\"{ID}\");",
        "var result = await Decide.DecideAsync(",
        "    new DecideRequest",
        "    {",
        "        Model = model,",
        "        State = \"I was charged twice for my subscription. Please refund the duplicate.\",",
        "        Questions = new Dictionary<string, DecisionQuestion>",
        "        {",
        "            [\"category\"] = new(",
        "                \"choice\",",
        "                \"Which team should handle this request?\",",
        "                new DecisionChoiceCriteria(",
        "                    new Dictionary<string, DecisionInput?>",
        "                    {",
        "                        [\"billing\"] = \"Charges, invoices, and refunds\",",
        "                        [\"technical\"] = \"Bugs and outages\",",
        "                    }",
        "                )",
        "            ),",
        "        },",
        "    }",
        ");",
        "",
        "foreach (var answer in result.Answers)",
        "{",
        "    Console.WriteLine($\"{answer.Key}: {answer.Value.Choice}\");",
        "}",
    ],
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

# Providers whose model does not implement the I*Caller interface that the Generate* helpers need. The page shows a
# hand-written example that calls the model's own method. build/generate-model-pages.py compiles each one.
EXAMPLES = {
    ("AssemblyAI", "speech"): [
        'var model = AssemblyAIProvider.Create().Speech("{ID}");',
        "var result = await model.GenerateAsync(",
        '    new AssemblyAISpeechRequest("Hello from the Vercel AI SDK for .NET.") { Voice = "aai-kyla" },',
        "    CancellationToken.None",
        ");",
        "",
        'await File.WriteAllBytesAsync("speech.mp3", result.Audio);',
    ],
    ("AssemblyAI", "transcription"): [
        'var model = AssemblyAIProvider.Create().Transcription("{ID}");',
        "var result = await model.TranscribeAsync(",
        '    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),',
        "    new AssemblyAITranscriptionRequest(),",
        "    CancellationToken.None",
        ");",
        "",
        "Console.WriteLine(result.Text);",
    ],
    ("ElevenLabs", "speech"): [
        'var model = (ISpeechCaller)ElevenLabsProvider.Create().SpeechModel("{ID}");',
        "var result = await GenerateSpeech.GenerateSpeechAsync(",
        '    new GenerateSpeechRequest { Model = model, Text = "Hello from the Vercel AI SDK for .NET." }',
        ");",
        "",
        'await File.WriteAllBytesAsync("speech.mp3", result.Audio.Data);',
    ],
    ("ElevenLabs", "transcription"): [
        'var model = (ElevenLabsProvider.ElevenLabsTranscriptionModel)ElevenLabsProvider.Create().TranscriptionModel("{ID}");',
        "var result = await model.TranscribeAsync(",
        '    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),',
        "    new ElevenLabsTranscriptionRequest(),",
        "    CancellationToken.None",
        ");",
        "",
        "Console.WriteLine(result.Text);",
    ],
    ("Gladia", "transcription"): [
        'var model = (GladiaProvider.GladiaTranscriptionModel)GladiaProvider.Create().TranscriptionModel("{ID}");',
        "var result = await model.TranscribeAsync(",
        '    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),',
        "    new GladiaTranscriptionRequest(),",
        "    CancellationToken.None",
        ");",
        "",
        "Console.WriteLine(result.Text);",
    ],
    ("Hume", "speech"): [
        "var model = HumeProvider.Create().Speech();",
        "var result = await model.GenerateAsync(",
        '    new HumeSpeechRequest("Hello from the Vercel AI SDK for .NET."),',
        "    CancellationToken.None",
        ");",
        "",
        'await File.WriteAllBytesAsync("speech.mp3", result.Audio);',
    ],
    ("Luma", "image"): [
        'var model = (LumaImageModel)LumaProvider.Create().ImageModel("{ID}");',
        "var result = await model.GenerateAsync(",
        '    new LumaImageRequest("A red apple sticker") { AspectRatio = "16:9" },',
        "    CancellationToken.None",
        ");",
        "",
        'await File.WriteAllBytesAsync("image.png", result.Images[0].Data!);',
    ],
    ("RevAI", "transcription"): [
        'var model = (RevAIProvider.RevAITranscriptionModel)RevAIProvider.Create().TranscriptionModel("{ID}");',
        "var result = await model.TranscribeAsync(",
        '    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),',
        "    new RevAITranscriptionRequest(),",
        "    CancellationToken.None",
        ");",
        "",
        "Console.WriteLine(result.Text);",
    ],
    ("Voyage", "embedding"): [
        'var model = (VoyageProvider.VoyageEmbeddingModel)VoyageProvider.Create().EmbeddingModel("{ID}");',
        "var result = await model.EmbedAsync(",
        '    new VoyageEmbeddingRequest(new[] { "sunny day at the beach" }),',
        "    CancellationToken.None",
        ");",
        "",
        "Console.WriteLine(result.Embeddings[0].Length);",
    ],
    ("Voyage", "reranking"): [
        'var model = (VoyageProvider.VoyageRerankingModel)VoyageProvider.Create().RerankingModel("{ID}");',
        "var result = await model.RerankAsync(",
        "    new VoyageRerankRequest",
        "    {",
        '        Query = "What is an agent?",',
        '        TextDocuments = new[] { "An agent calls tools in a loop.", "Bananas are yellow." },',
        "        TopK = 1,",
        "    },",
        "    CancellationToken.None",
        ");",
        "",
        "foreach (var ranked in result.Ranking)",
        "{",
        '    Console.WriteLine($"{ranked.Index}: {ranked.Score}");',
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


def provider_expr(info):
    return PROVIDER_EXPR.get(info["dir"], f"{info['cls']}.Create()")


def implemented_methods(info, kind, methods):
    """Keeps the factory methods that the provider source implements (does not just throw NoSuchModel)."""
    declared = collections.defaultdict(list)
    inherits_compatible = False
    for path in glob.glob(os.path.join(SRC, info["dir"], "*.cs")):
        text = open(path, encoding="utf-8-sig").read()
        if re.search(r"class \w+ : OpenAICompatibleProvider", text):
            inherits_compatible = True
        for match in re.finditer(r"public (?:override |virtual |new |static )*[\w.<>?]+ (\w+)\(\s*string modelId", text):
            tail = text[match.end() : match.end() + 1500]
            end = re.search(r"\n    (?:public|internal|private|protected) ", tail)
            body = tail[: end.start()] if end else tail
            # Not implemented only when the first statement throws, as in `{ throw new NoSuchModelError(...)`.
            throw_only = re.match(r"[^{=;]*(?:\{|=>)\s*throw\b", body) is not None
            declared[match.group(1)].append(not throw_only)
    kept = []
    for method in methods:
        if method in declared:
            if any(declared[method]):
                kept.append(method)
        elif kind == "language" and method == "LanguageModel" and inherits_compatible:
            kept.append(method)
    return kept


def variants(info, kind, methods, model_id):
    """Candidate examples for one kind, as {variant_key: (snippet, probe_expression or None)}."""
    out = collections.OrderedDict()
    provider = provider_expr(info)
    for method in methods:
        if kind in PROMPTS:
            out[f"{method}|direct"] = (render(PROMPTS[kind], provider, method, model_id), None)
            if kind in CALLERS:
                caller = CALLERS[kind]
                cast = render(PROMPTS[kind], provider, method, model_id).replace(
                    f"= {provider}.{method}(", f"= ({caller}){provider}.{method}(", 1
                )
                out[f"{method}|cast"] = (cast, f"{provider}.{method}(\"{model_id}\") is {caller}")
    if (info["dir"], kind) in EXAMPLES:
        out["example"] = (
            "\n".join(EXAMPLES[(info["dir"], kind)]).replace("{ID}", model_id),
            None,
        )
    for method in methods:
        out[f"{method}|create"] = (render(CREATE_ONLY, provider, method, model_id), None)
    return out


def build_and_probe(snippets, probes):
    """Compiles every snippet and probe. Returns (compiling snippet keys, probe keys that are true)."""
    work = tempfile.mkdtemp(prefix="model-pages-")
    try:
        namespaces = sorted({f"Vercel.AI.{k.split('|')[0]}" for k in list(snippets) + list(probes)})
        with open(os.path.join(work, "Check.csproj"), "w") as handle:
            handle.write(
                f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <NoWarn>$(NoWarn);CS0618;CS8321</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="{os.path.join(SRC, 'Vercel.AI.csproj')}" />
  </ItemGroup>
</Project>
"""
            )
        usings = "\n".join(USINGS + [f"using {n};" for n in namespaces])
        body = []
        for i, key in enumerate(sorted(snippets)):
            indented = "\n".join("        " + line if line else "" for line in snippets[key].splitlines())
            body.append(f"    // @@{key}\n    public static async Task S{i}()\n    {{\n{indented}\n    }}\n")
        probe_lines = [
            f"        Probe(\"{key}\", () => {probes[key]}); // @@{key}" for key in sorted(probes)
        ]
        cs = (
            f"{usings}\n\nnamespace Check;\n\npublic static class Program\n{{\n"
            "    public static void Main()\n    {\n" + "\n".join(probe_lines) + "\n    }\n\n"
            "    private static void Probe(string key, Func<bool> check)\n    {\n"
            "        try { Console.WriteLine(key + \"\\t\" + check()); }\n"
            "        catch (Exception) { Console.WriteLine(key + \"\\tFalse\"); }\n    }\n\n"
            + "\n".join(body)
            + "}\n"
        )
        with open(os.path.join(work, "Program.cs"), "w") as handle:
            handle.write(cs)
        lines = cs.splitlines()

        def failing():
            built = subprocess.run(
                ["dotnet", "build", work, "-nologo", "-v", "q", "-clp:NoSummary"],
                capture_output=True,
                text=True,
                cwd=work,
            )
            keys = set()
            for match in re.finditer(r"Program\.cs\((\d+),\d+\): error", built.stdout):
                for j in range(int(match.group(1)) - 1, -1, -1):
                    marker = re.search(r"// @@(.+)$", lines[j])
                    if marker:
                        keys.add(marker.group(1).strip())
                        break
            if built.returncode != 0 and not keys:
                sys.stderr.write(built.stdout[-4000:])
                raise SystemExit("build failed without a snippet error")
            return built.returncode == 0, keys

        bad = set()
        for _ in range(6):
            ok, keys = failing()
            if ok:
                break
            bad |= keys
            # Rewrite the file without the failing snippets and probes.
            cs = rebuild(usings, snippets, probes, bad)
            with open(os.path.join(work, "Program.cs"), "w") as handle:
                handle.write(cs)
            lines = cs.splitlines()
        else:
            raise SystemExit("could not get a clean build")
        run = subprocess.run(["dotnet", "run", "--no-build", "--project", work], capture_output=True, text=True, cwd=work)
        true_probes = {line.split("\t")[0] for line in run.stdout.splitlines() if line.endswith("\tTrue")}
        return set(snippets) - bad, true_probes
    finally:
        shutil.rmtree(work, ignore_errors=True)


def rebuild(usings, snippets, probes, bad):
    body = []
    for i, key in enumerate(sorted(snippets)):
        if key in bad:
            continue
        indented = "\n".join("        " + line if line else "" for line in snippets[key].splitlines())
        body.append(f"    // @@{key}\n    public static async Task S{i}()\n    {{\n{indented}\n    }}\n")
    probe_lines = [
        f"        Probe(\"{key}\", () => {probes[key]}); // @@{key}" for key in sorted(probes) if key not in bad
    ]
    return (
        f"{usings}\n\nnamespace Check;\n\npublic static class Program\n{{\n"
        "    public static void Main()\n    {\n" + "\n".join(probe_lines) + "\n    }\n\n"
        "    private static void Probe(string key, Func<bool> check)\n    {\n"
        "        try { Console.WriteLine(key + \"\\t\" + check()); }\n"
        "        catch (Exception) { Console.WriteLine(key + \"\\tFalse\"); }\n    }\n\n"
        + "\n".join(body)
        + "}\n"
    )


def main():
    providers = find_providers()
    found, methods = collect_models(providers)

    plan = {}
    snippets = {}
    probes = {}
    for name, info in providers.items():
        for kind, counter in found[name].items():
            observed = [m for m, _ in methods[name][kind].most_common()]
            kept = implemented_methods(info, kind, observed)
            if not kept:
                continue
            ids = [i for i, _ in counter.most_common(6)]
            plan.setdefault(name, {})[kind] = {"ids": ids, "methods": sorted(kept), "order": kept}
            for key, (snippet, probe) in variants(info, kind, kept, ids[0]).items():
                full = f"{name}|{kind}|{key}"
                snippets[full] = snippet
                if probe:
                    probes[full] = probe

    ok, true_probes = build_and_probe(snippets, probes)

    shutil.rmtree(OUT, ignore_errors=True)
    os.makedirs(OUT)
    rows = []
    for name, kinds in sorted(plan.items(), key=lambda kv: display(kv[0]).lower()):
        info = providers[name]
        chosen = None
        fallback = None
        examples = [
            (kind, snippets[f"{name}|{kind}|example"])
            for kind in KINDS
            if kind in kinds and f"{name}|{kind}|example" in snippets
        ]
        for example in examples:
            if f"{name}|{example[0]}|example" not in ok:
                raise SystemExit(f"the {name} {example[0]} example does not compile")
        for kind in KINDS:
            if kind not in kinds:
                continue
            for method in kinds[kind]["order"]:
                direct = f"{name}|{kind}|{method}|direct"
                cast = f"{name}|{kind}|{method}|cast"
                if direct in ok:
                    chosen = (kind, snippets[direct])
                elif cast in ok and cast in true_probes:
                    chosen = (kind, snippets[cast])
                if chosen:
                    break
            if chosen:
                break
            if fallback is None:
                for method in kinds[kind]["order"]:
                    create = f"{name}|{kind}|{method}|create"
                    if create in ok:
                        fallback = (kind, snippets[create])
                        break
        if examples:
            write_page(name, info, kinds, examples, False)
        else:
            write_page(name, info, kinds, [chosen or fallback] if chosen or fallback else [], chosen is None)
        rows.append((name, kinds))
    write_index(rows)
    print(f"Wrote {len(rows)} provider pages to {os.path.relpath(OUT, ROOT)}")


def write_page(name, info, kinds, chosen, create_only=False):
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
        "| Kind | Factory method | Model ids in this port's tests and docs |",
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
        "A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.",
        "",
        "## Example",
        "",
    ]
    for kind, snippet in chosen:
        lines += [f"{KIND_TITLE[kind]} model:", "", "```csharp", f"using {info['ns']};", ""]
        lines += [snippet, "```", ""]
    if create_only:
        lines += [
            "This model does not plug into the shared `Generate*` helpers. Call the model through the members the API reference lists for its type.",
            "",
        ]
    lines += ["See the [Providers](providers.md) guide for authentication and provider options.", ""]
    with open(os.path.join(OUT, slug(name) + ".md"), "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines))


def write_index(rows):
    lines = [
        "# Models",
        "",
        "One page per provider, with the model kinds this port implements, the model ids its tests and docs use, a link to the upstream model catalog, and a C# example.",
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
