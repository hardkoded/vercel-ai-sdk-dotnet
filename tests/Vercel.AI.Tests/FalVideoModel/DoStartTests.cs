// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.FalVideoModel.FalVideoModelFixture;

namespace Vercel.AI.Tests.FalVideoModel;

/// <summary>Port of <c>fal-video-model.test.ts</c> &gt; <c>FalVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Id = "packages/fal/src/fal-video-model.test.ts::FalVideoModel > doStart::";

    [Fact]
    [UpstreamTest(
        Id + "should submit to queue and return operation with responseUrl",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSubmitToQueueAndReturnOperationWithResponseUrl()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"responseUrl\":\"https://queue.fal.run/fal-ai/luma-dream-machine/requests/test-request-id-123\",\"submitUrl\":\"https://queue.fal.run/fal-ai/luma-dream-machine\"}",
            Operation(result)
        );
        Assert.Empty(result.Warnings);
        Assert.Equal("luma-dream-machine", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Id + "should pass the correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassTheCorrectRequestBody()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(seed: 42, aspectRatio: "16:9"), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"seed\":42,\"aspect_ratio\":\"16:9\"}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass the correct parameters including prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassTheCorrectParametersIncludingPrompt()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(), CancellationToken.None);

        AssertJson("{\"prompt\":\"" + PromptText + "\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should pass seed when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassSeedWhenProvided()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(seed: 42), CancellationToken.None);

        AssertJson("{\"prompt\":\"" + PromptText + "\",\"seed\":42}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass seed when it is 0",
        Coverage = UpstreamCoverage.Covered,
        Commit = "ef2dfb5e9fb7b72287b226eba8b743f809414172"
    )]
    public async Task ShouldPassSeedWhenItIs0()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(seed: 0), CancellationToken.None);

        AssertJson("{\"prompt\":\"" + PromptText + "\",\"seed\":0}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass aspect ratio when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassAspectRatioWhenProvided()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(aspectRatio: "16:9"), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"aspect_ratio\":\"16:9\"}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert duration to string format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldConvertDurationToStringFormat()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(duration: 5), CancellationToken.None);

        AssertJson("{\"prompt\":\"" + PromptText + "\",\"duration\":\"5s\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers",
        Coverage = UpstreamCoverage.Covered,
        Note = "Compares the headers without the provider Authorization and User-Agent headers, which the upstream test model does not have."
    )]
    public async Task ShouldPassHeaders()
    {
        var server = new FalVideoServer();
        var modelWithHeaders = CreateBasicModel(
            server,
            headers: new Dictionary<string, string>
            {
                ["Custom-Provider-Header"] = "provider-header-value",
            }
        );

        await modelWithHeaders.DoStartAsync(
            Call(
                headers: new Dictionary<string, string>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                }
            ),
            CancellationToken.None
        );

        // The provider also sends its Authorization and User-Agent headers, which the upstream test model does not have.
        var headers = server
            .Calls[0]
            .Headers.Where(header =>
                header.Key is not ("authorization" or "user-agent" or "content-length")
            )
            .OrderBy(header => header.Key, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(
            [
                new KeyValuePair<string, string>("content-type", "application/json"),
                new KeyValuePair<string, string>("custom-provider-header", "provider-header-value"),
                new KeyValuePair<string, string>("custom-request-header", "request-header-value"),
            ],
            headers
        );
    }

    [Fact]
    [UpstreamTest(Id + "should return warnings array", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnWarningsArray()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw error when no response URL returned",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowErrorWhenNoResponseURLReturned()
    {
        var server = new FalVideoServer();
        server.Json("https://queue.fal.run/fal-ai/luma-dream-machine", "{}");
        var model = CreateBasicModel(server);

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoStartAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("No response URL returned from queue endpoint", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle API errors from queue endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldHandleAPIErrorsFromQueueEndpoint()
    {
        var server = new FalVideoServer();
        server.Respond(
            "https://queue.fal.run/fal-ai/luma-dream-machine",
            400,
            "{\"error\":{\"message\":\"Invalid prompt\"}}"
        );
        var model = CreateBasicModel(server);

        var exception = await Assert.ThrowsAnyAsync<ApiException>(() =>
            model.DoStartAsync(Call(), CancellationToken.None)
        );

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Id + "should append webhookUrl as fal_webhook query parameter",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldAppendWebhookUrlAsFalWebhookQueryParameter()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(
            Call(webhookUrl: "https://smee.io/abc123"),
            CancellationToken.None
        );

        Assert.Equal(
            "https://queue.fal.run/fal-ai/luma-dream-machine?fal_webhook=https%3A%2F%2Fsmee.io%2Fabc123",
            server.Calls[0].Url
        );
    }

    [Fact]
    [UpstreamTest(Id + "should send image_url with file data", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendImageUrlWithFileData()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);
        var imageData = new byte[] { 137, 80, 78, 71 }; // PNG magic bytes

        await model.DoStartAsync(
            Call(image: VideoModelFile.FromFile(imageData, "image/png")),
            CancellationToken.None
        );

        AssertMatches(
            "{\"prompt\":\"" + PromptText + "\",\"image_url\":\"data:image/png;base64,iVBORw==\"}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send image_url with URL-based image",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendImageUrlWithURLBasedImage()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(
            Call(image: VideoModelFile.FromUrl("https://example.com/input-image.png")),
            CancellationToken.None
        );

        AssertMatches(
            "{\"prompt\":\""
                + PromptText
                + "\",\"image_url\":\"https://example.com/input-image.png\"}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass loop option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassLoopOption()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(fal: "{\"loop\":true}"), CancellationToken.None);

        AssertJson("{\"prompt\":\"" + PromptText + "\",\"loop\":true}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass motionStrength as motion_strength",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassMotionStrengthAsMotionStrength()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(fal: "{\"motionStrength\":0.8}"), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"motion_strength\":0.8}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass resolution option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassResolutionOption()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server, modelId: "luma-ray-2");

        await model.DoStartAsync(Call(fal: "{\"resolution\":\"1080p\"}"), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"resolution\":\"1080p\"}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass negativePrompt as negative_prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassNegativePromptAsNegativePrompt()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(
            Call(fal: "{\"negativePrompt\":\"blurry, low quality\"}"),
            CancellationToken.None
        );

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"negative_prompt\":\"blurry, low quality\"}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass promptOptimizer as prompt_optimizer",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassPromptOptimizerAsPromptOptimizer()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(Call(fal: "{\"promptOptimizer\":true}"), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"prompt_optimizer\":true}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through additional options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassThroughAdditionalOptions()
    {
        var server = new FalVideoServer();
        var model = CreateBasicModel(server);

        await model.DoStartAsync(
            Call(fal: "{\"custom_param\":\"custom_value\",\"another_param\":123}"),
            CancellationToken.None
        );

        AssertJson(
            "{\"prompt\":\""
                + PromptText
                + "\",\"custom_param\":\"custom_value\",\"another_param\":123}",
            RequestBody(server)
        );
    }
}
