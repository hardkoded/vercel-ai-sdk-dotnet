// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Zai;

namespace Vercel.AI.Tests;

public sealed class ZaiUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::maps Z.AI provider options and omits unsupported standard options", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_are_renamed_and_unsupported_fields_are_dropped()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.FrequencyPenalty = 0.2;
        options.PresencePenalty = 0.3;
        options.Seed = 42;
        options.Reasoning = "low";
        options.ToolChoice = ToolChoice.Required;
        options.Tools = new[] { new LanguageModelTool("calculator", "Calculate a value", UpstreamChat.Json("{\"type\":\"object\",\"properties\":{}}")) };
        options.ProviderOptions = UpstreamChat.Bag(
            "zai",
            "{\"doSample\":false,\"thinking\":{\"type\":\"enabled\",\"clearThinking\":false},\"reasoningEffort\":\"max\",\"toolStream\":true,\"requestId\":\"request-123456\",\"userId\":\"user-123456\",\"ignoredOption\":true}");
        var result = await Chat(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Equal("glm-5.3", body["model"]!.GetValue<string>());
        Assert.False(body["do_sample"]!.GetValue<bool>());
        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.False(body["thinking"]!["clear_thinking"]!.GetValue<bool>());
        Assert.Equal("max", body["reasoning_effort"]!.GetValue<string>());
        Assert.True(body["tool_stream"]!.GetValue<bool>());
        Assert.Equal("request-123456", body["request_id"]!.GetValue<string>());
        Assert.Equal("user-123456", body["user_id"]!.GetValue<string>());
        Assert.Null(body["frequency_penalty"]);
        Assert.Null(body["presence_penalty"]);
        Assert.Null(body["seed"]);
        Assert.Null(body["ignoredOption"]);
        Assert.Null(body["tool_choice"]);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "frequencyPenalty");
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "presencePenalty");
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "seed");
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "toolChoice required. Z.AI currently supports only automatic tool selection.");
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::implements toolChoice none by omitting tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_choice_none_omits_tools()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ToolChoice = ToolChoice.None;
        options.Tools = new[] { new LanguageModelTool("calculator", null, UpstreamChat.Json("{\"type\":\"object\",\"properties\":{}}")) };
        await Chat(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Null(body["tools"]);
        Assert.Null(body["tool_choice"]);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::validates Z.AI provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task A_short_request_id_is_rejected_before_the_request()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("zai", "{\"requestId\":\"short\"}");
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Chat(capture).DoGenerateAsync(options, CancellationToken.None));
        Assert.Equal("invalid zai provider options", error.Message);
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::parses text, reasoning, tool calls, cached usage, and finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_parses_text_reasoning_tool_calls_and_cached_usage()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"id\":\"chatcmpl-123\",\"request_id\":\"request-123\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"The answer is 42.\",\"reasoning_content\":\"I should calculate the answer.\",\"tool_calls\":[{\"id\":\"call-1\",\"type\":\"function\",\"function\":{\"name\":\"calculator\",\"arguments\":\"{\\\"value\\\":42}\"}}]},\"finish_reason\":\"tool_calls\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":7,\"prompt_tokens_details\":{\"cached_tokens\":3},\"total_tokens\":17}}",
        };
        var result = await Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(3, result.Content.Count);
        Assert.Equal("The answer is 42.", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        Assert.Equal("I should calculate the answer.", Assert.IsType<GeneratedReasoning>(result.Content[1]).Text);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[2]);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("calculator", call.ToolName);
        Assert.Equal("{\"value\":42}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(3, result.Usage.CacheReadTokens);
        Assert.Equal(7, result.Usage.NoCacheInputTokens);
        Assert.Equal("chatcmpl-123", result.ResponseId);
        Assert.Equal("glm-5.3", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1777000000), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::streams incremental tool-call arguments", Coverage = UpstreamCoverage.Partial, Note = "The argument fragments are joined into one tool call. The shared stream model does not emit tool-input start, delta, and end parts.")]
    public async Task Streamed_tool_call_arguments_are_joined()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(
                "{\"id\":\"chatcmpl-tool\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"delta\":{\"role\":\"assistant\",\"tool_calls\":[{\"index\":0,\"id\":\"call-weather\",\"function\":{\"name\":\"weather\",\"arguments\":\"{\\\"city\\\"\"}}]},\"finish_reason\":null}]}",
                "{\"id\":\"chatcmpl-tool\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"delta\":{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\":\\\"Paris\\\"}\"}}]},\"finish_reason\":null}]}",
                "{\"id\":\"chatcmpl-tool\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"delta\":{},\"finish_reason\":\"tool_calls\"}],\"usage\":{\"prompt_tokens\":5,\"completion_tokens\":4,\"total_tokens\":9}}"),
        };
        var options = UpstreamChat.Prompt();
        options.Tools = new[] { new LanguageModelTool("weather", null, UpstreamChat.Json("{\"type\":\"object\",\"properties\":{}}")) };
        options.ProviderOptions = UpstreamChat.Bag("zai", "{\"toolStream\":true}");
        var parts = await UpstreamChat.Read(Chat(capture).DoStreamAsync(options, CancellationToken.None));
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call-weather", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"city\":\"Paris\"}", call.ArgumentsJson);
        var finish = (FinishStreamPart)parts[parts.Count - 1];
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("tool_calls", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::maps the %s finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_finish_reasons_are_mapped()
    {
        await AssertFinish("sensitive", FinishReason.ContentFilter);
        await AssertFinish("model_context_window_exceeded", FinishReason.Length);
        await AssertFinish("network_error", FinishReason.Error);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::streams reasoning, text, usage, raw chunks, and tool-stream options", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_emits_reasoning_text_raw_chunks_and_tool_stream()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(
                "{\"id\":\"chatcmpl-stream\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"delta\":{\"role\":\"assistant\",\"reasoning_content\":\"Think.\"},\"finish_reason\":null}]}",
                "{\"id\":\"chatcmpl-stream\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"delta\":{\"content\":\"Answer.\"},\"finish_reason\":null}]}",
                "{\"id\":\"chatcmpl-stream\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}]}",
                "{\"id\":\"chatcmpl-stream\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[],\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":3,\"total_tokens\":7}}"),
        };
        var options = UpstreamChat.Prompt();
        options.IncludeRawChunks = true;
        options.ProviderOptions = UpstreamChat.Bag("zai", "{\"toolStream\":true}");
        var parts = await UpstreamChat.Read(Chat(capture).DoStreamAsync(options, CancellationToken.None));
        var body = UpstreamChat.Body(capture);
        Assert.True(body["stream"]!.GetValue<bool>());
        Assert.True(body["tool_stream"]!.GetValue<bool>());
        Assert.Null(body["stream_options"]);
        Assert.Equal(
            "stream-start,raw,response-metadata,reasoning-start,reasoning-delta,raw,reasoning-end,text-start,text-delta,raw,raw,text-end,finish",
            string.Join(",", parts.Select(part => part.Type)));
        var finish = (FinishStreamPart)parts[parts.Count - 1];
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(4, finish.Usage.InputTokens);
        Assert.Equal(3, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-chat-language-model.test.ts::ZaiChatLanguageModel::parses the documented Z.AI error envelope", Coverage = UpstreamCoverage.Covered)]
    public async Task A_top_level_error_message_is_surfaced()
    {
        var capture = new UpstreamCapture
        {
            Status = System.Net.HttpStatusCode.BadRequest,
            ResponseBody = "{\"code\":1001,\"message\":\"Invalid request.\"}",
        };
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Invalid request.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-provider.test.ts::createZai::creates callable, languageModel, and chat models", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_the_chat_model()
    {
        var model = (OpenAICompatibleLanguageModel)ZaiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }).LanguageModel("glm-5.3");
        Assert.Equal("V4", model.SpecificationVersion);
        Assert.Equal("zai.chat", model.Provider);
        Assert.Equal("glm-5.3", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-provider.test.ts::createZai::uses the default endpoint, bearer authentication, and user agent", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_endpoint_uses_bearer_auth_and_the_zai_user_agent()
    {
        var capture = new UpstreamCapture();
        await Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://api.z.ai/api/paas/v4/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer test-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Contains("ai-sdk/zai/" + AiSdkVersion.Version, capture.Requests[0].Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-provider.test.ts::createZai::reads the API key from ZAI_API_KEY by default", Coverage = UpstreamCoverage.Covered)]
    public async Task The_api_key_is_read_from_the_environment()
    {
        var previous = Environment.GetEnvironmentVariable("ZAI_API_KEY");
        Environment.SetEnvironmentVariable("ZAI_API_KEY", "environment-key");
        try
        {
            var capture = new UpstreamCapture();
            var model = (OpenAICompatibleLanguageModel)ZaiProvider.Create(handler: capture).LanguageModel("glm-5.3");
            await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
            Assert.Equal("Bearer environment-key", capture.Requests[0].Headers["Authorization"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZAI_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-provider.test.ts::createZai::uses custom settings and removes a trailing slash from baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_custom_base_url_drops_the_trailing_slash()
    {
        var capture = new UpstreamCapture();
        var provider = ZaiProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://example.com/zai/" },
            capture);
        provider.Options.Headers["x-custom"] = "value";
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("glm-5.3")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://example.com/zai/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer custom-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("value", capture.Requests[0].Headers["x-custom"]);
    }

    [Fact]
    [UpstreamTest("packages/zai/src/zai-provider.test.ts::createZai::throws NoSuchModelError for unsupported model types", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_and_images_are_rejected()
    {
        var provider = ZaiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" });
        var embedding = Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("model"));
        var image = Assert.Throws<AiSdkException>(() => provider.ImageModel("model"));
        Assert.Equal("Provider 'zai' does not support embedding model 'model'.", embedding.Message);
        Assert.Equal("Provider 'zai' does not support image model 'model'.", image.Message);
    }

    private static async Task AssertFinish(string raw, FinishReason unified)
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"id\":\"chatcmpl-123\",\"created\":1777000000,\"model\":\"glm-5.3\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null},\"finish_reason\":\"" + raw + "\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":7,\"total_tokens\":17}}",
        };
        var result = await Chat(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(unified, result.FinishReason);
        Assert.Equal(raw, result.RawFinishReason);
    }

    private static OpenAICompatibleLanguageModel Chat(UpstreamCapture capture)
    {
        return (OpenAICompatibleLanguageModel)ZaiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, capture).LanguageModel("glm-5.3");
    }
}
