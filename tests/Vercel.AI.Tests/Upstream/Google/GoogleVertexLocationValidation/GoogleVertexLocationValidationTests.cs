// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexLocationValidation;

/// <summary>Port of the top-level cases of <c>google-vertex-location-validation.test.ts</c>.</summary>
public sealed class GoogleVertexLocationValidationTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-location-validation.test.ts::";

    [Fact]
    [UpstreamTest(
        Prefix + "does not validate location in express mode",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Does_not_validate_location_in_express_mode()
    {
        GoogleVertexProvider
            .Create(
                new VertexOptions
                {
                    ApiKey = "test-key",
                    Region = "not a region",
                    UpstreamRoutes = true,
                },
                new RecordingHandler()
            )
            .LanguageModel("test-model");
    }

    [Theory]
    [InlineData("chirp_3")]
    [InlineData("gemini-3.5-transcribe-live")]
    [UpstreamTest(
        Prefix + "validates location for the separate transcription host of %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Validates_location_for_the_separate_transcription_host(string model)
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test",
                Region = "user@internal/#",
                BaseUrl = "https://proxy.example/v1",
                UpstreamRoutes = true,
                GenerateAuthToken = _ => Task.FromResult("token"),
            },
            new RecordingHandler()
        );

        Assert.ThrowsAny<ArgumentException>(() => provider.TranscriptionModel(model));
    }
}
