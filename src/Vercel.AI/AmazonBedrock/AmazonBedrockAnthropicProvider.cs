// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Settings for Anthropic Messages on Amazon Bedrock InvokeModel.</summary>
public sealed class AmazonBedrockAnthropicOptions
{
    /// <summary>AWS region. Falls back to <c>AWS_REGION</c>, then <c>us-east-1</c>.</summary>
    public string Region
    {
        get => _region ?? AmazonBedrockEndpoints.DefaultRegion();
        set => _region = value;
    }

    private string? _region;

    /// <summary>Bearer token. When set, SigV4 is not used. Falls back to <c>AWS_BEARER_TOKEN_BEDROCK</c>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Access key. Falls back to <c>AWS_ACCESS_KEY_ID</c>.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Secret key. Falls back to <c>AWS_SECRET_ACCESS_KEY</c>.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Session token. Falls back to <c>AWS_SESSION_TOKEN</c>.</summary>
    public string? SessionToken { get; set; }

    /// <summary>
    /// Runtime origin. When empty, <c>AWS_ENDPOINT_URL_BEDROCK_RUNTIME</c>, then <c>AWS_ENDPOINT_URL</c>,
    /// then the regional Bedrock Runtime host is used.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Supplies SigV4 credentials for each request. When set, its keys are used instead of
    /// <see cref="AccessKeyId"/>, <see cref="SecretAccessKey"/>, and <see cref="SessionToken"/>.
    /// The region always comes from <see cref="Region"/>.
    /// </summary>
    public Func<CancellationToken, Task<AmazonBedrockCredentials>>? CredentialProvider { get; set; }

    /// <summary>Clock used for signing.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }

    /// <summary>Extra headers sent on every call. The SDK user-agent suffix is appended.</summary>
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves extra headers on every call, after <see cref="Headers"/>. Its values win. A null value drops the header.
    /// </summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? HeadersProvider { get; set; }
}

/// <summary>Builds InvokeModel URLs and the Bedrock Anthropic request body.</summary>
public static class AmazonBedrockAnthropicRequests
{
    /// <summary>InvokeModel URL. Streaming uses <c>invoke-with-response-stream</c>.</summary>
    public static string BuildUrl(string baseUrl, string modelId, bool streaming)
    {
        var action = streaming ? "invoke-with-response-stream" : "invoke";
        return baseUrl.TrimEnd('/') + "/model/" + Uri.EscapeDataString(modelId) + "/" + action;
    }

    private static readonly Dictionary<string, string> ToolVersionMap = new(StringComparer.Ordinal)
    {
        ["bash_20241022"] = "bash_20250124",
        ["text_editor_20241022"] = "text_editor_20250728",
        ["computer_20241022"] = "computer_20250124",
    };

    private static readonly Dictionary<string, string> ToolNameMap = new(StringComparer.Ordinal)
    {
        ["text_editor_20250728"] = "str_replace_based_edit_tool",
    };

    private static readonly Dictionary<string, string> ToolBetaMap = new(StringComparer.Ordinal)
    {
        ["bash_20250124"] = "computer-use-2025-01-24",
        ["bash_20241022"] = "computer-use-2024-10-22",
        ["text_editor_20250124"] = "computer-use-2025-01-24",
        ["text_editor_20241022"] = "computer-use-2024-10-22",
        ["text_editor_20250429"] = "computer-use-2025-01-24",
        ["text_editor_20250728"] = "computer-use-2025-01-24",
        ["computer_20250124"] = "computer-use-2025-01-24",
        ["computer_20241022"] = "computer-use-2024-10-22",
        ["tool_search_tool_regex_20251119"] = "tool-search-tool-2025-10-19",
        ["tool_search_tool_bm25_20251119"] = "tool-search-tool-2025-10-19",
    };

