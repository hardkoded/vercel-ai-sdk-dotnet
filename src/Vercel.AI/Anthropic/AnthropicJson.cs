// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>JSON helpers for the Anthropic Messages request and prompt converters.</summary>
internal static class AnthropicJson
{
    public static JsonNode? Node(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return JsonNode.Parse(element.GetRawText());
    }

    public static JsonObject ObjectFrom(JsonElement element)
    {
        var node = Node(element) as JsonObject;
        return node ?? new JsonObject();
    }

    public static void Set(JsonObject target, string name, JsonNode? value)
    {
        if (value == null)
        {
            return;
        }

        target[name] = value;
    }

    public static void SetNull(JsonObject target, string name)
    {
        target[name] = JsonNode.Parse("null");
    }

    public static JsonElement? Property(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value;
    }

    public static string? String(JsonElement element, string name)
    {
        var value = Property(element, name);
        if (value == null)
        {
            return null;
        }

        return value.Value.ValueKind == JsonValueKind.String ? value.Value.GetString() : null;
    }

    public static bool Bool(JsonElement element, string name)
    {
        var value = Property(element, name);
        return value != null && value.Value.ValueKind == JsonValueKind.True;
    }

    public static JsonElement Anthropic(JsonElement providerOptions)
    {
        var anthropic = Property(providerOptions, "anthropic");
        if (anthropic == null || anthropic.Value.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        return anthropic.Value;
    }

    public static JsonElement ProviderOptionsOf(JsonElement element)
    {
        var options = Property(element, "providerOptions");
        if (options == null)
        {
            return default;
        }

        return options.Value;
    }

    public static JsonNode? CacheControl(JsonElement providerOptions)
    {
        var anthropic = Anthropic(providerOptions);
        if (anthropic.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (anthropic.TryGetProperty("cacheControl", out var camel) && camel.ValueKind != JsonValueKind.Null && camel.ValueKind != JsonValueKind.Undefined)
        {
            return Node(camel);
        }

        if (anthropic.TryGetProperty("cache_control", out var snake) && snake.ValueKind != JsonValueKind.Null && snake.ValueKind != JsonValueKind.Undefined)
        {
            return Node(snake);
        }

        return null;
    }

    public static string Stringify(JsonElement element)
    {
        return JsonSerializer.Serialize(element);
    }

    public static string TopLevelMediaType(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    public static bool IsFullMediaType(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        if (slash < 0 || slash == mediaType.Length - 1)
        {
            return false;
        }

        var subtype = mediaType.Substring(slash + 1);
        return subtype.Length > 0 && subtype != "*";
    }

    public static string? DetectMediaType(string base64, string topLevel)
    {
        byte[] bytes;
        try
        {
            bytes = DecodeBase64(base64);
        }
        catch (FormatException)
        {
            return null;
        }

        if (topLevel == "image")
        {
            if (StartsWith(bytes, 0x89, 0x50, 0x4E, 0x47))
            {
                return "image/png";
            }

            if (StartsWith(bytes, 0xFF, 0xD8))
            {
                return "image/jpeg";
            }

            if (StartsWith(bytes, 0x47, 0x49, 0x46, 0x38))
            {
                return "image/gif";
            }
        }

        if (topLevel == "application" && StartsWith(bytes, 0x25, 0x50, 0x44, 0x46))
        {
            return "application/pdf";
        }

        return null;
    }

    public static string ResolveFullMediaType(JsonElement part)
    {
        var mediaType = String(part, "mediaType") ?? string.Empty;
        if (IsFullMediaType(mediaType))
        {
            return mediaType;
        }

        var data = Property(part, "data");
        if (data != null && String(data.Value, "type") == "data")
        {
            var payload = DataString(data.Value);
            var detected = payload == null ? null : DetectMediaType(payload, TopLevelMediaType(mediaType));
            if (detected != null)
            {
                return detected;
            }

            throw new AiSdkException(
                "Unsupported functionality: file of media type \"" + mediaType + "\" must specify subtype since it could not be auto-detected");
        }

        throw new AiSdkException(
            "Unsupported functionality: file of media type \"" + mediaType + "\" must specify subtype since it is not passed as inline bytes");
    }

    public static string? DataString(JsonElement data)
    {
        var payload = Property(data, "data");
        if (payload == null)
        {
            return null;
        }

        if (payload.Value.ValueKind == JsonValueKind.String)
        {
            return payload.Value.GetString();
        }

        return null;
    }

    public static string DecodeBase64Text(string base64)
    {
        return Encoding.UTF8.GetString(DecodeBase64(base64));
    }

    public static byte[] DecodeBase64(string base64)
    {
        var normalized = base64.Replace('-', '+').Replace('_', '/');
        switch (normalized.Length % 4)
        {
            case 2:
                normalized += "==";
                break;
            case 3:
                normalized += "=";
                break;
        }

        return Convert.FromBase64String(normalized);
    }

    public static string ResolveProviderReference(JsonElement reference, string provider)
    {
        if (reference.ValueKind == JsonValueKind.Object && reference.TryGetProperty(provider, out var id) && id.ValueKind == JsonValueKind.String)
        {
            var value = id.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                return value!;
            }
        }

        var available = new List<string>();
        if (reference.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in reference.EnumerateObject())
            {
                available.Add(property.Name);
            }
        }

        throw new AiSdkException(
            "No provider reference found for provider '" + provider + "'. Available providers: " + string.Join(", ", available));
    }

    public static JsonObject ToolInput(JsonElement input)
    {
        if (input.ValueKind == JsonValueKind.Object)
        {
            return ObjectFrom(input);
        }

        var wrapped = new JsonObject();
        wrapped["rawInvalidInput"] = Node(input);
        return wrapped;
    }

    public static void CopyIfPresent(JsonObject target, string targetName, JsonElement source, string sourceName)
    {
        if (!source.TryGetProperty(sourceName, out var value) || value.ValueKind == JsonValueKind.Undefined)
        {
            return;
        }

        Set(target, targetName, Node(value));
    }

    private static bool StartsWith(byte[] bytes, params byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
        {
            return false;
        }

        for (var i = 0; i < prefix.Length; i++)
        {
            if (bytes[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>A warning produced while preparing an Anthropic request.</summary>
public sealed class AnthropicWarning
{
    /// <summary>Creates a warning.</summary>
    public AnthropicWarning(string type, string? feature, string? details)
    {
        Type = type ?? "other";
        Feature = feature;
        Details = details;
    }

    /// <summary>Warning category, such as <c>unsupported</c> or <c>other</c>.</summary>
    public string Type { get; }

    /// <summary>Feature name, when the warning names one.</summary>
    public string? Feature { get; }

    /// <summary>Human-readable detail.</summary>
    public string? Details { get; }

    /// <summary>Text stored on a call warning.</summary>
    public string CallMessage
    {
        get
        {
            if (!string.IsNullOrEmpty(Details))
            {
                return Details!;
            }

            if (!string.IsNullOrEmpty(Feature))
            {
                return Feature!;
            }

            return Type;
        }
    }
}
