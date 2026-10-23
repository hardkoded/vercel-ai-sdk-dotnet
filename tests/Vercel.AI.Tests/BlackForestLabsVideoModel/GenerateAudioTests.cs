// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>generateAudio</c>.</summary>
public sealed class GenerateAudioTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > generateAudio::";

    [Fact]
    [UpstreamTest(Id + "should pass generateAudio through as generate_audio", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassGenerateAudioThroughAsGenerate_audio()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(generateAudio: true), CancellationToken.None);

        AssertMatches("{\"generate_audio\":true}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send generate_audio when it is explicitly disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendGenerate_audioWhenItIsExplicitlyDisabled()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(generateAudio: false), CancellationToken.None);

        AssertMatches("{\"generate_audio\":false}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should omit generate_audio when it is unset, leaving the API default", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitGenerate_audioWhenItIsUnsetLeavingTheApiDefault()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        AssertNoProperty("generate_audio", RequestBody(server));
    }
}
