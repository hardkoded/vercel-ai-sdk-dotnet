// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; advisor tool</c>.</summary>
public sealed class DoStreamAdvisorToolTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > advisor tool::";

    private static async Task<List<LanguageModelStreamPart>> StreamAsync()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.advisor_20260301",
                "advisor",
                AnthropicParity.Json("""{"model":"claude-opus-4-7"}""")
            ),
        };
        return await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-advisor-20250301.1")),
            "claude-sonnet-4-6",
            options
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit the advisor server_tool_use as a provider-executed tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_the_advisor_server_tool_use_as_a_provider_executed_tool_call()
    {
        var parts = await StreamAsync();

        var advisorToolCall = parts.First(part =>
            part is ToolCallStreamPart { ToolName: "advisor" }
        );

        DoStreamSupport.AssertSnapshot(
            DoStreamSupport.SnapshotOf(advisorToolCall),
            "advisor-tool-call"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit the fully formed advisor result before executor text resumes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_the_fully_formed_advisor_result_before_executor_text_resumes()
    {
        var parts = await StreamAsync();

        var advisorToolResultIndex = parts.FindIndex(part =>
            part is ToolResultStreamPart { ToolName: "advisor" }
        );
        var nextTextStartIndex = parts.FindIndex(advisorToolResultIndex + 1, part =>
            part is TextStartStreamPart
        );

        Assert.True(advisorToolResultIndex > -1);
        Assert.True(nextTextStartIndex > advisorToolResultIndex);
        var result = (ToolResultStreamPart)parts[advisorToolResultIndex];
        Assert.Equal("tool-result", result.Type);
        Assert.Equal("srvtoolu_01R6zRtm9VnRaSJUVkGK9zvM", result.ToolCallId);
        Assert.Equal("advisor", result.ToolName);
        Assert.Equal("advisor_result", result.Result.GetProperty("type").GetString());
        Assert.Contains(
            "Concurrent Worker Pool in Go",
            result.Result.GetProperty("text").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose advisor usage iterations on the finish part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_advisor_usage_iterations_on_the_finish_part()
    {
        var parts = await StreamAsync();

        var finishPart = DoStreamSupport.Finish(parts);

        DoStreamSupport.AssertSnapshot(
            DoStreamSupport.AnthropicMetadata(finishPart, "iterations"),
            "advisor-usage-iterations"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream stopReason for plaintext and redacted advisor results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_stopReason_for_plaintext_and_redacted_advisor_results()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.advisor_20260301",
                "advisor",
                AnthropicParity.Json("""{"model":"claude-opus-4-7","maxTokens":2048}""")
            ),
        };
        var resultParts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-advisor-stop-reasons")
            ),
            "claude-sonnet-4-6",
            options
        );

        DoStreamSupport.AssertSnapshot(
            new JsonArray(
                resultParts
                    .Where(part => part is ToolResultStreamPart)
                    .Select(part => (JsonNode?)DoStreamSupport.SnapshotOf(part))
                    .ToArray()
            ),
            "advisor-stop-reasons"
        );
    }
}
