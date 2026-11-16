// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class LegacyOptionWarningsTests
{
    [Theory]
    [InlineData("contextPrompt", """{"contextPrompt":"context"}""")]
    [InlineData("moderation", """{"moderation":true}""")]
    [InlineData("chapterization", """{"chapterization":true}""")]
    [InlineData("nameConsistency", """{"nameConsistency":true}""")]
    [InlineData("structuredDataExtraction", """{"structuredDataExtraction":true}""")]
    [InlineData(
        "structuredDataExtractionConfig",
        """{"structuredDataExtractionConfig":{"classes":["test"]}}"""
    )]
    [InlineData("displayMode", """{"displayMode":false}""")]
    [InlineData("diarizationConfig.enhanced", """{"diarizationConfig":{"enhanced":true}}""")]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::legacy option warnings::warns when %s is ignored",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public async Task WarnsWhenOptionIsIgnored(string feature, string options)
    {
        var handler = GladiaFixtures.Handler();
        var result = await GladiaFixtures
            .Provider(handler)
            .Transcription()
            .TranscribeAsync(
                GladiaFixtures.Audio(),
                GladiaFixtures.Options(options),
                CancellationToken.None
            );
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(result.Warnings));
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(
            "No longer supported by the Gladia v2 pre-recorded API; ignored.",
            warning.Details
        );
        var body = GladiaFixtures.InitBody(handler);
        var expected =
            feature == "diarizationConfig.enhanced"
                ? """{"audio_url":"https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6","diarization_config":{}}"""
                : """{"audio_url":"https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6"}""";
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), body));
    }

    [Theory]
    [InlineData("language", """{"language":"en"}""")]
    [InlineData("detectLanguage", """{"detectLanguage":true}""")]
    [InlineData("enableCodeSwitching", """{"enableCodeSwitching":false}""")]
    [InlineData("codeSwitchingConfig", """{"codeSwitchingConfig":{"languages":["en"]}}""")]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::legacy option warnings::warns when translating deprecated %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public async Task WarnsWhenTranslatingDeprecatedOption(string setting, string options)
    {
        var handler = GladiaFixtures.Handler();
        var result = await GladiaFixtures
            .Provider(handler)
            .Transcription()
            .TranscribeAsync(
                GladiaFixtures.Audio(),
                GladiaFixtures.Options(options),
                CancellationToken.None
            );
        Assert.Contains(result.Warnings, warning =>
            warning is DeprecatedWarning deprecated
            && deprecated.Setting == setting
            && deprecated.Message == "Use languageConfig instead."
        );
    }
}
