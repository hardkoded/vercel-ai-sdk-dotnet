// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ElevenLabs;

/// <summary>Parsed ElevenLabs error body.</summary>
public sealed class ElevenLabsErrorData
{
    /// <summary>Creates a parsed error.</summary>
    public ElevenLabsErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider error message.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public int Code { get; }
}

/// <summary>Parses the ElevenLabs <c>{ error: { message, code } }</c> body.</summary>
public static class ElevenLabsError
{
    /// <summary>Parses an error body. Returns false when the JSON does not match the schema.</summary>
    public static bool TryParse(string json, out ElevenLabsErrorData? error)
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

                error = new ElevenLabsErrorData(message.GetString() ?? string.Empty, codeValue);
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
