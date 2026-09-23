#!/usr/bin/env python3
"""Generate thin OpenAI-compatible providers and modality-specific providers."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HEADER = """// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

"""

THIN = [
    ("Alibaba", "alibaba", "https://dashscope-intl.aliyuncs.com/compatible-mode/v1", "DASHSCOPE_API_KEY", True, False, []),
    ("Baseten", "baseten", "https://inference.baseten.co/v1", "BASETEN_API_KEY", True, False, []),
    ("Cerebras", "cerebras", "https://api.cerebras.ai/v1", "CEREBRAS_API_KEY", False, False, []),
    ("DeepInfra", "deepinfra", "https://api.deepinfra.com/v1/openai", "DEEPINFRA_API_KEY", True, True, []),
    ("DeepSeek", "deepseek", "https://api.deepseek.com", "DEEPSEEK_API_KEY", False, False, []),
    ("Fireworks", "fireworks", "https://api.fireworks.ai/inference/v1", "FIREWORKS_API_KEY", True, True, []),
    ("Groq", "groq", "https://api.groq.com/openai/v1", "GROQ_API_KEY", False, False, []),
    ("HuggingFace", "huggingface", "https://router.huggingface.co/v1", "HUGGINGFACE_API_KEY", True, False, []),
    ("MiniMax", "minimax", "https://api.minimax.io/v1", "MINIMAX_API_KEY", False, False, []),
    ("Moonshot", "moonshotai", "https://api.moonshot.ai/v1", "MOONSHOT_API_KEY", False, False, []),
    ("Perplexity", "perplexity", "https://api.perplexity.ai", "PERPLEXITY_API_KEY", True, False, []),
    ("TogetherAI", "togetherai", "https://api.together.xyz/v1", "TOGETHER_AI_API_KEY", True, True, ["TOGETHER_API_KEY"]),
    ("Xai", "xai", "https://api.x.ai/v1", "XAI_API_KEY", False, True, []),
    ("Zai", "zai", "https://api.z.ai/api/paas/v4", "ZAI_API_KEY", False, False, []),
    ("GmiCloud", "gmicloud", "https://api.gmi-serving.com/v1", "GMI_CLOUD_APIKEY", False, False, []),
    ("Mistral", "mistral", "https://api.mistral.ai/v1", "MISTRAL_API_KEY", True, False, []),
]

# name, id, env, base, auth (Bearer|ApiKeyHeader|Token|Key|RawAuthorization|CustomHeader), header name, kind, path, extra json keys
MEDIA = [
    ("AssemblyAI", "assemblyai", "ASSEMBLYAI_API_KEY", "https://api.assemblyai.com", "RawAuthorization", "Authorization", "transcription", "/v2/transcript", '{"audio_url":"data"}'),
    ("BlackForestLabs", "black-forest-labs", "BFL_API_KEY", "https://api.bfl.ai/v1", "CustomHeader", "x-key", "image", "/flux-pro-1.1", None),
    ("ByteDance", "bytedance", "ARK_API_KEY", "https://ark.ap-southeast.bytepluses.com/api/v3", "Bearer", "Authorization", "image", "/images/generations", None),
    ("Cartesia", "cartesia", "CARTESIA_API_KEY", "https://api.cartesia.ai", "Bearer", "Authorization", "speech", "/tts/bytes", None),
    ("Deepgram", "deepgram", "DEEPGRAM_API_KEY", "https://api.deepgram.com", "Token", "Authorization", "transcription", "/v1/listen", None),
    ("ElevenLabs", "elevenlabs", "ELEVENLABS_API_KEY", "https://api.elevenlabs.io", "CustomHeader", "xi-api-key", "speech", "/v1/text-to-speech/{voice}", None),
    ("Fal", "fal", "FAL_API_KEY", "https://fal.run", "Key", "Authorization", "image", "/fal-ai/flux/dev", None),
    ("FishAudio", "fish-audio", "FISH_AUDIO_API_KEY", "https://api.fish.audio", "Bearer", "Authorization", "speech", "/v1/tts", None),
    ("Gladia", "gladia", "GLADIA_API_KEY", "https://api.gladia.io", "CustomHeader", "x-gladia-key", "transcription", "/v2/transcription", None),
    ("Hume", "hume", "HUME_API_KEY", "https://api.hume.ai", "CustomHeader", "X-Hume-Api-Key", "speech", "/v0/tts", None),
    ("KlingAI", "klingai", "KLINGAI_API_KEY", "https://api-singapore.klingai.com", "Bearer", "Authorization", "video", "/v1/videos/text2video", None),
    ("Luma", "luma", "LUMA_API_KEY", "https://api.lumalabs.ai", "Bearer", "Authorization", "video", "/dream-machine/v1/generations", None),
    ("Prodia", "prodia", "PRODIA_API_KEY", "https://inference.prodia.com/v2", "Bearer", "Authorization", "image", "/job", None),
    ("QuiverAI", "quiverai", "QUIVERAI_API_KEY", "https://api.quiver.ai/v1", "Bearer", "Authorization", "image", "/images/generations", None),
    ("Replicate", "replicate", "REPLICATE_API_TOKEN", "https://api.replicate.com/v1", "Bearer", "Authorization", "image", "/models/{model}/predictions", None),
    ("RevAI", "revai", "REVAI_API_KEY", "https://api.rev.ai", "Bearer", "Authorization", "transcription", "/speechtotext/v1/jobs", None),
    ("Voyage", "voyage", "VOYAGE_API_KEY", "https://api.voyageai.com/v1", "Bearer", "Authorization", "embedding", "/embeddings", None),
]


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def csproj(folder: str, description: str, refs: list[str]) -> None:
    items = "\n    ".join(f'<ProjectReference Include="..\\{ref}\\{ref}.csproj" />' for ref in refs)
    packages = """<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.1" />"""
    write(
        ROOT / "src" / folder / f"{folder}.csproj",
        f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>{description} Not an official Vercel product.</Description>
    <PackageTags>ai;vercel</PackageTags>
  </PropertyGroup>
  <ItemGroup>
    {items}
    {packages}
  </ItemGroup>
</Project>
""",
    )


def thin_source(class_name, provider_id, base, env, embeddings, images, extra_env):
    extra = ", ".join(f'"{name}"' for name in extra_env)
    extra_assign = f"options.AdditionalApiKeyEnvironmentVariables = new[] {{ {extra} }};" if extra else ""
    return f"""{HEADER}using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;

namespace Vercel.AI.Sdk.{class_name};

/// <summary>{class_name} provider. OpenAI Chat Completions compatible at <c>{base}</c>.</summary>
public sealed class {class_name}Provider : OpenAICompatibleProvider
{{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "{provider_id}";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "{base}";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "{env}";

    /// <summary>Creates a provider.</summary>
    public {class_name}Provider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {{
    }}

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new {class_name}Provider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {{
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new {class_name}Provider(client, options);
    }}

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {{
        options ??= new OpenAICompatibleOptions();
        if (string.IsNullOrEmpty(options.ProviderName) || options.ProviderName == "openai-compatible")
        {{
            options.ProviderName = ProviderId;
        }}

        if (options.BaseUrl == "https://api.openai.com/v1")
        {{
            options.BaseUrl = DefaultBaseUrl;
        }}

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY")
        {{
            options.ApiKeyEnvironmentVariable = ApiKeyVariable;
        }}

        options.SupportsEmbeddings = {str(embeddings).lower()};
        options.SupportsImages = {str(images).lower()};
        {extra_assign}
        return options;
    }}
}}

/// <summary>Registers <see cref="{class_name}Provider"/>.</summary>
public static class {class_name}ServiceCollectionExtensions
{{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection Add{class_name}(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {{
        services.AddHttpClient({class_name}Provider.ProviderId);
        services.AddSingleton(sp =>
        {{
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new {class_name}Provider(sp.GetRequiredService<IHttpClientFactory>().CreateClient({class_name}Provider.ProviderId), options);
        }});
        return services;
    }}
}}
"""


def azure_source():
    return f"""{HEADER}using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;

namespace Vercel.AI.Sdk.Azure;

/// <summary>Azure OpenAI settings.</summary>
public sealed class AzureOpenAIOptions
{{
    /// <summary>Resource host, for example <c>https://my-resource.openai.azure.com</c>.</summary>
    public string? BaseUrl {{ get; set; }}

    /// <summary>Resource name used when <see cref="BaseUrl"/> is empty.</summary>
    public string? ResourceName {{ get; set; }}

    /// <summary>Explicit key. Falls back to <c>AZURE_API_KEY</c>.</summary>
    public string? ApiKey {{ get; set; }}

    /// <summary>API version query parameter.</summary>
    public string ApiVersion {{ get; set; }} = "2024-10-21";
}}

/// <summary>Azure OpenAI provider. Chat calls use the deployments route and the <c>api-key</c> header.</summary>
public sealed class AzureOpenAIProvider : OpenAICompatibleProvider
{{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "azure";

    /// <summary>Creates a provider.</summary>
    public AzureOpenAIProvider(HttpClient httpClient, AzureOpenAIOptions? options = null)
        : base(Prepare(options), httpClient)
    {{
    }}

    /// <summary>Creates a provider.</summary>
    public static AzureOpenAIProvider Create(AzureOpenAIOptions? options = null, HttpMessageHandler? handler = null)
    {{
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AzureOpenAIProvider(client, options);
    }}

    private static OpenAICompatibleOptions Prepare(AzureOpenAIOptions? options)
    {{
        options ??= new AzureOpenAIOptions();
        var resource = options.ResourceName ?? "resource";
        var baseUrl = string.IsNullOrEmpty(options.BaseUrl) ? "https://" + resource + ".openai.azure.com" : options.BaseUrl!;
        return new OpenAICompatibleOptions
        {{
            ProviderName = ProviderId,
            BaseUrl = baseUrl,
            ApiKey = options.ApiKey,
            ApiKeyEnvironmentVariable = "AZURE_API_KEY",
            ApiKeyStyle = ApiKeyStyle.ApiKeyHeader,
            ApiKeyHeaderName = "api-key",
            AzureApiVersion = options.ApiVersion,
            SupportsEmbeddings = true,
            SupportsImages = false,
        }};
    }}
}}

/// <summary>Registers Azure OpenAI.</summary>
public static class AzureOpenAIServiceCollectionExtensions
{{
    /// <summary>Adds <see cref="AzureOpenAIProvider"/>.</summary>
    public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, Action<AzureOpenAIOptions>? configure = null)
    {{
        services.AddHttpClient(AzureOpenAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {{
            var options = new AzureOpenAIOptions();
            configure?.Invoke(options);
            return new AzureOpenAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AzureOpenAIProvider.ProviderId), options);
        }});
        return services;
    }}
}}
"""


def auth_lines(style, header):
    if style == "Bearer":
        return "ApiKeyStyle = ApiKeyStyle.Bearer;"
    if style == "Token":
        return "ApiKeyStyle = ApiKeyStyle.Token;"
    if style == "Key":
        return "ApiKeyStyle = ApiKeyStyle.Key;"
    if style == "RawAuthorization":
        return "ApiKeyStyle = ApiKeyStyle.RawAuthorization;"
    return f'ApiKeyStyle = ApiKeyStyle.CustomHeader;\n        options.ApiKeyHeaderName = "{header}";'


def media_source(class_name, provider_id, env, base, style, header, kind, path):
    auth = auth_lines(style, header)
    if kind == "speech":
        model = f"""
    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new Speech(this, modelId);

    private sealed class Speech : ISpeechModel
    {{
        private readonly {class_name}Provider _provider;
        public Speech({class_name}Provider provider, string modelId) {{ _provider = provider; ModelId = modelId; }}
        public string Provider => "{provider_id}";
        public string ModelId {{ get; }}
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {{
            var voice = options.Voice ?? "default";
            var path = "{path}".Replace("{{voice}}", voice);
            var body = new JsonObject {{ ["text"] = options.Text, ["model_id"] = ModelId, ["model"] = ModelId }};
            var bytes = await _provider.Http.SendBytesAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            return new SpeechResult(bytes, "audio/mpeg");
        }}
    }}
