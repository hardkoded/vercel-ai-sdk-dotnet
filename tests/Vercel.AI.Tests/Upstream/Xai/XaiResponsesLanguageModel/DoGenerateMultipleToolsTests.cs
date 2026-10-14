// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; multiple tools</c>.</summary>
public sealed class DoGenerateMultipleToolsTests
{
    private const string Prefix = TestFile + " > doGenerate > multiple tools::";

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple server-side tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_multiple_server_side_tools()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[
              {"type":"web_search_call","id":"ws_123","name":"web_search","arguments":"{}","call_id":"","status":"completed"},
              {"type":"code_interpreter_call","id":"code_123","name":"code_execution","arguments":"{}","call_id":"","status":"completed"}],"usage":{"input_tokens":10,"output_tokens":5}}
            """);

        var result = await Model(capture).DoGenerateAsync(
            WithTools(ProviderTool("xai.web_search", "web_search"), ProviderTool("xai.code_execution", "code_execution")),
            CancellationToken.None);

        Assert.Equal(new[] { "tool-call", "tool-result", "tool-call" }, result.Content.Select(part => part.Type));
    }
}
