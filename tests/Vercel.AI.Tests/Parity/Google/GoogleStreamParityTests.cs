// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Upstream Gemini <c>streamGenerateContent</c> outcomes that do not need partial function-call accumulation.</summary>
public sealed class GoogleStreamParityTests
{
    private const string File = "packages/google/src/google-language-model.test.ts::";

    [Fact]
    [UpstreamTest(File + "doStream::should stream reasoning parts separately from text parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_reasoning_separately_from_text()
    {
        var capture = Sse(
            "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"think\",\"thought\":true}]}}]}",
            "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"answer\"}]},\"finishReason\":\"STOP\"}]}");
        var parts = await GoogleParity.Stream(capture, "gemini-2.5-flash", GoogleParity.Prompt());
        Assert.Contains(parts, part => part is ReasoningStartStreamPart);
        Assert.Contains(parts, part => part is ReasoningDeltaStreamPart delta && delta.Delta == "think");
        Assert.Contains(parts, part => part is TextDeltaStreamPart delta && delta.Delta == "answer");
        Assert.EndsWith(":streamGenerateContent?alt=sse", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "doStream::should stream thought signatures with reasoning and text parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_thought_signatures()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"answer\",\"thoughtSignature\":\"sig\"}]},\"finishReason\":\"STOP\"}]}");
        var parts = await GoogleParity.Stream(capture, "gemini-3-flash", GoogleParity.Prompt());
        var signed = Assert.Single(parts.OfType<GoogleSignedDelta>());
        Assert.Equal("sig", signed.ProviderMetadata.GetProperty("google").GetProperty("thoughtSignature").GetString());
        Assert.Equal("answer", signed.Delta);
    }

    [Fact]
    [UpstreamTest(File + "doStream::should stream files", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_files()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AQID\"}}]},\"finishReason\":\"STOP\"}]}");
        var parts = await GoogleParity.Stream(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        var file = Assert.Single(parts.OfType<GoogleFileStreamPart>());
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 1, 2, 3 }, file.Data);
    }

    [Fact]
    [UpstreamTest(File + "doStream::should set finishReason to tool-calls when chunk contains functionCall", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_the_finish_reason_to_tool_calls()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"weather\",\"args\":{\"city\":\"Paris\"},\"id\":\"c1\"}}]},\"finishReason\":\"STOP\"}]}");
        var parts = await GoogleParity.Stream(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("weather", call.ToolName);
        Assert.Equal(FinishReason.ToolCalls, Assert.Single(parts.OfType<FinishStreamPart>()).FinishReason);
    }

    [Fact]
    [UpstreamTest(File + "doStream::should deduplicate sources across chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Deduplicates_sources_across_chunks()
    {
        var chunk = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"a\"}]},\"groundingMetadata\":{\"groundingChunks\":[{\"web\":{\"uri\":\"https://example.com\",\"title\":\"Example\"}}]}}]}";
        var capture = Sse(chunk, chunk);
        var parts = await GoogleParity.Stream(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        Assert.Single(parts.OfType<SourceStreamPart>());
    }

    [Fact]
    [UpstreamTest(File + "doStream::should send streamFunctionCallArguments in toolConfig when provider option is set with Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_stream_function_call_arguments_on_Vertex()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}");
        var options = GoogleParity.Prompt();
        options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement> { ["vertex"] = GoogleParity.Json("{\"streamFunctionCallArguments\":true}") };
        options.Tools = new[] { new LanguageModelTool("weather", "Weather", GoogleParity.Json("{\"type\":\"object\"}")) };
        await GoogleParity.Stream(capture, "gemini-2.5-flash", options, vertex: true);
        Assert.True(GoogleParity.Request(capture)["toolConfig"]!["functionCallingConfig"]!["streamFunctionCallArguments"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(File + "doStream::should emit warning and not send streamFunctionCallArguments when using non-Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_and_drops_stream_function_call_arguments_off_Vertex()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}");
        var options = GoogleParity.Prompt();
        options.ProviderOptions = GoogleParity.GoogleOptions("{\"streamFunctionCallArguments\":true}");
        var parts = await GoogleParity.Stream(capture, "gemini-2.5-flash", options);
        Assert.Null(GoogleParity.Request(capture)["toolConfig"]);
        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Contains(start.Warnings, warning => warning.Message.Contains("streamFunctionCallArguments", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(File + "doStream::should not send streamFunctionCallArguments for Vertex provider doGenerate (unary API)", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_stream_function_call_arguments_on_unary_Vertex_calls()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                string.Empty,
                GoogleParity.Json("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}")),
        };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        var body = GoogleParity.Request(capture);
        Assert.Null(body["toolConfig"]);
        Assert.NotNull(body["tools"]);
        Assert.EndsWith(":generateContent", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "doStream::should not send streamFunctionCallArguments for Vertex provider doGenerate even when explicitly set", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_stream_function_call_arguments_on_unary_Vertex_calls()
    {
        var capture = new GoogleCapture();
        var options = GoogleParity.Prompt();
        options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement> { ["vertex"] = GoogleParity.Json("{\"streamFunctionCallArguments\":true}") };
        await GoogleParity.Generate(capture, "gemini-2.5-flash", options, vertex: true);
        Assert.Null(GoogleParity.Request(capture)["toolConfig"]);
        Assert.EndsWith(":generateContent", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "doStream > providerMetadata key based on provider string::should use \"google\" as providerMetadata key in finish event when provider does not include \"vertex\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_google_metadata_key()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}],\"usageMetadata\":{\"promptTokenCount\":1}}");
        var parts = await GoogleParity.Stream(capture, "gemini-2.0-flash", GoogleParity.Prompt());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.True(finish.ProviderMetadata!.Value.TryGetProperty("google", out _));
        Assert.False(finish.ProviderMetadata.Value.TryGetProperty("vertex", out _));
    }

    [Fact]
    [UpstreamTest(File + "doStream > providerMetadata key based on provider string::should use \"vertex\" as providerMetadata key in finish event when provider includes \"vertex\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_vertex_metadata_key()
    {
        var capture = Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}");
        var parts = await GoogleParity.Stream(capture, "gemini-2.0-flash", GoogleParity.Prompt(), vertex: true);
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.True(finish.ProviderMetadata!.Value.TryGetProperty("vertex", out _));
        Assert.True(finish.ProviderMetadata.Value.TryGetProperty("googleVertex", out _));
    }

    private static GoogleCapture Sse(params string[] events)
    {
        var payload = string.Join(string.Empty, events.Select(item => "data: " + item + "\n\n"));
        return new GoogleCapture { Sse = payload };
    }
}
