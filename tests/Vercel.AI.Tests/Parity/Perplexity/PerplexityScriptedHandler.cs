// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;

namespace Vercel.AI.Tests;

internal sealed class PerplexityScriptedHandler : HttpMessageHandler
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public string ResponseBody { get; set; } = "{}";

    public string MediaType { get; set; } = "application/json";

    public Dictionary<string, string>? ResponseHeaders { get; set; }

    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

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

        if (request.Headers.Authorization != null)
        {
            Headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        if (!Headers.ContainsKey("User-Agent") && request.Headers.UserAgent.Count > 0)
        {
            Headers["User-Agent"] = string.Join(" ", request.Headers.UserAgent);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }
        }

        var message = new HttpResponseMessage(StatusCode)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, MediaType),
        };
        if (ResponseHeaders != null)
        {
            foreach (var header in ResponseHeaders)
            {
                if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return message;
    }
}
