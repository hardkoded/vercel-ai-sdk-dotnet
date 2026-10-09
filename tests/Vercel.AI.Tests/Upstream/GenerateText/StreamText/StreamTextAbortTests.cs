// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; abort signal</c>.</summary>
public sealed class StreamTextAbortTests
{
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(5);

    public static TheoryData<string> Apis => new() { "streamText", "ToolLoopAgent" };

    [Theory]
    [MemberData(nameof(Apis))]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > abort signal::%s should reject on abort without waiting for callbacks or cancellation", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_on_abort_without_waiting_for_callbacks_or_cancellation(string api)
    {
        var abortController = new AbortController();
        var reason = new JsError("manual abort");
        var outputReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var abortCallback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var model = new OpenStreamModel(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello"));
        var onAbortCalls = new List<StreamAbortContext>();
        var onErrorCalls = 0;
        var onFinishCalls = 0;
        Task OnAbort(StreamAbortContext context, CancellationToken cancellationToken)
        {
            onAbortCalls.Add(context);
            return abortCallback.Task;
        }

        StreamTextResult result;
        if (api == "streamText")
        {
            result = Client().StreamTextAsync(new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                AbortSignal = abortController.Signal,
                OnAbort = OnAbort,
                OnError = (_, _) =>
                {
                    onErrorCalls++;
                    return Task.CompletedTask;
                },
                OnFinish = (_, _) =>
                {
                    onFinishCalls++;
                    return Task.CompletedTask;
                },
            });
        }
        else
        {
            result = new Agent(new AgentOptions { Model = model, OnAbort = OnAbort }, Client())
                .Stream("test-input", abortSignal: abortController.Signal);
        }

        var stream = Collect(result, outputReceived);
        var results = new Task[] { result.Text, result.Steps, result.FinishReason, result.Usage };

        await outputReceived.Task.WaitAsync(SettleTimeout);
        abortController.Abort(reason);

        try
        {
            await Settle(results);

            // Only let OnAbort finish after the result tasks have rejected.
            abortCallback.TrySetResult();
            var parts = await stream.WaitAsync(SettleTimeout);
            var abort = Assert.IsType<AbortPart>(parts[^1]);
            Assert.Equal("Error: manual abort", abort.Reason);
            Assert.Equal(1, model.Disposals);
            var context = Assert.Single(onAbortCalls);
            Assert.Empty(context.Steps);
            Assert.Same(reason, context.Exception);
            Assert.Equal(0, onErrorCalls);
            Assert.Equal(0, onFinishCalls);
        }
        finally
        {
            abortCallback.TrySetResult();
        }

        foreach (var task in results)
        {
            Assert.True(task.IsFaulted);
            Assert.Same(reason, task.Exception!.InnerException);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > abort signal > abort in 2nd step::should reject result promises when aborting after a completed step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_when_aborting_after_a_completed_step()
    {
        using var cancellation = new CancellationTokenSource();
        var streamCalls = 0;
        var model = new ScriptedStreamModel(() =>
        {
            streamCalls++;
            return streamCalls == 1 ? FirstStep() : SecondStep(cancellation);
        });
        var result = Client().StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    Tool.Function(
                        "tool1",
                        null,
                        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
                        (_, _) => Task.FromResult("\"result1\"")),
                },
                StopWhen = StopWhen.IsStepCount(3),
            },
            cancellation.Token);

        await Collect(result, null).WaitAsync(SettleTimeout);

        await Settle(new Task[] { result.Text, result.Steps, result.FinishReason, result.Usage });
        foreach (var task in new Task[] { result.Text, result.Steps, result.FinishReason, result.Usage })
        {
            Assert.True(task.IsFaulted);
            Assert.IsAssignableFrom<OperationCanceledException>(task.Exception!.InnerException);
        }
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> FirstStep()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }");
        yield return new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> SecondStep(CancellationTokenSource cancellation)
    {
        yield return new StreamStartStreamPart(null);
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Hello");
        await Task.Yield();
        cancellation.Cancel();
        throw new OperationCanceledException("The user aborted a request.", cancellation.Token);
    }

    // Reads the whole stream. Signals once the first text delta is seen.
    private static async Task<List<TextStreamPart>> Collect(StreamTextResult result, TaskCompletionSource? firstDelta)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
            if (part is TextDeltaPart)
            {
                firstDelta?.TrySetResult();
            }
        }

        return parts;
    }

    private static async Task Settle(Task[] results)
    {
        var settled = Task.WhenAll(results.Select(task => task.ContinueWith(_ => { }, TaskScheduler.Default)));
        await settled.WaitAsync(SettleTimeout);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    // Yields the parts, then waits forever. It ignores the cancellation token and never finishes disposing.
    private sealed class OpenStreamModel : ILanguageModel
    {
        private readonly LanguageModelStreamPart[] _parts;
        private int _disposals;

        public OpenStreamModel(params LanguageModelStreamPart[] parts)
        {
            _parts = parts;
        }

        public int Disposals => Volatile.Read(ref _disposals);

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "open-stream";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            return new Source(this);
        }

        private sealed class Source : IAsyncEnumerable<LanguageModelStreamPart>
        {
            private readonly OpenStreamModel _model;

            public Source(OpenStreamModel model)
            {
                _model = model;
            }

            public IAsyncEnumerator<LanguageModelStreamPart> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Reader(_model);
            }
        }

        private sealed class Reader : IAsyncEnumerator<LanguageModelStreamPart>
        {
            private readonly OpenStreamModel _model;
            private int _index = -1;

            public Reader(OpenStreamModel model)
            {
                _model = model;
            }

            public LanguageModelStreamPart Current => _model._parts[_index];

            public ValueTask<bool> MoveNextAsync()
            {
                _index++;
                return _index < _model._parts.Length
                    ? new ValueTask<bool>(true)
                    : new ValueTask<bool>(new TaskCompletionSource<bool>().Task);
            }

            public ValueTask DisposeAsync()
            {
                Interlocked.Increment(ref _model._disposals);
                return new ValueTask(new TaskCompletionSource().Task);
            }
        }
    }

    private sealed class ScriptedStreamModel : ILanguageModel
    {
        private readonly Func<IAsyncEnumerable<LanguageModelStreamPart>> _next;

        public ScriptedStreamModel(Func<IAsyncEnumerable<LanguageModelStreamPart>> next)
        {
            _next = next;
        }

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "scripted";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            return _next();
        }
    }
}
