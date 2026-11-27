// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of the <c>effort with thinking disabled</c> tests of <c>anthropic-language-model.test.ts</c>.</summary>
public sealed class EffortWithThinkingDisabledTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::effort with thinking disabled::";

    [Theory]
    [InlineData("xhigh")]
    [InlineData("max")]
    [UpstreamTest(
        Prefix
            + "should warn and lower effort to high when thinking is disabled with effort %s on an unknown Claude model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_lower_effort_to_high_when_thinking_is_disabled_with_effort_on_an_unknown_Claude_model(
        string effort
    )
    {
        var handler = AnthropicParity.Ok();
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"" + effort + "\"}"
        );
        options.MaxOutputTokens = 1024;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-future-9",
            options
        );

        var requestBody = JsonNode.Parse(handler.Body)!;
        Assert.Equal("high", (string?)requestBody["output_config"]!["effort"]);
        JsonAssert.Equal(requestBody["thinking"], """{"type":"disabled"}""");
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "providerOptions.anthropic.effort"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep effort high when thinking is disabled with effort high",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_effort_high_when_thinking_is_disabled_with_effort_high()
    {
        var handler = AnthropicParity.Ok();
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"high\"}"
        );
        options.MaxOutputTokens = 1024;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-future-9",
            options
        );

        var requestBody = JsonNode.Parse(handler.Body)!;
        Assert.Equal("high", (string?)requestBody["output_config"]!["effort"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not lower effort for models without the constraint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_lower_effort_for_models_without_the_constraint()
    {
        var handler = AnthropicParity.Ok();

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-opus-4-8",
            AnthropicParity.WithProvider(
                "{\"thinking\":{\"type\":\"disabled\"},\"effort\":\"xhigh\"}"
            )
        );

        var requestBody = JsonNode.Parse(handler.Body)!;
        Assert.Equal("xhigh", (string?)requestBody["output_config"]!["effort"]);
    }
}
