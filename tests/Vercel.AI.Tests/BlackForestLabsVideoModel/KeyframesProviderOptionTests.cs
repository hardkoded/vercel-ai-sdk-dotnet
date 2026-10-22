// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>keyframes provider option</c>.</summary>
public sealed class KeyframesProviderOptionTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > keyframes provider option::";

    public static IEnumerable<object[]> InvalidKeyframes()
    {
        yield return new object[] { "an empty array", "[]" };
        yield return new object[] { "mixed timed and untimed entries", "[\"https://cdn.example.com/a.png\",[3,\"https://cdn.example.com/b.png\"]]" };
        yield return new object[] { "out-of-order timed entries", "[[3,\"https://cdn.example.com/a.png\"],[2,\"https://cdn.example.com/b.png\"]]" };
        yield return new object[] { "a timestamp outside the video range", "[[21,\"data\"]]" };
    }

    [Fact]
    [UpstreamTest(Id + "should send timed keyframe pairs", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendTimedKeyframePairs()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(
            Call(duration: 8, blackForestLabs: "{\"keyframes\":[[0,\"https://cdn.example.com/a.png\"],[3.5,\"https://cdn.example.com/b.png\"]]}"),
            CancellationToken.None);

        AssertMatches(
            "{\"mode\":\"i2v\",\"keyframes\":[[0,\"https://cdn.example.com/a.png\"],[3.5,\"https://cdn.example.com/b.png\"]]}",
            RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should take precedence over top-level frame images and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldTakePrecedenceOverTopLevelFrameImagesAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(image: ImageUrlFile, blackForestLabs: "{\"keyframes\":[\"https://cdn.example.com/a.png\"]}"),
            CancellationToken.None);

        AssertMatches("{\"keyframes\":[\"https://cdn.example.com/a.png\"]}", RequestBody(server));
        Assert.Contains(
            (
                "unsupported",
                "image",
                "FLUX 3 video takes a single keyframe list. providerOptions.blackForestLabs.keyframes was used and the top-level frame images were ignored."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should reject more than 10 keyframes", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectMoreThan10Keyframes()
    {
        var server = CreateServer();
        var keyframes = string.Join(",", Enumerable.Range(0, 12).Select(index => "\"https://cdn.example.com/" + index + ".png\""));

        var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateModel(server).DoGenerateAsync(Call(duration: 10, blackForestLabs: "{\"keyframes\":[" + keyframes + "]}"), CancellationToken.None));

        Assert.Contains("invalid blackForestLabs provider options", exception.Message);
        Assert.Empty(server.Calls);
    }

    [Theory]
    [MemberData(nameof(InvalidKeyframes))]
    [UpstreamTest(Id + "should reject %s", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReject(string name, string keyframes)
    {
        Assert.NotNull(name);
        var server = CreateServer();

        var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"keyframes\":" + keyframes + "}"), CancellationToken.None));

        Assert.Contains("invalid blackForestLabs provider options", exception.Message);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "should reject 3 or more untimed keyframes without a duration", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReject3OrMoreUntimedKeyframesWithoutADuration()
    {
        var server = CreateServer();

        var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateModel(server).DoGenerateAsync(
                Call(blackForestLabs: "{\"keyframes\":[\"https://cdn.example.com/a.png\",\"https://cdn.example.com/b.png\",\"https://cdn.example.com/c.png\"]}"),
                CancellationToken.None));

        Assert.Contains(
            "FLUX 3 video requires an explicit duration when 3 or more keyframes are sent without a timestamp.",
            exception.Message);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "should not warn about duration when the keyframes are timed", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotWarnAboutDurationWhenTheKeyframesAreTimed()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(
            Call(blackForestLabs: "{\"keyframes\":[[0,\"https://cdn.example.com/a.png\"],[2,\"https://cdn.example.com/b.png\"],[4,\"https://cdn.example.com/c.png\"]]}"),
            CancellationToken.None);

        Assert.Empty(result.Warnings);
    }
}
