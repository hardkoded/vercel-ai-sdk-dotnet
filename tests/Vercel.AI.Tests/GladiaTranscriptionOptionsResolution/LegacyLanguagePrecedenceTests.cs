// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;

namespace Vercel.AI.Tests.GladiaTranscriptionOptionsResolution;

public sealed class LegacyLanguagePrecedenceTests
{
    private static ResolvedLanguageConfig? Resolve(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Vercel.AI.Gladia.GladiaTranscriptionOptionsResolution.ResolveLanguageConfig(document.RootElement);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-options-resolution.test.ts::legacy language precedence::treats an explicit config as the complete override", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public void TreatsAnExplicitConfigAsTheCompleteOverride()
    {
        var first = Resolve("""{"languageConfig":{"codeSwitching":false},"language":"en","enableCodeSwitching":true,"codeSwitchingConfig":{"languages":["fr"]}}""")!;
        Assert.Null(first.Languages);
        Assert.False(first.CodeSwitching);
        var second = Resolve("""{"languageConfig":{},"language":"en"}""")!;
        Assert.Null(second.Languages);
        Assert.Null(second.CodeSwitching);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-options-resolution.test.ts::legacy language precedence::does not enable code switching from an empty legacy config", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public void DoesNotEnableCodeSwitchingFromAnEmptyLegacyConfig()
    {
        Assert.Null(Resolve("""{"codeSwitchingConfig":{}}"""));
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-options-resolution.test.ts::legacy language precedence::uses code-switching candidates when enabled", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public void UsesCodeSwitchingCandidatesWhenEnabled()
    {
        var result = Resolve("""{"language":"en","enableCodeSwitching":true,"codeSwitchingConfig":{"languages":["en","fr"]}}""")!;
        Assert.Equal(["en", "fr"], result.Languages!);
        Assert.True(result.CodeSwitching);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-options-resolution.test.ts::legacy language precedence::ignores candidates when code switching is disabled", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public void IgnoresCandidatesWhenCodeSwitchingIsDisabled()
    {
        var result = Resolve("""{"enableCodeSwitching":false,"codeSwitchingConfig":{"languages":["fr"]}}""")!;
        Assert.Null(result.Languages);
        Assert.False(result.CodeSwitching);
    }
}
