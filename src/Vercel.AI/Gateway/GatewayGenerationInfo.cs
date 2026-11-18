// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>Configuration shared by Gateway fetchers (<c>GatewayConfig</c>).</summary>
public sealed class GatewayConfig
{
    /// <summary>Gets or sets the base URL of the Gateway.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the asynchronous function that resolves the request headers (upstream <c>headers</c>).</summary>
    public Func<
        CancellationToken,
        Task<IReadOnlyDictionary<string, string?>>
    >? Headers { get; set; }

    internal async Task<IReadOnlyDictionary<string, string?>?> ResolveHeadersAsync(
        CancellationToken cancellationToken
    )
    {
        return Headers is null ? null : await Headers(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Gets or sets the HTTP client (stands in for upstream's <c>fetch</c>).</summary>
    public ProviderHttp? Http { get; set; }
}

/// <summary>Parameters for <see cref="GatewayGenerationInfoFetcher.GetGenerationInfoAsync"/>.</summary>
public sealed class GatewayGenerationInfoParams
{
    /// <summary>Gets or sets the generation ID to look up (format: <c>gen_&lt;ulid&gt;</c>).</summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>Information about a Gateway generation.</summary>
public sealed class GatewayGenerationInfo
{
    /// <summary>Gets or sets the generation ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the total cost in USD.</summary>
    [JsonPropertyName("totalCost")]
    public double TotalCost { get; set; }

    /// <summary>Gets or sets the market cost in USD, excluding surcharges. Null when unavailable.</summary>
    [JsonPropertyName("marketCost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MarketCost { get; set; }

    /// <summary>Gets or sets the total surcharges applied in USD. Null when unavailable.</summary>
    [JsonPropertyName("surchargeCost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SurchargeCost { get; set; }

    /// <summary>Gets or sets the total amount debited in USD. Null when unavailable.</summary>
    [JsonPropertyName("gatewayCost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? GatewayCost { get; set; }

    /// <summary>Gets or sets the upstream inference cost in USD (BYOK only).</summary>
    [JsonPropertyName("upstreamInferenceCost")]
    public double UpstreamInferenceCost { get; set; }

    /// <summary>Gets or sets the usage cost in USD (same as <see cref="TotalCost"/>).</summary>
    [JsonPropertyName("usage")]
    public double Usage { get; set; }

    /// <summary>Gets or sets the ISO 8601 timestamp when the generation was created.</summary>
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>Gets or sets the model identifier.</summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether BYOK credentials were used.</summary>
    [JsonPropertyName("isByok")]
    public bool IsByok { get; set; }

    /// <summary>Gets or sets the provider that served this generation.</summary>
    [JsonPropertyName("providerName")]
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether streaming was used.</summary>
    [JsonPropertyName("streamed")]
    public bool Streamed { get; set; }

    /// <summary>Gets or sets the finish reason (for example <c>stop</c>).</summary>
    [JsonPropertyName("finishReason")]
    public string FinishReason { get; set; } = string.Empty;

    /// <summary>Gets or sets the time to first token in milliseconds.</summary>
    [JsonPropertyName("latency")]
    public double Latency { get; set; }

    /// <summary>Gets or sets the total generation time in milliseconds.</summary>
    [JsonPropertyName("generationTime")]
    public double GenerationTime { get; set; }

    /// <summary>Gets or sets the number of prompt tokens.</summary>
    [JsonPropertyName("promptTokens")]
    public double PromptTokens { get; set; }

    /// <summary>Gets or sets the number of completion tokens.</summary>
    [JsonPropertyName("completionTokens")]
    public double CompletionTokens { get; set; }

    /// <summary>Gets or sets the reasoning tokens used.</summary>
    [JsonPropertyName("reasoningTokens")]
    public double ReasoningTokens { get; set; }

    /// <summary>Gets or sets the cached tokens used.</summary>
    [JsonPropertyName("cachedTokens")]
    public double CachedTokens { get; set; }

    /// <summary>Gets or sets the cache creation input tokens.</summary>
    [JsonPropertyName("cacheCreationTokens")]
    public double CacheCreationTokens { get; set; }

    /// <summary>Gets or sets the billable web search calls.</summary>
    [JsonPropertyName("billableWebSearchCalls")]
    public double BillableWebSearchCalls { get; set; }
}

/// <summary>Fetches generation info (<c>GET /v1/generation?id=</c>) from the Gateway.</summary>
public sealed class GatewayGenerationInfoFetcher
{
    private readonly GatewayConfig _config;

    /// <summary>Initializes a new instance of the <see cref="GatewayGenerationInfoFetcher"/> class.</summary>
    public GatewayGenerationInfoFetcher(GatewayConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Gets the info of one generation.</summary>
    public async Task<GatewayGenerationInfo> GetGenerationInfoAsync(
        GatewayGenerationInfoParams parameters,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var uri = new Uri(
                baseUrl.GetLeftPart(UriPartial.Authority)
                    + "/v1/generation?id="
                    + Uri.EscapeDataString(parameters.Id)
            );
            var http = _config.Http
                ?? throw new InvalidOperationException("GatewayConfig.Http is required.");
            using var document = await http.SendJsonAsync(
                    HttpMethod.Get,
                    uri,
                    null,
                    await _config.ResolveHeadersAsync(cancellationToken).ConfigureAwait(false),
                    cancellationToken
                )
                .ConfigureAwait(false);
            return Parse(document.RootElement);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayErrors.AsGatewayError(exception);
        }
    }

    private static GatewayGenerationInfo Parse(JsonElement root)
    {
        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Object
        )
        {
            throw new FormatException(
                "Type validation failed: expected an object with a data object."
            );
        }

        return new GatewayGenerationInfo
        {
            Id = Str(data, "id"),
            TotalCost = Num(data, "total_cost"),
            MarketCost = OptionalNum(data, "market_cost"),
            SurchargeCost = OptionalNum(data, "surcharge_cost"),
            GatewayCost = OptionalNum(data, "gateway_cost"),
            UpstreamInferenceCost = Num(data, "upstream_inference_cost"),
            Usage = Num(data, "usage"),
            CreatedAt = Str(data, "created_at"),
            Model = Str(data, "model"),
            IsByok = Bool(data, "is_byok"),
            ProviderName = Str(data, "provider_name"),
            Streamed = Bool(data, "streamed"),
            FinishReason = Str(data, "finish_reason"),
            Latency = Num(data, "latency"),
            GenerationTime = Num(data, "generation_time"),
            PromptTokens = Num(data, "native_tokens_prompt"),
            CompletionTokens = Num(data, "native_tokens_completion"),
            ReasoningTokens = Num(data, "native_tokens_reasoning"),
            CachedTokens = Num(data, "native_tokens_cached"),
            CacheCreationTokens = Num(data, "native_tokens_cache_creation"),
            BillableWebSearchCalls = Num(data, "billable_web_search_calls"),
        };
    }

    private static JsonElement Field(JsonElement data, string name, JsonValueKind kind)
    {
        if (!data.TryGetProperty(name, out var value) || value.ValueKind != kind)
        {
            throw new FormatException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Type validation failed: invalid or missing {0}.",
                    name
                )
            );
        }

        return value;
    }

    private static string Str(JsonElement data, string name) =>
        Field(data, name, JsonValueKind.String).GetString()!;

    private static double Num(JsonElement data, string name) =>
        Field(data, name, JsonValueKind.Number).GetDouble();

    private static bool Bool(JsonElement data, string name)
    {
        if (
            !data.TryGetProperty(name, out var value)
            || value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
        )
        {
            throw new FormatException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Type validation failed: invalid or missing {0}.",
                    name
                )
            );
        }

        return value.GetBoolean();
    }

    private static double? OptionalNum(JsonElement data, string name)
    {
        if (!data.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return Field(data, name, JsonValueKind.Number).GetDouble();
    }
}
