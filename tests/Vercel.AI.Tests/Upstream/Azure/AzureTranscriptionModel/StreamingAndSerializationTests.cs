// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;
using AzureModel = Vercel.AI.Azure.AzureTranscriptionModel;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>streaming and serialization</c>.</summary>
[Collection("MockWebSocket")]
public sealed class StreamingAndSerializationTests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::streaming and serialization::";

    [Fact]
    [UpstreamTest(Prefix + "delegates OpenAI streaming without changing call options", Coverage = UpstreamCoverage.Partial, Note = "Upstream spies on OpenAITranscriptionModel.doStream and checks the result and options by identity. Upstream calls the mai-transcribe-2 deployment, which the real OpenAI model does not stream, so the port streams the gpt-realtime-whisper deployment and checks that the OpenAI realtime session is opened with the unchanged openai options.")]
    public async Task Partial_Delegates_OpenAI_streaming_without_changing_call_options()
    {
        var stream = await AzureMaiTestSupport.StartStream(
            options => options.NativeWebSocket = MockWebSocket.Factory,
            modelId: "gpt-realtime-whisper",
            providerOptions: "{\"azure\":{\"api\":\"openai\"},\"openai\":{\"language\":\"en\"}}",
            customWebSocket: false);

        var transcription = JsonNode.Parse(stream.Start.Request!.Body!)!["session"]!["audio"]!["input"]!["transcription"]!;
        Assert.Equal("gpt-realtime-whisper", transcription["model"]!.GetValue<string>());
        Assert.Equal("en", transcription["language"]!.GetValue<string>());
        Assert.Contains("/realtime?intent=transcription&api-version=", stream.Socket.Url.AbsoluteUri, StringComparison.Ordinal);
        Assert.Equal(new[] { "realtime" }, stream.Socket.Protocols);
        stream.Start.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects Speech streaming before sending audio", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Rejects_Speech_streaming_before_sending_audio()
    {
        var harness = Setup();
        var call = new TranscriptionStreamCall(
            new AudioChunkStream(),
            new AudioFormat("audio/pcm", 24000),
            JsonSerializer.Deserialize<JsonElement>("{}"),
            new Dictionary<string, string>(),
            null,
            CancellationToken.None);

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => harness.Transcription("mai-transcribe-2").DoStreamAsync(call, CancellationToken.None));

        Assert.Empty(harness.Handler.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "restores request-time routing after a workflow serialization round trip", Coverage = UpstreamCoverage.Partial, Note = "The OpenAI route of a custom base URL is https://proxy.example/openai/v1/audio/transcriptions?api-version=v1 in this port. Upstream sends a custom gateway URL to /audio/transcriptions without the Azure path.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Restores_request_time_routing_after_a_workflow_serialization_round_trip()
    {
        var harness = Setup(options =>
        {
            options.BaseUrl = "https://proxy.example";
            options.SpeechBaseUrl = "https://speech.example";
        });
        var model = harness.Provider.Transcription("mai-transcribe-2");

        var serialized = model.Serialize();

        Assert.False(serialized.Config.ContainsKey("fetch"));
        var restored = AzureModel.Deserialize(serialized, "test-key", harness.Handler);
        await restored.DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal("https://speech.example/speechtotext/transcriptions:transcribe?api-version=2025-10-15", harness.Request.Url);
        await restored.DoGenerateAsync(Call("{\"azure\":{\"api\":\"openai\"}}"), CancellationToken.None);
        Assert.Equal("https://proxy.example/openai/v1/audio/transcriptions?api-version=v1", harness.Request.Url);
    }
}