"""
    elif kind == "transcription":
        model = f"""
    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new Transcription(this, modelId);

    private sealed class Transcription : ITranscriptionModel
    {{
        private readonly {class_name}Provider _provider;
        public Transcription({class_name}Provider provider, string modelId) {{ _provider = provider; ModelId = modelId; }}
        public string Provider => "{provider_id}";
        public string ModelId {{ get; }}
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {{
            var body = new JsonObject {{ ["model"] = ModelId, ["audio"] = Convert.ToBase64String(audio.Data) }};
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "{path}"), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var text = document.RootElement.TryGetProperty("text", out var value) ? value.GetString() ?? string.Empty : string.Empty;
            return new TranscriptionResult(text);
        }}
    }}
"""
    elif kind == "image":
        model = f"""
    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    private sealed class Image : IImageModel
    {{
        private readonly {class_name}Provider _provider;
        public Image({class_name}Provider provider, string modelId) {{ _provider = provider; ModelId = modelId; }}
        public string Provider => "{provider_id}";
        public string ModelId {{ get; }}
        public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {{
            var path = "{path}".Replace("{{model}}", ModelId);
            var body = new JsonObject {{ ["prompt"] = options.Prompt, ["model"] = ModelId }};
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] {{ new GeneratedImage("image/png", null, url) }});
        }}
    }}
