// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI provider settings.</summary>
public sealed class OpenAIOptions : OpenAICompatibleOptions
{
    private bool _baseUrlAssigned;

    /// <summary>Settings for downloading JSON Lines batch results.</summary>
    public BatchResultDownloads? BatchResultDownloads { get; set; }

    /// <summary>Creates options aimed at <c>https://api.openai.com/v1</c> unless <c>OPENAI_BASE_URL</c> is set.</summary>
    public OpenAIOptions()
    {
        ProviderName = "openai";
        ApiKeyEnvironmentVariable = "OPENAI_API_KEY";
        SupportsEmbeddings = true;
        SupportsImages = true;
    }

    /// <summary>
    /// API origin. An assigned value wins over <c>OPENAI_BASE_URL</c>.
    /// A blank value is rejected when the provider is created.
    /// </summary>
    public new string BaseUrl
    {
        get => base.BaseUrl;
        set
        {
            _baseUrlAssigned = true;
            base.BaseUrl = value;
        }
    }

    /// <summary>True when the caller assigned <see cref="BaseUrl"/>.</summary>
    internal bool BaseUrlAssigned => _baseUrlAssigned;

    /// <summary>Sets the resolved origin without marking it as an explicit option.</summary>
    internal void UseResolvedBaseUrl(string value)
    {
        base.BaseUrl = value;
    }

    /// <summary>When true, <see cref="OpenAIProvider.LanguageModel"/> uses the Responses API.</summary>
    public bool UseResponsesApi { get; set; }

    /// <summary>Value of the <c>OpenAI-Organization</c> header.</summary>
    public string? Organization { get; set; }

    /// <summary>Value of the <c>OpenAI-Project</c> header.</summary>
    public string? Project { get; set; }
}

/// <summary>Function tools prepared for the OpenAI Responses API.</summary>
public sealed class PreparedResponsesTools
{
    /// <summary>Creates a prepared tool list.</summary>
    public PreparedResponsesTools(JsonArray? tools, ToolChoice? toolChoice, IReadOnlyList<CallWarning> toolWarnings)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        ToolWarnings = toolWarnings ?? Array.Empty<CallWarning>();
    }

    /// <summary>Provider tool objects. Null when the request has no tools.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Tool choice passed through for this request. Null when unset.</summary>
    public ToolChoice? ToolChoice { get; }

    /// <summary>Warnings produced while preparing tools.</summary>
    public IReadOnlyList<CallWarning> ToolWarnings { get; }
}

