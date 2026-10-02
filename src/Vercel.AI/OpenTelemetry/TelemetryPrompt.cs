// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.OpenTelemetry;

/// <summary>File bytes or a URL carried in a telemetry prompt.</summary>
public sealed class TelemetryFileData
{
    /// <summary>Creates inline bytes. They are base64-encoded in the telemetry JSON.</summary>
    public static TelemetryFileData FromBytes(byte[] bytes)
    {
        return new TelemetryFileData { Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes)) };
    }

    /// <summary>Creates a URL file. The telemetry JSON stores <see cref="Uri.ToString"/>.</summary>
    public static TelemetryFileData FromUrl(Uri url)
    {
        return new TelemetryFileData { Url = url ?? throw new ArgumentNullException(nameof(url)) };
    }

    /// <summary>Creates inline text or an already-encoded data string.</summary>
    public static TelemetryFileData FromText(string text)
    {
        return new TelemetryFileData { Text = text ?? string.Empty };
    }

    /// <summary>Inline bytes. Null when this file is a URL or text.</summary>
    public byte[]? Bytes { get; set; }

    /// <summary>File URL. Null when this file is inline.</summary>
    public Uri? Url { get; set; }

    /// <summary>Inline text or base64 data. Null when <see cref="Bytes"/> or <see cref="Url"/> is set.</summary>
    public string? Text { get; set; }
}

/// <summary>One content part of a telemetry prompt message.</summary>
public sealed class TelemetryContentPart
{
    /// <summary>Creates a part of <paramref name="type"/>.</summary>
    public TelemetryContentPart(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>Part type, such as <c>text</c> or <c>file</c>.</summary>
    public string Type { get; }

    /// <summary>Text body for a text part.</summary>
    public string? Text { get; set; }

    /// <summary>File name.</summary>
    public string? Filename { get; set; }

    /// <summary>Media type.</summary>
    public string? MediaType { get; set; }

    /// <summary>Provider options copied into the telemetry JSON.</summary>
    public JsonNode? ProviderOptions { get; set; }

    /// <summary>File payload.</summary>
    public TelemetryFileData? Data { get; set; }
}

/// <summary>One message in a telemetry prompt.</summary>
public sealed class TelemetryMessage
{
    /// <summary>Creates a message.</summary>
    public TelemetryMessage(string role)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
    }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>String content. Null when <see cref="Parts"/> is used.</summary>
    public string? Content { get; set; }

    /// <summary>Structured content. Null when <see cref="Content"/> is a string.</summary>
    public IReadOnlyList<TelemetryContentPart>? Parts { get; set; }
}

/// <summary>Serializes a prompt for a span attribute, encoding inline file bytes as base64.</summary>
public static class TelemetryPrompt
{
    /// <summary>Returns the JSON form of <paramref name="prompt"/> with file bytes replaced by base64.</summary>
    public static string Stringify(IReadOnlyList<TelemetryMessage> prompt)
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        var messages = new JsonArray();
        for (var index = 0; index < prompt.Count; index++)
        {
            var message = prompt[index];
            var obj = new JsonObject
            {
                ["role"] = message.Role,
            };
            if (message.Parts == null)
            {
                obj["content"] = message.Content ?? string.Empty;
            }
            else
            {
                var parts = new JsonArray();
                for (var partIndex = 0; partIndex < message.Parts.Count; partIndex++)
                {
                    parts.Add(WritePart(message.Parts[partIndex]));
                }

                obj["content"] = parts;
            }

            messages.Add(obj);
        }

        return messages.ToJsonString();
    }

    private static JsonObject WritePart(TelemetryContentPart part)
    {
        if (part.Type != "file")
        {
            return new JsonObject
            {
                ["type"] = part.Type,
                ["text"] = part.Text ?? string.Empty,
            };
        }

        var file = new JsonObject
        {
            ["type"] = "file",
        };
        if (part.Filename != null)
        {
            file["filename"] = part.Filename;
        }

        file["data"] = SerializeFile(part.Data);
        if (part.MediaType != null)
        {
            file["mediaType"] = part.MediaType;
        }

        if (part.ProviderOptions != null)
        {
            file["providerOptions"] = part.ProviderOptions.DeepClone();
        }

        return file;
    }

    private static string SerializeFile(TelemetryFileData? data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        if (data.Bytes != null)
        {
            return Convert.ToBase64String(data.Bytes);
        }

        if (data.Url != null)
        {
            return data.Url.ToString();
        }

        return data.Text ?? string.Empty;
    }
}
