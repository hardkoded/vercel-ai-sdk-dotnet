// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Azure;

/// <summary>One request seen by <see cref="AzureImageHandler"/>. Stands in for a <c>vi.fn</c> fetch call.</summary>
internal sealed class RecordedImageRequest
{
    public string Url { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public byte[] Body { get; init; } = Array.Empty<byte>();

    public string? ContentType { get; init; }

    public JsonNode Json()
    {
        return JsonNode.Parse(Encoding.UTF8.GetString(Body))!;
    }

    public List<MultipartPart> Parts()
    {
        return MultipartPart.Parse(ContentType, Body);
    }
}

/// <summary>One part of a multipart form body.</summary>
internal sealed class MultipartPart
{
    public string Name { get; init; } = string.Empty;

    public string? FileName { get; init; }

    public string? ContentType { get; init; }

    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string Text => Encoding.UTF8.GetString(Data);

    public static List<MultipartPart> Parse(string? contentType, byte[] body)
    {
        var boundary = contentType!.Split("boundary=", 2)[1].Trim('"', ' ');
        var text = Encoding.Latin1.GetString(body);
        var parts = new List<MultipartPart>();
        foreach (var raw in text.Split("--" + boundary).Skip(1))
        {
            if (raw.StartsWith("--", StringComparison.Ordinal))
            {
                break;
            }

            var split = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            var head = raw[..split];
            var content = raw[(split + 4)..];
            if (content.EndsWith("\r\n", StringComparison.Ordinal))
            {
                content = content[..^2];
            }

            string? name = null;
            string? fileName = null;
            string? type = null;
            foreach (var line in head.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("Content-Disposition", StringComparison.OrdinalIgnoreCase))
                {
                    name = Attribute(line, "name");
                    fileName = Attribute(line, "filename");
                }
                else if (line.StartsWith("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    type = line[(line.IndexOf(':') + 1)..].Trim();
                }
            }

            parts.Add(new MultipartPart { Name = name ?? string.Empty, FileName = fileName, ContentType = type, Data = Encoding.Latin1.GetBytes(content) });
        }

        return parts;
    }

    private static string? Attribute(string line, string key)
    {
        var marker = "; " + key + "=";
        var start = line.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        start += marker.Length;
        if (line[start] == '"')
        {
            return line[(start + 1)..line.IndexOf('"', start + 1)];
        }

        var end = line.IndexOf(';', start);
        return end < 0 ? line[start..] : line[start..end];
    }
}

/// <summary>Records every request, including image downloads, and answers with a response factory.</summary>
internal sealed class AzureImageHandler : HttpMessageHandler
{
    private readonly Func<RecordedImageRequest, CancellationToken, Task<HttpResponseMessage>> _respond;
    private readonly object _gate = new();
    private readonly List<RecordedImageRequest> _requests = new();

    public AzureImageHandler(Func<RecordedImageRequest, CancellationToken, Task<HttpResponseMessage>> respond)
    {
        _respond = respond;
    }

    public AzureImageHandler(Func<HttpResponseMessage> respond)
        : this((_, _) => Task.FromResult(respond()))
    {
    }

    public IReadOnlyList<RecordedImageRequest> Requests
    {
        get
        {
            lock (_gate)
            {
                return _requests.ToList();
            }
        }
    }

    public RecordedImageRequest Last => Requests[^1];

    public static HttpResponseMessage JsonResponse(object body, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var recorded = new RecordedImageRequest
        {
            Url = request.RequestUri!.AbsoluteUri,
            Body = request.Content == null ? Array.Empty<byte>() : await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false),
            ContentType = request.Content?.Headers.ContentType?.ToString(),
        };
        foreach (var header in request.Headers)
        {
            recorded.Headers[header.Key] = string.Join(",", header.Value);
        }

        lock (_gate)
        {
            _requests.Add(recorded);
        }

        return await _respond(recorded, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Shared fixtures of the Azure image tests.</summary>
internal static class AzureImageTestSupport
{
    public const string GenerationsUrl = "https://test-resource.services.ai.azure.com/mai/v1/images/generations";

    public const string EditsUrl = "https://test-resource.services.ai.azure.com/mai/v1/images/edits";

    public static readonly object MaiResponse = new
    {
        created = 1791229019,
        model = "MAI-Image-2.6-Flash",
        size = "768x768",
        usage = new { num_output_tokens = 576, num_input_text_tokens = 10, num_input_image_tokens = 0 },
        data = new[] { new { b64_json = "iVBORw0KGgo=" } },
    };

    public static readonly object OpenAIResponse = new { data = new[] { new { b64_json = "aGk=" } } };

    public static AzureOpenAIProvider CreateProvider(AzureImageHandler handler, Action<AzureOpenAIOptions>? configure = null)
    {
        var options = new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-key" };
        configure?.Invoke(options);
        return AzureOpenAIProvider.Create(options, handler);
    }

    public static ImageCallOptions CallOptions(string? providerOptions = null)
    {
        return new ImageCallOptions("A red apple")
        {
            Count = 1,
            ProviderOptions = providerOptions == null ? null : ProviderOptionsOf(providerOptions),
        };
    }

    public static IReadOnlyDictionary<string, JsonElement> ProviderOptionsOf(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
