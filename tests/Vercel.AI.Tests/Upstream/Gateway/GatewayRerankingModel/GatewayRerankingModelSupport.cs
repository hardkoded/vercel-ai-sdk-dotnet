// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel;

/// <summary>Fixtures and helpers shared by the <c>gateway-reranking-model.test.ts</c> port.</summary>
internal static class GatewayRerankingModelSupport
{
    internal const string DummyRanking =
        "[{\"index\":0,\"relevanceScore\":0.89},{\"index\":2,\"relevanceScore\":0.15},{\"index\":1,\"relevanceScore\":0.12}]";

    internal const string TestDocuments =
        "{\"type\":\"text\",\"values\":[\"Paris is the capital of France.\",\"Berlin is the capital of Germany.\",\"Madrid is the capital of Spain.\"]}";

    internal const string TestQuery = "What is the capital of France?";

    internal static RerankModelCall Call(
        int? topN = null,
        string? providerOptions = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        using var documents = JsonDocument.Parse(TestDocuments);
        using var options = providerOptions == null ? null : JsonDocument.Parse(providerOptions);
        return new RerankModelCall(
            new RerankModelDocuments(
                "text",
                documents
                    .RootElement.GetProperty("values")
                    .EnumerateArray()
                    .Select(value => (object?)value.GetString())
                    .ToList()
            ),
            TestQuery,
            topN,
            options?.RootElement.Clone(),
            headers,
            CancellationToken.None
        );
    }

    internal static (Vercel.AI.Gateway.GatewayRerankingModel Model, OpenAICapture Capture) Setup(
        string? body = null,
        HttpStatusCode status = HttpStatusCode.OK,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    )
    {
        var capture = new OpenAICapture
        {
            ResponseJson = body ?? "{\"ranking\":" + DummyRanking + "}",
            Status = status,
        };
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-token",
                BaseUrl = "https://api.test.com",
                O11yHeaders = o11yHeaders,
            },
            capture
        );
        return (
            Assert.IsType<Vercel.AI.Gateway.GatewayRerankingModel>(
                provider.RerankingModel("cohere/rerank-v3.5")
            ),
            capture
        );
    }
}
