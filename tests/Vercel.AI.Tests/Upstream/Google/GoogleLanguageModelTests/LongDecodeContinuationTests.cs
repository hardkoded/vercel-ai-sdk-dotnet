// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>long decode continuation</c>.</summary>
public sealed class LongDecodeContinuationTests
{
    private const string Prefix = "packages/google/src/google-language-model.test.ts::doGenerate > long decode continuation::";

    private const string ContinuationResponse = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}],\"role\":\"model\"},\"finishReason\":\"CONTINUATION\",\"index\":0,\"continuationToken\":\"token-1\"}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":5,\"thoughtsTokenCount\":20,\"totalTokenCount\":35}}";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "preserves grounding per request (terminal grounding: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_grounding_per_request(bool terminalGrounding)
    {
        var first = (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-continuation-grounding-1.json")))!;
        var last = (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-continuation-grounding-2.json")))!;
        var firstGrounding = first["candidates"]![0]!["groundingMetadata"]!.DeepClone();
        if (terminalGrounding)
        {
            last["candidates"]![0]!["groundingMetadata"] = firstGrounding.DeepClone();
        }

        var handler = new SequencedHandler().Json(first.ToJsonString()).Json(last.ToJsonString());
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 33_280;

        var result = await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(2, handler.Bodies.Count);
        var google = result.ProviderMetadata!.Value.GetProperty("google");
        var lastGrounding = last["candidates"]![0]!["groundingMetadata"]?.DeepClone();
        var expected = new JsonArray(firstGrounding.DeepClone(), lastGrounding?.DeepClone());
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(google.GetProperty("groundingMetadataPerRequest").GetRawText())));
        Assert.True(JsonNode.DeepEquals(lastGrounding, JsonNode.Parse(google.GetProperty("groundingMetadata").GetRawText())));
        Assert.Equal(33_246, result.Usage.InputTokens);
        Assert.Equal(33_306, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resend the same request with the continuation token on CONTINUATION", Coverage = UpstreamCoverage.Covered)]
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
    [UpstreamTest(Prefix + "should return the continuation token when the budget is used up after continuing", Coverage = UpstreamCoverage.Covered)]
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
