// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; VOD</c>.</summary>
public sealed class VodTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > VOD::";

    [Fact]
    [UpstreamTest(Prefix + "submits a signed CreateAigcVideoTask request", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Submits_a_signed_CreateAigcVideoTask_request()
    {
        var server = new Server();
        var result = await CreateModel(server, "vod/VS/2.0").DoStartAsync(
            Start(
                aspectRatio: "16:9",
                duration: 8,
                resolution: "1280x720",
                seed: 7,
                generateAudio: true,
                frameImages: Frames((Url("https://example.com/first.png"), "first_frame")),
                inputReferences: [VideoModelFile.FromFile([1, 2, 3], "video/mp4"), Url("https://example.com/voice.mp3", "audio/mpeg")],
                providerOptions: """{"tencent":{"negativePrompt":"dogs","enhancePrompt":false,"inputRegion":"Oversea","watermark":false,"referenceVideoType":"feature","sessionId":"session-1"}}"""),
            CancellationToken.None);

        var call = server.Calls[0];
        Assert.Equal(VodUrl + "/", call.Uri.AbsoluteUri);
        Assert.Equal("CreateAigcVideoTask", call.Header("x-tc-action"));
        Assert.Equal("2018-07-17", call.Header("x-tc-version"));
        Assert.Equal("1794096000", call.Header("x-tc-timestamp"));
        Assert.Equal("vod", call.Header("x-vod-header"));
        Assert.Matches(new Regex(@"^TC3-HMAC-SHA256 Credential=AKIDEXAMPLE/2026-11-08/vod/tc3_request, SignedHeaders=content-type;host;x-tc-action, Signature=[0-9a-f]{64}\z"), call.Header("authorization")!);
        Body(server, """{"SubAppId":1500000000,"ModelName":"VS","ModelVersion":"2.0","Prompt":"A cat walking on a beach","NegativePrompt":"dogs","EnhancePrompt":"Disabled","FileInfos":[{"Type":"Url","Url":"https://example.com/first.png","Category":"Image","Usage":"FirstFrame"},{"Type":"Base64","Base64":"AQID","Category":"Video","Usage":"Reference","ReferenceType":"feature"},{"Type":"Url","Url":"https://example.com/voice.mp3","Category":"Audio","Usage":"Reference"}],"OutputConfig":{"StorageMode":"Temporary","Resolution":"720P","AspectRatio":"16:9","Duration":8,"AudioGeneration":"Enabled","LogoAdd":"Disabled"},"InputRegion":"Oversea","Seed":7,"SessionId":"session-1"}""");
        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, """{"taskId":"task-123"}""");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about TokenHub-only options", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Warns_about_TokenHub_only_options()
    {
        var server = new Server();
        var result = await CreateModel(server, "vod/Kling/3.0").DoStartAsync(Start(providerOptions: """{"tencent":{"multiShot":true,"characterOrientation":"image"}}"""), CancellationToken.None);

        Assert.Equal(["providerOptions.tencent.multiShot", "providerOptions.tencent.characterOrientation"], Features(result.Warnings));
    }

    [Fact]
    [UpstreamTest(Prefix + "sends assets as URL file inputs", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_assets_as_URL_file_inputs()
    {
        var server = new Server();
        await CreateModel(server, "vod/VS/2.0").DoStartAsync(
            Start(providerOptions: """{"tencent":{"assets":[{"url":"asset://asset-1"},{"url":"asset://asset-2","usage":"FirstFrame"},{"url":"asset://asset-3","category":"Video"}]}}"""),
            CancellationToken.None);

        JsonAssert.Equal(
            server.RequestBody()["FileInfos"],
            """[{"Type":"Url","Url":"asset://asset-1","Category":"Image","Usage":"Reference"},{"Type":"Url","Url":"asset://asset-2","Category":"Image","Usage":"FirstFrame"},{"Type":"Url","Url":"asset://asset-3","Category":"Video","Usage":"Reference"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores asset frames when that frame is already set", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Ignores_asset_frames_when_that_frame_is_already_set()
    {
        var server = new Server();
        var result = await CreateModel(server, "vod/VS/2.0").DoStartAsync(
            Start(image: Url("https://example.com/first.png"), providerOptions: """{"tencent":{"assets":[{"url":"asset://asset-1","usage":"FirstFrame"},{"url":"asset://asset-2","usage":"LastFrame"},{"url":"asset://asset-3","usage":"LastFrame"}]}}"""),
            CancellationToken.None);

        var files = server.RequestBody()["FileInfos"]!.AsArray().Select(file => ((string?)file!["Usage"], (string?)file["Url"])).ToArray();
        Assert.Equal([("FirstFrame", "https://example.com/first.png"), ("LastFrame", "asset://asset-2")], files);
        Assert.All(result.Warnings, warning => Assert.Equal("providerOptions.tencent.assets", warning.Feature));
        Assert.Equal(
            ["VOD accepts one first frame; the asset asset://asset-1 was ignored.", "VOD accepts one last frame; the asset asset://asset-3 was ignored."],
            result.Warnings.Select(warning => warning.Details ?? string.Empty).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "throws an APICallError for Response.Error", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_an_APICallError_for_Response_Error()
    {
        var server = new Server();
        server.Respond(VodUrl, """{"Response":{"Error":{"Code":"InvalidParameterValue.SubAppId","Message":"Invalid SubAppId."},"RequestId":"req-vod-err"}}""");

        var error = await Assert.ThrowsAsync<BadRequestException>(() => CreateModel(server, "vod/VS/2.0").DoStartAsync(Start(), CancellationToken.None));

        Assert.Equal(400, error.StatusCode);
        Assert.Equal("InvalidParameterValue.SubAppId: Invalid SubAppId.", error.Message);
    }
}