"""
    elif kind == "video":
        model = f"""
    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new Video(this, modelId);

    private sealed class Video : IVideoModel
    {{
        private readonly {class_name}Provider _provider;
        public Video({class_name}Provider provider, string modelId) {{ _provider = provider; ModelId = modelId; }}
        public string Provider => "{provider_id}";
        public string ModelId {{ get; }}
        public async Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
        {{
            var body = new JsonObject {{ ["prompt"] = options.Prompt, ["model"] = ModelId }};
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "{path}"), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new VideoResult(url, null, "video/mp4");
        }}
    }}
"""
    else:
        model = f"""
    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId) => new Embedding(this, modelId);

    private sealed class Embedding : IEmbeddingModel
    {{
        private readonly {class_name}Provider _provider;
        public Embedding({class_name}Provider provider, string modelId) {{ _provider = provider; ModelId = modelId; }}
        public string SpecificationVersion => "V4";
        public string Provider => "{provider_id}";
        public string ModelId {{ get; }}
        public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
        {{
            var input = new JsonArray();
            foreach (var value in values) input.Add(value);
            var body = new JsonObject {{ ["model"] = ModelId, ["input"] = input }};
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "{path}"), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var vectors = new List<float[]>();
            foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
            {{
                var embedding = item.GetProperty("embedding");
                var vector = new float[embedding.GetArrayLength()];
                var index = 0;
                foreach (var number in embedding.EnumerateArray()) vector[index++] = number.GetSingle();
                vectors.Add(vector);
            }}
            return new EmbeddingResult(vectors, null);
        }}
    }}
