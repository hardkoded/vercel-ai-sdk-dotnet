// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Gemini message conversion, generateContent requests, and response parsing.</summary>
public sealed class GoogleLanguageUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::system messages::should store system message in system instruction", Coverage = UpstreamCoverage.Covered)]
    public void Stores_system_text_in_system_instruction()
    {
        var prompt = Convert(new GoogleMessageOptions(), new GoogleSystemTurn("Test"), new GoogleUserTurn(new GoogleUserPart[] { new GoogleTextUserPart("Hello") }));
        GoogleUpstream.JsonEqual(prompt.SystemInstruction, "{\"parts\":[{\"text\":\"Test\"}]}");
        GoogleUpstream.JsonEqual(prompt.Contents, "[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello\"}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::system messages::should throw error when there was already a user message", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_system_message_after_a_user_message()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Convert(
            new GoogleMessageOptions(),
            new GoogleUserTurn(new GoogleUserPart[] { new GoogleTextUserPart("Hello") }),
            new GoogleSystemTurn("late")));
        Assert.Equal("system messages are only supported at the beginning of the conversation", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::thought signatures::should preserve thought signatures in assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_thought_signatures_on_assistant_parts()
    {
        var text = new GoogleTextAssistantPart("Hello") { ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"sig1\"}}") };
        var reasoning = new GoogleReasoningAssistantPart("think") { ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"sig2\"}}") };
        var call = new GoogleToolCallAssistantPart("test", "{\"value\":\"test\"}") { ToolCallId = "call1", ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"sig3\"}}") };
        var prompt = Convert(new GoogleMessageOptions(), new GoogleAssistantTurn(new GoogleAssistantPart[] { text, reasoning, call }));
        GoogleUpstream.JsonEqual(prompt.Contents, "[{\"role\":\"model\",\"parts\":[{\"text\":\"Hello\",\"thoughtSignature\":\"sig1\"},{\"text\":\"think\",\"thought\":true,\"thoughtSignature\":\"sig2\"},{\"functionCall\":{\"name\":\"test\",\"args\":{\"value\":\"test\"},\"id\":\"call1\"},\"thoughtSignature\":\"sig3\"}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::thought signatures with vertex providerOptionsName::should resolve thoughtSignature from google namespace when using vertex providerOptionsName", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_a_google_thought_signature_for_a_vertex_namespace()
    {
        var text = new GoogleTextAssistantPart("Hello") { ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"from-google\"}}") };
        var prompt = Convert(new GoogleMessageOptions { ProviderOptionNames = new[] { "googleVertex", "vertex" } }, new GoogleAssistantTurn(new GoogleAssistantPart[] { text }));
        GoogleUpstream.JsonEqual(prompt.Contents, "[{\"role\":\"model\",\"parts\":[{\"text\":\"Hello\",\"thoughtSignature\":\"from-google\"}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::thought signatures with vertex providerOptionsName::should prefer vertex namespace over google namespace when both are present", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_vertex_thought_signature_namespace()
    {
        var text = new GoogleTextAssistantPart("Hello") { ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"from-google\"},\"vertex\":{\"thoughtSignature\":\"from-vertex\"}}") };
        var prompt = Convert(new GoogleMessageOptions { ProviderOptionNames = new[] { "googleVertex", "vertex" } }, new GoogleAssistantTurn(new GoogleAssistantPart[] { text }));
        GoogleUpstream.JsonEqual(prompt.Contents[0]!["parts"]![0]!, "{\"text\":\"Hello\",\"thoughtSignature\":\"from-vertex\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::Gemma model system instructions::should prepend system instruction to first user message for Gemma models", Coverage = UpstreamCoverage.Covered)]
    public void Prepends_the_system_instruction_for_Gemma()
    {
        var prompt = Convert(
            new GoogleMessageOptions { IsGemmaModel = true },
            new GoogleSystemTurn("You are a helpful assistant."),
            new GoogleUserTurn(new GoogleUserPart[] { new GoogleTextUserPart("Hello") }));
        Assert.Null(prompt.SystemInstruction);
        GoogleUpstream.JsonEqual(prompt.Contents, "[{\"role\":\"user\",\"parts\":[{\"text\":\"You are a helpful assistant.\\n\\n\"},{\"text\":\"Hello\"}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::Gemma model system instructions::should not affect non-Gemma models", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_system_instruction_on_non_Gemma_models()
    {
        var prompt = Convert(
            new GoogleMessageOptions { ModelId = "gemini-2.5-flash" },
            new GoogleSystemTurn("You are a helpful assistant."),
            new GoogleUserTurn(new GoogleUserPart[] { new GoogleTextUserPart("Hello") }));
        GoogleUpstream.JsonEqual(prompt.SystemInstruction, "{\"parts\":[{\"text\":\"You are a helpful assistant.\"}]}");
        GoogleUpstream.JsonEqual(prompt.Contents, "[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello\"}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::Gemini 3 missing thoughtSignature mitigation::injects skip_thought_signature_validator and emits a warning for Gemini 3 when a tool-call has no signature", Coverage = UpstreamCoverage.Covered)]
    public void Injects_the_skip_sentinel_for_an_unsigned_Gemini_3_tool_call()
    {
        var call = new GoogleToolCallAssistantPart("getWeather", "{\"location\":\"SF\"}");
        var options = new GoogleMessageOptions { ModelId = "gemini-3-pro-preview" };
        var prompt = Convert(options, new GoogleAssistantTurn(new GoogleAssistantPart[] { call }));
        Assert.Equal(GoogleMessages.SkipThoughtSignatureValidator, (string?)prompt.Contents[0]!["parts"]![0]!["thoughtSignature"]);
        Assert.Contains(options.Warnings, warning => warning.Message.Contains("getWeather") && warning.Message.Contains(GoogleMessages.SkipThoughtSignatureValidator));
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel for non-Gemini-3 models", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_skip_sentinel_for_Gemini_2_5()
    {
        var call = new GoogleToolCallAssistantPart("getWeather", "{}");
        var options = new GoogleMessageOptions { ModelId = "gemini-2.5-flash" };
        var prompt = Convert(options, new GoogleAssistantTurn(new GoogleAssistantPart[] { call }));
        Assert.Null(prompt.Contents[0]!["parts"]![0]!["thoughtSignature"]);
        Assert.Empty(options.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel when a real signature is present under `google`", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_skip_sentinel_when_a_signature_is_present()
    {
        var call = new GoogleToolCallAssistantPart("getWeather", "{}") { ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"signed\"}}") };
        var options = new GoogleMessageOptions { ModelId = "gemini-3-pro-preview" };
        var prompt = Convert(options, new GoogleAssistantTurn(new GoogleAssistantPart[] { call }));
        Assert.Equal("signed", (string?)prompt.Contents[0]!["parts"]![0]!["thoughtSignature"]);
        Assert.Empty(options.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass the model, messages, and options", Coverage = UpstreamCoverage.Covered)]
    public void Passes_the_model_messages_and_options()
    {
        var options = GoogleUpstream.Prompt(new SystemModelMessage("test system instruction"), new UserModelMessage("Hello"));
        options.Temperature = 0.5;
        options.Seed = 123;
        GoogleUpstream.JsonEqual(Prep("gemini-pro", options).Body, "{\"generationConfig\":{\"temperature\":0.5,\"seed\":123},\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello\"}]}],\"systemInstruction\":{\"parts\":[{\"text\":\"test system instruction\"}]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should send serviceTier in request body when specified", Coverage = UpstreamCoverage.Covered)]
    public void Sends_serviceTier_when_it_is_set()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"serviceTier\":\"flex\"}}");
        GoogleUpstream.JsonEqual(Prep("gemini-2.5-flash", options).Body["serviceTier"]!, "\"flex\"");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should not send serviceTier in request body when not specified", Coverage = UpstreamCoverage.Covered)]
    public void Omits_serviceTier_when_it_is_unset()
    {
        Assert.False(Prep("gemini-2.5-flash", Hello()).Body.ContainsKey("serviceTier"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should send sharedRequestType as X-Vertex-AI-LLM-Shared-Request-Type header on Vertex", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_Vertex_shared_request_type_header()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"sharedRequestType\":\"flex\"}}");
        var prepared = Prep("gemini-2.5-flash", options, "google.vertex.chat");
        Assert.Equal("flex", prepared.ExtraHeaders["X-Vertex-AI-LLM-Shared-Request-Type"]);
        Assert.False(prepared.Body.ContainsKey("serviceTier"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should send requestType as X-Vertex-AI-LLM-Request-Type header on Vertex", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_Vertex_request_type_header()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"sharedRequestType\":\"priority\",\"requestType\":\"shared\"}}");
        var prepared = Prep("gemini-2.5-flash", options, "google.vertex.chat");
        Assert.Equal("priority", prepared.ExtraHeaders["X-Vertex-AI-LLM-Shared-Request-Type"]);
        Assert.Equal("shared", prepared.ExtraHeaders["X-Vertex-AI-LLM-Request-Type"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should warn and drop serviceTier on Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public void Drops_serviceTier_on_Vertex_and_warns()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"serviceTier\":\"flex\"}}");
        var prepared = Prep("gemini-2.5-flash", options, "google.vertex.chat");
        Assert.False(prepared.Body.ContainsKey("serviceTier"));
        Assert.Contains(prepared.Warnings, warning => warning.Message.Contains("serviceTier") && warning.Message.Contains("Vertex"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should warn when sharedRequestType is set on a non-Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public void Warns_when_sharedRequestType_is_set_off_Vertex()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"sharedRequestType\":\"flex\"}}");
        var prepared = Prep("gemini-2.5-flash", options);
        Assert.Empty(prepared.ExtraHeaders);
        Assert.Contains(prepared.Warnings, warning => warning.Message.Contains("sharedRequestType"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should omit unsupported %s for %s", Coverage = UpstreamCoverage.Covered)]
    public void Omits_frequency_and_presence_penalties_on_Gemini_2_5()
    {
        foreach (var model in new[] { "gemini-2.5-pro", "gemini-2.5-flash", "gemini-2.5-flash-lite" })
        {
            foreach (var penalty in new[] { "frequencyPenalty", "presencePenalty" })
            {
                var options = Hello();
                if (penalty == "frequencyPenalty")
                {
                    options.FrequencyPenalty = 0.5;
                }
                else
                {
                    options.PresencePenalty = 0.5;
                }

                var prepared = Prep(model, options);
                Assert.False(prepared.Body["generationConfig"]!.AsObject().ContainsKey(penalty));
                Assert.Contains(prepared.Warnings, warning => warning.Type == "unsupported" && warning.Feature == penalty);
            }
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass penalty settings for Gemini 2.0 models", Coverage = UpstreamCoverage.Covered)]
    public void Sends_penalties_for_Gemini_2_0()
    {
        var options = Hello();
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;
        var prepared = Prep("gemini-2.0-pro", options);
        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!, "{\"frequencyPenalty\":0.5,\"presencePenalty\":0.5}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass penalty settings for Vertex Gemini 2.5 models", Coverage = UpstreamCoverage.Covered)]
    public void Sends_penalties_for_Vertex_Gemini_2_5()
    {
        var options = Hello();
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;
        var prepared = Prep("gemini-2.5-flash", options, "google.vertex.chat");
        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!, "{\"frequencyPenalty\":0.5,\"presencePenalty\":0.5}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should only pass valid provider options", Coverage = UpstreamCoverage.Covered)]
    public void Passes_response_modalities_and_drops_unknown_provider_options()
    {
        var options = Hello();
        options.Temperature = 0.2;
        options.Seed = 7;
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"responseModalities\":[\"TEXT\",\"IMAGE\"],\"foo\":\"bar\"}}");
        GoogleUpstream.JsonEqual(Prep("gemini-pro", options).Body["generationConfig"]!, "{\"temperature\":0.2,\"seed\":7,\"responseModalities\":[\"TEXT\",\"IMAGE\"]}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should expand standalone threshold provider option into safetySettings", Coverage = UpstreamCoverage.Covered)]
    public void Expands_a_threshold_into_safety_settings()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"threshold\":\"BLOCK_NONE\"}}");
        GoogleUpstream.JsonEqual(Prep("gemini-pro", options).Body["safetySettings"]!, "[{\"category\":\"HARM_CATEGORY_HATE_SPEECH\",\"threshold\":\"BLOCK_NONE\"},{\"category\":\"HARM_CATEGORY_DANGEROUS_CONTENT\",\"threshold\":\"BLOCK_NONE\"},{\"category\":\"HARM_CATEGORY_HARASSMENT\",\"threshold\":\"BLOCK_NONE\"},{\"category\":\"HARM_CATEGORY_SEXUALLY_EXPLICIT\",\"threshold\":\"BLOCK_NONE\"}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should let safetySettings take precedence over standalone threshold provider option", Coverage = UpstreamCoverage.Covered)]
    public void Lets_explicit_safety_settings_win_over_threshold()
    {
        var options = Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"threshold\":\"BLOCK_NONE\",\"safetySettings\":[{\"category\":\"HARM_CATEGORY_HATE_SPEECH\",\"threshold\":\"BLOCK_LOW_AND_ABOVE\"}]}}");
        GoogleUpstream.JsonEqual(Prep("gemini-pro", options).Body["safetySettings"]!, "[{\"category\":\"HARM_CATEGORY_HATE_SPEECH\",\"threshold\":\"BLOCK_LOW_AND_ABOVE\"}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_named_tool_choice()
    {
        var options = Hello();
        options.Tools = new[] { GoogleUpstream.Function("test-tool", "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}") };
        options.ToolChoice = ToolChoice.Tool("test-tool");
        var body = Prep("gemini-pro", options).Body;
        GoogleUpstream.JsonEqual(body["toolConfig"]!, "{\"functionCallingConfig\":{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"test-tool\"]}}");
        GoogleUpstream.JsonEqual(body["tools"]!, "[{\"functionDeclarations\":[{\"name\":\"test-tool\",\"description\":\"\",\"parametersJsonSchema\":{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass tools and toolChoice #2", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_required_tool_choice()
    {
        var options = Hello();
        options.Tools = new[] { GoogleUpstream.Function("test-tool", "{\"type\":\"object\",\"properties\":{\"property1\":{\"type\":\"string\"},\"property2\":{\"type\":\"number\"}},\"required\":[\"property1\",\"property2\"],\"additionalProperties\":false}") };
        options.ToolChoice = ToolChoice.Required;
        var body = Prep("gemini-pro", options).Body;
        GoogleUpstream.JsonEqual(body["toolConfig"]!, "{\"functionCallingConfig\":{\"mode\":\"ANY\"}}");
        Assert.Equal("test-tool", (string?)body["tools"]![0]!["functionDeclarations"]![0]!["name"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should set response mime type with responseFormat", Coverage = UpstreamCoverage.Covered)]
    public void Sets_the_response_mime_type_for_a_json_schema()
    {
        var options = Hello();
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}");
        var config = Prep("gemini-pro", options).Body["generationConfig"]!.AsObject();
        Assert.Equal("application/json", (string?)config["responseMimeType"]);
        GoogleUpstream.JsonEqual(config["responseJsonSchema"]!, "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass array length constraints in response schemas", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_array_length_constraints_in_response_schemas()
    {
        var options = Hello();
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"array\",\"items\":{\"type\":\"string\"},\"minItems\":1,\"maxItems\":3}");
        GoogleUpstream.JsonEqual(Prep("gemini-pro", options).Body["generationConfig"]!["responseJsonSchema"]!, "{\"type\":\"array\",\"items\":{\"type\":\"string\"},\"minItems\":1,\"maxItems\":3}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should preserve local JSON Schema references in response schemas", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_local_schema_references_in_response_schemas()
    {
        var options = Hello();
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"item\":{\"$ref\":\"#/$defs/item\"}},\"$defs\":{\"item\":{\"type\":\"string\"}}}");
        GoogleUpstream.JsonEqual(Prep("gemini-pro", options).Body["generationConfig"]!["responseJsonSchema"]!, "{\"type\":\"object\",\"properties\":{\"item\":{\"$ref\":\"#/$defs/item\"}},\"$defs\":{\"item\":{\"type\":\"string\"}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass specification with responseFormat and structuredOutputs = true (default)", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_response_schema_by_default()
    {
        var options = Hello();
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"property1\":{\"type\":\"string\"}}}");
        Assert.NotNull(Prep("gemini-pro", options).Body["generationConfig"]!["responseJsonSchema"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should not pass specification with responseFormat and structuredOutputs = false", Coverage = UpstreamCoverage.Covered)]
    public void Omits_the_response_schema_when_structured_outputs_are_disabled()
    {
        var options = Hello();
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"property1\":{\"type\":\"string\"},\"property2\":{\"type\":\"number\"}},\"required\":[\"property1\",\"property2\"],\"additionalProperties\":false}");
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"structuredOutputs\":false}}");
        var config = Prep("gemini-pro", options).Body["generationConfig"]!.AsObject();
        Assert.Equal("application/json", (string?)config["responseMimeType"]);
        Assert.False(config.ContainsKey("responseJsonSchema"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should map reasoning \"minimal\" to thinkingLevel \"minimal\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_reasoning_minimal_to_thinkingLevel_minimal()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3-pro-preview", "minimal"), "{\"thinkingLevel\":\"minimal\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should map reasoning \"low\" to thinkingLevel \"low\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_reasoning_low_to_thinkingLevel_low()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3-pro-preview", "low"), "{\"thinkingLevel\":\"low\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should map reasoning \"medium\" to thinkingLevel \"medium\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_reasoning_medium_to_thinkingLevel_medium()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3-pro-preview", "medium"), "{\"thinkingLevel\":\"medium\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should map reasoning \"high\" to thinkingLevel \"high\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_reasoning_high_to_thinkingLevel_high()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3-pro-preview", "high"), "{\"thinkingLevel\":\"high\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should map reasoning \"none\" to thinkingLevel \"minimal\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_reasoning_none_to_thinkingLevel_minimal()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3-pro-preview", "none"), "{\"thinkingLevel\":\"minimal\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should coerce reasoning \"xhigh\" to \"high\" with compatibility warning", Coverage = UpstreamCoverage.Covered)]
    public void Coerces_xhigh_reasoning_to_high()
    {
        var options = Hello();
        options.Reasoning = "xhigh";
        var prepared = Prep("gemini-3-pro-preview", options);
        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!["thinkingConfig"]!, "{\"thinkingLevel\":\"high\"}");
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility" && warning.Feature == "reasoning" && warning.Details == "reasoning \"xhigh\" is not directly supported by this model. mapped to effort \"high\".");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should coerce reasoning \"minimal\" to thinkingLevel \"low\" for Gemini 3.7 Flash", Coverage = UpstreamCoverage.Covered)]
    public void Coerces_minimal_reasoning_to_low_for_Gemini_3_7_Flash()
    {
        var options = Hello();
        options.Reasoning = "minimal";
        var prepared = Prep("gemini-3.7-flash", options);
        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!["thinkingConfig"]!, "{\"thinkingLevel\":\"low\"}");
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility" && warning.Feature == "reasoning" && warning.Details == "reasoning \"minimal\" is not directly supported by this model. mapped to effort \"low\".");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should coerce reasoning \"none\" to thinkingLevel \"low\" for Gemini 3.7 Flash", Coverage = UpstreamCoverage.Covered)]
    public void Coerces_none_reasoning_to_low_for_Gemini_3_7_Flash()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3.7-flash", "none"), "{\"thinkingLevel\":\"low\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should map reasoning \"$reasoning\" to thinkingLevel \"$expectedThinkingLevel\" for $modelId", Coverage = UpstreamCoverage.Covered)]
    public void Maps_minimum_thinking_levels_for_flash_model_ids()
    {
        var cases = new (string Model, string Reasoning, string Level)[]
        {
            ("gemini-3.7-flash-video-understanding-eap", "minimal", "low"),
            ("gemini-3.7-flash-video-understanding-eap", "none", "low"),
            ("gemini-flash-latest", "minimal", "low"),
            ("gemini-flash-latest", "none", "low"),
            ("models/gemini-3.7-flash", "minimal", "low"),
            ("gemini-3.8-flash", "minimal", "low"),
            ("gemini-3.10-flash-preview", "minimal", "low"),
            ("gemini-4.0-flash", "minimal", "low"),
            ("gemini-3-flash-preview", "minimal", "minimal"),
            ("gemini-3.6-flash", "minimal", "minimal"),
            ("gemini-3.7-flash-lite", "minimal", "minimal"),
            ("gemini-3.10-flash-lite-preview", "minimal", "minimal"),
            ("gemini-flash-lite-latest", "minimal", "minimal"),
        };
        foreach (var item in cases)
        {
            GoogleUpstream.JsonEqual(Thinking(item.Model, item.Reasoning), "{\"thinkingLevel\":\"" + item.Level + "\"}");
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 3 models (thinkingLevel)::should also detect gemini-3.1 models as Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Maps_Gemini_3_1_medium_reasoning_to_thinkingLevel()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-3.1-pro-preview", "medium"), "{\"thinkingLevel\":\"medium\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should map reasoning \"none\" to thinkingBudget 0", Coverage = UpstreamCoverage.Covered)]
    public void Maps_none_reasoning_to_a_zero_thinking_budget()
    {
        GoogleUpstream.JsonEqual(Thinking("gemini-2.5-pro", "none"), "{\"thinkingBudget\":0}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should map reasoning \"minimal\" to ~2% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public void Maps_minimal_reasoning_to_two_percent_of_the_budget()
    {
        Assert.Equal(1311, Thinking("gemini-2.5-pro", "minimal")!["thinkingBudget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should map reasoning \"low\" to ~10% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public void Maps_low_reasoning_to_ten_percent_of_the_budget()
    {
        Assert.Equal(6554, Thinking("gemini-2.5-pro", "low")!["thinkingBudget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should map reasoning \"medium\" to ~30% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public void Maps_medium_reasoning_to_thirty_percent_of_the_budget()
    {
        Assert.Equal(19661, Thinking("gemini-2.5-pro", "medium")!["thinkingBudget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should map reasoning \"high\" to ~60% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public void Clamps_high_reasoning_to_the_pro_thinking_budget()
    {
        Assert.Equal(32768, Thinking("gemini-2.5-pro", "high")!["thinkingBudget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should map reasoning \"xhigh\" to ~90% of maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public void Clamps_xhigh_reasoning_to_the_pro_thinking_budget()
    {
        Assert.Equal(32768, Thinking("gemini-2.5-pro", "xhigh")!["thinkingBudget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 2.5 models (thinkingBudget)::should use lower maxOutputTokens for flash-lite models", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_the_flash_lite_medium_budget_unclamped()
    {
        Assert.Equal(19661, Thinking("gemini-2.5-flash-lite", "medium")!["thinkingBudget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > providerOptions precedence::should use providerOptions thinkingConfig when both reasoning and providerOptions are set", Coverage = UpstreamCoverage.Covered)]
    public void Lets_provider_thinkingConfig_replace_the_resolved_budget()
    {
        var options = Hello();
        options.Reasoning = "high";
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thinkingConfig\":{\"thinkingBudget\":999}}}");
        var thinking = Prep("gemini-pro", options).Body["generationConfig"]!["thinkingConfig"]!.AsObject();
        Assert.Equal(999, thinking["thinkingBudget"]!.GetValue<int>());
        Assert.False(thinking.ContainsKey("thinkingLevel"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > providerOptions precedence::should not set thinkingConfig when neither reasoning nor providerOptions are set", Coverage = UpstreamCoverage.Covered)]
    public void Omits_thinkingConfig_when_reasoning_is_unset()
    {
        Assert.False(Prep("gemini-2.5-pro", Hello()).Body["generationConfig"]!.AsObject().ContainsKey("thinkingConfig"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > providerOptions precedence::should not set thinkingConfig when reasoning is \"provider-default\"", Coverage = UpstreamCoverage.Covered)]
    public void Omits_thinkingConfig_for_provider_default_reasoning()
    {
        var options = Hello();
        options.Reasoning = "provider-default";
        Assert.False(Prep("gemini-2.5-pro", options).Body["generationConfig"]!.AsObject().ContainsKey("thinkingConfig"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::GEMMA Model System Instruction Fix::should NOT send systemInstruction for GEMMA-3-12b-it model", Coverage = UpstreamCoverage.Covered)]
    public void Omits_systemInstruction_for_Gemma_3_12b()
    {
        var options = GoogleUpstream.Prompt(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"));
        Assert.False(Prep("gemma-3-12b-it", options).Body.ContainsKey("systemInstruction"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::GEMMA Model System Instruction Fix::should NOT send systemInstruction for GEMMA-3-27b-it model", Coverage = UpstreamCoverage.Covered)]
    public void Omits_systemInstruction_for_Gemma_3_27b()
    {
        var options = GoogleUpstream.Prompt(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"));
        Assert.False(Prep("GEMMA-3-27b-it", options).Body.ContainsKey("systemInstruction"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::GEMMA Model System Instruction Fix::should still send systemInstruction for Gemini models (regression test)", Coverage = UpstreamCoverage.Covered)]
    public void Still_sends_systemInstruction_for_Gemini()
    {
        var options = GoogleUpstream.Prompt(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"));
        GoogleUpstream.JsonEqual(Prep("gemini-2.5-flash", options).Body["systemInstruction"]!, "{\"parts\":[{\"text\":\"You are a helpful assistant.\"}]}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::GEMMA Model System Instruction Fix::should NOT generate warning when GEMMA model is used without system instructions", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_warn_for_Gemma_without_a_system_instruction()
    {
        Assert.Empty(Prep("gemma-3-12b-it", Hello()).Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::GEMMA Model System Instruction Fix::should NOT generate warning when Gemini model is used with system instructions", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_warn_for_Gemini_with_a_system_instruction()
    {
        var options = GoogleUpstream.Prompt(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"));
        Assert.Empty(Prep("gemini-2.5-flash", options).Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::GEMMA Model System Instruction Fix::should prepend system instruction to first user message for GEMMA models", Coverage = UpstreamCoverage.Covered)]
    public void Prepends_the_system_instruction_in_the_Gemma_request_body()
    {
        var options = GoogleUpstream.Prompt(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"));
        var body = Prep("gemma-3-12b-it", options).Body;
        Assert.False(body.ContainsKey("systemInstruction"));
        GoogleUpstream.JsonEqual(body["contents"]!, "[{\"role\":\"user\",\"parts\":[{\"text\":\"You are a helpful assistant.\\n\\n\"},{\"text\":\"Hello\"}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > text::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_text_and_its_thought_signature()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"There are **3** r's in strawberry.\\n\\nHere is the breakdown: st**r**awbe**rr**y.\",\"thoughtSignature\":\"EtoFCtcFAb4+9vtfe4MXRxQjw48U1WKrR/7lYsgFkVi/bepqsSPjY0VU7HEzkeCBIfy1fu5t9aUZ4IZ65aWagqbBrV45fc97olcg\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":9,\"candidatesTokenCount\":28,\"totalTokenCount\":281,\"promptTokensDetails\":[{\"modality\":\"TEXT\",\"tokenCount\":9}],\"thoughtsTokenCount\":244},\"modelVersion\":\"gemini-3-pro-preview\",\"responseId\":\"Un6LacrVMcjUxs0PmJfWoQc\"}", generateId: () => "test-id");
        var text = Assert.IsType<GoogleText>(result.Content[0]);
        Assert.Equal("There are **3** r's in strawberry.\n\nHere is the breakdown: st**r**awbe**rr**y.", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("EtoFCtcFAb4+9vtfe4MXRxQjw48U1WKrR/7lYsgFkVi/bepqsSPjY0VU7HEzkeCBIfy1fu5t9aUZ4IZ65aWagqbBrV45fc97olcg", text.ProviderMetadata!.Value.GetProperty("google").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > text::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_prompt_candidate_and_thought_tokens()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"There are **3** r's in strawberry.\\n\\nHere is the breakdown: st**r**awbe**rr**y.\",\"thoughtSignature\":\"EtoFCtcFAb4+9vtfe4MXRxQjw48U1WKrR/7lYsgFkVi/bepqsSPjY0VU7HEzkeCBIfy1fu5t9aUZ4IZ65aWagqbBrV45fc97olcg\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":9,\"candidatesTokenCount\":28,\"totalTokenCount\":281,\"promptTokensDetails\":[{\"modality\":\"TEXT\",\"tokenCount\":9}],\"thoughtsTokenCount\":244},\"modelVersion\":\"gemini-3-pro-preview\",\"responseId\":\"Un6LacrVMcjUxs0PmJfWoQc\"}");
        Assert.Equal(9, result.Usage.InputTokens);
        Assert.Equal(272, result.Usage.OutputTokens);
        Assert.Equal(244, result.Usage.ReasoningTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > text::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_the_response_model_id_and_timestamp_unset()
    {
        var handler = new RecordingHandler { ResponseText = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"There are **3** r's in strawberry.\\n\\nHere is the breakdown: st**r**awbe**rr**y.\",\"thoughtSignature\":\"EtoFCtcFAb4+9vtfe4MXRxQjw48U1WKrR/7lYsgFkVi/bepqsSPjY0VU7HEzkeCBIfy1fu5t9aUZ4IZ65aWagqbBrV45fc97olcg\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":9,\"candidatesTokenCount\":28,\"totalTokenCount\":281,\"promptTokensDetails\":[{\"modality\":\"TEXT\",\"tokenCount\":9}],\"thoughtsTokenCount\":244},\"modelVersion\":\"gemini-3-pro-preview\",\"responseId\":\"Un6LacrVMcjUxs0PmJfWoQc\"}" };
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler);
        var result = await provider.LanguageModel("gemini-3-pro-preview").DoGenerateAsync(Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Un6LacrVMcjUxs0PmJfWoQc", result.ResponseId);
        Assert.Null(result.ResponseModelId);
        Assert.Null(result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > tool-call::should extract tool calls", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_a_function_call_and_maps_stop_to_tool_calls()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"weather\",\"args\":{\"location\":\"San Francisco\"}},\"thoughtSignature\":\"EskgCsYgAb4+9vtF7/499YQS2bjZs3xcQI+iAl+ILn29nK1j0Kg6su7QsUUUk3nrAAfnS2w5WiVvlcCqu9fAebJ2cvfaEyBahEt5\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0,\"finishMessage\":\"Model generated function call(s).\"}],\"usageMetadata\":{\"promptTokenCount\":29,\"candidatesTokenCount\":15,\"totalTokenCount\":937,\"promptTokensDetails\":[{\"modality\":\"TEXT\",\"tokenCount\":29}],\"thoughtsTokenCount\":893},\"modelVersion\":\"gemini-3-pro-preview\",\"responseId\":\"m36LaZGyCLz1xs0PtNSB-QU\"}", generateId: () => "test-id");
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("test-id", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        GoogleUpstream.JsonEqual(GoogleUpstream.Element(call.ArgumentsJson), "{\"location\":\"San Francisco\"}");
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("Model generated function call(s).", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("finishMessage").GetString());
        Assert.Equal("EskgCsYgAb4+9vtF7/499YQS2bjZs3xcQI+iAl+ILn29nK1j0Kg6su7QsUUUk3nrAAfnS2w5WiVvlcCqu9fAebJ2cvfaEyBahEt5", call.ProviderMetadata!.Value.GetProperty("google").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should handle MALFORMED_FUNCTION_CALL finish reason and empty content object", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_malformed_function_call_to_an_error_finish()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{},\"finishReason\":\"MALFORMED_FUNCTION_CALL\"}]}");
        Assert.Empty(result.Content);
        Assert.Equal(FinishReason.Error, result.FinishReason);
        Assert.Equal("MALFORMED_FUNCTION_CALL", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should expose finishMessage in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_finishMessage_in_provider_metadata()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\",\"finishMessage\":\"Model generated function call(s).\"}]}");
        Assert.Equal("Model generated function call(s).", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("finishMessage").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should expose null finishMessage in provider metadata when not present", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_null_finishMessage_in_provider_metadata()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}");
        Assert.Equal(JsonValueKind.Null, result.ProviderMetadata!.Value.GetProperty("google").GetProperty("finishMessage").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should read serviceTier from usageMetadata.serviceTier", Coverage = UpstreamCoverage.Covered)]
    public void Reads_serviceTier_from_usage_metadata()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}],\"usageMetadata\":{\"promptTokenCount\":1,\"serviceTier\":\"flex\"}}");
        Assert.Equal("flex", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("serviceTier").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should expose null serviceTier in provider metadata when not present", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_null_serviceTier_in_provider_metadata()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}],\"usageMetadata\":{\"promptTokenCount\":1}}");
        Assert.Equal(JsonValueKind.Null, result.ProviderMetadata!.Value.GetProperty("google").GetProperty("serviceTier").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > no-args tool call (unary)::should extract a no-args tool call with thoughtSignature as input \"{}\"", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_a_no_args_tool_call_as_an_empty_object()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"read_theme\"},\"thoughtSignature\":\"sig-no-args-unary\"}]},\"finishReason\":\"STOP\"}]}", generateId: () => "test-id");
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("test-id", call.ToolCallId);
        Assert.Equal("{}", call.ArgumentsJson);
        Assert.Equal("sig-no-args-unary", call.ProviderMetadata!.Value.GetProperty("google").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > no-args tool call (unary)::should generate an ID when the function call ID is empty", Coverage = UpstreamCoverage.Covered)]
    public void Generates_an_id_when_the_function_call_id_is_empty()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"read_theme\",\"id\":\"\",\"args\":{}}}]},\"finishReason\":\"STOP\"}]}", generateId: () => "test-id");
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("test-id", call.ToolCallId);
        Assert.Null(call.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should surface prompt-level blocks without candidates", Coverage = UpstreamCoverage.Covered)]
    public void Classifies_a_confirmed_prompt_block_as_a_content_filter()
    {
        var result = GoogleUpstream.Parse("{\"promptFeedback\":{\"blockReason\":\"PROHIBITED_CONTENT\"},\"responseId\":\"blocked-response-id\"}");
        Assert.Empty(result.Content);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Equal("PROHIBITED_CONTENT", result.RawFinishReason);
        Assert.Equal("blocked-response-id", result.ResponseId);
        Assert.Equal(JsonValueKind.Null, result.ProviderMetadata!.Value.GetProperty("google").GetProperty("finishMessage").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should not classify the default prompt block reason %j as a content filter", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_classify_unspecified_prompt_block_reasons_as_content_filters()
    {
        foreach (var reason in new[] { "", "BLOCK_REASON_UNSPECIFIED", "BLOCKED_REASON_UNSPECIFIED" })
        {
            var json = reason.Length == 0
                ? "{\"promptFeedback\":{}}"
                : "{\"promptFeedback\":{\"blockReason\":\"" + reason + "\"}}";
            var result = GoogleUpstream.Parse(json);
            Assert.Equal(FinishReason.Other, result.FinishReason);
            Assert.Null(result.RawFinishReason);
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should return stop finish reason for code execution (provider-executed tool)", Coverage = UpstreamCoverage.Covered)]
    public void Returns_stop_for_provider_executed_code_execution()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"executableCode\":{\"language\":\"PYTHON\",\"code\":\"print(1)\"}},{\"codeExecutionResult\":{\"outcome\":\"OUTCOME_OK\",\"output\":\"1\"}}]},\"finishReason\":\"STOP\"}]}", generateId: () => "test-id");
        var call = Assert.IsType<GoogleToolCall>(result.Content[0]);
        Assert.True(call.ProviderExecuted);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > providerMetadata key based on provider string::should use \"vertex\" as providerMetadata key when provider includes \"vertex\"", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_vertex_metadata_key_when_the_provider_includes_vertex()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\",\"finishMessage\":\"done\"}]}", "google.vertex.chat");
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("vertex", out var vertex));
        Assert.Equal("done", vertex.GetProperty("finishMessage").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate > providerMetadata key based on provider string::should use \"google\" as providerMetadata key when provider does not include \"vertex\"", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_google_metadata_key_for_the_developer_api()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\",\"finishMessage\":\"done\"}]}", "google.generative-ai");
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("google", out var google));
        Assert.False(result.ProviderMetadata!.Value.TryGetProperty("vertex", out _));
        Assert.Equal("done", google.GetProperty("finishMessage").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should omit function call IDs from Vertex requests", Coverage = UpstreamCoverage.Covered)]
    public void Omits_function_call_ids_on_Vertex()
    {
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("Hello"),
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("call1", "weather", "{\"location\":\"SF\"}") }, null));
        var body = Prep("gemini-2.5-flash", options, "google.vertex.chat").Body;
        Assert.False(body["contents"]![1]!["parts"]![0]!["functionCall"]!.AsObject().ContainsKey("id"));
    }

    private static LanguageModelCallOptions Hello()
    {
        return GoogleUpstream.Prompt(new UserModelMessage("Hello"));
    }

    private static GooglePreparedRequest Prep(string model, LanguageModelCallOptions options, string provider = "google.generative-ai", bool streaming = false)
    {
        return GoogleRequest.Prepare(model, provider, options, streaming);
    }

    private static JsonNode Thinking(string model, string reasoning, string provider = "google.generative-ai")
    {
        var options = Hello();
        options.Reasoning = reasoning;
        return Prep(model, options, provider).Body["generationConfig"]!["thinkingConfig"]!;
    }

    private static GooglePrompt Convert(GoogleMessageOptions options, params GoogleTurn[] turns)
    {
        return GoogleMessages.Convert(turns, options);
    }
}
