// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; code_interpreter tool</c>.</summary>
public sealed class DoGenerateCodeInterpreterToolTests
{
    private const string Prefix = TestFile + " > doGenerate > code_interpreter tool::";

    [Fact]
    [UpstreamTest(Prefix + "should include code_interpreter tool call with real response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_code_interpreter_tool_call_with_real_response()
    {
        var result = await Model(JsonFixture("xai-code-execution-tool.1")).DoGenerateAsync(
            WithTools(ProviderTool("xai.code_execution", "code_execution")),
            CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), Expected("xai-code-execution-tool.1.content.json").ToJsonString());
    }
}
