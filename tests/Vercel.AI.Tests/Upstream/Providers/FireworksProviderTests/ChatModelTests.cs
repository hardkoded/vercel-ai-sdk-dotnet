// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.FireworksProviderTests;

/// <summary>Port of <c>fireworks-provider.test.ts</c> &gt; <c>FireworksProvider &gt; chatModel</c>.</summary>
public sealed class ChatModelTests
{
    private const string Prefix =
        "packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::";

    [Theory]
    [InlineData("test-model", "none", "none", false)]
    [InlineData("test-model", "minimal", "minimal", false)]
    [InlineData("test-model", "xhigh", "xhigh", false)]
    [InlineData("test-model", "max", "max", false)]
    [InlineData("accounts/fireworks/models/kimi-k3", "max", "max", false)]
    [InlineData("accounts/fireworks/models/gpt-oss-120b", "none", "minimal", true)]
    [InlineData("accounts/fireworks/models/glm-5p3", "none", "minimal", true)]
    [InlineData("accounts/fireworks/routers/glm-5p3-fast", "none", "minimal", true)]
    [InlineData("accounts/fireworks/models/muse-spark-open", "none", "minimal", true)]
    [InlineData("accounts/fireworks/models/nemotron-3-ultra-nvfp4", "minimal", "low", true)]
    [InlineData("accounts/fireworks/models/inkling", "minimal", "low", true)]
    [UpstreamTest(
        Prefix + "should map top-level reasoning for %s from %s to reasoning_effort %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c9c6402cbdfce3f82b36b4073236043b1946dd06"
    )]
    public void Should_map_top_level_reasoning_to_reasoning_effort(
        string modelId,
        string reasoning,
        string effort,
        bool hasWarning
    )
    {
        var provider = Vercel.AI.Fireworks.FireworksProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "secret" }
        );
        var warnings = new List<CallWarning>();

        var mapped = provider.Options.ResolveReasoningEffort!(modelId, reasoning, null, warnings);

        Assert.Equal(effort, mapped);
        if (hasWarning)
        {
            var warning = Assert.Single(warnings);
            Assert.Equal("compatibility", warning.Type);
            Assert.Equal("reasoning", warning.Feature);
            Assert.Equal(
                "reasoning \""
                    + reasoning
                    + "\" is not directly supported by this model. mapped to effort \""
                    + effort
                    + "\".",
                warning.Message
            );
        }
        else
        {
            Assert.Empty(warnings);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through provider reasoning_effort values unchanged",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c9c6402cbdfce3f82b36b4073236043b1946dd06"
    )]
    public void Should_pass_through_provider_reasoning_effort_values_unchanged()
    {
        var body = new JsonObject
        {
            ["model"] = "test-model",
            ["messages"] = new JsonArray(),
            ["reasoning_effort"] = "max",
        };

        var provider = Vercel.AI.Fireworks.FireworksProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "secret" }
        );

        var transformed = provider.Options.TransformRequestBody!(body, new List<CallWarning>());

        Assert.Equal(
            "{\"model\":\"test-model\",\"messages\":[],\"reasoning_effort\":\"max\"}",
            transformed.ToJsonString()
        );
    }
}
