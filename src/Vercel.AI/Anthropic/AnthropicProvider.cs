// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

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

    /// <summary>Headers merged into every request.</summary>
    public Dictionary<string, string?> Headers { get; set; } = new Dictionary<string, string?>();
}

/// <summary>Anthropic Messages provider.</summary>
public class AnthropicProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "anthropic";

    /// <summary>Creates a provider.</summary>
    public AnthropicProvider(HttpClient httpClient, AnthropicOptions? options = null)
        : this(httpClient, options, null)
    {
    }

    /// <summary>Creates a provider with an explicit retry policy.</summary>
    internal AnthropicProvider(HttpClient httpClient, AnthropicOptions? options, RetryPolicy? retry)
        : base(ProviderName)
    {
        Options = options ?? new AnthropicOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)), retry);
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
        var prepared = AnthropicRequest.Prepare(ModelId, options, false);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "v1/messages"),
            AnthropicRequest.Serialize(prepared.Body),
            Headers(prepared, options),
            cancellationToken).ConfigureAwait(false);
        return AnthropicResponse.Parse(response.Body, response.Headers, prepared);
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
        var prepared = AnthropicRequest.Prepare(ModelId, options, true);
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "v1/messages"),
            AnthropicRequest.Serialize(prepared.Body),
            Headers(prepared, options),
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

    private Dictionary<string, string?> Headers(AnthropicPreparedRequest prepared, LanguageModelCallOptions options)
    {
        var headers = _provider.Headers();
        foreach (var pair in _provider.Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        if (prepared.Betas.Count > 0)
        {
            headers["anthropic-beta"] = string.Join(",", prepared.Betas);
        }

        return headers;
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
