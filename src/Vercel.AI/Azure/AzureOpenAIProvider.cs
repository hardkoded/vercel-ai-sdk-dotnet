// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>Azure OpenAI settings.</summary>
public sealed class AzureOpenAIOptions
{
    /// <summary>
    /// Base URL prefix, for example <c>https://my-resource.openai.azure.com/openai</c>. Defaults to
    /// <c>https://{resourceName}.openai.azure.com/openai</c>. A URL that is not an Azure host is a custom gateway and is used as is.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Resource name used when <see cref="BaseUrl"/> is empty.
    /// Must be a single DNS label (letters, digits, and hyphens).
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Explicit key: a value, a task, or a synchronous or asynchronous callback that is resolved before each request.
    /// Falls back to <c>AZURE_API_KEY</c>.
    /// </summary>
    public Resolvable<string>? ApiKey { get; set; }

    /// <summary>API version query parameter. Defaults to <c>v1</c>, also when set to null or empty.</summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Uses the legacy <c>/openai/deployments/{deploymentId}{path}</c> routes instead of <c>/openai/v1{path}</c>.
    /// </summary>
    public bool UseDeploymentBasedUrls { get; set; }

    /// <summary>Microsoft Entra token provider. Each call receives a fresh bearer token.</summary>
    public Func<string>? TokenProvider { get; set; }

    /// <summary>
    /// URL prefix for Azure Speech (MAI-Transcribe transcription and MAI-Voice speech), for example
    /// <c>https://eastus.api.cognitive.microsoft.com</c>. Defaults to
    /// <c>https://{resourceName}.cognitiveservices.azure.com</c>. Speech requests do not use
    /// <see cref="BaseUrl"/> or <see cref="ApiVersion"/>. Maps to <c>speechBaseURL</c>.
    /// </summary>
    public string? SpeechBaseUrl { get; set; }

    /// <summary>
    /// URL prefix for MAI APIs (MAI-Image, MAI-Transcribe-2-Streaming). Defaults to
    /// <c>https://{resourceName}.services.ai.azure.com/mai/v1</c>. Maps to <c>maiBaseURL</c>.
    /// </summary>
    public string? MaiBaseUrl { get; set; }

    /// <summary>
    /// Custom WebSocket implementation for MAI streaming transcription. Without one, the key is sent as the
    /// <c>api-key</c> query parameter and Microsoft Entra tokens are rejected, because the default WebSocket cannot send
    /// headers. Maps to <c>webSocket</c>.
    /// </summary>
    public WebSocketFactory? WebSocket { get; set; }

    /// <summary>
    /// The WebSocket used for MAI streaming transcription when <see cref="WebSocket"/> is not set, and for OpenAI
    /// realtime streaming transcription. Defaults to <c>ClientWebSocket</c>. Maps to <c>globalThis.WebSocket</c>, which
    /// upstream tests stub.
    /// </summary>
    internal WebSocketFactory? NativeWebSocket { get; set; }

    /// <summary>Headers copied onto every request.</summary>
    public Dictionary<string, string> Headers { get; } = new();
}

/// <summary>Settings for an Azure OpenAI language model.</summary>
public sealed class AzureOpenAILanguageModelSettings
{
    /// <summary>
    /// Underlying model ID used for reported identity and capability detection.
    /// Defaults to the deployment ID. Requests always use the deployment ID.
    /// </summary>
    public string? ModelId { get; set; }
}

