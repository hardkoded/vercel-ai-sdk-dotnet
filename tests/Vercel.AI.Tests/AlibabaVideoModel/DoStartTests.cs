// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStart::";

    [Theory]
    [InlineData("wan2.7-r2v")]
    [InlineData("wan2.8-r2v")]
    [InlineData("wan2.10-r2v")]
    [UpstreamTest(
        Id + "should use the tiered reference protocol for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseTheTieredReferenceProtocolFor(string modelId)
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, modelId),
            Call(
                resolution: "1280x720",
                aspectRatio: "16:9",
                inputReferences: [Url("https://example.com/ref.jpg")]
            )
        );

        var body = RequestBody(server);
        AssertJson(
            "[{\"type\":\"reference_image\",\"url\":\"https://example.com/ref.jpg\"}]",
            body["input"]!["media"]
        );
        AssertNoProperty("reference_urls", body["input"]);
        AssertMatches("{\"resolution\":\"720P\",\"ratio\":\"16:9\"}", body["parameters"]);
        AssertNoProperty("size", body["parameters"]);
    }

    [Theory]
    [InlineData("wan2.6-t2v")]
    [InlineData("wan2.2-t2v-plus")]
    [InlineData("wanx2.1-t2v-turbo")]
    [UpstreamTest(
        Id + "should use the legacy size protocol for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseTheLegacySizeProtocolFor(string modelId)
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, modelId), Call(resolution: "1280x720"));

        var body = RequestBody(server);
        Assert.Equal("1280*720", body["parameters"]!["size"]!.GetValue<string>());
        AssertNoProperty("resolution", body["parameters"]);
    }

    [Theory]
    [InlineData("wan4.0-video")]
    [InlineData("wan10.0-video")]
    [InlineData("wan-next-video")]
    [UpstreamTest(
        Id + "should use the latest all-in-one protocol for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseTheLatestAllInOneProtocolFor(string modelId)
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, modelId),
            Call(
                resolution: "832x480",
                aspectRatio: "16:9",
                generateAudio: false,
                image: Url("https://example.com/first.jpg"),
                inputReferences: [Url("https://example.com/ref.jpg")]
            )
        );

        var body = RequestBody(server);
        AssertJson(
            "[{\"type\":\"reference_image\",\"url\":\"https://example.com/ref.jpg\"},{\"type\":\"first_frame\",\"url\":\"https://example.com/first.jpg\"}]",
            body["input"]!["media"]
        );
        AssertNoProperty("img_url", body["input"]);
        AssertNoProperty("reference_urls", body["input"]);
        AssertMatches(
            "{\"resolution\":\"480P\",\"ratio\":\"16:9\",\"audio\":false}",
            body["parameters"]
        );
        AssertNoProperty("size", body["parameters"]);
    }

    [Fact]
    [UpstreamTest(Id + "should return operation with taskId", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnOperationWithTaskId()
    {
        var result = await Start(CreateModel(new AlibabaVideoServer()), Call());

        AssertJson("{\"taskId\":\"task-abc-123\"}", Operation(result));
        Assert.Empty(result.Warnings);
        Assert.Equal("wan2.6-t2v", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Id + "should send X-DashScope-Async header", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendXDashScopeAsyncHeader()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server), Call());

        Assert.Equal("enable", server.Calls[0].Headers["x-dashscope-async"]);
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassHeaders()
    {
        var server = new AlibabaVideoServer();
        var model = CreateModel(
            server,
            headers: new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer custom-key",
                ["X-Custom"] = "value",
            }
        );

        await Start(
            model,
            Call(headers: new Dictionary<string, string> { ["X-Request-Header"] = "request-value" })
        );

        var headers = server.Calls[0].Headers;
        Assert.Equal("Bearer custom-key", headers["authorization"]);
        Assert.Equal("value", headers["x-custom"]);
        Assert.Equal("request-value", headers["x-request-header"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw when no task_id returned",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowWhenNoTaskIdReturned()
    {
        var server = new AlibabaVideoServer();
        server.Json(CreateUrl, "{\"output\":null,\"request_id\":\"req-003\"}");

        var model = CreateModel(server);

        var exception = await Assert.ThrowsAnyAsync<AiSdkException>(() => Start(model, Call()));
        Assert.Contains("No task_id", exception.Message);
    }
}
