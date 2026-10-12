// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = TestFile + " > doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported sampling settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_unsupported_sampling_settings()
    {
        var capture = ChunksFixture("xai-text-streaming.1.chunks.txt");
        var options = HelloPrompt();
        options.TopK = 10;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;

        var parts = await Collect(Model(capture), options);

        var start = First<StreamStartStreamPart>(parts);
        Assert.Equal(new[] { "frequencyPenalty", "presencePenalty" }, start.Warnings.Select(warning => warning.Message));
        Assert.All(start.Warnings, warning => Assert.Equal("unsupported", warning.Type));
    }
}
