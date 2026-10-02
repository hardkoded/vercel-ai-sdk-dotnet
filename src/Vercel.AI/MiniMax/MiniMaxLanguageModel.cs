// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.MiniMax;

/// <summary>
/// MiniMax chat model. Posts Anthropic Messages to <c>{chatBase}/messages</c>
/// with <c>x-api-key</c> and <c>anthropic-version</c>, and keeps bearer authorization.
/// </summary>
public sealed class MiniMaxLanguageModel : ILanguageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a MiniMax messages model.</summary>
    public MiniMaxLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <summary>Provider id <c>minimax.messages</c>.</summary>
    public string Provider => "minimax.messages";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["max_tokens"] = options.MaxOutputTokens ?? 4096,
            ["messages"] = Messages(options.Prompt),
        };
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("minimax", out var minimax)
            && minimax.ValueKind == JsonValueKind.Object
            && minimax.TryGetProperty("thinking", out var thinking)
            && thinking.ValueKind == JsonValueKind.Object)
        {
            body["thinking"] = JsonNode.Parse(thinking.GetRawText()) ?? new JsonObject();
        }

        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "messages"),
            body.ToJsonString(),
            Headers(options),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "{}" : http.Body);
        var content = ReadContent(document.RootElement);
        var stop = JsonValues.GetString(document.RootElement, "stop_reason");
        int? input = null;
        int? output = null;
        if (document.RootElement.TryGetProperty("usage", out var usage))
        {
            input = JsonValues.GetInt(usage, "input_tokens");
            output = JsonValues.GetInt(usage, "output_tokens");
        }

        return new LanguageModelGenerateResult(
            content,
            FinishReasons.Parse(string.IsNullOrEmpty(stop) ? "stop" : stop),
            new LanguageModelUsage(input, output, null),
            stop,
            Array.Empty<CallWarning>(),
            JsonValues.GetString(document.RootElement, "id"),
            null,
            http.Body,
            JsonValues.GetString(document.RootElement, "model") ?? ModelId,
            null,
            http.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        yield return new StreamStartStreamPart(result.Warnings);
        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("txt-0", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private Dictionary<string, string?> Headers(LanguageModelCallOptions options)
    {
        var headers = _provider.CreateHeaders();
        var key = headers.TryGetValue("Authorization", out var authorization) && authorization != null && authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            ? authorization.Substring("Bearer ".Length)
            : _provider.Options.ApiKey;
        headers["x-api-key"] = key;
        headers["anthropic-version"] = "2023-06-01";
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private static JsonArray Messages(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        if (prompt == null)
        {
            return messages;
        }

        foreach (var message in prompt)
        {
            if (message is UserModelMessage user)
            {
                var text = string.Empty;
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        text += textPart.Text;
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = text });
            }
        }

        return messages;
    }

    private static List<GeneratedContent> ReadContent(JsonElement root)
    {
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                var type = JsonValues.GetString(part, "type");
                if (string.Equals(type, "thinking", StringComparison.Ordinal))
                {
                    var thinking = JsonValues.GetString(part, "thinking");
                    if (!string.IsNullOrEmpty(thinking))
                    {
                        content.Add(new GeneratedReasoning(thinking!));
                    }
                }
                else if (string.Equals(type, "text", StringComparison.Ordinal))
                {
                    var text = JsonValues.GetString(part, "text");
                    if (!string.IsNullOrEmpty(text))
                    {
                        content.Add(new GeneratedText(text!));
                    }
                }
            }

            return content;
        }

        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
        {
            var message = choices[0].TryGetProperty("message", out var messageElement) ? messageElement : default;
            var text = message.ValueKind == JsonValueKind.Object ? JsonValues.GetString(message, "content") : null;
            if (!string.IsNullOrEmpty(text))
            {
                content.Add(new GeneratedText(text!));
            }
        }

        return content;
    }
}
