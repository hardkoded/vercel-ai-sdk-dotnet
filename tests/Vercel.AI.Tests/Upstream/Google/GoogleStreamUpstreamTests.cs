// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Gemini generateContent streaming.</summary>
public sealed class GoogleStreamUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > text::should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text_deltas_and_finishes_with_stop()
    {
        var first = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"He\"}]}}]}";
        var second = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"llo\"}]},\"finishReason\":\"STOP\"}]}";
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), Sse(first) + Sse(second), false).ConfigureAwait(false);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Equal("He", Assert.IsType<TextDeltaStreamPart>(parts.Single(part => part is TextDeltaStreamPart && ((TextDeltaStreamPart)part).Delta == "He")).Delta);
        Assert.Equal("llo", parts.OfType<TextDeltaStreamPart>().Single(part => part.Delta == "llo").Delta);
        Assert.Equal(FinishReason.Stop, parts.OfType<FinishStreamPart>().Single().FinishReason);
        Assert.DoesNotContain(parts, part => part is RawStreamPart);
        Assert.Contains(":streamGenerateContent?alt=sse", LastHandler.Uris[0]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > text::should include raw chunks when includeRawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_raw_stream_chunks_when_requested()
    {
        var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hi\"}]},\"finishReason\":\"STOP\"}]}";
        var options = GoogleUpstream.Hello();
        options.IncludeRawChunks = true;
        var parts = await Stream("gemini-2.5-flash", options, Sse(json), false).ConfigureAwait(false);
        Assert.Contains(parts.OfType<RawStreamPart>(), part => part.RawJson == json);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > text::should not include raw chunks when includeRawChunks is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_raw_stream_chunks_by_default()
    {
        var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hi\"}]},\"finishReason\":\"STOP\"}]}";
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), Sse(json), false).ConfigureAwait(false);
        Assert.DoesNotContain(parts, part => part is RawStreamPart);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > text::should emit response-metadata with the provider responseId", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_response_metadata_once_for_a_repeated_response_id()
    {
        var chunk = "{\"responseId\":\"bH6LaZW8Fp_3nsEPqtaSwQ4\",\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hi\"}]},\"finishReason\":\"STOP\"}]}";
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), Sse(chunk) + Sse(chunk), false).ConfigureAwait(false);
        var metadata = parts.OfType<ResponseMetadataStreamPart>().ToList();
        Assert.Single(metadata);
        Assert.Equal("bH6LaZW8Fp_3nsEPqtaSwQ4", metadata[0].Id);
        Assert.Equal("gemini-2.5-flash", metadata[0].ModelId);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > tool-call::should stream tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_a_function_call_and_finishes_with_tool_calls()
    {
        var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"weather\",\"id\":\"c1\",\"args\":{\"location\":\"SF\"}}}]},\"finishReason\":\"STOP\"}]}";
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), Sse(json), false).ConfigureAwait(false);
        var call = parts.OfType<ToolCallStreamPart>().Single();
        Assert.Equal("c1", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        GoogleUpstream.JsonEqual(GoogleUpstream.Element(call.ArgumentsJson), "{\"location\":\"SF\"}");
        Assert.Equal(FinishReason.ToolCalls, parts.OfType<FinishStreamPart>().Single().FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > reasoning::should stream reasoning and text parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_reasoning_before_text()
    {
        var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"think\",\"thought\":true},{\"text\":\"answer\"}]},\"finishReason\":\"STOP\"}]}";
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), Sse(json), false).ConfigureAwait(false);
        Assert.Equal("think", parts.OfType<ReasoningDeltaStreamPart>().Single().Delta);
        Assert.Equal("answer", parts.OfType<TextDeltaStreamPart>().Single().Delta);
        Assert.True(parts.FindIndex(part => part is ReasoningDeltaStreamPart) < parts.FindIndex(part => part is TextDeltaStreamPart));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should set finishReason to tool-calls when chunk contains functionCall", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_the_stream_finish_reason_to_tool_calls_for_a_function_call()
    {
        var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"weather\",\"args\":{}}}]},\"finishReason\":\"STOP\"}]}";
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), Sse(json), true).ConfigureAwait(false);
        Assert.Equal(FinishReason.ToolCalls, parts.OfType<FinishStreamPart>().Single().FinishReason);
        Assert.Equal("test-id", parts.OfType<ToolCallStreamPart>().Single().ToolCallId);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should omit server-side tool invocation flag for Vertex Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public void Omits_the_server_side_tool_invocation_flag_on_Vertex_Gemini_3()
    {
        var options = GoogleUpstream.Hello();
        options.Tools = new[] { GoogleUpstream.Function("test-tool", "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}") };
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"providerTools\":[{\"id\":\"google.google_search\"}]}}");
        var prepared = GoogleRequest.Prepare("gemini-3-pro-preview", "google.vertex.chat", options, true);
        Assert.False(prepared.Body["toolConfig"]!.AsObject().ContainsKey("includeServerSideToolInvocations"));
        GoogleUpstream.JsonEqual(prepared.Body["toolConfig"]!, "{\"functionCallingConfig\":{\"mode\":\"VALIDATED\"}}");
        GoogleUpstream.JsonEqual(prepared.Body["tools"]![0]!, "{\"googleSearch\":{}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should send streamFunctionCallArguments in toolConfig when provider option is set with Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public void Sends_streamFunctionCallArguments_for_a_Vertex_stream()
    {
        var options = ToolOptions("{\"vertex\":{\"streamFunctionCallArguments\":true}}");
        var prepared = GoogleRequest.Prepare("gemini-pro", "google.vertex.chat", options, true);
        GoogleUpstream.JsonEqual(prepared.Body["toolConfig"]!, "{\"functionCallingConfig\":{\"streamFunctionCallArguments\":true}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should emit warning and not send streamFunctionCallArguments when using non-Vertex provider", Coverage = UpstreamCoverage.Covered)]
    public void Warns_and_drops_streamFunctionCallArguments_off_Vertex()
    {
        var options = ToolOptions("{\"google\":{\"streamFunctionCallArguments\":true}}");
        var prepared = GoogleRequest.Prepare("gemini-pro", "google.generative-ai", options, true);
        Assert.Contains(prepared.Warnings, warning => warning.Message.Contains("streamFunctionCallArguments"));
        var config = prepared.Body["toolConfig"];
        Assert.True(config == null || config["functionCallingConfig"] == null || config["functionCallingConfig"]!["streamFunctionCallArguments"] == null);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should default streamFunctionCallArguments to false for Vertex provider without provider option", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_streamFunctionCallArguments_unset_when_the_option_is_absent()
    {
        var prepared = GoogleRequest.Prepare("gemini-pro", "google.vertex.chat", ToolOptions(null), true);
        Assert.True(prepared.Body["toolConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"]!["streamFunctionCallArguments"] == null);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should allow Vertex provider to opt out of streamFunctionCallArguments by setting it to false", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_send_streamFunctionCallArguments_when_it_is_false()
    {
        var options = ToolOptions("{\"vertex\":{\"streamFunctionCallArguments\":false}}");
        var prepared = GoogleRequest.Prepare("gemini-pro", "google.vertex.chat", options, true);
        Assert.True(prepared.Body["toolConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"]!["streamFunctionCallArguments"] == null);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should not send streamFunctionCallArguments for Vertex provider doGenerate (unary API)", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_send_streamFunctionCallArguments_for_a_unary_request()
    {
        var prepared = GoogleRequest.Prepare("gemini-pro", "google.vertex.chat", ToolOptions(null), false);
        Assert.True(prepared.Body["toolConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"]!["streamFunctionCallArguments"] == null);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should not send streamFunctionCallArguments for Vertex provider doGenerate even when explicitly set", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_send_streamFunctionCallArguments_for_unary_even_when_set()
    {
        var options = ToolOptions("{\"vertex\":{\"streamFunctionCallArguments\":true}}");
        var prepared = GoogleRequest.Prepare("gemini-pro", "google.vertex.chat", options, false);
        Assert.True(prepared.Body["toolConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"] == null || prepared.Body["toolConfig"]!["functionCallingConfig"]!["streamFunctionCallArguments"] == null);
    }


    private static RecordingHandler LastHandler = new();

    private static string Sse(string json)
    {
        return "data: " + json + "\n\n";
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(string modelId, LanguageModelCallOptions options, string sse, bool generateId)
    {
        LastHandler = new RecordingHandler { ResponseText = sse, MediaType = "text/event-stream" };
        var settings = new GoogleOptions { ApiKey = "test-api-key" };
        if (generateId)
        {
            settings.GenerateId = () => "test-id";
        }

        var provider = GoogleProvider.Create(settings, LastHandler);
        return await GoogleUpstream.Collect(provider.LanguageModel(modelId).DoStreamAsync(options, CancellationToken.None)).ConfigureAwait(false);
    }

    private static LanguageModelCallOptions ToolOptions(string? providerOptions)
    {
        var options = GoogleUpstream.Hello();
        options.Tools = new[] { GoogleUpstream.Function("test-tool", "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}") };
        if (providerOptions != null)
        {
            options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        }

        return options;
    }

}
