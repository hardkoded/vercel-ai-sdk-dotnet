// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;

namespace Vercel.AI.Tests.GladiaTranscriptionOptionsResolution;

public sealed class ResolveCustomVocabularyTests
{
    private static ResolvedCustomVocabulary Resolve(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Vercel.AI.Gladia.GladiaTranscriptionOptionsResolution.ResolveCustomVocabulary(
            document.RootElement
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveCustomVocabulary::maps a vocabulary array to customVocabularyConfig vocabulary",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void MapsAVocabularyArrayToCustomVocabularyConfigVocabulary()
    {
        var result = Resolve("""{"customVocabulary":["Gladia","AI SDK"]}""");
        Assert.True(result.CustomVocabulary);
        Assert.Equal("""["Gladia","AI SDK"]""", result.Vocabulary!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/gladia/src/gladia-transcription-options-resolution.test.ts::resolveCustomVocabulary::passes through boolean customVocabulary values",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb"
    )]
    public void PassesThroughBooleanCustomVocabularyValues()
    {
        var result = Resolve("""{"customVocabulary":true}""");
        Assert.True(result.CustomVocabulary);
        Assert.Null(result.Vocabulary);
    }
}
