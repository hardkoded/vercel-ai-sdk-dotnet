// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleVertexVideoModel.GoogleVertexVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVertexVideoModel;

/// <summary>Port of <c>google-vertex-video-model.test.ts</c> &gt; <c>GoogleVertexVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Id =
        "packages/google-vertex/src/google-vertex-video-model.test.ts::GoogleVertexVideoModel > doStart::";

    [Fact]
    [UpstreamTest(
        Id + "should return operation with operationName",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnOperationWithOperationName()
    {
        var server = new VertexVideoServer { OperationName = "operations/my-op-123" };

        var result = await CreateModel(server).DoStartAsync(Call(), CancellationToken.None);

        AssertJson("{\"operationName\":\"operations/my-op-123\"}", OperationOf(result));
    }

    [Fact]
    [UpstreamTest(Id + "should pass correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassCorrectRequestBody()
    {
        var server = new VertexVideoServer();

        await CreateModel(server).DoStartAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass seed when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassSeedWhenProvided()
    {
        var server = new VertexVideoServer();

        await CreateModel(server).DoStartAsync(Call(seed: 42), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"seed\":42}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass seed when it is 0",
        Coverage = UpstreamCoverage.Covered,
        Commit = "ef2dfb5e9fb7b72287b226eba8b743f809414172"
    )]
    public async Task ShouldPassSeedWhenItIs0()
    {
        var server = new VertexVideoServer();

        await CreateModel(server).DoStartAsync(Call(seed: 0), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"seed\":0}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass aspect ratio when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassAspectRatioWhenProvided()
    {
        var server = new VertexVideoServer();

        await CreateModel(server).DoStartAsync(Call(aspectRatio: "16:9"), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"aspectRatio\":\"16:9\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert resolution to Vertex format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldConvertResolutionToVertexFormat()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(Call(resolution: "1920x1080"), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"resolution\":\"1080p\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass duration as durationSeconds",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassDurationAsDurationSeconds()
    {
        var server = new VertexVideoServer();

        await CreateModel(server).DoStartAsync(Call(duration: 5), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"durationSeconds\":5}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass n as sampleCount", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassNAsSampleCount()
    {
        var server = new VertexVideoServer();

        await CreateModel(server).DoStartAsync(Call(n: 2), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":2}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should return warnings array", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnWarningsArray()
    {
        var result = await CreateModel(new VertexVideoServer())
            .DoStartAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw when no operation name returned",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowWhenNoOperationNameReturned()
    {
        var server = new VertexVideoServer { Respond = _ => "{\"done\":false}" };

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            CreateModel(server).DoStartAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("No operation name returned from API", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include warnings and response metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeWarningsAndResponseMetadata()
    {
        var testDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = CreateModel(new VertexVideoServer(), () => testDate);

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Warnings);
        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("veo-2.0-generate-001", result.Response.ModelId);
    }
}