"""
    text_using = "using System.Text;\n" if kind == "speech" else ""
    return f"""{HEADER}{text_using}using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.{class_name};

/// <summary>{class_name} provider.</summary>
public sealed class {class_name}Provider : OpenAICompatibleProvider
{{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "{provider_id}";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "{base}";

    /// <summary>Creates a provider.</summary>
    public {class_name}Provider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {{
    }}

    /// <summary>Creates a provider.</summary>
    public static new {class_name}Provider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {{
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new {class_name}Provider(client, options);
    }}

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {{
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "{env}";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.{auth}
        return options;
    }}
{model}
}}

/// <summary>Registers {class_name}.</summary>
public static class {class_name}ServiceCollectionExtensions
{{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection Add{class_name}(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {{
        services.AddHttpClient({class_name}Provider.ProviderId);
        services.AddSingleton(sp =>
        {{
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new {class_name}Provider(sp.GetRequiredService<IHttpClientFactory>().CreateClient({class_name}Provider.ProviderId), options);
        }});
        return services;
    }}
}}
"""


def main() -> None:
    for item in THIN:
        class_name, provider_id, base, env, embeddings, images, extra = item
        folder = f"Vercel.AI.Sdk.{class_name}"
        csproj(folder, f"{class_name} provider for the community .NET port of the Vercel AI SDK.", ["Vercel.AI.Sdk.OpenAICompatible"])
        write(ROOT / "src" / folder / f"{class_name}Provider.cs", thin_source(class_name, provider_id, base, env, embeddings, images, extra))

    folder = "Vercel.AI.Sdk.Azure"
    csproj(folder, "Azure OpenAI provider for the community .NET port of the Vercel AI SDK.", ["Vercel.AI.Sdk.OpenAICompatible"])
    write(ROOT / "src" / folder / "AzureOpenAIProvider.cs", azure_source())

    for class_name, provider_id, env, base, style, header, kind, path, _extra in MEDIA:
        folder = f"Vercel.AI.Sdk.{class_name}"
        csproj(folder, f"{class_name} provider for the community .NET port of the Vercel AI SDK.", ["Vercel.AI.Sdk.OpenAICompatible"])
        write(ROOT / "src" / folder / f"{class_name}Provider.cs", media_source(class_name, provider_id, env, base, style, header, kind, path))

    print("generated", len(THIN) + 1 + len(MEDIA), "providers")


if __name__ == "__main__":
    main()
