// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;

namespace Vercel.AI.Tests;

/// <summary>Records one request and returns a fixed JSON body.</summary>
internal sealed class UpstreamRecordingHandler : HttpMessageHandler
{
    public UpstreamRecordingHandler(string responseBody, HttpStatusCode status = HttpStatusCode.OK)
    {
        ResponseBody = responseBody;
        Status = status;
    }

    public string ResponseBody { get; set; }

    public HttpStatusCode Status { get; set; }

    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Headers.Clear();
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }
        }

        if (request.Headers.Authorization != null)
        {
            Headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        return new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
        };
    }
}
