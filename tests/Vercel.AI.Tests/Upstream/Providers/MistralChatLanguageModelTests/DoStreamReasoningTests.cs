// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doStream &gt; reasoning</c>.</summary>
public sealed class DoStreamReasoningTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream > reasoning::";

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning()
    {
        var handler = ChunksFixture("mistral-reasoning");
        var model = Model(handler, "magistral-medium-2507", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "stream-start:0",
                "response-metadata:a4e29c5b82f94d67b23e108a7c9df6e1:magistral-medium-2507:2026-01-22T13:35:12.000Z",
                "reasoning-start:id-0",
                "reasoning-delta:id-0:The user is asking",
                "reasoning-delta:id-0: for 2+2. This is basic arithmetic. 2+2=4.",
                "reasoning-end:id-0",
                "text-start:0",
                "text-delta:0:2 + 2 = 4",
                "text-end:0",
                "finish:Stop:stop:10/10/:46/46",
            },
            parts
        );
    }
}
