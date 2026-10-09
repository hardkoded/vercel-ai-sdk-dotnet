// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock settings.</summary>
public sealed class AmazonBedrockOptions
{
    /// <summary>AWS region.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Agent Runtime origin for rerank. When empty, the host is
    /// <c>https://bedrock-agent-runtime.{region}.{suffix}</c>.
    /// </summary>
    public string? AgentRuntimeBaseUrl { get; set; }

    /// <summary>Access key. Falls back to <c>AWS_ACCESS_KEY_ID</c>.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Secret key. Falls back to <c>AWS_SECRET_ACCESS_KEY</c>.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Session token. Falls back to <c>AWS_SESSION_TOKEN</c>.</summary>
    public string? SessionToken { get; set; }

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

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId) => new AmazonBedrockRerankingModel(this, modelId);

    internal Uri ConverseUri(string modelId)
    {
        if (!HostnameParts.IsValidHostnamePart(Options.Region))
        {
            throw new ArgumentException("An AWS region must be a single DNS label.", nameof(AmazonBedrockOptions.Region));
        }

        return new Uri("https://bedrock-runtime." + Options.Region + ".amazonaws.com/model/" + Uri.EscapeDataString(modelId) + "/converse");
    }
}

/// <summary>Bedrock Converse language model.</summary>
public sealed class AmazonBedrockLanguageModel : ILanguageModel
{
    private const string JsonToolName = "json";

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

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var bodyObject = Build(options, out var warnings, out var usesJsonTool);
        var body = bodyObject.ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, _provider.ConverseUri(ModelId))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        Sign(request, Encoding.UTF8.GetBytes(body));
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw AmazonBedrockErrors.Create((int)response.StatusCode, text);
        }

        using var document = JsonDocument.Parse(text);
        return Parse(document.RootElement, warnings, CopyHeaders(response), usesJsonTool);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        foreach (var part in result.Content)
        {
            if (part is GeneratedText text)
            {
                yield return new TextDeltaStreamPart("text", text.Text);
            }
            else if (part is GeneratedReasoning reasoning && reasoning.Text.Length > 0)
            {
                yield return new ReasoningDeltaStreamPart("reasoning", reasoning.Text);
            }
            else if (part is GeneratedToolCall call)
            {
                yield return new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.ArgumentsJson);
            }
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private void Sign(HttpRequestMessage request, byte[] payload)
    {
        var accessKey = _provider.Options.AccessKeyId ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secret = _provider.Options.SecretAccessKey ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
        }

        var token = _provider.Options.SessionToken ?? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        AwsSigV4.Sign(request, payload, _provider.Options.Region, "bedrock", accessKey!, secret!, token, _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
    }

    private JsonObject Build(LanguageModelCallOptions options, out List<CallWarning> warnings, out bool usesJsonTool)
    {
        AmazonBedrockMessages.Convert(ModelId, options.Prompt, out var system, out var messages);
        var body = new JsonObject { ["messages"] = messages };
        if (system != null)
        {
            body["system"] = system;
        }

        var inference = new JsonObject();
        if (options.MaxOutputTokens is { } max)
        {
            inference["maxTokens"] = max;
        }

        if (options.Temperature is { } temperature)
        {
            inference["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            inference["topP"] = topP;
        }

        if (options.TopK is { } topK)
        {
            inference["topK"] = topK;
        }

        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            inference["stopSequences"] = stop;
        }

        if (inference.Count > 0)
        {
            body["inferenceConfig"] = inference;
        }

        // Structured output goes through a forced `json` tool. Its input becomes the response text.
        usesJsonTool = options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Null && schema.ValueKind != JsonValueKind.Undefined;
        var tools = options.Tools;
        var toolChoice = options.ToolChoice;
        if (usesJsonTool)
        {
            var list = new List<LanguageModelTool>(tools ?? Array.Empty<LanguageModelTool>())
            {
                new LanguageModelTool(JsonToolName, "Respond with a JSON object.", options.JsonSchema!.Value),
            };
            tools = list;
            toolChoice = ToolChoice.Required;
        }

        var toolConfig = AmazonBedrockTools.Prepare(ModelId, tools, toolChoice, out warnings);
        if (toolConfig.Count > 0)
        {
            body["toolConfig"] = toolConfig;
        }

        var requestMetadata = ReadRequestMetadata(options);
        if (requestMetadata != null)
        {
            body["requestMetadata"] = requestMetadata;
        }

        return body;
    }

    /// <summary>Reads <c>requestMetadata</c> from <c>amazon-bedrock</c>, then <c>amazonBedrock</c>, then <c>bedrock</c>.</summary>
    private static JsonObject? ReadRequestMetadata(LanguageModelCallOptions options)
    {
        var providerOptions = options.ProviderOptions;
        if (providerOptions == null)
        {
            return null;
        }

        JsonElement provider;
        if (!providerOptions.TryGetValue("amazon-bedrock", out provider)
            && !providerOptions.TryGetValue("amazonBedrock", out provider)
            && !providerOptions.TryGetValue("bedrock", out provider))
        {
            return null;
        }

        if (provider.ValueKind != JsonValueKind.Object || !provider.TryGetProperty("requestMetadata", out var metadata))
        {
            return null;
        }

        if (metadata.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
        }

        var result = new JsonObject();
        foreach (var property in metadata.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
            }

            var value = property.Value.GetString();
            if (value == null)
            {
                throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
            }

            result[property.Name] = value;
        }

        return result;
    }

    private LanguageModelGenerateResult Parse(JsonElement root, IReadOnlyList<CallWarning> warnings, IReadOnlyDictionary<string, string> headers, bool usesJsonTool)
    {
        var isMistral = AmazonBedrockToolIds.IsMistralModel(ModelId);
        var content = new List<GeneratedContent>();
        var jsonTool = false;
        if (root.TryGetProperty("output", out var output) && output.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    content.Add(new GeneratedText(text.GetString() ?? string.Empty));
                }
                else if (part.TryGetProperty("citationsContent", out var citations) && citations.TryGetProperty("content", out var cited) && cited.ValueKind == JsonValueKind.Array)
                {
                    foreach (var citedPart in cited.EnumerateArray())
                    {
                        if (citedPart.TryGetProperty("text", out var citedText) && citedText.ValueKind == JsonValueKind.String)
                        {
                            content.Add(new GeneratedText(citedText.GetString() ?? string.Empty));
                        }
                    }
                }

                if (part.TryGetProperty("reasoningContent", out var reasoning) && reasoning.ValueKind == JsonValueKind.Object)
                {
                    if (reasoning.TryGetProperty("reasoningText", out var reasoningText) && reasoningText.ValueKind == JsonValueKind.Object && reasoningText.TryGetProperty("text", out var reasoningValue))
                    {
                        content.Add(new GeneratedReasoning(reasoningValue.GetString() ?? string.Empty));
                    }
                    else if (reasoning.TryGetProperty("redactedReasoning", out _) || reasoning.TryGetProperty("redactedContent", out _))
                    {
                        content.Add(new GeneratedReasoning(string.Empty));
                    }
                }

                if (part.TryGetProperty("toolUse", out var toolUse) && toolUse.ValueKind == JsonValueKind.Object)
                {
                    var name = toolUse.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "tool" : "tool";
                    if (usesJsonTool && string.Equals(name, JsonToolName, StringComparison.Ordinal))
                    {
                        jsonTool = true;
                        var json = toolUse.TryGetProperty("input", out var jsonInput) ? jsonInput.GetRawText() : "{}";
                        content.Add(new GeneratedText(json));
                        continue;
                    }

                    var id = toolUse.TryGetProperty("toolUseId", out var idElement) ? idElement.GetString() ?? "tool" : "tool";
                    var input = toolUse.TryGetProperty("input", out var inputElement) ? inputElement.GetRawText() : "{}";
                    content.Add(new GeneratedToolCall(AmazonBedrockToolIds.Normalize(id, isMistral), name, input));
                }
            }
        }

        var raw = root.TryGetProperty("stopReason", out var stop) ? stop.GetString() : "end_turn";
        var mapped = raw;
        if (jsonTool && string.Equals(raw, "tool_use", StringComparison.Ordinal))
        {
            mapped = "stop";
        }
        else if (string.Equals(raw, "guardrail_intervened", StringComparison.Ordinal))
        {
            mapped = "content-filter";
        }

        JsonElement? usageElement = root.TryGetProperty("usage", out var usageProperty) ? usageProperty : null;
        var usage = AmazonBedrockUsage.Convert(usageElement);
        string? responseId = null;
        if (headers.TryGetValue("x-amzn-requestid", out var requestId))
        {
            responseId = requestId;
        }

        JsonElement? providerMetadata = null;
        if (jsonTool)
        {
            string? stopSequence = null;
            if (root.TryGetProperty("additionalModelResponseFields", out var extra)
                && extra.ValueKind == JsonValueKind.Object
                && extra.TryGetProperty("delta", out var delta)
                && delta.ValueKind == JsonValueKind.Object
                && delta.TryGetProperty("stop_sequence", out var stopValue)
                && stopValue.ValueKind == JsonValueKind.String)
            {
                stopSequence = stopValue.GetString();
            }

            var payload = new JsonObject { ["isJsonResponseFromTool"] = true, ["stopSequence"] = stopSequence };
            var metadata = new JsonObject { ["amazonBedrock"] = payload, ["bedrock"] = payload.DeepClone() };
            using var document = JsonDocument.Parse(metadata.ToJsonString());
            providerMetadata = document.RootElement.Clone();
        }

        return new LanguageModelGenerateResult(content, FinishReasons.Parse(mapped), usage, raw, warnings, responseId, providerMetadata, responseHeaders: headers);
    }

    private static Dictionary<string, string> CopyHeaders(HttpResponseMessage response)
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
