// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.HuggingFace;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class HuggingFaceUpstreamTests
{
    private const string Model = "packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > ";
    private const string BasicText = "packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::";

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
    [UpstreamTest(BasicText + "should generate text", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_returns_output_text()
    {
        var result = await Generate("{\"output\":[{\"id\":\"msg_67c97c02656c81908e080dfdf4a03cd1\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello! How can I help you today?\"}]}],\"output_text\":\"Hello! How can I help you today?\"}");
        AssertText(Assert.Single(result.Content), "Hello! How can I help you today?", "msg_67c97c02656c81908e080dfdf4a03cd1");
    }

    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doGenerate > basic text response::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Usage_maps_input_output_and_total()
    {
        var result = await Generate("{\"usage\":{\"input_tokens\":12,\"output_tokens\":25,\"total_tokens\":37},\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello\"}]}]}");
        Assert.Equal(12, result.Usage.InputTokens);
        Assert.Equal(12, result.Usage.NoCacheInputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
        Assert.Equal(25, result.Usage.OutputTokens);
        Assert.Equal(25, result.Usage.TextTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
        Assert.Equal(37, result.Usage.TotalTokens);
        Assert.Equal("{\"input_tokens\":12,\"output_tokens\":25,\"total_tokens\":37}", result.Usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(BasicText + "should extract text from output array when output_text is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_is_read_from_the_output_array()
    {
        var result = await Generate("{\"output_text\":null,\"output\":[{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Extracted from output array\"}]}]}");
        AssertText(Assert.Single(result.Content), "Extracted from output array", "msg_test");
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
    [UpstreamTest(BasicText + "should generate text and sources from annotations", Coverage = UpstreamCoverage.Covered)]
    public async Task Annotations_become_sources()
    {
        const string text = "Here are some recent articles about AI: The first article discusses new developments ([example.com](https://example.com/article1)). Another piece covers industry trends ([test.com](https://test.com/article2)).";
        var result = await Generate("{\"output\":[{\"id\":\"msg_test_annotations\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"" + text + "\",\"annotations\":[{\"type\":\"url_citation\",\"url\":\"https://example.com/article1\",\"title\":\"AI Developments Article\"},{\"type\":\"url_citation\",\"url\":\"https://test.com/article2\",\"title\":\"Industry Trends Report\"}]}]}]}");
        Assert.Equal(3, result.Content.Count);
        AssertText(result.Content[0], text, "msg_test_annotations");
        AssertSource(result.Content[1], "id-0", "https://example.com/article1", "AI Developments Article");
        AssertSource(result.Content[2], "id-1", "https://test.com/article2", "Industry Trends Report");
    }

    [Fact]
    [UpstreamTest(BasicText + "should handle MCP tools with annotations", Coverage = UpstreamCoverage.Covered)]
    public async Task Mcp_calls_are_provider_executed_tool_calls()
    {
        const string text = "Based on the search results, here are the latest tech events in San Francisco: There are several AI conferences ([techevents.com](https://techevents.com/sf-ai)) and startup meetups ([eventbrite.com](https://eventbrite.com/sf-startups)) happening this week.";
        var result = await Generate("{\"output\":[{\"id\":\"mcp_search_test\",\"type\":\"mcp_call\",\"server_label\":\"web_search\",\"name\":\"search\",\"arguments\":\"{\\\"query\\\": \\\"San Francisco tech events\\\"}\",\"output\":\"Found 25 tech events in San Francisco\"},{\"id\":\"msg_mcp_response\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"" + text + "\",\"annotations\":[{\"type\":\"url_citation\",\"url\":\"https://techevents.com/sf-ai\",\"title\":\"SF AI Conference 2025\"},{\"type\":\"url_citation\",\"url\":\"https://eventbrite.com/sf-startups\",\"title\":\"SF Startup Meetups\"}]}]}]}");
        Assert.Equal(5, result.Content.Count);
        var call = Assert.IsType<HuggingFaceToolCall>(result.Content[0]);
        Assert.Equal("mcp_search_test", call.ToolCallId);
        Assert.Equal("search", call.ToolName);
        Assert.Equal("{\"query\": \"San Francisco tech events\"}", call.ArgumentsJson);
        Assert.True(call.ProviderExecuted);
        AssertToolResult(result.Content[1], "mcp_search_test", "search", "\"Found 25 tech events in San Francisco\"");
        AssertText(result.Content[2], text, "msg_mcp_response");
        AssertSource(result.Content[3], "id-0", "https://techevents.com/sf-ai", "SF AI Conference 2025");
        AssertSource(result.Content[4], "id-1", "https://eventbrite.com/sf-startups", "SF Startup Meetups");
    }

    [Fact]
    [UpstreamTest(Model + "doStream::should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_emits_text_deltas()
    {
        var parts = await Stream(
            "{\"type\":\"response.created\",\"response\":{\"id\":\"resp_test\",\"object\":\"response\",\"created_at\":1741269019,\"status\":\"in_progress\",\"model\":\"deepseek-ai/DeepSeek-V3-0324\"}}",
            "{\"type\":\"response.in_progress\",\"response\":{\"id\":\"resp_test\",\"object\":\"response\",\"created_at\":1741269019,\"status\":\"in_progress\"}}",
            "{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"in_progress\",\"content\":[]},\"sequence_number\":1}",
            "{\"type\":\"response.output_text.delta\",\"item_id\":\"msg_test\",\"output_index\":0,\"content_index\":0,\"delta\":\"Hello,\",\"sequence_number\":2}",
            "{\"type\":\"response.output_text.delta\",\"item_id\":\"msg_test\",\"output_index\":0,\"content_index\":0,\"delta\":\" World!\",\"sequence_number\":3}",
            "{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello, World!\"}]},\"sequence_number\":4}",
            "{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_test\",\"model\":\"deepseek-ai/DeepSeek-V3-0324\",\"object\":\"response\",\"created_at\":1741269112,\"status\":\"completed\",\"incomplete_details\":null,\"usage\":{\"input_tokens\":12,\"output_tokens\":25,\"total_tokens\":37},\"output\":[{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello, World!\"}]}]},\"sequence_number\":5}");
        Assert.Equal(new[] { "stream-start", "response-metadata", "text-start", "text-delta", "text-delta", "text-end", "finish" }, parts.Select(part => part.Type));
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        AssertMetadata(parts[1], "resp_test", "deepseek-ai/DeepSeek-V3-0324");
        AssertTextStart(parts[2], "msg_test");
        Assert.Equal(new[] { "Hello,", " World!" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.All(parts.OfType<TextDeltaStreamPart>(), part => Assert.Equal("msg_test", part.Id));
        Assert.Equal("msg_test", Assert.IsType<TextEndStreamPart>(parts[5]).Id);
        AssertFinish(parts[6], "resp_test", 12, 25, "{\"input_tokens\":12,\"output_tokens\":25,\"total_tokens\":37}");
    }

    [Fact]
    [UpstreamTest(Model + "doStream::should handle streaming without usage", Coverage = UpstreamCoverage.Partial, Note = "Input, output, cache, reasoning, and raw usage are null. Total becomes 0 because LanguageModelUsage fills a null total.")]
    public async Task Stream_without_usage_leaves_token_counts_unset()
    {
        var parts = await Stream(
            "{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"in_progress\"},\"sequence_number\":1}",
            "{\"type\":\"response.output_text.delta\",\"item_id\":\"msg_test\",\"output_index\":0,\"content_index\":0,\"delta\":\"Hi!\",\"sequence_number\":2}",
            "{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\"},\"sequence_number\":3}",
            "{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_test\",\"status\":\"completed\",\"incomplete_details\":null,\"usage\":null},\"sequence_number\":4}");
        var usage = parts.OfType<FinishStreamPart>().Single().Usage;
        Assert.Null(usage.InputTokens);
        Assert.Null(usage.OutputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest(Model + "doStream::should handle non-message item types", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_ignores_non_message_items()
    {
        var parts = await Stream(
            "{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"mcp_test\",\"type\":\"mcp_list_tools\",\"server_label\":\"test\"},\"sequence_number\":1}",
            "{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":{\"id\":\"mcp_test\",\"type\":\"mcp_list_tools\",\"server_label\":\"test\"},\"sequence_number\":2}",
            "{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_test\",\"status\":\"completed\",\"incomplete_details\":null},\"sequence_number\":3}");
        Assert.Equal(new[] { "stream-start", "finish" }, parts.Select(part => part.Type));
    }

    [Fact]
    [UpstreamTest(Model + "doStream::should handle streaming errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_reports_an_unparsable_chunk()
    {
        var parts = await Stream(
            "{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\"},\"sequence_number\":1}",
            "invalid json}");
        Assert.Single(parts.OfType<ErrorStreamPart>());
        var finish = parts.OfType<FinishStreamPart>().Single();
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Model + "doStream::preserves $expectedType stream errors", Coverage = UpstreamCoverage.Partial, Note = "The message and the error finish match, and the raw finish reason is the code. ErrorStreamPart has no type, code, or data fields.")]
    public async Task Stream_errors_become_error_parts()
    {
        foreach (var (json, message) in new[]
        {
            ("{\"type\":\"response.failed\",\"response\":{\"error\":{\"code\":\"429\",\"message\":\"Rate limit reached\"}},\"sequence_number\":1}", "Rate limit reached"),
            ("{\"type\":\"error\",\"code\":\"503\",\"message\":\"Service unavailable\",\"param\":null,\"sequence_number\":1}", "Service unavailable"),
        })
        {
            var parts = await Stream(json);
            Assert.Equal(message, parts.OfType<ErrorStreamPart>().Single().Message);
            var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
            Assert.Equal(FinishReason.Error, finish.FinishReason);
        }
    }

    [Fact]
    [UpstreamTest(Model + "doStream::should send correct streaming request", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_stream_true()
    {
        var capture = new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse("{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_test\",\"status\":\"completed\"},\"sequence_number\":1}") };
        await UpstreamChat.Read(Responses(capture).DoStreamAsync(
            new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") }, Temperature = 0.7 },
            CancellationToken.None));
        JsonAssert.Equal(
            JsonNode.Parse(capture.Requests[0].Body),
            "{\"model\":\"deepseek-ai/DeepSeek-V3-0324\",\"temperature\":0.7,\"stream\":true,\"input\":[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}]}");
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
    [UpstreamTest(Model + "tool calls::should handle function_call tool responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Function_calls_become_tool_calls()
    {
        var result = await Generate("{\"output\":[{\"id\":\"fc_test\",\"type\":\"function_call\",\"call_id\":\"call_123\",\"name\":\"getWeather\",\"arguments\":\"{\\\"location\\\": \\\"New York\\\"}\",\"output\":\"{\\\"temperature\\\": \\\"72°F\\\", \\\"condition\\\": \\\"sunny\\\"}\"},{\"id\":\"msg_after_tool\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"The weather in New York is 72°F and sunny.\"}]}]}");
        Assert.Equal(3, result.Content.Count);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("call_123", call.ToolCallId);
        Assert.Equal("getWeather", call.ToolName);
        Assert.Equal("{\"location\": \"New York\"}", call.ArgumentsJson);
        AssertToolResult(result.Content[1], "call_123", "getWeather", "\"{\\\"temperature\\\": \\\"72°F\\\", \\\"condition\\\": \\\"sunny\\\"}\"");
        AssertText(result.Content[2], "The weather in New York is 72°F and sunny.", "msg_after_tool");
    }

    [Fact]
    [UpstreamTest(Model + "tool calls::should stream tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_calls_stream_as_input_call_and_result()
    {
        var parts = await Stream(
            "{\"type\":\"response.created\",\"response\":{\"id\":\"resp_tool_stream\",\"object\":\"response\",\"created_at\":1741269019,\"status\":\"in_progress\",\"model\":\"deepseek-ai/DeepSeek-V3-0324\"}}",
            "{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"fc_stream\",\"type\":\"function_call\",\"call_id\":\"call_456\",\"name\":\"calculator\",\"arguments\":\"\"},\"sequence_number\":1}",
            "{\"type\":\"response.function_call_arguments.delta\",\"item_id\":\"fc_stream\",\"output_index\":0,\"delta\":\"{\\\"operation\\\"\",\"sequence_number\":2}",
            "{\"type\":\"response.function_call_arguments.delta\",\"item_id\":\"fc_stream\",\"output_index\":0,\"delta\":\": \\\"add\\\", \\\"a\\\": 5, \\\"b\\\": 3}\",\"sequence_number\":3}",
            "{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":{\"id\":\"fc_stream\",\"type\":\"function_call\",\"call_id\":\"call_456\",\"name\":\"calculator\",\"arguments\":\"{\\\"operation\\\": \\\"add\\\", \\\"a\\\": 5, \\\"b\\\": 3}\",\"output\":\"8\"},\"sequence_number\":4}",
            "{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_tool_stream\",\"status\":\"completed\",\"usage\":{\"input_tokens\":20,\"output_tokens\":15,\"total_tokens\":35}},\"sequence_number\":5}");
        Assert.Equal(new[] { "stream-start", "response-metadata", "tool-input-start", "tool-input-end", "tool-call", "tool-result", "finish" }, parts.Select(part => part.Type));
        AssertMetadata(parts[1], "resp_tool_stream", "deepseek-ai/DeepSeek-V3-0324");
        var start = Assert.IsType<ToolInputStartStreamPart>(parts[2]);
        Assert.Equal("call_456", start.Id);
        Assert.Equal("calculator", start.ToolName);
        Assert.Equal("call_456", Assert.IsType<ToolInputEndStreamPart>(parts[3]).Id);
        var call = Assert.IsType<ToolCallStreamPart>(parts[4]);
        Assert.Equal("call_456", call.ToolCallId);
        Assert.Equal("calculator", call.ToolName);
        Assert.Equal("{\"operation\": \"add\", \"a\": 5, \"b\": 3}", call.ArgumentsJson);
        var result = Assert.IsType<ToolResultStreamPart>(parts[5]);
        Assert.Equal("call_456", result.ToolCallId);
        Assert.Equal("calculator", result.ToolName);
        JsonAssert.Equal(result.Result, "\"8\"");
        AssertFinish(parts[6], "resp_tool_stream", 20, 15, "{\"input_tokens\":20,\"output_tokens\":15,\"total_tokens\":35}");
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
    [UpstreamTest(Model + "structured output::should handle structured output with custom name and description", Coverage = UpstreamCoverage.Covered)]
    public async Task Structured_output_sends_name_and_description()
    {
        var capture = new UpstreamCapture();
        await Responses(capture, "moonshotai/Kimi-K2-Instruct").DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                JsonSchema = UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}"),
                JsonSchemaName = "person_profile",
                ProviderOptions = UpstreamChat.Bag("huggingface", "{\"responseFormatDescription\":\"A person profile with basic information\"}"),
            },
            CancellationToken.None);
        var format = JsonNode.Parse(capture.Requests[0].Body)!["text"]!["format"]!;
        Assert.Equal("person_profile", format["name"]!.GetValue<string>());
        Assert.Equal("A person profile with basic information", format["description"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Model + "reasoning::should handle reasoning content in responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_items_precede_the_answer()
    {
        var result = await Generate("{\"output\":[{\"id\":\"reasoning_1\",\"type\":\"reasoning\",\"content\":[{\"type\":\"reasoning_text\",\"text\":\"Let me think about this problem step by step...\"}]},{\"id\":\"msg_after_reasoning\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"The answer is 42.\"}]}]}");
        Assert.Equal(2, result.Content.Count);
        var reasoning = Assert.IsType<HuggingFaceReasoning>(result.Content[0]);
        Assert.Equal("Let me think about this problem step by step...", reasoning.Text);
        JsonAssert.Equal(reasoning.ProviderMetadata!.Value, "{\"huggingface\":{\"itemId\":\"reasoning_1\"}}");
        AssertText(result.Content[1], "The answer is 42.", "msg_after_reasoning");
    }

    [Fact]
    [UpstreamTest(Model + "reasoning::should stream reasoning content", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_streams_before_the_answer()
    {
        var parts = await Stream(
            "{\"type\":\"response.created\",\"response\":{\"id\":\"resp_reasoning_stream\",\"object\":\"response\",\"created_at\":1741269019,\"status\":\"in_progress\",\"model\":\"deepseek-ai/DeepSeek-R1\"}}",
            "{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"reasoning_stream\",\"type\":\"reasoning\"},\"sequence_number\":1}",
            "{\"type\":\"response.reasoning_text.delta\",\"item_id\":\"reasoning_stream\",\"output_index\":0,\"content_index\":0,\"delta\":\"Thinking about\",\"sequence_number\":2}",
            "{\"type\":\"response.reasoning_text.delta\",\"item_id\":\"reasoning_stream\",\"output_index\":0,\"content_index\":0,\"delta\":\" the problem...\",\"sequence_number\":3}",
            "{\"type\":\"response.reasoning_text.done\",\"item_id\":\"reasoning_stream\",\"output_index\":0,\"content_index\":0,\"text\":\"Thinking about the problem...\",\"sequence_number\":4}",
            "{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":{\"id\":\"reasoning_stream\",\"type\":\"reasoning\",\"content\":[{\"type\":\"reasoning_text\",\"text\":\"Thinking about the problem...\"}]},\"sequence_number\":5}",
            "{\"type\":\"response.output_item.added\",\"output_index\":1,\"item\":{\"id\":\"msg_stream\",\"type\":\"message\",\"role\":\"assistant\"},\"sequence_number\":6}",
            "{\"type\":\"response.output_text.delta\",\"item_id\":\"msg_stream\",\"output_index\":1,\"content_index\":0,\"delta\":\"The solution is\",\"sequence_number\":7}",
            "{\"type\":\"response.output_text.delta\",\"item_id\":\"msg_stream\",\"output_index\":1,\"content_index\":0,\"delta\":\" simple.\",\"sequence_number\":8}",
            "{\"type\":\"response.output_item.done\",\"output_index\":1,\"item\":{\"id\":\"msg_stream\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\",\"content\":[{\"type\":\"output_text\",\"text\":\"The solution is simple.\"}]},\"sequence_number\":9}",
            "{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_reasoning_stream\",\"status\":\"completed\",\"usage\":{\"input_tokens\":10,\"output_tokens\":20,\"total_tokens\":30}},\"sequence_number\":10}");
        Assert.Equal(new[] { "stream-start", "response-metadata", "reasoning-start", "reasoning-delta", "reasoning-delta", "reasoning-end", "text-start", "text-delta", "text-delta", "text-end", "finish" }, parts.Select(part => part.Type));
        AssertMetadata(parts[1], "resp_reasoning_stream", "deepseek-ai/DeepSeek-R1");
        var reasoningStart = Assert.IsType<ReasoningStartStreamPart>(parts[2]);
        Assert.Equal("reasoning_stream", reasoningStart.Id);
        JsonAssert.Equal(reasoningStart.ProviderMetadata!.Value, "{\"huggingface\":{\"itemId\":\"reasoning_stream\"}}");
        Assert.Equal(new[] { "Thinking about", " the problem..." }, parts.OfType<ReasoningDeltaStreamPart>().Select(part => part.Delta));
        Assert.All(parts.OfType<ReasoningDeltaStreamPart>(), part => Assert.Equal("reasoning_stream", part.Id));
        Assert.Equal("reasoning_stream", Assert.IsType<ReasoningEndStreamPart>(parts[5]).Id);
        AssertTextStart(parts[6], "msg_stream");
        Assert.Equal(new[] { "The solution is", " simple." }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.All(parts.OfType<TextDeltaStreamPart>(), part => Assert.Equal("msg_stream", part.Id));
        Assert.Equal("msg_stream", Assert.IsType<TextEndStreamPart>(parts[9]).Id);
        AssertFinish(parts[10], "resp_reasoning_stream", 10, 20, "{\"input_tokens\":10,\"output_tokens\":20,\"total_tokens\":30}");
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

    private static void AssertText(GeneratedContent content, string text, string itemId)
    {
        var part = Assert.IsType<HuggingFaceText>(content);
        Assert.Equal(text, part.Text);
        JsonAssert.Equal(part.ProviderMetadata!.Value, "{\"huggingface\":{\"itemId\":\"" + itemId + "\"}}");
    }

    private static void AssertSource(GeneratedContent content, string id, string url, string title)
    {
        var source = Assert.IsType<GeneratedSource>(content);
        Assert.Equal(id, source.Id);
        Assert.Equal(url, source.Url);
        Assert.Equal(title, source.Title);
    }

    private static void AssertToolResult(GeneratedContent content, string toolCallId, string toolName, string result)
    {
        var part = Assert.IsType<HuggingFaceToolResult>(content);
        Assert.Equal(toolCallId, part.ToolCallId);
        Assert.Equal(toolName, part.ToolName);
        JsonAssert.Equal(part.Result, result);
    }

    private static void AssertMetadata(LanguageModelStreamPart part, string id, string modelId)
    {
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(part);
        Assert.Equal(id, metadata.Id);
        Assert.Equal(modelId, metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2025-03-06T13:50:19Z", System.Globalization.CultureInfo.InvariantCulture), metadata.Timestamp);
    }

    private static void AssertTextStart(LanguageModelStreamPart part, string id)
    {
        var start = Assert.IsType<TextStartStreamPart>(part);
        Assert.Equal(id, start.Id);
        JsonAssert.Equal(start.ProviderMetadata!.Value, "{\"huggingface\":{\"itemId\":\"" + id + "\"}}");
    }

    private static void AssertFinish(LanguageModelStreamPart part, string responseId, int input, int output, string raw)
    {
        var finish = Assert.IsType<FinishStreamPart>(part);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        JsonAssert.Equal(finish.ProviderMetadata!.Value, "{\"huggingface\":{\"responseId\":\"" + responseId + "\"}}");
        Assert.Equal(input, finish.Usage.InputTokens);
        Assert.Equal(output, finish.Usage.OutputTokens);
        Assert.Equal(input, finish.Usage.NoCacheInputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
        Assert.Equal(output, finish.Usage.TextTokens);
        JsonAssert.Equal(finish.Usage.Raw!.Value, raw);
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(params string[] events)
    {
        var capture = new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(events) };
        return await UpstreamChat.Read(Responses(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
    }

    private static async Task<LanguageModelGenerateResult> Generate(string body)
    {
        var capture = new UpstreamCapture { ResponseBody = body };
        return await Responses(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
    }

    private static HuggingFaceResponsesLanguageModel Responses(UpstreamCapture capture, string modelId = "deepseek-ai/DeepSeek-V3-0324")
    {
        var ids = 0;
        var provider = HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "APIKEY" }, capture);
        return new HuggingFaceResponsesLanguageModel(provider, modelId, () => "id-" + ids++.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
