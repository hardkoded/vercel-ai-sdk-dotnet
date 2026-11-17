// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Fixtures shared by the <c>gateway-realtime-model.test.ts</c> port.</summary>
internal static class GatewayRealtimeModelSupport
{
    internal const string UpstreamCommit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    /// <summary>Port of the test file's <c>createTestModel</c>.</summary>
    internal static Vercel.AI.Gateway.GatewayRealtimeModel CreateTestModel(
        string modelId = "openai/gpt-realtime",
        string baseUrl = "https://ai-gateway.vercel.sh/v4/ai",
        string token = "vcst_test-secret",
        string? teamIdOrSlug = null
    )
    {
        return new Vercel.AI.Gateway.GatewayRealtimeModel(
            modelId,
            new GatewayRealtimeModelConfig(
                "gateway.realtime",
                baseUrl,
                _ =>
                {
                    var headers = new Dictionary<string, string?>
                    {
                        ["Authorization"] = "Bearer " + token,
                    };
                    if (teamIdOrSlug is not null)
                    {
                        headers["x-vercel-ai-gateway-team"] = teamIdOrSlug;
                    }

                    return Task.FromResult<IReadOnlyDictionary<string, string?>>(headers);
                },
                (_, _, _) => Task.FromResult(new GatewayMintedClientSecret(token)),
                teamIdOrSlug
            )
        );
    }
}

/// <summary>Records requests and answers with a fixed JSON body.</summary>
internal sealed class RealtimeMintCapture : HttpMessageHandler
{
    private readonly string _response;

    public RealtimeMintCapture(string response)
    {
        _response = response;
    }

    public List<HttpRequestMessage> Requests { get; } = new();

    public List<string> Bodies { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Requests.Add(request);
        Bodies.Add(
            request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false)
        );
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_response, Encoding.UTF8, "application/json"),
        };
    }
}
