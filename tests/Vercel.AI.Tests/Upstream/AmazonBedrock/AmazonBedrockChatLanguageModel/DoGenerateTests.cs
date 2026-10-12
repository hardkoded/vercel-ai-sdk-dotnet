// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoGenerateTests
{
    private const string JsonToolResponse = "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"toolUse\":{\"toolUseId\":\"json-tool-id\",\"name\":\"json\",\"input\":{\"name\":\"Test\"}}}]}},\"usage\":{\"inputTokens\":4,\"outputTokens\":10,\"totalTokens\":14},\"stopReason\":\"tool_use\"}";

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should still use json tool fallback for structured output without thinking enabled",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_still_use_json_tool_fallback_for_structured_output_without_thinking_enabled()
    {
        var handler = new UpstreamRecordingHandler(JsonToolResponse);
        var result = await Generate(handler, "anthropic.claude-3-haiku-20240307-v1:0");

        using var request = JsonDocument.Parse(handler.Body);
        var toolConfig = request.RootElement.GetProperty("toolConfig");
        Assert.Equal(1, toolConfig.GetProperty("tools").GetArrayLength());
        Assert.Equal("json", toolConfig.GetProperty("tools")[0].GetProperty("toolSpec").GetProperty("name").GetString());
        Assert.Equal("{\"any\":{}}", toolConfig.GetProperty("toolChoice").GetRawText());
        Assert.False(request.RootElement.TryGetProperty("additionalModelRequestFields", out _));

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"name\":\"Test\"}", text.Text);
        Assert.True(result.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("isJsonResponseFromTool").GetBoolean());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use the json tool fallback for claude-opus-5 (Bedrock rejects output_config.format)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_json_tool_fallback_for_claude_opus_5_Bedrock_rejects_output_config_format()
    {
        var handler = new UpstreamRecordingHandler(JsonToolResponse);
        await Generate(handler, "us.anthropic.claude-opus-5");
        AssertJsonToolFallback(handler);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use the json tool fallback for claude-haiku-5-5 (Bedrock rejects output_config.format)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_json_tool_fallback_for_claude_haiku_5_5_Bedrock_rejects_output_config_format()
    {
        var handler = new UpstreamRecordingHandler(JsonToolResponse);
        await Generate(handler, "us.anthropic.claude-haiku-5-5");
        AssertJsonToolFallback(handler);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should transform reasoningConfig to thinking in additionalModelRequestFields",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_transform_reasoningConfig_to_thinking_in_additionalModelRequestFields()
    {
        var (_, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            MaxOutputTokens = 100,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":2000}}}"),
        });

        AssertMatches("{\"additionalModelRequestFields\":{\"thinking\":{\"type\":\"enabled\",\"budget_tokens\":2000}},\"inferenceConfig\":{\"maxTokens\":2100}}", body);
        Assert.False(body.TryGetProperty("reasoningConfig", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::merges user additionalModelRequestFields with derived thinking (generate)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_user_additionalModelRequestFields_with_derived_thinking_generate()
    {
        var (_, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":1234},\"additionalModelRequestFields\":{\"foo\":\"bar\",\"custom\":42}}}"),
        });

        Assert.False(body.TryGetProperty("reasoningConfig", out _));
        AssertMatches("{\"foo\":\"bar\",\"custom\":42,\"thinking\":{\"type\":\"enabled\",\"budget_tokens\":1234}}", body.GetProperty("additionalModelRequestFields"));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should forward display in adaptive reasoningConfig to thinking",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_display_in_adaptive_reasoningConfig_to_thinking()
    {
        var (_, body) = await Run(NewerAnthropicModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"adaptive\",\"display\":\"summarized\"}}}"),
        });

        AssertEqual("{\"type\":\"adaptive\",\"display\":\"summarized\"}", body.GetProperty("additionalModelRequestFields").GetProperty("thinking"));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::maps maxReasoningEffort for Nova without thinking (generate)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_maxReasoningEffort_for_Nova_without_thinking_generate()
    {
        var (_, body) = await Run("us.amazon.nova-2-lite-v1:0", TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"maxReasoningEffort\":\"medium\",\"budgetTokens\":2048}}}"),
        });

        AssertMatches("{\"additionalModelRequestFields\":{\"reasoningConfig\":{\"type\":\"enabled\",\"maxReasoningEffort\":\"medium\"}}}", body);
        Assert.False(body.GetProperty("additionalModelRequestFields").TryGetProperty("thinking", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::maps maxReasoningEffort to reasoning_effort for OpenAI gpt-oss models (generate)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_maxReasoningEffort_to_reasoning_effort_for_OpenAI_gpt_oss_models_generate()
    {
        var (_, body) = await Run("openai.gpt-oss-120b-1:0", TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"medium\"}}}"),
        });

        AssertMatches("{\"additionalModelRequestFields\":{\"reasoning_effort\":\"medium\"}}", body);
        var fields = body.GetProperty("additionalModelRequestFields");
        Assert.False(fields.TryGetProperty("reasoningConfig", out _));
        Assert.False(fields.TryGetProperty("thinking", out _));
    }

    [Theory]
    [InlineData("us.openai.gpt-5.6-luna")]
    [InlineData("global.openai.gpt-5.6-luna")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::maps maxReasoningEffort to nested reasoning.effort for CRIS model %s (generate)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_maxReasoningEffort_to_nested_reasoning_effort_for_CRIS_model_generate(string crisModelId)
    {
        var (_, body) = await Run(crisModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"medium\"}}}"),
        });

        AssertMatches("{\"additionalModelRequestFields\":{\"reasoning\":{\"effort\":\"medium\"}}}", body);
        var fields = body.GetProperty("additionalModelRequestFields");
        Assert.False(fields.TryGetProperty("reasoning_effort", out _));
        Assert.False(fields.TryGetProperty("reasoningConfig", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass maxReasoningEffort as output_config.effort for Anthropic models (generate)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_maxReasoningEffort_as_output_config_effort_for_Anthropic_models_generate()
    {
        var (_, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"maxReasoningEffort\":\"medium\"}}}"),
        });

        var fields = body.GetProperty("additionalModelRequestFields");
        Assert.False(fields.TryGetProperty("reasoningConfig", out _));
        AssertEqual("{\"effort\":\"medium\"}", fields.GetProperty("output_config"));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use native output_config.format instead of json tool when thinking is enabled with structured output",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_native_output_config_format_instead_of_json_tool_when_thinking_is_enabled_with_structured_output()
    {
        var (_, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a recipe") },
            JsonSchema = Schema("{\"type\":\"object\",\"properties\":{\"recipe\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}},\"required\":[\"name\",\"ingredients\"]}},\"required\":[\"recipe\"]}"),
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":2000}}}"),
        });

        Assert.False(body.TryGetProperty("toolConfig", out _));
        var fields = body.GetProperty("additionalModelRequestFields");
        AssertEqual("{\"format\":{\"type\":\"json_schema\",\"schema\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"recipe\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"},\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}},\"required\":[\"name\",\"ingredients\"]}},\"required\":[\"recipe\"]}}}", fields.GetProperty("output_config"));
        Assert.True(fields.TryGetProperty("thinking", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should merge output_config.effort and output_config.format when thinking with maxReasoningEffort and structured output",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_output_config_effort_and_output_config_format_when_thinking_with_maxReasoningEffort_and_structured_output()
    {
        var (_, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a recipe") },
            JsonSchema = Schema(NameSchema),
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":2000,\"maxReasoningEffort\":\"medium\"}}}"),
        });

        Assert.False(body.TryGetProperty("toolConfig", out _));
        AssertEqual("{\"effort\":\"medium\",\"format\":{\"type\":\"json_schema\",\"schema\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}}}", body.GetProperty("additionalModelRequestFields").GetProperty("output_config"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("jsonTool")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use JSON instructions instead of forced tool use for claude-opus-5-5 with structuredOutputMode %s",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_JSON_instructions_instead_of_forced_tool_use_for_claude_opus_5_5_with_structuredOutputMode(string? structuredOutputMode)
    {
        var (result, body) = await Run("us.anthropic.claude-opus-5-5", "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"{\\\"answer\\\":\\\"OK\\\"}\"}]}},\"usage\":{\"inputTokens\":4,\"outputTokens\":5,\"totalTokens\":9},\"stopReason\":\"end_turn\"}", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Return an answer of OK.") },
            JsonSchema = Schema("{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"]}"),
            ProviderOptions = structuredOutputMode == null ? null : Options("{\"amazonBedrock\":{\"structuredOutputMode\":\"" + structuredOutputMode + "\"}}"),
        });

        Assert.False(body.TryGetProperty("toolConfig", out _));
        Assert.False(HasOutputConfig(body));
        AssertEqual("[{\"text\":\"JSON schema:\\n{\\\"type\\\":\\\"object\\\",\\\"properties\\\":{\\\"answer\\\":{\\\"type\\\":\\\"string\\\"}},\\\"required\\\":[\\\"answer\\\"]}\\nYou MUST answer with only a JSON object that matches the JSON schema above. Do not wrap it in markdown fences or include any other text.\"}]", body.GetProperty("system"));
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"answer\":\"OK\"}", text.Text);
        Assert.False(result.ProviderMetadata?.TryGetProperty("bedrock", out _) ?? false);
    }

    [Theory]
    [InlineData("anthropic.claude-sonnet-4-6-v1")]
    [InlineData("us.anthropic.claude-haiku-4-5-20251001-v1:0")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should default to the json tool for $modelId",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_default_to_the_json_tool_for_modelId(string modelId)
    {
        var (result, body) = await Run(modelId, JsonToolResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            JsonSchema = Schema(NameSchema),
        });

        var toolConfig = body.GetProperty("toolConfig");
        Assert.Equal(1, toolConfig.GetProperty("tools").GetArrayLength());
        Assert.Equal("json", toolConfig.GetProperty("tools")[0].GetProperty("toolSpec").GetProperty("name").GetString());
        AssertEqual("{\"any\":{}}", toolConfig.GetProperty("toolChoice"));
        Assert.False(HasOutputConfigFormat(body));
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"name\":\"Test\"}", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.True(result.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("isJsonResponseFromTool").GetBoolean());
    }

    [Theory]
    [InlineData("amazonBedrock")]
    [InlineData("bedrock")]
    [InlineData("anthropic")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should force the json tool wire format with $providerOptionsName provider options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_force_the_json_tool_wire_format_with_providerOptionsName_provider_options(string providerOptionsName)
    {
        var (result, body) = await Run(NewerAnthropicModelId, JsonToolResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"" + providerOptionsName + "\":{\"structuredOutputMode\":\"jsonTool\"}}"),
            JsonSchema = Schema(NameSchema),
        });

        var toolConfig = body.GetProperty("toolConfig");
        Assert.Equal(1, toolConfig.GetProperty("tools").GetArrayLength());
        Assert.Equal("json", toolConfig.GetProperty("tools")[0].GetProperty("toolSpec").GetProperty("name").GetString());
        AssertEqual("{\"any\":{}}", toolConfig.GetProperty("toolChoice"));
        Assert.False(body.TryGetProperty("structuredOutputMode", out _));
        Assert.False(HasOutputConfigFormat(body));
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"name\":\"Test\"}", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.True(result.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("isJsonResponseFromTool").GetBoolean());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should remove a manually supplied output_config.format in jsonTool mode while preserving sibling fields",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_remove_a_manually_supplied_output_config_format_in_jsonTool_mode_while_preserving_sibling_fields()
    {
        var (_, body) = await Run(NewerAnthropicModelId, JsonToolResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"amazonBedrock\":{\"structuredOutputMode\":\"jsonTool\",\"additionalModelRequestFields\":{\"output_config\":{\"effort\":\"medium\",\"format\":{\"type\":\"manually-supplied-format\"}}}}}"),
            JsonSchema = Schema(NameSchema),
        });

        AssertEqual("{\"effort\":\"medium\"}", body.GetProperty("additionalModelRequestFields").GetProperty("output_config"));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should force output_config.format for models that auto mode routes to the json tool",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_force_output_config_format_for_models_that_auto_mode_routes_to_the_json_tool()
    {
        var (_, body) = await Run("us.anthropic.claude-opus-5", "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"{\\\"name\\\":\\\"Test\\\"}\"}]}},\"usage\":{\"inputTokens\":4,\"outputTokens\":10,\"totalTokens\":14},\"stopReason\":\"end_turn\"}", new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"amazonBedrock\":{\"structuredOutputMode\":\"outputFormat\"}}"),
            JsonSchema = Schema(NameSchema),
        });

        Assert.False(body.TryGetProperty("toolConfig", out _));
        AssertEqual("{\"type\":\"json_schema\",\"schema\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}}", body.GetProperty("additionalModelRequestFields").GetProperty("output_config").GetProperty("format"));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should prefer amazonBedrock structuredOutputMode over anthropic structuredOutputMode",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prefer_amazonBedrock_structuredOutputMode_over_anthropic_structuredOutputMode()
    {
        var (_, body) = await Run(NewerAnthropicModelId, JsonToolResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"amazonBedrock\":{\"structuredOutputMode\":\"jsonTool\"},\"anthropic\":{\"structuredOutputMode\":\"outputFormat\"}}"),
            JsonSchema = Schema(NameSchema),
        });

        AssertEqual("{\"any\":{}}", body.GetProperty("toolConfig").GetProperty("toolChoice"));
        Assert.False(HasOutputConfigFormat(body));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use native output_config.format for models with structured output support even without thinking enabled",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_native_output_config_format_for_models_with_structured_output_support_even_without_thinking_enabled()
    {
        var (_, body) = await Run("anthropic.claude-sonnet-4-5-20250929-v1:0", "{\"output\":{\"message\":{\"content\":[{\"text\":\"{\\\"name\\\":\\\"Test\\\"}\"}],\"role\":\"assistant\"}},\"usage\":{\"inputTokens\":4,\"outputTokens\":10,\"totalTokens\":14},\"stopReason\":\"end_turn\"}", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a name") },
            JsonSchema = Schema(NameSchema),
        });

        Assert.False(body.TryGetProperty("toolConfig", out _));
        AssertEqual("{\"format\":{\"type\":\"json_schema\",\"schema\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}}}", body.GetProperty("additionalModelRequestFields").GetProperty("output_config"));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should sanitize unsupported JSON schema keywords for native structured output",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_sanitize_unsupported_JSON_schema_keywords_for_native_structured_output()
    {
        var (_, body) = await Run("anthropic.claude-sonnet-4-5-20250929-v1:0", "{\"output\":{\"message\":{\"content\":[{\"text\":\"{\\\"labels\\\":[\\\"Spring\\\",\\\"Summer\\\",\\\"Autumn\\\"]}\"}],\"role\":\"assistant\"}},\"usage\":{\"inputTokens\":4,\"outputTokens\":10,\"totalTokens\":14},\"stopReason\":\"end_turn\"}", new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            JsonSchema = Schema("{\"type\":\"object\",\"properties\":{\"labels\":{\"type\":\"array\",\"maxItems\":3,\"items\":{\"type\":\"string\"}}},\"required\":[\"labels\"],\"additionalProperties\":false}"),
        });

        AssertEqual("{\"additionalProperties\":false,\"properties\":{\"labels\":{\"description\":\"max items: 3.\",\"items\":{\"type\":\"string\"},\"type\":\"array\"}},\"required\":[\"labels\"],\"type\":\"object\"}", body.GetProperty("additionalModelRequestFields").GetProperty("output_config").GetProperty("format").GetProperty("schema"));
    }

    [Theory]
    [InlineData("us.anthropic.claude-opus-4-8")]
    [InlineData("us.anthropic.claude-sonnet-5")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use JSON instructions instead of a response tool when structured output is combined with tools on %s",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_JSON_instructions_instead_of_a_response_tool_when_structured_output_is_combined_with_tools_on(string modelId)
    {
        using var toolSchema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}");
        var (result, body) = await Run(modelId, "{\"output\":{\"message\":{\"content\":[{\"text\":\"```json\\n{\\\"name\\\":\\\"Test\\\"}\\n```.\"}],\"role\":\"assistant\"}},\"usage\":{\"inputTokens\":4,\"outputTokens\":10,\"totalTokens\":14},\"stopReason\":\"end_turn\"}", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Look up and generate a name") },
            JsonSchema = Schema(NameSchema),
            Tools = new[] { new LanguageModelTool("lookupName", null, toolSchema.RootElement.Clone()) },
        });

        AssertEqual("{\"tools\":[{\"toolSpec\":{\"inputSchema\":{\"json\":{\"properties\":{},\"type\":\"object\"}},\"name\":\"lookupName\"}}]}", body.GetProperty("toolConfig"));
        Assert.False(HasOutputConfig(body));
        AssertEqual("[{\"text\":\"JSON schema:\\n{\\\"type\\\":\\\"object\\\",\\\"properties\\\":{\\\"name\\\":{\\\"type\\\":\\\"string\\\"}},\\\"required\\\":[\\\"name\\\"]}\\nYou MUST answer with only a JSON object that matches the JSON schema above. Do not wrap it in markdown fences or include any other text.\"}]", body.GetProperty("system"));
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"name\":\"Test\"}", text.Text);
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass serviceTier provider option in generate requests",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_serviceTier_provider_option_in_generate_requests()
    {
        var (_, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"serviceTier\":\"priority\"}}"),
        });

        AssertMatches("{\"serviceTier\":{\"type\":\"priority\"}}", body);
        Assert.False(body.TryGetProperty("additionalModelRequestFields", out var fields) && fields.TryGetProperty("serviceTier", out _));
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::does not classify custom model IDs containing openai. as OpenAI models",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_classify_custom_model_IDs_containing_openai_as_OpenAI_models()
    {
        var (_, body) = await Run("custom-openai.gpt-5.6-luna", TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            ProviderOptions = Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"medium\"}}}"),
        });

        AssertMatches("{\"additionalModelRequestFields\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"medium\"}}}", body);
        var fields = body.GetProperty("additionalModelRequestFields");
        Assert.False(fields.TryGetProperty("reasoning", out _));
        Assert.False(fields.TryGetProperty("reasoning_effort", out _));
    }


    [Theory]
    [InlineData("us.openai.gpt-5.6-luna")]
    [InlineData("global.openai.gpt-5.6-luna")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::strips unsupported sampling settings for OpenAI model %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Strips_unsupported_sampling_settings_for_OpenAI_model(string crisModelId)
    {
        var (result, body) = await Run(crisModelId, OpenAiResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = 0,
            TopP = 0.5,
            TopK = 5,
            StopSequences = Array.Empty<string>(),
            MaxOutputTokens = 100,
        });

        AssertEqual("{\"maxTokens\":100,\"topK\":5}", body.GetProperty("inferenceConfig"));
        AssertWarnings(
            result,
            ("unsupported", "temperature is not supported by this OpenAI model on the Converse API"),
            ("unsupported", "topP is not supported by this OpenAI model on the Converse API"),
            ("unsupported", "stopSequences is not supported by this OpenAI model on the Converse API"));
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::does not warn about clamping an unsupported OpenAI temperature",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Does_not_warn_about_clamping_an_unsupported_OpenAI_temperature()
    {
        var (result, body) = await Run("us.openai.gpt-5.6-luna", OpenAiResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = 2,
        });

        Assert.False(body.TryGetProperty("inferenceConfig", out _));
        AssertWarnings(result, ("unsupported", "temperature is not supported by this OpenAI model on the Converse API"));
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::keeps supported sampling settings for OpenAI gpt-oss models",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Keeps_supported_sampling_settings_for_OpenAI_gpt_oss_models()
    {
        var (result, body) = await Run("openai.gpt-oss-120b-1:0", OpenAiResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = 0.2,
            TopP = 0.5,
            StopSequences = new[] { "END" },
        });

        AssertEqual("{\"temperature\":0.2,\"topP\":0.5}", body.GetProperty("inferenceConfig"));
        AssertWarnings(result, ("unsupported", "stopSequences is not supported by this OpenAI model on the Converse API"));
    }


    [Theory]
    [InlineData("global.anthropic.claude-opus-4-7")]
    [InlineData("eu.anthropic.claude-opus-4-8")]
    [InlineData("us.anthropic.claude-opus-5")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should omit unsupported sampling parameters for %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Should_omit_unsupported_sampling_parameters_for(string samplingModelId)
    {
        var (result, body) = await Run(samplingModelId, "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"OK\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}", new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = 0.5,
            TopP = 0.7,
            TopK = 10,
        });

        if (body.TryGetProperty("inferenceConfig", out var inference))
        {
            Assert.False(inference.TryGetProperty("temperature", out _));
            Assert.False(inference.TryGetProperty("topP", out _));
            Assert.False(inference.TryGetProperty("topK", out _));
        }

        AssertWarnings(
            result,
            ("unsupported", "temperature is not supported by " + samplingModelId + " and will be ignored"),
            ("unsupported", "topK is not supported by " + samplingModelId + " and will be ignored"),
            ("unsupported", "topP is not supported by " + samplingModelId + " and will be ignored"));
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should clamp temperature above 1 to 1 and add warning",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Should_clamp_temperature_above_1_to_1_and_add_warning()
    {
        var (result, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = 1.5,
        });

        Assert.Equal(1, body.GetProperty("inferenceConfig").GetProperty("temperature").GetDouble());
        AssertWarnings(result, ("unsupported", "1.5 exceeds bedrock maximum of 1.0. clamped to 1.0"));
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should clamp temperature below 0 to 0 and add warning",
        Coverage = UpstreamCoverage.Partial,
        Note = "Warnings are type plus message rather than feature plus details.")]
    public async Task Should_clamp_temperature_below_0_to_0_and_add_warning()
    {
        var (result, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = -0.5,
        });

        Assert.Equal(0, body.GetProperty("inferenceConfig").GetProperty("temperature").GetDouble());
        AssertWarnings(result, ("unsupported", "-0.5 is below bedrock minimum of 0. clamped to 0"));
    }


    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should not clamp valid temperature between 0 and 1",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_clamp_valid_temperature_between_0_and_1()
    {
        var (result, body) = await Run(HaikuModelId, TextResponse, new LanguageModelCallOptions
        {
            Prompt = TestPrompt,
            Temperature = 0.7,
        });

        Assert.Equal(0.7, body.GetProperty("inferenceConfig").GetProperty("temperature").GetDouble());
        Assert.Empty(result.Warnings);
    }


    private static void AssertJsonToolFallback(UpstreamRecordingHandler handler)
    {
        using var request = JsonDocument.Parse(handler.Body);
        Assert.Equal("json", request.RootElement.GetProperty("toolConfig").GetProperty("tools")[0].GetProperty("toolSpec").GetProperty("name").GetString());
        Assert.False(request.RootElement.TryGetProperty("additionalModelRequestFields", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should properly combine headers from all sources",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_properly_combine_headers_from_all_sources()
    {
        var handler = new UpstreamRecordingHandler(TextResponse);
        await HeadersTestHelper.Generate(
            handler,
            new Dictionary<string, string?> { ["model-header"] = "model-value", ["shared-header"] = "model-shared" },
            new Dictionary<string, string?> { ["options-header"] = "options-value", ["shared-header"] = "options-shared" });

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
        Assert.Equal("options-shared", handler.Headers["shared-header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should work with partial headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_work_with_partial_headers()
    {
        var handler = new UpstreamRecordingHandler(TextResponse);
        await HeadersTestHelper.Generate(
            handler,
            new Dictionary<string, string?> { ["model-header"] = "model-value" },
            new Dictionary<string, string?> { ["options-header"] = "options-value" });

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
    }


    private const string OpenAiResponse = "{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello\"}],\"role\":\"assistant\"}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";
    internal const string HaikuModelId = "anthropic.claude-3-haiku-20240307-v1:0";
    internal const string NewerAnthropicModelId = "anthropic.claude-sonnet-4-6-v1";
    private const string NameSchema = "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}";
    internal const string TextResponse = "{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello\"}],\"role\":\"assistant\"}},\"stopReason\":\"stop_sequence\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";

    internal static readonly ModelMessage[] TestPrompt =
    {
        new SystemModelMessage("System Prompt"),
        new UserModelMessage("Hello"),
    };

    internal static Dictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        var result = new Dictionary<string, JsonElement>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            result[property.Name] = property.Value.Clone();
        }

        return result;
    }

    private static JsonElement Schema(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static async Task<(LanguageModelGenerateResult Result, JsonElement Body)> Run(string modelId, string response, LanguageModelCallOptions options)
    {
        var handler = new UpstreamRecordingHandler(response);
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        var result = await provider.LanguageModel(modelId).DoGenerateAsync(options, CancellationToken.None);
        using var document = JsonDocument.Parse(handler.Body);
        return (result, document.RootElement.Clone());
    }

    private static bool HasOutputConfig(JsonElement body)
    {
        return body.TryGetProperty("additionalModelRequestFields", out var fields) && fields.TryGetProperty("output_config", out _);
    }

    private static bool HasOutputConfigFormat(JsonElement body)
    {
        return body.TryGetProperty("additionalModelRequestFields", out var fields)
            && fields.TryGetProperty("output_config", out var config)
            && config.TryGetProperty("format", out _);
    }

    internal static void AssertWarnings(LanguageModelGenerateResult result, params (string Type, string Message)[] expected)
    {
        Assert.Equal(expected, result.Warnings.Select(warning => (warning.Type, warning.Message)).ToArray());
    }

    internal static void AssertEqual(string expectedJson, JsonElement actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expectedJson), JsonNode.Parse(actual.GetRawText())), "Expected " + expectedJson + " but was " + actual.GetRawText());
    }

    // Mirrors vitest's toMatchObject: every expected property must match, extra actual properties are fine.
    internal static void AssertMatches(string expectedJson, JsonElement actual)
    {
        Assert.True(Matches(JsonNode.Parse(expectedJson), JsonNode.Parse(actual.GetRawText())), "Expected " + expectedJson + " to match " + actual.GetRawText());
    }

    private static bool Matches(JsonNode? expected, JsonNode? actual)
    {
        if (expected is JsonObject expectedObject)
        {
            if (actual is not JsonObject actualObject)
            {
                return false;
            }

            foreach (var property in expectedObject)
            {
                if (!actualObject.TryGetPropertyValue(property.Key, out var value) || !Matches(property.Value, value))
                {
                    return false;
                }
            }

            return true;
        }

        return JsonNode.DeepEquals(expected, actual);
    }

    private static async Task<LanguageModelGenerateResult> Generate(UpstreamRecordingHandler handler, string modelId)
    {
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        using var schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}");
        return await provider.LanguageModel(modelId).DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a name") },
            JsonSchema = schema.RootElement.Clone(),
        }, CancellationToken.None);
    }
}

internal static class HeadersTestHelper
{
    public static async Task Generate(UpstreamRecordingHandler handler, IReadOnlyDictionary<string, string?> providerHeaders, IReadOnlyDictionary<string, string?> callHeaders)
    {
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret",
            Headers = (Dictionary<string, string?>)providerHeaders,
        }, handler);
        await provider.LanguageModel("anthropic.claude-3-haiku-20240307-v1:0").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Headers = callHeaders,
        }, CancellationToken.None);
    }
}
