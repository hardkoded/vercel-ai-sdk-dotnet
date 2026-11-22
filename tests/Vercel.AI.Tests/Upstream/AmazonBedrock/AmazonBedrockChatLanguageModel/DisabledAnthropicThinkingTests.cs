// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel.DoGenerateTests;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DisabledAnthropicThinkingTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::disabled Anthropic thinking::";
    private const string Legacy37ModelId = "us.anthropic.claude-3-7-sonnet-20250219-v1:0";
    private const string Sonnet5ModelId = "us.anthropic.claude-sonnet-5";
    private const string Sonnet55ModelId = "us.anthropic.claude-sonnet-5-5";
    private const string Opus5ModelId = "us.anthropic.claude-opus-5";
    private const string Opus55ModelId = "us.anthropic.claude-opus-5-5";

    [Theory]
    [InlineData(AnthropicModelId, "explicit", false)]
    [InlineData(Legacy37ModelId, "portable", true)]
    [InlineData(Sonnet5ModelId, "portable", true)]
    [InlineData(Sonnet5ModelId, "explicit", true)]
    [UpstreamTest(
        Prefix + "should serialize disabled thinking only when supported by %s (%s configuration)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_serialize_disabled_thinking_only_when_supported_by_model_configuration(
        string modelId,
        string source,
        bool supportsThinking
    )
    {
        var (result, body) = await Request(
            modelId,
            source == "portable"
                ? new LanguageModelCallOptions { Reasoning = "none" }
                : new LanguageModelCallOptions
                {
                    ProviderOptions = Options(
                        "{\"amazonBedrock\":{\"reasoningConfig\":{\"type\":\"disabled\"}}}"
                    ),
                }
        );

        if (supportsThinking)
        {
            AssertEqual("{\"thinking\":{\"type\":\"disabled\"}}", Fields(body)!.Value);
        }
        else
        {
            Assert.Null(Fields(body));
        }

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve request settings without adding a thinking budget or display",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_preserve_request_settings_without_adding_a_thinking_budget_or_display()
    {
        var (result, body) = await Request(
            NewerAnthropicModelId,
            new LanguageModelCallOptions
            {
                MaxOutputTokens = 1024,
                Temperature = 0.7,
                TopP = 0.9,
                TopK = 5,
                ProviderOptions = Options(
                    "{\"amazonBedrock\":{\"reasoningConfig\":{\"type\":\"disabled\",\"budgetTokens\":3000,\"display\":\"summarized\"},\"additionalModelRequestFields\":{\"custom\":\"preserved\"}}}"
                ),
            }
        );

        AssertEqual(
            "{\"custom\":\"preserved\",\"thinking\":{\"type\":\"disabled\"}}",
            Fields(body)!.Value
        );
        AssertEqual(
            "{\"maxTokens\":1024,\"temperature\":0.7,\"topP\":0.9,\"topK\":5}",
            body.GetProperty("inferenceConfig")
        );
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData(Sonnet55ModelId, "between_tools")]
    [InlineData(Opus55ModelId, "adaptive")]
    [UpstreamTest(
        Prefix + "should use supported thinking for portable none on %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_use_supported_thinking_for_portable_none(
        string modelId,
        string thinkingType
    )
    {
        var (result, body) = await Request(
            modelId,
            new LanguageModelCallOptions { Reasoning = "none" }
        );

        AssertEqual(
            thinkingType == "adaptive"
                ? "{\"thinking\":{\"type\":\"adaptive\"},\"output_config\":{\"effort\":\"low\"}}"
                : "{\"thinking\":{\"type\":\"" + thinkingType + "\"}}",
            Fields(body)!.Value
        );
        AssertWarnings(
            result,
            (
                "compatibility",
                "reasoning",
                "thinking cannot be disabled for "
                    + modelId
                    + ". Using '"
                    + thinkingType
                    + "' thinking instead."
            )
        );
    }

    [Theory]
    [InlineData(Sonnet55ModelId, "between_tools")]
    [InlineData(Opus55ModelId, "adaptive")]
    [UpstreamTest(
        Prefix + "should warn and preserve effort when explicit disabled thinking is rejected on %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_warn_and_preserve_effort_when_explicit_disabled_thinking_is_rejected(
        string modelId,
        string thinkingType
    )
    {
        var (result, body) = await Request(
            modelId,
            new LanguageModelCallOptions
            {
                ProviderOptions = Options(
                    "{\"amazonBedrock\":{\"reasoningConfig\":{\"type\":\"disabled\",\"maxReasoningEffort\":\"high\"},\"additionalModelRequestFields\":{\"thinking\":{\"type\":\"disabled\",\"budget_tokens\":3000}}}}"
                ),
            }
        );

        AssertEqual(
            "{\"thinking\":{\"type\":\"" + thinkingType + "\"},\"output_config\":{\"effort\":\"high\"}}",
            Fields(body)!.Value
        );
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.amazonBedrock.reasoningConfig", warning.Feature);
    }

    [Theory]
    [InlineData("raw", null, "max")]
    [InlineData("typed over raw", "high", "high")]
    [UpstreamTest(
        Prefix + "should preserve %s effort when portable none falls back to adaptive thinking",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_preserve_effort_when_portable_none_falls_back_to_adaptive_thinking(
        string source,
        string? typedEffort,
        string expectedEffort
    )
    {
        Assert.NotNull(source);
        var (result, body) = await Request(
            Opus55ModelId,
            new LanguageModelCallOptions
            {
                Reasoning = "none",
                ProviderOptions = Options(
                    "{\"amazonBedrock\":{"
                        + (
                            typedEffort != null
                                ? "\"reasoningConfig\":{\"maxReasoningEffort\":\"" + typedEffort + "\"},"
                                : ""
                        )
                        + "\"additionalModelRequestFields\":{\"output_config\":{\"effort\":\"max\"}}}}"
                ),
            }
        );

        AssertEqual(
            "{\"thinking\":{\"type\":\"adaptive\"},\"output_config\":{\"effort\":\""
                + expectedEffort
                + "\"}}",
            Fields(body)!.Value
        );
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
    }

    [Theory]
    [InlineData(Opus5ModelId, "typed", "xhigh", "disabled")]
    [InlineData(Sonnet55ModelId, "raw", "max", "between_tools")]
    [UpstreamTest(
        Prefix + "should lower incompatible %s %s %s effort for %s thinking",
        Coverage = UpstreamCoverage.Covered,
        Commit = "61a315e3331c0a147294fc1a391feba2631b3fa2"
    )]
    public async Task Should_lower_incompatible_effort_for_thinking(
        string modelId,
        string source,
        string effort,
        string thinkingType
    )
    {
        var (result, body) = await Request(
            modelId,
            new LanguageModelCallOptions
            {
                ProviderOptions = Options(
                    "{\"amazonBedrock\":{\"reasoningConfig\":{\"type\":\"disabled\""
                        + (
                            source == "typed"
                                ? ",\"maxReasoningEffort\":\"" + effort + "\""
                                : ""
                        )
                        + "},\"additionalModelRequestFields\":{\"custom\":\"preserved\",\"output_config\":{\"custom\":\"also-preserved\""
                        + (source == "raw" ? ",\"effort\":\"" + effort + "\"" : "")
                        + "}}}}"
                ),
            }
        );

        AssertEqual(
            "{\"custom\":\"preserved\",\"thinking\":{\"type\":\""
                + thinkingType
                + "\"},\"output_config\":{\"custom\":\"also-preserved\",\"effort\":\"high\"}}",
            Fields(body)!.Value
        );
        Assert.Contains(
            result.Warnings,
            warning =>
                warning.Type == "unsupported"
                && warning.Feature
                    == (
                        source == "typed"
                            ? "providerOptions.amazonBedrock.reasoningConfig.maxReasoningEffort"
                            : "providerOptions.amazonBedrock.additionalModelRequestFields.output_config.effort"
                    )
        );
    }

    private static Task<(LanguageModelGenerateResult Result, JsonElement Body)> Request(
        string modelId,
        LanguageModelCallOptions options
    )
    {
        options.Prompt = TestPrompt;
        return Run(modelId, TextResponse, options);
    }

    private static JsonElement? Fields(JsonElement body)
    {
        return body.TryGetProperty("additionalModelRequestFields", out var fields) ? fields : null;
    }
}
