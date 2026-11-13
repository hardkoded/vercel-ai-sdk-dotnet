// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest("packages/cohere/src/cohere-chat-language-model.test.ts::doStream::handles a truncated stream (with error: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_a_truncated_stream(bool withError)
    {
        var events = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "cohere-text.chunks.txt"))
            .Where(line => line.Length > 0)
            .ToList();
        var firstText = events.FindIndex(line => JsonNode.Parse(line)!["type"]!.GetValue<string>() == "content-delta");
        Assert.True(firstText >= 0);

        var sse = new StringBuilder();
        foreach (var line in events.Take(firstText + 1))
        {
            sse.Append("data: ").Append(line).Append("\n\n");
        }

        if (withError)
        {
            sse.Append("data: {invalid-json}\n\n");
        }

        var bytes = Encoding.UTF8.GetBytes(sse.ToString());
        var handler = new ParityHandler(_ => ParityHandler.Bytes(bytes, "text/event-stream"));
        var model = CohereProvider.Create(new CohereOptions { ApiKey = "test-api-key" }, handler).LanguageModel("command-r-plus");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("you are a friendly bot!"), new UserModelMessage("Hello") },
        };

        var parts = await UpstreamChat.Read(await model.DoStreamAsync(options, CancellationToken.None));

        Assert.Contains(parts, part => part is TextDeltaStreamPart);
        Assert.Equal(withError ? 1 : 0, parts.Count(part => part is ErrorStreamPart));
        var finish = Assert.IsType<FinishStreamPart>(Assert.Single(parts, part => part is FinishStreamPart));
        Assert.Equal(withError ? FinishReason.Error : FinishReason.Interrupted, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/cohere/src/cohere-chat-language-model.test.ts::doStream > text::should return undefined token counts when message-end has no tokens", Coverage = UpstreamCoverage.Partial, Note = "NoCacheInputTokens and TextTokens stay null because Cohere reports no cache-write or reasoning counts. Upstream reports them equal to the totals.", Commit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f")]
    public async Task Should_return_undefined_token_counts_when_message_end_has_no_tokens()
    {
        const string rawUsage = "{\"billed_units\":{\"input_tokens\":88,\"output_tokens\":13},\"cached_tokens\":128}";
        var handler = CohereTestSupport.ChunkLines("{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":" + rawUsage + "}}");

        var parts = await CohereTestSupport.Stream(CohereTestSupport.Model(handler), new LanguageModelCallOptions { Prompt = CohereTestSupport.TestPrompt().Prompt, IncludeRawChunks = false });

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.NoCacheInputTokens);
        Assert.Equal(128, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
        JsonAssert.Equal(finish.Usage.Raw!.Value, rawUsage);
    }
}
