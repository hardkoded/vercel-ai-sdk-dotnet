// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>The <c>deepseek</c> provider options of a Responses call. Maps to <c>deepseekLanguageModelResponsesOptions</c>.</summary>
internal sealed class DeepSeekResponsesOptions
{
    private static readonly string[] ThinkingTypes = { "enabled", "disabled" };
    private static readonly string[] ReasoningEfforts = { "low", "high", "max" };
    private static readonly Regex UserIdPattern = new Regex("^[a-zA-Z0-9_-]+\\z", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary><c>enabled</c> or <c>disabled</c>, or null when thinking is left to DeepSeek.</summary>
    public string? ThinkingType { get; private set; }

    public string? ReasoningEffort { get; private set; }

    public int? TopLogprobs { get; private set; }

    public string? UserId { get; private set; }

    /// <summary>Reads and validates the options under <paramref name="providerOptions"/>[<paramref name="providerOptionsName"/>].</summary>
    /// <exception cref="InvalidArgumentError">An option has the wrong type or value.</exception>
    public static DeepSeekResponsesOptions Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions, string providerOptionsName)
    {
        var options = new DeepSeekResponsesOptions();
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
                        options.ThinkingType = OneOf(scope + ".thinking.type", type, ThinkingTypes);
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
