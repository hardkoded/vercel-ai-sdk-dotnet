// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
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
    [UpstreamTest(Prefix + "delegates OpenAI streaming without changing call options", Coverage = UpstreamCoverage.Covered)]
    public async Task Delegates_OpenAI_streaming_without_changing_call_options()
    {
        var harness = Setup();
        var result = new TranscriptionStreamStart(new ModelPartStream(Array.Empty<ModelStreamPart>()));
        var spy = new DoStreamSpy(result);
        var model = harness.Provider.Transcription("mai-transcribe-2");
        model.OpenAIStreamModel = spy;
        var audio = new AudioChunkStream();
        var providerOptions = JsonSerializer.Deserialize<JsonElement>("{\"azure\":{\"api\":\"openai\"},\"openai\":{\"language\":\"en\"}}");
        var call = new TranscriptionStreamCall(audio, new AudioFormat("audio/pcm", 24000), providerOptions, new Dictionary<string, string?>(), null, CancellationToken.None);

        Assert.Same(result, await model.DoStreamAsync(call, CancellationToken.None));

        var received = Assert.Single(spy.Calls);
        Assert.Same(audio, received.Audio);
        Assert.Equal(call.InputAudioFormat.Type, received.InputAudioFormat.Type);
        Assert.Equal(call.InputAudioFormat.Rate, received.InputAudioFormat.Rate);
        Assert.Equal(providerOptions.GetRawText(), received.ProviderOptions.GetRawText());
        Assert.Same(call.Headers, received.Headers);
        Assert.Equal(call.IncludeRawChunks, received.IncludeRawChunks);
        Assert.Equal(call.CancellationToken, received.CancellationToken);
    }

    private sealed class DoStreamSpy(TranscriptionStreamStart result) : ITranscriptionCaller
    {
        public List<TranscriptionStreamCall> Calls { get; } = new();

        public string Provider => "spy";

        public string ModelId => "spy";

        public string SpecificationVersion => "v4";

        public bool CanStream => true;

        public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
        {
            Calls.Add(call);
            return Task.FromResult<TranscriptionStreamStart?>(result);
        }
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
            new Dictionary<string, string?>(),
            null,
            CancellationToken.None);

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => harness.Transcription("mai-transcribe-2").DoStreamAsync(call, CancellationToken.None));

        Assert.Empty(harness.Handler.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "restores request-time routing after a workflow serialization round trip", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
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
        Assert.Equal("https://proxy.example/audio/transcriptions", harness.Request.Url);
    }
}
