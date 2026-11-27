// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.CreateUiMessageStreamResponse;

/// <summary>
/// Port of the <c>consumeSseStream</c> tests of <c>create-ui-message-stream-response.test.ts</c> &gt; <c>createUIMessageStreamResponse</c>.
/// The other tests of that <c>describe</c> are in <c>UiMessageResponseTests</c>.
/// </summary>
public sealed class CreateUiMessageStreamResponseConsumeSseStreamTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/create-ui-message-stream-response.test.ts::createUIMessageStreamResponse::";
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    [Fact]
    [UpstreamTest(
        Prefix + "should call consumeSseStream with a teed stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_consumeSseStream_with_a_teed_stream()
    {
        var consumedData = new List<string>();
        var consumed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;

        var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<JsonObject>.FromArray([Delta("test-data-1"), Delta("test-data-2")]),
            new UIMessageStreamResponseInit
            {
                Status = 200,
                ConsumeSseStream = async stream =>
                {
                    calls++;
                    consumedData.AddRange(await stream.ToArrayAsync());
                    consumed.SetResult();
                },
            }
        );

        // Verify consumeSseStream was called
        Assert.Equal(1, calls);

        // Verify the response stream still works correctly
        var expected = new[]
        {
            "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"test-data-1\"}\n\n",
            "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"test-data-2\"}\n\n",
            "data: [DONE]\n\n",
        };
        Assert.Equal(expected, await Decode(response));

        // Wait for consumeSseStream to complete
        await consumed.Task.WaitAsync(Limit);

        // Verify consumeSseStream received the same data
        Assert.Equal(expected, consumedData);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not block the response when consumeSseStream takes time",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_block_the_response_when_consumeSseStream_takes_time()
    {
        var consumeResolve = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var calls = 0;

        var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<JsonObject>.FromArray([Delta("test-data")]),
            new UIMessageStreamResponseInit
            {
                Status = 200,
                ConsumeSseStream = async stream =>
                {
                    calls++;

                    // Consume the stream but wait for external resolution
                    await stream.ToArrayAsync();
                    await consumeResolve.Task;
                },
            }
        );

        // The response should be immediately available even though consumeSseStream hasn't finished
        Assert.IsType<UIMessageStreamResponse>(response);
        Assert.Equal(200, response.Status);

        // The response body should be readable immediately
        Assert.Equal(
            [
                "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"test-data\"}\n\n",
                "data: [DONE]\n\n",
            ],
            await Decode(response)
        );

        // Verify consumeSseStream was called but may still be running
        Assert.Equal(1, calls);

        // Now resolve the consumeSseStream
        consumeResolve.SetResult();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle synchronous consumeSseStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_synchronous_consumeSseStream()
    {
        var consumedData = new List<string>();
        var calls = 0;

        var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<JsonObject>.FromArray([Delta("sync-test")]),
            new UIMessageStreamResponseInit
            {
                Status = 200,

                // Synchronous consumption (not returning a task)
                ConsumeSseStream = stream =>
                {
                    calls++;
                    _ = Task.Run(async () => consumedData.AddRange(await stream.ToArrayAsync()));
                    return null;
                },
            }
        );

        Assert.Equal(1, calls);

        Assert.Equal(
            [
                "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"sync-test\"}\n\n",
                "data: [DONE]\n\n",
            ],
            await Decode(response)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle consumeSseStream errors gracefully",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_consumeSseStream_errors_gracefully()
    {
        var calls = 0;

        var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<JsonObject>.FromArray([Delta("error-test")]),
            new UIMessageStreamResponseInit
            {
                Status = 200,
                ConsumeSseStream = _ =>
                {
                    calls++;
                    return Task.FromException(
                        new InvalidOperationException("consumeSseStream error")
                    );
                },
            }
        );

        // The response should still work even if consumeSseStream fails
        Assert.Equal(
            [
                "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"error-test\"}\n\n",
                "data: [DONE]\n\n",
            ],
            await Decode(response)
        );

        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send opening and keep-alive comments for an idle stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_opening_and_keep_alive_comments_for_an_idle_stream()
    {
        var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
            new ReadableStream<JsonObject>(),
            new UIMessageStreamResponseInit { KeepAliveMs = 100 }
        );
        var reader = response.Body.GetReader();

        var open = await reader.ReadAsync().WaitAsync(Limit);
        Assert.False(open.Done);
        Assert.Equal(": stream-open\n\n", Encoding.UTF8.GetString(open.Value));

        var keepAlive = await reader.ReadAsync().WaitAsync(Limit);
        Assert.False(keepAlive.Done);
        Assert.Equal(": keep-alive\n\n", Encoding.UTF8.GetString(keepAlive.Value));

        await reader.CancelAsync();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "can respond with a stream created by toUIMessageStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Can_respond_with_a_stream_created_by_toUIMessageStream()
    {
        var response = UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<JsonObject>.From(
                UIMessageStreamConverter.ToUIMessageStream(
                    ToAsync([
                        new StartPart(),
                        new TextStartPart("t1"),
                        new TextDeltaPart("t1", "Hello"),
                        new TextEndPart("t1"),
                    ]),
                    new UIMessageStreamOptions { GenerateMessageId = () => "msg-123" }
                )
            ),
            new UIMessageStreamResponseInit { Status = 200 }
        );

        Assert.Equal(
            [
                "data: {\"type\":\"start\",\"messageId\":\"msg-123\"}\n\n",
                "data: {\"type\":\"text-start\",\"id\":\"t1\"}\n\n",
                "data: {\"type\":\"text-delta\",\"id\":\"t1\",\"delta\":\"Hello\"}\n\n",
                "data: {\"type\":\"text-end\",\"id\":\"t1\"}\n\n",
                "data: [DONE]\n\n",
            ],
            await Decode(response)
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

    private static JsonObject Delta(string delta) =>
        new()
        {
            ["type"] = "text-delta",
            ["id"] = "1",
            ["delta"] = delta,
        };

    private static async Task<List<string>> Decode(UIMessageStreamResponse response)
    {
        var chunks = await response.Body.ToArrayAsync();
        return chunks.ConvertAll(chunk => Encoding.UTF8.GetString(chunk));
    }
}