    /// <summary>
    /// Drops <c>model</c> and <c>stream</c>, keeps a tool choice type and name, renames the thinking
    /// block binding field, upgrades computer-use tool versions, moves the betas into
    /// <c>anthropic_beta</c>, and sets <c>anthropic_version</c> to <c>bedrock-2023-05-31</c>.
    /// </summary>
    public static JsonObject Transform(JsonObject source, IReadOnlyList<string>? betas = null)
    {
        var result = (JsonObject)(JsonNode.Parse(source.ToJsonString()) ?? new JsonObject());
        result.Remove("model");
        result.Remove("stream");
        var requiredBetas = new List<string>();
        if (betas != null)
        {
            foreach (var beta in betas)
            {
                AddBeta(requiredBetas, beta);
            }
        }

        if (result["tool_choice"] is JsonObject choice)
        {
            var transformed = new JsonObject();
            if (choice["type"] is JsonValue type)
            {
                transformed["type"] = type.GetValue<string>();
            }

            if (choice["name"] is JsonValue name)
            {
                transformed["name"] = name.GetValue<string>();
            }

            result["tool_choice"] = transformed;
        }

        // Bedrock names the binding field `mismatch_behavior`; the Messages API names it `prefix_mismatch_behavior`.
        if (result["thinking"] is JsonObject thinking && thinking["block_binding"] is JsonObject binding)
        {
            var renamed = new JsonObject();
            if (binding["prefix_mismatch_behavior"] is JsonNode mismatch)
            {
                renamed["mismatch_behavior"] = mismatch.DeepClone();
            }

            thinking["block_binding"] = renamed;
        }

        if (result["tools"] is JsonArray tools)
        {
            for (var index = 0; index < tools.Count; index++)
            {
                if (tools[index] is JsonObject tool)
                {
                    TransformTool(tool, requiredBetas);
                }
            }
        }

        if (requiredBetas.Count > 0)
        {
            var array = new JsonArray();
            foreach (var beta in requiredBetas)
            {
                array.Add(beta);
            }

            result["anthropic_beta"] = array;
        }

        result["anthropic_version"] = "bedrock-2023-05-31";
        return result;
    }

    private static void TransformTool(JsonObject tool, List<string> requiredBetas)
    {
        // Bedrock rejects the per-tool field, but the beta alone enables the same behavior.
        var eager = tool["eager_input_streaming"];
        tool.Remove("eager_input_streaming");
        if (eager is JsonValue eagerValue && eagerValue.TryGetValue<bool>(out var enabled) && enabled)
        {
            AddBeta(requiredBetas, "fine-grained-tool-streaming-2025-05-14");
        }

        var toolType = tool["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var text) ? text : null;
        if (toolType == null)
        {
            return;
        }

        if (ToolVersionMap.TryGetValue(toolType, out var newType))
        {
            if (ToolBetaMap.TryGetValue(newType, out var versionBeta))
            {
                AddBeta(requiredBetas, versionBeta);
            }

            tool["type"] = newType;
            if (ToolNameMap.TryGetValue(newType, out var newName))
            {
                tool["name"] = newName;
            }

            return;
        }

        if (ToolBetaMap.TryGetValue(toolType, out var beta))
        {
            AddBeta(requiredBetas, beta);
        }

        if (ToolNameMap.TryGetValue(toolType, out var name))
        {
            tool["name"] = name;
        }

        return;
    }

    private static void AddBeta(List<string> betas, string beta)
    {
        if (!betas.Contains(beta))
        {
            betas.Add(beta);
        }
    }

    /// <summary>Rewrites a Bedrock error body into the Anthropic <c>{ type: error, error }</c> shape.</summary>
    public static string ToAnthropicErrorBody(string text)
    {
        var message = text ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                using var document = JsonDocument.Parse(text);
                if (document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("message", out var messageElement)
                    && messageElement.ValueKind == JsonValueKind.String
                    && !string.IsNullOrEmpty(messageElement.GetString()))
                {
                    message = messageElement.GetString() ?? message;
                }
            }
            catch (JsonException)
            {
            }
        }

        return new JsonObject
        {
            ["type"] = "error",
            ["error"] = new JsonObject
            {
                ["type"] = "error",
                ["message"] = message,
            },
        }.ToJsonString();
    }
}

/// <summary>Anthropic Messages provider hosted on Amazon Bedrock.</summary>
public sealed class AmazonBedrockAnthropicProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "bedrock.anthropic.messages";

    /// <summary>Creates a provider.</summary>
    public AmazonBedrockAnthropicProvider(HttpClient httpClient, AmazonBedrockAnthropicOptions? options = null)
        : base(ProviderId)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? new AmazonBedrockAnthropicOptions();
    }

    /// <summary>Options.</summary>
    public AmazonBedrockAnthropicOptions Options { get; }

    /// <summary>HTTP client that receives the signed request.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static AmazonBedrockAnthropicProvider Create(AmazonBedrockAnthropicOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AmazonBedrockAnthropicProvider(client, options);
    }

    /// <summary>The Anthropic provider tool ids. The same list as <see cref="Vercel.AI.Anthropic.AnthropicProvider.Tools"/>.</summary>
    public static IReadOnlyList<string> Tools => Vercel.AI.Anthropic.AnthropicProvider.Tools;

    /// <summary>Bedrock Runtime origin for these options.</summary>
    public string ResolveBaseUrl()
    {
        return AmazonBedrockEndpoints.Resolve(Options.BaseUrl, Options.Region, "bedrock-runtime");
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new AmazonBedrockAnthropicLanguageModel(this, modelId);
    }

    /// <summary>Throws because the JavaScript provider function rejects <c>new</c>.</summary>
    public ILanguageModel New(string modelId)
    {
        throw new InvalidOperationException("The Bedrock Anthropic model function cannot be called with the new keyword.");
    }

    /// <summary>Bedrock Anthropic has no embedding models.</summary>
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <summary>Bedrock Anthropic has no image models.</summary>
    public override IImageModel ImageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "imageModel");
    }
}

