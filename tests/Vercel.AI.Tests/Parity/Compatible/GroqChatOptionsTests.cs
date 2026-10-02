// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Groq;

namespace Vercel.AI.Tests;

public sealed class GroqChatOptionsTests
{
    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > reasoningEffort::accepts valid reasoningEffort values", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_valid_reasoning_effort()
    {
        foreach (var value in new[] { "none", "default", "low", "medium", "high" })
        {
            var parsed = GroqChatOptions.TryParse(Parse("{\"reasoningEffort\":\"" + value + "\"}"));
            Assert.True(parsed.Success);
            Assert.Equal(value, parsed.Options!.ReasoningEffort);
        }
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > reasoningEffort::rejects invalid reasoningEffort values", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_reasoning_effort()
    {
        foreach (var value in new[] { "invalid", "high-effort", "minimal", "maximum", "" })
        {
            var json = value.Length == 0 ? "{\"reasoningEffort\":\"\"}" : "{\"reasoningEffort\":\"" + value + "\"}";
            Assert.False(GroqChatOptions.TryParse(Parse(json)).Success);
        }
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > reasoningEffort::allows reasoningEffort to be undefined", Coverage = UpstreamCoverage.Covered)]
    public void Allows_omitted_reasoning_effort()
    {
        var parsed = GroqChatOptions.TryParse(Parse("{}"));
        Assert.True(parsed.Success);
        Assert.Null(parsed.Options!.ReasoningEffort);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > reasoningEffort::allows reasoningEffort to be omitted explicitly", Coverage = UpstreamCoverage.Covered)]
    public void Allows_null_reasoning_effort()
    {
        var parsed = GroqChatOptions.TryParse(Parse("{\"reasoningEffort\":null}"));
        Assert.True(parsed.Success);
        Assert.Null(parsed.Options!.ReasoningEffort);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > combined options with reasoningEffort::accepts reasoningEffort with other valid options", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_reasoning_effort_with_other_options()
    {
        var parsed = GroqChatOptions.TryParse(Parse("{\"reasoningEffort\":\"high\",\"parallelToolCalls\":true,\"user\":\"test-user\",\"structuredOutputs\":false,\"serviceTier\":\"flex\"}"));
        Assert.True(parsed.Success);
        Assert.Equal("high", parsed.Options!.ReasoningEffort);
        Assert.True(parsed.Options.ParallelToolCalls);
        Assert.Equal("test-user", parsed.Options.User);
        Assert.False(parsed.Options.StructuredOutputs);
        Assert.Equal("flex", parsed.Options.ServiceTier);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > combined options with reasoningEffort::rejects when reasoningEffort is invalid among valid options", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_reasoning_effort_among_valid_options()
    {
        var parsed = GroqChatOptions.TryParse(Parse("{\"reasoningEffort\":\"ultra-high\",\"parallelToolCalls\":true,\"user\":\"test-user\"}"));
        Assert.False(parsed.Success);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > serviceTier::accepts valid serviceTier values", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_valid_service_tier()
    {
        foreach (var value in new[] { "on_demand", "performance", "flex", "auto" })
        {
            var parsed = GroqChatOptions.TryParse(Parse("{\"serviceTier\":\"" + value + "\"}"));
            Assert.True(parsed.Success);
            Assert.Equal(value, parsed.Options!.ServiceTier);
        }
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > serviceTier::rejects invalid serviceTier values", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_service_tier()
    {
        foreach (var value in new[] { "priority", "default", "turbo", "" })
        {
            var json = value.Length == 0 ? "{\"serviceTier\":\"\"}" : "{\"serviceTier\":\"" + value + "\"}";
            Assert.False(GroqChatOptions.TryParse(Parse(json)).Success);
        }
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > all reasoningEffort enum variants::validates all reasoningEffort variants individually", Coverage = UpstreamCoverage.Covered)]
    public void Validates_each_reasoning_effort_variant()
    {
        foreach (var value in new[] { "none", "default", "low", "medium", "high" })
        {
            var parsed = GroqChatOptions.TryParse(Parse("{\"reasoningEffort\":\"" + value + "\"}"));
            Assert.True(parsed.Success);
            Assert.Equal(value, parsed.Options!.ReasoningEffort);
        }
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model-options.test.ts::groqLanguageModelChatOptions > type inference::infers GroqLanguageModelChatOptions type correctly", Coverage = UpstreamCoverage.Covered)]
    public void Options_round_trip_assigned_values()
    {
        var options = new GroqChatOptions
        {
            ReasoningEffort = "medium",
            ParallelToolCalls = false,
        };
        Assert.Equal("medium", options.ReasoningEffort);
        Assert.False(options.ParallelToolCalls);
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
