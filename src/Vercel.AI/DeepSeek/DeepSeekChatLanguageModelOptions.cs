// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>The <c>deepseek</c> provider options of a chat call. Maps to <c>DeepSeekLanguageModelChatOptions</c>.</summary>
public sealed class DeepSeekLanguageModelChatOptions
{
    private static readonly string[] ThinkingTypes = ["adaptive", "enabled", "disabled"];
    private static readonly string[] ReasoningEfforts = ["low", "medium", "high", "xhigh", "max"];
    private static readonly Regex UserIdPattern = new(
        "^[a-zA-Z0-9_-]+\\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    /// <summary>Whether to return log probabilities for generated tokens.</summary>
    public bool? Logprobs { get; set; }

    /// <summary>
    /// Number of most likely tokens to return at each token position (0 to 20). Setting this option automatically enables
    /// <see cref="Logprobs"/>.
    /// </summary>
    public int? TopLogprobs { get; set; }

    /// <summary>
    /// An opaque identifier for the end user. DeepSeek uses it for content-safety tracing and request isolation.
    /// It must contain only ASCII letters, numbers, underscores, and hyphens, and be at most 512 characters long.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>Controls whether thinking mode is enabled. Thinking is enabled by default.</summary>
    public DeepSeekThinkingOptions? Thinking { get; set; }

    /// <summary>Controls the thinking strength for DeepSeek V4 reasoning models: <c>low</c>, <c>high</c> or <c>max</c>.</summary>
    public string? ReasoningEffort { get; set; }

    /// <summary>
    /// Whether to use strict JSON schema validation for structured outputs. Only applies when the serving endpoint supports
    /// JSON schema response formats (for example Azure). Defaults to <see langword="true"/>.
    /// </summary>
    public bool? StrictJsonSchema { get; set; }

    /// <summary>
    /// Validates the options and returns them as call provider options under <paramref name="providerOptionsName"/>
    /// (<c>deepseek</c> by default), ready for <c>LanguageModelCallOptions.ProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentError">An option has the wrong value.</exception>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = DeepSeekProvider.ProviderId
    )
    {
        var json = new JsonObject();
        if (Logprobs is { } logprobs)
        {
            json["logprobs"] = logprobs;
        }

        if (TopLogprobs is { } topLogprobs)
        {
            json["topLogprobs"] = topLogprobs;
        }

        if (UserId != null)
        {
            json["userId"] = UserId;
        }

        if (Thinking?.Type is { } type)
        {
            json["thinking"] = new JsonObject { ["type"] = type };
        }

        if (ReasoningEffort != null)
        {
            json["reasoningEffort"] = ReasoningEffort;
        }

        if (StrictJsonSchema is { } strict)
        {
            json["strictJsonSchema"] = strict;
        }

        var result = new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
        Parse(result, providerOptionsName);
        return result;
    }

