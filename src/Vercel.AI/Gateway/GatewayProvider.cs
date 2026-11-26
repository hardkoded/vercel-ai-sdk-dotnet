// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Gateway;

/// <summary>AI Gateway settings. The default base URL is <c>https://ai-gateway.vercel.sh/v4/ai</c>.</summary>
public sealed class GatewayOptions
{
    /// <summary>Settings for downloading JSON Lines batch results.</summary>
    public BatchResultDownloads? BatchResultDownloads { get; set; }

    /// <summary>Gateway origin for the V4 routes.</summary>
    public string BaseUrl { get; set; } = "https://ai-gateway.vercel.sh/v4/ai";

    /// <summary>
    /// Explicit key. Falls back to <c>AI_GATEWAY_API_KEY</c>. A callback is resolved before each request.
    /// </summary>
    public Resolvable<string>? ApiKey { get; set; }

    /// <summary>
    /// Observability headers sent on every request. When null, they come from <c>VERCEL_DEPLOYMENT_ID</c>
    /// (<c>ai-o11y-deployment-id</c>), <c>VERCEL_ENV</c> (<c>ai-o11y-environment</c>), <c>VERCEL_REGION</c>
    /// (<c>ai-o11y-region</c>), the <c>x-vercel-id</c> header of the <see cref="VercelRequestContext"/> (<c>ai-o11y-request-id</c>),
    /// and <c>VERCEL_PROJECT_ID</c> (<c>ai-o11y-project-id</c>).
    /// </summary>
    public IReadOnlyDictionary<string, string>? O11yHeaders { get; set; }

    /// <summary>Vercel team ID or slug sent as <c>x-vercel-ai-gateway-team</c> and carried in realtime WebSocket subprotocols.</summary>
    public string? TeamIdOrSlug { get; set; }

    /// <summary>Custom headers merged into the auth headers of realtime requests.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>How frequently to refresh the metadata cache of <see cref="GatewayProvider.GetAvailableModelsAsync"/>, in milliseconds. Defaults to five minutes.</summary>
    public int? MetadataCacheRefreshMillis { get; set; }

    /// <summary>Custom WebSocket implementation for streaming transcription. Defaults to <c>ClientWebSocket</c>. Maps to <c>webSocket</c>.</summary>
    public WebSocketFactory? WebSocket { get; set; }

    /// <summary>Maps to <c>_internal.currentDate</c>. For testing purposes only.</summary>
    internal Func<DateTimeOffset>? CurrentDate { get; set; }
}

/// <summary>
/// Vercel AI Gateway provider. Language, embedding, image, and video calls use the Gateway V4 routes
/// (<c>/language-model</c>, <c>/embedding-model</c>, <c>/image-model</c>, <c>/video-model</c>).
/// </summary>
#pragma warning disable CS0618
public sealed class GatewayProvider
    : ProviderBase,
        Operations.IBatchProvider,
        Operations.IDecisionProvider,
        Operations.IExtractionProvider,
        Operations.IEvaluationProvider
