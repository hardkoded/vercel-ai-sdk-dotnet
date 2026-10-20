// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c> (the tests added for AUDIO response modalities and thought signatures on text).</summary>
public sealed class DoStreamTests
{
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::preserves grounding per streamed request (terminal grounding: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_grounding_per_streamed_request(bool terminalGrounding)
    {
        var (first, last) = DoGenerateLongDecodeContinuationTests.ReadContinuationGroundingResponses();
        if (terminalGrounding)
        {
            last["candidates"]![0]!["groundingMetadata"] = first["candidates"]![0]!["groundingMetadata"]!.DeepClone();
        }

        var handler = new SequencedHandler()
            .Sse("data: " + first.ToJsonString() + "\n\n")
            .Sse("data: " + last.ToJsonString() + "\n\n");
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 33_280;
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);

        var events = await GoogleUpstream.Collect(await provider.LanguageModel("gemini-pro").DoStreamAsync(options, CancellationToken.None));
        var finish = events.OfType<FinishStreamPart>().First();

        Assert.Equal(2, handler.Bodies.Count);
        var lastGrounding = last["candidates"]![0]!["groundingMetadata"];
        var expected = new JsonArray(first["candidates"]![0]!["groundingMetadata"]!.DeepClone(), lastGrounding?.DeepClone());
        GoogleUpstream.JsonEqual(finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("groundingMetadataPerRequest"), expected.ToJsonString());
        Assert.Equal(33_246, finish.Usage.InputTokens);
        Assert.Equal(33_306, finish.Usage.OutputTokens);
    }
}
