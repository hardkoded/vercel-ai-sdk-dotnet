// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using StreamSpeechOperation = Vercel.AI.Operations.StreamSpeech;

namespace Vercel.AI.Tests.Upstream.Operations.StreamSpeech;

/// <summary>Port of the <c>cancellation while doStream is pending</c> describe of <c>stream-speech.test.ts</c>.</summary>
public sealed class CancellationWhileDoStreamIsPendingTests
{
    private const string Prefix = "packages/ai/src/generate-speech/stream-speech.test.ts::experimental_streamSpeech > cancellation while doStream is pending::";

    // A provider that never reacts to the cancellation token.
    private static (MockStreamingSpeechModel Model, Func<CancellationToken> Signal) HangingModel()
    {
        CancellationToken signal = default;
        var model = new MockStreamingSpeechModel
        {
            DoStream = (call, _) =>
            {
                signal = call.CancellationToken;
                return new TaskCompletionSource<SpeechModelStreamResult>().Task;
            },
        };
        return (model, () => signal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject result promises when fullStream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_when_fullStream_is_cancelled()
    {
        var (model, signal) = HangingModel();
        var result = StreamSpeechOperation.Start(new StreamSpeechRequest { Model = model, Text = "Hi" });

        var stream = result.FullStream;
        var usage = result.Usage;
        stream.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => usage);
        await Assert.ThrowsAsync<OperationCanceledException>(() => result.Responses);
        Assert.True(signal().IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject result promises when audioStream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_when_audioStream_is_cancelled()
    {
        var (model, signal) = HangingModel();
        var result = StreamSpeechOperation.Start(new StreamSpeechRequest { Model = model, Text = "Hi" });

        var stream = result.AudioStream;
        var usage = result.Usage;
        stream.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => usage);
        await Assert.ThrowsAsync<OperationCanceledException>(() => result.ProviderMetadata);
        Assert.True(signal().IsCancellationRequested);
    }
}
