// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Settings for the OpenAI-compatible Bedrock Mantle endpoint.</summary>
public sealed class BedrockMantleOptions
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

    /// <summary>Origin including the version path. When empty, the regional Mantle host is used.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Clock used for signing.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }
}

/// <summary>Mantle URL rules. Some model ids are served under <c>/openai/v1</c>.</summary>
public static class BedrockMantleRoutes
{
    /// <summary>True for Mantle models served on the OpenAI route, excluding <c>openai.gpt-oss-</c>.</summary>
    public static bool UsesOpenAiRoute(string modelId)
    {
        if (modelId == null)
        {
            return false;
        }

        if (modelId.StartsWith("openai.gpt-", StringComparison.Ordinal) && !modelId.StartsWith("openai.gpt-oss-", StringComparison.Ordinal))
        {
            return true;
        }

        if (modelId.StartsWith("google.gemma-4", StringComparison.Ordinal) || modelId.StartsWith("xai.", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    /// <summary>Joins the Mantle origin and <paramref name="path"/> for <paramref name="modelId"/>.</summary>
    public static string Url(string region, string? baseUrl, string modelId, string path)
    {
        string origin;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            origin = baseUrl!.TrimEnd('/');
        }
        else
        {
            if (!HostnameParts.IsValidHostnamePart(region))
            {
                throw new ArgumentException("An AWS region must be a single DNS label.", nameof(region));
            }

            origin = "https://bedrock-mantle." + region + ".api.aws/" + (UsesOpenAiRoute(modelId) ? "openai/v1" : "v1");
        }

        var relative = path.StartsWith("/", StringComparison.Ordinal) ? path : "/" + path;
        return origin + relative;
    }
}

/// <summary>Bedrock Mantle provider. Chat calls use <c>/chat/completions</c> and are signed for <c>bedrock-mantle</c>.</summary>
public sealed class BedrockMantleProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "bedrock-mantle.chat";

    /// <summary>Provider specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Creates a provider.</summary>
    public BedrockMantleProvider(HttpClient httpClient, BedrockMantleOptions? options = null)
        : base(ProviderId)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? new BedrockMantleOptions();
    }

    /// <summary>Options.</summary>
    public BedrockMantleOptions Options { get; }

    /// <summary>HTTP client that receives the signed request.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static BedrockMantleProvider Create(BedrockMantleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new BedrockMantleProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new BedrockMantleLanguageModel(this, modelId, responses: false);
    }

    /// <summary>Chat Completions model. Same route as <see cref="LanguageModel"/>.</summary>
    public ILanguageModel Chat(string modelId)
    {
        return LanguageModel(modelId);
    }

    /// <summary>Responses API model. The path is <c>/responses</c> on the same Mantle origin.</summary>
    public ILanguageModel Responses(string modelId)
    {
        return new BedrockMantleLanguageModel(this, modelId, responses: true);
    }
}

/// <summary>Mantle language model.</summary>
public sealed class BedrockMantleLanguageModel : ILanguageModel
{
    private readonly BedrockMantleProvider _provider;
    private readonly bool _responses;

    /// <summary>Creates a model.</summary>
    public BedrockMantleLanguageModel(BedrockMantleProvider provider, string modelId, bool responses)
    {
        _provider = provider;
        ModelId = modelId;
        _responses = responses;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _responses ? "bedrock-mantle.responses" : BedrockMantleProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var path = _responses ? "/responses" : "/chat/completions";
        var url = BedrockMantleRoutes.Url(_provider.Options.Region, _provider.Options.BaseUrl, ModelId, path);
        var body = Build(options);
        var json = body.ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        var apiKey = _provider.Options.ApiKey ?? Environment.GetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK");
        AmazonBedrockSigner.Apply(
            request,
            Encoding.UTF8.GetBytes(json),
            _provider.Options.Region,
            "bedrock-mantle",
            apiKey,
            _provider.Options.AccessKeyId,
            _provider.Options.SecretAccessKey,
            _provider.Options.SessionToken,
            _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw AmazonBedrockErrors.Create((int)response.StatusCode, text);
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text);
        return Parse(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (result.Text.Length > 0)
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private JsonObject Build(LanguageModelCallOptions options)
    {
        if (_responses)
        {
            var input = new JsonArray();
            foreach (var message in options.Prompt)
            {
                if (message is UserModelMessage user)
                {
                    var text = new StringBuilder();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart textPart)
                        {
                            text.Append(textPart.Text);
                        }
                    }

                    input.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = text.ToString(),
                    });
                }
            }

            return new JsonObject { ["model"] = ModelId, ["input"] = input };
        }

        var messages = new JsonArray();
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage system)
            {
                messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
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
        }

        var body = new JsonObject { ["model"] = ModelId, ["messages"] = messages };
        if (options.MaxOutputTokens is { } max)
        {
            body["max_tokens"] = max;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        return body;
    }

    private static LanguageModelGenerateResult Parse(JsonElement root)
    {
        var text = string.Empty;
        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array)
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                {
                    text = content.GetString() ?? string.Empty;
                    break;
                }
            }
        }
        else if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            text = outputText.GetString() ?? string.Empty;
        }

        var raw = "stop";
        if (root.TryGetProperty("choices", out var finishChoices) && finishChoices.GetArrayLength() > 0)
        {
            var first = finishChoices[0];
            if (first.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String)
            {
                raw = finish.GetString() ?? raw;
            }
        }

        return new LanguageModelGenerateResult(new GeneratedContent[] { new GeneratedText(text) }, FinishReasons.Parse(raw), LanguageModelUsage.Empty, raw);
    }
}
