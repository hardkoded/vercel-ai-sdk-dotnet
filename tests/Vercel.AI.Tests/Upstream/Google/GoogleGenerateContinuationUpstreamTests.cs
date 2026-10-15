// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Gemini generateContent continuation tokens and long decode.</summary>
public sealed class GoogleGenerateContinuationUpstreamTests
{
    private const string Prefix = "packages/google/src/google-language-model.test.ts::doGenerate::";

    private const string LongDecodePrefix = "packages/google/src/google-language-model.test.ts::doGenerate > long decode continuation::";

    private const string StopResponse = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"test response\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}";

    private const string ContinuationResponse = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"finishReason\":\"CONTINUATION\",\"index\":0,\"continuationToken\":\"token-1\"}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":5,\"thoughtsTokenCount\":20,\"totalTokenCount\":35}}";

    [Fact]
    [UpstreamTest(Prefix + "should send providerOptions.google.continuationToken as a top-level body field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_providerOptions_google_continuationToken_as_a_top_level_body_field()
    {
        var handler = new SequencedHandler().Json(StopResponse);
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 600;
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"continuationToken\":\"token-1\"}}");

        await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!;
        Assert.Equal("token-1", (string?)body["continuationToken"]);
        Assert.Equal(600, (int?)body["generationConfig"]!["maxOutputTokens"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit continuationToken from the request body when not set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_continuationToken_from_the_request_body_when_not_set()
    {
        var handler = new SequencedHandler().Json(StopResponse);

        await Model(handler).DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.False(((JsonObject)JsonNode.Parse(handler.Bodies[0])!).ContainsKey("continuationToken"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose the candidate continuationToken on a MAX_TOKENS response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_candidate_continuationToken_on_a_MAX_TOKENS_response()
    {
        var handler = new SequencedHandler().Json("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Partial\"}],\"role\":\"model\"},\"finishReason\":\"MAX_TOKENS\",\"index\":0,\"continuationToken\":\"token-2\"}],\"usageMetadata\":{\"promptTokenCount\":1,\"candidatesTokenCount\":2,\"totalTokenCount\":3}}");

        var result = await Model(handler).DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.Equal(FinishReason.Length, result.FinishReason);
        Assert.Equal("MAX_TOKENS", result.RawFinishReason);
        Assert.Equal("token-2", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("continuationToken").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not expose continuationToken in provider metadata when absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_expose_continuationToken_in_provider_metadata_when_absent()
    {
        var handler = new SequencedHandler().Json(StopResponse);

        var result = await Model(handler).DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.False(result.ProviderMetadata!.Value.GetProperty("google").TryGetProperty("continuationToken", out _));
    }

    [Fact]
    [UpstreamTest(LongDecodePrefix + "should resend the same request with the continuation token on CONTINUATION", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resend_the_same_request_with_the_continuation_token_on_CONTINUATION()
    {
        var handler = new SequencedHandler()
            .Json(ContinuationResponse)
            .Json("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\", world\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":35,\"candidatesTokenCount\":7,\"totalTokenCount\":42}}");
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 100_000;

        var result = await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(2, handler.Bodies.Count);
        var firstBody = (JsonObject)JsonNode.Parse(handler.Bodies[0])!;
        var secondBody = (JsonObject)JsonNode.Parse(handler.Bodies[1])!;
        Assert.False(firstBody.ContainsKey("continuationToken"));
        var expected = (JsonObject)firstBody.DeepClone();
        expected["continuationToken"] = "token-1";
        Assert.True(JsonNode.DeepEquals(expected, secondBody));
        Assert.Equal(100_000, (int?)secondBody["generationConfig"]!["maxOutputTokens"]);

        Assert.Collection(
            result.Content,
            part => Assert.Equal("Hello", Assert.IsAssignableFrom<GeneratedText>(part).Text),
            part => Assert.Equal(", world", Assert.IsAssignableFrom<GeneratedText>(part).Text));
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("STOP", result.RawFinishReason);
        Assert.Equal(45, result.Usage.InputTokens);
        Assert.Equal(32, result.Usage.OutputTokens);
        Assert.Equal(12, result.Usage.TextTokens);
        Assert.Equal(20, result.Usage.ReasoningTokens);
        var google = result.ProviderMetadata!.Value.GetProperty("google");
        Assert.False(google.TryGetProperty("continuationToken", out _));
        Assert.Equal(10, google.GetProperty("initialPromptTokenCount").GetInt32());
    }

    [Fact]
    [UpstreamTest(LongDecodePrefix + "should return the continuation token when the budget is used up after continuing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_the_continuation_token_when_the_budget_is_used_up_after_continuing()
    {
        var handler = new SequencedHandler()
            .Json(ContinuationResponse)
            .Json("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\", wor\"}],\"role\":\"model\"},\"finishReason\":\"MAX_TOKENS\",\"index\":0,\"continuationToken\":\"token-2\"}]}");

        var result = await Model(handler).DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.Equal(2, handler.Bodies.Count);
        Assert.Equal(FinishReason.Length, result.FinishReason);
        Assert.Equal("MAX_TOKENS", result.RawFinishReason);
        Assert.Equal("token-2", result.ProviderMetadata!.Value.GetProperty("google").GetProperty("continuationToken").GetString());
    }

    private static ILanguageModel Model(SequencedHandler handler)
    {
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
        return provider.LanguageModel("gemini-pro");
    }
}
