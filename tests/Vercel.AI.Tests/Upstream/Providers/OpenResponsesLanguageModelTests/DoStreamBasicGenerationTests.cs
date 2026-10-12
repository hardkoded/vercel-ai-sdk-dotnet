// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream &gt; basic generation</c>.</summary>
public sealed class DoStreamBasicGenerationTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream > basic generation::";

    [Fact]
    [UpstreamTest(Prefix + "should stream content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_content()
    {
        var parts = await Collect(Model(ChunksFixture("lmstudio-basic.1")), UpstreamChat.Prompt());

        JsonAssert.Equal(ToJson(parts), ExpectedParts("lmstudio-basic.1").ToJsonString());
    }
}
