// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>Whether a Grok model accepts <c>reasoning.effort</c>.</summary>
public static class XaiReasoning
{
    private static readonly Regex ModelsWithoutEffort = new Regex(
        "^grok-4\\.20(-\\d{4})?-(non-)?reasoning$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// False for <c>grok-4.20</c> reasoning and non-reasoning ids, including dated variants.
    /// </summary>
    public static bool SupportsReasoningEffort(string modelId)
    {
        return modelId == null || !ModelsWithoutEffort.IsMatch(modelId);
    }
}

/// <summary>xAI Responses language model. Posts to <c>{base}/responses</c>.</summary>
public sealed class XaiResponsesLanguageModel : ILanguageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a responses model.</summary>
    public XaiResponsesLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <summary>Provider id <c>xai.responses</c>.</summary>
    public string Provider => "xai.responses";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = BuildInput(options.Prompt),
        };
        if (options.MaxOutputTokens != null)
        {
            body["max_output_tokens"] = options.MaxOutputTokens.Value;
        }

        if (XaiReasoning.SupportsReasoningEffort(ModelId))
        {
            var effort = MapEffort(options.Reasoning);
            if (effort != null)
            {
                body["reasoning"] = new JsonObject { ["effort"] = effort };
            }
        }
        else if (IsCustom(options.Reasoning))
        {
            warnings.Add(new CallWarning(
                "unsupported",
                "reasoning | reasoning \"" + options.Reasoning + "\" is not supported by this model."));
        }

        var headers = _provider.CreateHeaders();
        if (!headers.ContainsKey("User-Agent"))
        {
            headers["User-Agent"] = XaiProvider.UserAgent;
        }

        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            body.ToJsonString(),
            headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "{}" : http.Body);
        var content = new List<GeneratedContent>();
        var text = ReadText(document.RootElement);
        if (!string.IsNullOrEmpty(text))
        {
            content.Add(new GeneratedText(text!));
        }

        int? input = JsonValues.GetInt(document.RootElement.TryGetProperty("usage", out var usage) ? usage : default, "input_tokens");
        int? output = document.RootElement.TryGetProperty("usage", out var usageAgain)
            ? JsonValues.GetInt(usageAgain, "output_tokens")
            : null;
        return new LanguageModelGenerateResult(
            content,
            FinishReason.Stop,
            new LanguageModelUsage(input, output, null),
            "stop",
            warnings,
            JsonValues.GetString(document.RootElement, "id"),
            null,
            http.Body,
            ModelId,
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
            yield return new TextStartStreamPart("txt-0");
            yield return new TextDeltaStreamPart("txt-0", result.Text);
            yield return new TextEndStreamPart("txt-0");
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private static JsonArray BuildInput(IReadOnlyList<ModelMessage> prompt)
    {
        var input = new JsonArray();
        if (prompt == null)
        {
            return input;
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

                input.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = text,
                });
            }
        }

        return input;
    }

    private static string? ReadText(JsonElement root)
    {
        var outputText = JsonValues.GetString(root, "output_text");
        if (!string.IsNullOrEmpty(outputText))
        {
            return outputText;
        }

        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array)
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
                {
                    var content = JsonValues.GetString(message, "content");
                    if (!string.IsNullOrEmpty(content))
                    {
                        return content;
                    }
                }
            }
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var text = string.Empty;
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in content.EnumerateArray())
            {
                var partText = JsonValues.GetString(part, "text");
                if (!string.IsNullOrEmpty(partText))
                {
                    text += partText;
                }
            }
        }

        return text;
    }

    private static bool IsCustom(string? reasoning)
    {
        return reasoning != null && !string.Equals(reasoning, "provider-default", StringComparison.Ordinal);
    }

    private static string? MapEffort(string? reasoning)
    {
        if (!IsCustom(reasoning))
        {
            return null;
        }

        switch (reasoning)
        {
            case "none":
                return "none";
            case "minimal":
            case "low":
                return "low";
            case "medium":
                return "medium";
            case "high":
                return "high";
            case "xhigh":
                return "xhigh";
            default:
                return null;
        }
    }
}
