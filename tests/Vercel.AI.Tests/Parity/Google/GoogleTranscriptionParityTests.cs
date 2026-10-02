// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Unary Interactions transcription. Live WebSocket cases are not covered.</summary>
public sealed class GoogleTranscriptionParityTests
{
    private const string File = "packages/google/src/transcription/google-transcription-model.test.ts::doGenerate::";

    [Fact]
    [UpstreamTest(File + "transcribes audio via the Interactions API with transcription_config", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcribes_audio_with_a_transcription_config()
    {
        var capture = new GoogleCapture
        {
            ResponseJson = """
                {"id":"interactions/test","status":"completed","steps":[{"type":"model_output","content":[{"type":"text","text":"Hello world."}]}],"usage":{"total_tokens":10,"total_input_tokens":10,"total_output_tokens":0}}
                """,
        };
        var provider = GoogleParity.Google(capture);
        var model = (GoogleTranscriptionModel)provider.TranscriptionModel("gemini-3.5-transcribe");
        var result = await model.DoTranscribeAsync(
            new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", null),
            GoogleParity.Json("{\"customVocabulary\":[\"Gemini\",\"Kubernetes\"],\"languageCodes\":[\"es-ES\"],\"mode\":\"SMART\"}"),
            CancellationToken.None);
        Assert.Equal("Hello world.", result.Text);
        Assert.Equal(10, result.Usage!.InputTokens);
        Assert.EndsWith("/interactions", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
        Assert.Equal("test-key", GoogleParity.Header(capture, "x-goog-api-key"));
        GoogleParity.Equal(GoogleParity.Request(capture), """
            {"model":"gemini-3.5-transcribe","input":[{"type":"audio","data":"AQIDBA==","mime_type":"audio/wav"}],"generation_config":{"transcription_config":{"language_codes":["es-ES"],"custom_vocabulary":["Gemini","Kubernetes"],"mode":{"type":"smart"}}}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "maps diarization and word timestamps into the mode object", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_diarization_and_word_timestamps()
    {
        var capture = Transcript();
        var model = (GoogleTranscriptionModel)GoogleParity.Google(capture).TranscriptionModel("gemini-3.5-transcribe");
        await model.DoTranscribeAsync(
            new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", null),
            GoogleParity.Json("{\"diarization\":true,\"wordTimestamp\":true}"),
            CancellationToken.None);
        GoogleParity.Equal(GoogleParity.Request(capture)["generation_config"]!["transcription_config"]!, """
            {"mode":{"type":"verbatim","diarization_mode":"speaker","timestamp_granularities":["word"]}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "omits generation_config when no transcription options are set", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_generation_config_when_no_options_are_set()
    {
        var capture = Transcript();
        var model = GoogleParity.Google(capture).TranscriptionModel("gemini-3.5-transcribe");
        await model.DoTranscribeAsync(new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", null), CancellationToken.None);
        Assert.Null(GoogleParity.Request(capture)["generation_config"]);
    }

    [Fact]
    [UpstreamTest(File + "rejects unary transcription on live model ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unary_transcription_on_live_model_ids()
    {
        var model = GoogleParity.Google(new GoogleCapture()).TranscriptionModel("gemini-3.5-transcribe-live");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => model.DoTranscribeAsync(new AudioInput(new byte[] { 1 }, "audio/wav", null), CancellationToken.None));
        Assert.Contains("only supports streaming transcription", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/gemini-transcription/google-vertex-gemini-transcription-model.test.ts::doGenerate::rejects unary transcription on live model ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unary_Vertex_transcription_on_live_model_ids()
    {
        var model = GoogleParity.Vertex(new GoogleCapture()).TranscriptionModel("gemini-3.5-transcribe-live");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => model.DoTranscribeAsync(new AudioInput(new byte[] { 1 }, "audio/wav", null), CancellationToken.None));
        Assert.Contains("only supports streaming transcription", error.Message, StringComparison.Ordinal);
    }

    private static GoogleCapture Transcript()
    {
        return new GoogleCapture
        {
            ResponseJson = "{\"steps\":[{\"content\":[{\"type\":\"text\",\"text\":\"ok\"}]}]}",
        };
    }
}
