// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Parity.Media;

/// <summary>One recorded provider request.</summary>
internal sealed class MediaCall
{
    /// <summary>Creates a recorded call.</summary>
    public MediaCall(string method, string url, string body, Dictionary<string, string> headers)
    {
        Method = method;
        Url = url;
        Body = body ?? string.Empty;
        Headers = headers;
    }

    /// <summary>HTTP method.</summary>
    public string Method { get; }

    /// <summary>Absolute request URL.</summary>
    public string Url { get; }

    /// <summary>Request body text.</summary>
    public string Body { get; }

    /// <summary>Request headers with lowercase names.</summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>Header value, or null when the header was not sent.</summary>
    public string? Header(string name)
    {
        if (Headers.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }
}

/// <summary>Routes absolute URLs to scripted responses and records every call.</summary>
internal sealed class MediaHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<MediaCall, HttpResponseMessage>> _routes = new(StringComparer.Ordinal);

    /// <summary>Calls in order.</summary>
    public List<MediaCall> Calls { get; } = new();

    /// <summary>Returns JSON for <paramref name="url"/>.</summary>
    public void Json(string url, string json, IDictionary<string, string>? headers = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        _routes[url] = _ => Response(status, bytes, "application/json", headers);
    }

    /// <summary>Returns raw text for <paramref name="url"/>.</summary>
    public void Text(string url, string body, HttpStatusCode status, string mediaType)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        _routes[url] = _ => Response(status, bytes, mediaType, null);
    }

    /// <summary>Returns bytes for <paramref name="url"/>.</summary>
    public void Bytes(string url, byte[] body, string mediaType, IDictionary<string, string>? headers = null)
    {
        _routes[url] = _ => Response(HttpStatusCode.OK, body, mediaType, headers);
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            headers["authorization"] = request.Headers.Authorization.ToString();
        }

        var body = string.Empty;
        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
            }

            body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        var call = new MediaCall(request.Method.Method, request.RequestUri?.AbsoluteUri ?? string.Empty, body, headers);
        Calls.Add(call);
        if (_routes.TryGetValue(call.Url, out var route))
        {
            return route(call);
        }

        return Response(HttpStatusCode.NotFound, Encoding.UTF8.GetBytes("{\"error\":{\"message\":\"missing route\"}}"), "application/json", null);
    }

    private static HttpResponseMessage Response(HttpStatusCode status, byte[] body, string mediaType, IDictionary<string, string>? headers)
    {
        var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Headers.ContentLength = body.Length;
        var response = new HttpResponseMessage(status) { Content = content };
        if (headers != null)
        {
            foreach (var pair in headers)
            {
                response.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        return response;
    }
}

/// <summary>JSON comparisons for recorded provider bodies.</summary>
internal static class MediaJson
{
    /// <summary>Parses a JSON document.</summary>
    public static JsonNode Parse(string json)
    {
        return JsonNode.Parse(json) ?? new JsonObject();
    }
}
