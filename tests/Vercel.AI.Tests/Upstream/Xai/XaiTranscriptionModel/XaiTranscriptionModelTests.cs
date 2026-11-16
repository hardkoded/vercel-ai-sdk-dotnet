// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Xai.XaiTranscriptionModel;

/// <summary>Port of <c>xai-transcription-model.test.ts</c> &gt; <c>XaiTranscriptionModel</c>.</summary>
public sealed class XaiTranscriptionModelTests
{
    private const string Prefix = "packages/xai/src/xai-transcription-model.test.ts::XaiTranscriptionModel::";

    [Fact]
    [UpstreamTest(Prefix + "should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Should_expose_correct_provider_and_model_information()
    {
        var model = XaiTranscriptionTestSupport.Model();

        Assert.Equal("xai.transcription", model.Provider);
        Assert.Equal(string.Empty, model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
    }
}
