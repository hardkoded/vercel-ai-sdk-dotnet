// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>provider options</c>.</summary>
public sealed class ProviderOptionsTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > provider options::";

    [Fact]
    [UpstreamTest(Id + "should map safetyTolerance and version into the request", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMapSafetyToleranceAndVersionIntoTheRequest()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"safetyTolerance\":0,\"version\":\"latest\"}"), CancellationToken.None);

        AssertMatches("{\"safety_tolerance\":0,\"version\":\"latest\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should reject an unavailable version", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectAnUnavailableVersion()
    {
        var server = CreateServer();

        var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"version\":\"2026-08-01\"}"), CancellationToken.None));

        Assert.Contains("invalid blackForestLabs provider options", exception.Message);
        Assert.Empty(server.Calls);
    }
}
