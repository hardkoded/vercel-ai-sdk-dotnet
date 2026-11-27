// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.PipeUiMessageStreamToResponse;

/// <summary>
/// Port of <c>pipe-ui-message-stream-to-response.test.ts</c> &gt; <c>pipeUIMessageStreamToResponse</c>.
/// The other tests of that <c>describe</c> are in <c>UiMessageResponseTests</c>.
/// </summary>
public sealed class PipeUIMessageStreamToResponseTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/pipe-ui-message-stream-to-response.test.ts::pipeUIMessageStreamToResponse::";
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    [Fact]
    [UpstreamTest(
        Prefix + "should write an opening comment when keep-alives are enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_write_an_opening_comment_when_keep_alives_are_enabled()
    {
        var response = new ServerResponse();

        var pipe = UIMessageStreamResponses.PipeUIMessageStreamToResponse(
            response,
            ReadableStream<JsonObject>.FromArray([
                new JsonObject
                {
                    ["type"] = "text-delta",
                    ["id"] = "1",
                    ["delta"] = "test-data",
                },
            ]),
            new UIMessageStreamResponseInit { KeepAliveMs = 100 }
        );

        await response.WaitForEndAsync().WaitAsync(Limit);
        await pipe.WaitAsync(Limit);

        Assert.Equal(": stream-open\n\n", response.GetDecodedChunks()[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "can pipe a stream created by toUIMessageStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Can_pipe_a_stream_created_by_toUIMessageStream()
    {
        var response = new ServerResponse();

        var pipe = UIMessageStreamResponses.PipeUIMessageStreamToResponse(
            response,
            ReadableStream<JsonObject>.From(
                UIMessageStreamConverter.ToUIMessageStream(
                    ToAsync([
                        new StartPart(),
                        new TextStartPart("t1"),
                        new TextDeltaPart("t1", "Hello"),
                        new TextEndPart("t1"),
                    ]),
                    new UIMessageStreamOptions { SendStart = false }
                )
            )
        );

        await response.WaitForEndAsync().WaitAsync(Limit);
        await pipe.WaitAsync(Limit);

        Assert.Equal(
            [
                "data: {\"type\":\"text-start\",\"id\":\"t1\"}\n\n",
                "data: {\"type\":\"text-delta\",\"id\":\"t1\",\"delta\":\"Hello\"}\n\n",
                "data: {\"type\":\"text-end\",\"id\":\"t1\"}\n\n",
                "data: [DONE]\n\n",
            ],
            response.GetDecodedChunks()
        );
    }

    private static async IAsyncEnumerable<TextStreamPart> ToAsync(
        IReadOnlyList<TextStreamPart> parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }
}
