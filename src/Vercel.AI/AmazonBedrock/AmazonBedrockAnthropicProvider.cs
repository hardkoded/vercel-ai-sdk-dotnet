// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Settings for Anthropic Messages on Amazon Bedrock InvokeModel.</summary>
public sealed class AmazonBedrockAnthropicOptions
{
    /// <summary>AWS region. Defaults to <c>us-east-1</c>.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Bearer token. When set, SigV4 is not used. Falls back to <c>AWS_BEARER_TOKEN_BEDROCK</c>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Access key. Falls back to <c>AWS_ACCESS_KEY_ID</c>.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Secret key. Falls back to <c>AWS_SECRET_ACCESS_KEY</c>.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Session token. Falls back to <c>AWS_SESSION_TOKEN</c>.</summary>
    public string? SessionToken { get; set; }

    /// <summary>Runtime origin. When empty, the regional Bedrock Runtime host is used.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Clock used for signing.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }
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

    /// <summary>
    /// Drops <c>model</c> and <c>stream</c>, keeps a tool choice type and name, and sets
    /// <c>anthropic_version</c> to <c>bedrock-2023-05-31</c>.
    /// </summary>
    public static JsonObject Transform(JsonObject source)
    {
        var result = (JsonObject)(JsonNode.Parse(source.ToJsonString()) ?? new JsonObject());
        result.Remove("model");
        result.Remove("stream");
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

        result["anthropic_version"] = "bedrock-2023-05-31";
        return result;
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

    /// <summary>Provider specification version.</summary>
    public string SpecificationVersion => "v4";

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

    /// <summary>Regional Bedrock Runtime origin for these options.</summary>
    public string ResolveBaseUrl()
    {
        return AmazonBedrockEndpoints.Resolve(Options.BaseUrl, Options.Region, "bedrock-runtime");
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new AmazonBedrockAnthropicLanguageModel(this, modelId);
    }
}

/// <summary>Anthropic language model invoked through Bedrock.</summary>
public sealed class AmazonBedrockAnthropicLanguageModel : ILanguageModel
{
    private readonly AmazonBedrockAnthropicProvider _provider;

    /// <summary>Creates a model.</summary>
    public AmazonBedrockAnthropicLanguageModel(AmazonBedrockAnthropicProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AmazonBedrockAnthropicProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var body = Build(options, streaming: false);
        return await SendAsync(body, streaming: false, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await SendAsync(Build(options, streaming: true), streaming: true, cancellationToken).ConfigureAwait(false);
        if (result.Text.Length > 0)
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private JsonObject Build(LanguageModelCallOptions options, bool streaming)
    {
        var messages = new JsonArray();
        var system = new StringBuilder();
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage systemMessage)
            {
                if (system.Length > 0)
                {
                    system.Append('\n');
                }

                system.Append(systemMessage.Content);
            }
            else if (message is UserModelMessage user)
            {
                var content = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = content });
            }
        }

        var raw = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = messages,
            ["max_tokens"] = options.MaxOutputTokens ?? 4096,
        };
        if (streaming)
        {
            raw["stream"] = true;
        }

        if (system.Length > 0)
        {
            raw["system"] = system.ToString();
        }

        return AmazonBedrockAnthropicRequests.Transform(raw);
    }

    private async Task<LanguageModelGenerateResult> SendAsync(JsonObject body, bool streaming, CancellationToken cancellationToken)
    {
        var json = body.ToJsonString();
        var url = AmazonBedrockAnthropicRequests.BuildUrl(_provider.ResolveBaseUrl(), ModelId, streaming);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        var apiKey = _provider.Options.ApiKey ?? Environment.GetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK");
        AmazonBedrockSigner.Apply(
            request,
            Encoding.UTF8.GetBytes(json),
            _provider.Options.Region,
            "bedrock",
            apiKey,
            _provider.Options.AccessKeyId,
            _provider.Options.SecretAccessKey,
            _provider.Options.SessionToken,
            _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var anthropic = AmazonBedrockAnthropicRequests.ToAnthropicErrorBody(text);
            throw AmazonBedrockErrors.Create((int)response.StatusCode, anthropic);
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text);
        return Parse(document.RootElement);
    }

    private static LanguageModelGenerateResult Parse(JsonElement root)
    {
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                var type = part.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
                if (type == "text")
                {
                    content.Add(new GeneratedText(part.TryGetProperty("text", out var text) ? text.GetString() ?? string.Empty : string.Empty));
                }
            }
        }

        var raw = root.TryGetProperty("stop_reason", out var stop) ? stop.GetString() : "end_turn";
        int? input = null;
        int? output = null;
        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
        {
            if (usage.TryGetProperty("input_tokens", out var inputTokens) && inputTokens.ValueKind == JsonValueKind.Number)
            {
                input = inputTokens.GetInt32();
            }

            if (usage.TryGetProperty("output_tokens", out var outputTokens) && outputTokens.ValueKind == JsonValueKind.Number)
            {
                output = outputTokens.GetInt32();
            }
        }

        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), new LanguageModelUsage(input, output, null), raw);
    }
}
