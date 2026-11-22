// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>claude-opus-4-7 specific behavior</c>.</summary>
public sealed class AnthropicLanguageModelClaudeOpus47SpecificBehaviorTests
{
    private const string Model = "claude-opus-4-7";
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::claude-opus-4-7 specific behavior::";

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and strip temperature when set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_strip_temperature_when_set()
    {
        var options = AnthropicParity.Hello();
        options.Temperature = 0.7;
        var (body, result, _) = await Run(options);

        Assert.Null(body["temperature"]);
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "temperature"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and strip topK when set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_strip_topK_when_set()
    {
        var options = AnthropicParity.Hello();
        options.TopK = 40;
        var (body, result, _) = await Run(options);

        Assert.Null(body["top_k"]);
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "topK"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and strip topP when set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_strip_topP_when_set()
    {
        var options = AnthropicParity.Hello();
        options.TopP = 0.9;
        var (body, result, _) = await Run(options);

        Assert.Null(body["top_p"]);
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "topP"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map xhigh reasoning effort to xhigh for claude-opus-4-7",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_xhigh_reasoning_effort_to_xhigh_for_claude_opus_4_7()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "xhigh";
        var (body, _, _) = await Run(options);

        Assert.Equal("xhigh", body["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map xhigh reasoning effort to high for claude-opus-4-6",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_map_xhigh_reasoning_effort_to_high_for_claude_opus_4_6()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "xhigh";
        var (body, _, _) = await Run(options, "claude-opus-4-6");

        Assert.Equal("high", body["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include task_budget in output_config and add beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_task_budget_in_output_config_and_add_beta_header()
    {
        var options = AnthropicParity.WithProvider(
            "{\"taskBudget\":{\"type\":\"tokens\",\"total\":400000}}"
        );
        var (body, _, handler) = await Run(options);

        Assert.Equal(
            "{\"type\":\"tokens\",\"total\":400000}",
            body["output_config"]!["task_budget"]!.ToJsonString()
        );
        Assert.Contains("task-budgets-2026-03-13", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include remaining in task_budget when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_remaining_in_task_budget_when_provided()
    {
        var options = AnthropicParity.WithProvider(
            "{\"taskBudget\":{\"type\":\"tokens\",\"total\":400000,\"remaining\":215000}}"
        );
        var (body, _, _) = await Run(options);

        Assert.Equal(
            "{\"type\":\"tokens\",\"total\":400000,\"remaining\":215000}",
            body["output_config"]!["task_budget"]!.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include display in thinking block when set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_display_in_thinking_block_when_set()
    {
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"adaptive\",\"display\":\"summarized\"}}"
        );
        var (body, _, _) = await Run(options);

        Assert.Equal(
            "{\"type\":\"adaptive\",\"display\":\"summarized\"}",
            body["thinking"]!.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include the updates thinking display mode",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_updates_thinking_display_mode()
    {
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"adaptive\",\"display\":\"updates\"}}"
        );
        var (body, _, handler) = await Run(options);

        Assert.Equal(
            "{\"type\":\"adaptive\",\"display\":\"updates\"}",
            body["thinking"]!.ToJsonString()
        );
        Assert.Contains("thinking-display-updates-2026-08-18", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize thinking binding controls and add the beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_serialize_thinking_binding_controls_and_add_the_beta_header()
    {
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"blockBinding\":{\"prefixMismatchBehavior\":\"drop_block\"}}}"
        );
        var (body, _, handler) = await Run(options, "claude-fable-5");

        Assert.Equal(
            "{\"block_binding\":{\"prefix_mismatch_behavior\":\"drop_block\"}}",
            body["thinking"]!.ToJsonString()
        );
        Assert.Contains("thinking-binding-controls-2026-08-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize strict thinking binding controls with adaptive thinking",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_serialize_strict_thinking_binding_controls_with_adaptive_thinking()
    {
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"adaptive\",\"blockBinding\":{\"prefixMismatchBehavior\":\"error\"}}}"
        );
        options.MaxOutputTokens = 4096;
        var (body, _, handler) = await Run(options, "claude-fable-5-1");

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"max_tokens\":4096,\"messages\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"claude-fable-5-1\",\"thinking\":{\"block_binding\":{\"prefix_mismatch_behavior\":\"error\"},\"type\":\"adaptive\"}}"
                ),
                body
            ),
            body.ToJsonString()
        );
        Assert.Contains("thinking-binding-controls-2026-08-01", handler.Headers["anthropic-beta"]);
    }

    private static async Task<(
        JsonNode Body,
        LanguageModelGenerateResult Result,
        CaptureHandler Handler
    )> Run(LanguageModelCallOptions options, string model = Model)
    {
        var handler = AnthropicParity.Ok();
        var result = await AnthropicParity
            .Generate(AnthropicParity.Client(handler), model, options)
            .ConfigureAwait(false);
        return (JsonNode.Parse(handler.Body)!, result, handler);
    }
}
