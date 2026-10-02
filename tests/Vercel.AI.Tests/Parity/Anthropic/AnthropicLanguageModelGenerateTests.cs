// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

/// <summary>Covers Anthropic doGenerate request bodies, parsed results, and HTTP status mapping.</summary>
public sealed class AnthropicLanguageModelGenerateTests
{

    private const string TextResponse = @"{
        ""id"":""msg_017TfcQ4AgGxKyBduUpqYPZn"",
        ""type"":""message"",
        ""role"":""assistant"",
        ""content"":[{""type"":""text"",""text"":""Hello, World!""}],
        ""model"":""claude-3-haiku-20240307"",
        ""stop_reason"":""end_turn"",
        ""stop_sequence"":null,
        ""usage"":{""input_tokens"":4,""output_tokens"":30}}";

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should pass thinking config; add budget tokens; clear out temperature, top_p, top_k; and return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Thinking_budget_is_added_and_sampling_is_stripped()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-5");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.MaxOutputTokens = 20000;
            options.Temperature = 0.5;
            options.TopP = 0.7;
            options.TopK = 1;
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""thinking"":{""type"":""enabled"",""budgetTokens"":1000}}");
        }), CancellationToken.None);

        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal(21000, body["max_tokens"]!.GetValue<int>());
        AnthropicParity.JsonEqual(body["thinking"], @"{""type"":""enabled"",""budget_tokens"":1000}");
        Assert.Null(body["temperature"]);
        Assert.Null(body["top_p"]);
        Assert.Null(body["top_k"]);
        Assert.Contains(result.Warnings, warning => warning.Message == "temperature is not supported when thinking is enabled");
        Assert.Contains(result.Warnings, warning => warning.Message == "topK is not supported when thinking is enabled");
        Assert.Contains(result.Warnings, warning => warning.Message == "topP is not supported when thinking is enabled");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should extract reasoning response", Coverage = UpstreamCoverage.Covered)]
    public async Task Thinking_blocks_become_reasoning_content()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{
            ""id"":""msg_017TfcQ4AgGxKyBduUpqYPZn"",""content"":[
                {""type"":""thinking"",""thinking"":""I am thinking..."",""signature"":""1234567890""},
                {""type"":""text"",""text"":""Hello, World!""}],
            ""stop_reason"":""end_turn"",""usage"":{""input_tokens"":4,""output_tokens"":30}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        var reasoning = Assert.IsType<GeneratedReasoning>(result.Content[0]);
        var text = Assert.IsType<GeneratedText>(result.Content[1]);
        Assert.Equal("I am thinking...", reasoning.Text);
        Assert.Equal("Hello, World!", text.Text);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should use default budget when thinking type is enabled without budgetTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Enabled_thinking_without_a_budget_uses_1024()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-haiku-4-5");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""thinking"":{""type"":""enabled""}}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal(64000, body["max_tokens"]!.GetValue<int>());
        Assert.Equal(1024, body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Message.Contains("default budget of 1024"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (adaptive thinking)::should send adaptive thinking without budget_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_thinking_omits_budget_tokens()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""thinking"":{""type"":""adaptive"",""display"":""summarized""}}");
        }), CancellationToken.None);
        var thinking = JsonNode.Parse(handler.Body!)!["thinking"]!;
        Assert.Equal("adaptive", thinking["type"]!.GetValue<string>());
        Assert.Null(thinking["budget_tokens"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (adaptive thinking)::should report thinking tokens as reasoning usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Thinking_tokens_are_reasoning_usage()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = @"{""id"":""msg"",""content"":[{""type"":""text"",""text"":""ok""}],""stop_reason"":""end_turn"",""usage"":{""input_tokens"":3,""output_tokens"":8,""output_tokens_details"":{""thinking_tokens"":5}}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        Assert.Equal(5, result.Usage.ReasoningTokens);
        Assert.Equal(8, result.Usage.OutputTokens);
        Assert.Equal(3, result.Usage.TextTokens);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking disabled)::should forward thinking { type: \"disabled\" } to the API instead of stripping it", Coverage = UpstreamCoverage.Covered)]
    public async Task Disabled_thinking_is_forwarded()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Temperature = 0.2;
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""thinking"":{""type"":""disabled""}}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        AnthropicParity.JsonEqual(body["thinking"], @"{""type"":""disabled""}");
        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should not set thinking config when reasoning is \"provider-default\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_default_reasoning_omits_thinking()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options => options.Reasoning = "provider-default"), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Null(body["thinking"]);
        Assert.Null(body["output_config"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"none\" to thinking disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_model_maps_none_to_disabled()
    {
        var body = await ReasoningBody("claude-sonnet-4-6", "none");
        AnthropicParity.JsonEqual(body["thinking"], @"{""type"":""disabled""}");
        Assert.Null(body["output_config"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"low\" to adaptive thinking with effort \"low\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_low_effort()
    {
        await AssertAdaptive("low", "low");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"medium\" to adaptive thinking with effort \"medium\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_medium_effort()
    {
        await AssertAdaptive("medium", "medium");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"high\" to adaptive thinking with effort \"high\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_high_effort()
    {
        await AssertAdaptive("high", "high");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"xhigh\" to adaptive thinking with effort \"max\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_xhigh_maps_to_max()
    {
        var (body, warnings) = await Reasoning("claude-sonnet-4-6", "xhigh");
        Assert.Equal("max", body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Equal("adaptive", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Contains(warnings, warning => warning.Message.Contains("mapped to effort \"max\""));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"minimal\" to adaptive thinking with effort \"low\" and emit compatibility warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Adaptive_minimal_maps_to_low()
    {
        var (body, warnings) = await Reasoning("claude-sonnet-4-6", "minimal");
        Assert.Equal("low", body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Contains(warnings, warning => warning.Type == "compatibility" && warning.Message.Contains("mapped to effort \"low\""));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"minimal\" to enabled thinking with ~2% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Budget_minimal_is_1280_on_sonnet_4_5()
    {
        var body = await ReasoningBody("claude-sonnet-4-5", "minimal");
        Assert.Equal(1280, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"low\" to enabled thinking with ~10% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Budget_low_is_6400_on_sonnet_4_5()
    {
        var body = await ReasoningBody("claude-sonnet-4-5", "low");
        Assert.Equal(6400, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"medium\" to enabled thinking with ~30% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Budget_medium_is_19200()
    {
        var body = await ReasoningBody("claude-sonnet-4-5", "medium");
        Assert.Equal(19200, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"high\" to enabled thinking with ~60% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Budget_high_is_38400()
    {
        var body = await ReasoningBody("claude-sonnet-4-5", "high");
        Assert.Equal(38400, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"xhigh\" to enabled thinking with ~90% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Budget_xhigh_is_57600()
    {
        var body = await ReasoningBody("claude-sonnet-4-5", "xhigh");
        Assert.Equal(57600, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should clamp budget to minimum 1024 tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Haiku_minimal_budget_is_clamped_to_1024()
    {
        var body = await ReasoningBody("claude-3-haiku-20240307", "minimal");
        Assert.Equal(1024, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should adjust max_tokens to include thinking budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Max_tokens_includes_the_thinking_budget()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-5");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Reasoning = "low";
            options.MaxOutputTokens = 10000;
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal(16400, body["max_tokens"]!.GetValue<int>());
        Assert.Equal(6400, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (providerOptions precedence)::should let anthropic.thinking take precedence over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_thinking_wins_over_top_level_reasoning()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Reasoning = "high";
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""thinking"":{""type"":""disabled""}}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        AnthropicParity.JsonEqual(body["thinking"], @"{""type"":""disabled""}");
        Assert.Null(body["output_config"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (providerOptions precedence)::should let anthropic.effort take precedence over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_effort_wins_over_top_level_reasoning()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Reasoning = "high";
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""effort"":""low""}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Null(body["thinking"]);
        Assert.Equal("low", body["output_config"]!["effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with json tool response (unsupported model)::should pass json schema response format as a tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Unsupported_json_schema_is_a_json_tool()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.JsonSchema = AnthropicParity.Element(@"{""type"":""object"",""properties"":{""name"":{""type"":""string""}},""required"":[""name""],""additionalProperties"":false,""$schema"":""http://json-schema.org/draft-07/schema#""}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        AnthropicParity.JsonEqual(body["tool_choice"], @"{""type"":""any"",""disable_parallel_tool_use"":true}");
        Assert.Equal("json", body["tools"]![0]!["name"]!.GetValue<string>());
        Assert.Equal("Respond with a JSON object.", body["tools"]![0]!["description"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with json tool response (unsupported model)::should return the json response", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_tool_input_is_returned_as_text()
    {
        var result = await JsonToolResult();
        var text = Assert.IsType<GeneratedText>(result.Content[0]);
        Assert.Equal(@"{""elements"":[{""location"":""San Francisco"",""temperature"":-5,""condition"":""snowy""},{""location"":""London"",""temperature"":0,""condition"":""snowy""},{""location"":""Paris"",""temperature"":23,""condition"":""cloudy""},{""location"":""Berlin"",""temperature"":-9,""condition"":""snowy""}]}", text.Text);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with json tool response (unsupported model)::should send stop finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_tool_stop_reason_is_stop()
    {
        var result = await JsonToolResult();
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("tool_use", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > json schema response format with output format (supported model)::should pass json schema response format as output_config.format", Coverage = UpstreamCoverage.Covered)]
    public async Task Supported_json_schema_uses_output_format()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-5");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.JsonSchema = AnthropicParity.Element(@"{""$schema"":""http://json-schema.org/draft-07/schema#"",""type"":""object"",""properties"":{""name"":{""type"":""string""}},""required"":[""name""],""additionalProperties"":false}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal("json_schema", body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Equal("object", body["output_config"]!["format"]!["schema"]!["type"]!.GetValue<string>());
        Assert.Null(body["tools"]);
        Assert.Equal(64000, body["max_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_content_is_extracted()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello, World!", text.Text);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Usage_keeps_uncached_input_as_the_total_when_cache_is_absent()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{""id"":""msg"",""content"":[{""type"":""text"",""text"":""""}],""stop_reason"":""end_turn"",""usage"":{""input_tokens"":20,""output_tokens"":5}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        Assert.Equal(20, result.Usage.InputTokens);
        Assert.Equal(5, result.Usage.OutputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(0, result.Usage.CacheWriteTokens);
        Assert.Equal(20, result.Usage.NoCacheInputTokens);
        Assert.Null(result.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_id_and_model_are_returned()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{""id"":""test-id"",""model"":""test-model"",""content"":[{""type"":""text"",""text"":""""}],""stop_reason"":""end_turn"",""usage"":{""input_tokens"":4,""output_tokens"":30}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        Assert.Equal("test-id", result.ResponseId);
        Assert.Equal("test-model", result.ResponseModelId);
        Assert.Contains("test-id", result.RawResponse);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should include stop_sequence in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Stop_sequence_is_provider_metadata()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{""id"":""msg"",""content"":[{""type"":""text"",""text"":""Hello, World!""}],""stop_reason"":""stop_sequence"",""stop_sequence"":""STOP"",""usage"":{""input_tokens"":4,""output_tokens"":30}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options => options.StopSequences = new[] { "STOP" }), CancellationToken.None);
        var metadata = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!;
        Assert.Equal("STOP", metadata["anthropic"]!["stopSequence"]!.GetValue<string>());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > refusal stop reason::should map a classifier refusal to content-filter and expose stop details", Coverage = UpstreamCoverage.Covered)]
    public async Task Refusal_maps_to_content_filter_and_stop_details()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-fable-5");
        handler.ResponseJson = @"{
            ""id"":""msg_01RefusalExampleAbcdefghijk"",""model"":""claude-fable-5"",""content"":[],
            ""stop_reason"":""refusal"",
            ""stop_details"":{""type"":""refusal"",""category"":""cyber"",""explanation"":""This request triggered restrictions on violative cyber content and was blocked under Anthropic's Usage Policy."",""recommended_model"":""claude-fable-5""},
            ""usage"":{""input_tokens"":18,""output_tokens"":5}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Equal("refusal", result.RawFinishReason);
        var details = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["anthropic"]!["stopDetails"]!;
        Assert.Equal("cyber", details["category"]!.GetValue<string>());
        Assert.Equal("claude-fable-5", details["recommendedModel"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > refusal stop reason::should map a refusal without stop details to content-filter and omit stop details", Coverage = UpstreamCoverage.Covered)]
    public async Task Refusal_without_details_omits_stop_details()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-fable-5");
        handler.ResponseJson = @"{""id"":""msg"",""content"":[],""stop_reason"":""refusal"",""usage"":{""input_tokens"":1,""output_tokens"":1}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        var metadata = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["anthropic"]!;
        Assert.Null(metadata["stopDetails"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > fallbacks::should pass fallbacks to the request body and add the beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Fallbacks_array_adds_the_june_beta()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-fable-5");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.MaxOutputTokens = 1024;
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""fallbacks"":[{""model"":""claude-opus-4-8"",""max_tokens"":8192,""thinking"":{""type"":""disabled""},""speed"":""fast""}]}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal("claude-opus-4-8", body["fallbacks"]![0]!["model"]!.GetValue<string>());
        Assert.Equal("server-side-fallback-2026-06-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > fallbacks::should pass fallbacks \"default\" through and add the 2026-07-01 beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_fallbacks_add_the_july_beta()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""fallbacks"":""default""}");
        }), CancellationToken.None);
        Assert.Equal("default", JsonNode.Parse(handler.Body!)!["fallbacks"]!.GetValue<string>());
        Assert.Equal("server-side-fallback-2026-07-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > fallbacks::should not add the beta header when fallbacks is an empty array", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_fallbacks_omit_the_beta()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""fallbacks"":[]}");
        }), CancellationToken.None);
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
        Assert.Null(JsonNode.Parse(handler.Body!)!["fallbacks"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > temperature and topP mutual exclusivity::should only send temperature when both temperature and topP are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Temperature_wins_over_top_p_on_claude()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Temperature = 0.7;
            options.TopP = 0.9;
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal(0.7, body["temperature"]!.GetValue<double>());
        Assert.Null(body["top_p"]);
        Assert.Contains(result.Warnings, warning => warning.Message == "topP is not supported when temperature is set. topP is ignored.");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > temperature and topP mutual exclusivity::should send temperature when only temperature is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Temperature_alone_is_sent()
    {
        var body = await SamplingBody("claude-3-haiku-20240307", 0.7, null);
        Assert.Equal(0.7, body["temperature"]!.GetValue<double>());
        Assert.Null(body["top_p"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > temperature and topP mutual exclusivity::should send topP when only topP is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Top_p_alone_is_sent()
    {
        var body = await SamplingBody("claude-3-haiku-20240307", null, 0.9);
        Assert.Null(body["temperature"]);
        Assert.Equal(0.9, body["top_p"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > temperature and topP mutual exclusivity::should not send temperature or topP when neither is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Sampling_parameters_are_omitted_when_unset()
    {
        var body = await SamplingBody("claude-3-haiku-20240307", null, null);
        Assert.Null(body["temperature"]);
        Assert.Null(body["top_p"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > temperature and topP mutual exclusivity::should send both temperature and topP for non-Anthropic models", Coverage = UpstreamCoverage.Covered)]
    public async Task Non_claude_models_send_temperature_and_top_p()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("MiniMax-M2.7");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Temperature = 0.7;
            options.TopP = 0.9;
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal(0.7, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.9, body["top_p"]!.GetValue<double>());
        Assert.DoesNotContain(result.Warnings, warning => warning.Message.Contains("topP"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should limit max output tokens to the model max and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Max_output_tokens_are_clamped_to_the_model_ceiling()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-haiku-4-5");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options => options.MaxOutputTokens = 999999), CancellationToken.None);
        Assert.Equal(64000, JsonNode.Parse(handler.Body!)!["max_tokens"]!.GetValue<int>());
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("limited to 64000"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should not limit max output tokens for unknown models", Coverage = UpstreamCoverage.Covered)]
    public async Task Unknown_models_keep_the_requested_max_tokens()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("future-model");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options => options.MaxOutputTokens = 123456), CancellationToken.None);
        Assert.Equal(123456, JsonNode.Parse(handler.Body!)!["max_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should use default thinking budget when it is not set", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_thinking_budget_is_1024_and_max_tokens_stay_at_the_ceiling()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-haiku-4-5");
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""thinking"":{""type"":""enabled""}}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal(64000, body["max_tokens"]!.GetValue<int>());
        Assert.Equal(1024, body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("default budget of 1024"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Named_tool_choice_is_sent()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Tools = new[]
            {
                new LanguageModelTool("test-tool", null, AnthropicParity.Element(@"{""type"":""object"",""properties"":{""value"":{""type"":""string""}},""required"":[""value""],""additionalProperties"":false,""$schema"":""http://json-schema.org/draft-07/schema#""}")),
            };
            options.ToolChoice = ToolChoice.Tool("test-tool");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal("claude-3-haiku-20240307", body["model"]!.GetValue<string>());
        Assert.Equal(4096, body["max_tokens"]!.GetValue<int>());
        Assert.Equal("test-tool", body["tools"]![0]!["name"]!.GetValue<string>());
        AnthropicParity.JsonEqual(body["tool_choice"], @"{""type"":""tool"",""name"":""test-tool""}");
        Assert.Equal("Hello", body["messages"]![0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass disableParallelToolUse", Coverage = UpstreamCoverage.Covered)]
    public async Task Disable_parallel_tool_use_sets_auto_tool_choice()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Tools = new[] { new LanguageModelTool("test-tool", null, AnthropicParity.Element(@"{""type"":""object""}")) };
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""disableParallelToolUse"":true}");
        }), CancellationToken.None);
        AnthropicParity.JsonEqual(JsonNode.Parse(handler.Body!)!["tool_choice"], @"{""type"":""auto"",""disable_parallel_tool_use"":true}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_and_request_headers_are_merged()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307", new AnthropicOptions
        {
            ApiKey = "test-api-key",
            Headers = new Dictionary<string, string?> { ["Custom-Provider-Header"] = "provider-header-value" },
        });
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        }), CancellationToken.None);
        Assert.Equal("https://api.anthropic.com/v1/messages", handler.RequestUri!.ToString());
        Assert.Equal("test-api-key", handler.Headers["x-api-key"]);
        Assert.Equal("2023-06-01", handler.Headers["anthropic-version"]);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
        Assert.StartsWith("application/json", handler.Headers["Content-Type"]);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > function tool::should extract tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_calls_are_parsed_with_a_tool_calls_finish()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{
            ""id"":""msg"",""content"":[
                {""type"":""text"",""text"":""Some text\n\n""},
                {""type"":""tool_use"",""id"":""toolu_1"",""name"":""test-tool"",""input"":{""value"":""example value""}}],
            ""stop_reason"":""tool_use"",""usage"":{""input_tokens"":4,""output_tokens"":30}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        Assert.Equal("Some text\n\n", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.Equal("toolu_1", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal(@"{""value"":""example value""}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("tool_use", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > function tool::should support tools with empty parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_tool_arguments_stay_an_object()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-opus-20240229");
        handler.ResponseJson = @"{
            ""id"":""msg_01GCBaV8gyWAYgMVggRqZbuQ"",
            ""content"":[
                {""type"":""text"",""text"":""Okay, I will update the current issue list:""},
                {""type"":""tool_use"",""id"":""toolu_01LRmxn9vGM1d2DZSDBowdZ1"",""name"":""updateIssueList"",""input"":{}}],
            ""stop_reason"":""tool_use"",""usage"":{""input_tokens"":602,""output_tokens"":93}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.Equal("{}", call.ArgumentsJson);
        Assert.Equal("updateIssueList", call.ToolName);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > programmatic tool calling::should include caller info when tool_use has caller field from code_execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_call_caller_is_provider_metadata()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-sonnet-4-6");
        handler.ResponseJson = @"{""id"":""msg"",""content"":[{""type"":""tool_use"",""id"":""toolu_1"",""name"":""roll"",""input"":{""sides"":6},""caller"":{""type"":""code_execution_20260120"",""tool_id"":""srvtoolu_1""}}],""stop_reason"":""tool_use"",""usage"":{""input_tokens"":1,""output_tokens"":1}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        var metadata = JsonNode.Parse(call.ProviderMetadata!.Value.GetRawText())!;
        Assert.Equal("code_execution_20260120", metadata["anthropic"]!["caller"]!["type"]!.GetValue<string>());
        Assert.Equal("srvtoolu_1", metadata["anthropic"]!["caller"]!["toolId"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > programmatic tool calling::should not include caller info when tool_use has no caller field", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_call_without_caller_has_no_metadata()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{""id"":""msg"",""content"":[{""type"":""tool_use"",""id"":""toolu_1"",""name"":""roll"",""input"":{}}],""stop_reason"":""tool_use"",""usage"":{""input_tokens"":1,""output_tokens"":1}}";
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Null(call.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should throw an api error when the server is returning a 529 overloaded error", Coverage = UpstreamCoverage.Covered)]
    public async Task Overloaded_529_is_retryable_and_preserves_the_body()
    {
        const string Body = @"{""type"":""error"",""error"":{""details"":null,""type"":""overloaded_error"",""message"":""Overloaded""}}";
        var error = await Status((HttpStatusCode)529, Body);
        var overloaded = Assert.IsType<InternalServerException>(error);
        Assert.Equal("Overloaded", overloaded.Message);
        Assert.Equal(529, overloaded.StatusCode);
        Assert.Equal(Body, overloaded.ResponseBody);
        Assert.True(overloaded.IsRetryable);
        Assert.True(ProviderHttp.IsRetryable(529));
    }

    [Fact]
    public async Task Status_400_is_a_bad_request()
    {
        var error = await Status(HttpStatusCode.BadRequest, ErrorBody("bad"));
        Assert.IsType<BadRequestException>(error);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    public async Task Status_401_is_authentication()
    {
        Assert.IsType<AuthenticationException>(await Status(HttpStatusCode.Unauthorized, ErrorBody("unauthorized")));
    }

    [Fact]
    public async Task Status_403_is_permission_denied()
    {
        Assert.IsType<PermissionDeniedException>(await Status(HttpStatusCode.Forbidden, ErrorBody("forbidden")));
    }

    [Fact]
    public async Task Status_404_is_not_found()
    {
        Assert.IsType<NotFoundException>(await Status(HttpStatusCode.NotFound, ErrorBody("missing")));
    }

    [Fact]
    public async Task Status_422_is_unprocessable()
    {
        Assert.IsType<UnprocessableEntityException>(await Status((HttpStatusCode)422, ErrorBody("invalid")));
    }

    [Fact]
    public async Task Status_429_is_a_retryable_rate_limit()
    {
        var error = Assert.IsType<RateLimitException>(await Status((HttpStatusCode)429, ErrorBody("slow down")));
        Assert.True(error.IsRetryable);
    }

    [Fact]
    public async Task Status_500_is_a_retryable_internal_error()
    {
        var error = Assert.IsType<InternalServerException>(await Status(HttpStatusCode.InternalServerError, ErrorBody("down")));
        Assert.Equal(500, error.StatusCode);
        Assert.True(error.IsRetryable);
    }

    [Fact]
    public async Task Status_413_is_a_non_retryable_api_error()
    {
        var error = await Status((HttpStatusCode)413, ErrorBody("too large"));
        Assert.IsType<ApiException>(error);
        Assert.Equal(413, error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should set effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Effort_is_output_config()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.ProviderOptions = AnthropicParity.AnthropicOptions(@"{""effort"":""medium""}");
        }), CancellationToken.None);
        var body = JsonNode.Parse(handler.Body!)!;
        Assert.Equal("medium", body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Equal(4096, body["max_tokens"]!.GetValue<int>());
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
    }

    private static async Task AssertAdaptive(string reasoning, string effort)
    {
        var (body, warnings) = await Reasoning("claude-sonnet-4-6", reasoning);
        Assert.Equal("adaptive", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("summarized", body["thinking"]!["display"]!.GetValue<string>());
        Assert.Equal(effort, body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    private static async Task<JsonNode> ReasoningBody(string modelId, string reasoning)
    {
        var (body, _) = await Reasoning(modelId, reasoning);
        return body;
    }

    private static async Task<(JsonNode Body, IReadOnlyList<CallWarning> Warnings)> Reasoning(string modelId, string reasoning)
    {
        var (model, handler) = AnthropicGenerateHarness.Create(modelId);
        handler.ResponseJson = TextResponse;
        var result = await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options => options.Reasoning = reasoning), CancellationToken.None);
        return (JsonNode.Parse(handler.Body!)!, result.Warnings);
    }

    private static async Task<JsonNode> SamplingBody(string modelId, double? temperature, double? topP)
    {
        var (model, handler) = AnthropicGenerateHarness.Create(modelId);
        handler.ResponseJson = TextResponse;
        await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.Temperature = temperature;
            options.TopP = topP;
        }), CancellationToken.None);
        return JsonNode.Parse(handler.Body!)!;
    }

    private static async Task<LanguageModelGenerateResult> JsonToolResult()
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.ResponseJson = @"{
            ""id"":""msg_0191iYfpERYfS27xLsdW2nbb"",""stop_reason"":""tool_use"",
            ""content"":[{""type"":""tool_use"",""id"":""toolu_01Q9ExVZnzZj7E2QQYHYtNUa"",""name"":""json"",""input"":{""elements"":[
                {""location"":""San Francisco"",""temperature"":-5,""condition"":""snowy""},
                {""location"":""London"",""temperature"":0,""condition"":""snowy""},
                {""location"":""Paris"",""temperature"":23,""condition"":""cloudy""},
                {""location"":""Berlin"",""temperature"":-9,""condition"":""snowy""}]}}],
            ""usage"":{""input_tokens"":1151,""output_tokens"":87}}";
        return await model.DoGenerateAsync(AnthropicGenerateHarness.Hello(options =>
        {
            options.JsonSchema = AnthropicParity.Element(@"{""type"":""object""}");
        }), CancellationToken.None);
    }

    private static async Task<ApiException> Status(HttpStatusCode status, string body)
    {
        var (model, handler) = AnthropicGenerateHarness.Create("claude-3-haiku-20240307");
        handler.StatusCode = status;
        handler.ResponseJson = body;
        return await Assert.ThrowsAnyAsync<ApiException>(() => model.DoGenerateAsync(AnthropicGenerateHarness.Hello(), CancellationToken.None));
    }

    private static string ErrorBody(string message)
    {
        return @"{""type"":""error"",""error"":{""type"":""invalid_request_error"",""message"":""" + message + @"""}}";
    }
}