#pragma warning restore CS0618
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "gateway";

    /// <summary>Environment variable for the Gateway key.</summary>
    public const string ApiKeyEnvironmentVariable = "AI_GATEWAY_API_KEY";

    private readonly object _metadataLock = new();
    private Task<GatewayFetchMetadataResponse>? _pendingMetadata;
    private GatewayFetchMetadataResponse? _metadataCache;
    private long _lastFetchTime;

    /// <summary>Creates a Gateway provider.</summary>
    public GatewayProvider(GatewayOptions? options, HttpClient httpClient)
        : base(ProviderId)
    {
        Options = options ?? new GatewayOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public GatewayOptions Options { get; }

    /// <summary>Gateway batch API. Downloads batch results.</summary>
    public Operations.IBatchApi? ExperimentalBatch()
    {
        return new GatewayBatchApi(this);
    }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static GatewayProvider Create(
        GatewayOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new GatewayProvider(options, client);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new GatewayLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new GatewayEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new GatewayImageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId)
    {
        return new GatewayVideoModel(this, modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GatewayTranscriptionModel(this, modelId);
    }

    /// <summary>
    /// Returns a realtime model. Maps to <c>experimental_realtime(modelId)</c>. Building the model touches no credential;
    /// only minting a client secret and server connections resolve the Gateway credential. Known Live model ids return a
    /// <see cref="GatewayRealtimeModelLive"/>.
    /// </summary>
    public override IRealtimeModel RealtimeModel(string modelId)
    {
        return CreateRealtimeModel(modelId, null);
    }

    /// <summary>
    /// Returns a turn-based realtime model or, for Live model ids or <see cref="GatewayRealtimeApi.Live"/>, a continuous
    /// <see cref="GatewayRealtimeModelLive"/>. Maps to <c>experimental_realtime(modelId, options)</c>.
    /// </summary>
    public IRealtimeModel ExperimentalRealtime(string modelId, GatewayRealtimeOptions? options)
    {
        return CreateRealtimeModel(modelId, options);
    }

    /// <summary>
    /// Mints a client secret for a realtime model and returns it with the WebSocket URL. A Live model gets a Live-bound
    /// secret (<c>routeKind: 'live'</c>); realtime mints omit <c>routeKind</c>. Maps to
    /// <c>experimental_realtime.getToken</c>.
    /// </summary>
    public async Task<GatewayRealtimeClientSecret> ExperimentalRealtimeGetTokenAsync(
        string model,
        int? expiresAfterSeconds = null,
        CancellationToken cancellationToken = default,
        GatewayRealtimeOptions? options = null
    )
    {
        return CreateRealtimeModel(model, options) switch
        {
            GatewayRealtimeModelLive live => await live.DoCreateClientSecretAsync(
                    expiresAfterSeconds,
                    cancellationToken
                )
                .ConfigureAwait(false),
            var realtime => await ((GatewayRealtimeModel)realtime)
                .DoCreateClientSecretAsync(expiresAfterSeconds, cancellationToken)
                .ConfigureAwait(false),
        };
    }

    /// <summary>
    /// Returns a streaming transcription model. Maps to <c>experimental_transcription(modelId)</c>.
    /// </summary>
    public GatewayTranscriptionModel ExperimentalTranscription(string modelId)
    {
        return new GatewayTranscriptionModel(this, modelId);
    }

    /// <summary>
    /// Mints a transcription-bound short-lived client secret (<c>vcst_</c>) a browser can use to open the streaming
    /// transcription WebSocket without holding the Gateway credential. Server-side only. Maps to
    /// <c>experimental_transcription.getToken</c>.
    /// </summary>
    public async Task<GatewayTranscriptionFactoryGetTokenResult> ExperimentalTranscriptionGetTokenAsync(
        string model,
        int? expiresAfterSeconds = null,
        CancellationToken cancellationToken = default
    )
    {
        var secret = await MintClientSecretAsync(
                model,
                expiresAfterSeconds,
                "transcription",
                cancellationToken
            )
            .ConfigureAwait(false);
        return new GatewayTranscriptionFactoryGetTokenResult(
            secret.Token,
            GatewayTranscriptionModel.ToGatewayTranscriptionUrl(Options.BaseUrl, model),
            secret.ExpiresAt
        );
    }

    /// <summary>
    /// Mints a speech-bound short-lived client secret (<c>vcst_</c>) a browser can use to open the streaming speech
    /// WebSocket without holding the Gateway credential. Server-side only. Maps to <c>experimental_speech.getToken</c>.
    /// </summary>
    public async Task<GatewaySpeechFactoryGetTokenResult> ExperimentalSpeechGetTokenAsync(
        string model,
        int? expiresAfterSeconds = null,
        CancellationToken cancellationToken = default
    )
    {
        var secret = await MintClientSecretAsync(
                model,
                expiresAfterSeconds,
                "speech",
                cancellationToken
            )
            .ConfigureAwait(false);
        return new GatewaySpeechFactoryGetTokenResult(
            secret.Token,
            GatewaySpeechModel.ToGatewaySpeechUrl(Options.BaseUrl, model),
            secret.ExpiresAt
        );
    }

    /// <summary>Decision model routed through <c>/decision-model</c>.</summary>
    public GatewayDecisionModel DecisionModel(string modelId)
    {
        return new GatewayDecisionModel(this, modelId);
    }

    /// <inheritdoc />
    Operations.IDecisionCaller Operations.IDecisionProvider.DecisionModel(string modelId)
    {
        return DecisionModel(modelId);
    }

    /// <summary>Deprecated alias of <see cref="DecisionModel"/>. Maps to <c>evaluationModel</c>.</summary>
    [Obsolete("Use DecisionModel instead.")]
    public GatewayDecisionModel EvaluationModel(string modelId)
    {
        return DecisionModel(modelId);
    }

#pragma warning disable CS0618
    Operations.IEvaluationCaller? Operations.IEvaluationProvider.EvaluationModel(string modelId)
    {
        return DecisionModel(modelId);
    }
#pragma warning restore CS0618

    /// <summary>Reranking model routed through <c>/reranking-model</c>.</summary>
    public override IRerankingModel RerankingModel(string modelId)
    {
        return new GatewayRerankingModel(this, modelId);
    }

    /// <summary>Alias of <see cref="RerankingModel"/>. Maps to <c>reranking</c>. Returns the concrete model so it can be passed to <c>RerankRequest.Model</c>.</summary>
    public GatewayRerankingModel Reranking(string modelId)
    {
        return new GatewayRerankingModel(this, modelId);
    }

    /// <summary>Extraction model routed through <c>/extraction-model</c>. Maps to <c>extractionModel</c>.</summary>
    public GatewayExtractionModel ExtractionModel(string modelId)
    {
        return new GatewayExtractionModel(this, modelId);
    }

    /// <summary>Alias of <see cref="ExtractionModel"/>. Maps to <c>extraction</c>.</summary>
    public GatewayExtractionModel Extraction(string modelId)
    {
        return ExtractionModel(modelId);
    }

    /// <inheritdoc />
    Operations.IExtractionCaller Operations.IExtractionProvider.ExtractionModel(string modelId)
    {
        return ExtractionModel(modelId);
    }

    internal IEnumerable<KeyValuePair<string, string>> O11yHeaders()
    {
        if (Options.O11yHeaders != null)
        {
            return Options.O11yHeaders;
        }

        var headers = new List<KeyValuePair<string, string>>();
        foreach (
            var (name, variable) in new[]
            {
                ("ai-o11y-deployment-id", "VERCEL_DEPLOYMENT_ID"),
                ("ai-o11y-environment", "VERCEL_ENV"),
                ("ai-o11y-region", "VERCEL_REGION"),
                ("ai-o11y-request-id", null),
                ("ai-o11y-project-id", "VERCEL_PROJECT_ID"),
            }
        )
        {
            var value = variable is null
                ? VercelEnvironment.GetVercelRequestId()
                : Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrEmpty(value))
            {
                headers.Add(new KeyValuePair<string, string>(name, value!));
            }
        }

        return headers;
    }

    internal async Task<Dictionary<string, string?>> HeadersAsync(
        string specificationHeader,
        string specificationValue,
        string modelHeader,
        string modelId,
        bool? streaming,
        IReadOnlyDictionary<string, string?>? extra,
        CancellationToken cancellationToken
    )
    {
        var auth = await GetRealtimeAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in CreateAuthHeaders(auth))
        {
            headers[pair.Key] = pair.Value;
        }

        // Upstream applies the model headers after the caller headers, so a caller cannot override them.
        if (extra != null)
        {
            foreach (var pair in extra)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        headers[specificationHeader] = specificationValue;
        headers[modelHeader] = modelId;
        if (streaming is { } value)
        {
            headers["ai-language-model-streaming"] = value ? "true" : "false";
        }

        foreach (var pair in O11yHeaders())
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }

    internal async Task<Dictionary<string, string?>> BatchHeadersAsync(
        IReadOnlyDictionary<string, string?>? extra,
        CancellationToken cancellationToken
    )
    {
        var auth = await GetRealtimeAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in CreateAuthHeaders(auth))
        {
            headers[pair.Key] = pair.Value;
        }

        if (extra != null)
        {
            foreach (var pair in extra)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        foreach (var pair in O11yHeaders())
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }

    /// <summary>
    /// Gets the models the Gateway offers (upstream <c>getAvailableModels</c>). The result is cached for
    /// <see cref="GatewayOptions.MetadataCacheRefreshMillis"/>. A failed fetch is not cached.
    /// </summary>
    public Task<GatewayFetchMetadataResponse> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        Task<GatewayFetchMetadataResponse> result;
        lock (_metadataLock)
        {
            var now = (
                Options.CurrentDate?.Invoke() ?? DateTimeOffset.UtcNow
            ).ToUnixTimeMilliseconds();
            var refreshMillis = Options.MetadataCacheRefreshMillis ?? 1000 * 60 * 5;
            if (_pendingMetadata == null || now - _lastFetchTime > refreshMillis)
            {
                _lastFetchTime = now;
                var fetch = FetchMetadataAsync();
                _pendingMetadata = fetch;

                // Do not keep a failed fetch: the next call fetches again. This also observes the fault.
                _ = fetch.ContinueWith(
                    failed =>
                    {
                        lock (_metadataLock)
                        {
                            if (ReferenceEquals(_pendingMetadata, failed))
                            {
                                _pendingMetadata = null;
                            }
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.Default
                );
            }

            result = _metadataCache != null ? Task.FromResult(_metadataCache) : _pendingMetadata;
        }

        return WaitAsync(result, cancellationToken);
    }

    /// <summary>Gets the credit balance of the account (upstream <c>getCredits</c>).</summary>
    public Task<GatewayCreditsResponse> GetCreditsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return WithGatewayErrorAsync(config =>
            new GatewayFetchMetadata(config).GetCreditsAsync(cancellationToken)
        );
    }

    /// <summary>Gets the spend report of the account (upstream <c>getSpendReport</c>).</summary>
    public Task<GatewaySpendReportResponse> GetSpendReportAsync(
        GatewaySpendReportParams parameters,
        CancellationToken cancellationToken = default
    )
    {
        return WithGatewayErrorAsync(config =>
            new GatewaySpendReport(config).GetSpendReportAsync(parameters, cancellationToken)
        );
    }

    /// <summary>Gets the info of one generation, including its cost breakdown.</summary>
    public Task<GatewayGenerationInfo> GetGenerationInfoAsync(
        GatewayGenerationInfoParams parameters,
        CancellationToken cancellationToken = default
    )
    {
        return WithGatewayErrorAsync(config =>
            new GatewayGenerationInfoFetcher(config).GetGenerationInfoAsync(
                parameters,
                cancellationToken
            )
        );
    }

    /// <summary>Resolves the Gateway credential: an explicit key, <c>AI_GATEWAY_API_KEY</c>, then the Vercel OIDC token.</summary>
    internal async Task<(string Token, string AuthMethod)> GetGatewayAuthTokenAsync(
        CancellationToken cancellationToken
    )
    {
        var apiKey = Options.ApiKey is null
            ? null
            : await Options.ApiKey.ResolveAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return (apiKey!, "api-key");
        }

        var oidcToken = await VercelEnvironment
            .GetVercelOidcTokenAsync(cancellationToken)
            .ConfigureAwait(false);
        return (oidcToken, "oidc");
    }

    /// <summary>Resolves the credential, wrapping a failure in a <see cref="GatewayAuthenticationError"/>.</summary>
    internal async Task<(string Token, string AuthMethod)> GetRealtimeAuthTokenAsync(
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await GetGatewayAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayAuthenticationError.CreateContextual(
                apiKeyProvided: false,
                oidcTokenProvided: false,
                statusCode: 401,
                cause: exception
            );
        }
    }

    /// <summary>Builds the auth headers for a resolved credential. Maps to <c>createAuthHeaders</c>.</summary>
    internal Dictionary<string, string> CreateAuthHeaders((string Token, string AuthMethod) auth)
    {
        var headers = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Authorization"] = "Bearer " + auth.Token,
            ["ai-gateway-protocol-version"] = "0.0.1",
            [GatewayErrors.AuthMethodHeader] = auth.AuthMethod,
        };
        if (Options.TeamIdOrSlug is not null)
        {
            headers[GatewayErrors.TeamHeader] = Options.TeamIdOrSlug;
        }

        if (Options.Headers is not null)
        {
            foreach (var pair in Options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return ProviderValues.WithUserAgentSuffix(
            headers,
            "ai-sdk-gateway/" + AiSdkVersion.Version
        );
    }

    /// <summary>Resolves the credential and builds the auth headers. Maps to <c>getHeaders</c>.</summary>
    internal async Task<IReadOnlyDictionary<string, string?>> GetHeadersAsync(
        CancellationToken cancellationToken
    )
    {
        var auth = await GetRealtimeAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        var headers = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var pair in CreateAuthHeaders(auth))
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }

    /// <summary>
    /// Mints a short-lived client secret (<c>vcst_</c>) through <c>/v1/realtime/client-secrets</c> on the Gateway origin
    /// (not under the <c>BaseUrl</c> path), authenticated with the long-lived Gateway credential. Maps to <c>mintClientSecret</c>.
    /// </summary>
    internal Task<GatewayMintedClientSecret> MintClientSecretAsync(
        string modelId,
        int? expiresAfterSeconds,
        CancellationToken cancellationToken
    )
    {
        return MintClientSecretAsync(modelId, expiresAfterSeconds, null, cancellationToken);
    }

    /// <summary>
    /// Like <see cref="MintClientSecretAsync(string, int?, CancellationToken)"/>. <paramref name="routeKind"/> binds the
    /// secret to a streaming surface (<c>speech</c> or <c>transcription</c>). It is omitted for realtime (the gateway
    /// default) so older gateway deployments keep accepting realtime mints.
    /// </summary>
    internal async Task<GatewayMintedClientSecret> MintClientSecretAsync(
        string modelId,
        int? expiresAfterSeconds,
        string? routeKind,
        CancellationToken cancellationToken
    )
    {
        AssertGatewayClientSecretServerEnvironment();
        var auth = await GetRealtimeAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        var headers = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var pair in CreateAuthHeaders(auth))
        {
            headers[pair.Key] = pair.Value;
        }

        var url = new Uri(
            new Uri(Options.BaseUrl, UriKind.Absolute),
            "/v1/realtime/client-secrets"
        );
        var body = new JsonObject { ["model"] = modelId };
        if (routeKind is not null)
        {
            body["routeKind"] = routeKind;
        }

        if (expiresAfterSeconds is { } seconds)
        {
            body["expiresIn"] = seconds;
        }

        try
        {
            using var document = await Http.SendJsonAsync(
                    HttpMethod.Post,
                    url,
                    body.ToJsonString(),
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            var root = document.RootElement;
            var token =
                root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("token", out var tokenValue)
                && tokenValue.ValueKind == JsonValueKind.String
                    ? tokenValue.GetString()
                    : null;
            if (token is null)
            {
                throw new AiSdkException(
                    "AI Gateway client secret response did not include a token."
                );
            }

            long? expiresAt = null;
            if (
                root.TryGetProperty("expiresAt", out var expiresValue)
                && expiresValue.ValueKind == JsonValueKind.Number
                && expiresValue.TryGetInt64(out var expires)
            )
            {
                expiresAt = expires;
            }

            return new GatewayMintedClientSecret(token, expiresAt);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayErrors.AsGatewayError(exception, GatewayErrors.ParseAuthMethod(headers));
        }
    }

    /// <summary>
    /// Tells whether the code runs in a browser, where a Gateway credential must never be used. .NET has no
    /// <c>window</c> global, so this maps <c>typeof globalThis.window !== 'undefined'</c> to the Blazor WebAssembly
    /// runtime. Tests replace it.
    /// </summary>
    internal Func<bool> IsBrowserEnvironment { get; set; } = DetectBrowserEnvironment;

    /// <summary>
    /// Default <see cref="IsBrowserEnvironment"/> predicate. Blazor WebAssembly reports the OS description
    /// <c>Browser</c>; the netstandard2.0 build has no <c>OperatingSystem.IsBrowser()</c>, so it reads that value.
    /// </summary>
    internal static bool DetectBrowserEnvironment() =>
