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
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), FixtureSse("google-tool-call"), true).ConfigureAwait(false);
        Assert.Equal(
            new[]
            {
                "stream-start",
                "response-metadata b36LacjwM668nsEP2tbsgQQ",
                "tool-input-start test-id weather signed",
                "tool-input-delta test-id {\"location\":\"San Francisco\"} signed",
                "tool-input-end test-id signed",
                "tool-call test-id weather {\"location\":\"San Francisco\"} signed",
                "finish ToolCalls STOP",
            },
            Sequence(parts, FixtureSignature("google-tool-call"), "google"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > tool-call-gemini3::should stream tool call with thoughtSignature", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_a_gemini3_function_call_with_its_thought_signature()
    {
        var parts = await Stream("gemini-2.5-flash", GoogleUpstream.Hello(), FixtureSse("google-tool-call-gemini3"), true).ConfigureAwait(false);
        Assert.Equal(
            new[]
            {
                "stream-start",
                "response-metadata QHiLaa6LBrb8vdIPoNztsAg",
                "tool-input-start test-id weather signed",
                "tool-input-delta test-id {\"location\":\"San Francisco\"} signed",
                "tool-input-end test-id signed",
                "tool-call test-id weather {\"location\":\"San Francisco\"} signed",
                "finish ToolCalls STOP",
            },
            Sequence(parts, FixtureSignature("google-tool-call-gemini3"), "google"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > streaming-tool-call-arguments::should stream partial function call arguments with parallel tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_partial_arguments_for_consecutive_function_calls()
    {
        var parts = await VertexStream(GoogleUpstream.Hello(), "google-stream-tool-call-arguments").ConfigureAwait(false);
        Assert.Equal(
            new[]
            {
                "stream-start",
                "response-metadata dqHOab6xGLzWodAPkPuViA4",
                "tool-input-start test-id getWeather signed",
                "tool-input-delta test-id {\"location\":\"Boston -",
                "tool-input-delta test-id \"} signed",
                "tool-input-end test-id signed",
                "tool-call test-id getWeather {\"location\":\"Boston\"} signed",
                "tool-input-start test-id getWeather -",
                "tool-input-delta test-id {\"location\":\"San Francisco -",
                "tool-input-delta test-id \"} -",
                "tool-input-end test-id -",
                "tool-call test-id getWeather {\"location\":\"San Francisco\"} -",
                "finish ToolCalls STOP",
            },
            Sequence(parts, FixtureSignature("google-stream-tool-call-arguments"), "vertex", "googleVertex"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > streaming-tool-call-array-arguments::should finalize streamed function call arguments when the final partialArgs chunk omits willContinue", Coverage = UpstreamCoverage.Covered)]
    public async Task Finishes_a_streamed_call_when_the_last_partial_argument_omits_will_continue()
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"streamFunctionCallArguments\":true}}");
        var parts = await VertexStream(options, "google-stream-tool-call-array-arguments-missing-terminal-function-call").ConfigureAwait(false);
        var expected = "{\"operations\":[{\"action\":\"add\",\"description\":\"Fresh red apple\",\"itemid\":\"apple_001\",\"price\":0.5},{\"action\":\"add\",\"description\":\"Ripe yellow banana\",\"itemid\":\"banana_001\",\"price\":0.3}]}";
        Assert.Equal(expected, string.Concat(parts.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta)));
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("writeItems", call.ToolName);
        Assert.Equal(expected, call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, parts.OfType<FinishStreamPart>().Single().FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > streaming-no-args-tool-call::should emit no-args function calls and preserve thoughtSignature alongside streamed-args calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_a_no_args_function_call_with_its_signature_beside_streamed_calls()
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"streamFunctionCallArguments\":true}}");
        var parts = await VertexStream(options, "google-stream-no-args-tool-call").ConfigureAwait(false);
        Assert.Equal(
            new[] { "read_theme {}", "read_screen {\"id\":\"A\"}", "read_screen {\"id\":\"B\"}", "read_screen {\"id\":\"C\"}" },
            parts.OfType<ToolCallStreamPart>().Select(call => call.ToolName + " " + call.ArgumentsJson));

        var readTheme = parts.OfType<ToolCallStreamPart>().First(call => call.ToolName == "read_theme");
        var signature = readTheme.ProviderMetadata!.Value.GetProperty("vertex").GetProperty("thoughtSignature").GetString()!;
        Assert.True(signature.Length > 100);
        Assert.Equal(signature, readTheme.ProviderMetadata!.Value.GetProperty("googleVertex").GetProperty("thoughtSignature").GetString());
        Assert.Equal("signed", Signed(parts.OfType<ToolInputStartStreamPart>().First(start => start.ToolName == "read_theme").ProviderMetadata, signature, "vertex", "googleVertex"));
        Assert.Contains(parts.OfType<ToolInputEndStreamPart>(), end => Signed(end.ProviderMetadata, signature, "vertex", "googleVertex") == "signed");
        Assert.All(parts.OfType<ToolInputDeltaStreamPart>(), delta => Assert.NotEqual("signed", Signed(delta.ProviderMetadata, signature, "vertex", "googleVertex")));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > streaming-tool-call-arguments-nested::should stream nested partial function call arguments into proper nested JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_nested_partial_arguments_into_nested_json()
    {
        var parts = await VertexStream(GoogleUpstream.Hello(), "google-vertex-stream-tool-call-arguments-nested.1").ConfigureAwait(false);
        var deltas = new[]
        {
            "{\"recipe\":{\"ingredients\":[{\"amount\":\"16 oz", "\",\"name\":\"Lasagna noodles",
            "\"},{\"amount\":\"1 lb", "\",\"name\":\"Ground beef",
            "\"},{\"amount\":\"15 oz", "\",\"name\":\"Ricotta cheese",
            "\"},{\"amount\":\"3 cups", "\",\"name\":\"Mozzarella cheese",
            "\"},{\"amount\":\"1/2 cup", "\",\"name\":\"Parmesan cheese",
            "\"},{\"amount\":\"24 oz", "\",\"name\":\"Tomato sauce",
            "\"},{\"amount\":\"1", "\",\"name\":\"Egg",
            "\"},{\"amount\":\"2 cloves", "\",\"name\":\"Garlic",
            "\"},{\"amount\":\"1 tsp", "\",\"name\":\"Salt",
            "\"},{\"amount\":\"1/2 tsp", "\",\"name\":\"Pepper",
            "\"}],\"name\":\"Lasagna",
            "\",\"steps\":[\"Preheat oven to 375°F (190°C).",
            "\",\"Cook lasagna noodles according to package directions, drain and set aside",
            ".",
            "\",\"Brown ground beef with minced garlic in a skillet. Drain fat and stir in tomato sauce. Simmer for 10 minutes.",
            "\",\"In a bowl, mix ricotta cheese, egg, salt, pepper, and Parmesan cheese.",
            "\",\"In a 9x13 baking dish, spread a",
            " thin layer of meat sauce.",
            "\",\"Layer noodles, ricotta mixture, mozzarella, and meat sauce. Repeat.",
            "\",\"Top with remaining mozzarella cheese.",
            "\",\"Cover with foil and bake for 25 minutes.",
            "\",\"Remove foil and bake for another 25 minutes until golden.",
            "\",\"Let stand for 15 minutes before serving.",
        };
        var expected = new List<string> { "stream-start", "response-metadata tjXVaYaxFISTq8YP_MWiyAo", "tool-input-start test-id cookRecipe signed" };
        expected.AddRange(deltas.Select(delta => "tool-input-delta test-id " + delta + " -"));
        expected.Add("tool-input-delta test-id \"]}} signed");
        expected.Add("tool-input-end test-id signed");
        expected.Add("tool-call test-id cookRecipe " + string.Concat(deltas) + "\"]}} signed");
        expected.Add("finish ToolCalls STOP");
        Assert.Equal(expected, Sequence(parts, FixtureSignature("google-vertex-stream-tool-call-arguments-nested.1"), "vertex", "googleVertex"));
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

    private static async Task<List<LanguageModelStreamPart>> VertexStream(LanguageModelCallOptions options, string fixture)
    {
        LastHandler = new RecordingHandler { ResponseText = FixtureSse(fixture), MediaType = "text/event-stream" };
        var settings = new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "test-api-key", GenerateId = () => "test-id" };
        var provider = GoogleVertexProvider.Create(settings, LastHandler);
        return await GoogleUpstream.Collect(provider.LanguageModel("gemini-pro").DoStreamAsync(options, CancellationToken.None)).ConfigureAwait(false);
    }

    private static string[] FixtureLines(string name)
    {
        return File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".chunks.txt"))
            .Where(line => line.Trim().Length > 0)
            .ToArray();
    }

    private static string FixtureSse(string name)
    {
        return string.Concat(FixtureLines(name).Select(Sse));
    }

    // The fixtures carry one thoughtSignature, on the part that names the first call.
    private static string FixtureSignature(string name)
    {
        using var document = JsonDocument.Parse(FixtureLines(name)[0]);
        return document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("thoughtSignature").GetString()!;
    }

    // One line per part, so a test can assert the full upstream sequence, ids and signatures included.
    private static List<string> Sequence(List<LanguageModelStreamPart> parts, string signature, params string[] keys)
    {
        return parts.Select(part => part switch
        {
            ResponseMetadataStreamPart metadata => "response-metadata " + metadata.Id,
            ToolInputStartStreamPart start => $"tool-input-start {start.Id} {start.ToolName} {Signed(start.ProviderMetadata, signature, keys)}",
            ToolInputDeltaStreamPart delta => $"tool-input-delta {delta.Id} {delta.Delta} {Signed(delta.ProviderMetadata, signature, keys)}",
            ToolInputEndStreamPart end => $"tool-input-end {end.Id} {Signed(end.ProviderMetadata, signature, keys)}",
            ToolCallStreamPart call => $"tool-call {call.ToolCallId} {call.ToolName} {call.ArgumentsJson} {Signed(call.ProviderMetadata, signature, keys)}",
            FinishStreamPart finish => $"finish {finish.FinishReason} {finish.RawFinishReason}",
            _ => part.Type,
        }).ToList();
    }

    private static string Signed(JsonElement? metadata, string signature, params string[] keys)
    {
        if (metadata == null)
        {
            return "-";
        }

        var names = metadata.Value.EnumerateObject().Select(property => property.Name).ToArray();
        var signed = names.SequenceEqual(keys) && keys.All(key => metadata.Value.GetProperty(key).GetProperty("thoughtSignature").GetString() == signature);
        return signed ? "signed" : metadata.Value.GetRawText();
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
