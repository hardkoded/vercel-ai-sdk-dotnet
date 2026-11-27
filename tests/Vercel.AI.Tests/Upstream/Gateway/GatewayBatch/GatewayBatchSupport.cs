// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

/// <summary>Helpers shared by the <c>gateway-batch.test.ts</c> ports.</summary>
internal static class GatewayBatchSupport
{
    internal const string TestPromptJson =
        "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]";

    internal const string SucceededLine =
        "{\"type\":\"text\",\"id\":\"req-1\",\"status\":\"succeeded\",\"result\":{\"content\":[{\"type\":\"text\",\"text\":\"pong 1\"}],\"finishReason\":{\"unified\":\"stop\"},\"response\":{\"modelId\":\"openai/gpt-5.6-luna\",\"timestamp\":\"2026-08-18T00:00:00.000Z\"},\"usage\":{\"inputTokens\":{\"total\":4,\"noCache\":4},\"outputTokens\":{\"total\":2,\"text\":2}},\"warnings\":[]}}";

    internal const string FailedLine =
        "{\"type\":\"text\",\"id\":\"req-2\",\"status\":\"failed\",\"error\":{\"message\":\"boom\"}}";

    internal const string CancelledLine =
        "{\"type\":\"text\",\"id\":\"req-3\",\"status\":\"cancelled\"}";

    /// <summary>Upstream <c>BATCH_PROMPT_REQUESTS</c>.</summary>
    internal static readonly NormalizedBatchRequest[] BatchPromptRequests =
    [
        new("req-1", "text", "test-model-1", Parse("{\"prompt\":" + TestPromptJson + "}")),
        new(
            "req-2",
            "text",
            "test-model-1",
            Parse("{\"prompt\":" + TestPromptJson + ",\"maxOutputTokens\":32,\"temperature\":0}")
        ),
    ];

    internal const string BatchPromptRequestsJson =
        "[{\"id\":\"req-1\",\"type\":\"text\",\"modelId\":\"test-model-1\",\"options\":{\"prompt\":"
        + TestPromptJson
        + "}},{\"id\":\"req-2\",\"type\":\"text\",\"modelId\":\"test-model-1\",\"options\":{\"prompt\":"
        + TestPromptJson
        + ",\"maxOutputTokens\":32,\"temperature\":0}}]";

    internal static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Builds the batch API as upstream <c>createTestBatch</c>.</summary>
    internal static IBatchApi CreateTestBatch(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    ) => CreateProvider(handler, o11yHeaders).ExperimentalBatch()!;

    /// <summary>Builds the batch API with a <c>fetch</c> hook, as upstream <c>createTestBatch({ fetch })</c>.</summary>
    internal static IBatchApi CreateTestBatch(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> fetch,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    )
    {
        var provider = CreateProvider(new GatewayLanguageModelServer(), o11yHeaders);
        provider.Http.Fetch = fetch;
        return provider.ExperimentalBatch()!;
    }

    private static GatewayProvider CreateProvider(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? o11yHeaders
    ) =>
        GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-token",
                BaseUrl = "https://api.test.com",
                O11yHeaders = o11yHeaders ?? new Dictionary<string, string>(),
            },
            handler
        );

    /// <summary>The call options of upstream <c>doStartBatch</c>, <c>doGetBatchStatus</c>, and the other batch methods.</summary>
    internal static BatchOperationCall Call(
        string? batchId = null,
        string? providerOptions = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default,
        string? webhookUrl = null,
        IReadOnlyList<NormalizedBatchRequest>? requests = null
    ) =>
        new(
            batchId,
            providerOptions is null ? null : Parse(providerOptions),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            webhookUrl: webhookUrl,
            requests: requests
        );

    internal static GatewayLanguageModelServer JsonResponse(string body) =>
        new() { ContentType = "application/json", ResponseBody = body };

    internal static GatewayLanguageModelServer ResultsResponse(params string[] chunks) =>
        new() { ContentType = "application/x-ndjson", ResponseChunks = chunks };

    internal static async Task<List<BatchItem>> ReadAll(IAsyncEnumerable<BatchItem> items)
    {
        var all = new List<BatchItem>();
        await foreach (var item in items)
        {
            all.Add(item);
        }

        return all;
    }
}
