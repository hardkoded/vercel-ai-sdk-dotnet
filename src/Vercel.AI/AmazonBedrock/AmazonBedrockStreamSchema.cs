// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Status and retry metadata for ConverseStream error events. Maps to <c>getAmazonBedrockStreamErrorMetadata</c>.</summary>
internal static class AmazonBedrockStreamError
{
    public static (int? StatusCode, bool? IsRetryable) Metadata(string type)
    {
        switch (type)
        {
            case "internalServerException":
            case "InternalServerException":
                return (500, true);
            case "modelStreamErrorException":
            case "ModelStreamErrorException":
                return (424, true);
            case "serviceUnavailableException":
            case "ServiceUnavailableException":
                return (503, true);
            case "throttlingException":
            case "ThrottlingException":
                return (429, true);
            case "validationException":
            case "ValidationException":
                return (400, false);
            default:
                return (null, null);
        }
    }
}

/// <summary>
/// A limited check of a wrapped ConverseStream chunk, focused on what the stream mapping needs.
/// Maps to <c>AmazonBedrockStreamSchema</c>.
/// </summary>
internal static class AmazonBedrockStreamSchema
{
    private static readonly string[] ExceptionKeys =
    {
        "internalServerException",
        "modelStreamErrorException",
        "serviceUnavailableException",
        "throttlingException",
        "validationException",
    };

    /// <summary>Returns an error message when <paramref name="chunk"/> does not match the schema, or null.</summary>
    public static string? Validate(JsonElement chunk)
    {
        if (chunk.ValueKind != JsonValueKind.Object)
        {
            return "Type validation failed: expected an object.";
        }

        foreach (var key in ExceptionKeys)
        {
            if (Has(chunk, key) && chunk.GetProperty(key).ValueKind != JsonValueKind.Object)
            {
                return Invalid(key, "an object");
            }
        }

        if (Has(chunk, "contentBlockDelta"))
        {
            var delta = chunk.GetProperty("contentBlockDelta");
            if (!HasNumber(delta, "contentBlockIndex"))
            {
                return Invalid("contentBlockDelta.contentBlockIndex", "a number");
            }

            if (Has(delta, "delta") && !ValidDelta(delta.GetProperty("delta")))
            {
                return Invalid("contentBlockDelta.delta", "a text, citation, toolUse, or reasoningContent delta");
            }
        }

        if (Has(chunk, "contentBlockStart"))
        {
            var start = chunk.GetProperty("contentBlockStart");
            if (!HasNumber(start, "contentBlockIndex"))
            {
                return Invalid("contentBlockStart.contentBlockIndex", "a number");
            }

            if (Has(start, "start") && Has(start.GetProperty("start"), "toolUse"))
            {
                var toolUse = start.GetProperty("start").GetProperty("toolUse");
                if (!HasString(toolUse, "toolUseId") || !HasString(toolUse, "name"))
                {
                    return Invalid("contentBlockStart.start.toolUse", "an object with toolUseId and name");
                }
            }
        }

        if (Has(chunk, "contentBlockStop") && !HasNumber(chunk.GetProperty("contentBlockStop"), "contentBlockIndex"))
        {
            return Invalid("contentBlockStop.contentBlockIndex", "a number");
        }

        if (Has(chunk, "messageStop") && !HasString(chunk.GetProperty("messageStop"), "stopReason"))
        {
            return Invalid("messageStop.stopReason", "a string");
        }

        if (Has(chunk, "metadata") && Has(chunk.GetProperty("metadata"), "usage"))
        {
            var usage = chunk.GetProperty("metadata").GetProperty("usage");
            if (!HasNumber(usage, "inputTokens") || !HasNumber(usage, "outputTokens"))
            {
                return Invalid("metadata.usage", "an object with inputTokens and outputTokens");
            }
        }

        return null;
    }

    private static bool ValidDelta(JsonElement delta)
    {
        if (delta.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (HasString(delta, "text"))
        {
            return true;
        }

        if (Has(delta, "citation"))
        {
            return delta.GetProperty("citation").ValueKind == JsonValueKind.Object;
        }

        if (Has(delta, "toolUse"))
        {
            return HasString(delta.GetProperty("toolUse"), "input");
        }

        if (Has(delta, "reasoningContent"))
        {
            var reasoning = delta.GetProperty("reasoningContent");
            return HasString(reasoning, "text") || HasString(reasoning, "signature") || HasString(reasoning, "data") || HasString(reasoning, "redactedContent");
        }

        return false;
    }

    private static string Invalid(string path, string expected)
    {
        return "Type validation failed: " + path + " must be " + expected + ".";
    }

    private static bool Has(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null;
    }

    private static bool HasNumber(JsonElement element, string name)
    {
        return Has(element, name) && element.GetProperty(name).ValueKind == JsonValueKind.Number;
    }

    private static bool HasString(JsonElement element, string name)
    {
        return Has(element, name) && element.GetProperty(name).ValueKind == JsonValueKind.String;
    }
}