    /// <summary>
    /// The error <c>parseProviderOptions</c> throws when the options do not match the schema: an <see cref="InvalidArgumentError"/>
    /// for <c>providerOptions</c>, with the validation failure as its cause.
    /// </summary>
    internal static InvalidArgumentError InvalidProviderOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName,
        InvalidArgumentError cause
    )
    {
        return new InvalidArgumentError(
            "providerOptions",
            providerOptions != null
            && providerOptions.TryGetValue(providerOptionsName, out var value)
                ? value.GetRawText()
                : null,
            "invalid " + providerOptionsName + " provider options",
            cause
        );
    }

    /// <summary>
    /// Reads and validates the options under <paramref name="providerOptions"/>[<paramref name="providerOptionsName"/>].
    /// <c>adaptive</c> thinking and the <c>medium</c> and <c>xhigh</c> efforts are accepted for backwards compatibility.
    /// </summary>
    internal static DeepSeekLanguageModelChatOptions Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        try
        {
            return Validate(providerOptions, providerOptionsName);
        }
        catch (InvalidArgumentError error)
        {
            throw InvalidProviderOptions(providerOptions, providerOptionsName, error);
        }
    }

    private static DeepSeekLanguageModelChatOptions Validate(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        var options = new DeepSeekLanguageModelChatOptions();
        if (
            providerOptions == null
            || !providerOptions.TryGetValue(providerOptionsName, out var source)
            || source.ValueKind == JsonValueKind.Null
        )
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
                case "logprobs":
                    options.Logprobs = Boolean(scope + ".logprobs", value);
                    break;
                case "topLogprobs":
                    if (
                        value.ValueKind != JsonValueKind.Number
                        || !value.TryGetInt32(out var top)
                        || top < 0
                        || top > 20
                    )
                    {
                        throw new InvalidArgumentError(
                            scope + ".topLogprobs",
                            null,
                            "expected an integer between 0 and 20."
                        );
                    }

                    options.TopLogprobs = top;
                    break;
                case "userId":
                    var userId =
                        value.ValueKind == JsonValueKind.String
                            ? value.GetString()!
                            : throw new InvalidArgumentError(
                                scope + ".userId",
                                null,
                                "expected a string."
                            );
                    if (!UserIdPattern.IsMatch(userId))
                    {
                        throw new InvalidArgumentError(
                            scope + ".userId",
                            null,
                            "userId must match /^[a-zA-Z0-9_-]+$/"
                        );
                    }

                    if (userId.Length > 512)
                    {
                        throw new InvalidArgumentError(
                            scope + ".userId",
                            null,
                            "userId must be at most 512 characters long"
                        );
                    }

                    options.UserId = userId;
                    break;
                case "thinking":
                    if (value.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidArgumentError(
                            scope + ".thinking",
                            null,
                            "expected an object."
                        );
                    }

                    if (
                        value.TryGetProperty("type", out var type)
                        && type.ValueKind != JsonValueKind.Null
                    )
                    {
                        var thinkingType = OneOf(scope + ".thinking.type", type, ThinkingTypes);
                        options.Thinking = new DeepSeekThinkingOptions { Type = thinkingType };
                    }
                    else
                    {
                        options.Thinking = new DeepSeekThinkingOptions();
                    }

                    break;
                case "reasoningEffort":
                    options.ReasoningEffort = OneOf(
                        scope + ".reasoningEffort",
                        value,
                        ReasoningEfforts
                    );
                    break;
                case "strictJsonSchema":
                    options.StrictJsonSchema = Boolean(scope + ".strictJsonSchema", value);
                    break;
            }
        }

        return options;
    }

    private static bool Boolean(string name, JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new InvalidArgumentError(name, null, "expected a boolean."),
        };
    }

    private static string OneOf(string name, JsonElement value, string[] allowed)
    {
        var text = value.ValueKind == JsonValueKind.String ? value.GetString()! : null;
        if (text == null || Array.IndexOf(allowed, text) < 0)
        {
            throw new InvalidArgumentError(
                name,
                null,
                "expected one of " + string.Join(", ", allowed) + "."
            );
        }

        return text;
    }
}

/// <summary>The <c>deepseek</c> provider options of a chat message. Maps to <c>DeepSeekMessageProviderOptions</c>.</summary>
public class DeepSeekMessageProviderOptions
{
    /// <summary>The name of the participant represented by the message. Supported on system, user, and assistant messages.</summary>
    public string? Name { get; set; }

    /// <summary>Returns the options as message provider options under <paramref name="providerOptionsName"/>.</summary>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = DeepSeekProvider.ProviderId
    )
    {
        var json = new JsonObject();
        if (Name != null)
        {
            json["name"] = Name;
        }

        AddTo(json);
        return new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
    }

    private protected virtual void AddTo(JsonObject json) { }

    /// <summary>
    /// Reads and validates the options of a message. The assistant schema extends the common one, so one parse validates
    /// names for every role and the assistant-only prefix option.
    /// </summary>
    internal static DeepSeekAssistantMessageProviderOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        try
        {
            return Validate(providerOptions, providerOptionsName);
        }
        catch (InvalidArgumentError error)
        {
            throw DeepSeekLanguageModelChatOptions.InvalidProviderOptions(
                providerOptions,
                providerOptionsName,
                error
            );
        }
    }

    private static DeepSeekAssistantMessageProviderOptions? Validate(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue(providerOptionsName, out var source)
            || source.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (source.ValueKind != JsonValueKind.Object)
        {
            throw DeepSeekFilePartProviderOptions.InvalidOptions(providerOptionsName);
        }

        var options = new DeepSeekAssistantMessageProviderOptions();
        foreach (var property in source.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            switch (property.Name)
            {
                case "name":
                    options.Name =
                        property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString()
                            : throw DeepSeekFilePartProviderOptions.InvalidOptions(
                                providerOptionsName
                            );
                    break;
                case "prefix":
                    if (property.Value.ValueKind != JsonValueKind.True)
                    {
                        throw DeepSeekFilePartProviderOptions.InvalidOptions(providerOptionsName);
                    }

                    options.Prefix = true;
                    break;
            }
        }

        return options;
    }
}