/// <summary>
/// Azure OpenAI provider. Calls use the <c>/openai/v1</c> routes and the <c>api-key</c> header, or the
/// deployments routes when <see cref="AzureOpenAIOptions.UseDeploymentBasedUrls"/> is set.
/// </summary>
public sealed class AzureOpenAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "azure";

    // $ can succeed before a trailing newline, so the match must cover the whole value.
    private static readonly Regex HostnamePart = new(
        "^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled
    );

    private readonly AzureOpenAIOptions _azure;

    /// <summary>Creates a provider.</summary>
    public AzureOpenAIProvider(HttpClient httpClient, AzureOpenAIOptions? options = null)
        : this(httpClient, options, null) { }

    // The download client is the transport for untrusted file URLs: it never carries the provider credentials.
    // It maps to the raw `downloadFetch` delegate that upstream hands to the image models.
    private AzureOpenAIProvider(
        HttpClient httpClient,
        AzureOpenAIOptions? options,
        HttpClient? downloadClient
    )
        : base(Prepare(options, downloadClient), httpClient)
    {
        _azure = options ?? new AzureOpenAIOptions();
    }

    /// <summary>The Azure options this provider was created with.</summary>
    internal AzureOpenAIOptions AzureOptions => _azure;

    /// <summary>Entra token provider, when one is configured.</summary>
    internal Func<string>? TokenProvider => _azure.TokenProvider;

    /// <summary>
    /// Azure Speech URL prefix without a trailing slash. Maps to <c>speechBaseURL()</c>.
    /// </summary>
    internal string SpeechBaseUrl()
    {
        return !string.IsNullOrEmpty(_azure.SpeechBaseUrl)
            ? _azure.SpeechBaseUrl!.TrimEnd('/')
            : "https://" + ResourceName() + ".cognitiveservices.azure.com";
    }

    /// <summary>MAI URL prefix without a trailing slash. Maps to <c>maiBaseURL()</c>.</summary>
    internal string MaiBaseUrl()
    {
        return !string.IsNullOrEmpty(_azure.MaiBaseUrl)
            ? _azure.MaiBaseUrl!.TrimEnd('/')
            : "https://" + ResourceName() + ".services.ai.azure.com/mai/v1";
    }

    /// <summary>
    /// Headers for the Azure Speech API (<c>Ocp-Apim-Subscription-Key</c>) or the MAI API (<c>api-key</c>).
    /// With a token provider, no key header is added; callers add the bearer token. Maps to <c>getHeaders('speech' | 'mai')</c>.
    /// </summary>
    internal async Task<Dictionary<string, string?>> CreateHeadersAsync(
        AzureApi api,
        IReadOnlyDictionary<string, string?>? callHeaders = null,
        CancellationToken cancellationToken = default
    )
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (_azure.TokenProvider == null)
        {
            var key = await ApiKeys
                .RequireAsync(_azure.ApiKey, "AZURE_API_KEY", null, cancellationToken)
                .ConfigureAwait(false);
            headers[api == AzureApi.Speech ? "Ocp-Apim-Subscription-Key" : "api-key"] = key;
        }

        foreach (var pair in _azure.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        headers.TryGetValue("User-Agent", out var current);
        var suffix = "ai-sdk-azure/" + Operations.AiSdkVersion.Version;
        headers["user-agent"] = string.IsNullOrEmpty(current) ? suffix : current + " " + suffix;
        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    /// <summary>
    /// Serializes a model id and the JSON-serializable provider settings of an Azure model for a workflow boundary. The API
    /// key and the token provider are not part of the serialized config. Maps to <c>serializeModelOptions</c> over
    /// <c>{ modelId, config }</c> in <c>[WORKFLOW_SERIALIZE]</c>.
    /// </summary>
    internal static SerializedModelOptions SerializeModel(
        string modelId,
        AzureOpenAIOptions options
    )
    {
        var config = new Dictionary<string, object?>();
        void Add(string name, object? value)
        {
            if (value != null)
            {
                config[name] = value;
            }
        }

        Add("baseURL", options.BaseUrl);
        Add("resourceName", options.ResourceName);
        Add("apiVersion", options.ApiVersion);
        Add("useDeploymentBasedUrls", options.UseDeploymentBasedUrls);
        Add("speechBaseURL", options.SpeechBaseUrl);
        Add("maiBaseURL", options.MaiBaseUrl);
        Add("headers", options.Headers.ToDictionary(pair => pair.Key, pair => (object?)pair.Value));
        return ModelOptionsSerialization.SerializeModelOptions(modelId, config);
    }

    /// <summary>Creates the provider that <c>[WORKFLOW_DESERIALIZE]</c> builds with <c>createAzure(options.config)</c>.</summary>
    internal static AzureOpenAIProvider DeserializeProvider(
        SerializedModelOptions serialized,
        string? apiKey,
        HttpMessageHandler? handler
    )
    {
        if (serialized is null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        var options = new AzureOpenAIOptions();
        if (apiKey is not null)
        {
            options.ApiKey = apiKey;
        }

        string? Text(string name) =>
            serialized.Config.TryGetValue(name, out var value) ? value as string : null;
        options.BaseUrl = Text("baseURL");
        options.ResourceName = Text("resourceName");
        options.SpeechBaseUrl = Text("speechBaseURL");
        options.MaiBaseUrl = Text("maiBaseURL");
        if (Text("apiVersion") is { } apiVersion)
        {
            options.ApiVersion = apiVersion;
        }

        if (
            serialized.Config.TryGetValue("useDeploymentBasedUrls", out var deployment)
            && deployment is bool useDeploymentBasedUrls
        )
        {
            options.UseDeploymentBasedUrls = useDeploymentBasedUrls;
        }

        if (
            serialized.Config.TryGetValue("headers", out var headers)
            && headers is System.Collections.IDictionary map
        )
        {
            foreach (System.Collections.DictionaryEntry entry in map)
            {
                if (entry.Value is string value)
                {
                    options.Headers[(string)entry.Key] = value;
                }
            }
        }

        return Create(options, handler);
    }

    private string ResourceName()
    {
        var resource = !string.IsNullOrEmpty(_azure.ResourceName)
            ? _azure.ResourceName
            : Environment.GetEnvironmentVariable("AZURE_RESOURCE_NAME");
        if (string.IsNullOrEmpty(resource))
        {
            throw new ArgumentException(
                "Azure OpenAI resource name is missing. Pass it in AzureOpenAIOptions.ResourceName or set the AZURE_RESOURCE_NAME environment variable.",
                nameof(AzureOpenAIOptions.ResourceName)
            );
        }

        if (!IsValidHostnamePart(resource))
        {
            throw new ArgumentException(
                "An Azure resource name must be a single DNS label (letters, digits, and hyphens). Custom endpoints belong in BaseUrl.",
                nameof(AzureOpenAIOptions.ResourceName)
            );
        }

        return resource;
    }

    /// <summary>Creates a provider.</summary>
    public static AzureOpenAIProvider Create(
        AzureOpenAIOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        options ??= new AzureOpenAIOptions();
        HttpMessageHandler? transport = handler;
        if (options.TokenProvider != null && options.ApiKey is null)
        {
            transport = new AzureEntraTokenHandler(
                options.TokenProvider,
                handler ?? new GlobalFetchHandler(allowAutoRedirect: true)
            );
        }

        var client = transport is null
            ? new HttpClient(new GlobalFetchHandler(allowAutoRedirect: true))
            : new HttpClient(transport, disposeHandler: false);

        // Downloads skip the token-injecting handler. A caller-supplied handler stays the transport, like a custom `fetch`;
        // without one, the shared client that never follows a redirect is used.
        var downloadClient = handler is null
            ? null
            : new HttpClient(handler, disposeHandler: false);
        return new AzureOpenAIProvider(client, options, downloadClient);
    }

    /// <summary>
    /// Responses API model, like <see cref="ResponsesModel(string)"/>. The optional model ID in
    /// <paramref name="settings"/> is used for reported identity and capability detection.
    /// </summary>
    public OpenAI.OpenAIResponsesLanguageModel LanguageModel(
        string deploymentId,
        AzureOpenAILanguageModelSettings? settings
    )
    {
        return new OpenAI.OpenAIResponsesLanguageModel(this, deploymentId, settings);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return ResponsesModel(modelId);
    }

    /// <summary>
    /// Responses API model. The provider id is <c>azure.responses</c>.
    /// Options are read from the <c>azure</c> provider options and fall back to <c>openai</c>.
    /// </summary>
    public OpenAI.OpenAIResponsesLanguageModel ResponsesModel(string modelId)
    {
        return new OpenAI.OpenAIResponsesLanguageModel(this, modelId);
    }

    /// <summary>
    /// Responses API model for a deployment. The optional model ID is used for reported identity and
    /// capability detection; requests always use the deployment ID.
    /// </summary>
    public OpenAI.OpenAIResponsesLanguageModel ResponsesModel(
        string deploymentId,
        AzureOpenAILanguageModelSettings? settings
    )
    {
        return new OpenAI.OpenAIResponsesLanguageModel(this, deploymentId, settings);
    }

    /// <summary>
    /// Chat model for a deployment. The optional model ID is used for reported identity and capability
    /// detection; requests always use the deployment ID.
    /// </summary>
    public OpenAI.OpenAIChatLanguageModel ChatModel(
        string deploymentId,
        AzureOpenAILanguageModelSettings? settings
    )
    {
        return new OpenAI.OpenAIChatLanguageModel(this, deploymentId, settings);
    }

    /// <summary>
    /// Azure-hosted DeepSeek chat model for a deployment. Images are sent as URLs when they are <c>http</c> or
    /// <c>https</c>. The provider id is <c>azure.deepseek</c>. Maps to <c>deepseek</c> (<c>createDeepSeekModel</c>).
    /// </summary>
    public DeepSeek.DeepSeekChatLanguageModel DeepSeek(string deploymentName)
    {
        return new DeepSeek.DeepSeekChatLanguageModel(
            deploymentName,
            new DeepSeek.DeepSeekChatConfig(ProviderId + ".deepseek", this, (_, _) =>
                ChatUri(deploymentName)
            )
            {
                SupportsPenaltySampling = true,
                SupportsThinking = false,
                // json_object with thinking enabled makes Azure return the JSON in reasoning_content with empty content
                SupportsStructuredOutputs = true,
            }
        );
    }

    /// <summary>
    /// Completion model for a deployment. The optional model ID is used for reported identity;
    /// requests always use the deployment ID.
    /// </summary>
    public OpenAICompatibleCompletionLanguageModel CompletionModel(
        string deploymentId,
        AzureOpenAILanguageModelSettings? settings
    )
    {
        return new OpenAICompatibleCompletionLanguageModel(
            this,
            settings?.ModelId ?? deploymentId,
            null,
            deploymentId
        );
    }

    /// <summary>
    /// Embedding model. Azure OpenAI embeddings accept up to 300,000 input bytes in one call, as
    /// <see cref="OpenAI.OpenAIEmbeddingModel"/> does. Maps to <c>embedding</c>.
    /// </summary>
    public override IEmbeddingModel EmbeddingModel(string deploymentId)
    {
        return new OpenAICompatibleEmbeddingModel(this, deploymentId)
        {
            MaxInputBytesPerCall = OpenAI.OpenAIEmbeddingModel.DefaultMaxInputBytesPerCall,
        };
    }

    /// <summary>
    /// Image model. MAI-Image models use the MAI image API by default; other IDs use OpenAI. Override with
    /// <c>providerOptions.azure.api</c>. Maps to <c>image</c>.
    /// </summary>
    public AzureImageModel Image(string deploymentId)
    {
        return new AzureImageModel(this, deploymentId);
    }

    /// <summary>Image model. Alias of <see cref="Image(string)"/>. Maps to <c>imageModel</c>.</summary>
    public override IImageModel ImageModel(string deploymentId)
    {
        return Image(deploymentId);
    }

    /// <summary>
    /// Speech model. MAI-Voice models use the Azure Speech API and every other model uses the Azure OpenAI speech API;
    /// <c>providerOptions.azure.api</c> overrides that for a call. The provider id is <c>azure.speech</c>.
    /// </summary>
    public AzureSpeechModel Speech(string deploymentId)
    {
        return new AzureSpeechModel(
            deploymentId,
            _azure,
            new OpenAI.OpenAISpeechModel(this, deploymentId),
            new AzureSpeechSpeechModel(
                deploymentId,
                Http,
                () => new Uri(SpeechBaseUrl() + "/tts/cognitiveservices/v1"),
                cancellationToken => CreateHeadersAsync(AzureApi.Speech, null, cancellationToken)
            )
        );
    }

    /// <summary>Speech model. Alias of <see cref="Speech(string)"/>.</summary>
    public override ISpeechModel SpeechModel(string modelId)
    {
        return Speech(modelId);
    }

    /// <summary>
    /// Transcription model. MAI-Transcribe models use the Speech API and MAI-Transcribe-2-Streaming the MAI realtime API
    /// by default; other deployment names use the Azure OpenAI transcription API. Maps to <c>transcription</c>.
    /// </summary>
    public AzureTranscriptionModel Transcription(string deploymentId)
    {
        return new AzureTranscriptionModel(
            deploymentId,
            _azure,
            new OpenAI.OpenAITranscriptionModel(
                this,
                deploymentId,
                webSocket: _azure.NativeWebSocket
            ),
            new AzureSpeechTranscriptionModel(
                deploymentId,
                Http,
                () =>
                    new Uri(
                        SpeechBaseUrl()
                            + "/speechtotext/transcriptions:transcribe?api-version=2025-10-15"
                    ),
                (callHeaders, cancellationToken) =>
                    CreateHeadersAsync(AzureApi.Speech, callHeaders, cancellationToken)
            ),
            new AzureMaiTranscriptionModel(
                deploymentId,
                () => new Uri(MaiBaseUrl() + "/realtime?intent=transcription"),
                async cancellationToken =>
                {
                    var headers = await CreateHeadersAsync(AzureApi.Mai, null, cancellationToken)
                        .ConfigureAwait(false);
                    if (_azure.TokenProvider == null)
                    {
                        return headers;
                    }

                    var withToken = new Dictionary<string, string?>(
                        StringComparer.OrdinalIgnoreCase
                    )
                    {
                        ["authorization"] = "Bearer " + _azure.TokenProvider(),
                    };
                    foreach (var pair in headers)
                    {
                        withToken[pair.Key] = pair.Value;
                    }

                    return withToken;
                },
                _azure.WebSocket,
                _azure.NativeWebSocket
            )
        );
    }

    /// <summary>Transcription model. Alias of <see cref="Transcription(string)"/>. Maps to <c>transcriptionModel</c>.</summary>
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return Transcription(modelId);
    }

    private static OpenAICompatibleOptions Prepare(
        AzureOpenAIOptions? options,
        HttpClient? downloadClient
    )
    {
        options ??= new AzureOpenAIOptions();
        if (options.ApiKey is not null && options.TokenProvider != null)
        {
            throw new ArgumentException(
                "Both apiKey and tokenProvider were provided. Please use only one authentication method."
            );
        }

        var baseUrl = ResolveBaseUrl(options);
        var prepared = new OpenAICompatibleOptions
        {
            ProviderName = ProviderId,
            BaseUrl = baseUrl,
            ApiKey =
                options.TokenProvider == null
                    ? options.ApiKey
                    : AzureEntraTokenHandler.PlaceholderToken,
            ApiKeyEnvironmentVariable = "AZURE_API_KEY",
            ApiKeyStyle =
                options.TokenProvider == null ? ApiKeyStyle.ApiKeyHeader : ApiKeyStyle.Bearer,
            ApiKeyHeaderName = "api-key",
            AzureApiVersion = string.IsNullOrEmpty(options.ApiVersion) ? "v1" : options.ApiVersion,
            AzureUseDeploymentBasedUrls = options.UseDeploymentBasedUrls,
            SupportsEmbeddings = true,
            SupportsImages = false,
            DownloadHttpClient = downloadClient,
            UserAgent = "ai-sdk-azure/" + Operations.AiSdkVersion.Version,
        };
        foreach (var pair in options.Headers)
        {
            prepared.Headers[pair.Key] = pair.Value;
        }

        return prepared;
    }

    private static string ResolveBaseUrl(AzureOpenAIOptions options)
    {
        if (!string.IsNullOrEmpty(options.BaseUrl))
        {
            return options.BaseUrl;
        }

        // ResourceName is concatenated into the host. A value such as
        // user@internal:8080/# would send the request somewhere else.
        var resource = options.ResourceName
            ?? Environment.GetEnvironmentVariable("AZURE_RESOURCE_NAME")
            ?? "resource";
        if (!IsValidHostnamePart(resource))
        {
            throw new ArgumentException(
                "An Azure resource name must be a single DNS label (letters, digits, and hyphens). Custom endpoints belong in BaseUrl.",
                nameof(options.ResourceName)
            );
        }

        return "https://" + resource + ".openai.azure.com/openai";
    }

    /// <summary>
    /// Classifies a base URL. A missing URL is Azure OpenAI. Maps to <c>getAzureOpenAIBaseURLInfo</c>.
    /// </summary>
    internal static AzureOpenAIBaseUrlInfo GetAzureOpenAIBaseURLInfo(string? baseUrl)
    {
        if (baseUrl is null)
        {
            return new AzureOpenAIBaseUrlInfo(true, false, false);
        }

        var url = new Uri(baseUrl, UriKind.Absolute);
        var hostname = url.Host;
        var isAzureOpenAI =
            hostname.EndsWith(".openai.azure.com", StringComparison.Ordinal)
            || hostname.EndsWith(".services.ai.azure.com", StringComparison.Ordinal)
            || hostname.EndsWith(".cognitiveservices.azure.com", StringComparison.Ordinal);
        var pathname = url.AbsolutePath.TrimEnd('/');

        return new AzureOpenAIBaseUrlInfo(
            isAzureOpenAI,
            hostname.EndsWith(".services.ai.azure.com", StringComparison.Ordinal)
                && pathname.StartsWith("/api/projects/", StringComparison.Ordinal),
            isAzureOpenAI && pathname.EndsWith("/openai/v1", StringComparison.OrdinalIgnoreCase)
        );
    }

    private static bool IsValidHostnamePart(string value)
    {
        var match = HostnamePart.Match(value);
        return match.Success && match.Value == value;
    }
}

