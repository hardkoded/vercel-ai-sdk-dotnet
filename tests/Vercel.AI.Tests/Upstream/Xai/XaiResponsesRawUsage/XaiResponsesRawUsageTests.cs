// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using XaiResponsesLanguageModelType = Vercel.AI.Xai.XaiResponsesLanguageModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesRawUsage;

/// <summary>Port of <c>xai-responses-raw-usage.test.ts</c> &gt; <c>XaiResponsesLanguageModel raw usage</c>.</summary>
public sealed class XaiResponsesRawUsageTests
{
    private const string Prefix = "packages/xai/src/responses/xai-responses-raw-usage.test.ts::XaiResponsesLanguageModel raw usage::";

    private static readonly string CompleteUsage = System.Text.Json.Nodes.JsonNode.Parse("""
        {"input_tokens":100,"output_tokens":50,"total_tokens":150,
         "input_tokens_details":{"cached_tokens":20,"provider_input_metadata":{"preserved":true}},
         "output_tokens_details":{"reasoning_tokens":30,"provider_output_metadata":["preserved",1]},
         "context_details":{"input_tokens":90,"output_tokens":40},
         "server_side_tool_usage_details":{"web_search_calls":1},
         "provider_usage_metadata":{"nested":{"preserved":true}}}
        """)!.ToJsonString();

    [Fact]
    [UpstreamTest(Prefix + "preserves complete raw usage through doGenerate parsing", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_complete_raw_usage_through_doGenerate_parsing()
    {
        var capture = new UpstreamCapture { ResponseBody = CreateResponse(CompleteUsage, "completed") };

        var result = await Model(capture).DoGenerateAsync(UpstreamChat.Prompt("hello"), CancellationToken.None);

        AssertCompleteUsage(result.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves complete raw usage from the final doStream usage event", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_complete_raw_usage_from_the_final_doStream_usage_event()
    {
        var initialUsage = """{"input_tokens":0,"output_tokens":0,"provider_usage_metadata":{"stage":"initial"}}""";
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(
                "{\"type\":\"response.created\",\"response\":" + CreateResponse(initialUsage, "in_progress") + "}",
                "{\"type\":\"response.completed\",\"response\":" + CreateResponse(CompleteUsage, "completed") + "}"),
        };

        var parts = await UpstreamChat.Read((await Model(capture).DoStreamAsync(UpstreamChat.Prompt("hello"), CancellationToken.None)));

        AssertCompleteUsage(parts.OfType<FinishStreamPart>().Single().Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "continues validating known usage field types", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_validating_known_usage_field_types()
    {
        var usage = CompleteUsage.Replace("\"input_tokens\":100", "\"input_tokens\":\"100\"", StringComparison.Ordinal);
        var capture = new UpstreamCapture { ResponseBody = CreateResponse(usage, "completed") };

        await Assert.ThrowsAsync<ApiException>(() => Model(capture).DoGenerateAsync(UpstreamChat.Prompt("hello"), CancellationToken.None));
    }

    private static void AssertCompleteUsage(LanguageModelUsage usage)
    {
        JsonAssert.Equal(usage.Raw!.Value, CompleteUsage);
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(80, usage.NoCacheInputTokens);
        Assert.Equal(20, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(20, usage.TextTokens);
        Assert.Equal(30, usage.ReasoningTokens);
    }

    private static string CreateResponse(string usage, string status)
    {
        return "{\"id\":\"resp_123\",\"object\":\"response\",\"status\":\"" + status + "\",\"model\":\"grok-4-fast-non-reasoning\",\"output\":[],\"usage\":" + usage + "}";
    }

    private static XaiResponsesLanguageModelType Model(UpstreamCapture capture)
    {
        var provider = Vercel.AI.Xai.XaiProvider.Create(
            new Vercel.AI.OpenAICompatible.OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://api.x.ai/v1" },
            capture);
        return provider.ResponsesModel("grok-4-fast-non-reasoning");
    }
}
