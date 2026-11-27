// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";
    private const string Suite = Chat + "doGenerate > top-level reasoning::";

    private static async Task<JsonElement> Body(LanguageModelCallOptions options)
    {
        var handler = JsonFixture("cohere-text");
        await Model(handler).DoGenerateAsync(options, CancellationToken.None);
        return Json(handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should map top-level reasoning to thinking enabled with budget",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_top_level_reasoning_to_thinking_enabled_with_budget()
    {
        var options = TestPrompt();
        options.Reasoning = "high";

        var thinking = (await Body(options)).GetProperty("thinking");

        Assert.Equal("enabled", thinking.GetProperty("type").GetString());
        Assert.Equal(JsonValueKind.Number, thinking.GetProperty("token_budget").ValueKind);
        Assert.True(thinking.GetProperty("token_budget").GetInt32() > 0);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should map top-level reasoning none to thinking disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_top_level_reasoning_none_to_thinking_disabled()
    {
        var options = TestPrompt();
        options.Reasoning = "none";

        JsonAssert.Equal((await Body(options)).GetProperty("thinking"), "{\"type\":\"disabled\"}");
    }

    [Fact]
    [UpstreamTest(
        Suite + "should prefer providerOptions over top-level reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_providerOptions_over_top_level_reasoning()
    {
        var options = TestPrompt();
        options.Reasoning = "none";
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["cohere"] = Json("{\"thinking\":{\"type\":\"enabled\"}}"),
        };

        var thinking = (await Body(options)).GetProperty("thinking");

        Assert.Equal("enabled", thinking.GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest(
        Suite + "should not set thinking when reasoning is not specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_set_thinking_when_reasoning_is_not_specified()
    {
        Assert.False((await Body(TestPrompt())).TryGetProperty("thinking", out _));
    }
}
