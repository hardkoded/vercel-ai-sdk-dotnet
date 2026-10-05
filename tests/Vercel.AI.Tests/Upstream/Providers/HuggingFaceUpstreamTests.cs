// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.HuggingFace;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class HuggingFaceUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/huggingface/src/huggingface-provider.test.ts::HuggingFaceProvider > createHuggingFace::should create provider with default configuration", Coverage = UpstreamCoverage.Covered)]
    public void The_default_provider_exposes_responses_and_chat()
    {
        var provider = HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.Equal("huggingface.responses", provider.ResponsesModel("model").Provider);
        Assert.Equal("huggingface.chat", provider.LanguageModel("model").Provider);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/huggingface-provider.test.ts::HuggingFaceProvider > createHuggingFace::should create provider with custom settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Custom_settings_are_kept()
    {
        var capture = new UpstreamCapture();
        var provider = HuggingFaceProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.url" },
            capture);
        provider.Options.Headers["Custom-Header"] = "test";
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("model")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://custom.url/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer custom-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("test", capture.Requests[0].Headers["Custom-Header"]);
        Assert.Equal("huggingface.responses", provider.ResponsesModel("model").Provider);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/huggingface-provider.test.ts::HuggingFaceProvider > model creation methods::should expose responses method", Coverage = UpstreamCoverage.Covered)]
    public void Responses_returns_a_responses_model()
    {
        var model = HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ResponsesModel("org/model");
        Assert.Equal("huggingface.responses", model.Provider);
        Assert.Equal("org/model", model.ModelId);
        Assert.Equal("V4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/huggingface-provider.test.ts::HuggingFaceProvider > model creation methods::should expose languageModel method", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_stays_on_chat_completions()
    {
        var model = HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("org/model");
        Assert.Equal("huggingface.chat", model.Provider);
        Assert.Equal("org/model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/huggingface-provider.test.ts::HuggingFaceProvider > unsupported functionality::should throw for text embedding models", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_use_the_responses_api_message()
    {
        var error = Assert.Throws<AiSdkException>(() => HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("any-model"));
        Assert.Equal("Hugging Face Responses API does not support text embeddings. Use the Hugging Face Inference API directly for embeddings.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/huggingface-provider.test.ts::HuggingFaceProvider > unsupported functionality::should throw for image models", Coverage = UpstreamCoverage.Covered)]
    public void Images_use_the_responses_api_message()
    {
        var error = Assert.Throws<AiSdkException>(() => HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ImageModel("any-model"));
        Assert.Equal("Hugging Face Responses API does not support image generation. Use the Hugging Face Inference API directly for image models.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should generate text", Coverage = UpstreamCoverage.Partial, Note = "The answer text is returned. GeneratedText has no provider metadata, so itemId is not attached.")]
    public async Task Generate_returns_output_text()
    {
        var result = await Generate("{\"output\":[{\"id\":\"msg_67c97c02656c81908e080dfdf4a03cd1\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello! How can I help you today?\"}]}],\"output_text\":\"Hello! How can I help you today?\"}");
        Assert.Equal("Hello! How can I help you today?", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should extract usage", Coverage = UpstreamCoverage.Partial, Note = "Input, output, total, cache read, and reasoning tokens are mapped. noCache stays null because cache write is unset.")]
    public async Task Usage_maps_input_output_and_total()
    {
        var result = await Generate("{\"usage\":{\"input_tokens\":12,\"output_tokens\":25,\"total_tokens\":37},\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello\"}]}]}");
        Assert.Equal(12, result.Usage.InputTokens);
        Assert.Equal(25, result.Usage.OutputTokens);
        Assert.Equal(37, result.Usage.TotalTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
        Assert.Equal(25, result.Usage.TextTokens);
        Assert.Null(result.Usage.NoCacheInputTokens);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should extract text from output array when output_text is missing", Coverage = UpstreamCoverage.Partial, Note = "Text is read from output content. GeneratedText has no provider metadata, so itemId is not attached.")]
    public async Task Text_is_read_from_the_output_array()
    {
        var result = await Generate("{\"output_text\":null,\"output\":[{\"id\":\"msg_test\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Extracted from output array\"}]}]}");
        Assert.Equal("Extracted from output array", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should handle missing usage gracefully", Coverage = UpstreamCoverage.Partial, Note = "Missing usage leaves input and output null. Total becomes 0 because LanguageModelUsage fills a null total.")]
    public async Task Missing_usage_leaves_token_counts_unset()
    {
        var result = await Generate("{\"usage\":null,\"output\":[],\"output_text\":\"Test response\"}");
        Assert.Null(result.Usage.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Null(result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.ReasoningTokens);
        Assert.Equal(0, result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should send model id, settings, and input", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_model_settings_and_input()
    {
        var capture = new UpstreamCapture();
        var model = Responses(capture);
        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new SystemModelMessage("You are a helpful assistant."),
                    new UserModelMessage("Hello"),
                },
                Temperature = 0.5,
                TopP = 0.3,
                MaxOutputTokens = 100,
            },
            CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("https://router.huggingface.co/v1/responses", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("deepseek-ai/DeepSeek-V3-0324", body["model"]!.GetValue<string>());
        Assert.Equal(0.5, body["temperature"]!.GetValue<double>(), 5);
        Assert.Equal(0.3, body["top_p"]!.GetValue<double>(), 5);
        Assert.Equal(100, body["max_output_tokens"]!.GetValue<int>());
        Assert.False(body["stream"]!.GetValue<bool>());
        Assert.Equal("system", body["input"]![0]!["role"]!.GetValue<string>());
        Assert.Equal("You are a helpful assistant.", body["input"]![0]!["content"]!.GetValue<string>());
        Assert.Equal("user", body["input"]![1]!["role"]!.GetValue<string>());
        Assert.Equal("input_text", body["input"]![1]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("Hello", body["input"]![1]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should handle unsupported settings with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Unsupported_settings_warn()
    {
        var capture = new UpstreamCapture();
        var result = await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                TopK = 10,
                Seed = 123,
                PresencePenalty = 0.5,
                FrequencyPenalty = 0.3,
                StopSequences = new[] { "stop" },
            },
            CancellationToken.None);
        Assert.Equal(
            new[] { "topK", "seed", "presencePenalty", "frequencyPenalty", "stopSequences" },
            result.Warnings.Select(warning => warning.Message).ToArray());
        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should generate text and sources from annotations", Coverage = UpstreamCoverage.Partial, Note = "Citation URL and title are returned. Source ids are source-N, and text parts do not carry itemId.")]
    public async Task Annotations_become_sources()
    {
        var result = await Generate("{\"output\":[{\"id\":\"msg_test_annotations\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Cited.\",\"annotations\":[{\"type\":\"url_citation\",\"url\":\"https://example.com/article1\",\"title\":\"AI Developments Article\"}]}]}]}");
        Assert.Equal("Cited.", result.Text);
        var source = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.Equal("https://example.com/article1", source.Url);
        Assert.Equal("AI Developments Article", source.Title);
        Assert.Equal("source-0", source.Id);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > message conversion::should convert user messages with images", Coverage = UpstreamCoverage.Covered)]
    public async Task User_images_are_sent_as_input_image()
    {
        var capture = new UpstreamCapture();
        await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage(new UserContentPart[]
                    {
                        new TextContentPart("What do you see?"),
                        new FileContentPart("image/jpeg", null, Convert.FromBase64String("AQIDBA=="), null),
                    }),
                },
            },
            CancellationToken.None);
        var content = JsonNode.Parse(capture.Requests[0].Body)!["input"]![0]!["content"]!;
        Assert.Equal("input_text", content[0]!["type"]!.GetValue<string>());
        Assert.Equal("What do you see?", content[0]!["text"]!.GetValue<string>());
        Assert.Equal("input_image", content[1]!["type"]!.GetValue<string>());
        Assert.Equal("data:image/jpeg;base64,AQIDBA==", content[1]!["image_url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > message conversion::should throw for file parts with provider references", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_file_references_are_rejected()
    {
        var capture = new UpstreamCapture();
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Responses(capture, "Qwen/Qwen2.5-VL-32B-Instruct").DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage(new UserContentPart[] { new FileContentPart("image/jpeg", null, null, null) }),
                },
            },
            CancellationToken.None));
        Assert.Equal("'file parts with provider references' functionality not supported.", error.Message);
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > message conversion::should handle assistant messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Assistant_text_is_output_text()
    {
        var capture = new UpstreamCapture();
        await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage("Hello"),
                    new AssistantModelMessage("Hi there!", null, null),
                    new UserModelMessage("How are you?"),
                },
            },
            CancellationToken.None);
        var input = JsonNode.Parse(capture.Requests[0].Body)!["input"]!;
        Assert.Equal("user", input[0]!["role"]!.GetValue<string>());
        Assert.Equal("Hello", input[0]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("assistant", input[1]!["role"]!.GetValue<string>());
        Assert.Equal("output_text", input[1]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("Hi there!", input[1]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("How are you?", input[2]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > message conversion::should warn about unsupported assistant content types", Coverage = UpstreamCoverage.Covered)]
    public async Task Assistant_tool_calls_and_reasoning_do_not_warn()
    {
        var capture = new UpstreamCapture();
        var result = await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new AssistantModelMessage(
                        null,
                        new[] { new GeneratedToolCall("test", "test", "{}") },
                        "thinking..."),
                },
            },
            CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > message conversion::should warn about tool messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_messages_warn()
    {
        var capture = new UpstreamCapture();
        var result = await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new ToolModelMessage("test", "test", "\"test\"", false) },
            },
            CancellationToken.None);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("tool messages", warning.Message);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > tool calls::should handle function_call tool responses", Coverage = UpstreamCoverage.Partial, Note = "The function call and following text are returned. Provider-executed tool results are not a generated content type, and text has no itemId.")]
    public async Task Function_calls_become_tool_calls()
    {
        var result = await Generate("{\"output\":[{\"type\":\"function_call\",\"call_id\":\"call_123\",\"name\":\"getWeather\",\"arguments\":\"{\\\"location\\\": \\\"New York\\\"}\",\"output\":\"{\\\"temperature\\\": \\\"72°F\\\"}\"},{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"The weather in New York is 72°F and sunny.\"}]}]}");
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("call_123", call.ToolCallId);
        Assert.Equal("getWeather", call.ToolName);
        Assert.Equal("{\"location\": \"New York\"}", call.ArgumentsJson);
        Assert.Equal("The weather in New York is 72°F and sunny.", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > structured output::should send text.format for structured output", Coverage = UpstreamCoverage.Covered)]
    public async Task Structured_output_uses_text_format()
    {
        var capture = new UpstreamCapture();
        await Responses(capture, "moonshotai/Kimi-K2-Instruct").DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                JsonSchema = UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"}},\"required\":[\"name\",\"age\"]}"),
            },
            CancellationToken.None);
        var format = JsonNode.Parse(capture.Requests[0].Body)!["text"]!["format"]!;
        Assert.Equal("json_schema", format["type"]!.GetValue<string>());
        Assert.False(format["strict"]!.GetValue<bool>());
        Assert.Equal("response", format["name"]!.GetValue<string>());
        Assert.Equal("object", format["schema"]!["type"]!.GetValue<string>());
        Assert.Equal("string", format["schema"]!["properties"]!["name"]!["type"]!.GetValue<string>());
        Assert.Equal("age", format["schema"]!["required"]![1]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > reasoning::should handle reasoning content in responses", Coverage = UpstreamCoverage.Partial, Note = "Reasoning and answer text are separate parts. Neither carries itemId metadata.")]
    public async Task Reasoning_items_precede_the_answer()
    {
        var result = await Generate("{\"output\":[{\"id\":\"reasoning_1\",\"type\":\"reasoning\",\"content\":[{\"type\":\"reasoning_text\",\"text\":\"Let me think about this problem step by step...\"}]},{\"id\":\"msg_after_reasoning\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"The answer is 42.\"}]}]}");
        var reasoning = Assert.IsType<GeneratedReasoning>(result.Content[0]);
        var text = Assert.IsType<GeneratedText>(result.Content[1]);
        Assert.Equal("Let me think about this problem step by step...", reasoning.Text);
        Assert.Equal("The answer is 42.", text.Text);
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > provider options::should send provider-specific options", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_set_metadata_instructions_and_strict()
    {
        var capture = new UpstreamCapture();
        await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}"),
                ProviderOptions = UpstreamChat.Bag("huggingface", "{\"metadata\":{\"key\":\"value\"},\"instructions\":\"Be concise\",\"strictJsonSchema\":true}"),
            },
            CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("value", body["metadata"]!["key"]!.GetValue<string>());
        Assert.Equal("Be concise", body["instructions"]!.GetValue<string>());
        Assert.True(body["text"]!["format"]!["strict"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > tool preparation::should prepare tools correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Tools_and_a_named_choice_are_sent()
    {
        var capture = new UpstreamCapture();
        await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                Tools = new[]
                {
                    new LanguageModelTool(
                        "getWeather",
                        "Get weather information",
                        UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"]}")),
                },
                ToolChoice = ToolChoice.Tool("getWeather"),
            },
            CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("function", body["tools"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("getWeather", body["tools"]![0]!["name"]!.GetValue<string>());
        Assert.Equal("Get weather information", body["tools"]![0]!["description"]!.GetValue<string>());
        Assert.Equal("location", body["tools"]![0]!["parameters"]!["required"]![0]!.GetValue<string>());
        Assert.Equal("function", body["tool_choice"]!["type"]!.GetValue<string>());
        Assert.Equal("getWeather", body["tool_choice"]!["function"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > tool preparation::should handle auto and required tool choices", Coverage = UpstreamCoverage.Covered)]
    public async Task Auto_and_required_tool_choices_are_strings()
    {
        var capture = new UpstreamCapture();
        var model = Responses(capture);
        var tools = new[] { new LanguageModelTool("test", null, UpstreamChat.Json("{\"type\":\"object\"}")) };
        await model.DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") }, Tools = tools, ToolChoice = ToolChoice.Auto }, CancellationToken.None);
        await model.DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") }, Tools = tools, ToolChoice = ToolChoice.Required }, CancellationToken.None);
        Assert.Equal("auto", JsonNode.Parse(capture.Requests[0].Body)!["tool_choice"]!.GetValue<string>());
        Assert.Equal("required", JsonNode.Parse(capture.Requests[1].Body)!["tool_choice"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > top-level-only media type resolution::passes full image/png through unchanged for inline data", Coverage = UpstreamCoverage.Covered)]
    public async Task A_full_png_media_type_is_unchanged()
    {
        await AssertImageUrl("image/png", "data:image/png;base64,iVBORw0KGgo=");
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > top-level-only media type resolution::detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public async Task A_top_level_image_type_is_detected_as_png()
    {
        await AssertImageUrl("image", "data:image/png;base64,iVBORw0KGgo=");
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > top-level-only media type resolution::passes through URL source for top-level-only image", Coverage = UpstreamCoverage.Covered)]
    public async Task An_image_url_is_passed_through()
    {
        var capture = new UpstreamCapture();
        await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage(new UserContentPart[] { new FileContentPart("image", "https://example.com/x.png", null, null) }),
                },
            },
            CancellationToken.None);
        Assert.Equal("https://example.com/x.png", JsonNode.Parse(capture.Requests[0].Body)!["input"]![0]!["content"]![0]!["image_url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > top-level-only media type resolution::normalizes image/* wildcard via detection", Coverage = UpstreamCoverage.Covered)]
    public async Task An_image_wildcard_is_detected_as_png()
    {
        await AssertImageUrl("image/*", "data:image/png;base64,iVBORw0KGgo=");
    }

    private static async Task AssertImageUrl(string mediaType, string expected)
    {
        var capture = new UpstreamCapture();
        await Responses(capture).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage(new UserContentPart[] { new FileContentPart(mediaType, null, Convert.FromBase64String("iVBORw0KGgo="), null) }),
                },
            },
            CancellationToken.None);
        var part = JsonNode.Parse(capture.Requests[0].Body)!["input"]![0]!["content"]![0]!;
        Assert.Equal("input_image", part["type"]!.GetValue<string>());
        Assert.Equal(expected, part["image_url"]!.GetValue<string>());
    }

    private static async Task<LanguageModelGenerateResult> Generate(string body)
    {
        var capture = new UpstreamCapture { ResponseBody = body };
        return await Responses(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
    }

    private static HuggingFaceResponsesLanguageModel Responses(UpstreamCapture capture, string modelId = "deepseek-ai/DeepSeek-V3-0324")
    {
        return HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "APIKEY" }, capture).ResponsesModel(modelId);
    }
}
