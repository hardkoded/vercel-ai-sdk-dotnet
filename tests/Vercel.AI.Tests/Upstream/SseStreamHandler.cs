// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Vercel.AI.Tests.Upstream.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>
/// Answers every request with an SSE stream, like the upstream <c>stream-chunks</c> response:
/// <c>content-type</c>, <c>cache-control</c> and <c>connection</c> headers, no content length, and the extra headers of the test.
/// </summary>
internal sealed class SseStreamHandler : HttpMessageHandler
{
    private readonly byte[] _body;
    private readonly IReadOnlyDictionary<string, string>? _headers;

    public SseStreamHandler(IEnumerable<string> chunks, IReadOnlyDictionary<string, string>? headers = null)
    {
        _body = Encoding.UTF8.GetBytes(string.Concat(chunks));
        _headers = headers;
    }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    /// <summary>Builds the chunks of an upstream <c>*.chunks.txt</c> fixture: one <c>data:</c> event per line, and a final <c>[DONE]</c> when asked.</summary>
    public static IEnumerable<string> FixtureChunks(string name, bool done)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".chunks.txt"))
            .Where(line => line.Trim().Length > 0)
            .Select(line => "data: " + line + "\n\n")
            .ToList();
        if (done)
        {
            lines.Add("data: [DONE]\n\n");
        }

        return lines;
    }

    /// <summary>
    /// Checks the headers upstream expects: the default stream headers plus <c>test-header</c>, and nothing else.
    /// Header names are case-insensitive, and the port keeps the casing the HTTP client reports.
    /// </summary>
    public static void AssertDefaultHeaders(IReadOnlyDictionary<string, string> headers)
    {
        Assert.Equal(4, headers.Count);
        Assert.Equal("no-cache", headers["cache-control"]);
        Assert.Equal("keep-alive", headers["connection"]);
        Assert.Equal("text/event-stream", headers["content-type"]);
        Assert.Equal("test-value", headers["test-header"]);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnknownLengthContent(new MemoryStream(_body)) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
        response.Headers.TryAddWithoutValidation("cache-control", "no-cache");
        response.Headers.TryAddWithoutValidation("connection", "keep-alive");
        if (_headers != null)
        {
            foreach (var header in _headers)
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return response;
    }
}
