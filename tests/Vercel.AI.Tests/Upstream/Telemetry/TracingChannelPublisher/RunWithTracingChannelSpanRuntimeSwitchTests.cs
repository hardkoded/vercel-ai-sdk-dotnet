// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Telemetry.TracingChannel;
using Publisher = Vercel.AI.TracingChannelPublisher;

namespace Vercel.AI.Tests.Upstream.Telemetry.TracingChannelPublisher;

/// <summary>
/// Port of <c>tracing-channel-publisher.test.ts</c> &gt; <c>runWithTracingChannelSpan runtime switch</c>. The three tests that mock
/// <c>isNodeRuntime</c> are not ported, because the port has no runtime switch: a .NET process always has the diagnostics listener.
/// </summary>
public sealed class RunWithTracingChannelSpanRuntimeSwitchTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/tracing-channel-publisher.test.ts::runWithTracingChannelSpan runtime switch::";

    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";

    private static LanguageModelStreamRequest RequestWith(bool redact)
    {
        var body = JsonDocument.Parse("{\"credential\":\"test-token\"}").RootElement.Clone();
        return redact
            ? new LanguageModelStreamRequest(body)
            {
                RedactBody = _ =>
                    JsonDocument.Parse("{\"credential\":\"[redacted]\"}").RootElement.Clone(),
            }
            : new LanguageModelStreamRequest(body);
    }

    private static string CredentialOf(LanguageModelStreamRequest? request) =>
        request!.Body!.Value.GetProperty("credential").GetString()!;

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(params string[] deltas)
    {
        foreach (var delta in deltas)
        {
            yield return new TextDeltaStreamPart("1", delta);
        }

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> AppendBang(
        IAsyncEnumerable<LanguageModelStreamPart> source
    )
    {
        await foreach (var part in source)
        {
            yield return part is TextDeltaStreamPart text
                ? new TextDeltaStreamPart(text.Id, text.Delta + "!")
                : part;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "preserves subscriber stream replacements with redaction=%s",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_subscriber_stream_replacements_with_redaction(bool redact)
    {
        var message = new TelemetryTracingChannelMessage(
            TelemetryTracingEventType.LanguageModelCall,
            new LanguageModelCallStartEvent { CallId = "stream-replacement-" + redact }
        );
        var original = new LanguageModelStreamResult(Parts("hello"), RequestWith(redact));
        var originalBody = original.Request!.Body;
        var observedCredentials = new List<string>();
        var subscriber = new AsyncEndSubscriber(message, value =>
        {
            var result = Assert.IsType<LanguageModelStreamResult>(value);
            observedCredentials.Add(CredentialOf(result.Request));
            result.Stream = AppendBang(result.Stream);
        });
        using var first = DiagnosticListener.AllListeners.Subscribe(subscriber);
        using var second = DiagnosticListener.AllListeners.Subscribe(subscriber.Another());

        var result = await Publisher.RunWithTracingChannelSpan(message, () =>
            Task.FromResult(original)
        );

        Assert.Same(original, result);
        Assert.Equal(originalBody, result.Request!.Body);
        Assert.Equal("test-token", CredentialOf(result.Request));
        Assert.Equal(
            [redact ? "[redacted]" : "test-token", redact ? "[redacted]" : "test-token"],
            observedCredentials
        );
        await using var enumerator = result.Stream.GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("hello!!", Assert.IsType<TextDeltaStreamPart>(enumerator.Current).Delta);
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "redacts long-lived completion metadata with recordResult=%s",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Redacts_long_lived_completion_metadata_with_recordResult(bool recordResult)
    {
        var message = new TelemetryTracingChannelMessage(
            TelemetryTracingEventType.StreamText,
            new GenerateTextStartEvent { CallId = "long-lived-" + recordResult }
        );
        var result = new LanguageModelGenerateResult(
            [new GeneratedText("")],
            FinishReason.Stop,
            new LanguageModelUsage(1, 1, 2),
            request: RequestWith(redact: true)
        );
        var asyncEnd = new List<object?>();
        var subscriber = new AsyncEndSubscriber(message, value => asyncEnd.Add(value));
        using var subscription = DiagnosticListener.AllListeners.Subscribe(subscriber);

        var completion = Task.FromResult<object?>(result);
        Publisher.OpenTelemetryChannelSpanContext(message, completion, recordResult);
        await completion;

        Assert.Single(asyncEnd);
        if (recordResult)
        {
            var observed = Assert.IsType<LanguageModelGenerateResult>(asyncEnd[0]);
            Assert.Equal("[redacted]", CredentialOf(observed.Request));
        }
        else
        {
            Assert.Null(asyncEnd[0]);
        }

        Assert.Equal("test-token", CredentialOf(result.Request));
    }

    // Subscribes to the asyncEnd sub-channel for one message and hands its result to the callback.
    private sealed class AsyncEndSubscriber(
        TelemetryTracingChannelMessage message,
        Action<object?> onAsyncEnd
    ) : IObserver<DiagnosticListener>
    {
        public AsyncEndSubscriber Another() => new(message, onAsyncEnd);

        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == Vercel.AI.TracingChannel.AiSdkTelemetryTracingChannel)
            {
                listener.Subscribe(new Events(message, onAsyncEnd));
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        private sealed class Events(
            TelemetryTracingChannelMessage message,
            Action<object?> onAsyncEnd
        ) : IObserver<KeyValuePair<string, object?>>
        {
            public void OnNext(KeyValuePair<string, object?> item)
            {
                // The tracing channel is shared by every test of the process, so only this message counts.
                if (item.Key == "asyncEnd" && ReferenceEquals(item.Value, message))
                {
                    onAsyncEnd(message.Result);
                }
            }

            public void OnCompleted() { }

            public void OnError(Exception error) { }
        }
    }
}
