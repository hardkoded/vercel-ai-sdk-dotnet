// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// The client's session start frame. Maps to <c>TranscriptionStreamStartFrame</c>. Optional keys are omitted when null.
/// </summary>
public sealed class TranscriptionStreamStartFrame
{
    /// <summary>Creates a start frame.</summary>
    public TranscriptionStreamStartFrame(AudioFormat inputAudioFormat, JsonElement? providerOptions = null, bool? includeRawChunks = null)
    {
        InputAudioFormat = inputAudioFormat ?? throw new ArgumentNullException(nameof(inputAudioFormat));
        ProviderOptions = providerOptions;
        IncludeRawChunks = includeRawChunks;
    }

    /// <summary>Always <see cref="TranscriptionStreamEnvelope.TranscriptionStreamStartFrameType"/>.</summary>
    public string Type => TranscriptionStreamEnvelope.TranscriptionStreamStartFrameType;

    /// <summary>Audio format of the binary audio frames, for example <c>audio/pcm</c> at 16000 Hz.</summary>
    public AudioFormat InputAudioFormat { get; }

    /// <summary>Provider-specific options, passed through verbatim.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>When true, the server should include <c>raw</c> parts in the stream.</summary>
    public bool? IncludeRawChunks { get; }

    /// <summary>Serializes the frame. Optional keys are omitted when null.</summary>
    public string ToJson()
    {
        var format = new JsonObject { ["type"] = InputAudioFormat.Type };
        if (InputAudioFormat.Rate is { } rate)
        {
            format["rate"] = rate;
        }

        var frame = new JsonObject
        {
            ["type"] = Type,
            ["inputAudioFormat"] = format,
        };
        if (ProviderOptions is { } options)
        {
            frame["providerOptions"] = JsonNode.Parse(options.GetRawText());
        }

        if (IncludeRawChunks is { } include)
        {
            frame["includeRawChunks"] = include;
        }

        return frame.ToJsonString();
    }
}

/// <summary>Server-side classification of a client text frame. Maps to <c>TranscriptionStreamClientFrame</c>.</summary>
public sealed class TranscriptionStreamClientFrame
{
    internal TranscriptionStreamClientFrame(string type, TranscriptionStreamStartFrame? frame = null, string? message = null)
    {
        Type = type;
        Frame = frame;
        Message = message;
    }

    /// <summary><c>start</c>, <c>audio-done</c>, <c>invalid</c> (malformed JSON or a recognized frame with an invalid shape), or <c>unknown</c> (ignore for forward compatibility).</summary>
    public string Type { get; }

    /// <summary>The start frame, when <see cref="Type"/> is <c>start</c>.</summary>
    public TranscriptionStreamStartFrame? Frame { get; }

    /// <summary>Why the frame is invalid, when <see cref="Type"/> is <c>invalid</c>.</summary>
    public string? Message { get; }
}

/// <summary>
/// Experimental transcription-stream WebSocket envelope (v1): the standard serialization of the transcription model
/// <c>doStream</c> over a WebSocket. Maps to <c>transcription-stream-envelope.ts</c>.
/// </summary>
/// <remarks>
/// Envelope rules:
/// 1. The client sends exactly one <c>transcription-stream.start</c> text frame first.
/// 2. Audio rides binary frames containing raw bytes in the declared <c>inputAudioFormat</c> (base64 string chunks are decoded before sending).
/// 3. The client signals end of audio with the <c>transcription-stream.audio-done</c> text frame; a plain close without it is an abort.
/// 4. Every server-to-client text frame is one JSON-serialized stream part (flattened, no wrapper). Timestamps ride as ISO 8601 strings.
/// 5. The server closes with code 1000 after the <c>finish</c> part; on failure it sends an <c>error</c> part and closes non-1000. A close without a prior <c>finish</c> is an error.
/// 6. Unknown frame and part types are ignored in both directions (forward compatibility).
/// 7. Servers may enforce a maximum frame size (the AI Gateway rejects frames over 256 KiB); clients should split audio into frames of at most 64 KiB.
/// 8. Connection establishment (URL, auth) is transport-specific and out of scope.
/// The envelope validates frame shape only. Both parsers reject frames carrying <c>__proto__</c> keys.
/// </remarks>
public static class TranscriptionStreamEnvelope
{
    /// <summary>Type of the first client text frame. Maps to <c>TRANSCRIPTION_STREAM_START_FRAME_TYPE</c>.</summary>
    public const string TranscriptionStreamStartFrameType = "transcription-stream.start";

