// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; server-side <c>toolCall</c> parts (tool combination), which Gemini runs without a declaration.</summary>
public sealed class ServerToolDynamicTests
{
    [Fact]
    [UpstreamTest(
        "packages/google/src/google-language-model.test.ts::doGenerate::should handle server-side toolCall and toolResponse parts (tool combination)",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the dynamic, provider-executed call and the text. The tool result, the provider metadata and the finish reason are not asserted.")]
    public void Should_handle_server_side_toolCall_and_toolResponse_parts_in_doGenerate()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"toolCall\":{\"toolType\":\"GOOGLE_SEARCH_WEB\",\"args\":{\"query\":\"San Francisco weather\"},\"id\":\"server-call-1\"},\"thoughtSignature\":\"sig-abc\"},{\"toolResponse\":{\"toolType\":\"GOOGLE_SEARCH_WEB\",\"response\":{\"results\":[{\"title\":\"Weather in SF\"}]},\"id\":\"server-call-1\"},\"thoughtSignature\":\"sig-def\"},{\"text\":\"The weather in San Francisco is sunny.\"}],\"role\":\"model\"},\"finishReason\":\"STOP\"}]}");

        var call = Assert.IsAssignableFrom<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("server-call-1", call.ToolCallId);
        Assert.Equal("server:GOOGLE_SEARCH_WEB", call.ToolName);
        Assert.Equal("{\"query\":\"San Francisco weather\"}", call.ArgumentsJson);
        Assert.True(call.ProviderExecuted);
        Assert.True(call.Dynamic);
        Assert.Equal("The weather in San Francisco is sunny.", Assert.IsAssignableFrom<GeneratedText>(result.Content[2]).Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/google-language-model.test.ts::doStream::should stream server-side toolCall and toolResponse parts (tool combination)",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the dynamic, provider-executed call and the text. The tool result, the provider metadata and the finish reason are not asserted.")]
    public async Task Should_stream_server_side_toolCall_and_toolResponse_parts_in_doStream()
    {
        var handler = new RecordingHandler
        {
            MediaType = "text/event-stream",
            ResponseText =
                "data: {\"candidates\":[{\"content\":{\"parts\":[{\"toolCall\":{\"toolType\":\"GOOGLE_SEARCH_WEB\",\"args\":{\"query\":\"SF weather\"},\"id\":\"sc-1\"},\"thoughtSignature\":\"sig-1\"}]}}]}\n\n" +
                "data: {\"candidates\":[{\"content\":{\"parts\":[{\"toolResponse\":{\"toolType\":\"GOOGLE_SEARCH_WEB\",\"response\":{\"results\":[{\"title\":\"Weather\"}]},\"id\":\"sc-1\"}}]}}]}\n\n" +
                "data: {\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"It is sunny.\"}]},\"finishReason\":\"STOP\"}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":20,\"totalTokenCount\":30}}\n\n",
        };
        var model = GoogleUpstream.Client(handler).LanguageModel("gemini-3-pro-preview");

        var parts = await GoogleUpstream.Collect(await model.DoStreamAsync(GoogleUpstream.Hello(), CancellationToken.None));

        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("sc-1", call.ToolCallId);
        Assert.Equal("server:GOOGLE_SEARCH_WEB", call.ToolName);
        Assert.Equal("{\"query\":\"SF weather\"}", call.ArgumentsJson);
        Assert.True(call.ProviderExecuted);
        Assert.True(call.Dynamic);
        Assert.Equal("It is sunny.", Assert.Single(parts.OfType<TextDeltaStreamPart>()).Delta);
    }
}
