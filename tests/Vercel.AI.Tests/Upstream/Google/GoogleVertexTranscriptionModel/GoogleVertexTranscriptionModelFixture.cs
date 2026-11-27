// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexTranscriptionModel;

/// <summary>Maps to the module-level provider, URLs and <c>prepareJsonResponse</c> of <c>google-vertex-transcription-model.test.ts</c>.</summary>
internal static class GoogleVertexTranscriptionModelFixture
{
    internal const string Url =
        "https://us-central1-speech.googleapis.com/v2/projects/test-project/locations/us-central1/recognizers/_:recognize";
    internal const string UsUrl =
        "https://us-speech.googleapis.com/v2/projects/test-project/locations/us/recognizers/_:recognize";
    internal const string GlobalUrl =
        "https://speech.googleapis.com/v2/projects/test-project/locations/global/recognizers/_:recognize";

    // 8 bytes of audio; base64 -> 'AQIDBAUGBwg='.
    internal static readonly byte[] AudioData = [1, 2, 3, 4, 5, 6, 7, 8];
    internal const string AudioBase64 = "AQIDBAUGBwg=";

    internal const string DefaultBody =
        "{\"results\":[{\"alternatives\":[{\"transcript\":\"hello world\",\"words\":[{\"word\":\"hello\",\"startOffset\":\"0s\",\"endOffset\":\"0.500s\"},{\"word\":\"world\",\"startOffset\":\"0.500s\",\"endOffset\":\"1s\"}]}],\"languageCode\":\"en-US\"}],\"metadata\":{\"totalBilledDuration\":\"1s\"}}";

    internal static GoogleVertexProvider Provider(
        RecordingHandler handler,
        Action<VertexOptions>? configure = null,
        DateTimeOffset? now = null
    )
    {
        var options = new VertexOptions
        {
            Project = "test-project",
            Region = "us-central1",
            ApiKey = "k",
        };
        configure?.Invoke(options);
        var provider = GoogleVertexProvider.Create(options, handler);
        if (now is { } clock)
        {
            provider.Clock = () => clock;
        }

        return provider;
    }

    internal static ITranscriptionCaller Model(
        RecordingHandler handler,
        string modelId = "chirp_2",
        Action<VertexOptions>? configure = null,
        DateTimeOffset? now = null
    )
    {
        return (ITranscriptionCaller)Provider(handler, configure, now).TranscriptionModel(modelId);
    }

    internal static RecordingHandler Server(string body = DefaultBody)
    {
        return new RecordingHandler { ResponseText = body };
    }

    internal static TranscriptionModelCall Call(
        byte[]? audio = null,
        string? audioBase64 = null,
        string providerOptions = "{}",
        Dictionary<string, string>? headers = null
    )
    {
        var options = JsonDocument.Parse(providerOptions).RootElement;
        var map = headers ?? new Dictionary<string, string>();
        return audioBase64 is not null
            ? new TranscriptionModelCall(
                audioBase64,
                "audio/wav",
                options,
                map,
                CancellationToken.None
            )
            : new TranscriptionModelCall(
                audio ?? AudioData,
                "audio/wav",
                options,
                map,
                CancellationToken.None
            );
    }
}
