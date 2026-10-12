// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream &gt; reasoning with tool call</c>.</summary>
public sealed class DoStreamReasoningWithToolCallTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream > reasoning with tool call::";

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning and tool call content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_and_tool_call_content()
    {
        var parts = await Collect(Model(ChunksFixture("lmstudio-tool-call.2")), UpstreamChat.Prompt());

        JsonAssert.Equal(ToJson(parts), ExpectedParts("lmstudio-tool-call.2").ToJsonString());
    }
}
