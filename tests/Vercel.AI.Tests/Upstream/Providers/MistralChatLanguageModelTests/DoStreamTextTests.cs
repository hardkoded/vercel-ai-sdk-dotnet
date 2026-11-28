// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doStream &gt; text</c>.</summary>
public sealed class DoStreamTextTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream > text::";

    [Fact]
    [UpstreamTest(Prefix + "should stream text", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text()
    {
        var handler = ChunksFixture("mistral-text");
        var model = Model(handler, "mistral-small-latest", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal("stream-start:0", parts[0]);
        Assert.Equal(
            "response-metadata:5319bd0299614c679a0068a4f2c8ffd0:mistral-small-latest:2026-01-22T13:32:00.000Z",
            parts[1]
        );
        Assert.Equal("text-start:0", parts[2]);
        Assert.StartsWith("text-delta:0:", parts[3]);
        var end = parts.IndexOf("text-end:0");
        Assert.True(end > 3);
        Assert.StartsWith("finish:Stop:stop:13/13/:8/8", parts[end + 1]);
        Assert.Equal(end + 2, parts.Count);
    }
}
