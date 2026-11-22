// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Anthropic;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Google;

/// <summary>Settings for Anthropic on Vertex.</summary>
public sealed class GoogleVertexAnthropicOptions
{
    /// <summary>Project id.</summary>
    public string? Project { get; set; }

    /// <summary>Location. One DNS label.</summary>
    public string? Location { get; set; }

    /// <summary>Publisher base URL. When empty, it is built from the project and location.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Headers merged into every request. A caller-supplied Authorization value wins over the generated token.</summary>
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns a bearer token. Called once per request. Without it, no Authorization header is added.</summary>
    public Func<CancellationToken, Task<string>>? GenerateAuthToken { get; set; }

    /// <summary>
    /// Enables native structured output in <c>structuredOutputMode: "auto"</c> for Claude 4.5 and newer. Defaults to false.
    /// Requires the Google Cloud organization policy to allow the <c>structured_outputs</c> feature for each model used.
    /// This does not enable strict tool input schemas.
    /// </summary>
    public bool SupportsNativeStructuredOutput { get; set; }
}

/// <summary>Settings for the OpenAI-compatible MaaS and xAI endpoints on Vertex.</summary>
public sealed class GoogleVertexPartnerOptions
{
    /// <summary>Project id.</summary>
    public string? Project { get; set; }

    /// <summary>Location. One DNS label. Defaults to <c>global</c>.</summary>
    public string? Location { get; set; }

    /// <summary>OpenAI-compatible base URL. When empty, it is built from the project and location.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Headers set on every request. They replace request headers with the same name.</summary>
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns a bearer token. Called once per request. The token replaces any Authorization header.</summary>
    public Func<CancellationToken, Task<string>>? GenerateAuthToken { get; set; }

    /// <summary>
    /// Client for image downloads and redirect hops off the provider origin. It carries no credentials and must not follow redirects,
    /// because each hop is validated before it is requested. Null uses a shared client that does not follow them.
    /// Upstream sends these downloads through the configured <c>fetch</c>.
    /// </summary>
    public HttpClient? DownloadHttpClient { get; set; }
}

/// <summary>
/// Anthropic on Vertex. Sends the Anthropic Messages body to <c>rawPredict</c> with <c>anthropic_version</c> in the body.
/// Vertex does not accept URL sources or strict tools. Native output is opt-in.
/// </summary>
public sealed class GoogleVertexAnthropicProvider : AnthropicProvider
{
    /// <summary>Provider id reported by language models.</summary>
    public const string ModelProviderName = "googleVertex.anthropic.messages";

    private static readonly IReadOnlyDictionary<string, string> NoUrls =
        new Dictionary<string, string>();

    private const string LabelsHeader = "x-vertex-ai-labels";

    private readonly GoogleVertexAnthropicOptions _vertex;

    /// <summary>Creates a provider.</summary>
    public GoogleVertexAnthropicProvider(
        HttpClient httpClient,
        GoogleVertexAnthropicOptions? options = null
    )
        : base(httpClient, Anthropic(options ??= new GoogleVertexAnthropicOptions()))
    {
        _vertex = options;
    }

    /// <summary>Provider tool ids that Vertex accepts. A subset of the Anthropic tools.</summary>
    public static new IReadOnlyList<string> Tools { get; } =
        new[]
        {
            "anthropic.bash_20241022",
            "anthropic.bash_20250124",
            "anthropic.text_editor_20241022",
            "anthropic.text_editor_20250124",
            "anthropic.text_editor_20250429",
            "anthropic.text_editor_20250728",
            "anthropic.computer_20241022",
            "anthropic.web_search_20250305",
            "anthropic.tool_search_regex_20251119",
            "anthropic.tool_search_bm25_20251119",
        };

    /// <summary>Project id.</summary>
    public string? Project => _vertex.Project;

    /// <summary>Location.</summary>
    public string? Location => _vertex.Location;

    /// <summary>Publisher base URL.</summary>
    public string BaseUrl => Options.BaseUrl;

    /// <inheritdoc />
    protected internal override bool SupportsNativeStructuredOutput =>
        _vertex.SupportsNativeStructuredOutput;

    /// <inheritdoc />
    // Native output is default-denied by GCP policy and requires Claude 4.5+.
    protected internal override bool SupportsNativeStructuredOutputFor(string modelId)
    {
        return _vertex.SupportsNativeStructuredOutput
            && SupportsNativeStructuredOutputModel(modelId);
    }

