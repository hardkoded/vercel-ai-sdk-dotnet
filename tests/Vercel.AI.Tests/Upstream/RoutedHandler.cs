// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;

namespace Vercel.AI.Tests.Upstream;

/// <summary>Answers each request with the body registered for its absolute URL.</summary>
internal sealed class RoutedHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _bodies = new(StringComparer.Ordinal);

    public RoutedHandler Add(string url, string body)
    {
        _bodies[url] = body;
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.AbsoluteUri ?? string.Empty;
        if (!_bodies.TryGetValue(url, out var body))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"error\":{\"message\":\"no route for " + url + "\"}}", Encoding.UTF8, "application/json"),
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        });
    }
}
