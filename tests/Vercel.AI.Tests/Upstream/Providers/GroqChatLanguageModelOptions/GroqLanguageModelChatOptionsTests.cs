// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Groq;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.GroqChatLanguageModelOptions;

/// <summary>Port of <c>groq-chat-language-model-options.test.ts</c> &gt; <c>groqLanguageModelChatOptions</c>.</summary>
public sealed class GroqLanguageModelChatOptionsTests
{
    private const string Prefix =
        "packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > ";

    private static Dictionary<string, JsonElement> Options(string json)
    {
        return new Dictionary<string, JsonElement>
        {
            ["groq"] = JsonDocument.Parse(json).RootElement.Clone(),
        };
    }

    private static GroqLanguageModelChatOptions? SafeParse(string json, out bool success)
    {
        try
        {
            var result = global::Vercel.AI.Groq.GroqChatLanguageModelOptions.Parse(Options(json));
            success = true;
            return result;
        }
        catch (InvalidArgumentException)
        {
            success = false;
            return null;
        }
    }

    [Theory]
    [InlineData("none")]
    [InlineData("default")]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [UpstreamTest(
        Prefix + "reasoningEffort::accepts valid reasoningEffort values",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_valid_reasoningEffort_values(string value)
    {
        var result = SafeParse($$"""{"reasoningEffort":"{{value}}"}""", out var success);

        Assert.True(success);
        Assert.Equal(value, result!.ReasoningEffort);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("high-effort")]
    [InlineData("minimal")]
    [InlineData("maximum")]
    [InlineData("")]
    [UpstreamTest(
        Prefix + "reasoningEffort::rejects invalid reasoningEffort values",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_invalid_reasoningEffort_values(string value)
    {
        SafeParse($$"""{"reasoningEffort":"{{value}}"}""", out var success);

        Assert.False(success);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reasoningEffort::allows reasoningEffort to be undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Allows_reasoningEffort_to_be_undefined()
    {
        var result = SafeParse("{}", out var success);

        Assert.True(success);
        Assert.Null(result!.ReasoningEffort);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reasoningEffort::allows reasoningEffort to be omitted explicitly",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Allows_reasoningEffort_to_be_omitted_explicitly()
    {
        // `reasoningEffort: undefined` is not serialized, so the bag has no such property.
        var result = SafeParse("{}", out var success);

        Assert.True(success);
        Assert.Null(result!.ReasoningEffort);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "combined options with reasoningEffort::accepts reasoningEffort with other valid options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_reasoningEffort_with_other_valid_options()
    {
        var result = SafeParse(
            """{"reasoningEffort":"high","parallelToolCalls":true,"user":"test-user","structuredOutputs":false,"serviceTier":"flex"}""",
            out var success
        );

        Assert.True(success);
        Assert.Equal("high", result!.ReasoningEffort);
        Assert.True(result.ParallelToolCalls);
        Assert.Equal("test-user", result.User);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "combined options with reasoningEffort::rejects when reasoningEffort is invalid among valid options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_when_reasoningEffort_is_invalid_among_valid_options()
    {
        SafeParse(
            """{"reasoningEffort":"ultra-high","parallelToolCalls":true,"user":"test-user"}""",
            out var success
        );

        Assert.False(success);
    }

    [Theory]
    [InlineData("on_demand")]
    [InlineData("performance")]
    [InlineData("flex")]
    [InlineData("auto")]
    [UpstreamTest(
        Prefix + "serviceTier::accepts valid serviceTier values",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_valid_serviceTier_values(string value)
    {
        var result = SafeParse($$"""{"serviceTier":"{{value}}"}""", out var success);

        Assert.True(success);
        Assert.Equal(value, result!.ServiceTier);
    }

    [Theory]
    [InlineData("priority")]
    [InlineData("default")]
    [InlineData("turbo")]
    [InlineData("")]
    [UpstreamTest(
        Prefix + "serviceTier::rejects invalid serviceTier values",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_invalid_serviceTier_values(string value)
    {
        SafeParse($$"""{"serviceTier":"{{value}}"}""", out var success);

        Assert.False(success);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "all reasoningEffort enum variants::validates all reasoningEffort variants individually",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Validates_all_reasoningEffort_variants_individually()
    {
        foreach (var variant in new[] { "none", "default", "low", "medium", "high" })
        {
            var result = SafeParse($$"""{"reasoningEffort":"{{variant}}"}""", out var success);

            Assert.True(success);
            Assert.Equal(variant, result!.ReasoningEffort);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "type inference::infers GroqLanguageModelChatOptions type correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Infers_GroqLanguageModelChatOptions_type_correctly()
    {
        var options = new GroqLanguageModelChatOptions
        {
            ReasoningEffort = "medium",
            ParallelToolCalls = false,
        };

        Assert.Equal("medium", options.ReasoningEffort);
        Assert.False(options.ParallelToolCalls);
    }
}
