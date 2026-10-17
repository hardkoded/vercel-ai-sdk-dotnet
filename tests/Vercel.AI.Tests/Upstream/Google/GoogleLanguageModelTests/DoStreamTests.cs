// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
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
        var first = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"I need to think about this.\",\"thought\":true,\"thoughtSignature\":\"reasoning_sig1\"}],\"role\":\"model\"},\"index\":0}]}";
        var second = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Here is the answer.\",\"thoughtSignature\":\"text_sig1\"}],\"role\":\"model\"},\"index\":0,\"finishReason\":\"STOP\"}]}";
        var handler = new RecordingHandler { ResponseText = "data: " + first + "\n\ndata: " + second + "\n\n", MediaType = "text/event-stream" };
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler);

        var chunks = await GoogleUpstream.Collect(await provider.LanguageModel("gemini-pro").DoStreamAsync(GoogleUpstream.Hello(), CancellationToken.None));

        Assert.Equal(["stream-start", "reasoning-start", "reasoning-delta", "reasoning-end", "text-start", "text-delta", "text-end", "finish"], chunks.Select(part => part.Type).ToList());
        var reasoningStart = Assert.IsType<ReasoningStartStreamPart>(chunks[1]);
        var reasoningDelta = Assert.IsType<ReasoningDeltaStreamPart>(chunks[2]);
        var reasoningEnd = Assert.IsType<ReasoningEndStreamPart>(chunks[3]);
        var textStart = Assert.IsType<TextStartStreamPart>(chunks[4]);
        var textDelta = Assert.IsType<TextDeltaStreamPart>(chunks[5]);
        var textEnd = Assert.IsType<TextEndStreamPart>(chunks[6]);
        Assert.Equal(["0", "0", "0"], [reasoningStart.Id, reasoningDelta.Id, reasoningEnd.Id]);
        Assert.Equal(["1", "1", "1"], [textStart.Id, textDelta.Id, textEnd.Id]);
        Assert.Equal("I need to think about this.", reasoningDelta.Delta);
        Assert.Equal("Here is the answer.", textDelta.Delta);
        GoogleUpstream.JsonEqual(reasoningStart.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"reasoning_sig1\"}}");
        GoogleUpstream.JsonEqual(reasoningDelta.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"reasoning_sig1\"}}");
        GoogleUpstream.JsonEqual(textStart.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"text_sig1\"}}");
        GoogleUpstream.JsonEqual(textDelta.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"text_sig1\"}}");
        var finish = Assert.IsType<FinishStreamPart>(chunks[7]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("STOP", finish.RawFinishReason);
    }
}
