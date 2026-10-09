// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.StreamText;

internal static class StreamTextAbortSupport
{
    internal static async IAsyncEnumerable<LanguageModelStreamPart> FirstStep()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }");
        yield return new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }

    internal static async IAsyncEnumerable<LanguageModelStreamPart> SecondStep(CancellationTokenSource cancellation)
    {
        yield return new StreamStartStreamPart(null);
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Hello");
        await Task.Yield();
        cancellation.Cancel();
        throw new OperationCanceledException("The user aborted a request.", cancellation.Token);
    }

    // Reads the whole stream. Signals once the first text delta is seen.
    internal static async Task<List<TextStreamPart>> Collect(StreamTextResult result, TaskCompletionSource? firstDelta)
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

    internal static async Task Settle(Task[] results)
    {
        var settled = Task.WhenAll(results.Select(task => task.ContinueWith(_ => { }, TaskScheduler.Default)));
        await settled.WaitAsync(TimeSpan.FromSeconds(5));
    }

    internal static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    // Yields the parts, then waits forever. It ignores the cancellation token and never finishes disposing.
    internal sealed class OpenStreamModel : ILanguageModel
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

        internal sealed class Source : IAsyncEnumerable<LanguageModelStreamPart>
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

        internal sealed class Reader : IAsyncEnumerator<LanguageModelStreamPart>
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

    internal sealed class ScriptedStreamModel : ILanguageModel
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
