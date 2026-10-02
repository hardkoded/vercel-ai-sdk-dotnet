// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Upstream Gemini <c>generateContent</c> request and response mapping.</summary>
public sealed class GoogleGenerateContentTests
{
    private const string File = "packages/google/src/google-language-model.test.ts::";

    [Fact]
    [UpstreamTest(File + "doGenerate > text::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text()
    {
        var capture = Response("{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"Hello\"}]},\"finishReason\":\"STOP\"}]}");
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("Hello", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.EndsWith("/models/gemini-2.0-flash:generateContent", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
        Assert.Equal("test-key", GoogleParity.Header(capture, "x-goog-api-key"));
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > text::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_usage()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"ok"}],"role":"model"},"finishReason":"STOP"}],
             "usageMetadata":{"promptTokenCount":9,"candidatesTokenCount":28,"thoughtsTokenCount":244,"totalTokenCount":281}}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Equal(9, result.Usage.InputTokens);
        Assert.Equal(272, result.Usage.OutputTokens);
        Assert.Equal(244, result.Usage.ReasoningTokens);
        Assert.Equal(281, result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > text::should preserve complete raw usage metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_raw_usage_metadata()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"ok"}]},"finishReason":"STOP"}],
             "usageMetadata":{"promptTokenCount":9,"candidatesTokenCount":28,"thoughtsTokenCount":244,"totalTokenCount":281,"trafficType":"ON_DEMAND"}}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Equal("ON_DEMAND", result.Usage.Raw!.Value.GetProperty("trafficType").GetString());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > text::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_response_id()
    {
        var capture = Response("{\"responseId\":\"Un6LacrVMcjUxs0PmJfWoQc\",\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}");
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Equal("Un6LacrVMcjUxs0PmJfWoQc", result.ResponseId);
        Assert.Null(result.ResponseModelId);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should handle MALFORMED_FUNCTION_CALL finish reason and empty content object", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_a_malformed_function_call_to_an_error()
    {
        var capture = Response("{\"candidates\":[{\"content\":{},\"finishReason\":\"MALFORMED_FUNCTION_CALL\"}]}");
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Equal(FinishReason.Error, result.FinishReason);
        Assert.Equal("MALFORMED_FUNCTION_CALL", result.RawFinishReason);
        Assert.Empty(result.Content);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should expose finishMessage in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_finish_message_in_provider_metadata()
    {
        var capture = Response("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\",\"finishMessage\":\"stopped\"}]}");
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("stopped", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("finishMessage").GetString());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should expose null finishMessage in provider metadata when not present", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_a_null_finish_message_when_it_is_absent()
    {
        var result = await GoogleParity.Generate(new GoogleCapture(), "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal(JsonValueKindNull(), result.ProviderMetadata!.Value.GetProperty("google").GetProperty("finishMessage").ValueKind);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should send serviceTier in request body when specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_service_tier()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"serviceTier\":\"flex\"}");
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options);
        Assert.Equal("flex", GoogleParity.Request(capture)["serviceTier"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should not send serviceTier in request body when not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_service_tier_when_it_is_unset()
    {
        var capture = new GoogleCapture();
        await GoogleParity.Generate(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Null(GoogleParity.Request(capture)["serviceTier"]);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should send sharedRequestType as X-Vertex-AI-LLM-Shared-Request-Type header on Vertex", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_Vertex_shared_request_header()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement> { ["vertex"] = GoogleParity.Json("{\"sharedRequestType\":\"priority\"}") };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        Assert.Equal("priority", GoogleParity.Header(capture, "X-Vertex-AI-LLM-Shared-Request-Type"));
        Assert.Contains("us-central1-aiplatform.googleapis.com", capture.Url!.Host, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should send requestType as X-Vertex-AI-LLM-Request-Type header on Vertex", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_Vertex_request_type_header()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement> { ["vertex"] = GoogleParity.Json("{\"requestType\":\"shared\"}") };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        Assert.Equal("shared", GoogleParity.Header(capture, "X-Vertex-AI-LLM-Request-Type"));
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should warn and drop serviceTier on Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Drops_service_tier_on_Vertex()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement> { ["vertex"] = GoogleParity.Json("{\"serviceTier\":\"flex\"}") };
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        Assert.Null(GoogleParity.Request(capture)["serviceTier"]);
        Assert.Contains("serviceTier", result.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should warn when sharedRequestType is set on a non-Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_a_Vertex_request_type_is_set_on_Google()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"sharedRequestType\":\"priority\"}");
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", options);
        Assert.Null(GoogleParity.Header(capture, "X-Vertex-AI-LLM-Shared-Request-Type"));
        Assert.Contains("sharedRequestType", result.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should read serviceTier from usageMetadata.serviceTier", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_service_tier_from_usage()
    {
        var capture = Response("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}],\"usageMetadata\":{\"promptTokenCount\":1,\"serviceTier\":\"flex\"}}");
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Equal("flex", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("serviceTier").GetString());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should expose null serviceTier in provider metadata when not present", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_a_null_service_tier_when_it_is_absent()
    {
        var result = await GoogleParity.Generate(new GoogleCapture(), "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal(JsonValueKindNull(), result.ProviderMetadata!.Value.GetProperty("google").GetProperty("serviceTier").ValueKind);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > tool-call::should extract tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_tool_calls()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"functionCall":{"name":"getWeather","args":{"city":"Paris"},"id":"call-1"}}]},"finishReason":"STOP"}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("getWeather", call.ToolName);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > reasoning-gemini3::should extract reasoning with thoughtSignature", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_reasoning_and_its_thought_signature()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"thinking","thought":true,"thoughtSignature":"sig"}]},"finishReason":"STOP"}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-3-flash", GoogleParity.Prompt());
        Assert.Equal("thinking", Assert.IsType<GeneratedReasoning>(result.Content[0]).Text);
        var signed = Assert.IsType<GoogleSignedPart>(result.Content[1]);
        Assert.Equal("sig", signed.ProviderMetadata.GetProperty("google").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > tool-call-gemini3::should extract tool call with thoughtSignature", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_a_tool_call_thought_signature()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"functionCall":{"name":"weather","args":{"city":"SF"},"id":"c1"},"thoughtSignature":"sig"}]},"finishReason":"STOP"}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-3-flash", GoogleParity.Prompt());
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("sig", call.ProviderMetadata!.Value.GetProperty("google").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > no-args tool call (unary)::should extract a no-args tool call with thoughtSignature as input \"{}\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_a_no_args_tool_call_as_an_empty_object()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"functionCall":{"name":"ping","id":"c1"},"thoughtSignature":"sig"}]},"finishReason":"STOP"}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-3-flash", GoogleParity.Prompt());
        Assert.Equal("{}", Assert.IsType<GeneratedToolCall>(result.Content[0]).ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate > no-args tool call (unary)::should generate an ID when the function call ID is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Generates_an_id_when_the_function_call_id_is_empty()
    {
        var capture = Response("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"ping\",\"id\":\"\"}}]},\"finishReason\":\"STOP\"}]}");
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("id-1", Assert.IsType<GeneratedToolCall>(result.Content[0]).ToolCallId);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should omit function call IDs from Vertex requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_function_call_ids_from_Vertex_requests()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new AssistantModelMessage(null, new[] { new GeneratedToolCall("call-1", "weather", "{\"city\":\"Paris\"}") }, null),
            },
        };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        var call = GoogleParity.Request(capture)["contents"]![0]!["parts"]![0]!["functionCall"]!;
        Assert.Null(call["id"]);
        Assert.Equal("weather", call["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should pass the model, messages, and options", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_model_messages_and_options()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
            MaxOutputTokens = 16,
            Temperature = 0.2,
            TopP = 0.9,
            TopK = 8,
            Seed = 7,
            StopSequences = new[] { "END" },
        };
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var body = GoogleParity.Request(capture);
        Assert.Equal(16, body["generationConfig"]!["maxOutputTokens"]!.GetValue<int>());
        Assert.Equal(0.2, body["generationConfig"]!["temperature"]!.GetValue<double>());
        Assert.Equal("Be brief.", body["systemInstruction"]!["parts"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("Hello", body["contents"]![0]!["parts"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("END", body["generationConfig"]!["stopSequences"]![0]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should omit unsupported %s for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_penalties_for_Gemini_2_5()
    {
        var frequency = new GoogleCapture();
        var frequencyOptions = GoogleParity.Prompt();
        frequencyOptions.FrequencyPenalty = 0.5;
        var frequencyResult = await GoogleParity.Generate(frequency, "gemini-2.5-flash", frequencyOptions);
        Assert.Null(GoogleParity.Request(frequency)["generationConfig"]!["frequencyPenalty"]);
        Assert.Contains(frequencyResult.Warnings, warning => warning.Message == "frequencyPenalty");

        var presence = new GoogleCapture();
        var presenceOptions = GoogleParity.Prompt();
        presenceOptions.PresencePenalty = 0.4;
        var presenceResult = await GoogleParity.Generate(presence, "gemini-2.5-pro", presenceOptions);
        Assert.Null(GoogleParity.Request(presence)["generationConfig"]!["presencePenalty"]);
        Assert.Contains(presenceResult.Warnings, warning => warning.Message == "presencePenalty");
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should pass penalty settings for Gemini 2.0 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_penalties_for_Gemini_2_0()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.4;
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var config = GoogleParity.Request(capture)["generationConfig"]!;
        Assert.Equal(0.5, config["frequencyPenalty"]!.GetValue<double>());
        Assert.Equal(0.4, config["presencePenalty"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should pass penalty settings for Vertex Gemini 2.5 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_penalties_for_Vertex_Gemini_2_5()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.4;
        var result = await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        var config = GoogleParity.Request(capture)["generationConfig"]!;
        Assert.Equal(0.5, config["frequencyPenalty"]!.GetValue<double>());
        Assert.Equal(0.4, config["presencePenalty"]!.GetValue<double>());
        Assert.DoesNotContain(result.Warnings, warning => warning.Message == "frequencyPenalty");
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should expand standalone threshold provider option into safetySettings", Coverage = UpstreamCoverage.Covered)]
    public async Task Expands_a_threshold_into_safety_settings()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"threshold\":\"BLOCK_ONLY_HIGH\"}");
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var settings = GoogleParity.Request(capture)["safetySettings"]!.AsArray();
        Assert.Equal(4, settings.Count);
        Assert.Equal("HARM_CATEGORY_HATE_SPEECH", settings[0]!["category"]!.GetValue<string>());
        Assert.Equal("BLOCK_ONLY_HIGH", settings[0]!["threshold"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should let safetySettings take precedence over standalone threshold provider option", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_explicit_safety_settings_over_a_threshold()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("""
            {"threshold":"BLOCK_ONLY_HIGH","safetySettings":[{"category":"HARM_CATEGORY_HARASSMENT","threshold":"BLOCK_NONE"}]}
            """);
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var settings = GoogleParity.Request(capture)["safetySettings"]!.AsArray();
        Assert.Single(settings);
        Assert.Equal("BLOCK_NONE", settings[0]!["threshold"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_tools_and_tool_choice()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Tools = new[] { new LanguageModelTool("weather", "Weather", GoogleParity.Json("{\"type\":\"object\"}")) };
        options.ToolChoice = ToolChoice.Required;
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var body = GoogleParity.Request(capture);
        Assert.Equal("weather", body["tools"]![0]!["functionDeclarations"]![0]!["name"]!.GetValue<string>());
        Assert.Equal("ANY", body["toolConfig"]!["functionCallingConfig"]!["mode"]!.GetValue<string>());
        Assert.NotNull(body["tools"]![0]!["functionDeclarations"]![0]!["parametersJsonSchema"]);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should send recursive tool schemas as JSON Schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_recursive_tool_schemas_as_json_schema()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Tools = new[]
        {
            new LanguageModelTool("search", "Search", GoogleParity.Json("{\"type\":\"object\",\"properties\":{\"node\":{\"$ref\":\"#/$defs/Node\"}},\"$defs\":{\"Node\":{\"type\":\"object\"}}}")),
        };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options);
        Assert.Equal("#/$defs/Node", GoogleParity.Request(capture)["tools"]![0]!["functionDeclarations"]![0]!["parametersJsonSchema"]!["properties"]!["node"]!["$ref"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-language-model.test.ts::should preserve local JSON Schema references in tool requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_local_schema_references_on_Vertex()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Tools = new[] { new LanguageModelTool("search", "Search", GoogleParity.Json("{\"type\":\"object\",\"properties\":{\"node\":{\"$ref\":\"#/$defs/Node\"}}}")) };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        Assert.Equal("#/$defs/Node", GoogleParity.Request(capture)["tools"]![0]!["functionDeclarations"]![0]!["parametersJsonSchema"]!["properties"]!["node"]!["$ref"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should set response mime type with responseFormat", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_the_json_response_mime_type()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.JsonSchema = GoogleParity.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}");
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        Assert.Equal("application/json", GoogleParity.Request(capture)["generationConfig"]!["responseMimeType"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should pass specification with responseFormat and structuredOutputs = true (default)", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_response_json_schema_by_default()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.JsonSchema = GoogleParity.Json("{\"type\":\"object\",\"properties\":{\"kind\":{\"type\":\"string\",\"const\":\"fruit\"}}}");
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var schema = GoogleParity.Request(capture)["generationConfig"]!["responseJsonSchema"]!;
        Assert.Equal("fruit", schema["properties"]!["kind"]!["enum"]![0]!.GetValue<string>());
        Assert.Null(schema["properties"]!["kind"]!["const"]);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should not pass specification with responseFormat and structuredOutputs = false", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_the_response_schema_when_structured_outputs_are_disabled()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.JsonSchema = GoogleParity.Json("{\"type\":\"object\"}");
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"structuredOutputs\":false}");
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var config = GoogleParity.Request(capture)["generationConfig"]!;
        Assert.Equal("application/json", config["responseMimeType"]!.GetValue<string>());
        Assert.Null(config["responseJsonSchema"]);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_request_headers()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        Assert.Equal("yes", GoogleParity.Header(capture, "X-Custom"));
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should extract sources from grounding metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_web_sources()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"ok"}]},"finishReason":"STOP","groundingMetadata":{"groundingChunks":[{"web":{"uri":"https://example.com","title":"Example"}}]}}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        var source = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.Equal("https://example.com", source.Url);
        Assert.Equal("Example", source.Title);
        Assert.Equal("id-1", source.Id);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should extract sources from maps grounding metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_maps_sources()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"ok"}]},"finishReason":"STOP","groundingMetadata":{"groundingChunks":[{"maps":{"uri":"https://maps.google.com/?cid=1","title":"Place"}}]}}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("https://maps.google.com/?cid=1", Assert.IsType<GeneratedSource>(result.Content[1]).Url);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should extract sources from image grounding metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_image_sources()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"ok"}]},"finishReason":"STOP","groundingMetadata":{"groundingChunks":[{"image":{"sourceUri":"https://example.com/a.png","title":"A"}}]}}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("https://example.com/a.png", Assert.IsType<GeneratedSource>(result.Content[1]).Url);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should expose safety ratings in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_safety_ratings()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"text":"ok"}]},"finishReason":"STOP","safetyRatings":[{"category":"HARM_CATEGORY_HARASSMENT","probability":"NEGLIGIBLE"}]}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("NEGLIGIBLE", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("safetyRatings")[0].GetProperty("probability").GetString());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should expose PromptFeedback in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_prompt_feedback()
    {
        var capture = Response("""
            {"promptFeedback":{"blockReason":"SAFETY"},"candidates":[{"content":{"parts":[{"text":"ok"}]},"finishReason":"SAFETY"}]}
            """);
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Equal("SAFETY", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("promptFeedback").GetProperty("blockReason").GetString());
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "GEMMA Model System Instruction Fix::should NOT send systemInstruction for GEMMA-3-12b-it model", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_send_a_system_instruction_for_Gemma()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
        };
        await GoogleParity.Generate(capture, "gemma-3-12b-it", options);
        var body = GoogleParity.Request(capture);
        Assert.Null(body["systemInstruction"]);
        Assert.StartsWith("Be brief.", body["contents"]![0]!["parts"]![0]!["text"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "GEMMA Model System Instruction Fix::should still send systemInstruction for Gemini models (regression test)", Coverage = UpstreamCoverage.Covered)]
    public async Task Still_sends_a_system_instruction_for_Gemini()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
        };
        await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        Assert.NotNull(GoogleParity.Request(capture)["systemInstruction"]);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should use the custom code execution tool name for generated and streamed results", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_custom_code_execution_name()
    {
        var capture = Response("""
            {"candidates":[{"content":{"parts":[{"executableCode":{"language":"PYTHON","code":"print(1)"}},{"codeExecutionResult":{"outcome":"OUTCOME_OK","output":"1"}}]},"finishReason":"STOP"}]}
            """);
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"providerTools\":[{\"id\":\"google.code_execution\",\"name\":\"run_code\",\"args\":{}}]}");
        var result = await GoogleParity.Generate(capture, "gemini-2.0-flash", options);
        var call = Assert.IsType<GoogleProviderToolCall>(result.Content[0]);
        Assert.Equal("run_code", call.ToolName);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("run_code", Assert.IsType<GoogleToolResultContent>(result.Content[1]).ToolName);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should use newest request behavior for an unknown future Gemini model", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_Gemini_3_request_behavior_for_an_unknown_model()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Tools = new[] { new LanguageModelTool("weather", "Weather", GoogleParity.Json("{\"type\":\"object\"}")) };
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"providerTools\":[{\"id\":\"google.google_search\",\"name\":\"google_search\",\"args\":{}}]}");
        await GoogleParity.Generate(capture, "gemini-99-pro-preview", options);
        var body = GoogleParity.Request(capture);
        Assert.True(body["toolConfig"]!["includeServerSideToolInvocations"]!.GetValue<bool>());
        Assert.Equal("VALIDATED", body["toolConfig"]!["functionCallingConfig"]!["mode"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "doGenerate::should send PDF tool result data as inlineData for Gemini 2.5 legacy tool results", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_pdf_tool_results_as_inline_data_for_Gemini_2_5()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new GooglePromptMessage(
                    "tool",
                    null,
                    new[]
                    {
                        GooglePromptPart.ToolResultContent(
                            "call-1",
                            "catalogSearch",
                            new[] { GooglePromptPart.FileBase64("JVBERi0xLjQK", "application/pdf") }),
                    }),
            },
        };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options);
        var parts = GoogleParity.Request(capture)["contents"]![0]!["parts"]!.AsArray();
        Assert.Equal("JVBERi0xLjQK", parts[0]!["inlineData"]!["data"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "GEMMA Model System Instruction Fix::should NOT send systemInstruction for GEMMA-3-27b-it model", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_send_a_system_instruction_for_Gemma_27b()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
        };
        await GoogleParity.Generate(capture, "gemma-3-27b-it", options);
        Assert.Null(GoogleParity.Request(capture)["systemInstruction"]);
    }

    [Fact]
    [UpstreamTest(File + "GEMMA Model System Instruction Fix::should NOT generate warning when GEMMA model is used without system instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_warn_when_Gemma_has_no_system_instruction()
    {
        var result = await GoogleParity.Generate(new GoogleCapture(), "gemma-3-12b-it", GoogleParity.Prompt());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "GEMMA Model System Instruction Fix::should NOT generate warning when Gemini model is used with system instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_warn_when_Gemini_has_a_system_instruction()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
        };
        var result = await GoogleParity.Generate(new GoogleCapture(), "gemini-2.0-flash", options);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "GEMMA Model System Instruction Fix::should prepend system instruction to first user message for GEMMA models", Coverage = UpstreamCoverage.Covered)]
    public async Task Prepends_the_system_instruction_on_the_Gemma_request()
    {
        var capture = new GoogleCapture();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
        };
        await GoogleParity.Generate(capture, "gemma-3-12b-it", options);
        Assert.Equal("Be brief.\n\n", GoogleParity.Request(capture)["contents"]![0]!["parts"]![0]!["text"]!.GetValue<string>());
    }

    private static GoogleCapture Response(string json)
    {
        return new GoogleCapture { ResponseJson = json };
    }

    private static System.Text.Json.JsonValueKind JsonValueKindNull()
    {
        return System.Text.Json.JsonValueKind.Null;
    }
}
