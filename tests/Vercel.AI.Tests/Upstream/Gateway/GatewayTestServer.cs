// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Gateway;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Gateway;

/// <summary>Stands in for upstream's <c>createTestServer</c> and the <c>createBasic*</c> helpers of the Gateway fetcher tests.</summary>
internal sealed class GatewayTestServer : HttpMessageHandler
{
    public GatewayTestServer(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        Body = body;
        Status = status;
    }

    public string Body { get; set; }

    public HttpStatusCode Status { get; set; }

    public List<HttpRequestMessage> Calls { get; } = [];

    public static GatewayConfig CreateConfig(
        GatewayTestServer server,
        Func<IReadOnlyDictionary<string, string?>>? headers = null
    )
    {
        return new GatewayConfig
        {
            BaseUrl = "https://api.example.com",
            Headers = headers
                ?? (
                    () =>
                        new Dictionary<string, string?> { ["Authorization"] = "Bearer test-token" }
                ),
            Http = new ProviderHttp(
                new HttpClient(server, disposeHandler: false),
                new RetryPolicy { MaxRetries = 0 }
            ),
        };
    }

    public Dictionary<string, string> RequestHeaders(int call = 0)
    {
        return Calls[call]
            .Headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => string.Join(",", h.Value));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Calls.Add(request);
        return Task.FromResult(
            new HttpResponseMessage(Status)
            {
                Content = new StringContent(Body, Encoding.UTF8, "application/json"),
            }
        );
    }
}
