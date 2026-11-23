// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>
/// Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>speech and transcription (recordInputs=$recordInputs, recordOutputs=$recordOutputs)</c>.
/// </summary>
public sealed class SpeechAndTranscriptionTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > speech and transcription (recordInputs=$recordInputs, recordOutputs=$recordOutputs)::";

    public static TheoryData<bool?, bool?, string> Cases
    {
        get
        {
            var data = new TheoryData<bool?, bool?, string>();
            foreach (
                var (recordInputs, recordOutputs) in new (bool?, bool?)[]
                {
                    (null, null),
                    (false, null),
                    (null, false),
                    (false, false),
                }
            )
            {
                foreach (var text in new[] { "Hello", "" })
                {
                    data.Add(recordInputs, recordOutputs, text);
                }
            }

            return data;
        }
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static string Messages(string role, string text) =>
        JsonSerializer.Serialize(
            new[] { new { role, parts = new[] { new { type = "text", content = text } } } }
        );

    // Numbers are compared as doubles: the attribute can hold any numeric type.
    private static object? Attribute(MockSpan span, string key)
    {
        span.Attributes.TryGetValue(key, out var value);
        return value is null or string or bool ? value : Convert.ToDouble(value);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Test + "records speech input %j and output metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_speech_input_and_output_metadata(
        bool? recordInputs,
        bool? recordOutputs,
        string text
    )
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            new GenerateSpeechStartEvent(
                "call-1",
                "openai.speech",
                "gpt-4o-mini-tts",
                text,
                "alloy",
                "mp3",
                null,
                null,
                null,
                2,
                null,
                Json("{}")
            )
            {
                RecordInputs = recordInputs,
                RecordOutputs = recordOutputs,
            },
            default
        );
        await integration.OnEnd!(
            new GenerateSpeechEndEvent(
                "call-1",
                "openai.speech",
                "gpt-4o-mini-tts",
                text,
                new GenerateSpeechAudioInfo(1234, "audio/mpeg", "mp3"),
                Json("""{"characters":5}"""),
                [],
                null,
                new ProviderResponse(timestamp: DateTime.UnixEpoch, modelId: "gpt-4o-mini-tts")
            )
            {
                RecordInputs = recordInputs,
                RecordOutputs = recordOutputs,
            },
            default
        );

        var span = Assert.Single(tracer.Spans);
        Assert.Equal(System.Diagnostics.ActivityKind.Client, span.Activity.Kind);
        Assert.Equal("ai.generateSpeech gpt-4o-mini-tts", span.Name);
        Assert.True(span.Ended);
        Assert.Equal("ai.generateSpeech", span.InitAttributes["gen_ai.operation.name"]);
        Assert.Equal("openai", span.InitAttributes["gen_ai.provider.name"]);
        Assert.Equal("gpt-4o-mini-tts", span.InitAttributes["gen_ai.request.model"]);
        Assert.Equal("speech", span.InitAttributes["gen_ai.output.type"]);
        Assert.Equal(false, span.InitAttributes["gen_ai.request.stream"]);
        Assert.Equal(recordInputs == false ? null : text, Attribute(span, "ai.request.text"));
        Assert.Equal(
            recordInputs == false ? null : Messages("user", text),
            Attribute(span, "gen_ai.input.messages")
        );
        Assert.Null(Attribute(span, "gen_ai.output.messages"));
        Assert.Equal(
            recordOutputs == false ? null : 1234.0,
            Attribute(span, "ai.response.audio.size")
        );
        Assert.Equal(
            recordOutputs == false ? null : "audio/mpeg",
            Attribute(span, "ai.response.audio.media_type")
        );
        Assert.Equal(
            recordOutputs == false ? null : "mp3",
            Attribute(span, "ai.response.audio.format")
        );
        Assert.Equal(5.0, Attribute(span, "gen_ai.usage.characters"));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Test + "records transcription audio metadata and transcript %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_transcription_audio_metadata_and_transcript(
        bool? recordInputs,
        bool? recordOutputs,
        string text
    )
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var audio = new TranscriptionAudioInfo(4321, "audio/mpeg");

        await integration.OnStart!(
            new TranscriptionStartEvent(
                "call-1",
                "openai.transcription",
                "gpt-4o-transcribe",
                audio,
                2,
                null,
                Json("{}"),
                null,
                null,
                recordInputs,
                recordOutputs,
                null
            ),
            default
        );
        await integration.OnEnd!(
            new TranscriptionEndEvent(
                "call-1",
                "openai.transcription",
                "gpt-4o-transcribe",
                audio,
                text,
                [],
                null,
                "en",
                1,
                Json("""{"seconds":1}"""),
                [],
                null,
                new ProviderResponse(timestamp: DateTime.UnixEpoch, modelId: "gpt-4o-transcribe"),
                recordInputs,
                recordOutputs,
                null
            ),
            default
        );

        var span = Assert.Single(tracer.Spans);
        Assert.Equal("ai.transcribe gpt-4o-transcribe", span.Name);
        Assert.True(span.Ended);
        Assert.Equal("text", span.InitAttributes["gen_ai.output.type"]);
        Assert.Equal(false, span.InitAttributes["gen_ai.request.stream"]);
        Assert.Equal(
            recordInputs == false ? null : 4321.0,
            Attribute(span, "ai.request.audio.size")
        );
        Assert.Equal(
            recordInputs == false ? null : "audio/mpeg",
            Attribute(span, "ai.request.audio.media_type")
        );
        Assert.Equal(recordOutputs == false ? null : text, Attribute(span, "ai.response.text"));
        Assert.Equal(1.0, Attribute(span, "gen_ai.usage.seconds"));
        Assert.Equal(
            recordOutputs == false ? null : Messages("assistant", text),
            Attribute(span, "gen_ai.output.messages")
        );
        Assert.Null(Attribute(span, "gen_ai.input.messages"));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Test + "records streaming transcript %j through isolated callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_streaming_transcript_through_isolated_callbacks(
        bool? recordInputs,
        bool? recordOutputs,
        string text
    )
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStreamTranscriptionStart!(
            new StreamTranscriptionStartEvent(
                "call-1",
                "openai.transcription",
                "gpt-realtime-whisper",
                new TranscriptionAudioInfo(null, "audio/pcm"),
                new AudioFormat("audio/pcm", 24000),
                null,
                null,
                Json("{}")
            )
            {
                RecordInputs = recordInputs,
                RecordOutputs = recordOutputs,
            },
            default
        );
        await integration.OnStreamTranscriptionEnd!(
            new StreamTranscriptionEndEvent(
                "call-1",
                "openai.transcription",
                "gpt-realtime-whisper",
                new TranscriptionAudioInfo(2048, "audio/pcm"),
                text,
                [],
                "en",
                1,
                Json("""{"inputTokens":3}"""),
                [],
                null,
                new ProviderResponse(timestamp: DateTime.UnixEpoch, modelId: "gpt-realtime-whisper")
            )
            {
                RecordInputs = recordInputs,
                RecordOutputs = recordOutputs,
            },
            default
        );

        var span = Assert.Single(tracer.Spans);
        Assert.Equal("ai.streamTranscribe gpt-realtime-whisper", span.Name);
        Assert.True(span.Ended);
        Assert.Equal("text", span.InitAttributes["gen_ai.output.type"]);
        Assert.Equal(true, span.InitAttributes["gen_ai.request.stream"]);
        Assert.Equal(
            recordInputs == false ? null : "audio/pcm",
            Attribute(span, "ai.request.audio.media_type")
        );
        Assert.Equal(
            recordInputs == false ? null : 2048.0,
            Attribute(span, "ai.request.audio.size")
        );
        Assert.Equal(recordOutputs == false ? null : text, Attribute(span, "ai.response.text"));
        Assert.Equal(3.0, Attribute(span, "gen_ai.usage.input_tokens"));
        Assert.Equal(
            recordOutputs == false ? null : Messages("assistant", text),
            Attribute(span, "gen_ai.output.messages")
        );
        Assert.Null(Attribute(span, "gen_ai.input.messages"));
    }
}
