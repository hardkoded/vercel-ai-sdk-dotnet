// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;

namespace Vercel.AI.Tests.GladiaTranscriptionOptionsResolution;

public sealed class ResolveLanguageConfigTests
{
    private static ResolvedLanguageConfig? Resolve(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Vercel.AI.Gladia.GladiaTranscriptionOptionsResolution.ResolveLanguageConfig(
            document.RootElement
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveLanguageConfig::returns undefined when no language options are provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void ReturnsUndefinedWhenNoLanguageOptionsAreProvided()
    {
        Assert.Null(Resolve("{}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveLanguageConfig::uses languageConfig when provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void UsesLanguageConfigWhenProvided()
    {
        var result = Resolve("""{"languageConfig":{"languages":["en"],"codeSwitching":true}}""")!;
        Assert.Equal(["en"], result.Languages!);
        Assert.True(result.CodeSwitching);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveLanguageConfig::maps deprecated language to languageConfig.languages",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void MapsDeprecatedLanguageToLanguageConfigLanguages()
    {
        var result = Resolve("""{"language":"en"}""")!;
        Assert.Equal(["en"], result.Languages!);
        Assert.Null(result.CodeSwitching);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveLanguageConfig::maps deprecated code switching options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void MapsDeprecatedCodeSwitchingOptions()
    {
        var result = Resolve(
            """{"enableCodeSwitching":true,"codeSwitchingConfig":{"languages":["en","fr"]}}"""
        )!;
        Assert.Equal(["en", "fr"], result.Languages!);
        Assert.True(result.CodeSwitching);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveLanguageConfig::maps detectLanguage to an empty languages array",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void MapsDetectLanguageToAnEmptyLanguagesArray()
    {
        var result = Resolve("""{"detectLanguage":true}""")!;
        Assert.Empty(result.Languages!);
        Assert.Null(result.CodeSwitching);
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveLanguageConfig::prefers languageConfig over deprecated options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void PrefersLanguageConfigOverDeprecatedOptions()
    {
        var result = Resolve("""{"language":"en","languageConfig":{"languages":["de"]}}""")!;
        Assert.Equal(["de"], result.Languages!);
        Assert.Null(result.CodeSwitching);
    }
}
