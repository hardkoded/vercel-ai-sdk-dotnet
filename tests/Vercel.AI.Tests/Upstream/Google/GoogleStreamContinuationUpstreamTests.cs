// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Gemini streamGenerateContent continuation tokens and long decode.</summary>
public sealed class GoogleStreamContinuationUpstreamTests
{
    private const string Prefix = "packages/google/src/google-language-model.test.ts::doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should expose the continuationToken from the final chunk on finish", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_continuationToken_from_the_final_chunk_on_finish()
    {
        var handler = new SequencedHandler().Sse(
            Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Partial\"}],\"role\":\"model\"},\"index\":0}]}")
            + Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"\"}],\"role\":\"model\"},\"finishReason\":\"MAX_TOKENS\",\"index\":0,\"continuationToken\":\"token-3\"}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":300,\"totalTokenCount\":310}}"));
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"continuationToken\":\"token-prev\"}}");

        var events = await Stream(handler, options);

        var finish = events.OfType<FinishStreamPart>().First();
        Assert.Equal(FinishReason.Length, finish.FinishReason);
        Assert.Equal("MAX_TOKENS", finish.RawFinishReason);
        Assert.Equal("token-3", finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("continuationToken").GetString());
        Assert.Equal("token-prev", (string?)JsonNode.Parse(handler.Bodies[0])!["continuationToken"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not expose continuationToken on finish when absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_expose_continuationToken_on_finish_when_absent()
    {
        var handler = new SequencedHandler().Sse(
            Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}"));

        var events = await Stream(handler, GoogleUpstream.Hello());

        var finish = events.OfType<FinishStreamPart>().First();
        Assert.False(finish.ProviderMetadata!.Value.GetProperty("google").TryGetProperty("continuationToken", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "should continue the stream automatically on CONTINUATION", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_continue_the_stream_automatically_on_CONTINUATION()
    {
        var handler = new SequencedHandler()
            .Sse(
                Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"index\":0}]}")
                + Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"\"}],\"role\":\"model\"},\"index\":0,\"continuationToken\":\"checkpoint-1\"}]}")
                + Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"\"}],\"role\":\"model\"},\"finishReason\":\"CONTINUATION\",\"index\":0,\"continuationToken\":\"token-1\"}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":5,\"thoughtsTokenCount\":20,\"totalTokenCount\":35}}"))
            .Sse(
                Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\", world\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":35,\"candidatesTokenCount\":7,\"totalTokenCount\":42}}"));
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 100_000;

        var events = await Stream(handler, options);

        Assert.Equal(2, handler.Bodies.Count);
        var expected = (JsonObject)JsonNode.Parse(handler.Bodies[0])!;
        expected["continuationToken"] = "token-1";
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(handler.Bodies[1])));

        Assert.Equal(new[] { "Hello", ", world" }, events.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Single(events.OfType<TextStartStreamPart>());

        var finish = events.OfType<FinishStreamPart>().First();
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("STOP", finish.RawFinishReason);
        Assert.Equal(45, finish.Usage.InputTokens);
        Assert.Equal(32, finish.Usage.OutputTokens);
        var google = finish.ProviderMetadata!.Value.GetProperty("google");
        Assert.Equal(10, google.GetProperty("initialPromptTokenCount").GetInt32());
        Assert.False(google.TryGetProperty("continuationToken", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not expose checkpoint tokens or initialPromptTokenCount on a stream that finishes in one request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_expose_checkpoint_tokens_or_initialPromptTokenCount_on_a_stream_that_finishes_in_one_request()
    {
        var handler = new SequencedHandler().Sse(
            Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"index\":0,\"continuationToken\":\"checkpoint-1\"}]}")
            + Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\", world\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":40000,\"totalTokenCount\":40010}}"));

        var events = await Stream(handler, GoogleUpstream.Hello());

        var google = events.OfType<FinishStreamPart>().First().ProviderMetadata!.Value.GetProperty("google");
        Assert.False(google.TryGetProperty("continuationToken", out _));
        Assert.False(google.TryGetProperty("initialPromptTokenCount", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "should finish with the last continuation token when continuing the stream fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_finish_with_the_last_continuation_token_when_continuing_the_stream_fails()
    {
        var handler = new SequencedHandler()
            .Sse(Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"finishReason\":\"CONTINUATION\",\"index\":0,\"continuationToken\":\"token-1\"}]}"))
            .Json("{\"error\":{\"code\":400,\"message\":\"Invalid continuation token\",\"status\":\"INVALID_ARGUMENT\"}}", HttpStatusCode.BadRequest);

        var events = await Stream(handler, GoogleUpstream.Hello());

        Assert.Contains(events, part => part is ErrorStreamPart);
        var finish = events.OfType<FinishStreamPart>().First();
        Assert.Equal(FinishReason.Length, finish.FinishReason);
        Assert.Equal("CONTINUATION", finish.RawFinishReason);
        Assert.Equal("token-1", finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("continuationToken").GetString());
    }

    [Fact]
    public async Task Should_stop_with_an_error_when_the_model_keeps_answering_with_CONTINUATION()
    {
        // The handler repeats its last response, so the model never stops continuing.
        var handler = new SequencedHandler()
            .Sse(Sse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"finishReason\":\"CONTINUATION\",\"index\":0,\"continuationToken\":\"token-1\"}]}"));

        var events = await Stream(handler, GoogleUpstream.Hello());

        Assert.Contains(events.OfType<ErrorStreamPart>(), part => part.Message.Contains("CONTINUATION"));
        Assert.Equal(GoogleLanguageModel.MaxContinuationHops + 1, handler.Bodies.Count);
        var finish = events.OfType<FinishStreamPart>().First();
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Equal("CONTINUATION", finish.RawFinishReason);
        Assert.Equal("token-1", finish.ProviderMetadata!.Value.GetProperty("google").GetProperty("continuationToken").GetString());
    }

    private static string Sse(string json)
    {
        return "data: " + json + "\n\n";
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(SequencedHandler handler, LanguageModelCallOptions options)
    {
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
        return await GoogleUpstream.Collect((await provider.LanguageModel("gemini-pro").DoStreamAsync(options, CancellationToken.None))).ConfigureAwait(false);
    }
}
