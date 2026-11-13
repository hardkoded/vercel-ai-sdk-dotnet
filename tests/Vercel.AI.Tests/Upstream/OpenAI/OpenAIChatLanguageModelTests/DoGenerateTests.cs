// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIChatLanguageModelTests;

/// <summary>Port of <c>openai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> (refusals).</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate::";

    private static Task<LanguageModelGenerateResult> Generate(string json)
    {
        var capture = new OpenAICapture { ResponseJson = json };
        return OpenAIUpstream.Provider(capture).ChatModel("gpt-4o-2024-08-06").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract a refusal as text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_a_refusal_as_text_content()
    {
        var result = await Generate(
            "{\"id\":\"chatcmpl-refusal\",\"object\":\"chat.completion\",\"created\":1711115037,\"model\":\"gpt-4o-2024-08-06\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":null,\"refusal\":\"I'm sorry, I can't assist with that request.\"},\"finish_reason\":\"stop\"}]}");

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("I'm sorry, I can't assist with that request.", text.Text);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract a refusal from a real response as text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_a_refusal_from_a_real_response_as_text_content()
    {
        var result = await Generate(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "openai-chat-refusal.json")));

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("I'm very sorry, but I can't assist with that request.", text.Text);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract content and refusal as text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_content_and_refusal_as_text_content()
    {
        var result = await Generate(
            "{\"id\":\"chatcmpl-content-and-refusal\",\"object\":\"chat.completion\",\"created\":1711115037,\"model\":\"gpt-4o-2024-08-06\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Here is the first part. \",\"refusal\":\"I cannot continue with the rest.\"},\"finish_reason\":\"stop\"}]}");

        Assert.Collection(
            result.Content,
            first => Assert.Equal("Here is the first part. ", Assert.IsType<GeneratedText>(first).Text),
            second => Assert.Equal("I cannot continue with the rest.", Assert.IsType<GeneratedText>(second).Text));
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep the tool call finish reason when a refusal is present", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_the_tool_call_finish_reason_when_a_refusal_is_present()
    {
        var result = await Generate(
            "{\"id\":\"chatcmpl-refusal-tool-call\",\"object\":\"chat.completion\",\"created\":1711115037,\"model\":\"gpt-4o-2024-08-06\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":null,\"refusal\":\"Let me look that up.\",\"tool_calls\":[{\"id\":\"call-1\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"{\\\"value\\\":\\\"Spark\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}]}");

        Assert.Collection(
            result.Content,
            first => Assert.Equal("Let me look that up.", Assert.IsType<GeneratedText>(first).Text),
            second =>
            {
                var call = Assert.IsType<GeneratedToolCall>(second);
                Assert.Equal("call-1", call.ToolCallId);
                Assert.Equal("test-tool", call.ToolName);
                Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
            });
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
    }
}
