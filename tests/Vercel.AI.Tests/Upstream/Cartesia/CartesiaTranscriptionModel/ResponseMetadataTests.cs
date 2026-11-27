// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Cartesia;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaTranscriptionModel;

/// <summary>Port of <c>cartesia-transcription-model.test.ts</c> &gt; <c>doGenerate &gt; response metadata</c>.</summary>
public sealed class ResponseMetadataTests
{
    private const string Id =
        "packages/cartesia/src/cartesia-transcription-model.test.ts::doGenerate > ";

    private const string TranscriptionJson =
        "{\"text\":\"Hello from the Vercel AI SDK.\",\"language\":\"en\",\"duration\":2.479,\"words\":[{\"word\":\"Hello\",\"start\":0.199,\"end\":0.479},{\"word\":\"from\",\"start\":0.5,\"end\":0.639},{\"word\":\"the\",\"start\":0.66,\"end\":0.759},{\"word\":\"Vercel\",\"start\":0.759,\"end\":1.12},{\"word\":\"AI\",\"start\":1.2,\"end\":1.519},{\"word\":\"SDK.\",\"start\":1.58,\"end\":2.479}]}";

    [Fact]
    [UpstreamTest(
        Id + "response metadata::should include response data with timestamp and modelId",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_the_timestamp_and_model()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(TranscriptionJson));
        var provider = CartesiaProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = new global::Vercel.AI.Cartesia.CartesiaTranscriptionModel(
            provider,
            "ink-whisper",
            () => DateTime.UnixEpoch
        );
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal(DateTime.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("ink-whisper", result.Response.ModelId);
    }

    private static global::Vercel.AI.Operations.TranscriptionModelCall TranscriptionCall()
    {
        return new global::Vercel.AI.Operations.TranscriptionModelCall(
            new byte[] { 1, 2, 3 },
            "audio/wav",
            System.Text.Json.JsonDocument.Parse("{}").RootElement.Clone(),
            new Dictionary<string, string>(),
            CancellationToken.None
        );
    }
}
