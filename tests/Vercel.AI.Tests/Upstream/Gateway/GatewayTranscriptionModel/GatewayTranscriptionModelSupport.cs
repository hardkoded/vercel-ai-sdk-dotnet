// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel;

/// <summary>Helpers of <c>gateway-transcription-model.test.ts</c>.</summary>
internal static class GatewayTranscriptionModelSupport
{
    public const string ModelId = "openai/gpt-4o-transcribe";

    /// <summary>Stands for upstream's <c>'base64-audio'</c> input. The port takes audio as bytes.</summary>
    public static readonly byte[] Audio = { 1, 2, 3 };

    public static Vercel.AI.Gateway.GatewayTranscriptionModel CreateTestModel(HttpMessageHandler handler, string modelId = ModelId, IReadOnlyDictionary<string, string>? o11yHeaders = null)
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com", O11yHeaders = o11yHeaders ?? new Dictionary<string, string>() },
            handler);
        return (Vercel.AI.Gateway.GatewayTranscriptionModel)provider.TranscriptionModel(modelId);
    }

    /// <summary>Maps to <c>prepareJsonResponse</c>.</summary>
    public static UpstreamRecordingHandler PrepareJsonResponse(string text = "Hello world")
    {
        return new UpstreamRecordingHandler("{\"text\":\"" + text + "\"}");
    }

    public static Task<TranscriptionModelResult> DoGenerate(
        Vercel.AI.Gateway.GatewayTranscriptionModel model,
        byte[]? audio = null,
        string? audioBase64 = null,
        string mediaType = "audio/wav",
        string? providerOptions = null,
        IReadOnlyList<string>? timestampGranularities = null,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var options = providerOptions is null ? default : JsonDocument.Parse(providerOptions).RootElement.Clone();
        var callHeaders = headers ?? new Dictionary<string, string>();
        var call = audioBase64 is null
            ? new TranscriptionModelCall(audio ?? Audio, mediaType, options, callHeaders, CancellationToken.None) { TimestampGranularities = timestampGranularities }
            : new TranscriptionModelCall(audioBase64, mediaType, options, callHeaders, CancellationToken.None) { TimestampGranularities = timestampGranularities };
        return model.DoGenerateAsync(call, CancellationToken.None);
    }

    public static JsonNode? Body(UpstreamRecordingHandler handler) => JsonNode.Parse(handler.Body);

    public static bool Equal(string expected, JsonNode? actual) => JsonNode.DeepEquals(JsonNode.Parse(expected), actual);

    public const string StreamingModelId = "openai/gpt-realtime-whisper";

    /// <summary>Maps to <c>createStreamingTestModel</c>. <paramref name="headers"/> stands for the <c>headers</c> config.</summary>
    public static Vercel.AI.Gateway.GatewayTranscriptionModel CreateStreamingTestModel(
        IReadOnlyDictionary<string, string?>? headers = null,
        Func<DateTimeOffset>? currentDate = null)
    {
        var provider = GatewayProvider.Create(new GatewayOptions
        {
            ApiKey = "test-token",
            BaseUrl = "https://api.test.com",
            O11yHeaders = new Dictionary<string, string>(),
            WebSocket = MockWebSocket.Factory,
            CurrentDate = currentDate,
        });
        var model = (Vercel.AI.Gateway.GatewayTranscriptionModel)provider.TranscriptionModel(StreamingModelId);
        var configHeaders = headers ?? new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer test-token",
            ["ai-gateway-auth-method"] = "api-key",
        };
        model.HeadersResolver = _ => Task.FromResult(configHeaders);
        return model;
    }

    public static AudioChunkStream AudioOf(params byte[][] chunks)
    {
        var stream = new AudioChunkStream();
        foreach (var chunk in chunks)
        {
            stream.Write(chunk);
        }

        stream.Complete();
        return stream;
    }

    public static Task<TranscriptionStreamStart?> DoStream(
        Vercel.AI.Gateway.GatewayTranscriptionModel model,
        AudioChunkStream? audio = null,
        AudioFormat? inputAudioFormat = null,
        string? providerOptions = null,
        bool? includeRawChunks = null,
        IReadOnlyDictionary<string, string?>? headers = null,
        CancellationToken abortSignal = default,
        Exception? abortReason = null)
    {
        var options = providerOptions is null ? default : JsonDocument.Parse(providerOptions).RootElement.Clone();
        return model.DoStreamAsync(
            new TranscriptionStreamCall(
                audio ?? AudioOf(new byte[] { 1, 2, 3 }),
                inputAudioFormat ?? new AudioFormat("audio/pcm", 16000),
                options,
                headers ?? new Dictionary<string, string?>(),
                includeRawChunks,
                abortSignal,
                abortReason),
            CancellationToken.None);
    }

    /// <summary>Maps to <c>convertReadableStreamToArray</c>.</summary>
    public static async Task<List<ModelStreamPart>> Collect(TranscriptionStreamStart start)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in start.Stream.ReadAsync())
        {
            parts.Add(part);
        }

        return parts;
    }

    public static JsonNode? Parse(object frame) => JsonNode.Parse((string)frame);

    public static JsonNode Node(string json) => JsonNode.Parse(json)!;
}
