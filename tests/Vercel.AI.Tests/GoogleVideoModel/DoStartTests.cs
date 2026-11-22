// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleVideoModel.GoogleVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVideoModel;

/// <summary>Port of <c>google-video-model.test.ts</c> &gt; <c>GoogleVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Id =
        "packages/google/src/google-video-model.test.ts::GoogleVideoModel > doStart::";

    private static readonly string PromptJson = "\"" + PromptText + "\"";

    [Fact]
    [UpstreamTest(
        Id + "should return operation with operationName",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnOperationWithOperationName()
    {
        var (model, _) = CreateMockModel(operationName: "operations/start-test-op");

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"operationName\":\"operations/start-test-op\"}",
            System.Text.Json.Nodes.JsonNode.Parse(
                ((System.Text.Json.JsonElement)result.Operation!).GetRawText()
            )
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassCorrectRequestBody()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":" + PromptJson + "}],\"parameters\":{\"sampleCount\":1}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass seed when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassSeedWhenProvided()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(Call(seed: 42), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"seed\":42}}",
            CapturedBody(fetch)
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
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(Call(seed: 0), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"seed\":0}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass aspect ratio when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassAspectRatioWhenProvided()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(Call(aspectRatio: "16:9"), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"aspectRatio\":\"16:9\"}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert resolution to Google format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldConvertResolutionToGoogleFormat()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(Call(resolution: "1920x1080"), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"resolution\":\"1080p\"}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass duration as durationSeconds",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassDurationAsDurationSeconds()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(Call(duration: 5), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"durationSeconds\":5}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass n as sampleCount", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassNAsSampleCount()
    {
        var (model, fetch) = CreateMockModel(
            videos: ["https://example.com/video1.mp4", "https://example.com/video2.mp4"]
        );

        await model.DoStartAsync(Call(n: 2), CancellationToken.None);

        AssertJson(
            "{\"instances\":[{\"prompt\":" + PromptJson + "}],\"parameters\":{\"sampleCount\":2}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should return warnings array", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnWarningsArray()
    {
        var (model, _) = CreateMockModel();

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include timestamp and modelId in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeTimestampAndModelIdInResponse()
    {
        var testDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (model, _) = CreateMockModel(currentDate: () => testDate);

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("veo-3.1-generate-preview", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassHeaders()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(
            Call(headers: new Dictionary<string, string> { ["x-custom-header"] = "custom-value" }),
            CancellationToken.None
        );

        var headers = fetch
            .Requests.First(request => request.Url.Contains(":predictLongRunning"))
            .Headers;
        Assert.Equal("custom-value", headers["x-custom-header"]);
        Assert.Equal("test-api-key", headers["x-goog-api-key"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw when no operation name returned",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowWhenNoOperationNameReturned()
    {
        var (model, _) = CreateFixedModel("{\"done\":false}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoStartAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("No operation name returned from API", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should send image as inlineData", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendImageAsInlineData()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(
            Call(image: VideoModelFile.FromBase64("base64-image-data", "image/png")),
            CancellationToken.None
        );

        AssertJson(
            "{\"bytesBase64Encoded\":\"base64-image-data\",\"mimeType\":\"image/png\"}",
            CapturedBody(fetch)!["instances"]![0]!["image"]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn when URL-based image is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldWarnWhenUrlBasedImageIsProvided()
    {
        var (model, _) = CreateMockModel();

        var result = await model.DoStartAsync(
            Call(image: VideoModelFile.FromUrl("https://example.com/image.png")),
            CancellationToken.None
        );

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("URL-based image input", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Id + "should pass personGeneration option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassPersonGenerationOption()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(
            Call(google: "{\"personGeneration\":\"allow_adult\"}"),
            CancellationToken.None
        );

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"personGeneration\":\"allow_adult\"}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass negativePrompt option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassNegativePromptOption()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(
            Call(google: "{\"negativePrompt\":\"blurry, low quality\"}"),
            CancellationToken.None
        );

        AssertJson(
            "{\"instances\":[{\"prompt\":"
                + PromptJson
                + "}],\"parameters\":{\"sampleCount\":1,\"negativePrompt\":\"blurry, low quality\"}}",
            CapturedBody(fetch)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass referenceImages option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassReferenceImagesOption()
    {
        var (model, fetch) = CreateMockModel();

        await model.DoStartAsync(
            Call(
                google: "{\"referenceImages\":[{\"bytesBase64Encoded\":\"reference-image-data\"},{\"gcsUri\":\"gs://bucket/reference.png\"}]}"
            ),
            CancellationToken.None
        );

        AssertJson(
            "[{\"image\":{\"bytesBase64Encoded\":\"reference-image-data\",\"mimeType\":\"image/png\"},\"referenceType\":\"asset\"},{\"image\":{\"gcsUri\":\"gs://bucket/reference.png\",\"mimeType\":\"image/png\"},\"referenceType\":\"asset\"}]",
            CapturedBody(fetch)!["instances"]![0]!["referenceImages"]
        );
    }
}
