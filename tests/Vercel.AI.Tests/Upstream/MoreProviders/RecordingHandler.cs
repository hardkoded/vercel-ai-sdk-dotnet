// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;

namespace Vercel.AI.Tests;

/// <summary>One request captured by <see cref="RecordingHandler"/>.</summary>
internal sealed class RecordedCall
{
    /// <summary>Creates a captured call.</summary>
    public RecordedCall(HttpMethod method, Uri uri, byte[] body, IReadOnlyDictionary<string, string> headers)
    {
        Method = method;
        Uri = uri;
        Body = body ?? Array.Empty<byte>();
        Headers = headers;
        Text = Encoding.UTF8.GetString(Body);
    }

    /// <summary>HTTP method.</summary>
    public HttpMethod Method { get; }

    /// <summary>Request URI.</summary>
    public Uri Uri { get; }

    /// <summary>Raw body.</summary>
    public byte[] Body { get; }

    /// <summary>Request and content headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Body decoded as UTF-8.</summary>
    public string Text { get; }

    /// <summary>Reads a header, ignoring case.</summary>
    public string? Header(string name)
    {
        foreach (var pair in Headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }
}

/// <summary>Records every request and returns a scripted response.</summary>
internal sealed class RecordingHandler : HttpMessageHandler
{
    /// <summary>Creates a handler. <paramref name="respond"/> receives each recorded call.</summary>
    public RecordingHandler(Func<RecordedCall, HttpResponseMessage>? respond = null)
    {
        Respond = respond ?? (_ => Json("{}"));
    }

    /// <summary>Calls in order.</summary>
    public List<RecordedCall> Calls { get; } = new();

    /// <summary>Response factory.</summary>
    public Func<RecordedCall, HttpResponseMessage> Respond { get; set; }

    /// <summary>JSON response.</summary>
    public static HttpResponseMessage Json(string json, params (string Name, string Value)[] headers)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json ?? "{}"));
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        Add(response, headers);
        return response;
    }

    /// <summary>Binary response. A null content type is omitted.</summary>
    public static HttpResponseMessage Bytes(byte[] body, string? contentType, params (string Name, string Value)[] headers)
    {
        var content = new ByteArrayContent(body ?? Array.Empty<byte>());
        if (!string.IsNullOrEmpty(contentType))
        {
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        Add(response, headers);
        return response;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content == null
            ? Array.Empty<byte>()
            : await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        var call = new RecordedCall(request.Method, request.RequestUri ?? new Uri("https://example.invalid/"), body, headers);
        Calls.Add(call);
        return Respond(call);
    }

    private static void Add(HttpResponseMessage response, (string Name, string Value)[] headers)
    {
        foreach (var header in headers)
        {
            response.Headers.TryAddWithoutValidation(header.Name, header.Value);
        }
    }
}
