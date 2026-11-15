// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>The <c>deepseek</c> provider options of a Responses call. Maps to <c>DeepSeekLanguageModelResponsesOptions</c>.</summary>
public sealed class DeepSeekLanguageModelResponsesOptions
{
    private static readonly string[] ThinkingTypes = { "enabled", "disabled" };
    private static readonly string[] ReasoningEfforts = { "low", "high", "max" };
    private static readonly Regex UserIdPattern = new Regex("^[a-zA-Z0-9_-]+\\z", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>Controls whether thinking mode is enabled. Thinking is enabled by default.</summary>
    public DeepSeekThinkingOptions? Thinking { get; set; }

    /// <summary>Controls the thinking strength when thinking is enabled: <c>low</c>, <c>high</c> or <c>max</c>.</summary>
    public string? ReasoningEffort { get; set; }

    /// <summary>
    /// Number of most likely tokens to return at each token position (0 to 20).
    /// Log probabilities are returned in <c>providerMetadata.deepseek.logprobs</c>.
    /// </summary>
    public int? TopLogprobs { get; set; }

    /// <summary>
    /// An opaque identifier for the end user. DeepSeek uses it for content-safety tracing and request isolation.
    /// It must contain only ASCII letters, numbers, underscores, and hyphens, and be at most 512 characters long.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Validates the options and returns them as call provider options under <paramref name="providerOptionsName"/>
    /// (<c>deepseek</c> by default), ready for <c>LanguageModelCallOptions.ProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentError">An option has the wrong value.</exception>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = DeepSeekProvider.ProviderId)
    {
        var json = new System.Text.Json.Nodes.JsonObject();
        if (Thinking?.Type is { } type)
        {
            json["thinking"] = new System.Text.Json.Nodes.JsonObject { ["type"] = type };
        }

        if (ReasoningEffort != null)
        {
            json["reasoningEffort"] = ReasoningEffort;
        }

        if (TopLogprobs.HasValue)
        {
            json["topLogprobs"] = TopLogprobs.Value;
        }

        if (UserId != null)
        {
            json["userId"] = UserId;
        }

        var element = JsonSerializer.SerializeToElement(json);
        var result = new Dictionary<string, JsonElement> { [providerOptionsName] = element };
        Parse(result, providerOptionsName);
        return result;
    }

    /// <summary>Reads and validates the options under <paramref name="providerOptions"/>[<paramref name="providerOptionsName"/>].</summary>
    /// <exception cref="InvalidArgumentError">An option has the wrong type or value.</exception>
    internal static DeepSeekLanguageModelResponsesOptions Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions, string providerOptionsName)
    {
        var options = new DeepSeekLanguageModelResponsesOptions();
        if (providerOptions == null || !providerOptions.TryGetValue(providerOptionsName, out var source) || source.ValueKind == JsonValueKind.Null)
        {
            return options;
        }

        var scope = "providerOptions." + providerOptionsName;
        if (source.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentError(scope, null, "expected an object.");
        }

        foreach (var property in source.EnumerateObject())
        {
            var value = property.Value;
            if (value.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            switch (property.Name)
            {
                case "thinking":
                    if (value.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidArgumentError(scope + ".thinking", null, "expected an object.");
                    }

                    if (value.TryGetProperty("type", out var type) && type.ValueKind != JsonValueKind.Null)
                    {
                        (options.Thinking ??= new DeepSeekThinkingOptions()).Type = OneOf(scope + ".thinking.type", type, ThinkingTypes);
                    }

                    break;
                case "reasoningEffort":
                    options.ReasoningEffort = OneOf(scope + ".reasoningEffort", value, ReasoningEfforts);
                    break;
                case "topLogprobs":
                    if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var top) || top < 0 || top > 20)
                    {
                        throw new InvalidArgumentError(scope + ".topLogprobs", null, "expected an integer between 0 and 20.");
                    }

                    options.TopLogprobs = top;
                    break;
                case "userId":
                    var userId = value.ValueKind == JsonValueKind.String ? value.GetString()! : throw new InvalidArgumentError(scope + ".userId", null, "expected a string.");
                    if (!UserIdPattern.IsMatch(userId))
                    {
                        throw new InvalidArgumentError(scope + ".userId", null, "userId must match /^[a-zA-Z0-9_-]+$/");
                    }

                    if (userId.Length > 512)
                    {
                        throw new InvalidArgumentError(scope + ".userId", null, "userId must be at most 512 characters long");
                    }

                    options.UserId = userId;
                    break;
            }
        }

        return options;
    }

    private static string OneOf(string name, JsonElement value, string[] allowed)
    {
        var text = value.ValueKind == JsonValueKind.String ? value.GetString()! : null;
        if (text == null || Array.IndexOf(allowed, text) < 0)
        {
            throw new InvalidArgumentError(name, null, "expected one of " + string.Join(", ", allowed) + ".");
        }

        return text;
    }
}

/// <summary>The <c>thinking</c> option of <see cref="DeepSeekLanguageModelResponsesOptions"/>.</summary>
public sealed class DeepSeekThinkingOptions
{
    /// <summary><c>enabled</c> or <c>disabled</c>.</summary>
    public string? Type { get; set; }
}
