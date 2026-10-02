// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Converse command bodies for tools, reasoning, and structured output.</summary>
public sealed class BedrockConverseRequestTests
{
    private const string Haiku = "anthropic.claude-3-haiku-20240307-v1:0";

    private const string Sonnet46 = "anthropic.claude-sonnet-4-6-v1";

    private const string Sonnet45 = "anthropic.claude-sonnet-4-5-20250929-v1:0";

    private const string GptOss = "openai.gpt-oss-120b-1:0";

    private const string Nova = "us.amazon.nova-2-lite-v1:0";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate request metadata::should return the request body", Coverage = UpstreamCoverage.Covered)]
    public void Returns_system_messages_and_the_stop_sequence_path()
    {
        var prepared = BedrockParity.Prepare(Haiku, BedrockParity.Call());
        var body = prepared.Body;

        Assert.Equal("System Prompt", body["system"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("Hello", body["messages"]![0]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("/delta/stop_sequence", body["additionalModelResponseFieldPaths"]![0]!.GetValue<string>());
        Assert.Null(body["additionalModelRequestFields"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass the model and the messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_prompt_on_the_converse_route()
    {
        var handler = new BedrockJsonHandler(TextResponse("ok"));
        var model = BedrockParity.Model(handler, Haiku);
        await model.DoGenerateAsync(BedrockParity.Call(), CancellationToken.None);

        using var document = JsonDocument.Parse(handler.Body);
        Assert.Contains("/model/" + Uri.EscapeDataString(Haiku) + "/converse", handler.Uri, StringComparison.Ordinal);
        Assert.Equal("System Prompt", document.RootElement.GetProperty("system")[0].GetProperty("text").GetString());
        Assert.Equal("Hello", document.RootElement.GetProperty("messages")[0].GetProperty("content")[0].GetProperty("text").GetString());
        Assert.Equal("/delta/stop_sequence", document.RootElement.GetProperty("additionalModelResponseFieldPaths")[0].GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doStream > text::should return the request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_the_same_converse_prompt()
    {
        var handler = new BedrockJsonHandler(TextResponse("ok"));
        var model = BedrockParity.Model(handler, Haiku);
        await foreach (var part in model.DoStreamAsync(BedrockParity.Call(), CancellationToken.None))
        {
            _ = part;
        }

        using var document = JsonDocument.Parse(handler.Body);
        Assert.Equal("Hello", document.RootElement.GetProperty("messages")[0].GetProperty("content")[0].GetProperty("text").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::request URL > ARN model IDs containing a slash::should generate text through the encoded Converse route", Coverage = UpstreamCoverage.Covered)]
    public async Task Encodes_a_slash_in_an_arn_model_id()
    {
        const string arn = "arn:aws:bedrock:eu-west-1:474668406012:inference-profile/eu.amazon.nova-lite-v1:0";
        var handler = new BedrockJsonHandler(TextResponse("ok"));
        var model = BedrockParity.Model(handler, arn);
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Contains("/model/" + Uri.EscapeDataString(arn) + "/converse", handler.Uri, StringComparison.Ordinal);
        Assert.DoesNotContain("/inference-profile/eu.amazon", handler.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass settings", Coverage = UpstreamCoverage.Covered)]
    public void Copies_sampling_settings_into_inference_config()
    {
        var options = BedrockParity.Call();
        options.MaxOutputTokens = 100;
        options.Temperature = 0.5;
        options.TopP = 0.5;
        options.TopK = 1;
        var inference = BedrockParity.Prepare(Haiku, options).Body["inferenceConfig"]!;

        Assert.Equal(100, inference["maxTokens"]!.GetValue<int>());
        Assert.Equal(0.5, inference["temperature"]!.GetValue<double>());
        Assert.Equal(0.5, inference["topP"]!.GetValue<double>());
        Assert.Equal(1, inference["topK"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should omit unsupported sampling parameters for %s", Coverage = UpstreamCoverage.Covered)]
    public void Omits_sampling_parameters_rejected_by_newer_opus_models()
    {
        foreach (var modelId in new[] { "global.anthropic.claude-opus-4-7", "eu.anthropic.claude-opus-4-8", "us.anthropic.claude-opus-5" })
        {
            var options = BedrockParity.Call();
            options.Temperature = 0.5;
            options.TopP = 0.7;
            options.TopK = 10;
            var prepared = BedrockParity.Prepare(modelId, options);

            Assert.Null(prepared.Body["inferenceConfig"]);
            Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "temperature is not supported by " + modelId + " and will be ignored"));
            Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "topK", "topK is not supported by " + modelId + " and will be ignored"));
            Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "topP", "topP is not supported by " + modelId + " and will be ignored"));
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should clamp temperature above 1 to 1 and add warning", Coverage = UpstreamCoverage.Covered)]
    public void Clamps_temperature_above_one()
    {
        var options = BedrockParity.Call();
        options.Temperature = 1.5;
        var prepared = BedrockParity.Prepare(Haiku, options);

        Assert.Equal(1, prepared.Body["inferenceConfig"]!["temperature"]!.GetValue<double>());
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "1.5 exceeds bedrock maximum of 1.0. clamped to 1.0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should clamp temperature below 0 to 0 and add warning", Coverage = UpstreamCoverage.Covered)]
    public void Clamps_temperature_below_zero()
    {
        var options = BedrockParity.Call();
        options.Temperature = -0.2;
        var prepared = BedrockParity.Prepare(Haiku, options);

        Assert.Equal(0, prepared.Body["inferenceConfig"]!["temperature"]!.GetValue<double>());
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "-0.2 is below bedrock minimum of 0. clamped to 0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should not clamp valid temperature between 0 and 1", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_temperature_inside_the_bedrock_range()
    {
        var options = BedrockParity.Call();
        options.Temperature = 0.4;
        var prepared = BedrockParity.Prepare(Haiku, options);

        Assert.Equal(0.4, prepared.Body["inferenceConfig"]!["temperature"]!.GetValue<double>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass tools and tool choice correctly", Coverage = UpstreamCoverage.Covered)]
    public void Sends_function_tools_and_a_named_tool_choice()
    {
        var options = BedrockParity.Call();
        options.Tools = new[] { Tool("lookup", "{\"type\":\"object\",\"properties\":{}}") };
        options.ToolChoice = ToolChoice.Tool("lookup");
        var toolConfig = BedrockParity.Prepare(Nova, options).Body["toolConfig"]!;

        Assert.Equal("lookup", toolConfig["tools"]![0]!["toolSpec"]!["name"]!.GetValue<string>());
        Assert.Equal("lookup", toolConfig["toolChoice"]!["tool"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should disable parallel tool use without sending conflicting tool choices", Coverage = UpstreamCoverage.Covered)]
    public void Disables_parallel_tool_use_through_anthropic_tool_choice()
    {
        var options = BedrockParity.Call();
        options.Tools = new[] { Tool("lookup", "{\"type\":\"object\"}") };
        options.ProviderOptions = BedrockParity.Options("{\"anthropic\":{\"disableParallelToolUse\":true}}");
        var body = BedrockParity.Prepare(Haiku, options).Body;

        Assert.Null(body["toolConfig"]!["toolChoice"]);
        Assert.Equal("auto", body["additionalModelRequestFields"]!["tool_choice"]!["type"]!.GetValue<string>());
        Assert.True(body["additionalModelRequestFields"]!["tool_choice"]!["disable_parallel_tool_use"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should only send the forced tool when toolChoice specifies a specific tool", Coverage = UpstreamCoverage.Covered)]
    public void Sends_only_the_forced_tool()
    {
        var options = BedrockParity.Call();
        options.Tools = new[]
        {
            Tool("getWeather", "{\"type\":\"object\"}"),
            Tool("getTime", "{\"type\":\"object\"}"),
        };
        options.ToolChoice = ToolChoice.Tool("getWeather");
        var tools = BedrockParity.Prepare(Nova, options).Body["toolConfig"]!["tools"]!.AsArray();

        Assert.Single(tools);
        Assert.Equal("getWeather", tools[0]!["toolSpec"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should send all tools when toolChoice is auto", Coverage = UpstreamCoverage.Covered)]
    public void Sends_every_tool_when_choice_is_auto()
    {
        var options = BedrockParity.Call();
        options.Tools = new[] { Tool("getWeather", "{\"type\":\"object\"}"), Tool("getTime", "{\"type\":\"object\"}") };
        options.ToolChoice = ToolChoice.Auto;
        var tools = BedrockParity.Prepare(Nova, options).Body["toolConfig"]!["tools"]!.AsArray();

        Assert.Equal(2, tools.Count);
        Assert.Empty(BedrockParity.Prepare(Nova, options).Body["toolConfig"]!["toolChoice"]!["auto"]!.AsObject());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should omit empty tool descriptions to avoid Bedrock validation errors", Coverage = UpstreamCoverage.Covered)]
    public void Omits_an_empty_tool_description_from_the_command()
    {
        var options = BedrockParity.Call();
        options.Tools = new[] { Tool("lookup", "{\"type\":\"object\"}", string.Empty) };
        var spec = BedrockParity.Prepare(Haiku, options).Body["toolConfig"]!["tools"]![0]!["toolSpec"]!;

        Assert.Null(spec["description"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include anthropic_beta in additionalModelRequestFields when using extended context", Coverage = UpstreamCoverage.Covered)]
    public void Copies_anthropic_beta_into_additional_fields()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"anthropicBeta\":[\"context-1m-2025-08-07\"]}}");
        var betas = BedrockParity.Prepare(Haiku, options).Body["additionalModelRequestFields"]!["anthropic_beta"]!.AsArray();

        Assert.Equal("context-1m-2025-08-07", betas[0]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should not include anthropic-beta in HTTP headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_send_anthropic_beta_as_an_http_header()
    {
        var handler = new BedrockJsonHandler(TextResponse("ok"));
        var model = BedrockParity.Model(handler, Haiku);
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"anthropicBeta\":[\"context-1m-2025-08-07\"]}}");
        var prepared = BedrockParity.Prepare(Haiku, options);
        await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("context-1m-2025-08-07", prepared.Body["additionalModelRequestFields"]!["anthropic_beta"]![0]!.GetValue<string>());
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass serviceTier provider option in generate requests", Coverage = UpstreamCoverage.Covered)]
    public void Puts_service_tier_on_the_prepared_command()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"serviceTier\":\"priority\"}}");
        var prepared = BedrockParity.Prepare(Haiku, options);

        Assert.Equal("priority", prepared.Body["serviceTier"]!["type"]!.GetValue<string>());
        Assert.False(prepared.CreateTransportBody().ContainsKey("serviceTier"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should transform reasoningConfig to thinking in additionalModelRequestFields", Coverage = UpstreamCoverage.Covered)]
    public void Turns_an_enabled_reasoning_budget_into_thinking()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":1024}}}");
        var thinking = BedrockParity.Prepare(Haiku, options).Body["additionalModelRequestFields"]!["thinking"]!;

        Assert.Equal("enabled", thinking["type"]!.GetValue<string>());
        Assert.Equal(1024, thinking["budget_tokens"]!.GetValue<int>());
        Assert.Equal(5120, BedrockParity.Prepare(Haiku, options).Body["inferenceConfig"]!["maxTokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should forward display in adaptive reasoningConfig to thinking", Coverage = UpstreamCoverage.Covered)]
    public void Forwards_adaptive_display_into_thinking()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"adaptive\",\"display\":\"summarized\",\"maxReasoningEffort\":\"high\"}}}");
        var additional = BedrockParity.Prepare(Sonnet46, options).Body["additionalModelRequestFields"]!;

        Assert.Equal("adaptive", additional["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("summarized", additional["thinking"]!["display"]!.GetValue<string>());
        Assert.Equal("high", additional["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should not set reasoning config when reasoning is \"provider-default\" for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_provider_default_reasoning_unset()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "provider-default";
        var body = BedrockParity.Prepare(Sonnet46, options).Body;

        Assert.Null(body["additionalModelRequestFields"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should map reasoning to adaptive thinking with effort for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Maps_high_reasoning_to_adaptive_thinking()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        var additional = BedrockParity.Prepare(Sonnet46, options).Body["additionalModelRequestFields"]!;

        Assert.Equal("adaptive", additional["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("high", additional["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should map reasoning \"xhigh\" to effort \"max\" for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Maps_xhigh_reasoning_to_max_effort()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "xhigh";
        var prepared = BedrockParity.Prepare(Sonnet46, options);

        Assert.Equal("adaptive", prepared.Body["additionalModelRequestFields"]!["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("max", prepared.Body["additionalModelRequestFields"]!["output_config"]!["effort"]!.GetValue<string>());
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "compatibility", "reasoning", "reasoning \"xhigh\" is not directly supported by this model. mapped to effort \"max\"."));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should warn when reasoning \"minimal\" is mapped for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_minimal_reasoning_maps_to_low()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "minimal";
        var prepared = BedrockParity.Prepare(Sonnet46, options);

        Assert.Equal("low", prepared.Body["additionalModelRequestFields"]!["output_config"]!["effort"]!.GetValue<string>());
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "compatibility", "reasoning", "reasoning \"minimal\" is not directly supported by this model. mapped to effort \"low\"."));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should map reasoning to budget-based thinking for older Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Maps_high_reasoning_to_a_token_budget_on_haiku()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        var thinking = BedrockParity.Prepare(Haiku, options).Body["additionalModelRequestFields"]!["thinking"]!;

        Assert.Equal("enabled", thinking["type"]!.GetValue<string>());
        Assert.Equal(2458, thinking["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should map reasoning to budget with minimum of 1024 tokens for older Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Raises_a_small_reasoning_budget_to_1024()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "minimal";
        var thinking = BedrockParity.Prepare(Haiku, options).Body["additionalModelRequestFields"]!["thinking"]!;

        Assert.Equal(1024, thinking["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should map reasoning directly to reasoning_effort for OpenAI gpt-oss models", Coverage = UpstreamCoverage.Covered)]
    public void Maps_gpt_oss_reasoning_to_reasoning_effort()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "medium";
        var additional = BedrockParity.Prepare(GptOss, options).Body["additionalModelRequestFields"]!;

        Assert.Equal("medium", additional["reasoning_effort"]!.GetValue<string>());
        Assert.Null(additional["thinking"]);
        Assert.Null(additional["reasoningConfig"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should map reasoning to reasoningConfig.maxReasoningEffort for other models", Coverage = UpstreamCoverage.Covered)]
    public void Maps_nova_reasoning_to_max_reasoning_effort()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        var config = BedrockParity.Prepare(Nova, options).Body["additionalModelRequestFields"]!["reasoningConfig"]!;

        Assert.Equal("high", config["maxReasoningEffort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::maps maxReasoningEffort for Nova without thinking (generate)", Coverage = UpstreamCoverage.Covered)]
    public void Maps_an_explicit_nova_effort_without_thinking()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"low\"}}}");
        var config = BedrockParity.Prepare(Nova, options).Body["additionalModelRequestFields"]!["reasoningConfig"]!;

        Assert.Equal("low", config["maxReasoningEffort"]!.GetValue<string>());
        Assert.Null(config["type"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::maps maxReasoningEffort to reasoning_effort for OpenAI gpt-oss models (generate)", Coverage = UpstreamCoverage.Covered)]
    public void Maps_an_explicit_gpt_oss_effort()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"high\"}}}");

        Assert.Equal("high", BedrockParity.Prepare(GptOss, options).Body["additionalModelRequestFields"]!["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::maps maxReasoningEffort to nested reasoning.effort for CRIS model %s (generate)", Coverage = UpstreamCoverage.Covered)]
    public void Maps_effort_into_nested_reasoning_for_openai_models()
    {
        foreach (var modelId in new[] { "us.openai.gpt-5.6-luna", "global.openai.gpt-5.6-luna" })
        {
            var options = BedrockParity.Call();
            options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"medium\"}}}");
            var additional = BedrockParity.Prepare(modelId, options).Body["additionalModelRequestFields"]!;

            Assert.Equal("medium", additional["reasoning"]!["effort"]!.GetValue<string>());
            Assert.Null(additional["reasoning_effort"]);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::does not classify custom model IDs containing openai. as OpenAI models", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_treat_a_custom_id_as_openai()
    {
        var options = BedrockParity.Call();
        options.Temperature = 1.5;
        var prepared = BedrockParity.Prepare("custom-openai.gpt-5.6-luna", options);

        Assert.Equal(1, prepared.Body["inferenceConfig"]!["temperature"]!.GetValue<double>());
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "1.5 exceeds bedrock maximum of 1.0. clamped to 1.0"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::strips unsupported sampling settings for OpenAI model %s", Coverage = UpstreamCoverage.Covered)]
    public void Strips_sampling_settings_for_non_gpt_oss_openai_models()
    {
        foreach (var modelId in new[] { "openai.gpt-4o", "us.openai.gpt-5.6-luna" })
        {
            var options = BedrockParity.Call();
            options.Temperature = 0.2;
            options.TopP = 0.3;
            options.StopSequences = new[] { "END" };
            var prepared = BedrockParity.Prepare(modelId, options);

            Assert.Null(prepared.Body["inferenceConfig"]);
            Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "temperature is not supported by this OpenAI model on the Converse API"));
            Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "stopSequences", "stopSequences is not supported by this OpenAI model on the Converse API"));
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::does not warn about clamping an unsupported OpenAI temperature", Coverage = UpstreamCoverage.Covered)]
    public void Drops_an_openai_temperature_without_clamping_it()
    {
        var options = BedrockParity.Call();
        options.Temperature = 1.5;
        var prepared = BedrockParity.Prepare("openai.gpt-4o", options);

        Assert.Null(prepared.Body["inferenceConfig"]);
        Assert.False(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "1.5 exceeds bedrock maximum of 1.0. clamped to 1.0"));
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "temperature is not supported by this OpenAI model on the Converse API"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::keeps supported sampling settings for OpenAI gpt-oss models", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_temperature_and_top_p_for_gpt_oss()
    {
        var options = BedrockParity.Call();
        options.Temperature = 0.4;
        options.TopP = 0.6;
        options.StopSequences = new[] { "END" };
        var prepared = BedrockParity.Prepare(GptOss, options);

        Assert.Equal(0.4, prepared.Body["inferenceConfig"]!["temperature"]!.GetValue<double>());
        Assert.Equal(0.6, prepared.Body["inferenceConfig"]!["topP"]!.GetValue<double>());
        Assert.Null(prepared.Body["inferenceConfig"]!["stopSequences"]);
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "stopSequences", "stopSequences is not supported by this OpenAI model on the Converse API"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should pass maxReasoningEffort as output_config.effort for Anthropic models (generate)", Coverage = UpstreamCoverage.Covered)]
    public void Sends_anthropic_effort_on_output_config()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"high\"}}}");

        Assert.Equal("high", BedrockParity.Prepare(Sonnet46, options).Body["additionalModelRequestFields"]!["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should let explicit reasoningConfig fields win over derived reasoning values", Coverage = UpstreamCoverage.Covered)]
    public void Lets_an_explicit_budget_replace_the_derived_budget()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":5000}}}");
        var thinking = BedrockParity.Prepare(Haiku, options).Body["additionalModelRequestFields"]!["thinking"]!;

        Assert.Equal("enabled", thinking["type"]!.GetValue<string>());
        Assert.Equal(5000, thinking["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should merge top-level reasoning with partial reasoningConfig for newer Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Merges_display_onto_derived_adaptive_thinking()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"display\":\"summarized\"}}}");
        var additional = BedrockParity.Prepare(Sonnet46, options).Body["additionalModelRequestFields"]!;

        Assert.Equal("adaptive", additional["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("summarized", additional["thinking"]!["display"]!.GetValue<string>());
        Assert.Equal("high", additional["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should honor reasoning \"none\" even when partial reasoningConfig is provided", Coverage = UpstreamCoverage.Covered)]
    public void Disables_thinking_when_reasoning_is_none()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "none";
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"display\":\"summarized\"}}}");
        var body = BedrockParity.Prepare(Sonnet46, options).Body;

        Assert.Null(body["additionalModelRequestFields"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should let user-specified type win while still deriving maxReasoningEffort from reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_user_thinking_type_and_the_derived_effort()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"type\":\"enabled\",\"budgetTokens\":3000}}}");
        var additional = BedrockParity.Prepare(Sonnet46, options).Body["additionalModelRequestFields"]!;

        Assert.Equal("enabled", additional["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(3000, additional["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Equal("high", additional["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should let user-specified maxReasoningEffort win over derived for non-Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Lets_an_explicit_nova_effort_win()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"reasoningConfig\":{\"maxReasoningEffort\":\"low\"}}}");

        Assert.Equal("low", BedrockParity.Prepare(Nova, options).Body["additionalModelRequestFields"]!["reasoningConfig"]!["maxReasoningEffort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should strip temperature, topP, topK for Anthropic models when reasoning enables thinking", Coverage = UpstreamCoverage.Covered)]
    public void Strips_sampling_parameters_when_thinking_is_enabled()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "high";
        options.Temperature = 0.7;
        options.TopP = 0.9;
        options.TopK = 5;
        var prepared = BedrockParity.Prepare(Haiku, options);
        var inference = prepared.Body["inferenceConfig"]!;

        Assert.Null(inference["temperature"]);
        Assert.Null(inference["topP"]);
        Assert.Null(inference["topK"]);
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "temperature", "temperature is not supported when thinking is enabled"));
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "topP", "topP is not supported when thinking is enabled"));
        Assert.True(BedrockParity.HasWarning(prepared.Warnings, "unsupported", "topK", "topK is not supported when thinking is enabled"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should handle reasoning \"none\" for Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_enable_thinking_when_anthropic_reasoning_is_none()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "none";

        Assert.Null(BedrockParity.Prepare(Haiku, options).Body["additionalModelRequestFields"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > top-level reasoning parameter::should handle reasoning \"none\" for OpenAI models", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_set_reasoning_effort_when_openai_reasoning_is_none()
    {
        var options = BedrockParity.Call();
        options.Reasoning = "none";

        Assert.Null(BedrockParity.Prepare(GptOss, options).Body["additionalModelRequestFields"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should still use json tool fallback for structured output without thinking enabled", Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_json_tool_for_haiku_structured_output()
    {
        var options = SchemaCall();
        var prepared = BedrockParity.Prepare(Haiku, options);
        var tools = prepared.Body["toolConfig"]!["tools"]!.AsArray();

        Assert.True(prepared.UsesJsonResponseTool);
        Assert.Equal("json", tools[0]!["toolSpec"]!["name"]!.GetValue<string>());
        Assert.Equal("Respond with a JSON object.", tools[0]!["toolSpec"]!["description"]!.GetValue<string>());
        Assert.NotNull(prepared.Body["toolConfig"]!["toolChoice"]!["any"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with json tool response::should pass json schema response format as a tool", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_json_schema_as_the_json_tool_input_schema()
    {
        var tools = BedrockParity.Prepare(Haiku, SchemaCall()).Body["toolConfig"]!["tools"]!.AsArray();

        Assert.Equal("object", tools[0]!["toolSpec"]!["inputSchema"]!["json"]!["type"]!.GetValue<string>());
        Assert.Equal("string", tools[0]!["toolSpec"]!["inputSchema"]!["json"]!["properties"]!["name"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use native output_config.format for models with structured output support even without thinking enabled", Coverage = UpstreamCoverage.Covered)]
    public void Uses_native_json_schema_for_sonnet_4_5()
    {
        var prepared = BedrockParity.Prepare(Sonnet45, SchemaCall());
        var format = prepared.Body["additionalModelRequestFields"]!["output_config"]!["format"]!;

        Assert.False(prepared.UsesJsonResponseTool);
        Assert.Equal("json_schema", format["type"]!.GetValue<string>());
        Assert.Equal("object", format["schema"]!["type"]!.GetValue<string>());
        Assert.Null(prepared.Body["toolConfig"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use the json tool fallback for claude-opus-5 (Bedrock rejects output_config.format)", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_json_tool_for_opus_5()
    {
        var prepared = BedrockParity.Prepare("us.anthropic.claude-opus-5", SchemaCall());

        Assert.True(prepared.UsesJsonResponseTool);
        Assert.Equal("json", prepared.Body["toolConfig"]!["tools"]![0]!["toolSpec"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use JSON instructions instead of forced tool use for claude-opus-5-5 with structuredOutputMode %s", Coverage = UpstreamCoverage.Covered)]
    public void Injects_json_instructions_for_opus_5_5()
    {
        foreach (var mode in new string?[] { null, "jsonTool" })
        {
            var options = BedrockParity.Call("Return an answer of OK.", null);
            options.JsonSchema = BedrockParity.Element("{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"]}");
            if (mode != null)
            {
                options.ProviderOptions = BedrockParity.Options("{\"amazonBedrock\":{\"structuredOutputMode\":\"" + mode + "\"}}");
            }

            var prepared = BedrockParity.Prepare("anthropic.claude-opus-5-5", options);
            var expected = "JSON schema:\n{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"]}\nYou MUST answer with only a JSON object that matches the JSON schema above. Do not wrap it in markdown fences or include any other text.";

            Assert.True(prepared.UsesJsonInstruction);
            Assert.Null(prepared.Body["toolConfig"]);
            Assert.Null(prepared.Body["additionalModelRequestFields"]);
            Assert.Equal(expected, prepared.Body["system"]![0]!["text"]!.GetValue<string>());
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should force the json tool wire format with $providerOptionsName provider options", Coverage = UpstreamCoverage.Covered)]
    public void Forces_the_json_tool_when_the_mode_says_so()
    {
        foreach (var key in new[] { "amazon-bedrock", "amazonBedrock", "bedrock" })
        {
            var options = SchemaCall();
            options.ProviderOptions = BedrockParity.Options("{\"" + key + "\":{\"structuredOutputMode\":\"jsonTool\"}}");
            var prepared = BedrockParity.Prepare(Sonnet45, options);

            Assert.True(prepared.UsesJsonResponseTool);
            Assert.Equal("json", prepared.Body["toolConfig"]!["tools"]![0]!["toolSpec"]!["name"]!.GetValue<string>());
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should remove a manually supplied output_config.format in jsonTool mode while preserving sibling fields", Coverage = UpstreamCoverage.Covered)]
    public void Removes_output_format_and_keeps_sibling_fields_in_json_tool_mode()
    {
        var options = SchemaCall();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"structuredOutputMode\":\"jsonTool\",\"additionalModelRequestFields\":{\"output_config\":{\"format\":{\"type\":\"json_schema\"},\"effort\":\"low\"},\"top_k\":1}}}");
        var additional = BedrockParity.Prepare(Sonnet45, options).Body["additionalModelRequestFields"]!;

        Assert.Null(additional["output_config"]!["format"]);
        Assert.Equal("low", additional["output_config"]!["effort"]!.GetValue<string>());
        Assert.Equal(1, additional["top_k"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should prefer amazonBedrock structuredOutputMode over anthropic structuredOutputMode", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_bedrock_structured_output_mode()
    {
        var options = SchemaCall();
        options.ProviderOptions = BedrockParity.Options("{\"amazonBedrock\":{\"structuredOutputMode\":\"jsonTool\"},\"anthropic\":{\"structuredOutputMode\":\"outputFormat\"}}");
        var prepared = BedrockParity.Prepare(Sonnet45, options);

        Assert.True(prepared.UsesJsonResponseTool);
        Assert.False(prepared.UsesJsonInstruction);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should support guardrails", Coverage = UpstreamCoverage.Covered)]
    public void Copies_guardrail_config_onto_the_command()
    {
        var options = BedrockParity.Call();
        options.ProviderOptions = BedrockParity.Options("{\"bedrock\":{\"guardrailConfig\":{\"guardrailIdentifier\":\"-1\",\"guardrailVersion\":\"1\",\"trace\":\"enabled\"}}}");
        var guard = BedrockParity.Prepare(Haiku, options).Body["guardrailConfig"]!;

        Assert.Equal("-1", guard["guardrailIdentifier"]!.GetValue<string>());
        Assert.Equal("enabled", guard["trace"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should send text parts as guardContent blocks when providerOptions.bedrock.guardContent is set", Coverage = UpstreamCoverage.Covered)]
    public void Sends_guard_content_text_from_provider_options()
    {
        var text = new AmazonBedrockTextPart("guard me")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":true}"),
        };
        var prepared = AmazonBedrockMessages.Convert(new[] { AmazonBedrockPromptMessage.User(new AmazonBedrockPromptPart[] { text }) });

        Assert.Equal("guard me", prepared.Messages[0]!["content"]![0]!["guardContent"]!["text"]!["text"]!.GetValue<string>());
    }

    private static LanguageModelCallOptions SchemaCall()
    {
        var options = BedrockParity.Call("Hello", "System Prompt");
        options.JsonSchema = BedrockParity.Element("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}");
        return options;
    }

    private static LanguageModelTool Tool(string name, string schema, string? description = "tool")
    {
        return new LanguageModelTool(name, description, BedrockParity.Element(schema));
    }

    private static string TextResponse(string text)
    {
        return "{\"output\":{\"message\":{\"content\":[{\"text\":\"" + text + "\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";
    }
}