/// <summary>The <c>deepseek</c> provider options of an assistant message. Maps to <c>DeepSeekAssistantMessageProviderOptions</c>.</summary>
public sealed class DeepSeekAssistantMessageProviderOptions : DeepSeekMessageProviderOptions
{
    /// <summary>
    /// Whether the assistant message content is a prefix that DeepSeek should continue. This beta feature is only supported
    /// on the final assistant message when using a beta base URL.
    /// </summary>
    public bool? Prefix { get; set; }

    private protected override void AddTo(JsonObject json)
    {
        if (Prefix == true)
        {
            json["prefix"] = true;
        }
    }
}

/// <summary>The <c>deepseek</c> provider options of an image part. Maps to <c>DeepSeekFilePartProviderOptions</c>.</summary>
public sealed class DeepSeekFilePartProviderOptions
{
    private static readonly string[] ImageDetails = ["low", "high", "original", "auto"];

    /// <summary>Controls how DeepSeek processes an image sent as an <c>image_url</c> part: <c>low</c>, <c>high</c>, <c>original</c> or <c>auto</c>.</summary>
    public string? ImageDetail { get; set; }

    /// <summary>
    /// Sends inline image data as a DeepSeek <c>file</c> part using <c>file_data</c> instead of an <c>image_url</c> data URL.
    /// The file part's filename is preserved. It only applies to inline image data and cannot be combined with
    /// <see cref="ImageDetail"/>.
    /// </summary>
    public bool? FileData { get; set; }

    /// <summary>Returns the options as file part provider options under <paramref name="providerOptionsName"/>.</summary>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = DeepSeekProvider.ProviderId
    )
    {
        var json = new JsonObject();
        if (ImageDetail != null)
        {
            json["imageDetail"] = ImageDetail;
        }

        if (FileData == true)
        {
            json["fileData"] = true;
        }

        var result = new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
        Parse(result, providerOptionsName);
        return result;
    }

    internal static DeepSeekFilePartProviderOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        try
        {
            return Validate(providerOptions, providerOptionsName);
        }
        catch (InvalidArgumentError error)
        {
            throw DeepSeekLanguageModelChatOptions.InvalidProviderOptions(
                providerOptions,
                providerOptionsName,
                error
            );
        }
    }

    private static DeepSeekFilePartProviderOptions? Validate(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string providerOptionsName
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue(providerOptionsName, out var options)
            || options.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (options.ValueKind != JsonValueKind.Object)
        {
            throw InvalidOptions(providerOptionsName);
        }

        var result = new DeepSeekFilePartProviderOptions();
        if (
            options.TryGetProperty("imageDetail", out var detailValue)
            && detailValue.ValueKind != JsonValueKind.Null
        )
        {
            var detail =
                detailValue.ValueKind == JsonValueKind.String ? detailValue.GetString() : null;
            if (detail == null || Array.IndexOf(ImageDetails, detail) < 0)
            {
                throw InvalidOptions(providerOptionsName);
            }

            result.ImageDetail = detail;
        }

        if (
            options.TryGetProperty("fileData", out var fileDataValue)
            && fileDataValue.ValueKind != JsonValueKind.Null
        )
        {
            if (fileDataValue.ValueKind != JsonValueKind.True)
            {
                throw InvalidOptions(providerOptionsName);
            }

            result.FileData = true;
        }

        return result;
    }

    internal static InvalidArgumentError InvalidOptions(string providerOptionsName)
    {
        return new InvalidArgumentError(
            "providerOptions",
            null,
            "invalid " + providerOptionsName + " provider options"
        );
    }
}
