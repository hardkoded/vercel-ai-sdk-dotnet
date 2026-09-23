// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Anthropic;

/// <summary>Anthropic Messages API settings.</summary>
public class AnthropicOptions
{
    /// <summary>API origin. Message calls append <c>/v1/messages</c>.</summary>
    public string BaseUrl { get; set; } = "https://api.anthropic.com";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable. Defaults to <c>ANTHROPIC_API_KEY</c>.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "ANTHROPIC_API_KEY";

    /// <summary><c>anthropic-version</c> header.</summary>
    public string Version { get; set; } = "2023-06-01";
}

/// <summary>Anthropic Messages provider.</summary>
public class AnthropicProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "anthropic";

    /// <summary>Creates a provider.</summary>
    public AnthropicProvider(HttpClient httpClient, AnthropicOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new AnthropicOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public AnthropicOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static AnthropicProvider Create(AnthropicOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AnthropicProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new AnthropicLanguageModel(this, modelId);
    }

    internal Dictionary<string, string?> Headers()
    {
        return new Dictionary<string, string?>
        {
            ["x-api-key"] = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable),
            ["anthropic-version"] = Options.Version,
        };
    }
}

/// <summary>Anthropic Messages language model.</summary>
public sealed class AnthropicLanguageModel : ILanguageModel
{
    private readonly AnthropicProvider _provider;

    /// <summary>Creates a model.</summary>
    public AnthropicLanguageModel(AnthropicProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AnthropicProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "v1/messages"),
            Build(options, false).ToJsonString(),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false);
        return Parse(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var toolId = string.Empty;
        var toolName = string.Empty;
        var toolArgs = new StringBuilder();
        string? finish = null;
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "v1/messages"),
            Build(options, true).ToJsonString(),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false))
        {
            JsonObject? node;
            try
            {
                node = JsonNode.Parse(data) as JsonObject;
            }
            catch (JsonException)
            {
                continue;
            }

            var type = StringOf(node?["type"]);
            if (type == "content_block_start" && node?["content_block"] is JsonObject block && StringOf(block["type"]) == "tool_use")
            {
                toolId = StringOf(block["id"]) ?? string.Empty;
                toolName = StringOf(block["name"]) ?? string.Empty;
            }
            else if (type == "content_block_delta" && node?["delta"] is JsonObject delta)
            {
                var deltaType = StringOf(delta["type"]);
                if (deltaType == "text_delta")
                {
                    yield return new TextDeltaStreamPart("text", StringOf(delta["text"]) ?? string.Empty);
                }
                else if (deltaType == "input_json_delta")
                {
                    toolArgs.Append(StringOf(delta["partial_json"]) ?? string.Empty);
                }
            }
            else if (type == "message_delta" && node?["delta"] is JsonObject messageDelta)
            {
                finish = StringOf(messageDelta["stop_reason"]);
            }
        }

        if (toolName.Length > 0)
        {
            yield return new ToolCallStreamPart(toolId, toolName, toolArgs.ToString());
        }

        yield return new FinishStreamPart(FinishReasons.Parse(finish), LanguageModelUsage.Empty, finish);
    }

    private JsonObject Build(LanguageModelCallOptions options, bool stream)
    {
        var system = new StringBuilder();
        var messages = new JsonArray();
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
                var text = new StringBuilder();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        text.Append(textPart.Text);
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = text.ToString() });
            }
            else if (message is AssistantModelMessage assistant)
            {
                var content = new JsonArray();
                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = assistant.Text });
                }

                foreach (var call in assistant.ToolCalls)
                {
                    content.Add(new JsonObject
                    {
                        ["type"] = "tool_use",
                        ["id"] = call.ToolCallId,
                        ["name"] = call.ToolName,
                        ["input"] = JsonNode.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson),
                    });
                }

                messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = content });
            }
            else if (message is ToolModelMessage tool)
            {
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "tool_result",
                            ["tool_use_id"] = tool.ToolCallId,
                            ["content"] = tool.OutputJson,
                            ["is_error"] = tool.IsError,
                        },
                    },
                });
            }
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["max_tokens"] = options.MaxOutputTokens ?? 4096,
            ["messages"] = messages,
            ["stream"] = stream,
        };
        if (system.Length > 0)
        {
            body["system"] = system.ToString();
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["input_schema"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                });
            }

            body["tools"] = tools;
        }

        return body;
    }

    private static LanguageModelGenerateResult Parse(JsonElement root)
    {
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("content", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                var type = part.GetProperty("type").GetString();
                if (type == "text")
                {
                    content.Add(new GeneratedText(part.GetProperty("text").GetString() ?? string.Empty));
                }
                else if (type == "tool_use")
                {
                    content.Add(new GeneratedToolCall(
                        part.GetProperty("id").GetString() ?? "tool",
                        part.GetProperty("name").GetString() ?? string.Empty,
                        part.GetProperty("input").GetRawText()));
                }
            }
        }

        var raw = root.TryGetProperty("stop_reason", out var stop) ? stop.GetString() : null;
        var usage = LanguageModelUsage.Empty;
        if (root.TryGetProperty("usage", out var usageElement))
        {
            usage = new LanguageModelUsage(
                usageElement.TryGetProperty("input_tokens", out var input) ? input.GetInt32() : null,
                usageElement.TryGetProperty("output_tokens", out var output) ? output.GetInt32() : null,
                null);
        }

        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), usage, raw, responseId: root.TryGetProperty("id", out var id) ? id.GetString() : null);
    }

    private static string? StringOf(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }
}

/// <summary>Anthropic on AWS. Same Messages body, with <c>ANTHROPIC_AWS_API_KEY</c> and a regional base URL.</summary>
public sealed class AnthropicAwsProvider : AnthropicProvider
{
    /// <summary>Creates an Anthropic on AWS provider.</summary>
    public AnthropicAwsProvider(HttpClient httpClient, AnthropicOptions? options = null)
        : base(httpClient, options ?? new AnthropicOptions
        {
            BaseUrl = "https://aws-external-anthropic.us-east-1.api.aws",
            ApiKeyEnvironmentVariable = "ANTHROPIC_AWS_API_KEY",
        })
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new AnthropicAwsProvider Create(AnthropicOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AnthropicAwsProvider(client, options);
    }
}

/// <summary>Registers Anthropic.</summary>
public static class AnthropicServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AnthropicProvider"/>.</summary>
    public static IServiceCollection AddAnthropic(this IServiceCollection services, Action<AnthropicOptions>? configure = null)
    {
        services.AddHttpClient(AnthropicProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new AnthropicOptions();
            configure?.Invoke(options);
            return new AnthropicProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AnthropicProvider.ProviderName), options);
        });
        return services;
    }

    /// <summary>Adds <see cref="AnthropicAwsProvider"/>.</summary>
    public static IServiceCollection AddAnthropicAws(this IServiceCollection services, Action<AnthropicOptions>? configure = null)
    {
        services.AddHttpClient("anthropic-aws");
        services.AddSingleton(sp =>
        {
            var options = new AnthropicOptions
            {
                BaseUrl = "https://aws-external-anthropic.us-east-1.api.aws",
                ApiKeyEnvironmentVariable = "ANTHROPIC_AWS_API_KEY",
            };
            configure?.Invoke(options);
            return new AnthropicAwsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient("anthropic-aws"), options);
        });
        return services;
    }
}
