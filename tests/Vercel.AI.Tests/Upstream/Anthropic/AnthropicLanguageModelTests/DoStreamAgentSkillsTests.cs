// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; agent skills</c>.</summary>
public sealed class DoStreamAgentSkillsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > agent skills::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream code execution tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_code_execution_tool_results()
    {
        var options = AnthropicParity.WithProvider(
            "{\"container\":{\"skills\":[{\"type\":\"anthropic\",\"skillId\":\"pptx\"}]}}"
        );
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
                DoStreamSupport.FixtureStream("anthropic-code-execution-20250825.pptx-skill")
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "agent-skills-stream");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve the text editor discriminator for streamed skill tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_text_editor_discriminator_for_streamed_skill_tool_calls()
    {
        var options = AnthropicParity.WithProvider(
            "{\"container\":{\"skills\":[{\"type\":\"anthropic\",\"skillId\":\"pptx\"}]}}"
        );
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
                DoStreamSupport.FixtureStream("anthropic-code-execution-20250825.pptx-skill")
            ),
            "claude-3-haiku-20240307",
            options
        );

        var skillReadToolCall = parts
            .OfType<ToolCallStreamPart>()
            .First(part =>
                part.ToolName == "code_execution"
                && part.ArgumentsJson.Contains("/skills/pptx/SKILL.md")
            );

        Assert.Equal("code_execution", skillReadToolCall.ToolName);
        Assert.Equal(
            "{\"type\": \"text_editor_code_execution\",\"command\": \"view\", \"path\": \"/skills/pptx/SKILL.md\"}",
            skillReadToolCall.ArgumentsJson
        );
        Assert.True(skillReadToolCall.ProviderExecuted);
    }
}
