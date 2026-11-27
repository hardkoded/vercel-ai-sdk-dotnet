// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS0618 // upstream tests the deprecated onFinish alias

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.CreateUiMessageStream;

/// <summary>Port of <c>create-ui-message-stream.test.ts</c> &gt; <c>createUIMessageStream</c>.</summary>
public sealed class CreateUIMessageStreamTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/create-ui-message-stream.test.ts::createUIMessageStream::";

    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    private static JsonObject Chunk(string json) => JsonAssert.ParseObject(json);

    private static void AssertChunks(string expected, IReadOnlyList<JsonObject> actual)
    {
        var array = new JsonArray();
        foreach (var chunk in actual)
        {
            array.Add(chunk.DeepClone());
        }

        JsonAssert.Equal(array, expected);
    }

    private static Task<List<JsonObject>> Read(ReadableStream<JsonObject> stream) =>
        stream.ToArrayAsync().WaitAsync(Limit);

    private static ReadableStream<JsonObject> ControlledStream(
        out ReadableStreamController<JsonObject> controller
    )
    {
        ReadableStreamController<JsonObject> captured = null!;
        var stream = new ReadableStream<JsonObject>(start: c => captured = c);
        controller = captured;
        return stream;
    }

    private static UIMessage Original(
        string id,
        UIMessageRole role,
        params UIMessagePart[] parts
    ) => new(role, parts) { Id = id };

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void AssertDoneText(UIMessagePart part, string text)
    {
        var textPart = Assert.IsType<TextUIPart>(part);
        Assert.Equal("done", textPart.State);
        Assert.Equal(text, textPart.Text);
        Assert.Null(textPart.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send data stream part and close the stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_data_stream_part_and_close_the_stream()
    {
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"text-start","id":"1"}"""));
                    writer.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
                    writer.Write(Chunk("""{"type":"text-end","id":"1"}"""));
                    return null;
                },
            }
        );

        AssertChunks(
            """
            [
              {"id":"1","type":"text-start"},
              {"delta":"1a","id":"1","type":"text-delta"},
              {"id":"1","type":"text-end"}
            ]
            """,
            await Read(stream)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward a single stream with 2 elements",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_a_single_stream_with_2_elements()
    {
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Merge(
                        new ReadableStream<JsonObject>(start: controller =>
                        {
                            controller.Enqueue(
                                Chunk("""{"type":"text-delta","id":"1","delta":"1a"}""")
                            );
                            controller.Enqueue(
                                Chunk("""{"type":"text-delta","id":"1","delta":"1b"}""")
                            );
                            controller.Close();
                        })
                    );
                    return null;
                },
            }
        );

        AssertChunks(
            """
            [
              {"delta":"1a","id":"1","type":"text-delta"},
              {"delta":"1b","id":"1","type":"text-delta"}
            ]
            """,
            await Read(stream)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel merged streams when the consumer cancels",
        Coverage = UpstreamCoverage.Partial,
        Note = "ReadableStream cancel callbacks do not receive the cancel reason, so the reason forwarded to the merged source is not asserted."
    )]
    public async Task Should_cancel_merged_streams_when_the_consumer_cancels()
    {
        var pullStarted = NewSignal();
        var pullRelease = NewSignal();
        var pullFinished = NewSignal();
        var sourceCancelled = false;
        var sourceContinuedAfterCancel = false;

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Merge(
                        new ReadableStream<JsonObject>(
                            start: controller => controller.Enqueue(Chunk("""{"type":"start"}""")),
                            pull: async controller =>
                            {
                                pullStarted.TrySetResult();
                                await pullRelease.Task;

                                if (!sourceCancelled)
                                {
                                    sourceContinuedAfterCancel = true;
                                    controller.Enqueue(
                                        Chunk(
                                            """{"type":"text-delta","id":"continued-source","delta":"discarded"}"""
                                        )
                                    );
                                    controller.Close();
                                }

                                pullFinished.TrySetResult();
                            },
                            cancel: () =>
                            {
                                sourceCancelled = true;
                                return Task.CompletedTask;
                            }
                        )
                    );
                    return null;
                },
            }
        );

        var reader = stream.GetReader();
        await reader.ReadAsync().WaitAsync(Limit);
        await pullStarted.Task.WaitAsync(Limit);
        await reader.CancelAsync().WaitAsync(Limit);

        pullRelease.SetResult();
        await pullFinished.Task.WaitAsync(Limit);

        Assert.True(sourceCancelled);
        Assert.False(sourceContinuedAfterCancel);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send async message annotation and close the stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_async_message_annotation_and_close_the_stream()
    {
        var wait = NewSignal();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = async writer =>
                {
                    await wait.Task;
                    writer.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
                },
            }
        );

        wait.SetResult();

        AssertChunks("""[{"delta":"1a","id":"1","type":"text-delta"}]""", await Read(stream));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward elements from multiple streams and data parts",
        Coverage = UpstreamCoverage.Partial,
        Note = "The interleaving of chunks of concurrently merged streams depends on JavaScript microtask order; the test asserts the data parts first and the order within each stream."
    )]
    public async Task Should_forward_elements_from_multiple_streams_and_data_parts()
    {
        ReadableStreamController<JsonObject> controller1 = null!;
        ReadableStreamController<JsonObject> controller2 = null!;

        JsonObject Delta(string id, string delta) =>
            Chunk($$"""{"type":"text-delta","id":"{{id}}","delta":"{{delta}}"}""");

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Delta("1", "data-part-1"));

                    writer.Merge(ControlledStream(out controller1));

                    controller1.Enqueue(Delta("1", "1a"));
                    writer.Write(Delta("1", "data-part-2"));
                    controller1.Enqueue(Delta("1", "1b"));

                    writer.Merge(ControlledStream(out controller2));

                    writer.Write(Delta("1", "data-part-3"));
                    return null;
                },
            }
        );

        controller2.Enqueue(Delta("2", "2a"));
        controller1.Enqueue(Delta("1", "1c"));
        controller2.Enqueue(Delta("2", "2b"));
        controller2.Close();
        controller1.Enqueue(Delta("1", "1d"));
        controller1.Enqueue(Delta("1", "1e"));
        controller1.Close();

        var chunks = await Read(stream);

        // the writes of execute come first, in order
        AssertChunks(
            """
            [
              {"delta":"data-part-1","id":"1","type":"text-delta"},
              {"delta":"data-part-2","id":"1","type":"text-delta"},
              {"delta":"data-part-3","id":"1","type":"text-delta"}
            ]
            """,
            chunks.Take(3).ToList()
        );

        // each merged stream keeps its own order
        var rest = chunks.Skip(3).Select(chunk => (string)chunk["id"]! + (string)chunk["delta"]!);
        var restList = rest.ToList();
        Assert.Equal(["11a", "11b", "11c", "11d", "11e"], restList.Where(x => x[0] == '1'));
        Assert.Equal(["22a", "22b"], restList.Where(x => x[0] == '2'));
        Assert.Equal(7, restList.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add error parts when stream errors",
        Coverage = UpstreamCoverage.Partial,
        Note = "The position of the error chunk relative to chunks of the other merged stream depends on JavaScript microtask order; the test asserts the same chunks without that order."
    )]
    public async Task Should_add_error_parts_when_stream_errors()
    {
        ReadableStreamController<JsonObject> controller1 = null!;
        ReadableStreamController<JsonObject> controller2 = null!;

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Merge(ControlledStream(out controller1));
                    writer.Merge(ControlledStream(out controller2));
                    return null;
                },
                OnError = _ => "error-message",
            }
        );

        controller1.Enqueue(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
        controller1.Error(new JsError("1-error"));
        controller2.Enqueue(Chunk("""{"type":"text-delta","id":"2","delta":"2a"}"""));
        controller2.Enqueue(Chunk("""{"type":"text-delta","id":"2","delta":"2b"}"""));
        controller2.Close();

        var chunks = (await Read(stream)).Select(chunk => chunk.ToJsonString()).ToList();

        Assert.Equal(4, chunks.Count);
        Assert.Contains("""{"type":"error","errorText":"error-message"}""", chunks);
        Assert.Equal(
            [
                """{"type":"text-delta","id":"1","delta":"1a"}""",
                """{"type":"text-delta","id":"2","delta":"2a"}""",
                """{"type":"text-delta","id":"2","delta":"2b"}""",
            ],
            chunks.Where(chunk => !chunk.Contains("\"error\"")).OrderBy(chunk => chunk)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add error parts when execute throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_error_parts_when_execute_throws()
    {
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = _ => throw new JsError("execute-error"),
                OnError = _ => "error-message",
            }
        );

        AssertChunks("""[{"errorText":"error-message","type":"error"}]""", await Read(stream));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should add error parts when execute throws with promise",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_error_parts_when_execute_throws_with_promise()
    {
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = async _ =>
                {
                    await Task.Yield();
                    throw new JsError("execute-error");
                },
                OnError = _ => "error-message",
            }
        );

        AssertChunks("""[{"errorText":"error-message","type":"error"}]""", await Read(stream));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle reader acquisition errors without interrupting execute",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_reader_acquisition_errors_without_interrupting_execute()
    {
        var source = new ReadableStream<JsonObject>();
        var sourceReader = source.GetReader();
        var errors = new List<Exception>();

        try
        {
            var stream = UIMessageStreams.CreateUIMessageStream(
                new CreateUIMessageStreamOptions
                {
                    Execute = writer =>
                    {
                        writer.Merge(source);
                        writer.Write(Chunk("""{"type":"text-start","id":"1"}"""));
                        return null;
                    },
                    OnError = error =>
                    {
                        errors.Add(error);
                        return "merge-error";
                    },
                }
            );

            AssertChunks(
                """
                [
                  {"type":"text-start","id":"1"},
                  {"type":"error","errorText":"merge-error"}
                ]
                """,
                await Read(stream)
            );
            Assert.IsType<InvalidOperationException>(Assert.Single(errors));
        }
        finally
        {
            sourceReader.ReleaseLock();
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle reader acquisition errors when merging after execute returns",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_reader_acquisition_errors_when_merging_after_execute_returns()
    {
        var source = new ReadableStream<JsonObject>();
        var sourceReader = source.GetReader();
        var execution = NewSignal();
        var errors = new List<Exception>();
        IUIMessageStreamWriter streamWriter = null!;

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    streamWriter = writer;
                    return execution.Task;
                },
                OnError = error =>
                {
                    errors.Add(error);
                    return "merge-error";
                },
            }
        );

        try
        {
            streamWriter.Merge(source);
            execution.TrySetResult();

            AssertChunks("""[{"type":"error","errorText":"merge-error"}]""", await Read(stream));
            Assert.IsType<InvalidOperationException>(Assert.Single(errors));
        }
        finally
        {
            execution.TrySetResult();
            sourceReader.ReleaseLock();
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should suppress error when writing to closed stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_suppress_error_when_writing_to_closed_stream()
    {
        IUIMessageStreamWriter uiMessageStreamWriter = null!;

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
                    uiMessageStreamWriter = writer;
                    return null;
                },
            }
        );

        AssertChunks("""[{"delta":"1a","id":"1","type":"text-delta"}]""", await Read(stream));

        var exception = Record.Exception(() =>
            uiMessageStreamWriter.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1b"}"""))
        );
        Assert.Null(exception);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support writing from delayed merged streams",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_writing_from_delayed_merged_streams()
    {
        IUIMessageStreamWriter uiMessageStreamWriter = null!;
        ReadableStreamController<JsonObject> controller1 = null!;
        ReadableStreamController<JsonObject> controller2 = null!;
        var done = false;

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Merge(ControlledStream(out controller1));

                    uiMessageStreamWriter = writer;
                    done = true;
                    return null;
                },
            }
        );

        var result = new List<JsonObject>();
        var reader = stream.GetReader();

        async Task Pull()
        {
            var read = await reader.ReadAsync().WaitAsync(Limit);
            result.Add(read.Value);
        }

        // function is finished
        Assert.True(done);

        controller1.Enqueue(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
        await Pull();

        // controller1 is still open, create 2nd stream
        uiMessageStreamWriter.Merge(ControlledStream(out controller2));

        // close controller1
        controller1.Close();

        await Task.Delay(50); // relinquish control

        // it should still be able to write to controller2
        controller2.Enqueue(Chunk("""{"type":"text-delta","id":"2","delta":"2a"}"""));
        controller2.Close();

        await Pull();

        AssertChunks(
            """
            [
              {"delta":"1a","id":"1","type":"text-delta"},
              {"delta":"2a","id":"2","type":"text-delta"}
            ]
            """,
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle onFinish without original messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_onFinish_without_original_messages()
    {
        var recordedOptions = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"text-start","id":"1"}"""));
                    writer.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
                    writer.Write(Chunk("""{"type":"text-end","id":"1"}"""));
                    return null;
                },
                OnFinish = options =>
                {
                    recordedOptions.Add(options);
                    return Task.CompletedTask;
                },
                GenerateId = () => "response-message-id",
            }
        );
        await ConsumeStream.ConsumeAsync(stream).WaitAsync(Limit);

        var options = Assert.Single(recordedOptions);
        Assert.Null(options.FinishReason);
        Assert.False(options.IsAborted);
        Assert.False(options.IsContinuation);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, options.Outcome.Status);
        var message = Assert.Single(options.Messages);
        Assert.Same(options.ResponseMessage, message);
        Assert.Equal("response-message-id", message.Id);
        Assert.Equal(UIMessageRole.Assistant, message.Role);
        Assert.Null(message.Metadata);
        AssertDoneText(Assert.Single(message.Parts), "1a");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle onEnd without original messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_onEnd_without_original_messages()
    {
        var recordedOptions = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"text-start","id":"1"}"""));
                    writer.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1a"}"""));
                    writer.Write(Chunk("""{"type":"text-end","id":"1"}"""));
                    return null;
                },
                OnEnd = options =>
                {
                    recordedOptions.Add(options);
                    return Task.CompletedTask;
                },
                GenerateId = () => "response-message-id",
            }
        );
        await ConsumeStream.ConsumeAsync(stream).WaitAsync(Limit);

        var options = Assert.Single(recordedOptions);
        Assert.False(options.IsAborted);
        Assert.False(options.IsContinuation);
        Assert.Equal("response-message-id", options.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, options.ResponseMessage.Role);
        AssertDoneText(Assert.Single(options.ResponseMessage.Parts), "1a");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should report consumer cancellation when the consumer cancels before an outcome is declared",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_consumer_cancellation_when_the_consumer_cancels_before_an_outcome_is_declared()
    {
        var execution = NewSignal();
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"start"}"""));
                    return execution.Task;
                },
                OnEnd = options =>
                {
                    lock (onEndEvents)
                    {
                        onEndEvents.Add(options);
                    }

                    return Task.CompletedTask;
                },
                GenerateId = () => "response-message-id",
            }
        );

        var reader = stream.GetReader();
        await reader.ReadAsync().WaitAsync(Limit);
        await reader.CancelAsync().WaitAsync(Limit);
        execution.TrySetResult();

        var options = Assert.Single(onEndEvents);
        Assert.False(options.IsAborted);
        Assert.True(options.IsCancelled);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, options.Outcome.Status);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle onFinish with messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_onFinish_with_messages()
    {
        var recordedOptions = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"text-start","id":"1"}"""));
                    writer.Write(Chunk("""{"type":"text-delta","id":"1","delta":"1b"}"""));
                    writer.Write(Chunk("""{"type":"text-end","id":"1"}"""));
                    return null;
                },
                OriginalMessages =
                [
                    Original("0", UIMessageRole.User, new TextUIPart("0a")),
                    Original("1", UIMessageRole.Assistant, new TextUIPart("1a") { State = "done" }),
                ],
                OnFinish = options =>
                {
                    recordedOptions.Add(options);
                    return Task.CompletedTask;
                },
            }
        );
        await ConsumeStream.ConsumeAsync(stream).WaitAsync(Limit);

        var options = Assert.Single(recordedOptions);
        Assert.Null(options.FinishReason);
        Assert.False(options.IsAborted);
        Assert.True(options.IsContinuation);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, options.Outcome.Status);

        Assert.Equal(2, options.Messages.Count);
        Assert.Equal("0", options.Messages[0].Id);
        Assert.Equal(UIMessageRole.User, options.Messages[0].Role);
        Assert.Equal(
            "0a",
            Assert.IsType<TextUIPart>(Assert.Single(options.Messages[0].Parts)).Text
        );

        var assistant = options.Messages[1];
        Assert.Same(options.ResponseMessage, assistant);
        Assert.Equal("1", assistant.Id);
        Assert.Equal(UIMessageRole.Assistant, assistant.Role);
        Assert.Equal(2, assistant.Parts.Count);
        AssertDoneText(assistant.Parts[0], "1a");
        AssertDoneText(assistant.Parts[1], "1b");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject a messageId into the stream when originalMessages are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_inject_a_messageId_into_the_stream_when_originalMessages_are_provided()
    {
        var recordedOptions = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"start"}""")); // no messageId
                    return null;
                },
                OriginalMessages =
                [
                    Original("0", UIMessageRole.User, new TextUIPart("0a")),
                    // no assistant message
                ],
                OnFinish = options =>
                {
                    recordedOptions.Add(options);
                    return Task.CompletedTask;
                },
                GenerateId = () => "response-message-id",
            }
        );

        AssertChunks(
            """[{"messageId":"response-message-id","type":"start"}]""",
            await Read(stream)
        );

        var options = Assert.Single(recordedOptions);
        AssertEmptyAssistantResponse(options, "response-message-id");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should keep existing messageId from start chunk when originalMessages are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_existing_messageId_from_start_chunk_when_originalMessages_are_provided()
    {
        var recordedOptions = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.Write(Chunk("""{"type":"start","messageId":"existing-message-id"}"""));
                    return null;
                },
                OriginalMessages =
                [
                    Original("0", UIMessageRole.User, new TextUIPart("0a")),
                    // no assistant message
                ],
                OnFinish = options =>
                {
                    recordedOptions.Add(options);
                    return Task.CompletedTask;
                },
                GenerateId = () => "response-message-id",
            }
        );

        AssertChunks(
            """[{"messageId":"existing-message-id","type":"start"}]""",
            await Read(stream)
        );

        var options = Assert.Single(recordedOptions);
        AssertEmptyAssistantResponse(options, "existing-message-id");
    }

    private static void AssertEmptyAssistantResponse(UIMessageStreamOnEndEvent options, string id)
    {
        Assert.Null(options.FinishReason);
        Assert.False(options.IsAborted);
        Assert.False(options.IsContinuation);
        Assert.Equal(UIMessageStreamOutcomeStatus.Unknown, options.Outcome.Status);

        Assert.Equal(2, options.Messages.Count);
        Assert.Equal("0", options.Messages[0].Id);
        Assert.Equal(UIMessageRole.User, options.Messages[0].Role);
        Assert.Equal(
            "0a",
            Assert.IsType<TextUIPart>(Assert.Single(options.Messages[0].Parts)).Text
        );

        var response = options.Messages[1];
        Assert.Same(options.ResponseMessage, response);
        Assert.Equal(id, response.Id);
        Assert.Equal(UIMessageRole.Assistant, response.Role);
        Assert.Null(response.Metadata);
        Assert.Empty(response.Parts);
    }

    private sealed record Observation(
        string ChunkTypes,
        int OnEndCalls,
        bool IsAborted,
        UIMessageStreamOutcomeStatus Status,
        string? ErrorMessage
    );

    private static async Task<Observation> Observe(
        Func<IUIMessageStreamWriterWithOutcome, Task?> execute
    )
    {
        var onEndCalls = 0;
        UIMessageStreamOnEndEvent? observation = null;
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = execute,
                OnError = error => error.Message,
                OnEnd = options =>
                {
                    onEndCalls++;
                    observation = options;
                    return Task.CompletedTask;
                },
            }
        );

        var chunks = await Read(stream);
        return new Observation(
            string.Join(",", chunks.Select(chunk => (string)chunk["type"]!)),
            onEndCalls,
            observation!.IsAborted,
            observation.Outcome.Status,
            observation.Outcome.Status == UIMessageStreamOutcomeStatus.Failed
                ? observation.Outcome.Error?.Message
                : null
        );
    }

    private static ReadableStream<JsonObject> FailingStream(string message) =>
        new(start: controller => controller.Error(new JsError(message)));

    [Fact]
    [UpstreamTest(
        Prefix + "reports operation outcomes without inferring failure from error chunks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_operation_outcomes_without_inferring_failure_from_error_chunks()
    {
        static Observation Expect(
            string[] chunkTypes,
            UIMessageStreamOutcomeStatus status,
            string? errorMessage = null,
            bool isAborted = false
        ) => new(string.Join(",", chunkTypes), 1, isAborted, status, errorMessage);

        string[] none = [];
        string[] error = ["error"];

        Assert.Equal(Expect(none, UIMessageStreamOutcomeStatus.Unknown), await Observe(_ => null));
        Assert.Equal(
            Expect(error, UIMessageStreamOutcomeStatus.Unknown),
            await Observe(writer =>
            {
                writer.Write(Chunk("""{"type":"error","errorText":"recoverable error"}"""));
                return null;
            })
        );
        Assert.Equal(
            Expect(none, UIMessageStreamOutcomeStatus.Completed),
            await Observe(writer =>
            {
                writer.SetOutcome(
                    new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                );
                return null;
            })
        );
        Assert.Equal(
            Expect(none, UIMessageStreamOutcomeStatus.Completed),
            await Observe(writer =>
            {
                writer.SetOutcome(
                    new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                );
                writer.SetOutcome(
                    new UIMessageStreamOutcome(
                        UIMessageStreamOutcomeStatus.Failed,
                        new JsError("ignored")
                    )
                );
                return null;
            })
        );
        Assert.Equal(
            Expect(none, UIMessageStreamOutcomeStatus.Failed, "declared failure"),
            await Observe(writer =>
            {
                writer.SetOutcome(
                    new UIMessageStreamOutcome(
                        UIMessageStreamOutcomeStatus.Failed,
                        new JsError("declared failure")
                    )
                );
                return null;
            })
        );
        Assert.Equal(
            Expect(none, UIMessageStreamOutcomeStatus.Aborted, isAborted: true),
            await Observe(writer =>
            {
                writer.SetOutcome(new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Aborted));
                return null;
            })
        );
        Assert.Equal(
            Expect(error, UIMessageStreamOutcomeStatus.Failed, "execute failure"),
            await Observe(_ => Task.FromException(new JsError("execute failure")))
        );
        Assert.Equal(
            Expect(error, UIMessageStreamOutcomeStatus.Failed, "execute failure after completion"),
            await Observe(writer =>
            {
                writer.SetOutcome(
                    new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                );
                return Task.FromException(new JsError("execute failure after completion"));
            })
        );
        Assert.Equal(
            Expect(error, UIMessageStreamOutcomeStatus.Failed, "merged stream failure"),
            await Observe(writer =>
            {
                writer.Merge(FailingStream("merged stream failure"));
                return null;
            })
        );
        Assert.Equal(
            Expect(
                error,
                UIMessageStreamOutcomeStatus.Failed,
                "merged stream failure after completion"
            ),
            await Observe(writer =>
            {
                writer.SetOutcome(
                    new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                );
                writer.Merge(FailingStream("merged stream failure after completion"));
                return null;
            })
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports errors thrown by onError as failed exactly once",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_errors_thrown_by_onError_as_failed_exactly_once()
    {
        Func<IUIMessageStreamWriterWithOutcome, Task?>[] executes =
        [
            _ => Task.FromException(new JsError("execute failure")),
            writer =>
            {
                writer.Merge(FailingStream("merged stream failure"));
                return null;
            },
        ];

        foreach (var execute in executes)
        {
            var onErrorError = new JsError("onError failure");
            var onEndEvents = new List<UIMessageStreamOnEndEvent>();
            var stream = UIMessageStreams.CreateUIMessageStream(
                new CreateUIMessageStreamOptions
                {
                    Execute = execute,
                    OnError = _ => throw onErrorError,
                    OnEnd = options =>
                    {
                        onEndEvents.Add(options);
                        return Task.CompletedTask;
                    },
                }
            );

            var thrown = await Assert.ThrowsAnyAsync<Exception>(() => Read(stream));
            Assert.Same(onErrorError, thrown);

            var onEnd = Assert.Single(onEndEvents);
            Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
            Assert.Same(onErrorError, onEnd.Outcome.Error);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "reports invalid chunk processing as failed after completion was declared",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_invalid_chunk_processing_as_failed_after_completion_was_declared()
    {
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.SetOutcome(
                        new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                    );
                    writer.Write(Chunk("""{"type":"finish"}"""));
                    writer.Write(Chunk("""{"type":"text-delta","id":"missing","delta":"text"}"""));
                    return null;
                },
                OnEnd = options =>
                {
                    onEndEvents.Add(options);
                    return Task.CompletedTask;
                },
            }
        );

        var processingError = await Assert.ThrowsAnyAsync<Exception>(() => Read(stream));

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
        Assert.Same(processingError, onEnd.Outcome.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "injects message IDs without mutating frozen start chunks",
        Coverage = UpstreamCoverage.Partial,
        Note = "JsonObject cannot be frozen; the test asserts that the written start chunk is not mutated."
    )]
    public async Task Injects_message_IDs_without_mutating_frozen_start_chunks()
    {
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();
        var startChunk = Chunk("""{"type":"start"}""");
        var stream = UIMessageStreams.CreateUIMessageStream(
            new CreateUIMessageStreamOptions
            {
                Execute = writer =>
                {
                    writer.SetOutcome(
                        new UIMessageStreamOutcome(UIMessageStreamOutcomeStatus.Completed)
                    );
                    writer.Write(startChunk);
                    return null;
                },
                GenerateId = () => "generated-message-id",
                OnEnd = options =>
                {
                    onEndEvents.Add(options);
                    return Task.CompletedTask;
                },
            }
        );

        AssertChunks(
            """[{"type":"start","messageId":"generated-message-id"}]""",
            await Read(stream)
        );
        JsonAssert.Equal(startChunk, """{"type":"start"}""");

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Completed, onEnd.Outcome.Status);
        Assert.Null(onEnd.Outcome.Error);
    }
}
