// Copyright 2023 Vercel, Inc.
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

    internal Uri ConverseUri(string modelId)
    {
        return new Uri("https://bedrock-runtime." + Options.Region + ".amazonaws.com/model/" + Uri.EscapeDataString(modelId) + "/converse");
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

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var body = Build(options).ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, _provider.ConverseUri(ModelId))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        Sign(request, Encoding.UTF8.GetBytes(body));
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw ProviderHttp.MapStatus((int)response.StatusCode, text);
        }

        using var document = JsonDocument.Parse(text);
        return Parse(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        foreach (var part in result.Content)
        {
            if (part is GeneratedToolCall call)
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

    private JsonObject Build(LanguageModelCallOptions options)
    {
        var messages = new JsonArray();
        JsonArray? system = null;
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage systemMessage)
            {
                system ??= new JsonArray();
                system.Add(new JsonObject { ["text"] = systemMessage.Content });
            }
            else if (message is UserModelMessage user)
            {
                var content = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        content.Add(new JsonObject { ["text"] = text.Text });
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = content });
            }
            else if (message is AssistantModelMessage assistant)
            {
                var content = new JsonArray();
                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    content.Add(new JsonObject { ["text"] = assistant.Text });
                }

                messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = content });
            }
        }

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

        if (inference.Count > 0)
        {
            body["inferenceConfig"] = inference;
        }

        if (options.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["toolSpec"] = new JsonObject
                    {
                        ["name"] = tool.Name,
                        ["description"] = tool.Description,
                        ["inputSchema"] = new JsonObject { ["json"] = JsonNode.Parse(tool.InputSchema.GetRawText()) },
                    },
                });
            }

            body["toolConfig"] = new JsonObject { ["tools"] = tools };
        }

        return body;
    }

    private static LanguageModelGenerateResult Parse(JsonElement root)
    {
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("output", out var output) && output.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text))
                {
                    content.Add(new GeneratedText(text.GetString() ?? string.Empty));
                }
            }
        }

        var raw = root.TryGetProperty("stopReason", out var stop) ? stop.GetString() : "end_turn";
        var usage = LanguageModelUsage.Empty;
        if (root.TryGetProperty("usage", out var usageElement))
        {
            usage = new LanguageModelUsage(
                usageElement.TryGetProperty("inputTokens", out var input) ? input.GetInt32() : null,
                usageElement.TryGetProperty("outputTokens", out var outputTokens) ? outputTokens.GetInt32() : null,
                usageElement.TryGetProperty("totalTokens", out var total) ? total.GetInt32() : null);
        }

        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), usage, raw);
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