/// <summary>OpenAI provider: Chat Completions, Responses, embeddings, images, speech, transcription, files, and batches.</summary>
public sealed class OpenAIProvider : OpenAICompatibleProvider, Operations.IBatchProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "openai";

    /// <summary>User-Agent suffix appended to every request. Tracks <c>@ai-sdk/openai</c> 4.0.73.</summary>
    public const string UserAgentSuffix = "ai-sdk/openai/4.0.73";

    private readonly OpenAIOptions _openAI;

    /// <summary>Creates an OpenAI provider.</summary>
    public OpenAIProvider(HttpClient httpClient, OpenAIOptions? options = null)
        : base(options ?? new OpenAIOptions(), httpClient)
    {
        _openAI = (OpenAIOptions)Options;
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ResolveBaseUrl(_openAI);
    }

    /// <summary>HTTP client used for calls that need response headers.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static OpenAIProvider Create(OpenAIOptions? options = null, HttpMessageHandler? handler = null)
    {
        options ??= new OpenAIOptions();
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new OpenAIProvider(client, options);
    }

    /// <summary>Chat Completions model. The provider id is <c>openai.chat</c>.</summary>
    public OpenAIChatLanguageModel ChatModel(string modelId)
    {
        return new OpenAIChatLanguageModel(this, modelId);
    }

    /// <summary>Responses API model. The provider id is <c>openai.responses</c>.</summary>
    public OpenAIResponsesLanguageModel ResponsesModel(string modelId)
    {
        return new OpenAIResponsesLanguageModel(this, modelId);
    }

    /// <summary>Legacy completions model. The provider id is <c>openai.completion</c>.</summary>
    public new OpenAICompletionLanguageModel CompletionModel(string modelId)
    {
        return new OpenAICompletionLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        if (_openAI.UseResponsesApi)
        {
            return ResponsesModel(modelId);
        }

        return ChatModel(modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new OpenAIEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new OpenAIImageModel(this, modelId);
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return new OpenAISpeechModel(this, modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new OpenAITranscriptionModel(this, modelId);
    }

    /// <inheritdoc />
    public override ISpeechTranslationModel SpeechTranslationModel(string modelId)
    {
        return new OpenAISpeechTranslationModel(this, modelId);
    }

    /// <summary>Batch API. Downloads batch results.</summary>
    public Operations.IBatchApi? ExperimentalBatch()
    {
        return new OpenAIBatchApi(this);
    }

    internal int? MaxBatchLineBytes => _openAI.BatchResultDownloads?.MaxLineBytes;

    /// <inheritdoc />
    public override IBatchModel BatchModel()
    {
        return new OpenAIBatchModel(this);
    }

    /// <inheritdoc />
    public override IFileStore FileStore()
    {
        return new OpenAIFileStore(this);
    }

    /// <inheritdoc />
    public override ISkillStore SkillStore()
    {
        return new OpenAISkillStore(this);
    }

    /// <inheritdoc />
    public override IRealtimeModel RealtimeModel(string modelId)
    {
        return new OpenAIRealtimeModel(this, modelId);
    }

    /// <summary>
    /// Builds request headers. Custom provider headers override the API key, organization, and project.
    /// The user-agent suffix is appended after that, and per-call headers override the result.
    /// </summary>
    public Dictionary<string, string?> CreateOpenAIHeaders(IReadOnlyDictionary<string, string?>? callHeaders = null)
    {
        var key = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable, Options.AdditionalApiKeyEnvironmentVariables);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = "Bearer " + key,
        };
        if (!string.IsNullOrEmpty(_openAI.Organization))
        {
            headers["OpenAI-Organization"] = _openAI.Organization;
        }

        if (!string.IsNullOrEmpty(_openAI.Project))
        {
            headers["OpenAI-Project"] = _openAI.Project;
        }

        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        headers.TryGetValue("User-Agent", out var current);
        headers["user-agent"] = string.IsNullOrEmpty(current) ? UserAgentSuffix : current + " " + UserAgentSuffix;
        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private static void ResolveBaseUrl(OpenAIOptions options)
    {
        string? candidate;
        if (options.BaseUrlAssigned)
        {
            candidate = options.BaseUrl;
        }
        else
        {
            candidate = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
            if (candidate == null)
            {
                return;
            }
        }

        if (candidate.Trim().Length == 0)
        {
            throw new ArgumentException("baseURL must be a non-empty string.", "baseURL");
        }

        options.UseResolvedBaseUrl(StripOneTrailingSlash(candidate));
    }

    private static string StripOneTrailingSlash(string value)
    {
        if (value.Length > 0 && value[value.Length - 1] == '/')
        {
            return value.Substring(0, value.Length - 1);
        }

        return value;
    }
}

/// <summary>A realtime client secret.</summary>
public sealed class OpenAIRealtimeClientSecret
{
    internal OpenAIRealtimeClientSecret(string token, string url, long? expiresAt)
    {
        Token = token;
        Url = url;
        ExpiresAt = expiresAt;
    }

    /// <summary>Client secret value.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL that includes the model.</summary>
    public string Url { get; }

