// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel.DeepSeekResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel;

/// <summary>Port of <c>deepseek-responses-language-model.test.ts</c> &gt; <c>DeepSeekResponsesLanguageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = TestFile + " > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "maps a recorded reasoning response", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_a_recorded_reasoning_response()
    {
        var capture = JsonFixture("deepseek-responses");

        var result = await Model(capture).DoGenerateAsync(ReasoningPrompt(), CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(Snapshot("deepseek-responses.generate.snap.json"), GenerateSnapshot(result, capture)),
            GenerateSnapshot(result, capture).ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "maps reasoning, text, web search, function calls and usage", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_reasoning_text_web_search_function_calls_and_usage()
    {
        var capture = JsonResponse("""
            {"output":[
               {"type":"reasoning","id":"rs_1","content":[{"type":"reasoning_text","text":"Let me search."}]},
               __WS__,
               {"type":"message","id":"msg_1","content":[{"type":"output_text","text":"https://python.org"}]},
               {"type":"function_call","id":"fc_1","call_id":"call_1","name":"lookup","arguments":"{\"q\":\"x\"}"}],
             "usage":{"input_tokens":3475,"input_tokens_details":{"cached_tokens":512},"output_tokens":54,
                      "output_tokens_details":{"reasoning_tokens":10},"total_tokens":3529,
                      "server_tool_use":{"tool_search":1,"tool_fetch":2}}}
            """.Replace("__WS__", WebSearchCall));
        var options = TestPrompt();
        options.Tools = new[] { WebSearchLanguageModelTool() };

        var result = await Model(capture, "deepseek-v4-pro").DoGenerateAsync(options, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(Snapshot("deepseek-responses-web-search-mixed.generate.snap.json"), GenerateSnapshot(result, capture)),
            GenerateSnapshot(result, capture).ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "throws when DeepSeek returns a failed response", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_when_DeepSeek_returns_a_failed_response()
    {
        var capture = JsonResponse("""{"status":"failed","error":{"code":"server_error","message":"boom"}}""");

        var error = await Assert.ThrowsAsync<ApiException>(() => Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None));

        Assert.Equal("boom", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.True(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps incomplete max_output_tokens to length", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_incomplete_max_output_tokens_to_length()
    {
        var capture = JsonResponse("""{"status":"incomplete","incomplete_details":{"reason":"max_output_tokens"}}""");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(FinishReason.Length, result.FinishReason);
        Assert.Equal("max_output_tokens", result.RawFinishReason);
    }
}
