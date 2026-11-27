// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; code execution 20260120 tool</c>.</summary>
public sealed class DoStreamCodeExecution20260120ToolTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > code execution 20260120 tool::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream code execution tool results without beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_code_execution_tool_results_without_beta_header()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20260120",
                "code_execution",
                AnthropicParity.Json("""{}""")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-code-execution-20250825.1")
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "code-execution-20260120-stream");
    }
}
