// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>
/// The module-level functions of upstream <c>mistral-realtime-transport.ts</c>. The <c>MistralRealtimeTransport</c>
/// class itself (LiveKit) is not ported: the port has no LiveKit client.
/// </summary>
internal static class MistralRealtimeTransportFunctions
{
    /// <summary>Voice transcripts contain replacement text, rather than append-only deltas. Maps to <c>parseMistralRealtimeEvent</c>.</summary>
    /// <exception cref="Vercel.AI.Operations.InvalidResponseDataException">A known event has the wrong shape.</exception>
    public static IReadOnlyList<RealtimeModelV4ServerEvent> ParseMistralRealtimeEvent(
        JsonElement raw
    )
    {
        var type =
            raw.ValueKind == JsonValueKind.Object
            && raw.TryGetProperty("type", out var typeValue)
            && typeValue.ValueKind == JsonValueKind.String
                ? typeValue.GetString()
                : null;
        switch (type)
        {
            case "transcription.segment":
            {
                var id = Required(raw, "id", JsonValueKind.String).GetString()!;
                var role = Required(raw, "role", JsonValueKind.String).GetString();
                var content = Required(raw, "content", JsonValueKind.String).GetString()!;
                var final = Required(raw, "final", JsonValueKind.True, JsonValueKind.False)
                    .GetBoolean();
                if (role != "user" && role != "assistant")
                {
                    throw Invalid(raw, "role");
                }

                if (!final)
                {
                    return [];
                }

                if (role == "user")
                {
                    return
                    [
                        new RealtimeModelV4ServerEvent("input-transcription-completed")
                        {
                            ItemId = id,
                            Transcript = content,
                            Raw = raw,
                        },
                    ];
                }

                return
                [
                    new RealtimeModelV4ServerEvent("response-created")
                    {
                        ResponseId = id,
                        Raw = raw,
                    },
                    new RealtimeModelV4ServerEvent("audio-transcript-delta")
                    {
                        ResponseId = id,
                        ItemId = id,
                        Delta = content,
                        Raw = raw,
                    },
                    new RealtimeModelV4ServerEvent("audio-transcript-done")
                    {
                        ResponseId = id,
                        ItemId = id,
                        Transcript = content,
                        Raw = raw,
                    },
                    new RealtimeModelV4ServerEvent("response-done")
                    {
                        ResponseId = id,
                        Status = "completed",
                        Raw = raw,
                    },
                ];
            }
            case "session.updated":
                return [new RealtimeModelV4ServerEvent("session-updated") { Raw = raw }];
            case "error":
            {
                var message = Required(raw, "message", JsonValueKind.String).GetString();
                var code = Required(raw, "code", JsonValueKind.String).GetString();
                Required(raw, "fatal", JsonValueKind.True, JsonValueKind.False);
                return
                [
                    new RealtimeModelV4ServerEvent("error")
                    {
                        Message = message,
                        Code = code,
                        Raw = raw,
                    },
                ];
            }
            default:
                return
                [
                    new RealtimeModelV4ServerEvent("custom")
                    {
                        RawType = type ?? "unknown",
                        Raw = raw,
                    },
                ];
        }
    }

    private static JsonElement Required(JsonElement raw, string name, params JsonValueKind[] kinds)
    {
        if (!raw.TryGetProperty(name, out var value) || !kinds.Contains(value.ValueKind))
        {
            throw Invalid(raw, name);
        }

        return value;
    }

    private static Vercel.AI.Operations.InvalidResponseDataException Invalid(
        JsonElement raw,
        string field
    )
    {
        return new Vercel.AI.Operations.InvalidResponseDataException(
            raw.GetRawText(),
            "Invalid Mistral realtime event: " + field + " is missing or has the wrong type."
        );
    }
}
