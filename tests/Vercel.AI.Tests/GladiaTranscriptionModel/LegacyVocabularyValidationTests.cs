// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class LegacyVocabularyValidationTests
{
    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::legacy vocabulary validation::filters invalid legacy entries without crashing",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public async Task FiltersInvalidLegacyEntriesWithoutCrashing()
    {
        var handler = GladiaFixtures.Handler();
        var request = GladiaFixtures.Options(
            """
            {"customVocabulary":["Gladia",{"value":"AI SDK","intensity":0.5},null,42,{},{"value":3},
              {"value":"Invalid intensity","intensity":"high"},
              {"value":"Invalid pronunciations","pronunciations":[3]},
              {"value":"Invalid language","language":42}]}
            """
        );
        var result = await GladiaFixtures
            .Provider(handler)
            .Transcription()
            .TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);
        Assert.Equal(
            """["Gladia",{"value":"AI SDK","intensity":0.5}]""",
            GladiaFixtures.InitBody(handler)["custom_vocabulary_config"]![
                "vocabulary"
            ]!.ToJsonString()
        );
        Assert.Collection(
            result.Warnings,
            warning =>
            {
                var deprecated = Assert.IsType<DeprecatedWarning>(warning);
                Assert.Equal("customVocabulary", deprecated.Setting);
                Assert.Equal(
                    "Use customVocabulary: true with customVocabularyConfig instead of an array.",
                    deprecated.Message
                );
            },
            warning =>
                Assert.Equal(
                    "Invalid customVocabulary entries were ignored; use strings or objects with a string value and correctly typed optional fields.",
                    Assert.IsType<OtherWarning>(warning).Message
                )
        );
    }

    [Theory]
    [InlineData("""{"customVocabulary":[]}""")]
    [InlineData("""{"customVocabulary":[null,{"value":"Invalid","intensity":"high"}]}""")]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::legacy vocabulary validation::omits vocabulary configuration when no valid legacy entries remain: $customVocabulary",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public async Task OmitsVocabularyConfigurationWhenNoValidLegacyEntriesRemain(string options)
    {
        var handler = GladiaFixtures.Handler();
        await GladiaFixtures
            .Provider(handler)
            .Transcription()
            .TranscribeAsync(
                GladiaFixtures.Audio(),
                GladiaFixtures.Options(options),
                CancellationToken.None
            );
        var body = GladiaFixtures.InitBody(handler);
        Assert.False(body.ContainsKey("custom_vocabulary"));
        Assert.False(body.ContainsKey("custom_vocabulary_config"));
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-model.test.ts::legacy vocabulary validation::warns when an explicit vocabulary overrides the legacy array",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public async Task WarnsWhenAnExplicitVocabularyOverridesTheLegacyArray()
    {
        var handler = GladiaFixtures.Handler();
        var request = GladiaFixtures.Options(
            """{"customVocabulary":["Legacy"],"customVocabularyConfig":{"vocabulary":["Modern"],"defaultIntensity":0.4}}"""
        );
        var result = await GladiaFixtures
            .Provider(handler)
            .Transcription()
            .TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);
        Assert.Equal(
            """{"vocabulary":["Modern"],"default_intensity":0.4}""",
            GladiaFixtures.InitBody(handler)["custom_vocabulary_config"]!.ToJsonString()
        );
        Assert.Contains(result.Warnings, warning =>
            warning is OtherWarning other
            && other.Message
                == "customVocabularyConfig.vocabulary overrides the deprecated customVocabulary array."
        );
    }
}
