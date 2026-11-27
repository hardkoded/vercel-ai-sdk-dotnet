// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; code execution 20250825 tool</c>.</summary>
public sealed class DoStreamCodeExecution20250825ToolTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > code execution 20250825 tool::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream code execution tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_code_execution_tool_results()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20250825",
                "code_execution",
                AnthropicParity.Json("{}")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-code-execution-20250825.1")
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "code-execution-20250825-stream");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include file id list in code execution tool call result.",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream ends the snapshot with an error part for the empty event after the fixture's last line (AI_JSONParseError). The port's SSE reader skips empty events, so that error part is not asserted."
    )]
    public async Task Should_include_file_id_list_in_code_execution_tool_call_result()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20250825",
                "code_execution",
                AnthropicParity.Json("{}")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-code-execution-20250825.2")
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(
            parts,
            "code-execution-20250825-file-id-list-stream",
            trailingErrorNotReported: true
        );
    }
}