    /// <summary>Expiry as unix seconds, when the provider sent one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>OpenAI Realtime session URI builder and WebSocket client.</summary>
public sealed class OpenAIRealtimeModel : IRealtimeModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a realtime model.</summary>
    public OpenAIRealtimeModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        var http = ApiKeys.Combine(_provider.Options.BaseUrl, "realtime");
        var builder = new UriBuilder(http) { Scheme = http.Scheme == "https" ? "wss" : "ws", Query = "model=" + Uri.EscapeDataString(ModelId) };
        return builder.Uri;
    }

    /// <summary>Requests a realtime client secret. <paramref name="expiresAfterSeconds"/> is omitted when null.</summary>
    public async Task<OpenAIRealtimeClientSecret> CreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["session"] = new JsonObject
            {
                ["type"] = "realtime",
                ["model"] = ModelId,
            },
        };
        if (expiresAfterSeconds != null)
        {
            body["expires_after"] = new JsonObject
            {
                ["anchor"] = "created_at",
                ["seconds"] = expiresAfterSeconds.Value,
            };
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "realtime/client_secrets"),
            body.ToJsonString(),
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var token = document.RootElement.TryGetProperty("value", out var value) ? value.GetString() ?? string.Empty : string.Empty;
        long? expires = null;
        if (document.RootElement.TryGetProperty("expires_at", out var expiresElement) && expiresElement.TryGetInt64(out var seconds))
        {
            expires = seconds;
        }

        var host = new Uri(_provider.Options.BaseUrl).Host;
        var url = "wss://" + host + "/v1/realtime?model=" + Uri.EscapeDataString(ModelId);
        return new OpenAIRealtimeClientSecret(token, url, expires);
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        throw new PlatformNotSupportedException("OpenAI Realtime WebSocket headers require .NET 10.");
#else
        return ConnectCoreAsync(cancellationToken);
#endif
    }

#if !NETSTANDARD2_0
    private async Task<IRealtimeSession> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        var socket = new System.Net.WebSockets.ClientWebSocket();
        var headers = _provider.CreateOpenAIHeaders();
        socket.Options.SetRequestHeader("Authorization", headers["Authorization"] ?? string.Empty);
        socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
        await socket.ConnectAsync(BuildUri(), cancellationToken).ConfigureAwait(false);
        return new WebSocketRealtimeSession(socket);
    }
#endif
}

internal sealed class OpenAIBatchModel : IBatchModel
{
    private readonly OpenAIProvider _provider;

    public OpenAIBatchModel(OpenAIProvider provider)
    {
        _provider = provider;
    }

    public string Provider => _provider.Name + ".batch";

    public async Task<BatchJob> SubmitAsync(string inputFileId, string endpoint, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["input_file_id"] = inputFileId,
            ["endpoint"] = endpoint,
            ["completion_window"] = "24h",
        };
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "batches"),
            body.ToJsonString(),
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        return Read(document.RootElement);
    }

    public async Task<BatchJob> GetAsync(string id, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Get,
            ApiKeys.Combine(_provider.Options.BaseUrl, "batches/" + id),
            null,
            _provider.CreateOpenAIHeaders(),
            cancellationToken).ConfigureAwait(false);
        return Read(document.RootElement);
    }

    private static BatchJob Read(JsonElement root)
    {
        return new BatchJob(
            root.GetProperty("id").GetString() ?? string.Empty,
            root.TryGetProperty("status", out var status) ? status.GetString() ?? "unknown" : "unknown");
    }
}

internal sealed class WebSocketRealtimeSession : IRealtimeSession
{
    private readonly System.Net.WebSockets.ClientWebSocket _socket;

    public WebSocketRealtimeSession(System.Net.WebSockets.ClientWebSocket socket)
    {
        _socket = socket;
    }

    public async IAsyncEnumerable<string> Events([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new byte[8192];
        while (_socket.State == System.Net.WebSockets.WebSocketState.Open)
        {
            var builder = new StringBuilder();
            System.Net.WebSockets.WebSocketReceiveResult result;
            do
            {
                result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    yield break;
                }

                builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            yield return builder.ToString();
        }
    }

    IAsyncEnumerable<string> IRealtimeSession.Events => Events();

    public Task SendAsync(string json, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        return _socket.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _socket.Dispose();
        return default;
    }
}

/// <summary>Registers <see cref="OpenAIProvider"/>.</summary>
public static class OpenAIServiceCollectionExtensions
{
    /// <summary>Adds the OpenAI provider.</summary>
    public static IServiceCollection AddOpenAI(this IServiceCollection services, Action<OpenAIOptions>? configure = null)
    {
        services.AddHttpClient(OpenAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAIOptions();
            configure?.Invoke(options);
            return new OpenAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(OpenAIProvider.ProviderId), options);
        });
        return services;
    }
}
