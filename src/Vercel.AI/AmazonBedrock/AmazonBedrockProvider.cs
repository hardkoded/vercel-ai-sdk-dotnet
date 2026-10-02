// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>AWS credentials used to sign a Bedrock request.</summary>
public sealed class AmazonBedrockCredentials
{
    /// <summary>Creates credentials.</summary>
    public AmazonBedrockCredentials(string accessKeyId, string secretAccessKey, string? sessionToken = null)
    {
        AccessKeyId = accessKeyId ?? string.Empty;
        SecretAccessKey = secretAccessKey ?? string.Empty;
        SessionToken = sessionToken;
    }

    /// <summary>Access key id.</summary>
    public string AccessKeyId { get; }

    /// <summary>Secret access key.</summary>
    public string SecretAccessKey { get; }

    /// <summary>Optional session token.</summary>
    public string? SessionToken { get; }
}

/// <summary>Amazon Bedrock settings.</summary>
public sealed class AmazonBedrockOptions
{
    /// <summary>AWS region.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Access key. Falls back to <c>AWS_ACCESS_KEY_ID</c>.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Secret key. Falls back to <c>AWS_SECRET_ACCESS_KEY</c>.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Session token. Falls back to <c>AWS_SESSION_TOKEN</c> unless both access keys are set explicitly.</summary>
    public string? SessionToken { get; set; }

    /// <summary>Bearer token. Falls back to <c>AWS_BEARER_TOKEN_BEDROCK</c> when this is null.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Explicit base URL. When set, region and endpoint environment variables are not used.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Supplies credentials for each signed request.</summary>
    public Func<AmazonBedrockCredentials>? CredentialProvider { get; set; }

    /// <summary>Clock used for signing. Tests can pin this.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }
}

/// <summary>Amazon Bedrock Converse provider.</summary>
public sealed class AmazonBedrockProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "amazon-bedrock";

    /// <summary>Creates a provider.</summary>
    public AmazonBedrockProvider(HttpClient httpClient, AmazonBedrockOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new AmazonBedrockOptions();
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Http = new ProviderHttp(HttpClient);
    }

    /// <summary>Options.</summary>
    public AmazonBedrockOptions Options { get; }

    /// <summary>HTTP client that receives the signed request.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static AmazonBedrockProvider Create(AmazonBedrockOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AmazonBedrockProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new AmazonBedrockLanguageModel(this, modelId);

    /// <summary>Creates a chat model and records its family, such as <c>anthropic</c>.</summary>
    public AmazonBedrockLanguageModel ChatModel(string modelId, string? modelFamily = null)
    {
        return new AmazonBedrockLanguageModel(this, modelId) { ModelFamily = modelFamily };
    }

    /// <summary>Creates a reranking model.</summary>
    public new AmazonBedrockRerankingModel RerankingModel(string modelId)
    {
        return new AmazonBedrockRerankingModel(this, modelId);
    }

    internal Uri ConverseUri(string modelId)
    {
        return new Uri(AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(Options) + "/model/" + Uri.EscapeDataString(modelId) + "/converse");
    }

    internal Uri RerankUri()
    {
        return new Uri(AmazonBedrockEndpoints.ResolveAgentRuntimeBaseUrl(Options) + "/rerank");
    }
}

/// <summary>Bedrock Converse language model.</summary>
public sealed class AmazonBedrockLanguageModel : ILanguageModel
{
    private readonly AmazonBedrockProvider _provider;

    /// <summary>Creates a model.</summary>
    public AmazonBedrockLanguageModel(AmazonBedrockProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AmazonBedrockProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Model family used when the id does not identify the underlying model.</summary>
    public string? ModelFamily { get; set; }

    /// <summary>Id factory for tool calls that arrive without a tool-use id.</summary>
    public Func<string>? GenerateId { get; set; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = AmazonBedrockConverseRequest.Prepare(ModelId, options, ModelFamily);
        var bodyNode = prepared.CreateTransportBody();
        var body = bodyNode.ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, _provider.ConverseUri(ModelId))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        CopyHeaders(request, options?.Headers);
        Sign(request, Encoding.UTF8.GetBytes(body));
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = CopyResponseHeaders(response);
        if (!response.IsSuccessStatusCode)
        {
            throw ProviderHttp.MapStatus((int)response.StatusCode, text);
        }

        using var document = JsonDocument.Parse(text);
        var parsed = AmazonBedrockResponseParser.Parse(document.RootElement, ModelId, prepared.UsesJsonResponseTool, GenerateId, headers);
        return new LanguageModelGenerateResult(
            parsed.Content,
            parsed.FinishReason,
            parsed.Usage,
            parsed.RawFinishReason,
            MapWarnings(prepared.Warnings),
            parsed.ResponseId,
            parsed.ProviderMetadata,
            text,
            ModelId,
            parsed.Timestamp,
            headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (result.Warnings.Count > 0)
        {
            yield return new StreamStartStreamPart(result.Warnings);
        }

        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        foreach (var part in result.Content)
        {
            if (part is GeneratedReasoning reasoning && reasoning.Text.Length > 0)
            {
                yield return new ReasoningDeltaStreamPart("reasoning", reasoning.Text);
            }
            else if (part is GeneratedToolCall call)
            {
                yield return new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.ArgumentsJson);
            }
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason, result.ProviderMetadata);
    }