    /// <inheritdoc />
    protected internal override bool SupportsStrictTools => false;

    /// <inheritdoc />
    protected internal override IReadOnlyDictionary<string, string> SupportedUrls => NoUrls;

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static GoogleVertexAnthropicProvider Create(
        GoogleVertexAnthropicOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new GoogleVertexAnthropicProvider(client, options);
    }

    /// <summary><c>rawPredict</c> or <c>streamRawPredict</c> URL for a model.</summary>
    public string PredictUrl(string modelId, bool streaming)
    {
        return BaseUrl.TrimEnd('/')
            + "/"
            + modelId
            + ":"
            + (streaming ? "streamRawPredict" : "rawPredict");
    }

    /// <summary>Removes <c>model</c> and sets <c>anthropic_version</c>.</summary>
    public static JsonObject TransformBody(JsonObject body)
    {
        var clone = (JsonObject)body.DeepClone();
        clone.Remove("model");
        clone["anthropic_version"] = "vertex-2023-10-16";
        return clone;
    }

    /// <summary>Sends <c>providerOptions.vertex.labels</c> as the <c>x-vertex-ai-labels</c> header. An explicit header wins.</summary>
    public static Dictionary<string, string?> TransformHeaders(
        Dictionary<string, string?> headers,
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue("vertex", out var vertex)
            || vertex.ValueKind != JsonValueKind.Object
            || !vertex.TryGetProperty("labels", out var labels)
        )
        {
            return headers;
        }

        if (labels.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentError(
                "providerOptions.vertex.labels",
                labels.ToString(),
                "labels must be an object with string values."
            );
        }

        foreach (var property in labels.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new InvalidArgumentError(
                    "providerOptions.vertex.labels",
                    labels.ToString(),
                    "labels must be an object with string values."
                );
            }
        }

        var result = new Dictionary<string, string?>(headers, StringComparer.OrdinalIgnoreCase);
        if (result.TryGetValue(LabelsHeader, out var existing) && existing != null)
        {
            return result;
        }

        result[LabelsHeader] = Convert.ToBase64String(Encoding.UTF8.GetBytes(Compact(labels)));
        return result;
    }

    private static string Compact(JsonElement labels)
    {
        using var stream = new MemoryStream();
        using (
            var writer = new Utf8JsonWriter(
                stream,
                new JsonWriterOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }
            )
        )
        {
            labels.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>Generated bearer token, then configured headers, call headers, and beta flags.</summary>
    public override async Task<Dictionary<string, string?>> CreateHeadersAsync(
        IReadOnlyList<string> betas,
        IReadOnlyDictionary<string, string?>? callHeaders,
        string? body,
        CancellationToken cancellationToken
    )
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (_vertex.GenerateAuthToken != null)
        {
            headers["Authorization"] =
                "Bearer "
                + await _vertex.GenerateAuthToken(cancellationToken).ConfigureAwait(false);
        }

        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        if (betas.Count > 0)
        {
            headers["anthropic-beta"] = string.Join(",", betas);
        }

        return headers;
    }

    /// <summary>Embedding models are not available on this provider.</summary>
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new AiSdkException(
            "Provider 'google.vertex.anthropic' does not support embedding model '" + modelId + "'."
        );
    }

    /// <inheritdoc />
    protected internal override Uri RequestUri(string modelId, bool streaming)
    {
        return new Uri(PredictUrl(modelId, streaming));
    }

    private static bool SupportsNativeStructuredOutputModel(string modelId)
    {
        var match = Regex.Match(
            modelId,
            @"^claude-[a-z]+-(\d+)(?:-(\d+))?(?:@|\z)",
            RegexOptions.None,
            TimeSpan.FromSeconds(1)
        );
        if (!match.Success)
        {
            return false;
        }

        var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var minor = match.Groups[2].Success
            ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
            : 0;
        return major > 4 || (major == 4 && minor >= 5);
    }

    private static AnthropicOptions Anthropic(GoogleVertexAnthropicOptions vertex)
    {
        var options = new AnthropicOptions
        {
            BaseUrl = GoogleVertexEndpoints.AnthropicBaseUrl(
                vertex.Project,
                vertex.Location,
                vertex.BaseUrl
            ),
            Name = ModelProviderName,
            TransformRequestBody = TransformBody,
            TransformRequestHeaders = TransformHeaders,
        };
        foreach (var pair in vertex.Headers)
        {
            options.Headers[pair.Key] = pair.Value;
        }

        return options;
    }
}

