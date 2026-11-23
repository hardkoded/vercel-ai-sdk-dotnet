// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Telemetry;
using GenerateSpeechOperation = Vercel.AI.Operations.GenerateSpeech;

namespace Vercel.AI.Tests.Upstream.GenerateSpeech;

/// <summary>Port of the telemetry tests of <c>generate-speech.test.ts</c> &gt; <c>generateSpeech</c>.</summary>
public sealed class GenerateSpeechTests
{
    private const string Test =
        "packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::";

    private const string SampleText = "This is a sample text to convert to speech.";

    private static readonly DateTime TestDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    [UpstreamTest(
        Test + "should emit telemetry start and end events without raw audio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_telemetry_start_and_end_events_without_raw_audio()
    {
        var events = new List<(string Type, object Event)>();
        var rawAudio = new byte[] { 1, 2, 3, 4 };

        await GenerateSpeechOperation.GenerateSpeechAsync(
            new GenerateSpeechRequest
            {
                Model = new SpeechModel(_ =>
                    Task.FromResult(
                        new SpeechModelResult(
                            rawAudio,
                            [],
                            OperationJson.Parse("""{"mock":{"traceId":"trace-1"}}"""),
                            new ProviderResponse(
                                new Dictionary<string, string> { ["content-type"] = "audio/mp3" },
                                OperationJson.Parse("""{"audioContent":"AQIDBA=="}"""),
                                timestamp: TestDate,
                                modelId: "test-model"
                            ),
                            usage: OperationJson.Parse("{\"characters\":" + SampleText.Length + "}")
                        )
                    )
                ),
                Text = SampleText,
                Voice = "alloy",
                GenerateCallId = () => "call-1",
                Telemetry = new TelemetryOptions
                {
                    FunctionId = "speak-text",
                    RecordOutputs = false,
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStart = (e, _) =>
                            {
                                if (e is GenerateSpeechStartEvent)
                                {
                                    events.Add(("start", e));
                                }

                                return Task.CompletedTask;
                            },
                            OnEnd = (e, _) =>
                            {
                                if (e is GenerateSpeechEndEvent)
                                {
                                    events.Add(("end", e));
                                }

                                return Task.CompletedTask;
                            },
                        },
                    ],
                },
            }
        );

        Assert.Equal(2, events.Count);
        Assert.Equal("start", events[0].Type);
        var start = Assert.IsType<GenerateSpeechStartEvent>(events[0].Event);
        Assert.Equal("call-1", start.CallId);
        Assert.Equal("ai.generateSpeech", start.OperationId);
        Assert.Equal(SampleText, start.Text);
        Assert.Equal("alloy", start.Voice);
        Assert.Equal("speak-text", start.FunctionId);
        Assert.False(start.RecordOutputs);

        Assert.Equal("end", events[1].Type);
        var end = Assert.IsType<GenerateSpeechEndEvent>(events[1].Event);
        Assert.Equal("call-1", end.CallId);
        Assert.Equal("ai.generateSpeech", end.OperationId);
        Assert.Equal(SampleText, end.Text);
        Assert.Equal(4, end.Audio.ByteLength);
        Assert.Equal("audio/mp3", end.Audio.MediaType);
        Assert.Equal("mp3", end.Audio.Format);
        Assert.Equal(SampleText.Length, end.Usage!.Value.GetProperty("characters").GetInt32());
        Assert.Equal("speak-text", end.FunctionId);
        Assert.Null(end.ProviderMetadata);
        Assert.False(end.RecordOutputs);
        Assert.Null(end.Response.Body);
    }

    [Fact]
    [UpstreamTest(
        Test + "should emit a telemetry error event when speech generation fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_a_telemetry_error_event_when_speech_generation_fails()
    {
        var error = new InvalidOperationException("speech failed");
        var delivered = new List<TelemetryErrorEvent>();

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GenerateSpeechOperation.GenerateSpeechAsync(
                new GenerateSpeechRequest
                {
                    Model = new SpeechModel(_ => throw error),
                    Text = SampleText,
                    MaxRetries = 0,
                    GenerateCallId = () => "call-1",
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnError = (e, _) =>
                                {
                                    delivered.Add(e);
                                    return Task.CompletedTask;
                                },
                            },
                        ],
                    },
                }
            )
        );

        Assert.Same(error, thrown);
        var onError = Assert.Single(delivered);
        Assert.Equal(AiSdkVersion.Version, onError.AiVersion);
        Assert.Equal("call-1", onError.CallId);
        Assert.Same(error, onError.Error);
    }

    private sealed class SpeechModel(Func<SpeechModelCall, Task<SpeechModelResult>> generate)
        : ISpeechCaller
    {
        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public Task<SpeechModelResult> DoGenerateAsync(
            SpeechModelCall call,
            CancellationToken cancellationToken
        ) => generate(call);
    }
}