    private void Sign(HttpRequestMessage request, byte[] payload)
    {
        var apiKey = ResolveApiKey();
        if (apiKey != null)
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiKey);
            return;
        }

        var credentials = ResolveCredentials();
        AwsSigV4.Sign(request, payload, _provider.Options.Region, "bedrock", credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
    }

    private string? ResolveApiKey()
    {
        if (_provider.Options.ApiKey != null)
        {
            var trimmed = _provider.Options.ApiKey.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        var environment = Environment.GetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK");
        if (string.IsNullOrWhiteSpace(environment))
        {
            return null;
        }

        return environment!.Trim();
    }

    private AmazonBedrockCredentials ResolveCredentials()
    {
        if (_provider.Options.CredentialProvider != null)
        {
            var provided = _provider.Options.CredentialProvider() ?? throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
            if (string.IsNullOrEmpty(provided.AccessKeyId) || string.IsNullOrEmpty(provided.SecretAccessKey))
            {
                throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
            }

            return provided;
        }

        var explicitAccess = _provider.Options.AccessKeyId != null;
        var explicitSecret = _provider.Options.SecretAccessKey != null;
        var accessKey = _provider.Options.AccessKeyId ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secret = _provider.Options.SecretAccessKey ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
        }

        string? token;
        if (explicitAccess && explicitSecret)
        {
            token = _provider.Options.SessionToken;
        }
        else
        {
            token = _provider.Options.SessionToken ?? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        }

        return new AmazonBedrockCredentials(accessKey!, secret!, token);
    }

    private static void CopyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var header in headers)
        {
            if (string.IsNullOrEmpty(header.Key) || header.Value == null)
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static Dictionary<string, string> CopyResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }

    private static IReadOnlyList<CallWarning> MapWarnings(IReadOnlyList<AmazonBedrockWarning> warnings)
    {
        var mapped = new List<CallWarning>();
        foreach (var warning in warnings)
        {
            mapped.Add(new CallWarning(warning.Type, warning.Details ?? warning.Feature));
        }

        return mapped;
    }
}

/// <summary>Bedrock Agent Runtime reranking model.</summary>
public sealed class AmazonBedrockRerankingModel
{
    private readonly AmazonBedrockProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public AmazonBedrockRerankingModel(AmazonBedrockProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <summary>Provider id.</summary>
    public string Provider => AmazonBedrockProvider.ProviderName;

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Extra headers merged into the rerank request.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Reranks <paramref name="documents"/>.</summary>
    public async Task<AmazonBedrockRerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, bool jsonDocuments, int? topN, IReadOnlyDictionary<string, JsonElement>? providerOptions, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
    {
        var nodes = new List<JsonNode?>();
        foreach (var document in documents ?? Array.Empty<string>())
        {
            if (jsonDocuments)
            {
                nodes.Add(JsonNode.Parse(document));
            }
            else
            {
                nodes.Add(JsonValue.Create(document));
            }
        }

        AmazonBedrockRerank.ReadOptions(providerOptions, out var nextToken, out var additional);
        var bodyNode = AmazonBedrockRerank.BuildRequest(ModelId, _provider.Options.Region, query, topN, jsonDocuments, nodes, nextToken, additional);
        var body = bodyNode.ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, _provider.RerankUri())
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        CopyHeaders(request, Headers);
        CopyHeaders(request, headers);
        if (!string.IsNullOrEmpty(_provider.Options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _provider.Options.ApiKey.Trim());
        }
        else if (!string.IsNullOrEmpty(_provider.Options.AccessKeyId) && !string.IsNullOrEmpty(_provider.Options.SecretAccessKey))
        {
            AwsSigV4.Sign(request, Encoding.UTF8.GetBytes(body), _provider.Options.Region, "bedrock", _provider.Options.AccessKeyId!, _provider.Options.SecretAccessKey!, _provider.Options.SessionToken, _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
        }

        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw ProviderHttp.MapStatus((int)response.StatusCode, text);
        }

        return AmazonBedrockRerank.Parse(text, CopyResponseHeaders(response));
    }

    private static void CopyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var header in headers)
        {
            if (!string.IsNullOrEmpty(header.Key) && header.Value != null)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static Dictionary<string, string> CopyResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }
}

/// <summary>Registers Bedrock.</summary>
public static class AmazonBedrockServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AmazonBedrockProvider"/>.</summary>
    public static IServiceCollection AddAmazonBedrock(this IServiceCollection services, Action<AmazonBedrockOptions>? configure = null)
    {
        services.AddHttpClient(AmazonBedrockProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new AmazonBedrockOptions();
            configure?.Invoke(options);
            return new AmazonBedrockProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AmazonBedrockProvider.ProviderName), options);
        });
        return services;
    }
}