/// <summary>Vertex MaaS OpenAI-compatible endpoint.</summary>
public sealed class GoogleVertexMaasProvider
{
    private static readonly Dictionary<string, int> MaxTokens = new(StringComparer.Ordinal)
    {
        ["meta/llama-4-maverick-17b-128e-instruct-maas"] = 8192,
        ["meta/llama-4-scout-17b-16e-instruct-maas"] = 8192,
    };

    private readonly GoogleVertexPartnerOptions _options;

    /// <summary>Creates a provider. The OpenAI-compatible client is created on first use.</summary>
    public GoogleVertexMaasProvider(
        string project,
        string? location,
        string? baseUrl = null,
        HttpMessageHandler? handler = null
    )
        : this(
            new GoogleVertexPartnerOptions
            {
                Project = project,
                Location = location,
                BaseUrl = baseUrl,
            },
            handler
        ) { }

    /// <summary>Creates a provider. <paramref name="handler"/> sends the request after the auth headers are added.</summary>
    public GoogleVertexMaasProvider(
        GoogleVertexPartnerOptions options,
        HttpMessageHandler? handler = null
    )
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        Project = options.Project
            ?? throw new ArgumentNullException(nameof(options), "A Vertex project is required.");
        Location = string.IsNullOrEmpty(options.Location) ? "global" : options.Location!;
        BaseUrl = GoogleVertexEndpoints.MaasBaseUrl(Project, Location, options.BaseUrl);
        Handler = handler;
    }

    /// <summary>Project id.</summary>
    public string Project { get; }

    /// <summary>Location. Defaults to <c>global</c>.</summary>
    public string Location { get; }

    /// <summary>OpenAI-compatible base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>True after <see cref="LanguageModel"/> has created the inner provider.</summary>
    public bool ClientCreated => Volatile.Read(ref _client) != null;

    private HttpMessageHandler? Handler { get; }

    private readonly object _clientGate = new();

    private OpenAICompatibleProvider? _client;

    /// <summary>Returns a chat model and caches the inner provider.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        return Client().LanguageModel(modelId);
    }

    /// <summary>Returns an image model. An image URL on another origin is downloaded without the provider credentials.</summary>
    public IImageModel ImageModel(string modelId)
    {
        return Client().ImageModel(modelId);
    }

    private OpenAICompatibleProvider Client()
    {
        lock (_clientGate)
        {
            return _client ??= new GoogleVertexOpenAICompatibleProvider(
                new OpenAICompatibleOptions
                {
                    BaseUrl = BaseUrl,
                    ProviderName = "vertex.maas",
                    SupportsImages = true,
                    DownloadHttpClient = _options.DownloadHttpClient,
                },
                _options,
                Handler
            );
        }
    }

    /// <summary>Sets Llama 4 <c>max_tokens</c> when the caller did not.</summary>
    public static JsonObject TransformBody(string modelId, JsonObject body)
    {
        var clone = (JsonObject)body.DeepClone();
        if (MaxTokens.TryGetValue(modelId, out var max) && !clone.ContainsKey("max_tokens"))
        {
            clone["max_tokens"] = max;
        }

        return clone;
    }
}

/// <summary>xAI Grok models hosted on Vertex.</summary>
public sealed class GoogleVertexXaiProvider
{
    private readonly GoogleVertexPartnerOptions _options;

    /// <summary>Creates a provider.</summary>
    public GoogleVertexXaiProvider(
        string project,
        string? location,
        string? baseUrl = null,
        HttpMessageHandler? handler = null
    )
        : this(
            new GoogleVertexPartnerOptions
            {
                Project = project,
                Location = location,
                BaseUrl = baseUrl,
            },
            handler
        ) { }