    /// <summary>Type of the client text frame that signals the end of the audio input. Maps to <c>TRANSCRIPTION_STREAM_AUDIO_DONE_FRAME_TYPE</c>.</summary>
    public const string TranscriptionStreamAudioDoneFrameType = "transcription-stream.audio-done";

    /// <summary>
    /// Server-side: parses a client text frame. Validates envelope shape only and rejects prototype-pollution
    /// payloads. Never throws. Maps to <c>parseTranscriptionStreamClientFrame</c>.
    /// </summary>
    public static TranscriptionStreamClientFrame ParseTranscriptionStreamClientFrame(string text)
    {
        JsonElement frame;
        try
        {
            frame = SecureJsonParse.Parse(text);
        }
        catch (JsonException)
        {
            return new TranscriptionStreamClientFrame("invalid", message: "malformed JSON");
        }

        if (frame.ValueKind != JsonValueKind.Object)
        {
            return new TranscriptionStreamClientFrame("invalid", message: "frame must be a JSON object");
        }

        if (!frame.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
        {
            return new TranscriptionStreamClientFrame("invalid", message: "frame type must be a string");
        }

        switch (type.GetString())
        {
            case TranscriptionStreamStartFrameType:
                if (!frame.TryGetProperty("inputAudioFormat", out var format)
                    || format.ValueKind != JsonValueKind.Object
                    || !format.TryGetProperty("type", out var formatType)
                    || formatType.ValueKind != JsonValueKind.String)
                {
                    return new TranscriptionStreamClientFrame("invalid", message: "start frame must have an inputAudioFormat object with a string type");
                }

                double? rate = null;
                if (format.TryGetProperty("rate", out var rateValue))
                {
                    if (rateValue.ValueKind != JsonValueKind.Number)
                    {
                        return new TranscriptionStreamClientFrame("invalid", message: "inputAudioFormat.rate must be a number when present");
                    }

                    rate = rateValue.GetDouble();
                }

                JsonElement? providerOptions = null;
                if (frame.TryGetProperty("providerOptions", out var options))
                {
                    if (options.ValueKind != JsonValueKind.Object)
                    {
                        return new TranscriptionStreamClientFrame("invalid", message: "providerOptions must be an object when present");
                    }

                    providerOptions = options;
                }

                bool? includeRawChunks = null;
                if (frame.TryGetProperty("includeRawChunks", out var include))
                {
                    if (include.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    {
                        return new TranscriptionStreamClientFrame("invalid", message: "includeRawChunks must be a boolean when present");
                    }

                    includeRawChunks = include.GetBoolean();
                }

                return new TranscriptionStreamClientFrame(
                    "start",
                    new TranscriptionStreamStartFrame(new AudioFormat(formatType.GetString()!, rate), providerOptions, includeRawChunks));
            case TranscriptionStreamAudioDoneFrameType:
                return new TranscriptionStreamClientFrame("audio-done");
            default:
                return new TranscriptionStreamClientFrame("unknown");
        }
    }

    /// <summary>
    /// Server-side: serializes a transcription stream part as one text frame. Returns null for a part type the
    /// envelope does not carry; callers drop the frame. Maps to <c>serializeTranscriptionStreamPart</c>.
    /// </summary>
    public static string? SerializeTranscriptionStreamPart(ModelStreamPart part)
    {
        part = part ?? throw new ArgumentNullException(nameof(part));
        var frame = new JsonObject { ["type"] = part.Type };
        switch (part.Type)
        {
            case "stream-start":
                frame["warnings"] = new JsonArray((part.Warnings ?? Array.Empty<OperationWarning>()).Select(WarningToJson).ToArray());
                break;
            case "transcript-delta":
                SetIf(frame, "id", part.Id);
                frame["delta"] = part.Delta;
                SetMetadata(frame, part);
                break;
            case "transcript-partial":
                frame["text"] = part.Text;
                SetIf(frame, "id", part.Id);
                SetIf(frame, "startSecond", part.StartSecond);
                SetIf(frame, "durationInSeconds", part.DurationInSeconds);
                SetIf(frame, "channelIndex", part.ChannelIndex);
                SetMetadata(frame, part);
                break;
            case "transcript-final":
                frame["text"] = part.Text;
                SetIf(frame, "id", part.Id);
                SetIf(frame, "startSecond", part.StartSecond);
                SetIf(frame, "endSecond", part.EndSecond);
                SetIf(frame, "channelIndex", part.ChannelIndex);
                SetMetadata(frame, part);
                break;
            case "finish":
                frame["text"] = part.Text;
                frame["segments"] = new JsonArray((part.Segments ?? Array.Empty<TranscriptSegment>())
                    .Select(segment => (JsonNode)new JsonObject { ["text"] = segment.Text, ["startSecond"] = segment.StartSecond, ["endSecond"] = segment.EndSecond })
                    .ToArray());
                SetIf(frame, "language", part.Language);
                SetIf(frame, "durationInSeconds", part.DurationInSeconds);
                if (part.Usage is { } usage)
                {
                    var usageNode = new JsonObject();
                    SetIf(usageNode, "inputTokens", usage.InputTokens);
                    SetIf(usageNode, "outputTokens", usage.OutputTokens);
                    SetIf(usageNode, "totalTokens", usage.TotalTokens);
                    SetIf(usageNode, "tokens", usage.Tokens);
                    SetIf(usageNode, "seconds", usage.Seconds);
                    frame["usage"] = usageNode;
                }

                SetMetadata(frame, part);
                break;
            case "response-metadata":
                if (part.Timestamp is { } timestamp)
                {
                    frame["timestamp"] = timestamp.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                }

                SetIf(frame, "modelId", part.ModelId);

                if (part.Headers is { } headers)
                {
                    var headersNode = new JsonObject();
                    foreach (var pair in headers)
                    {
                        headersNode[pair.Key] = pair.Value;
                    }

                    frame["headers"] = headersNode;
                }

                break;
            case "raw":
                frame["rawValue"] = part.Raw is { } raw ? JsonNode.Parse(raw.GetRawText()) : null;
                break;
            case "error":
                if (part.ErrorValue is { } error)
                {
                    frame["error"] = JsonNode.Parse(error.GetRawText());
                }
                else
                {
                    // `Error` properties are not serializable as-is, so they travel as { name, message }.
                    frame["error"] = new JsonObject
                    {
                        ["name"] = part.Error is null || part.Error.GetType() == typeof(Exception) ? "Error" : part.Error.GetType().Name,
                        ["message"] = part.Error?.Message ?? string.Empty,
                    };
                }

                break;
            default:
                return null;
        }

        return frame.ToJsonString();
    }

    /// <summary>
    /// Client-side: parses a server text frame into a transcription stream part. Returns null for malformed or unsafe
    /// (prototype-polluting) JSON, unknown part types, and known part types whose required or optional fields are
    /// missing or mistyped (including warning and segment elements): downstream SDK code dereferences those fields,
    /// so a drifted server must not crash or pollute the stream. Revives <c>response-metadata.timestamp</c> to a date.
    /// Maps to <c>parseTranscriptionStreamPart</c>.
    /// </summary>
    public static ModelStreamPart? ParseTranscriptionStreamPart(string text)
    {
        JsonElement part;
        try
        {
            part = SecureJsonParse.Parse(text);
        }
        catch (JsonException)
        {
            return null;
        }

        if (part.ValueKind != JsonValueKind.Object || !part.TryGetProperty("type", out var typeValue) || typeValue.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var type = typeValue.GetString()!;
        switch (type)
        {
            case "stream-start":
            {
                if (!part.TryGetProperty("warnings", out var warnings) || warnings.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                var result = new List<OperationWarning>();
                foreach (var warning in warnings.EnumerateArray())
                {
                    if (warning.ValueKind != JsonValueKind.Object || GetString(warning, "type") is not { } warningType)
                    {
                        return null;
                    }

                    result.Add(new OperationWarning(
                        warningType,
                        message: GetString(warning, "message"),
                        feature: GetString(warning, "feature"),
                        details: GetString(warning, "details"),
                        setting: GetString(warning, "setting")));
                }

                return new ModelStreamPart { Type = type, Warnings = result };
            }

            case "transcript-delta":
                if (GetString(part, "delta") is not { } delta
                    || !IsOptional(part, "id", JsonValueKind.String)
                    || !IsOptional(part, "providerMetadata", JsonValueKind.Object))
                {
                    return null;
                }

                return WithMetadata(new ModelStreamPart { Type = type, Delta = delta, Id = GetString(part, "id") }, part);
            case "transcript-partial":
                if (GetString(part, "text") is not { } partialText
                    || !IsOptional(part, "id", JsonValueKind.String)
                    || !IsOptional(part, "startSecond", JsonValueKind.Number)
                    || !IsOptional(part, "durationInSeconds", JsonValueKind.Number)
                    || !IsOptional(part, "channelIndex", JsonValueKind.Number)
                    || !IsOptional(part, "providerMetadata", JsonValueKind.Object))
                {
                    return null;
                }

                return WithMetadata(
                    new ModelStreamPart
                    {
                        Type = type,
                        Text = partialText,
                        Id = GetString(part, "id"),
                        StartSecond = GetNumber(part, "startSecond"),
                        DurationInSeconds = GetNumber(part, "durationInSeconds"),
                        ChannelIndex = GetInt(part, "channelIndex"),
                    },
                    part);
            case "transcript-final":
                if (GetString(part, "text") is not { } finalText
                    || !IsOptional(part, "id", JsonValueKind.String)
                    || !IsOptional(part, "startSecond", JsonValueKind.Number)
                    || !IsOptional(part, "endSecond", JsonValueKind.Number)
                    || !IsOptional(part, "channelIndex", JsonValueKind.Number)
                    || !IsOptional(part, "providerMetadata", JsonValueKind.Object))
                {
                    return null;
                }

                return WithMetadata(
                    new ModelStreamPart
                    {
                        Type = type,
                        Text = finalText,
                        Id = GetString(part, "id"),
                        StartSecond = GetNumber(part, "startSecond"),
                        EndSecond = GetNumber(part, "endSecond"),
                        ChannelIndex = GetInt(part, "channelIndex"),
                    },
                    part);
            case "finish":
            {
                if (GetString(part, "text") is not { } text2
                    || !part.TryGetProperty("segments", out var segments)
                    || segments.ValueKind != JsonValueKind.Array
                    || !IsOptional(part, "language", JsonValueKind.String)
                    || !IsOptional(part, "durationInSeconds", JsonValueKind.Number)
                    || !IsOptional(part, "usage", JsonValueKind.Object)
                    || !IsOptional(part, "providerMetadata", JsonValueKind.Object))
                {
                    return null;
                }

                var parsed = new List<TranscriptSegment>();
                foreach (var segment in segments.EnumerateArray())
                {
                    if (segment.ValueKind != JsonValueKind.Object
                        || GetString(segment, "text") is not { } segmentText
                        || GetNumber(segment, "startSecond") is not { } start
                        || GetNumber(segment, "endSecond") is not { } end)
                    {
                        return null;
                    }

                    parsed.Add(new TranscriptSegment(segmentText, start, end));
                }

                OperationUsage? usage = null;
                if (part.TryGetProperty("usage", out var usageValue) && usageValue.ValueKind == JsonValueKind.Object)
                {
                    usage = new OperationUsage(
                        GetInt(usageValue, "inputTokens"),
                        GetInt(usageValue, "outputTokens"),
                        GetInt(usageValue, "totalTokens"),
                        GetNumber(usageValue, "tokens"),
                        GetNumber(usageValue, "seconds"));
                }

                return WithMetadata(
                    new ModelStreamPart
                    {
                        Type = type,
                        Text = text2,
                        Segments = parsed,
                        Language = GetString(part, "language"),
                        DurationInSeconds = GetNumber(part, "durationInSeconds"),
                        Usage = usage,
                    },
                    part);
            }

            case "response-metadata":
            {
                if (!IsOptional(part, "modelId", JsonValueKind.String) || !IsOptional(part, "headers", JsonValueKind.Object))
                {
                    return null;
                }

                var result = new ModelStreamPart { Type = type, ModelId = GetString(part, "modelId") };
                if (part.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
                {
                    var map = new Dictionary<string, string>();
                    foreach (var header in headers.EnumerateObject())
                    {
                        map[header.Name] = header.Value.ValueKind == JsonValueKind.String ? header.Value.GetString()! : header.Value.GetRawText();
                    }

                    result.Headers = map;
                }

                // Envelope rule 4: timestamps ride as ISO 8601 strings.
                if (part.TryGetProperty("timestamp", out var timestamp) && timestamp.ValueKind != JsonValueKind.Null)
                {
                    if (timestamp.ValueKind != JsonValueKind.String
                        || !DateTimeOffset.TryParse(timestamp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var revived))
                    {
                        return null;
                    }

                    result.Timestamp = revived.UtcDateTime;
                }

                return result;
            }

            case "raw":
                return part.TryGetProperty("rawValue", out var rawValue) ? new ModelStreamPart { Type = type, Raw = rawValue.Clone() } : null;
            case "error":
            {
                if (!part.TryGetProperty("error", out var error))
                {
                    return null;
                }

                return new ModelStreamPart { Type = type, ErrorValue = error.Clone(), Error = new AiSdkException(DescribeError(error)) };
            }

            default:
                return null;
        }
    }

    private static string DescribeError(JsonElement error)
    {
        if (error.ValueKind == JsonValueKind.Object && GetString(error, "message") is { } message)
        {
            return message;
        }

        return error.ValueKind == JsonValueKind.String ? error.GetString()! : error.GetRawText();
    }

    private static ModelStreamPart WithMetadata(ModelStreamPart result, JsonElement part)
    {
        if (part.TryGetProperty("providerMetadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object)
        {
            result.ProviderMetadata = metadata.Clone();
        }

        return result;
    }

    private static string? GetString(JsonElement value, string name)
    {
        return value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static double? GetNumber(JsonElement value, string name)
    {
        return value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDouble() : null;
    }

    // A number that is not a 32-bit integer is dropped rather than wrapped or truncated.
    private static int? GetInt(JsonElement value, string name)
    {
        return value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number) ? number : null;
    }

    // An absent property is valid. Upstream treats `undefined` as absent, and JSON has no `undefined`, so null counts as mistyped.
    private static bool IsOptional(JsonElement value, string name, JsonValueKind kind)
    {
        return !value.TryGetProperty(name, out var property) || property.ValueKind == kind;
    }

    private static JsonNode WarningToJson(OperationWarning warning)
    {
        var node = new JsonObject { ["type"] = warning.Type };
        SetIf(node, "message", warning.Message);
        SetIf(node, "feature", warning.Feature);
        SetIf(node, "details", warning.Details);
        SetIf(node, "setting", warning.Setting);
        return node;
    }

    private static void SetMetadata(JsonObject frame, ModelStreamPart part)
    {
        if (part.ProviderMetadata is { } metadata)
        {
            frame["providerMetadata"] = JsonNode.Parse(metadata.GetRawText());
        }
    }

    private static void SetIf(JsonObject frame, string name, string? value)
    {
        if (value is not null)
        {
            frame[name] = value;
        }
    }

    private static void SetIf(JsonObject frame, string name, double? value)
    {
        if (value is { } number)
        {
            frame[name] = number;
        }
    }

    private static void SetIf(JsonObject frame, string name, int? value)
    {
        if (value is { } number)
        {
            frame[name] = number;
        }
    }
}