/// <summary>The result of <c>getAzureOpenAIBaseURLInfo</c>.</summary>
internal readonly record struct AzureOpenAIBaseUrlInfo(
    bool IsAzureOpenAI,
    bool IsFoundryProject,
    bool IsVersioned
);

/// <summary>The Azure API a request goes to.</summary>
internal enum AzureApi
{
    /// <summary>Azure OpenAI.</summary>
    OpenAI,

    /// <summary>Azure Speech (MAI-Voice, MAI-Transcribe).</summary>
    Speech,

    /// <summary>MAI (MAI-Image, MAI-Transcribe-2-Streaming).</summary>
    Mai,
}

/// <summary>Replaces the placeholder bearer value with a token from the provider on every request.</summary>
internal sealed class AzureEntraTokenHandler : DelegatingHandler
{
    internal const string PlaceholderToken = "azure-ad";

    private readonly Func<string> _tokenProvider;

    /// <summary>Creates a handler that calls <paramref name="tokenProvider"/> for each request.</summary>
    public AzureEntraTokenHandler(Func<string> tokenProvider, HttpMessageHandler inner)
        : base(inner)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Remove("api-key");
        // A bearer token the caller supplied stays. Only the placeholder or a missing header gets a fresh token.
        if (
            request.Headers.Authorization is null
            || request.Headers.Authorization is { Scheme: "Bearer", Parameter: PlaceholderToken }
        )
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _tokenProvider()
            );
        }

        return base.SendAsync(request, cancellationToken);
    }
}

/// <summary>Registers Azure OpenAI.</summary>
public static class AzureOpenAIServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AzureOpenAIProvider"/>.</summary>
    public static IServiceCollection AddAzureOpenAI(
        this IServiceCollection services,
        Action<AzureOpenAIOptions>? configure = null
    )
    {
        services.AddHttpClient(AzureOpenAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new AzureOpenAIOptions();
            configure?.Invoke(options);
            if (options.TokenProvider != null)
            {
                return AzureOpenAIProvider.Create(options);
            }

            return new AzureOpenAIProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(AzureOpenAIProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
