// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; function tools</c>.</summary>
public sealed class DoGenerateFunctionToolsTests
{
    private const string Prefix = TestFile + " > doGenerate > function tools::";

    [Fact]
    [UpstreamTest(Prefix + "should include function tool calls without providerExecuted", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_function_tool_calls_without_providerExecuted()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[{"type":"function_call","id":"fc_123","name":"weather","arguments":"{\"location\":\"sf\"}","call_id":"call_123"}],
             "usage":{"input_tokens":10,"output_tokens":5}}
            """);
        var options = HelloPrompt();
        options.Tools = new[] { WeatherTool() };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_123", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"location\":\"sf\"}", call.ArgumentsJson);
    }
}