    /// <summary>Creates a provider. <paramref name="handler"/> sends the request after the auth headers are added.</summary>
    public GoogleVertexXaiProvider(
        GoogleVertexPartnerOptions options,
        HttpMessageHandler? handler = null
    )
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        Project = options.Project
            ?? throw new ArgumentNullException(nameof(options), "A Vertex project is required.");
        Location = string.IsNullOrEmpty(options.Location) ? "global" : options.Location!;
        BaseUrl = GoogleVertexEndpoints.XaiBaseUrl(Project, Location, options.BaseUrl);
        Handler = handler;
    }

    /// <summary>Project id.</summary>
    public string Project { get; }

    /// <summary>Location.</summary>
    public string Location { get; }

    /// <summary>OpenAI-compatible base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>True after the inner provider has been created.</summary>
    public bool ClientCreated => Volatile.Read(ref _client) != null;

    private HttpMessageHandler? Handler { get; }

    private readonly object _clientGate = new();

    private OpenAICompatibleProvider? _client;

    /// <summary>Returns a chat model.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        return Client().LanguageModel(modelId);
    }

    private OpenAICompatibleProvider Client()
    {
        lock (_clientGate)
        {
            return _client ??= new GoogleVertexOpenAICompatibleProvider(
                new OpenAICompatibleOptions
                {
                    BaseUrl = BaseUrl,
                    ProviderName = "googleVertex.xai",
                    IncludeUsage = true,
                    SupportsStructuredOutputs = true,
                    SupportedUrls = new Dictionary<string, string>
                    {
                        ["image/*"] = "^https?://.*\\z",
                    },
                    TransformRequestBody = (body, _) => TransformBody(body),
                },
                _options,
                Handler
            );
        }
    }

    /// <summary>Removes <c>reasoning_effort</c>. Vertex Grok does not accept it.</summary>
    public static JsonObject TransformBody(JsonObject body)
    {
        var clone = (JsonObject)body.DeepClone();
        clone.Remove("reasoning_effort");
        return clone;
    }

    /// <summary>
    /// Counts Grok reasoning tokens separately. Output total is completion tokens plus reasoning tokens.
    /// </summary>
    public static GoogleTokenUsage ConvertUsage(
        int? promptTokens,
        int? completionTokens,
        int? cachedTokens,
        int? reasoningTokens
    )
    {
        var prompt = promptTokens ?? 0;
        var completion = completionTokens ?? 0;
        var cached = cachedTokens ?? 0;
        var reasoning = reasoningTokens ?? 0;
        return new GoogleTokenUsage(
            prompt,
            prompt - cached,
            cached,
            completion + reasoning,
            completion,
            reasoning,
            null
        );
    }

    /// <summary>Embedding and image models are not available.</summary>
    public IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new AiSdkException(
            "Provider 'google.vertex.xai' does not support embedding model '" + modelId + "'."
        );
    }
}

/// <summary>OpenAI-compatible client for Vertex. Vertex authenticates with <see cref="GoogleVertexAuthHandler"/>, never an API key.</summary>
internal sealed class GoogleVertexOpenAICompatibleProvider : OpenAICompatibleProvider
{
    public GoogleVertexOpenAICompatibleProvider(
        OpenAICompatibleOptions options,
        GoogleVertexPartnerOptions vertex,
        HttpMessageHandler? handler
    )
        : base(
            options,
            new HttpClient(
                new GoogleVertexAuthHandler(
                    vertex,
                    handler ?? new GlobalFetchHandler(allowAutoRedirect: true)
                ),
                disposeHandler: false
            )
        ) { }

    /// <summary>Returns no key, so an <c>OPENAI_API_KEY</c> in the environment is never sent to Vertex.</summary>
    protected override Task<string?> ResolveApiKeyAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }
}

/// <summary>Sets the configured headers, then a generated bearer token, on each request.</summary>
internal sealed class GoogleVertexAuthHandler : DelegatingHandler
{
    private readonly GoogleVertexPartnerOptions _options;

    public GoogleVertexAuthHandler(GoogleVertexPartnerOptions options, HttpMessageHandler inner)
        : base(inner)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token =
            _options.GenerateAuthToken == null
                ? null
                : await _options.GenerateAuthToken(cancellationToken).ConfigureAwait(false);
        foreach (var pair in _options.Headers)
        {
            Set(request, pair.Key, pair.Value);
        }

        if (token != null)
        {
            Set(request, "Authorization", "Bearer " + token);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static void Set(HttpRequestMessage request, string name, string? value)
    {
        request.Headers.Remove(name);
        if (value != null)
        {
            request.Headers.TryAddWithoutValidation(name, value);
        }
    }
}
