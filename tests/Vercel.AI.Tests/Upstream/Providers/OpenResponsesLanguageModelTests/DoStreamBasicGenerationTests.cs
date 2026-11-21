// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream &gt; basic generation</c>.</summary>
public sealed class DoStreamBasicGenerationTests
{
    private const string Prefix =
        "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream > basic generation::";

    [Fact]
    [UpstreamTest(Prefix + "should stream content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_content()
    {
        var parts = await Collect(Model(ChunksFixture("lmstudio-basic.1")), UpstreamChat.Prompt());

        JsonAssert.Equal(ToJson(parts), ExpectedParts("lmstudio-basic.1").ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward server response metadata from the response.created event",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b16915083dad4415e9553dfb116d2ff045bcfe6"
    )]
    public async Task Should_forward_server_response_metadata_from_the_response_created_event()
    {
        var capture = new UpstreamCapture();
        PrepareStreamChunks(
            capture,
            Obj(
                "{\"type\":\"response.created\",\"sequence_number\":0,\"response\":{\"id\":\"resp_123\",\"object\":\"response\",\"created_at\":1750000000,\"status\":\"in_progress\",\"model\":\"server-reported-model-id\",\"output\":[]}}"
            ),
            Obj(
                "{\"type\":\"response.output_item.added\",\"sequence_number\":1,\"output_index\":0,\"item\":{\"id\":\"msg_1\",\"type\":\"message\",\"status\":\"in_progress\",\"role\":\"assistant\",\"content\":[]}}"
            ),
            Obj(
                "{\"type\":\"response.output_text.delta\",\"sequence_number\":2,\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"delta\":\"Hello\"}"
            )
        );

        var parts = await Collect(CreateModel(capture, "test-model"), TestPrompt());

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("resp_123", metadata.Id);
        Assert.Equal("server-reported-model-id", metadata.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1750000000), metadata.Timestamp);
    }
}
