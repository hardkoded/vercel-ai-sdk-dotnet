// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAIChatLanguageModelGenerateTests
{
    private const string File = "packages/openai/src/chat/openai-chat-language-model.test.ts";

    private const string PriorityWarning = "priority processing is only available for supported models (gpt-4, gpt-5, gpt-5-mini, o3, o4-mini) and requires Enterprise access. gpt-5-nano is not supported";

    private const string SearchWarning = "temperature is not supported for the search preview models and has been removed.";

    [Fact]
    [UpstreamTest(File + "::doGenerate::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text()
    {
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":"Hello, World!"}""", "stop"));
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello, World!", text.Text);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should extract an audio transcript alongside tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_an_audio_transcript_alongside_tool_calls()
    {
        var message = """
            {"role":"assistant","content":null,"audio":{"id":"audio-1","data":"base64-audio","expires_at":1711118637,"transcript":"Fix the login bug"},"tool_calls":[{"id":"call-1","type":"function","function":{"name":"test-tool","arguments":"{\"value\":\"Spark\"}"}}]}
            """;
        var result = await GenerateAsync(Choice(message, "tool_calls"));
        Assert.Equal(2, result.Content.Count);
        Assert.Equal("Fix the login bug", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should reject a response without choices", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_response_without_choices()
    {
        var handler = new RecordingHandler
        {
            ResponseBody = """{"id":"chatcmpl-empty","choices":[],"usage":{"prompt_tokens":4,"total_tokens":4,"completion_tokens":0}}""",
        };
        var model = OpenAIParity.Model(handler);
        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(OpenAIParity.Hello(), CancellationToken.None));
        Assert.Equal("Response did not contain any choices.", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_usage()
    {
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "stop", """{"prompt_tokens":20,"total_tokens":25,"completion_tokens":5}"""));
        OpenAIParity.AssertUsage(result.Usage, 20, 5, 0, 0, null, 25);
        OpenAIParity.AssertJson(result.Usage.Raw!.Value.GetRawText(), """{"prompt_tokens":20,"total_tokens":25,"completion_tokens":5}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_request_body()
    {
        var handler = await SendAsync(OpenAIParity.Hello());
        OpenAIParity.AssertBody(handler, OpenAIParity.HelloBody);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_additional_response_information()
    {
        var body = """
            {"id":"test-id","object":"chat.completion","created":123,"model":"test-model","choices":[{"index":0,"message":{"role":"assistant","content":""},"finish_reason":"stop"}],"usage":{"prompt_tokens":4,"total_tokens":34,"completion_tokens":30},"system_fingerprint":"fp_3bc1b5746c"}
            """;
        var (result, handler) = await CallAsync(body, OpenAIParity.Hello());
        Assert.Equal("test-id", result.ResponseId);
        Assert.Equal("test-model", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(123), result.ResponseTimestamp);
        OpenAIParity.AssertJson(result.RawResponse!, body);
        Assert.Equal(OpenAIParity.ChatUrl, handler.Uri);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should support partial usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_partial_usage()
    {
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "stop", """{"prompt_tokens":20,"total_tokens":20}"""));
        OpenAIParity.AssertUsage(result.Usage, 20, 0, 0, 0, null, 20);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should extract logprobs", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_logprobs()
    {
        const string content = """[{"token":"Hello","logprob":-0.0009994634,"top_logprobs":[{"token":"Hello","logprob":-0.0009994634}]}]""";
        var body = """{"id":"chatcmpl","object":"chat.completion","created":1711115037,"model":"gpt-3.5-turbo-0125","choices":[{"index":0,"message":{"role":"assistant","content":""},"logprobs":{"content":"""
            + content
            + """},"finish_reason":"stop"}],"usage":{"prompt_tokens":4,"total_tokens":34,"completion_tokens":30}}""";
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"logprobs":1}""");
        var (result, handler) = await CallAsync(body, options);
        OpenAIParity.AssertJson(result.ProviderMetadata!.Value.GetProperty("openai").GetProperty("logprobs").GetRawText(), content);
        Assert.Contains("\"logprobs\":true", handler.Body, StringComparison.Ordinal);
        Assert.Contains("\"top_logprobs\":1", handler.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should extract finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_the_finish_reason()
    {
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "stop"));
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_an_unknown_finish_reason()
    {
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "eos"));
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_raw_response_headers()
    {
        var handler = new RecordingHandler();
        handler.ResponseHeaders["test-header"] = "test-value";
        var model = OpenAIParity.Model(handler);
        var result = await model.DoGenerateAsync(OpenAIParity.Hello(), CancellationToken.None);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Equal("test-value", model.LastResponseHeaders["test-header"]);
        Assert.StartsWith("application/json", result.ResponseHeaders["Content-Type"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass the model and the messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_model_and_the_messages()
    {
        var handler = await SendAsync(OpenAIParity.Hello());
        OpenAIParity.AssertBody(handler, OpenAIParity.HelloBody);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_settings()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"logitBias":{"50256":-100},"parallelToolCalls":false,"user":"test-user-id"}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """
            {"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"logit_bias":{"50256":-100},"parallel_tool_calls":false,"user":"test-user-id"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should not set reasoning_effort when reasoning is \"provider-default\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_provider_default_reasoning()
    {
        var handler = await SendAsync(OpenAIParity.Hello("provider-default"), "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}]}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass top-level reasoning as reasoning_effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_top_level_reasoning()
    {
        var handler = await SendAsync(OpenAIParity.Hello("medium"), "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}],"reasoning_effort":"medium"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should prefer providerOptions reasoningEffort over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_provider_reasoning_effort()
    {
        var options = OpenAIParity.Hello("medium");
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"reasoningEffort":"high"}""");
        var handler = await SendAsync(options, "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}],"reasoning_effort":"high"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass reasoningEffort setting from provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_reasoning_effort_from_provider_metadata()
    {
        await AssertReasoningEffort("low");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass reasoningEffort setting from settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_reasoning_effort_from_settings()
    {
        await AssertReasoningEffort("high");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass reasoningEffort xhigh setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_xhigh_reasoning_effort()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"reasoningEffort":"xhigh"}""");
        var handler = await SendAsync(options, "gpt-5.1-codex-max");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-5.1-codex-max","messages":[{"role":"user","content":"Hello"}],"reasoning_effort":"xhigh"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass reasoningEffort max setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_max_reasoning_effort()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"reasoningEffort":"max"}""");
        var handler = await SendAsync(options, "gpt-5.6");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-5.6","messages":[{"role":"user","content":"Hello"}],"reasoning_effort":"max"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass textVerbosity setting from provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_text_verbosity()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"textVerbosity":"low"}""");
        var handler = await SendAsync(options, "gpt-4o");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-4o","messages":[{"role":"user","content":"Hello"}],"verbosity":"low"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_tools_and_tool_choice()
    {
        var options = ToolOptions(ToolChoice.Tool("test-tool"));
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, ToolBody("gpt-3.5-turbo", """{"type":"function","function":{"name":"test-tool"}}""", includeDescription: false));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_headers()
    {
        var options = new OpenAIOptions { ApiKey = "test-api-key", Organization = "test-organization", Project = "test-project" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var call = OpenAIParity.Hello();
        call.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        var handler = await SendAsync(call, provider: options);
        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.StartsWith("application/json", handler.Headers["Content-Type"], StringComparison.OrdinalIgnoreCase);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
        Assert.Equal("test-organization", handler.Headers["OpenAI-Organization"]);
        Assert.Equal("test-project", handler.Headers["OpenAI-Project"]);
        Assert.Contains("ai-sdk/openai/0.0.0-test", handler.Headers["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should parse tool results", Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_tool_results()
    {
        var message = """{"role":"assistant","content":"","tool_calls":[{"id":"call_O17Uplv4lJvD6DVdIvFFeRMw","type":"function","function":{"name":"test-tool","arguments":"{\"value\":\"Spark\"}"}}]}""";
        var result = await GenerateAsync(Choice(message, "stop"), ToolOptions(ToolChoice.Tool("test-tool")));
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_O17Uplv4lJvD6DVdIvFFeRMw", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should parse annotations/citations", Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_annotations()
    {
        var message = """
            {"role":"assistant","content":"Based on the search results [doc1], I found information.","annotations":[{"type":"url_citation","url_citation":{"start_index":24,"end_index":29,"url":"https://example.com/doc1.pdf","title":"Document 1"}}]}
            """;
        var result = await GenerateAsync(Choice(message, "stop"));
        Assert.Equal("Based on the search results [doc1], I found information.", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        var source = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.False(string.IsNullOrEmpty(source.Id));
        Assert.Equal("https://example.com/doc1.pdf", source.Url);
        Assert.Equal("Document 1", source.Title);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should not send a response_format when response format is text", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_response_format_for_text()
    {
        var handler = await SendAsync(OpenAIParity.Hello(), "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-4o-2024-08-06","messages":[{"role":"user","content":"Hello"}]}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should forward json response format as \"json_object\" without schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_json_object_without_a_schema()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"responseFormat":"json"}""");
        var handler = await SendAsync(options, "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-4o-2024-08-06","messages":[{"role":"user","content":"Hello"}],"response_format":{"type":"json_object"}}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should forward json response format as \"json_object\" and include schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_json_schema_when_a_schema_is_present()
    {
        var (result, handler) = await CallAsync(DefaultBody(), SchemaOptions(), "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, SchemaBody("response", null));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should remove string propertyNames from response schemas and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Removes_string_property_names()
    {
        var options = OpenAIParity.Hello();
        options.JsonSchema = OpenAIParity.Element("""
            {"type":"object","properties":{"variables":{"type":"object","propertyNames":{"type":"string","format":"uuid"},"additionalProperties":{"type":"string"}}},"required":["variables"],"additionalProperties":false}
            """);
        var (result, handler) = await CallAsync(DefaultBody(), options, "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, """
            {"model":"gpt-4o-2024-08-06","messages":[{"role":"user","content":"Hello"}],"response_format":{"type":"json_schema","json_schema":{"name":"response","strict":true,"schema":{"type":"object","properties":{"variables":{"type":"object","additionalProperties":{"type":"string"}}},"required":["variables"],"additionalProperties":false}}}}
            """);
        OpenAIParity.AssertWarnings(
            result.Warnings,
            ("compatibility", "JSON Schema propertyNames", "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints.", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should use json_schema & strict with responseFormat json", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_strict_json_schema()
    {
        var handler = await SendAsync(SchemaOptions(), "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, SchemaBody("response", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should set name & description with responseFormat json", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_schema_name_and_description()
    {
        var options = SchemaOptions();
        options.JsonSchemaName = "test-name";
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"schemaDescription":"test description"}""");
        var handler = await SendAsync(options, "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, SchemaBody("test-name", "test description"));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should allow for undefined schema with responseFormat json when structuredOutputs are enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_json_object_when_the_schema_is_absent()
    {
        var options = OpenAIParity.Hello();
        options.JsonSchemaName = "test-name";
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"responseFormat":"json","schemaDescription":"test description"}""");
        var handler = await SendAsync(options, "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-4o-2024-08-06","messages":[{"role":"user","content":"Hello"}],"response_format":{"type":"json_object"}}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > response format::should set strict with tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_strict_when_the_tool_does_not_set_it()
    {
        var message = """{"role":"assistant","content":"","tool_calls":[{"id":"call_O17Uplv4lJvD6DVdIvFFeRMw","type":"function","function":{"name":"test-tool","arguments":"{\"value\":\"Spark\"}"}}]}""";
        var options = ToolOptions(ToolChoice.Required, "test description");
        var (result, handler) = await CallAsync(Choice(message, "stop"), options, "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, ToolBody("gpt-4o-2024-08-06", "\"required\"", includeDescription: true));
        Assert.DoesNotContain("\"strict\"", handler.Body, StringComparison.Ordinal);
        Assert.Equal("{\"value\":\"Spark\"}", Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content)).ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should set strict for tool usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_strict_for_named_tool_choice()
    {
        var message = """{"role":"assistant","content":"","tool_calls":[{"id":"call_O17Uplv4lJvD6DVdIvFFeRMw","type":"function","function":{"name":"test-tool","arguments":"{\"value\":\"Spark\"}"}}]}""";
        var (result, handler) = await CallAsync(Choice(message, "stop"), ToolOptions(ToolChoice.Tool("test-tool")), "gpt-4o-2024-08-06");
        OpenAIParity.AssertBody(handler, ToolBody("gpt-4o-2024-08-06", """{"type":"function","function":{"name":"test-tool"}}""", includeDescription: false));
        Assert.DoesNotContain("\"strict\"", handler.Body, StringComparison.Ordinal);
        Assert.Equal("test-tool", Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content)).ToolName);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should return cached_tokens in prompt_details_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_cached_tokens()
    {
        var usage = """{"prompt_tokens":2000,"completion_tokens":20,"total_tokens":2020,"prompt_tokens_details":{"cached_tokens":1152,"cache_write_tokens":256}}""";
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "stop", usage), modelId: "gpt-4o-mini");
        OpenAIParity.AssertUsage(result.Usage, 2000, 20, 0, 1152, 256, 2020);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should return accepted_prediction_tokens and rejected_prediction_tokens in completion_details_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_prediction_tokens()
    {
        var usage = """{"prompt_tokens":15,"completion_tokens":20,"total_tokens":35,"completion_tokens_details":{"accepted_prediction_tokens":123,"rejected_prediction_tokens":456}}""";
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "stop", usage), modelId: "gpt-4o-mini");
        OpenAIParity.AssertJson(
            result.ProviderMetadata!.Value.GetProperty("openai").GetRawText(),
            """{"acceptedPredictionTokens":123,"rejectedPredictionTokens":456}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should clear out temperature, top_p, frequency_penalty, presence_penalty and return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Clears_sampling_parameters_for_reasoning_models()
    {
        var options = OpenAIParity.Hello();
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.FrequencyPenalty = 0.2;
        options.PresencePenalty = 0.3;
        var (result, handler) = await CallAsync(DefaultBody(), options, "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}]}""");
        OpenAIParity.AssertWarnings(
            result.Warnings,
            ("unsupported", "temperature", "temperature is not supported for reasoning models", null),
            ("unsupported", "topP", "topP is not supported for reasoning models", null),
            ("unsupported", "frequencyPenalty", "frequencyPenalty is not supported for reasoning models", null),
            ("unsupported", "presencePenalty", "presencePenalty is not supported for reasoning models", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should convert maxOutputTokens to max_completion_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Converts_max_output_tokens()
    {
        var options = OpenAIParity.Hello();
        options.MaxOutputTokens = 1000;
        var handler = await SendAsync(options, "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}],"max_completion_tokens":1000}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should allow temperature when top-level reasoning is none on gpt-5.1", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_temperature_when_gpt51_reasoning_is_none()
    {
        var options = OpenAIParity.Hello("none");
        options.Temperature = 0.5;
        var (result, handler) = await CallAsync(DefaultBody(), options, "gpt-5.1");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-5.1","messages":[{"role":"user","content":"Hello"}],"temperature":0.5,"reasoning_effort":"none"}""");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should preserve disabled reasoning for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_disabled_reasoning_for_gpt6_sol_and_luna()
    {
        foreach (var modelId in new[] { "gpt-6-sol", "gpt-6-luna" })
        {
            var options = OpenAIParity.Hello();
            options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"reasoningEffort":"none"}""");
            var (result, handler) = await CallAsync(DefaultBody(), options, modelId);
            using var document = JsonDocument.Parse(handler.Body);
            Assert.Equal(modelId, document.RootElement.GetProperty("model").GetString());
            Assert.Equal("none", document.RootElement.GetProperty("reasoning_effort").GetString());
            Assert.Empty(result.Warnings);
        }
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should omit unsupported GPT-6 reasoning effort %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_unsupported_gpt6_reasoning_efforts()
    {
        foreach (var effort in new[] { "none", "minimal" })
        {
            var options = OpenAIParity.Hello();
            options.ProviderOptions = OpenAIParity.OpenAIOptions("{\"reasoningEffort\":\"" + effort + "\"}");
            var (result, handler) = await CallAsync(DefaultBody(), options, "gpt-6-astra");
            OpenAIParity.AssertBody(handler, """{"model":"gpt-6-astra","messages":[{"role":"user","content":"Hello"}]}""");
            OpenAIParity.AssertWarnings(
                result.Warnings,
                ("unsupported", "reasoningEffort", "gpt-6-astra only supports the following reasoning efforts: low, medium, high, xhigh, max", null));
        }
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should strip sampling and logprob settings for GPT-6 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Strips_sampling_and_logprobs_for_gpt6()
    {
        var options = OpenAIParity.Hello();
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"reasoningEffort":"low","logprobs":5}""");
        var (result, handler) = await CallAsync(DefaultBody(), options, "gpt-6-astra");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-6-astra","messages":[{"role":"user","content":"Hello"}],"reasoning_effort":"low"}""");
        OpenAIParity.AssertWarnings(
            result.Warnings,
            ("unsupported", "temperature", "temperature is not supported for reasoning models", null),
            ("unsupported", "topP", "topP is not supported for reasoning models", null),
            ("other", null, null, "logprobs is not supported for reasoning models"),
            ("other", null, null, "topLogprobs is not supported for reasoning models"));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate > reasoning models::should still clear temperature when top-level reasoning is none on o4-mini", Coverage = UpstreamCoverage.Covered)]
    public async Task Clears_temperature_when_o4_mini_reasoning_is_none()
    {
        var options = OpenAIParity.Hello("none");
        options.Temperature = 0.5;
        var (result, handler) = await CallAsync(DefaultBody(), options, "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}],"reasoning_effort":"none"}""");
        OpenAIParity.AssertWarnings(result.Warnings, ("unsupported", "temperature", "temperature is not supported for reasoning models", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should allow forcing reasoning behavior for unrecognized model IDs via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task Forces_reasoning_for_unrecognized_ids()
    {
        var options = OpenAIParity.Hello();
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"forceReasoning":true}""");
        var (result, handler) = await CallAsync(DefaultBody(), options, "stealth-reasoning-model");
        OpenAIParity.AssertBody(handler, """{"model":"stealth-reasoning-model","messages":[{"role":"user","content":"Hello"}]}""");
        OpenAIParity.AssertWarnings(
            result.Warnings,
            ("unsupported", "temperature", "temperature is not supported for reasoning models", null),
            ("unsupported", "topP", "topP is not supported for reasoning models", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should default systemMessageMode to developer when forcing reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_forced_reasoning_to_developer_messages()
    {
        var options = SystemPrompt();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"forceReasoning":true}""");
        var (result, handler) = await CallAsync(DefaultBody(), options, "stealth-reasoning-model");
        AssertDeveloperMessages(handler, "stealth-reasoning-model");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should use developer messages for o1", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_developer_messages_for_o1()
    {
        var (result, handler) = await CallAsync(DefaultBody(), SystemPrompt(), "o1");
        AssertDeveloperMessages(handler, "o1");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should allow overriding systemMessageMode via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task Overrides_system_message_mode()
    {
        var options = SystemPrompt();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"systemMessageMode":"developer"}""");
        var (result, handler) = await CallAsync(DefaultBody(), options, "gpt-4o");
        AssertDeveloperMessages(handler, "gpt-4o");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should use default systemMessageMode when not overridden", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_system_messages_for_gpt4o()
    {
        var (result, handler) = await CallAsync(DefaultBody(), SystemPrompt(), "gpt-4o");
        OpenAIParity.AssertBody(handler, """
            {"model":"gpt-4o","messages":[{"role":"system","content":"You are a helpful assistant."},{"role":"user","content":"Hello"}]}
            """);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should return the reasoning tokens in the provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_reasoning_tokens_on_usage()
    {
        var usage = """{"prompt_tokens":15,"completion_tokens":20,"total_tokens":35,"completion_tokens_details":{"reasoning_tokens":10}}""";
        var result = await GenerateAsync(Choice("""{"role":"assistant","content":""}""", "stop", usage), modelId: "o4-mini");
        OpenAIParity.AssertUsage(result.Usage, 15, 20, 10, 0, null, 35);
        Assert.Equal("{}", result.ProviderMetadata!.Value.GetProperty("openai").GetRawText());
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send max_completion_tokens extension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_max_completion_tokens()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"maxCompletionTokens":255}""");
        var handler = await SendAsync(options, "o4-mini");
        OpenAIParity.AssertBody(handler, """{"model":"o4-mini","messages":[{"role":"user","content":"Hello"}],"max_completion_tokens":255}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send prediction extension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_prediction()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"prediction":{"type":"content","content":"Hello, World!"}}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"prediction":{"type":"content","content":"Hello, World!"}}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send store extension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_store()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"store":true}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"store":true}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send metadata extension values", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_metadata()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"metadata":{"custom":"value"}}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"metadata":{"custom":"value"}}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send promptCacheKey extension value", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_prompt_cache_key()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"promptCacheKey":"test-cache-key-123"}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"prompt_cache_key":"test-cache-key-123"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send promptCacheRetention extension value", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_prompt_cache_retention()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"promptCacheRetention":"24h"}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"prompt_cache_retention":"24h"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send promptCacheOptions extension value", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_prompt_cache_options()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"promptCacheOptions":{"mode":"explicit","ttl":"30m"}}""");
        var handler = await SendAsync(options, "gpt-5.6");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-5.6","messages":[{"role":"user","content":"Hello"}],"prompt_cache_options":{"mode":"explicit","ttl":"30m"}}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should omit legacy prompt cache retention for GPT-6 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_legacy_prompt_cache_retention_for_gpt6()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"promptCacheRetention":"24h"}""");
        var (result, handler) = await CallAsync(DefaultBody(), options, "gpt-6-astra");
        OpenAIParity.AssertBody(handler, """{"model":"gpt-6-astra","messages":[{"role":"user","content":"Hello"}]}""");
        OpenAIParity.AssertWarnings(
            result.Warnings,
            ("unsupported", "promptCacheRetention", "promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send safetyIdentifier extension value", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_safety_identifier()
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("""{"safetyIdentifier":"test-safety-identifier-123"}""");
        var handler = await SendAsync(options);
        OpenAIParity.AssertBody(handler, """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}],"safety_identifier":"test-safety-identifier-123"}""");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should remove temperature setting for gpt-4o-search-preview and add warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Removes_temperature_for_gpt4o_search_preview()
    {
        await AssertSearchTemperatureRemoved("gpt-4o-search-preview");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should remove temperature setting for gpt-4o-mini-search-preview and add warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Removes_temperature_for_gpt4o_mini_search_preview()
    {
        await AssertSearchTemperatureRemoved("gpt-4o-mini-search-preview");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should remove temperature setting for gpt-4o-mini-search-preview-2025-03-11 and add warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Removes_temperature_for_dated_search_preview()
    {
        await AssertSearchTemperatureRemoved("gpt-4o-mini-search-preview-2025-03-11");
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send serviceTier flex processing setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_flex_service_tier()
    {
        await AssertServiceTier("o4-mini", "flex", sent: true);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should show warning when using flex processing with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_flex_is_unsupported()
    {
        var result = await AssertServiceTier("gpt-4o-mini", "flex", sent: false);
        OpenAIParity.AssertWarnings(result.Warnings, ("unsupported", "serviceTier", "flex processing is only available for o3, o4-mini, and gpt-5 models", null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should allow flex processing with o4-mini model without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_flex_on_o4_mini()
    {
        var result = await AssertServiceTier("o4-mini", "flex", sent: true);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send serviceTier priority processing setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_priority_service_tier()
    {
        await AssertServiceTier("gpt-4o-mini", "priority", sent: true);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should show warning when using priority processing with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_priority_is_unsupported()
    {
        var result = await AssertServiceTier("gpt-3.5-turbo", "priority", sent: false);
        OpenAIParity.AssertWarnings(result.Warnings, ("unsupported", "serviceTier", PriorityWarning, null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should allow priority processing with gpt-4o model without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_priority_on_gpt4o()
    {
        var result = await AssertServiceTier("gpt-4o", "priority", sent: true);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should allow priority processing with o3 model without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_priority_on_o4_mini()
    {
        var result = await AssertServiceTier("o4-mini", "priority", sent: true);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send serviceTier fast processing setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_fast_service_tier()
    {
        await AssertServiceTier("gpt-4o-mini", "fast", sent: true);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should show warning when using fast processing with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_fast_is_unsupported()
    {
        var result = await AssertServiceTier("gpt-3.5-turbo", "fast", sent: false);
        OpenAIParity.AssertWarnings(result.Warnings, ("unsupported", "serviceTier", PriorityWarning, null));
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should allow fast processing with gpt-4o model without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_fast_on_gpt4o()
    {
        var result = await AssertServiceTier("gpt-4o", "fast", sent: true);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::doGenerate::should send serviceTier ultrafast processing setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_ultrafast_service_tier()
    {
        await AssertServiceTier("gpt-5.6-sol", "ultrafast", sent: true);
    }

    private static async Task AssertReasoningEffort(string effort)
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("{\"reasoningEffort\":\"" + effort + "\"}");
        var handler = await SendAsync(options, "o4-mini");
        OpenAIParity.AssertBody(handler, "{\"model\":\"o4-mini\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"reasoning_effort\":\"" + effort + "\"}");
    }

    private static async Task AssertSearchTemperatureRemoved(string modelId)
    {
        var options = OpenAIParity.Hello();
        options.Temperature = 0.7;
        var (result, handler) = await CallAsync(DefaultBody(), options, modelId);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.Equal(modelId, document.RootElement.GetProperty("model").GetString());
        Assert.False(document.RootElement.TryGetProperty("temperature", out _));
        OpenAIParity.AssertWarnings(result.Warnings, ("unsupported", "temperature", SearchWarning, null));
    }

    private static async Task<LanguageModelGenerateResult> AssertServiceTier(string modelId, string tier, bool sent)
    {
        var options = OpenAIParity.Hello();
        options.ProviderOptions = OpenAIParity.OpenAIOptions("{\"serviceTier\":\"" + tier + "\"}");
        var (result, handler) = await CallAsync(DefaultBody(), options, modelId);
        using var document = JsonDocument.Parse(handler.Body);
        if (sent)
        {
            Assert.Equal(tier, document.RootElement.GetProperty("service_tier").GetString());
            Assert.Equal(modelId, document.RootElement.GetProperty("model").GetString());
        }
        else
        {
            Assert.False(document.RootElement.TryGetProperty("service_tier", out _));
        }

        return result;
    }

    private static void AssertDeveloperMessages(RecordingHandler handler, string modelId)
    {
        OpenAIParity.AssertBody(handler, "{\"model\":\"" + modelId + "\",\"messages\":[{\"role\":\"developer\",\"content\":\"You are a helpful assistant.\"},{\"role\":\"user\",\"content\":\"Hello\"}]}");
    }

    private static LanguageModelCallOptions SystemPrompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
        };
    }

    private static LanguageModelCallOptions SchemaOptions()
    {
        var options = OpenAIParity.Hello();
        options.JsonSchema = OpenAIParity.Element(OpenAIParity.ValueSchema);
        return options;
    }

    private static string SchemaBody(string name, string? description)
    {
        var descriptionJson = description == null ? string.Empty : ",\"description\":\"" + description + "\"";
        return "{\"model\":\"gpt-4o-2024-08-06\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"response_format\":{\"type\":\"json_schema\",\"json_schema\":{\"schema\":" + OpenAIParity.ValueSchema + ",\"strict\":true,\"name\":\"" + name + "\"" + descriptionJson + "}}}";
    }

    private static LanguageModelCallOptions ToolOptions(ToolChoice choice, string? description = null)
    {
        var options = OpenAIParity.Hello();
        options.Tools = new[] { OpenAIParity.ValueTool(description: description) };
        options.ToolChoice = choice;
        return options;
    }

    private static string ToolBody(string modelId, string toolChoice, bool includeDescription)
    {
        var description = includeDescription ? "\"description\":\"test description\"," : string.Empty;
        return "{\"model\":\"" + modelId + "\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"tools\":[{\"type\":\"function\",\"function\":{" + description + "\"name\":\"test-tool\",\"parameters\":" + OpenAIParity.ValueSchema + "}}],\"tool_choice\":" + toolChoice + "}";
    }

    private static string Choice(string message, string finishReason, string? usage = null, bool usageSpecified = true)
    {
        var usageJson = usage ?? """{"prompt_tokens":4,"total_tokens":34,"completion_tokens":30}""";
        var usageField = usageSpecified ? ",\"usage\":" + usageJson : string.Empty;
        return "{\"id\":\"chatcmpl-95ZTZkhr0mHNKqerQfiwkuox3PHAd\",\"object\":\"chat.completion\",\"created\":1711115037,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[{\"index\":0,\"message\":" + message + ",\"finish_reason\":\"" + finishReason + "\"}]" + usageField + ",\"system_fingerprint\":\"fp_3bc1b5746c\"}";
    }

    private static string DefaultBody()
    {
        return Choice("""{"role":"assistant","content":"ok"}""", "stop");
    }

    private static async Task<LanguageModelGenerateResult> GenerateAsync(string body, LanguageModelCallOptions? options = null, string modelId = "gpt-3.5-turbo")
    {
        var (result, _) = await CallAsync(body, options ?? OpenAIParity.Hello(), modelId);
        return result;
    }

    private static async Task<RecordingHandler> SendAsync(LanguageModelCallOptions options, string modelId = "gpt-3.5-turbo", OpenAIOptions? provider = null)
    {
        var (_, handler) = await CallAsync(DefaultBody(), options, modelId, provider);
        return handler;
    }

    private static async Task<(LanguageModelGenerateResult Result, RecordingHandler Handler)> CallAsync(
        string body,
        LanguageModelCallOptions options,
        string modelId = "gpt-3.5-turbo",
        OpenAIOptions? provider = null)
    {
        var handler = new RecordingHandler { ResponseBody = body };
        var model = OpenAIParity.Model(handler, modelId, provider);
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        return (result, handler);
    }
}