/// <summary>Anthropic language model invoked through Bedrock. Delegates to <see cref="Vercel.AI.Anthropic.AnthropicLanguageModel"/>.</summary>
public sealed class AmazonBedrockAnthropicLanguageModel : ILanguageModel
{
    private readonly AmazonBedrockAnthropicProvider _provider;

    /// <summary>Creates a model.</summary>
    public AmazonBedrockAnthropicLanguageModel(AmazonBedrockAnthropicProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AmazonBedrockAnthropicProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return Inner().DoGenerateAsync(options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return Inner().DoStreamAsync(options, cancellationToken);
    }

    private ILanguageModel Inner()
    {
        return new Vercel.AI.Anthropic.AnthropicLanguageModel(CreateInnerProvider(), ModelId);
    }

    /// <summary>The Anthropic provider this model delegates to.</summary>
    internal AmazonBedrockAnthropicInnerProvider CreateInnerProvider()
    {
        var baseUrl = _provider.ResolveBaseUrl();
        var options = _provider.Options;
        var apiKey = options.ApiKey ?? Environment.GetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK");
        var signing = new AmazonBedrockSigningHandler(
            new HttpClientMessageHandler(_provider.HttpClient),
            options.Region,
            "bedrock",
            apiKey,
            options.AccessKeyId,
            options.SecretAccessKey,
            options.SessionToken,
            options.UtcNow,
            options.CredentialProvider);
        var client = new HttpClient(AmazonBedrockAnthropicFetch.Create(signing), disposeHandler: false);
        return new AmazonBedrockAnthropicInnerProvider(client, baseUrl, ModelId, options.Headers, options.HeadersProvider);
    }
}

/// <summary>The Anthropic Messages provider that Bedrock models run on.</summary>
internal sealed class AmazonBedrockAnthropicInnerProvider : Vercel.AI.Anthropic.AnthropicProvider
{
    private readonly string _baseUrl;
    private readonly string _modelId;
    private readonly IReadOnlyDictionary<string, string?> _headers;
    private readonly Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? _headersProvider;

    public AmazonBedrockAnthropicInnerProvider(
        HttpClient httpClient,
        string baseUrl,
        string modelId,
        IReadOnlyDictionary<string, string?> headers,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? headersProvider = null)
        : base(httpClient, new Vercel.AI.Anthropic.AnthropicOptions { BaseUrl = baseUrl, Name = AmazonBedrockAnthropicProvider.ProviderId })
    {
        _baseUrl = baseUrl;
        _modelId = modelId;
        _headers = headers;
        _headersProvider = headersProvider;
    }

    // Bedrock Anthropic does not support URL sources, so images and files are downloaded and sent as base64.
    protected internal override IReadOnlyDictionary<string, string> SupportedUrls { get; } = new Dictionary<string, string>();

    protected internal override bool SupportsNativeStructuredOutput => AmazonBedrockModelSupport.SupportsNativeStructuredOutput(_modelId);

    protected internal override bool SupportsStrictTools => AmazonBedrockModelSupport.SupportsStrictTools(_modelId);

    protected internal override Uri RequestUri(string modelId, bool streaming)
    {
        return new Uri(AmazonBedrockAnthropicRequests.BuildUrl(_baseUrl, modelId, streaming));
    }

    protected internal override JsonObject TransformRequestBody(JsonObject body, IReadOnlyList<string> betas)
    {
        return AmazonBedrockAnthropicRequests.Transform(body, betas);
    }

    public override async Task<Dictionary<string, string?>> CreateHeadersAsync(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders, string? body, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var configured = _headers;
        if (_headersProvider != null)
        {
            var merged = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in _headers)
            {
                merged[pair.Key] = pair.Value;
            }

            foreach (var pair in await AmazonBedrockProvider.WaitAsync(_headersProvider(cancellationToken), cancellationToken).ConfigureAwait(false))
            {
                merged[pair.Key] = pair.Value;
            }

            configured = merged;
        }

        foreach (var pair in ProviderValues.WithUserAgentSuffix(configured, "ai-sdk-amazon-bedrock/" + AiSdkVersion.Version))
        {
            headers[pair.Key] = pair.Value;
        }

        if (betas.Count > 0)
        {
            headers["anthropic-beta"] = string.Join(",", betas);
        }

        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }
}
