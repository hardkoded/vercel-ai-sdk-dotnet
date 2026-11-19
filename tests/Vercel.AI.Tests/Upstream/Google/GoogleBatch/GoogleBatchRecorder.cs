// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Upstream.Google.GoogleBatch;

/// <summary>Stand-in for upstream's createTestServer: routes by URL without the query string and records each call.</summary>
internal sealed class GoogleBatchRecorder : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpResponseMessage>> _routes = new(
        StringComparer.Ordinal
    );

    public List<RecordedCall> Calls { get; } = [];

    public GoogleBatchRecorder Json(
        string url,
        JsonNode body,
        HttpStatusCode status = HttpStatusCode.OK,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        _routes[url] = () =>
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
            };
            foreach (var header in headers ?? new Dictionary<string, string>())
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return response;
        };
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var url = request.RequestUri!.GetLeftPart(UriPartial.Path);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        var body =
            request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
        Calls.Add(new RecordedCall(request.Method.Method, url, request.RequestUri.AbsoluteUri, headers, body));
        return _routes.TryGetValue(url, out var respond)
            ? respond()
            : new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };
    }

    internal sealed record RecordedCall(
        string Method,
        string Url,
        string FullUrl,
        Dictionary<string, string> Headers,
        string Body
    )
    {
        public JsonNode BodyJson => JsonNode.Parse(Body)!;
    }
}
