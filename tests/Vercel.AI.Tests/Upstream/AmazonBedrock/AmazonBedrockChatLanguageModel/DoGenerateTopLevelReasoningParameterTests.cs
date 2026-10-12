// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel.DoGenerateTests;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoGenerateTopLevelReasoningParameterTests
{
    private const string Prefix = "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::";
    private const string NovaModelId = "us.amazon.nova-2-lite-v1:0";
    private const string OpenAiModelId = "openai.gpt-oss-120b-1:0";

    [Fact]
    [UpstreamTest(Prefix + "should not set reasoning config when reasoning is \"provider-default\" for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_set_reasoning_config_when_reasoning_is_provider_default_for_newer_Anthropic_models()
    {
        var (_, body) = await Generate(NewerAnthropicModelId, "provider-default");

        Assert.Null(Field(body, "thinking"));
        Assert.Null(Field(body, "output_config"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning to adaptive thinking with effort for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_to_adaptive_thinking_with_effort_for_newer_Anthropic_models()
    {
        var (_, body) = await Generate(NewerAnthropicModelId, "high");

        AssertEqual("{\"type\":\"adaptive\"}", Field(body, "thinking")!.Value);
        Assert.Equal("high", Field(body, "output_config")!.Value.GetProperty("effort").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning \"xhigh\" to effort \"max\" for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_xhigh_to_effort_max_for_newer_Anthropic_models()
    {
        var (_, body) = await Generate(NewerAnthropicModelId, "xhigh");

        AssertEqual("{\"type\":\"adaptive\"}", Field(body, "thinking")!.Value);
        Assert.Equal("max", Field(body, "output_config")!.Value.GetProperty("effort").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when reasoning \"minimal\" is mapped for newer Anthropic models", Coverage = UpstreamCoverage.Partial, Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Should_warn_when_reasoning_minimal_is_mapped_for_newer_Anthropic_models()
    {
        var (result, body) = await Generate(NewerAnthropicModelId, "minimal");

        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility"
            && warning.Message == "reasoning \"minimal\" is not directly supported by this model. mapped to effort \"low\".");
        Assert.Equal("low", Field(body, "output_config")!.Value.GetProperty("effort").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning to budget-based thinking for older Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_to_budget_based_thinking_for_older_Anthropic_models()
    {
        var (_, body) = await Generate(HaikuModelId, "high");

        AssertEqual("{\"type\":\"enabled\",\"budget_tokens\":" + Math.Max(1024, (int)Math.Round(4096 * 0.6)) + "}", Field(body, "thinking")!.Value);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map reasoning to budget with minimum of 1024 tokens for older Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_reasoning_to_budget_with_minimum_of_1024_tokens_for_older_Anthropic_models()
    {
        var (_, body) = await Generate(HaikuModelId, "minimal");

        Assert.Equal(1024, Field(body, "thinking")!.Value.GetProperty("budget_tokens").GetInt32());
    }

    [Theory]
    [InlineData("medium")]
    [UpstreamTest(Prefix + "should map reasoning \"%s\" directly to reasoning_effort for OpenAI gpt-oss models", Coverage = UpstreamCoverage.Partial, Note = "Covers medium. The port has no portable max reasoning level.")]
    public async Task Should_map_reasoning_directly_to_reasoning_effort_for_OpenAI_gpt_oss_models(string reasoning)
    {
        var (result, body) = await Generate(OpenAiModelId, reasoning);

        Assert.Equal(reasoning, Field(body, "reasoning_effort")!.Value.GetString());
        Assert.Null(Field(body, "thinking"));
        Assert.Null(Field(body, "reasoningConfig"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map portable reasoning for Nova 2", Coverage = UpstreamCoverage.Partial, Note = "The port does not send type enabled in the Nova 2 reasoningConfig.")]
    public async Task Should_map_portable_reasoning_for_Nova_2()
    {
        var (_, body) = await Generate(NovaModelId, "high");

        Assert.Equal("high", Field(body, "reasoningConfig")!.Value.GetProperty("maxReasoningEffort").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should let explicit reasoningConfig fields win over derived reasoning values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_let_explicit_reasoningConfig_fields_win_over_derived_reasoning_values()
    {
        var (_, body) = await Generate(HaikuModelId, "high", "{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":5000}}}");

        AssertEqual("{\"type\":\"enabled\",\"budget_tokens\":5000}", Field(body, "thinking")!.Value);
    }

    [Theory]
    [InlineData("amazonBedrock")]
    [InlineData("bedrock")]
    [UpstreamTest(Prefix + "should derive portable effort with display-only %s reasoningConfig for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_derive_portable_effort_with_display_only_reasoningConfig_for_newer_Anthropic_models(string providerKey)
    {
        var (result, body) = await Generate(NewerAnthropicModelId, "high", "{\"" + providerKey + "\":{\"reasoningConfig\":{\"display\":\"summarized\"}}}");

        AssertEqual("{\"type\":\"adaptive\",\"display\":\"summarized\"}", Field(body, "thinking")!.Value);
        Assert.Equal("high", Field(body, "output_config")!.Value.GetProperty("effort").GetString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should honor reasoning \"none\" with display-only reasoningConfig", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_honor_reasoning_none_with_display_only_reasoningConfig()
    {
        var (result, body) = await Generate(NewerAnthropicModelId, "none", "{\"bedrock\":{\"reasoningConfig\":{\"display\":\"summarized\"}}}");

        Assert.Null(Field(body, "thinking"));
        Assert.Null(Field(body, "output_config"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve an explicit thinking budget alongside portable effort for adaptive-capable models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_an_explicit_thinking_budget_alongside_portable_effort_for_adaptive_capable_models()
    {
        var (_, body) = await Generate(NewerAnthropicModelId, "high", "{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":3000}}}");

        AssertEqual("{\"type\":\"enabled\",\"budget_tokens\":3000}", Field(body, "thinking")!.Value);
        Assert.Equal("high", Field(body, "output_config")!.Value.GetProperty("effort").GetString());
    }

    [Theory]
    [InlineData("amazonBedrock")]
    [InlineData("bedrock")]
    [UpstreamTest(Prefix + "should ignore portable max without warnings when %s reasoning effort is explicit", Coverage = UpstreamCoverage.Partial, Note = "The port has no portable max reasoning level, so it still warns that max is unsupported.")]
    public async Task Should_ignore_portable_max_without_warnings_when_reasoning_effort_is_explicit(string providerKey)
    {
        var (_, body) = await Run(NovaModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Reasoning = "max",
            MaxOutputTokens = 1024,
            ProviderOptions = Options("{\"" + providerKey + "\":{\"reasoningConfig\":{\"type\":\"enabled\",\"maxReasoningEffort\":\"low\"}}}"),
        });

        AssertEqual("{\"type\":\"enabled\",\"maxReasoningEffort\":\"low\"}", Field(body, "reasoningConfig")!.Value);
        Assert.Equal(1024, body.GetProperty("inferenceConfig").GetProperty("maxTokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(Prefix + "should strip temperature, topP, topK for Anthropic models when reasoning enables thinking", Coverage = UpstreamCoverage.Partial, Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Should_strip_temperature_topP_topK_for_Anthropic_models_when_reasoning_enables_thinking()
    {
        var (result, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Reasoning = "high",
            Temperature = 0.7,
            TopP = 0.9,
            TopK = 5,
        });

        var inference = body.GetProperty("inferenceConfig");
        Assert.False(inference.TryGetProperty("temperature", out _));
        Assert.False(inference.TryGetProperty("topP", out _));
        Assert.False(inference.TryGetProperty("topK", out _));
        var warnings = result.Warnings.Select(warning => (warning.Type, warning.Message)).ToArray();
        Assert.Contains(("unsupported", "temperature is not supported when thinking is enabled"), warnings);
        Assert.Contains(("unsupported", "topP is not supported when thinking is enabled"), warnings);
        Assert.Contains(("unsupported", "topK is not supported when thinking is enabled"), warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle reasoning \"none\" for Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_reasoning_none_for_Anthropic_models()
    {
        var (_, body) = await Generate(HaikuModelId, "none");

        Assert.Null(Field(body, "thinking"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle reasoning \"none\" for OpenAI models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_reasoning_none_for_OpenAI_models()
    {
        var (_, body) = await Generate(OpenAiModelId, "none");

        Assert.Null(Field(body, "reasoning_effort"));
    }

    private static Task<(LanguageModelGenerateResult Result, JsonElement Body)> Generate(string modelId, string reasoning, string? providerOptions = null)
    {
        return Run(modelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Reasoning = reasoning,
            ProviderOptions = providerOptions == null ? null : Options(providerOptions),
        });
    }

    // Reads requestBody.additionalModelRequestFields?.<name>, which is undefined when either level is missing.
    private static JsonElement? Field(JsonElement body, string name)
    {
        return body.TryGetProperty("additionalModelRequestFields", out var fields) && fields.TryGetProperty(name, out var value)
            ? value
            : null;
    }
}
