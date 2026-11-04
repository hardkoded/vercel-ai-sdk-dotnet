// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; providerMetadata key based on provider string</c>.</summary>
public sealed class DoStreamProviderMetadataKeyBasedOnProviderStringTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > providerMetadata key based on provider string::";

    private static readonly string[] StreamingChunks =
    [
        """
        {"type":"response.created","response":{"id":"resp_67c9a81b6a048190a9ee441c5755a4e8","object":"response","created_at":1741269019,"status":"in_progress","error":null,"incomplete_details":null,"input":[],"instructions":null,"max_output_tokens":null,"model":"gpt-4o-2024-07-18","output":[],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":0.3,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1,"truncation":"disabled","usage":null,"user":null,"metadata":{}}}
        """,
        """
        {"type":"response.in_progress","response":{"id":"resp_67c9a81b6a048190a9ee441c5755a4e8","object":"response","created_at":1741269019,"status":"in_progress","error":null,"incomplete_details":null,"input":[],"instructions":null,"max_output_tokens":null,"model":"gpt-4o-2024-07-18","output":[],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":0.3,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1,"truncation":"disabled","usage":null,"user":null,"metadata":{}}}
        """,
        """
        {"type":"response.output_item.added","output_index":0,"item":{"id":"msg_67c9a81dea8c8190b79651a2b3adf91e","type":"message","status":"in_progress","role":"assistant","content":[]}}
        """,
        """
        {"type":"response.content_part.added","item_id":"msg_67c9a81dea8c8190b79651a2b3adf91e","output_index":0,"content_index":0,"part":{"type":"output_text","text":"","annotations":[],"logprobs":[]}}
        """,
        """
        {"type":"response.output_text.delta","item_id":"msg_67c9a81dea8c8190b79651a2b3adf91e","output_index":0,"content_index":0,"delta":"Hello,"}
        """,
        """
        {"type":"response.output_text.delta","item_id":"msg_67c9a81dea8c8190b79651a2b3adf91e","output_index":0,"content_index":0,"delta":" World!"}
        """,
        """
        {"type":"response.output_text.done","item_id":"msg_67c9a8787f4c8190b49c858d4c1cf20c","output_index":0,"content_index":0,"text":"Hello, World!"}
        """,
        """
        {"type":"response.content_part.done","item_id":"msg_67c9a8787f4c8190b49c858d4c1cf20c","output_index":0,"content_index":0,"part":{"type":"output_text","text":"Hello, World!","annotations":[]}}
        """,
        """
        {"type":"response.output_item.done","output_index":0,"item":{"id":"msg_67c9a8787f4c8190b49c858d4c1cf20c","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Hello, World!","annotations":[]}]}}
        """,
        """
        {"type":"response.completed","response":{"id":"resp_67c9a878139c8190aa2e3105411b408b","object":"response","created_at":1741269112,"status":"completed","error":null,"incomplete_details":null,"input":[],"instructions":null,"max_output_tokens":null,"model":"gpt-4o-2024-07-18","output":[{"id":"msg_67c9a8787f4c8190b49c858d4c1cf20c","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Hello, World!","annotations":[]}]}],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":0.3,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1,"truncation":"disabled","usage":{"input_tokens":543,"input_tokens_details":{"cached_tokens":234},"output_tokens":478,"output_tokens_details":{"reasoning_tokens":123},"total_tokens":512},"user":null,"metadata":{}}}
        """,
    ];

    private static readonly string[] ReasoningChunks =
    [
        """
        {"type":"response.created","response":{"id":"resp_reasoning","object":"response","created_at":1741269019,"status":"in_progress","error":null,"incomplete_details":null,"input":[],"instructions":null,"max_output_tokens":null,"model":"o3-mini-2025-01-31","output":[],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":"low","summary":"auto"},"store":true,"temperature":null,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":null,"truncation":"disabled","usage":null,"user":null,"metadata":{}}}
        """,
        """
        {"type":"response.output_item.added","output_index":0,"item":{"id":"rs_reasoning_item","type":"reasoning"}}
        """,
        """
        {"type":"response.reasoning_summary_part.added","item_id":"rs_reasoning_item","summary_index":0}
        """,
        """
        {"type":"response.reasoning_summary_text.delta","item_id":"rs_reasoning_item","summary_index":0,"delta":"thinking through the steps"}
        """,
        """
        {"type":"response.reasoning_summary_part.done","item_id":"rs_reasoning_item","summary_index":0}
        """,
        """
        {"type":"response.output_item.done","output_index":0,"item":{"id":"rs_reasoning_item","type":"reasoning","summary":[{"type":"summary_text","text":"thinking through the steps"}]}}
        """,
        """
        {"type":"response.completed","response":{"id":"resp_reasoning","object":"response","created_at":1741269019,"status":"completed","error":null,"incomplete_details":null,"input":[],"instructions":null,"max_output_tokens":null,"model":"o3-mini-2025-01-31","output":[{"id":"rs_reasoning_item","type":"reasoning","summary":[{"type":"summary_text","text":"thinking through the steps"}]}],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":"low","summary":"auto"},"store":true,"temperature":null,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":null,"truncation":"disabled","usage":{"input_tokens":10,"input_tokens_details":{"cached_tokens":0},"output_tokens":20,"output_tokens_details":{"reasoning_tokens":20},"total_tokens":30},"user":null,"metadata":{}}}
        """,
    ];

    private static IEnumerable<string> Sse(IEnumerable<string> chunks) => chunks.Select(chunk => "data:" + chunk + "\n\n");

    private static OpenAIResponsesLanguageModel AzureModel(string modelId, SseStreamHandler handler) =>
        AzureOpenAIProvider.Create(new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-api-key" }, handler).ResponsesModel(modelId);

    private static OpenAIResponsesLanguageModel OpenAIModel(string modelId, SseStreamHandler handler) =>
        OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler).ResponsesModel(modelId);

    [Fact]
    [UpstreamTest(Prefix + "should use \"azure\" as providerMetadata key in finish event when provider \"azure\" but providerOptions is \"openai\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_azure_as_providerMetadata_key_in_finish_event_when_provider_azure_but_providerOptions_is_openai()
    {
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("""{"reasoningSummary":"auto"}""");

        var events = await OpenAIUpstream.Read(await AzureModel("gpt-4o", new SseStreamHandler(Sse(StreamingChunks))).DoStreamAsync(options, CancellationToken.None));

        var finish = events.OfType<FinishStreamPart>().First();
        var metadata = finish.ProviderMetadata!.Value;
        Assert.True(metadata.TryGetProperty("azure", out var azure));
        Assert.False(metadata.TryGetProperty("openai", out _));
        Assert.Equal("resp_67c9a81b6a048190a9ee441c5755a4e8", azure.GetProperty("responseId").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should use \"openai\" as providerMetadata key in finish event when provider does not include \"azure\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_openai_as_providerMetadata_key_in_finish_event_when_provider_does_not_include_azure()
    {
        var events = await OpenAIUpstream.Read(
            await OpenAIModel("gpt-4o", new SseStreamHandler(Sse(StreamingChunks))).DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));

        var finish = events.OfType<FinishStreamPart>().First();
        var metadata = finish.ProviderMetadata!.Value;
        Assert.True(metadata.TryGetProperty("openai", out var openai));
        Assert.False(metadata.TryGetProperty("azure", out _));
        Assert.Equal("resp_67c9a81b6a048190a9ee441c5755a4e8", openai.GetProperty("responseId").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should use \"azure\" as providerMetadata key in streaming reasoning events when provider includes \"azure\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_azure_as_providerMetadata_key_in_streaming_reasoning_events_when_provider_includes_azure()
    {
        var events = await OpenAIUpstream.Read(
            await AzureModel("o3-mini", new SseStreamHandler(Sse(ReasoningChunks))).DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));

        var reasoningStart = events.OfType<ReasoningStartStreamPart>().First();
        Assert.True(reasoningStart.ProviderMetadata!.Value.TryGetProperty("azure", out var startAzure));
        Assert.False(reasoningStart.ProviderMetadata!.Value.TryGetProperty("openai", out _));
        Assert.Equal("rs_reasoning_item", startAzure.GetProperty("itemId").GetString());
        Assert.Equal(JsonValueKind.Null, startAzure.GetProperty("reasoningEncryptedContent").ValueKind);

        var reasoningDelta = events.OfType<ReasoningDeltaStreamPart>().First();
        Assert.True(reasoningDelta.ProviderMetadata!.Value.TryGetProperty("azure", out var deltaAzure));
        Assert.False(reasoningDelta.ProviderMetadata!.Value.TryGetProperty("openai", out _));
        Assert.Equal("rs_reasoning_item", deltaAzure.GetProperty("itemId").GetString());
    }
}
