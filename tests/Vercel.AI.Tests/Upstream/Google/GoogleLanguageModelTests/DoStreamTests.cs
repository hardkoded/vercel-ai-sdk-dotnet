// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c> (the tests added for AUDIO response modalities and thought signatures on text).</summary>
public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should send videoMetadata on video file parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_videoMetadata_on_video_file_parts()
    {
        var handler = new RecordingHandler
        {
            MediaType = "text/event-stream",
            ResponseText = "data: {\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"done\"}]},\"finishReason\":\"STOP\"}]}\n\n",
        };
        var model = GoogleUpstream.Client(handler).LanguageModel("gemini-2.5-flash");
        var file = new FileContentPart("video/mp4", "https://www.youtube.com/watch?v=example", null, null)
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                """{"google":{"videoMetadata":{"startOffset":"0s","endOffset":"10.5s","fps":0.5}}}"""),
        };

        await GoogleUpstream.Collect(await model.DoStreamAsync(
            GoogleUpstream.Prompt(new UserModelMessage(new UserContentPart[] { file })),
            CancellationToken.None));

        using var body = JsonDocument.Parse(handler.Body);
        GoogleUpstream.JsonEqual(
            body.RootElement.GetProperty("contents"),
            """[{"role":"user","parts":[{"fileData":{"mimeType":"video/mp4","fileUri":"https://www.youtube.com/watch?v=example"},"videoMetadata":{"startOffset":"0s","endOffset":"10.5s","fps":0.5}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should only pass valid provider options with AUDIO responseModalities", Coverage = UpstreamCoverage.Covered)]
    public void Should_only_pass_valid_provider_options_with_AUDIO_responseModalities()
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"foo\":\"bar\",\"responseModalities\":[\"TEXT\",\"AUDIO\"]}}");

        var prepared = GoogleRequest.Prepare("gemini-pro", "google.generative-ai", options, true);

        GoogleUpstream.JsonEqual(prepared.Body["contents"]!, "[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello\"}]}]");
        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!, "{\"responseModalities\":[\"TEXT\",\"AUDIO\"]}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should stream thought signatures with reasoning and text parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_thought_signatures_with_reasoning_and_text_parts()
    {
        var safetyRatings = "[{\"category\":\"HARM_CATEGORY_SEXUALLY_EXPLICIT\",\"probability\":\"NEGLIGIBLE\"},{\"category\":\"HARM_CATEGORY_HATE_SPEECH\",\"probability\":\"NEGLIGIBLE\"},{\"category\":\"HARM_CATEGORY_HARASSMENT\",\"probability\":\"NEGLIGIBLE\"},{\"category\":\"HARM_CATEGORY_DANGEROUS_CONTENT\",\"probability\":\"NEGLIGIBLE\"}]";
        var handler = new RecordingHandler
        {
            MediaType = "text/event-stream",
            ResponseText =
                "data: {\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"I need to think about this.\",\"thought\":true,\"thoughtSignature\":\"reasoning_sig1\"}],\"role\":\"model\"},\"index\":0}]}\n\n" +
                "data: {\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Here is the answer.\",\"thoughtSignature\":\"text_sig1\"}],\"role\":\"model\"},\"index\":0,\"finishReason\":\"STOP\",\"safetyRatings\":" + safetyRatings + "}]}\n\n",
        };
        var model = GoogleUpstream.Client(handler).LanguageModel("gemini-pro");

        var parts = await GoogleUpstream.Collect(await model.DoStreamAsync(GoogleUpstream.Hello(), CancellationToken.None));

        Assert.Equal(
            new[] { "stream-start", "reasoning-start", "reasoning-delta", "reasoning-end", "text-start", "text-delta", "text-end", "finish" },
            parts.Select(part => part.Type).ToArray());
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var reasoningStart = Assert.IsType<ReasoningStartStreamPart>(parts[1]);
        var reasoningId = reasoningStart.Id;
        Assert.Equal("0", reasoningId);
        GoogleUpstream.JsonEqual(reasoningStart.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"reasoning_sig1\"}}");
        var reasoningDelta = Assert.IsType<ReasoningDeltaStreamPart>(parts[2]);
        Assert.Equal(reasoningId, reasoningDelta.Id);
        Assert.Equal("I need to think about this.", reasoningDelta.Delta);
        GoogleUpstream.JsonEqual(reasoningDelta.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"reasoning_sig1\"}}");
        var reasoningEnd = Assert.IsType<ReasoningEndStreamPart>(parts[3]);
        Assert.Equal(reasoningId, reasoningEnd.Id);
        Assert.Null(reasoningEnd.ProviderMetadata);
        var textStart = Assert.IsType<TextStartStreamPart>(parts[4]);
        var textId = textStart.Id;
        Assert.Equal("1", textId);
        GoogleUpstream.JsonEqual(textStart.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"text_sig1\"}}");
        var textDelta = Assert.IsType<TextDeltaStreamPart>(parts[5]);
        Assert.Equal(textId, textDelta.Id);
        Assert.Equal("Here is the answer.", textDelta.Delta);
        GoogleUpstream.JsonEqual(textDelta.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"text_sig1\"}}");
        var textEnd = Assert.IsType<TextEndStreamPart>(parts[6]);
        Assert.Equal(textId, textEnd.Id);
        Assert.Null(textEnd.ProviderMetadata);
        var finish = Assert.IsType<FinishStreamPart>(parts[7]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("STOP", finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.TotalTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
        Assert.Null(finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Null(finish.Usage.NoCacheInputTokens);
        Assert.Null(finish.Usage.TextTokens);
        Assert.Null(finish.Usage.Raw);
        GoogleUpstream.JsonEqual(
            finish.ProviderMetadata!.Value,
            "{\"google\":{\"promptFeedback\":null,\"groundingMetadata\":null,\"urlContextMetadata\":null,\"safetyRatings\":" + safetyRatings + ",\"usageMetadata\":null,\"finishMessage\":null,\"serviceTier\":null}}");
    }

    private const string Prefix = "packages/google/src/google-language-model.test.ts::doStream::";

    private const string ContinuationChunk = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}]},\"finishReason\":\"CONTINUATION\",\"continuationToken\":\"token-1\"}]}";

    private static string Sse(string json)
    {
        return "data: " + json + "\n\n";
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(SequencedHandler handler, LanguageModelCallOptions options)
    {
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
        return await GoogleUpstream.Collect(await provider.LanguageModel("gemini-pro").DoStreamAsync(options, CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData("BLOCK_REASON_UNSPECIFIED")]
    [InlineData("BLOCKED_REASON_UNSPECIFIED")]
    [UpstreamTest(Prefix + "should not classify the default prompt block reason %j as a content filter", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_classify_the_default_prompt_block_reason_as_a_content_filter(string blockReason)
    {
        var handler = new SequencedHandler().Sse(
            Sse("{\"candidates\":[],\"promptFeedback\":{\"blockReason\":" + JsonSerializer.Serialize(blockReason) + "},\"usageMetadata\":{\"promptTokenCount\":9,\"totalTokenCount\":9}}"));

        var parts = await Stream(handler, GoogleUpstream.Hello());

        var finish = parts.OfType<FinishStreamPart>().First();
        Assert.Equal(FinishReason.Interrupted, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "preserves grounding per streamed request (terminal grounding: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_grounding_per_streamed_request(bool terminalGrounding)
    {
        var first = (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-continuation-grounding-1.json")))!;
        var last = (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-continuation-grounding-2.json")))!;
        var firstGrounding = first["candidates"]![0]!["groundingMetadata"]!.DeepClone();
        if (terminalGrounding)
        {
            last["candidates"]![0]!["groundingMetadata"] = firstGrounding.DeepClone();
        }

        var handler = new SequencedHandler().Sse(Sse(first.ToJsonString())).Sse(Sse(last.ToJsonString()));
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 33_280;

        var parts = await Stream(handler, options);
        var finish = parts.OfType<FinishStreamPart>().First();

        Assert.Equal(2, handler.Bodies.Count);
        var expected = new JsonArray(firstGrounding.DeepClone(), last["candidates"]![0]!["groundingMetadata"]?.DeepClone());
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("groundingMetadataPerRequest").GetRawText())));
        Assert.Equal(33_246, finish.Usage.InputTokens);
        Assert.Equal(33_306, finish.Usage.OutputTokens);
    }

    [Theory]
    [InlineData(null, FinishReason.Interrupted)]
    [InlineData("STOP", FinishReason.Stop)]
    [InlineData("MAX_TOKENS", FinishReason.Length)]
    [InlineData("SAFETY", FinishReason.ContentFilter)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [UpstreamTest(Prefix + "maps a stream ending with finishReason %j to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_a_stream_ending_with_finishReason(string? raw, FinishReason unified)
    {
        var candidate = new JsonObject
        {
            ["content"] = new JsonObject { ["role"] = "model", ["parts"] = new JsonArray(new JsonObject { ["text"] = "Partial answer" }) },
            ["index"] = 0,
        };
        if (raw != null)
        {
            candidate["finishReason"] = raw;
        }

        var handler = new SequencedHandler().Sse(Sse(new JsonObject { ["candidates"] = new JsonArray(candidate) }.ToJsonString()));

        var parts = await Stream(handler, GoogleUpstream.Hello());

        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part => part.Delta == "Partial answer");
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(raw, finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports interruption when an automatically continued stream ends without a finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Reports_interruption_when_an_automatically_continued_stream_ends_without_a_finish_reason()
    {
        var handler = new SequencedHandler()
            .Sse(Sse(ContinuationChunk))
            .Sse(Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\", partial\"}]}}]}"));

        var parts = await Stream(handler, GoogleUpstream.Hello());

        Assert.Equal(2, handler.Bodies.Count);
        Assert.Equal(new[] { "Hello", ", partial" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Interrupted, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("partial")]
    [InlineData("complete")]
    [InlineData("request-error")]
    [UpstreamTest(Prefix + "tracks a %s continuation in stream order under backpressure", Coverage = UpstreamCoverage.Partial, Note = "The stream is pull-based, so the first response's CONTINUATION chunk is never buffered ahead of the new response and the test does not wait for the second request before reading.")]
    public async Task Tracks_a_continuation_in_stream_order_under_backpressure(string ending)
    {
        var firstChunk = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}]},\"finishReason\":\"CONTINUATION\",\"continuationToken\":\"token-1\"}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":5}}";
        var continuedChunks = new List<string>();
        if (ending is "partial" or "complete")
        {
            var finishReason = ending == "complete" ? ",\"finishReason\":\"STOP\"" : string.Empty;
            continuedChunks.Add("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\", world\"}]}" + finishReason + "}],\"usageMetadata\":{\"promptTokenCount\":20,\"candidatesTokenCount\":2}}");
        }

        var handler = new SequencedHandler().Sse(Sse(firstChunk));
        if (ending == "request-error")
        {
            handler.Json("{\"error\":{\"code\":400,\"message\":\"Invalid continuation token\"}}", HttpStatusCode.BadRequest);
        }
        else
        {
            handler.Sse(string.Concat(continuedChunks.Select(Sse)));
        }

        var options = GoogleUpstream.Hello();
        options.IncludeRawChunks = true;
        var parts = await Stream(handler, options);

        Assert.Equal(2, handler.Bodies.Count);
        Assert.Equal(continuedChunks.Count > 0 ? new[] { "Hello", ", world" } : new[] { "Hello" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Equal(ending == "request-error" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var raw = parts.OfType<RawStreamPart>().Select(part => JsonNode.Parse(part.RawJson)).ToList();
        var expectedRaw = new[] { firstChunk }.Concat(continuedChunks).Select(chunk => JsonNode.Parse(chunk)).ToList();
        Assert.Equal(expectedRaw.Count, raw.Count);
        for (var index = 0; index < raw.Count; index++)
        {
            Assert.True(JsonNode.DeepEquals(expectedRaw[index], raw[index]));
        }

        var finish = parts.OfType<FinishStreamPart>().First();
        var (expectedReason, expectedRawReason) = ending switch
        {
            "complete" => (FinishReason.Stop, "STOP"),
            "request-error" => (FinishReason.Length, "CONTINUATION"),
            _ => (FinishReason.Interrupted, (string?)null),
        };
        Assert.Equal(expectedReason, finish.FinishReason);
        Assert.Equal(expectedRawReason, finish.RawFinishReason);
        Assert.Equal(continuedChunks.Count > 0 ? 30 : 10, finish.Usage.InputTokens);
        Assert.Equal(continuedChunks.Count > 0 ? 7 : 5, finish.Usage.OutputTokens);
        if (ending == "request-error")
        {
            Assert.Equal("token-1", finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("continuationToken").GetString());
        }
    }
}
