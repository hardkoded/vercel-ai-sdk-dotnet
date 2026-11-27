// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewaySpeechModel.GatewaySpeechModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewaySpeechModel;

/// <summary>Port of <c>gateway-speech-model.test.ts</c> &gt; <c>GatewaySpeechModel &gt; URL construction</c>.</summary>
public sealed class GatewaySpeechModelUrlConstructionTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-speech-model.test.ts::GatewaySpeechModel > URL construction::";

    [Fact]
    [UpstreamTest(
        Prefix + "should post to /speech-model endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_post_to_speech_model_endpoint()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler));

        Assert.Equal("https://api.test.com/speech-model", handler.Uri);
    }
}
