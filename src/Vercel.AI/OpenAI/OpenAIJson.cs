// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

internal static class OpenAIJson
{
    public static JsonElement Clone(JsonElement element)
    {
        return element.Clone();
    }

    public static JsonElement Element(JsonNode node)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    public static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static JsonElement? Provider(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        if (providerOptions != null && providerOptions.TryGetValue("openai", out var value))
        {
            return value;
        }

        return null;
    }

    public static JsonElement? Child(JsonElement? element, string name)
    {
        if (element == null || element.Value.ValueKind != JsonValueKind.Object || !element.Value.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value;
    }

    public static string? String(JsonElement? element, string name)
    {
        var child = Child(element, name);
        if (child == null)
        {
            return null;
        }

        return child.Value.ValueKind == JsonValueKind.String ? child.Value.GetString() : null;
    }

    public static bool? Bool(JsonElement? element, string name)
    {
        var child = Child(element, name);
        if (child == null || child.Value.ValueKind != JsonValueKind.True && child.Value.ValueKind != JsonValueKind.False)
        {
            return null;
        }

        return child.Value.GetBoolean();
    }

    public static int? Int(JsonElement? element, string name)
    {
        var child = Child(element, name);
        if (child == null || child.Value.ValueKind != JsonValueKind.Number || !child.Value.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    public static double? Double(JsonElement? element, string name)
    {
        var child = Child(element, name);
        if (child == null || child.Value.ValueKind != JsonValueKind.Number || !child.Value.TryGetDouble(out var number))
        {
            return null;
        }

        return number;
    }

    public static int? NestedInt(JsonElement element, string objectName, string name)
    {
        if (!element.TryGetProperty(objectName, out var child) || child.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!child.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    public static bool HasProperty(JsonElement element, string objectName, string name)
    {
        return element.TryGetProperty(objectName, out var child)
            && child.ValueKind == JsonValueKind.Object
            && child.TryGetProperty(name, out _);
    }

    public static void Set(JsonObject body, string name, JsonNode? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    public static JsonNode? Node(JsonElement? element)
    {
        if (element == null || element.Value.ValueKind == JsonValueKind.Null || element.Value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return JsonNode.Parse(element.Value.GetRawText());
    }

    public static JsonElement? OpenAIObject(JsonElement? providerOptions)
    {
        if (providerOptions == null || providerOptions.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (providerOptions.Value.TryGetProperty("openai", out var openai) && openai.ValueKind == JsonValueKind.Object)
        {
            return openai;
        }

        return providerOptions;
    }

    public static JsonObject? PromptCacheBreakpoint(JsonElement? providerOptions)
    {
        var breakpoint = Child(OpenAIObject(providerOptions), "promptCacheBreakpoint");
        if (breakpoint == null)
        {
            return null;
        }

        return JsonNode.Parse(breakpoint.Value.GetRawText()) as JsonObject;
    }

    public static string? ImageDetail(JsonElement? providerOptions)
    {
        return String(OpenAIObject(providerOptions), "imageDetail");
    }

    public static LanguageModelUsage ChatUsage(JsonElement? usage)
    {
        if (usage == null || usage.Value.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var element = usage.Value;
        var prompt = Int(element, "prompt_tokens") ?? 0;
        var completion = Int(element, "completion_tokens") ?? 0;
        var total = Int(element, "total_tokens");
        var cacheRead = NestedInt(element, "prompt_tokens_details", "cached_tokens") ?? 0;
        int? cacheWrite = HasProperty(element, "prompt_tokens_details", "cache_write_tokens")
            ? NestedInt(element, "prompt_tokens_details", "cache_write_tokens")
            : null;
        var reasoning = NestedInt(element, "completion_tokens_details", "reasoning_tokens") ?? 0;
        return new LanguageModelUsage(prompt, completion, total, cacheRead, cacheWrite, reasoning, element.Clone());
    }

    public static LanguageModelUsage ResponsesUsage(JsonElement? usage)
    {
        if (usage == null || usage.Value.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var element = usage.Value;
        var input = Int(element, "input_tokens") ?? 0;
        var output = Int(element, "output_tokens") ?? 0;
        var total = Int(element, "total_tokens");
        var cacheRead = NestedInt(element, "input_tokens_details", "cached_tokens") ?? 0;
        int? cacheWrite = HasProperty(element, "input_tokens_details", "cache_write_tokens")
            ? NestedInt(element, "input_tokens_details", "cache_write_tokens")
            : null;
        var reasoning = NestedInt(element, "output_tokens_details", "reasoning_tokens") ?? 0;
        return new LanguageModelUsage(input, output, total, cacheRead, cacheWrite, reasoning, element.Clone());
    }

    public static LanguageModelUsage CompletionUsage(JsonElement? usage)
    {
        if (usage == null || usage.Value.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var element = usage.Value;
        return new LanguageModelUsage(
            Int(element, "prompt_tokens"),
            Int(element, "completion_tokens"),
            Int(element, "total_tokens"),
            raw: element.Clone());
    }

    public static JsonElement ProviderMetadata(JsonObject openai)
    {
        var root = new JsonObject { ["openai"] = openai };
        return Element(root);
    }

    public static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Value))
            {
                continue;
            }

            if (pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = pair.Value!;
                var space = value.IndexOf(' ');
                if (space > 0)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(value.Substring(0, space), value.Substring(space + 1));
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", value);
                }

                continue;
            }

            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }

    public static Dictionary<string, string> CopyHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }

    public static string MediaTypeFromBytes(byte[] data, string topLevel)
    {
        if (!string.Equals(topLevel, "image", StringComparison.OrdinalIgnoreCase))
        {
            return topLevel;
        }

        if (StartsWith(data, 0x89, 0x50, 0x4E, 0x47))
        {
            return "image/png";
        }

        if (StartsWith(data, 0xFF, 0xD8))
        {
            return "image/jpeg";
        }

        if (StartsWith(data, 0x47, 0x49, 0x46, 0x38))
        {
            return "image/gif";
        }

        if (data.Length >= 12
            && data[0] == 0x52
            && data[1] == 0x49
            && data[2] == 0x46
            && data[3] == 0x46
            && data[8] == 0x57
            && data[9] == 0x45
            && data[10] == 0x42
            && data[11] == 0x50)
        {
            return "image/webp";
        }

        return topLevel;
    }

    public static bool IsFullMediaType(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash > 0 && slash < mediaType.Length - 1 && mediaType.IndexOf('*') < 0;
    }

    public static string TopLevel(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    public static string ResolveFullMediaType(string mediaType, byte[]? data, bool inline)
    {
        if (IsFullMediaType(mediaType))
        {
            return mediaType;
        }

        if (inline && data != null)
        {
            var detected = MediaTypeFromBytes(data, TopLevel(mediaType));
            if (IsFullMediaType(detected))
            {
                return detected;
            }

            throw new AiSdkException("file of media type \"" + mediaType + "\" must specify subtype since it could not be auto-detected");
        }

        throw new AiSdkException("file of media type \"" + mediaType + "\" must specify subtype since it is not passed as inline bytes");
    }

    public static string Extension(string mediaType)
    {
        switch (mediaType)
        {
            case "audio/wav":
            case "audio/x-wav":
                return "wav";
            case "audio/mpeg":
            case "audio/mp3":
                return "mp3";
            case "audio/mp4":
            case "audio/m4a":
                return "m4a";
            case "audio/webm":
                return "webm";
            case "audio/flac":
                return "flac";
            case "audio/ogg":
                return "ogg";
            default:
                var slash = mediaType.IndexOf('/');
                return slash >= 0 && slash < mediaType.Length - 1 ? mediaType.Substring(slash + 1) : "bin";
        }
    }

    public static string EncodePathSegment(string value)
    {
        var encoded = Uri.EscapeDataString(value);
        if (encoded == ".")
        {
            return "%252E";
        }

        if (encoded == "..")
        {
            return "%252E%252E";
        }

        return encoded;
    }

    public static DateTimeOffset? UnixSeconds(long? seconds)
    {
        if (seconds == null || seconds.Value == 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds.Value);
    }

    public static long? Unix(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.TryGetInt64(out var seconds) ? seconds : null;
    }

    private static bool StartsWith(byte[] data, params int[] prefix)
    {
        if (data.Length < prefix.Length)
        {
            return false;
        }

        for (var i = 0; i < prefix.Length; i++)
        {
            if (data[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }
}

internal static class OpenAIClock
{
    public static DateTimeOffset Now(Func<DateTimeOffset>? clock)
    {
        return clock != null ? clock() : DateTimeOffset.UtcNow;
    }
}

internal static class OpenAIForm
{
    public static string Describe(double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
