// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>usage</c>.</summary>
public sealed class DoGenerateUsageTests
{
    private const string UpstreamCommit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f";
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::doGenerate > usage::";
    private const string Note = "NoCacheInputTokens and TextTokens stay null because Cohere reports no cache-write or reasoning counts. Upstream reports them equal to the totals.";

    [Fact]
    [UpstreamTest(Chat + "should return undefined token counts when usage is absent", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_return_undefined_token_counts_when_usage_is_absent()
    {
        var handler = JsonBody("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\"}");

        var usage = (await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None)).Usage;

        AssertUndefinedCounts(usage);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest(Chat + "should return undefined token counts for an empty usage object", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_return_undefined_token_counts_for_an_empty_usage_object()
    {
        var handler = JsonBody("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":{}}");

        var usage = (await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None)).Usage;

        AssertUndefinedCounts(usage);
        Assert.Null(usage.CacheReadTokens);
        JsonAssert.Equal(usage.Raw!.Value, "{}");
    }

    [Fact]
    [UpstreamTest(Chat + "should not treat billed units as token counts", Coverage = UpstreamCoverage.Partial, Note = Note, Commit = UpstreamCommit)]
    public async Task Should_not_treat_billed_units_as_token_counts()
    {
        var handler = JsonBody("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"cached_tokens\":448}}");

        var usage = (await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None)).Usage;

        // billed units measure something else than tokens, so no token counts are reported and cached
        // tokens are not subtracted from them.
        AssertUndefinedCounts(usage);
        Assert.Equal(448, usage.CacheReadTokens);
        JsonAssert.Equal(usage.Raw!.Value, "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"cached_tokens\":448}");
    }

    private static void AssertUndefinedCounts(LanguageModelUsage usage)
    {
        Assert.Null(usage.InputTokens);
        Assert.Null(usage.OutputTokens);
        Assert.Null(usage.NoCacheInputTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Null(usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
    }
}
