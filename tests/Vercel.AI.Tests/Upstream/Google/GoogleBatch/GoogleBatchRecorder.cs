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

    private CancellationTokenSource? _abort;
    private int _abortOnCall;

    public List<RecordedCall> Calls { get; } = [];

    /// <summary>
    /// Stand-in for upstream's <c>mockFetch.mock.calls[n][1]?.signal</c> check. HttpClient hands the handler a linked
    /// token and disposes it once the call ends, so identity and a later cancel cannot show that the caller's token
    /// reached the call. Instead the handler cancels <paramref name="abort"/> while call number <paramref name="onCall"/>
    /// (1-based) is still open and records whether the token the handler saw followed.
    /// </summary>
    public GoogleBatchRecorder ObserveAbort(CancellationTokenSource abort, int onCall)
    {
        _abort = abort;
        _abortOnCall = onCall;
        return this;
    }

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
        var call = new RecordedCall(
            request.Method.Method,
            url,
            request.RequestUri.AbsoluteUri,
            headers,
            body
        );
        Calls.Add(call);
        var response = _routes.TryGetValue(url, out var respond)
            ? respond()
            : new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };
        if (_abort != null && Calls.Count == _abortOnCall)
        {
            // Buffer first so the caller can still read the body once its token is cancelled.
            await response.Content.LoadIntoBufferAsync();
            _abort.Cancel();
            call.SawCancellation = cancellationToken.IsCancellationRequested;
        }

        return response;
    }

    internal sealed record RecordedCall(
        string Method,
        string Url,
        string FullUrl,
        Dictionary<string, string> Headers,
        string Body
    )
    {
        public bool SawCancellation { get; set; }

        public JsonNode BodyJson => JsonNode.Parse(Body)!;
    }
}
