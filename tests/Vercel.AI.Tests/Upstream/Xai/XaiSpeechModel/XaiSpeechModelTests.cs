// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Xai.XaiSpeechModel.XaiSpeechTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiSpeechModel;

/// <summary>Port of <c>xai-speech-model.test.ts</c> &gt; <c>XaiSpeechModel</c>.</summary>
public sealed class XaiSpeechModelTests
{
    private const string Id = "packages/xai/src/xai-speech-model.test.ts::XaiSpeechModel::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_expose_correct_provider_and_model_information()
    {
        var model = Speech(CreateProvider(AudioHandler()));

        Assert.Equal("xai.speech", model.Provider);
        Assert.Equal("", model.ModelId);
        Assert.Equal("v4", ((Vercel.AI.Xai.XaiSpeechModel)model).SpecificationVersion);
    }
}
