// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>error handling</c>.</summary>
public sealed class DoStreamErrorHandlingTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doStream > error handling::should handle unparsable stream parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_unparsable_stream_parts()
    {
        var options = TestPrompt();
        options.IncludeRawChunks = false;

        var parts = await Stream(Model(Sse("event: foo-message\ndata: {unparsable}\n\n")), options);

        Assert.Equal(3, parts.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        Assert.IsType<ErrorStreamPart>(parts[1]);
        var finish = Assert.IsType<FinishStreamPart>(parts[2]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.Raw);
    }
}
