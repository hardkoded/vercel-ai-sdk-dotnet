// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doStream &gt; text</c>.</summary>
public sealed class DoStreamTextTests
{
    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doStream > text::should stream text", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text()
    {
        var model = StreamModel(FixtureStream("alibaba-text"));

        var parts = await ReadStream(model, UpstreamChat.Prompt());

        AssertStreamSnapshot(parts, "alibaba-text");
    }
}
