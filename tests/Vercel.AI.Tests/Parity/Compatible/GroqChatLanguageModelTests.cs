// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GroqChatLanguageModelTests
{
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > text::should extract text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"id\":\"chatcmpl-09d64d2a-ed1c-4473-829f-78db43f45d13\",\"created\":1770770798,\"model\":\"llama-3.3-70b-versatile\",\"choices\":[{\"message\":{\"content\":\"I'd like to introduce \\\"Luminaria\\\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":45,\"completion_tokens\":607,\"total_tokens\":652}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal("I'd like to introduce \"Luminaria\"", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > text::should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_model_and_messages_only()
    {
        var (model, handler) = Create();
        await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal("{\"model\":\"gemma2-9b-it\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}", handler.Body);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should reject a response without choices", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_response_without_choices()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"id\":\"chatcmpl-empty\",\"choices\":[],\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":0,\"total_tokens\":4}}";
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoGenerateAsync(Hello(), CancellationToken.None));
        Assert.Equal("Response did not contain any choices.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > tool call::should extract tool call content", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_tool_calls()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"tool_calls\":[{\"id\":\"ax9fskhev\",\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("ax9fskhev", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > reasoning::should extract reasoning content", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_reasoning()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"Answer: 3.\",\"reasoning\":\"Okay, so the user is asking\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":17,\"completion_tokens\":649,\"completion_tokens_details\":{\"reasoning_tokens\":570}}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal("Answer: 3.", result.Text);
        var reasoning = Assert.IsType<GeneratedReasoning>(result.Content[1]);
        Assert.Equal("Okay, so the user is asking", reasoning.Text);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should map top-level reasoning to reasoning_effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_high_reasoning_to_effort()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.Reasoning = "high";
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("high", JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should coerce top-level reasoning minimal to low", Coverage = UpstreamCoverage.Covered)]
    public async Task Coerces_minimal_reasoning_to_low()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.Reasoning = "minimal";
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("low", JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should coerce top-level reasoning xhigh to high", Coverage = UpstreamCoverage.Covered)]
    public async Task Coerces_xhigh_reasoning_to_high()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.Reasoning = "xhigh";
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("high", JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should map top-level reasoning none to reasoning_effort for Qwen 3.6", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_none_for_qwen()
    {
        var (model, handler) = Create("qwen/qwen3.6-27b");
        var options = Hello();
        options.Reasoning = "none";
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("none", JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should omit unsupported top-level reasoning none and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_unsupported_none()
    {
        var (model, handler) = Create("openai/gpt-oss-120b");
        var options = Hello();
        options.Reasoning = "none";
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.False(JsonDocument.Parse(handler.Body).RootElement.TryGetProperty("reasoning_effort", out _));
        Assert.Contains(result.Warnings, warning => GroqWarnings.Matches(warning, "unsupported", "reasoning", "reasoning \"none\" is not supported by this model."));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should prefer providerOptions reasoningEffort over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_reasoning_effort_wins()
    {
        var (model, handler) = Create("openai/gpt-oss-120b");
        var options = Hello();
        options.Reasoning = "medium";
        options.ProviderOptions = Groq("{\"reasoningEffort\":\"high\"}");
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("high", JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_usage()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":45,\"completion_tokens\":607,\"total_tokens\":652}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        var usage = GroqUsage.Convert(result.Usage.Raw);
        Assert.Equal(45, usage.InputTotal);
        Assert.Equal(45, usage.NoCache);
        Assert.Null(usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Equal(607, usage.OutputTotal);
        Assert.Equal(607, usage.Text);
        Assert.Null(usage.Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_response_metadata()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"id\":\"chatcmpl-09d64d2a-ed1c-4473-829f-78db43f45d13\",\"created\":1770770798,\"model\":\"llama-3.3-70b-versatile\",\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal("chatcmpl-09d64d2a-ed1c-4473-829f-78db43f45d13", result.ResponseId);
        Assert.Equal("llama-3.3-70b-versatile", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1770770798), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should support partial usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_partial_usage()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":20,\"total_tokens\":20}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        var usage = GroqUsage.Convert(result.Usage.Raw);
        Assert.Equal(20, usage.InputTotal);
        Assert.Equal(20, usage.NoCache);
        Assert.Equal(0, usage.OutputTotal);
        Assert.Equal(0, usage.Text);
        Assert.Null(usage.Reasoning);
        Assert.False(usage.Raw!.Value.TryGetProperty("completion_tokens", out _));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should extract cached input tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_cached_input_tokens()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":20,\"completion_tokens\":5,\"total_tokens\":25,\"prompt_tokens_details\":{\"cached_tokens\":15}}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        var usage = GroqUsage.Convert(result.Usage.Raw);
        Assert.Equal(15, usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Equal(5, usage.NoCache);
        Assert.Equal(5, usage.Text);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should extract reasoning tokens from completion_tokens_details", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_reasoning_tokens()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":17,\"completion_tokens\":649,\"total_tokens\":666,\"completion_tokens_details\":{\"reasoning_tokens\":570}}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        var usage = GroqUsage.Convert(result.Usage.Raw);
        Assert.Equal(17, usage.NoCache);
        Assert.Equal(570, usage.Reasoning);
        Assert.Equal(79, usage.Text);
        Assert.Equal(649, usage.OutputTotal);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_unknown_finish_reason()
    {
        var (model, handler) = Create();
        handler.ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"\"},\"finish_reason\":\"eos\"}],\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":30,\"total_tokens\":34}}";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_generate_response_headers()
    {
        var (model, handler) = Create();
        handler.ResponseHeaders["test-header"] = "test-value";
        var result = await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_options()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.ProviderOptions = Groq("{\"reasoningFormat\":\"hidden\",\"user\":\"test-user-id\",\"parallelToolCalls\":false}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        var root = JsonDocument.Parse(handler.Body).RootElement;
        Assert.Equal("hidden", root.GetProperty("reasoning_format").GetString());
        Assert.Equal("test-user-id", root.GetProperty("user").GetString());
        Assert.False(root.GetProperty("parallel_tool_calls").GetBoolean());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass serviceTier provider option", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_flex_service_tier()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.ProviderOptions = Groq("{\"serviceTier\":\"flex\"}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("flex", JsonDocument.Parse(handler.Body).RootElement.GetProperty("service_tier").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass performance serviceTier provider option", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_performance_service_tier()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.ProviderOptions = Groq("{\"serviceTier\":\"performance\"}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("performance", JsonDocument.Parse(handler.Body).RootElement.GetProperty("service_tier").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_tools_and_tool_choice()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.Tools = new[] { new LanguageModelTool("test-tool", null, Parse(Schema)) };
        options.ToolChoice = ToolChoice.Tool("test-tool");
        await model.DoGenerateAsync(options, CancellationToken.None);
        var root = JsonDocument.Parse(handler.Body).RootElement;
        Assert.Equal("test-tool", root.GetProperty("tool_choice").GetProperty("function").GetProperty("name").GetString());
        Assert.Equal("function", root.GetProperty("tools")[0].GetProperty("type").GetString());
        Assert.Equal("test-tool", root.GetProperty("tools")[0].GetProperty("function").GetProperty("name").GetString());
        Assert.False(root.GetProperty("tools")[0].GetProperty("function").TryGetProperty("description", out _));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_generate_headers()
    {
        var handler = new CaptureHandler();
        var provider = GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = provider.LanguageModel("gemma2-9b-it");
        var options = Hello();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/groq/0.0.0-test", handler.RequestHeaders["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass response format information as json_schema when structuredOutputs enabled by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_schema_by_default()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.JsonSchema = Parse(Schema);
        options.JsonSchemaName = "test-name";
        options.ProviderOptions = Groq("{\"responseFormatDescription\":\"test description\"}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        var format = JsonDocument.Parse(handler.Body).RootElement.GetProperty("response_format");
        Assert.Equal("json_schema", format.GetProperty("type").GetString());
        var schema = format.GetProperty("json_schema");
        Assert.Equal("test-name", schema.GetProperty("name").GetString());
        Assert.Equal("test description", schema.GetProperty("description").GetString());
        Assert.True(schema.GetProperty("strict").GetBoolean());
        Assert.Equal("object", schema.GetProperty("schema").GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass response format information as json_object when structuredOutputs explicitly disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_object_when_structured_outputs_are_disabled()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.JsonSchema = Parse(Schema);
        options.ProviderOptions = Groq("{\"structuredOutputs\":false}");
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_object", JsonDocument.Parse(handler.Body).RootElement.GetProperty("response_format").GetProperty("type").GetString());
        Assert.Contains(result.Warnings, warning => GroqWarnings.Matches(warning, "unsupported", "responseFormat", "JSON response format schema is only supported with structuredOutputs"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should use json_schema format when structuredOutputs explicitly enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_schema_when_explicitly_enabled()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.JsonSchema = Parse(Schema);
        options.JsonSchemaName = "test-name";
        options.ProviderOptions = Groq("{\"structuredOutputs\":true,\"responseFormatDescription\":\"test description\"}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        var schema = JsonDocument.Parse(handler.Body).RootElement.GetProperty("response_format").GetProperty("json_schema");
        Assert.True(schema.GetProperty("strict").GetBoolean());
        Assert.Equal("test-name", schema.GetProperty("name").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should allow explicit structuredOutputs override", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_structured_outputs_override()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.JsonSchema = Parse(Schema);
        options.ProviderOptions = Groq("{\"structuredOutputs\":false}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_object", JsonDocument.Parse(handler.Body).RootElement.GetProperty("response_format").GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should send strict: false when strictJsonSchema is explicitly disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_strict_false()
    {
        var (model, handler) = Create();
        var options = Hello();
        options.JsonSchema = Parse(Schema);
        options.JsonSchemaName = "test-name";
        options.ProviderOptions = Groq("{\"strictJsonSchema\":false,\"responseFormatDescription\":\"test description\"}");
        await model.DoGenerateAsync(options, CancellationToken.None);
        var schema = JsonDocument.Parse(handler.Body).RootElement.GetProperty("response_format").GetProperty("json_schema");
        Assert.False(schema.GetProperty("strict").GetBoolean());
        Assert.Equal("test description", schema.GetProperty("description").GetString());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should handle structured outputs with Kimi K2 model", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_kimi_structured_outputs()
    {
        var (model, handler) = Create("moonshotai/kimi-k2-instruct-0905");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a simple pasta recipe") },
            JsonSchemaName = "recipe_response",
            JsonSchema = Parse("{\"type\":\"object\",\"properties\":{\"recipe\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}},\"required\":[\"recipe\"]}"),
            ProviderOptions = Groq("{\"structuredOutputs\":true,\"responseFormatDescription\":\"A recipe with ingredients and instructions\"}"),
        };
        await model.DoGenerateAsync(options, CancellationToken.None);
        var root = JsonDocument.Parse(handler.Body).RootElement;
        Assert.Equal("moonshotai/kimi-k2-instruct-0905", root.GetProperty("model").GetString());
        Assert.Equal("recipe_response", root.GetProperty("response_format").GetProperty("json_schema").GetProperty("name").GetString());
        Assert.True(root.GetProperty("response_format").GetProperty("json_schema").GetProperty("strict").GetBoolean());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should include warnings when structured outputs explicitly disabled but schema provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_schema_is_ignored()
    {
        var (model, _) = Create();
        var options = Hello();
        options.JsonSchema = Parse(Schema);
        options.ProviderOptions = Groq("{\"structuredOutputs\":false}");
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Contains(result.Warnings, warning => GroqWarnings.Matches(warning, "unsupported", "responseFormat", "JSON response format schema is only supported with structuredOutputs"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_request_body_omits_stream()
    {
        var (model, handler) = Create();
        await model.DoGenerateAsync(Hello(), CancellationToken.None);
        Assert.Equal("{\"model\":\"gemma2-9b-it\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}", handler.Body);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream > text::should stream text", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"id1\",\"created\":1,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"He\"},\"finish_reason\":null}]}",
            "{\"id\":\"id1\",\"created\":1,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"llo\"},\"finish_reason\":\"stop\"}],\"x_groq\":{\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":2}}}");
        var parts = await Read(model, Hello());
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Equal("He", Assert.IsType<TextDeltaStreamPart>(parts.Single(part => part is TextDeltaStreamPart delta && delta.Delta == "He")).Delta);
        Assert.Contains(parts, part => part is TextStartStreamPart start && start.Id == "txt-0");
        Assert.Contains(parts, part => part is TextEndStreamPart end && end.Id == "txt-0");
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(1, GroqUsage.Convert(finish.Usage.Raw).NoCache);
        Assert.Equal(2, GroqUsage.Convert(finish.Usage.Raw).Text);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream > tool call::should stream tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_a_tool_call()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"chatcmpl-b610d559-f156-4aca-8827-24b4fe6af54f\",\"created\":1770770843,\"model\":\"llama-3.3-70b-versatile\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":null},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-b610d559-f156-4aca-8827-24b4fe6af54f\",\"created\":1770770843,\"model\":\"llama-3.3-70b-versatile\",\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"id\":\"tk85n1k4m\",\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"},\"index\":0}]},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-b610d559-f156-4aca-8827-24b4fe6af54f\",\"created\":1770770843,\"model\":\"llama-3.3-70b-versatile\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"tool_calls\"}],\"x_groq\":{\"usage\":{\"prompt_tokens\":210,\"completion_tokens\":15}}}");
        var parts = await Read(model, Hello());
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("tk85n1k4m", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream > reasoning::should keep reasoning active when deltas include empty tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_tool_calls_do_not_end_reasoning()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"chatcmpl-test\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"reasoning\":\"Think \",\"tool_calls\":[]},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-test\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"reasoning\":\"more...\",\"tool_calls\":[]},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-test\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\",\"reasoning\":\"\",\"tool_calls\":[]},\"finish_reason\":\"stop\"}]}");
        var parts = await Read(model, Hello());
        var reasoning = parts.Where(part => part.Type.StartsWith("reasoning-", StringComparison.Ordinal)).ToList();
        Assert.Equal("reasoning-start", reasoning[0].Type);
        Assert.Equal("Think ", Assert.IsType<ReasoningDeltaStreamPart>(reasoning[1]).Delta);
        Assert.Equal("more...", Assert.IsType<ReasoningDeltaStreamPart>(reasoning[2]).Delta);
        Assert.Equal("reasoning-end", reasoning[3].Type);
        Assert.Equal(4, reasoning.Count);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream > reasoning::should stream reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_reasoning()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"id\",\"created\":1,\"model\":\"qwen\",\"choices\":[{\"index\":0,\"delta\":{\"reasoning\":\"Think\"},\"finish_reason\":null}]}",
            "{\"id\":\"id\",\"created\":1,\"model\":\"qwen\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Answer\"},\"finish_reason\":\"stop\"}]}");
        var parts = await Read(model, Hello());
        Assert.Contains(parts, part => part is ReasoningStartStreamPart start && start.Id == "reasoning-0");
        Assert.Contains(parts, part => part is ReasoningDeltaStreamPart delta && delta.Delta == "Think");
        var textIndex = parts.FindIndex(part => part is TextDeltaStreamPart);
        var endIndex = parts.FindIndex(part => part is ReasoningEndStreamPart);
        Assert.True(endIndex >= 0 && endIndex < textIndex);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should stream tool call deltas when tool call arguments are passed in the first chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_split_tool_arguments()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798\",\"created\":1711357598,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"id\":\"call_O17Uplv4lJvD6DVdIvFFeRMw\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"{\\\"\"}}]},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798\",\"created\":1711357598,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"value\\\":\\\"Sparkle Day\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}],\"x_groq\":{\"usage\":{\"prompt_tokens\":18,\"completion_tokens\":439,\"total_tokens\":457}}}");
        var parts = await Read(model, Hello());
        var metadata = Assert.Single(parts.OfType<ResponseMetadataStreamPart>());
        Assert.Equal("chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798", metadata.Id);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1711357598), metadata.Timestamp);
        var deltas = parts.OfType<GroqToolInputDeltaStreamPart>().Select(part => part.Delta).ToArray();
        Assert.Equal(new[] { "{\"", "value\":\"Sparkle Day\"}" }, deltas);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("{\"value\":\"Sparkle Day\"}", call.ArgumentsJson);
        var usage = GroqUsage.Convert(Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).Usage.Raw);
        Assert.Equal(18, usage.NoCache);
        Assert.Equal(439, usage.Text);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should not duplicate tool calls when there is an additional empty chunk after the tool call has been completed", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_duplicate_finished_tool_calls()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"chat-2267f7e2910a4254bac0650ba74cfc1c\",\"created\":1733162241,\"model\":\"meta/llama-3.1-8b-instruct:fp8\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\"},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"completion_tokens\":0}}",
            "{\"id\":\"chat-2267f7e2910a4254bac0650ba74cfc1c\",\"created\":1733162241,\"model\":\"meta/llama-3.1-8b-instruct:fp8\",\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"id\":\"chatcmpl-tool-b3b307239370432d9910d4b79b4dbbaa\",\"index\":0,\"function\":{\"name\":\"searchGoogle\"}}]},\"finish_reason\":null}]}",
            "{\"id\":\"chat-2267f7e2910a4254bac0650ba74cfc1c\",\"created\":1733162241,\"model\":\"meta/llama-3.1-8b-instruct:fp8\",\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"{\\\"query\\\": \\\"latest news on ai\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}]}",
            "{\"id\":\"chat-2267f7e2910a4254bac0650ba74cfc1c\",\"created\":1733162241,\"model\":\"meta/llama-3.1-8b-instruct:fp8\",\"choices\":[],\"usage\":{\"prompt_tokens\":226,\"completion_tokens\":20}}");
        var parts = await Read(model, Hello());
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("searchGoogle", call.ToolName);
        Assert.Equal("{\"query\": \"latest news on ai\"}", call.ArgumentsJson);
        Assert.Null(Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).Usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should stream tool call that is sent in one chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_a_single_chunk_tool_call()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse(
            "{\"id\":\"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798\",\"created\":1711357598,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"id\":\"call_O17Uplv4lJvD6DVdIvFFeRMw\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"{\\\"value\\\":\\\"Sparkle Day\\\"}\"}}]},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-e7f8e220-656c-4455-a132-dacfc1370798\",\"created\":1729171479,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"tool_calls\"}],\"x_groq\":{\"usage\":{\"prompt_tokens\":18,\"completion_tokens\":439}}}");
        var parts = await Read(model, Hello());
        Assert.Contains(parts, part => part is GroqToolInputStartStreamPart start && start.ToolName == "test-tool");
        Assert.Equal("{\"value\":\"Sparkle Day\"}", Assert.Single(parts.OfType<GroqToolInputDeltaStreamPart>()).Delta);
        Assert.Contains(parts, part => part is GroqToolInputEndStreamPart);
        Assert.Equal("{\"value\":\"Sparkle Day\"}", Assert.Single(parts.OfType<ToolCallStreamPart>()).ArgumentsJson);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should handle error stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_error_chunks()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse("{\"error\":{\"message\":\"Rate limit reached\",\"type\":\"rate_limit_error\"}}");
        var parts = await Read(model, Hello());
        Assert.Equal("Rate limit reached", Assert.Single(parts.OfType<ErrorStreamPart>()).Message);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        var usage = GroqUsage.Convert(finish.Usage.Raw);
        Assert.Null(usage.InputTotal);
        Assert.Null(usage.OutputTotal);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should handle unparsable stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_unparsable_chunks()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = "data: {unparsable}\n\n";
        var parts = await Read(model, Hello());
        var error = Assert.Single(parts.OfType<ErrorStreamPart>());
        Assert.Contains("JSON parsing failed", error.Message, StringComparison.Ordinal);
        Assert.Contains("{unparsable}", error.Message, StringComparison.Ordinal);
        Assert.Equal(FinishReason.Error, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_stream_response_headers()
    {
        var (model, handler) = Create();
        handler.ResponseHeaders["test-header"] = "test-value";
        handler.ServerSentEvents = Sse("{\"id\":\"id\",\"created\":1,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}");
        await Read(model, Hello());
        Assert.Equal("test-value", model.LastResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should send correct streaming request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_with_stream_true_only()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse("{\"id\":\"id\",\"created\":1,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}");
        await Read(model, Hello());
        Assert.Equal("{\"model\":\"gemma2-9b-it\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"stream\":true}", handler.Body);
        Assert.DoesNotContain("stream_options", handler.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_stream_headers()
    {
        var handler = new CaptureHandler();
        var provider = GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        handler.ServerSentEvents = Sse("{\"id\":\"id\",\"created\":1,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}");
        var options = Hello();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await Read((GroqChatLanguageModel)provider.LanguageModel("gemma2-9b-it"), options);
        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_request_body_matches_the_wire_string()
    {
        var (model, handler) = Create();
        handler.ServerSentEvents = Sse("{\"id\":\"id\",\"created\":1,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}");
        await Read(model, Hello());
        Assert.Equal("{\"model\":\"gemma2-9b-it\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"stream\":true}", handler.Body);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream with raw chunks::should stream raw chunks when includeRawChunks is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_raw_chunks()
    {
        var (model, handler) = Create();
        var first = "{\"id\":\"chatcmpl-123\",\"created\":1234567890,\"model\":\"gemma2-9b-it\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\"},\"finish_reason\":null}]}";
        handler.ServerSentEvents = Sse(first);
        var options = Hello();
        options.IncludeRawChunks = true;
        var parts = await Read(model, options);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Equal(first, Assert.IsType<RawStreamPart>(parts[1]).RawJson);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[2]);
        Assert.Equal("chatcmpl-123", metadata.Id);
        Assert.Equal("gemma2-9b-it", metadata.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1234567890), metadata.Timestamp);
    }

    private static (GroqChatLanguageModel Model, CaptureHandler Handler) Create(string modelId = "gemma2-9b-it")
    {
        var handler = new CaptureHandler();
        var provider = GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        return ((GroqChatLanguageModel)provider.LanguageModel(modelId), handler);
    }

    private static LanguageModelCallOptions Hello()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };
    }

    private static Dictionary<string, JsonElement> Groq(string json)
    {
        return new Dictionary<string, JsonElement> { ["groq"] = Parse(json) };
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string Sse(params string[] events)
    {
        return string.Join(string.Empty, events.Select(item => "data: " + item + "\n\n")) + "data: [DONE]\n\n";
    }

    private static async Task<List<LanguageModelStreamPart>> Read(GroqChatLanguageModel model, LanguageModelCallOptions options)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in model.DoStreamAsync(options, CancellationToken.None))
        {
            parts.Add(part);
        }

        return parts;
    }
}