#if NET
        OperatingSystem.IsBrowser();
#else
        string.Equals(
            System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            "Browser",
            StringComparison.OrdinalIgnoreCase
        );
#endif

    /// <summary>Throws when called in a browser. Maps to <c>assertGatewayClientSecretServerEnvironment</c>.</summary>
    private void AssertGatewayClientSecretServerEnvironment()
    {
        if (IsBrowserEnvironment())
        {
            throw new AiSdkException(
                "AI Gateway client secrets must be minted server-side: minting needs your Gateway credential, which must never reach the browser. Call gateway.ExperimentalRealtimeGetTokenAsync(), gateway.ExperimentalTranscriptionGetTokenAsync() or gateway.ExperimentalSpeechGetTokenAsync() from your server and pass the returned token to the client."
            );
        }
    }

    private static async Task<T> WaitAsync<T>(Task<T> task, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled || task.IsCompleted)
        {
            return await task.ConfigureAwait(false);
        }

        var cancelled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
        {
            if (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) != task)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        return await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a fetcher with a config whose headers are the auth headers (<c>getHeaders</c>), then maps a failure to a
    /// Gateway error using the auth method of the resolved headers, as upstream does.
    /// </summary>
    private async Task<T> WithGatewayErrorAsync<T>(Func<GatewayConfig, Task<T>> run)
    {
        IReadOnlyDictionary<string, string?>? resolvedHeaders = null;
        try
        {
            return await run(CreateConfig(headers => resolvedHeaders = headers))
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayErrors.AsGatewayError(
                exception,
                GatewayErrors.ParseAuthMethod(resolvedHeaders ?? new Dictionary<string, string?>())
            );
        }
    }

    internal GatewayConfig CreateConfig(
        Action<IReadOnlyDictionary<string, string?>>? onResolved = null
    )
    {
        return new GatewayConfig
        {
            BaseUrl = Options.BaseUrl,
            Headers = async cancellationToken =>
            {
                var headers = await GetHeadersAsync(cancellationToken).ConfigureAwait(false);
                onResolved?.Invoke(headers);
                return headers;
            },
            Http = Http,
        };
    }

    private async Task<GatewayFetchMetadataResponse> FetchMetadataAsync()
    {
        // The fetch is shared by every caller, so a caller's token does not cancel it.
        var metadata = await WithGatewayErrorAsync(config =>
                new GatewayFetchMetadata(config).GetAvailableModelsAsync()
            )
            .ConfigureAwait(false);
        lock (_metadataLock)
        {
            _metadataCache = metadata;
        }

        return metadata;
    }

    internal static Exception MapFailure(
        Exception exception,
        IReadOnlyDictionary<string, string?>? headers,
        string? requestBody = null
    )
    {
        return GatewayErrors.AsGatewayError(
            WithRequestBody(exception, requestBody),
            GatewayErrors.ParseAuthMethod(headers)
        );
    }

    /// <summary>
    /// Keeps the request body on a failed API call as <c>requestBodyValues</c>, annotated so telemetry copies hide
    /// <c>providerOptions.gateway.byok</c>. Maps to the body <c>postJsonToApi</c> stores on its <c>APICallError</c>.
    /// </summary>
    internal static Exception WithRequestBody(Exception exception, string? requestBody)
    {
        if (requestBody is null || exception is not ApiException { RequestBodyValues: null } api)
        {
            return exception;
        }

        try
        {
            // The body was serialized by the Gateway models, so it is read back with a depth limit that cannot reject it.
            using var document = JsonDocument.Parse(
                requestBody,
                new JsonDocumentOptions { MaxDepth = int.MaxValue }
            );
            return ProviderHttp.MapStatusWithHeaders(
                api.StatusCode,
                api.Message,
                api.ResponseBody,
                api.ResponseHeaders,
                api.Url,
                RedactGatewayRequestCredentials.WithGatewayRequestTelemetryRedaction(
                    document.RootElement.Clone()
                ),
                api.IsRetryable
            );
        }
        catch (JsonException)
        {
            // A body that cannot be read back must not hide the failure itself.
            return exception;
        }
    }

    internal Uri Route(string path)
    {
        return ApiKeys.Combine(Options.BaseUrl, path);
    }

    private IRealtimeModel CreateRealtimeModel(string modelId, GatewayRealtimeOptions? options)
    {
        var baseUrl = Options.BaseUrl.TrimEnd('/');
        return ResolveGatewayRealtimeApi(modelId, options) == GatewayRealtimeApi.Live
            ? new GatewayRealtimeModelLive(
                modelId,
                new GatewayRealtimeModelLiveConfig(
                    "gateway.live",
                    baseUrl,
                    GetHeadersAsync,
                    (id, expiresAfterSeconds, routeKind, cancellationToken) =>
                        MintClientSecretAsync(
                            id,
                            expiresAfterSeconds,
                            routeKind,
                            cancellationToken
                        ),
                    Options.TeamIdOrSlug
                )
            )
            : new GatewayRealtimeModel(
                modelId,
                new GatewayRealtimeModelConfig(
                    "gateway.realtime",
                    baseUrl,
                    GetHeadersAsync,
                    MintClientSecretAsync,
                    Options.TeamIdOrSlug
                )
            );
    }

    private static GatewayRealtimeApi ResolveGatewayRealtimeApi(
        string modelId,
        GatewayRealtimeOptions? options
    )
    {
        if (options?.Api is { } api)
        {
            if (api != GatewayRealtimeApi.Live && api != GatewayRealtimeApi.Realtime)
            {
                throw new InvalidArgumentException(
                    "api",
                    api,
                    "AI Gateway realtime api must be \"live\" or \"realtime\"."
                );
            }

            return api;
        }

        return GatewayKnownLiveModelId.GatewayLiveModelIds.Contains(modelId)
            ? GatewayRealtimeApi.Live
            : GatewayRealtimeApi.Realtime;
    }
}

