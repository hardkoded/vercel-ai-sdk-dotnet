// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GroqUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_the_model_and_messages()
    {
        var capture = new UpstreamCapture();
        await Chat(capture, "gemma2-9b-it").DoGenerateAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None);
        Assert.Equal("{\"model\":\"gemma2-9b-it\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}", capture.Requests[0].Body);
        Assert.Equal("api.groq.com", capture.Requests[0].Uri!.Host);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Contains("ai-sdk/groq/0.0.0", capture.Requests[0].Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doStream::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_stream_true_without_usage_options()
    {
        var capture = Stream();
        await UpstreamChat.Read(Chat(capture, "gemma2-9b-it").DoStreamAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None));
        Assert.Equal("{\"model\":\"gemma2-9b-it\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"stream\":true}", capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_passes_headers()
    {
        var capture = new UpstreamCapture();
        var provider = GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var options = UpstreamChat.Prompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("gemma2-9b-it")).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("provider-header-value", capture.Requests[0].Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", capture.Requests[0].Headers["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/groq/0.0.0", capture.Requests[0].Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should coerce top-level reasoning minimal to low", Coverage = UpstreamCoverage.Covered)]
    public async Task Minimal_reasoning_is_sent_as_low()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "minimal";
        var result = await Chat(capture, "llama-3.3-70b-versatile").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("low", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Message.Contains("minimal") && warning.Message.Contains("low"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should coerce top-level reasoning xhigh to high", Coverage = UpstreamCoverage.Covered)]
    public async Task Xhigh_reasoning_is_sent_as_high()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "xhigh";
        var result = await Chat(capture, "llama-3.3-70b-versatile").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("high", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Message.Contains("xhigh") && warning.Message.Contains("high"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should map top-level reasoning none to reasoning_effort for Qwen 3.6", Coverage = UpstreamCoverage.Covered)]
    public async Task Qwen_keeps_reasoning_none()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";
        var result = await Chat(capture, "qwen/qwen3.6-27b").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("none", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.DoesNotContain(result.Warnings, warning => warning.Message.Contains("none"));
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate > top-level reasoning::should omit unsupported top-level reasoning none and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Other_models_drop_reasoning_none()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";
        var result = await Chat(capture, "llama-3.3-70b-versatile").DoGenerateAsync(options, CancellationToken.None);
        Assert.Null(UpstreamChat.Body(capture)["reasoning_effort"]);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "reasoning \"none\" is not supported by this model.");
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass response format information as json_schema when structuredOutputs enabled by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Structured_outputs_default_to_json_schema()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}");
        options.ProviderOptions = UpstreamChat.Bag("groq", "{\"responseFormat\":\"json\"}");
        await Chat(capture, "llama-3.3-70b-versatile").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_schema", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/groq-chat-language-model.test.ts::doGenerate::should pass response format information as json_object when structuredOutputs explicitly disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Disabling_structured_outputs_sends_json_object()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}");
        options.ProviderOptions = UpstreamChat.Bag("groq", "{\"responseFormat\":\"json\",\"structuredOutputs\":false}");
        var result = await Chat(capture, "llama-3.3-70b-versatile").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_object", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("structuredOutputs"));
    }

    private static OpenAICompatibleLanguageModel Chat(UpstreamCapture capture, string modelId)
    {
        return (OpenAICompatibleLanguageModel)GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel(modelId);
    }

    private static UpstreamCapture Stream()
    {
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"),
        };
    }
}
