// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Hume;

/// <summary>Parsed Hume error body.</summary>
public sealed class HumeErrorData
{
    /// <summary>Creates a parsed error.</summary>
    public HumeErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider error message.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public int Code { get; }
}

/// <summary>Parses the Hume <c>{ error: { message, code } }</c> body.</summary>
public static class HumeError
{
    /// <summary>Parses an error body. Returns false when the JSON does not match the schema.</summary>
    public static bool TryParse(string json, out HumeErrorData? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var body) || body.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (!body.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                if (!body.TryGetProperty("code", out var code) || !code.TryGetInt32(out var codeValue))
                {
                    return false;
                }

                error = new HumeErrorData(message.GetString() ?? string.Empty, codeValue);
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