/// <summary>The result of minting a streaming transcription client secret. Maps to <c>GatewayTranscriptionFactoryGetTokenResult</c>.</summary>
public sealed class GatewayTranscriptionFactoryGetTokenResult
{
    /// <summary>Creates a result.</summary>
    public GatewayTranscriptionFactoryGetTokenResult(
        string token,
        string url,
        long? expiresAt = null
    )
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        ExpiresAt = expiresAt;
    }

    /// <summary>The minted <c>vcst_</c> client secret.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL of the streaming transcription surface for this model.</summary>
    public string Url { get; }

    /// <summary>Token expiry, epoch seconds.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>The result of minting a streaming speech client secret. Maps to <c>GatewaySpeechFactoryGetTokenResult</c>.</summary>
public sealed class GatewaySpeechFactoryGetTokenResult
{
    /// <summary>Creates a result.</summary>
    public GatewaySpeechFactoryGetTokenResult(string token, string url, long? expiresAt = null)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        ExpiresAt = expiresAt;
    }

    /// <summary>The minted <c>vcst_</c> client secret.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL of the streaming speech surface for this model.</summary>
    public string Url { get; }

    /// <summary>Token expiry, epoch seconds.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>Gateway language model. The request body is the V4 call options.</summary>
public sealed class GatewayLanguageModel : ILanguageModel
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway language model.</summary>
    public GatewayLanguageModel(GatewayProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SupportedUrls { get; } =
        new Dictionary<string, string> { ["*/*"] = ".*" };

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        // Built outside the try so a missing key surfaces as it is.
        var headers = await _provider.HeadersAsync(
            "ai-language-model-specification-version",
            "4",
            "ai-language-model-id",
            ModelId,
            false,
            options.Headers,
            cancellationToken
        );
        var json = V4Json.CallOptions(options);
        try
        {
            using var document = await _provider
                .Http.SendJsonAsync(
                    HttpMethod.Post,
                    _provider.Route("language-model"),
                    json,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            return V4Json.ParseGenerate(document.RootElement).WithRequest(Request(json));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayProvider.MapFailure(exception, headers, json);
        }
    }

    // The request body metadata keeps the body unchanged and carries the telemetry view that hides BYOK credentials.
    private static LanguageModelStreamRequest Request(string json)
    {
        // The body was serialized by V4Json.CallOptions, so it is read back with a depth limit that cannot reject it.
        using var document = JsonDocument.Parse(
            json,
            new JsonDocumentOptions { MaxDepth = int.MaxValue }
        );
        var body = document.RootElement.Clone();
        return new LanguageModelStreamRequest(body)
        {
            RedactBody = RedactGatewayRequestCredentials
                .WithGatewayRequestTelemetryRedaction(body)
                .Redact,
        };
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        // The request is sent here, so HTTP errors surface from this call as they do upstream.
        var json = V4Json.CallOptions(options, stream: true);
        var headers = await _provider.HeadersAsync(
            "ai-language-model-specification-version",
            "4",
            "ai-language-model-id",
            ModelId,
            true,
            options.Headers,
            cancellationToken
        );
        ProviderSseResponse sse;
        try
        {
            sse = await _provider
                .Http.OpenSseResponseAsync(
                    _provider.Route("language-model"),
                    json,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayProvider.MapFailure(exception, headers, json);
        }

        Func<JsonElement, JsonElement>? redactBody;
        try
        {
            redactBody = Request(json).RedactBody;
        }
        catch
        {
            sse.Resource.Dispose();
            throw;
        }

        return sse.ToStreamResult(
            StreamAsync(sse.Events, options.IncludeRawChunks, headers, cancellationToken),
            json,
            redactBody
        );
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(
        IAsyncEnumerable<string> events,
        bool includeRawChunks,
        IReadOnlyDictionary<string, string?> headers,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var enumerator = events.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                bool hasEvent;
                try
                {
                    hasEvent = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (ApiException exception)
                {
                    throw GatewayProvider.MapFailure(exception, headers);
                }
                catch (ApiTimeoutException exception)
                {
                    throw GatewayProvider.MapFailure(exception, headers);
                }
                catch (ApiConnectionException exception)
                {
                    throw GatewayProvider.MapFailure(exception, headers);
                }

                if (!hasEvent)
                {
                    yield break;
                }

                // A chunk that does not parse fails the stream, as upstream calls controller.error.
                LanguageModelStreamPart? part;
                try
                {
                    part = V4Json.ParseStreamPart(enumerator.Current);
                }
                catch (JsonException exception)
                {
                    throw new JsonParseException(enumerator.Current, exception);
                }

                // Raw chunks are dropped unless the caller asked for them.
                if (part != null && (includeRawChunks || part is not RawStreamPart))
                {
                    yield return part;
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>Gateway embedding model.</summary>
public sealed class GatewayEmbeddingModel : IEmbeddingModel
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway embedding model.</summary>
    public GatewayEmbeddingModel(GatewayProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Embeds values without provider options.</summary>
    public Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        CancellationToken cancellationToken
    )
    {
        return DoEmbedAsync(values, null, null, cancellationToken);
    }

    /// <inheritdoc />
    Task<EmbeddingResult> IEmbeddingModel.DoEmbedAsync(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions,
        IReadOnlyDictionary<string, string?>? headers
    )
    {
        return DoEmbedAsync(values, null, headers, cancellationToken, dimensions);
    }

    /// <summary>Embeds values and sends <paramref name="providerOptions"/> when it is a JSON object.</summary>
    public Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        JsonElement? providerOptions,
        CancellationToken cancellationToken
    )
    {
        return DoEmbedAsync(values, providerOptions, null, cancellationToken);
    }

    /// <summary>Embeds values. Call headers are merged onto the Gateway V4 headers.</summary>
    public async Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        JsonElement? providerOptions,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken,
        int? dimensions = null
    )
    {
        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var body = new JsonObject { ["values"] = input };
        if (dimensions is { } width)
        {
            body["dimensions"] = width;
        }

        if (providerOptions is { ValueKind: JsonValueKind.Object } options)
        {
            body["providerOptions"] = JsonNode.Parse(options.GetRawText());
        }

        JsonDocument document;
        var requestHeaders = await _provider
            .HeadersAsync(
                "ai-embedding-model-specification-version",
                "4",
                "ai-model-id",
                ModelId,
                null,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        try
        {
            document = await _provider
                .Http.SendJsonAsync(
                    HttpMethod.Post,
                    _provider.Route("embedding-model"),
                    body.ToJsonString(),
                    requestHeaders,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiConnectionException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }

        using (document)
        {
            var vectors = new List<float[]>();
            foreach (
                var embedding in document.RootElement.GetProperty("embeddings").EnumerateArray()
            )
            {
                var vector = new float[embedding.GetArrayLength()];
                var index = 0;
                foreach (var number in embedding.EnumerateArray())
                {
                    vector[index++] = number.GetSingle();
                }

                vectors.Add(vector);
            }

            int? tokens = null;
            if (
                document.RootElement.TryGetProperty("usage", out var usage)
                && usage.ValueKind == JsonValueKind.Object
                && usage.TryGetProperty("tokens", out var tokenCount)
            )
            {
                tokens = tokenCount.GetInt32();
            }

            return new EmbeddingResult(vectors, tokens);
        }
    }
}

/// <summary>Gateway image model.</summary>
public sealed class GatewayImageModel : IImageModel, IImageCaller
{
    private const string StreamingImageDecodingOption = "experimental_streamingImageDecoding";

    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway image model.</summary>
    /// <param name="provider">Gateway provider that sends the requests.</param>
    /// <param name="modelId">Model id.</param>
    /// <param name="providerName">Value of <see cref="Provider"/> (upstream <c>GatewayConfig.provider</c>). Defaults to <c>gateway</c>.</param>
    public GatewayImageModel(GatewayProvider provider, string modelId, string? providerName = null)
    {
        _provider = provider;
        ModelId = modelId;
        Provider = providerName ?? GatewayProvider.ProviderId;
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Large so that requests are never split client-side.</summary>
    public int? MaxImagesPerCall => int.MaxValue;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var result = await DoGenerateAsync(
                options.ToModelCall(cancellationToken),
                cancellationToken
            )
            .ConfigureAwait(false);
        var images = new List<ProviderImage>();
        foreach (var image in result.Images)
        {
            var bytes = image is string text ? Convert.FromBase64String(text) : (byte[])image!;
            images.Add(
                new ProviderImage(
                    MediaTypes.DetectMediaType(bytes, "image") ?? "image/png",
                    bytes,
                    null
                )
            );
        }

        var response = new ImageResponseInfo(
            result.Response.Timestamp ?? DateTime.UtcNow,
            result.Response.ModelId,
            result.Response.Headers
        );
        var usage = result.Usage is null
            ? null
            : new LanguageModelUsage(
                result.Usage.InputTokens,
                result.Usage.OutputTokens,
                result.Usage.TotalTokens
            );
        return new ImageGenerationResult(
            images,
            result.Warnings.Select(warning => warning.ToCallWarning()).ToList(),
            response,
            usage
        );
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(
        ImageModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var body = new JsonObject { ["prompt"] = call.Prompt, ["n"] = call.N };
        if (!string.IsNullOrEmpty(call.Size))
        {
            body["size"] = call.Size;
        }

        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            body["aspectRatio"] = call.AspectRatio;
        }

        if (call.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        var streamingImageDecoding = false;
        if (call.ProviderOptions is { } providerOptions)
        {
            var requestProviderOptions = JsonNode.Parse(providerOptions.GetRawText());
            if (
                requestProviderOptions is JsonObject optionsObject
                && optionsObject["gateway"] is JsonObject gatewayOptions
                && gatewayOptions.ContainsKey(StreamingImageDecodingOption)
            )
            {
                // A client option: the service never sees it.
                streamingImageDecoding =
                    gatewayOptions[StreamingImageDecodingOption] is JsonValue flag
                    && flag.TryGetValue<bool>(out var enabled)
                    && enabled;
                gatewayOptions.Remove(StreamingImageDecodingOption);
                if (gatewayOptions.Count == 0)
                {
                    optionsObject.Remove("gateway");
                }

                if (optionsObject.Count == 0)
                {
                    requestProviderOptions = null;
                }
            }

            if (requestProviderOptions != null)
            {
                body["providerOptions"] = requestProviderOptions;
            }
        }

        if (call.Files != null)
        {
            var files = new JsonArray();
            foreach (var file in call.Files)
            {
                files.Add(EncodeFile(file));
            }

            body["files"] = files;
        }

        if (call.Mask != null)
        {
            body["mask"] = EncodeFile(call.Mask);
        }

        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        IReadOnlyDictionary<string, string> responseHeaders;
        JsonElement root;
        IReadOnlyDictionary<string, byte[]> binaries = new Dictionary<string, byte[]>();
        JsonDocument? document = null;
        var requestHeaders = await _provider
            .HeadersAsync(
                "ai-image-model-specification-version",
                "4",
                "ai-model-id",
                ModelId,
                null,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        try
        {
            if (streamingImageDecoding)
            {
                var json = await _provider
                    .Http.SendJsonResponseAsync(
                        HttpMethod.Post,
                        _provider.Route("image-model"),
                        body.ToJsonString(),
                        requestHeaders,
                        ["images[*]"],
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                responseHeaders = json.Headers;
                root = json.Value;
                binaries = json.Binaries;
            }
            else
            {
                var response = await _provider
                    .Http.SendJsonStringAsync(
                        HttpMethod.Post,
                        _provider.Route("image-model"),
                        body.ToJsonString(),
                        requestHeaders,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                responseHeaders = response.Headers;
                document = JsonDocument.Parse(
                    string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
                );
                root = document.RootElement;
            }
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiConnectionException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (Exception exception)
            when (streamingImageDecoding
                && exception is Vercel.AI.Util.DownloadError or JsonParseException
            )
        {
            // The streamed body is not valid: report it like any invalid Gateway response, without the image.
            throw new GatewayResponseError(
                "Invalid JSON response",
                validationError: exception.GetType().Name,
                cause: exception
            );
        }

        using var ownedDocument = document;
        var images = new List<object?>();
        if (streamingImageDecoding)
        {
            images.AddRange(ReadStreamedImages(root, binaries));
        }
        else if (
            root.TryGetProperty("images", out var array)
            && array.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    images.Add(item.GetString());
                }
                else if (
                    item.ValueKind == JsonValueKind.Object
                    && (
                        item.TryGetProperty("b64", out var b64)
                        || item.TryGetProperty("base64", out b64)
                    )
                    && b64.ValueKind == JsonValueKind.String
                )
                {
                    images.Add(b64.GetString());
                }
            }
        }

        var warnings = new List<OperationWarning>();
        if (
            root.TryGetProperty("warnings", out var warningArray)
            && warningArray.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var warning in warningArray.EnumerateArray())
            {
                warnings.Add(ReadWarning(warning));
            }
        }

        OperationUsage? usage = null;
        if (
            root.TryGetProperty("usage", out var usageElement)
            && usageElement.ValueKind == JsonValueKind.Object
        )
        {
            usage = new OperationUsage(
                ReadCount(usageElement, "inputTokens"),
                ReadCount(usageElement, "outputTokens"),
                ReadCount(usageElement, "totalTokens")
            );
        }

        JsonElement? providerMetadata =
            root.TryGetProperty("providerMetadata", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object
                ? metadata.Clone()
                : null;
        bool? isRetryable =
            root.TryGetProperty("isRetryable", out var retryable)
            && retryable.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? retryable.GetBoolean()
                : null;
        return new ImageModelResult(
            images,
            warnings,
            usage,
            providerMetadata,
            new ProviderResponse(responseHeaders, timestamp: DateTime.UtcNow, modelId: ModelId),
            isRetryable
        );
    }

    // The images of a streamed response: strings, with the extracted ones as bytes. Maps to `toImageModelImages`.
    private static List<object?> ReadStreamedImages(
        JsonElement root,
        IReadOnlyDictionary<string, byte[]> binaries
    )
    {
        if (
            !root.TryGetProperty("images", out var array)
            || array.ValueKind != JsonValueKind.Array
            || array.EnumerateArray().Any(item => item.ValueKind != JsonValueKind.String)
        )
        {
            // The error does not carry the image.
            throw new GatewayResponseError(
                "Invalid JSON response: images must be an array of strings",
                validationError: "images must be an array of strings"
            );
        }

        var images = new List<object?>();
        var extracted = false;
        foreach (var item in array.EnumerateArray())
        {
            var text = item.GetString()!;
            if (binaries.TryGetValue(text, out var bytes))
            {
                extracted = true;
                images.Add(bytes);
            }
            else
            {
                images.Add(text);
            }
        }

        if (!extracted)
        {
            return images;
        }

        // A result that mixes bytes and strings is returned as bytes.
        return images
            .Select(image => image is string text ? (object?)ByteEncoding.FromBase64(text) : image)
            .ToList();
    }

    private static JsonNode EncodeFile(ImageModelFile file)
    {
        var node = new JsonObject { ["type"] = file.Type };
        if (file.Type == "url")
        {
            node["url"] = file.Url;
            return AddProviderOptions(node, file);
        }

        node["mediaType"] = file.MediaType;
        node["data"] = file.Base64 ?? ByteEncoding.ToBase64(file.Data ?? Array.Empty<byte>());
        return AddProviderOptions(node, file);
    }

    private static JsonObject AddProviderOptions(JsonObject node, ImageModelFile file)
    {
        if (file.ProviderOptions != null)
        {
            node["providerOptions"] = JsonSerializer.SerializeToNode(file.ProviderOptions);
        }

        return node;
    }

    internal static OperationWarning ReadWarning(JsonElement warning)
    {
        string? Text(string name) =>
            warning.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        var type = Text("type") ?? "other";
        return type switch
        {
            "deprecated" => new OperationWarning(
                type,
                message: Text("message"),
                setting: Text("setting")
            ),
            "unsupported" or "compatibility" => new OperationWarning(
                type,
                feature: Text("feature"),
                details: Text("details")
            ),
            _ => new OperationWarning(type, message: Text("message")),
        };
    }

    private static int? ReadCount(JsonElement usage, string name)
    {
        return usage.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? (int)value.GetDouble()
            : null;
    }
}

/// <summary>Registers the Gateway provider.</summary>
public static class GatewayServiceCollectionExtensions
{
    /// <summary>Registers <see cref="GatewayProvider"/>.</summary>
    public static IServiceCollection AddGateway(
        this IServiceCollection services,
        Action<GatewayOptions>? configure = null
    )
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHttpClient(GatewayProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new GatewayOptions();
            configure?.Invoke(options);
            var http = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(GatewayProvider.ProviderId);
            return new GatewayProvider(options, http);
        });
        return services;
    }
}
