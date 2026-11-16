// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

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
}
