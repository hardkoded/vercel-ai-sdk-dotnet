// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Groq;

/// <summary>Provider options accepted by Groq chat completions.</summary>
public sealed class GroqChatOptions
{
    private static readonly string[] ReasoningEfforts = { "none", "default", "low", "medium", "high" };

    private static readonly string[] ReasoningFormats = { "parsed", "raw", "hidden" };

    private static readonly string[] ServiceTiers = { "on_demand", "performance", "flex", "auto" };

    /// <summary>Creates options. Unset members are omitted from the request.</summary>
    public GroqChatOptions()
    {
    }

    /// <summary><c>parsed</c>, <c>raw</c>, or <c>hidden</c>.</summary>
    public string? ReasoningFormat { get; set; }

    /// <summary><c>none</c>, <c>default</c>, <c>low</c>, <c>medium</c>, or <c>high</c>.</summary>
    public string? ReasoningEffort { get; set; }

    /// <summary>Whether the model may call tools in parallel.</summary>
    public bool? ParallelToolCalls { get; set; }

    /// <summary>End-user identifier.</summary>
    public string? User { get; set; }

    /// <summary>Whether a JSON schema is sent as <c>json_schema</c>. Defaults to true when omitted.</summary>
    public bool? StructuredOutputs { get; set; }

    /// <summary>Whether <c>json_schema.strict</c> is true. Defaults to true when omitted.</summary>
    public bool? StrictJsonSchema { get; set; }

    /// <summary><c>on_demand</c>, <c>performance</c>, <c>flex</c>, or <c>auto</c>.</summary>
    public string? ServiceTier { get; set; }

    /// <summary>Description copied into <c>response_format.json_schema.description</c>.</summary>
    public string? ResponseFormatDescription { get; set; }

    /// <summary>Parses a Groq provider-options object. Missing and null fields are omitted.</summary>
    public static GroqChatOptionsParseResult TryParse(JsonElement? element)
    {
        if (element is null || element.Value.ValueKind == JsonValueKind.Null || element.Value.ValueKind == JsonValueKind.Undefined)
        {
            return GroqChatOptionsParseResult.Ok(new GroqChatOptions());
        }

        if (element.Value.ValueKind != JsonValueKind.Object)
        {
            return GroqChatOptionsParseResult.Fail("Groq provider options must be an object.");
        }

        var options = new GroqChatOptions();
        foreach (var property in element.Value.EnumerateObject())
        {
            switch (property.Name)
            {
                case "reasoningEffort":
                    if (!TryOptionalString(property.Value, ReasoningEfforts, out var effort, out var effortError))
                    {
                        return GroqChatOptionsParseResult.Fail(effortError ?? "Invalid reasoningEffort.");
                    }

                    options.ReasoningEffort = effort;
                    break;
                case "reasoningFormat":
                    if (!TryOptionalString(property.Value, ReasoningFormats, out var format, out var formatError))
                    {
                        return GroqChatOptionsParseResult.Fail(formatError ?? "Invalid reasoningFormat.");
                    }

                    options.ReasoningFormat = format;
                    break;
                case "serviceTier":
                    if (!TryOptionalString(property.Value, ServiceTiers, out var tier, out var tierError))
                    {
                        return GroqChatOptionsParseResult.Fail(tierError ?? "Invalid serviceTier.");
                    }

                    options.ServiceTier = tier;
                    break;
                case "parallelToolCalls":
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        break;
                    }

                    if (property.Value.ValueKind != JsonValueKind.True && property.Value.ValueKind != JsonValueKind.False)
                    {
                        return GroqChatOptionsParseResult.Fail("Invalid parallelToolCalls.");
                    }

                    options.ParallelToolCalls = property.Value.GetBoolean();
                    break;
                case "user":
                    if (!TryOptionalFreeString(property.Value, out var user, out var userError))
                    {
                        return GroqChatOptionsParseResult.Fail(userError ?? "Invalid user.");
                    }

                    options.User = user;
                    break;
                case "structuredOutputs":
                    if (!TryOptionalBool(property.Value, out var structured, out var structuredError))
                    {
                        return GroqChatOptionsParseResult.Fail(structuredError ?? "Invalid structuredOutputs.");
                    }

                    options.StructuredOutputs = structured;
                    break;
                case "strictJsonSchema":
                    if (!TryOptionalBool(property.Value, out var strict, out var strictError))
                    {
                        return GroqChatOptionsParseResult.Fail(strictError ?? "Invalid strictJsonSchema.");
                    }

                    options.StrictJsonSchema = strict;
                    break;
                case "responseFormatDescription":
                    if (!TryOptionalFreeString(property.Value, out var description, out var descriptionError))
                    {
                        return GroqChatOptionsParseResult.Fail(descriptionError ?? "Invalid responseFormatDescription.");
                    }

                    options.ResponseFormatDescription = description;
                    break;
            }
        }

        return GroqChatOptionsParseResult.Ok(options);
    }

    private static bool TryOptionalString(JsonElement value, string[] allowed, out string? parsed, out string? error)
    {
        parsed = null;
        error = null;
        if (value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            error = "Expected a string.";
            return false;
        }

        var text = value.GetString();
        if (string.IsNullOrEmpty(text))
        {
            error = "Expected a supported value.";
            return false;
        }

        for (var i = 0; i < allowed.Length; i++)
        {
            if (string.Equals(allowed[i], text, StringComparison.Ordinal))
            {
                parsed = text;
                return true;
            }
        }

        error = "Expected a supported value.";
        return false;
    }

    private static bool TryOptionalFreeString(JsonElement value, out string? parsed, out string? error)
    {
        parsed = null;
        error = null;
        if (value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            error = "Expected a string.";
            return false;
        }

        parsed = value.GetString();
        return true;
    }

    private static bool TryOptionalBool(JsonElement value, out bool? parsed, out string? error)
    {
        parsed = null;
        error = null;
        if (value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.True && value.ValueKind != JsonValueKind.False)
        {
            error = "Expected a boolean.";
            return false;
        }

        parsed = value.GetBoolean();
        return true;
    }
}

/// <summary>Result of <see cref="GroqChatOptions.TryParse"/>.</summary>
public sealed class GroqChatOptionsParseResult
{
    private GroqChatOptionsParseResult(bool success, GroqChatOptions? options, string? error)
    {
        Success = success;
        Options = options;
        Error = error;
    }

    /// <summary>Whether the object matched the Groq chat options schema.</summary>
    public bool Success { get; }

    /// <summary>Parsed options when <see cref="Success"/> is true.</summary>
    public GroqChatOptions? Options { get; }

    /// <summary>Validation error when <see cref="Success"/> is false.</summary>
    public string? Error { get; }

    internal static GroqChatOptionsParseResult Ok(GroqChatOptions options)
    {
        return new GroqChatOptionsParseResult(true, options, null);
    }

    internal static GroqChatOptionsParseResult Fail(string error)
    {
        return new GroqChatOptionsParseResult(false, null, error);
    }
}
