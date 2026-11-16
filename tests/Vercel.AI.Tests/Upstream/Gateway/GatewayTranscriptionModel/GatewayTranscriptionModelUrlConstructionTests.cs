// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel.GatewayTranscriptionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel;

/// <summary>Port of <c>gateway-transcription-model.test.ts</c> &gt; <c>GatewayTranscriptionModel &gt; URL construction</c>.</summary>
public sealed class GatewayTranscriptionModelUrlConstructionTests
{
    private const string Prefix = "packages/gateway/src/gateway-transcription-model.test.ts::GatewayTranscriptionModel > URL construction::";

    [Fact]
    [UpstreamTest(Prefix + "should post to /transcription-model endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_post_to_transcription_model_endpoint()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler));

        Assert.Equal("https://api.test.com/transcription-model", handler.Uri);
    }
}
