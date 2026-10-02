// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;

namespace Vercel.AI.Tests;

internal sealed class CaptureHandler : HttpMessageHandler
{
    public Uri? Uri { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string ResponseBody { get; set; } = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}";

    public string? ServerSentEvents { get; set; }

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri = request.RequestUri;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        RequestHeaders.Clear();
        foreach (var header in request.Headers)
        {
            RequestHeaders[header.Key] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            RequestHeaders["Authorization"] = request.Headers.Authorization.ToString();
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                RequestHeaders[header.Key] = string.Join(",", header.Value);
            }
        }

        var response = new HttpResponseMessage(StatusCode);
        foreach (var pair in ResponseHeaders)
        {
            if (!response.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
            {
                response.Content ??= new StringContent(string.Empty);
            }
        }

        if (ServerSentEvents != null)
        {
            response.Content = new StringContent(ServerSentEvents, Encoding.UTF8, "text/event-stream");
        }
        else
        {
            response.Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json");
        }

        foreach (var pair in ResponseHeaders)
        {
            response.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }

        return response;
    }
}
