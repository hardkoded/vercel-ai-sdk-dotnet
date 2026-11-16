// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.XaiVideoModel.XaiVideoModelFixture;

namespace Vercel.AI.Tests.XaiVideoModel;

/// <summary>Port of <c>xai-video-model.test.ts</c> &gt; <c>XaiVideoModel</c> &gt; <c>upload delivery</c>.</summary>
public sealed class UploadDeliveryTests
{
    private const string Id =
        "packages/xai/src/xai-video-model.test.ts::XaiVideoModel > upload delivery::";
    private const string Commit = "2e4414481225ed70af7c39326a5f0ef5052a90ca";

    [Fact]
    [UpstreamTest(
        Id + "sends output.upload_url",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task SendsOutputUploadUrl()
    {
        var server = new XaiVideoServer();

        var result = await CreateModel(server)
            .DoStartAsync(
                Call(xai: "{\"output\":{\"uploadUrl\":\"https://example.com/dest.mp4\"}}"),
                CancellationToken.None
            );

        AssertMatches(
            "{\"output\":{\"upload_url\":\"https://example.com/dest.mp4\"}}",
            RequestBody(server)
        );
        AssertMatches("{\"hasUploadUrl\":true}", Operation(result));
    }

    [Fact]
    [UpstreamTest(
        Id + "completes without a video URL when upload was requested",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task CompletesWithoutAVideoUrlWhenUploadWasRequested()
    {
        var server = new XaiVideoServer();
        server.Json(Req123Url, "{\"status\":\"done\",\"video\":null}");

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("uploaded", video.Type);
        Assert.Equal("video/mp4", video.MediaType);
    }

    [Fact]
    [UpstreamTest(
        Id + "does not expose or download a video URL returned in upload mode",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task DoesNotExposeOrDownloadAVideoUrlReturnedInUploadMode()
    {
        var server = new XaiVideoServer();

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("uploaded", video.Type);
        Assert.Equal("video/mp4", video.MediaType);
        AssertNoProperty("videoUrl", Metadata(result)!["xai"]);
        Assert.Single(server.Calls);
    }

    [Theory]
    [InlineData("generation", "{}", "generations")]
    [InlineData(
        "editing",
        "{\"mode\":\"edit-video\",\"videoUrl\":\"https://example.com/source.mp4\"}",
        "edits"
    )]
    [InlineData("legacy editing", "{\"videoUrl\":\"https://example.com/source.mp4\"}", "edits")]
    [InlineData(
        "extension",
        "{\"mode\":\"extend-video\",\"videoUrl\":\"https://example.com/source.mp4\"}",
        "extensions"
    )]
    [UpstreamTest(
        Id + "forwards upload delivery for %s without persisting the signed URL",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ForwardsUploadDeliveryWithoutPersistingTheSignedUrl(
        string name,
        string modeOptions,
        string endpoint
    )
    {
        Assert.NotEmpty(name);
        var server = new XaiVideoServer();
        const string uploadUrl = "https://storage.example.com/video.mp4?signature=test-secret";
        var options = JsonNodeObject(modeOptions);
        options["output"] = new System.Text.Json.Nodes.JsonObject { ["uploadUrl"] = uploadUrl };

        var result = await CreateModel(server)
            .DoStartAsync(Call(xai: options.ToJsonString()), CancellationToken.None);

        Assert.Equal(TestBaseUrl + "/videos/" + endpoint, server.Calls[0].Url);
        AssertMatches("{\"output\":{\"upload_url\":\"" + uploadUrl + "\"}}", RequestBody(server));
        AssertJson("{\"requestId\":\"req-123\",\"hasUploadUrl\":true}", Operation(result));
        Assert.DoesNotContain(
            "test-secret",
            JsonSerializer.Serialize(
                new
                {
                    result.Operation,
                    result.Warnings,
                    Metadata = result.ProviderMetadata?.GetRawText(),
                    result.Response.Timestamp,
                    result.Response.ModelId,
                }
            )
        );
    }

    [Theory]
    [InlineData(
        "moderation",
        "{\"status\":\"done\",\"video\":{\"respect_moderation\":false}}",
        "content policy violation"
    )]
    [InlineData(
        "failure",
        "{\"status\":\"failed\",\"error\":{\"message\":\"upload failed\"}}",
        "Video generation failed"
    )]
    [InlineData("expiry", "{\"status\":\"expired\"}", "expired")]
    [UpstreamTest(
        Id + "preserves %s failures in upload mode",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task PreservesFailuresInUploadMode(string name, string body, string error)
    {
        Assert.NotEmpty(name);
        var server = new XaiVideoServer();
        server.Json(Req123Url, body);

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Contains(error, result.Error);
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps a pending upload pending",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task KeepsAPendingUploadPending()
    {
        var server = new XaiVideoServer();
        server.Json(Req123Url, "{\"status\":\"pending\"}");

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None);

        Assert.Equal("pending", result.Status);
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps Files storage options independent of upload delivery",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task KeepsFilesStorageOptionsIndependentOfUploadDelivery()
    {
        var server = new XaiVideoServer();
        await CreateModel(server)
            .DoStartAsync(
                Call(
                    xai: "{\"output\":{\"uploadUrl\":\"https://storage.example.com/dest.mp4?signature=secret\"},\"storageOptions\":{\"filename\":\"copy.mp4\",\"publicUrl\":true}}"
                ),
                CancellationToken.None
            );

        var body = RequestBody(server);
        Assert.False(string.IsNullOrEmpty(body["output"]!["upload_url"]!.GetValue<string>()));
        AssertMatches(
            "{\"storage_options\":{\"filename\":\"copy.mp4\",\"public_url\":true}}",
            body
        );

        server.Json(
            Req123Url,
            "{\"status\":\"done\",\"video\":{\"respect_moderation\":true,\"storage_error\":\"Files persistence failed\",\"file_output\":{\"file_id\":\"file-1\",\"filename\":\"copy.mp4\",\"public_url\":\"https://files.x.ai/copy.mp4\"}}}"
        );
        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None);

        Assert.Equal("completed", result.Status);
        var video = Assert.Single(result.Videos);
        Assert.Equal("uploaded", video.Type);
        Assert.Equal("video/mp4", video.MediaType);
        AssertMatches(
            "{\"xai\":{\"storageError\":\"Files persistence failed\",\"fileOutput\":{\"fileId\":\"file-1\",\"publicUrl\":\"https://files.x.ai/copy.mp4\"}}}",
            Metadata(result)
        );
    }

    [Theory]
    [InlineData("{\"uploadUrl\":\"\"}")]
    [InlineData("{\"uploadUrl\":\"not-a-url\"}")]
    [InlineData("{\"uploadUrl\":\"http://example.com/video.mp4?signature=secret\"}")]
    [InlineData("{\"uploadUrl\":\"https://user:password@example.com/video.mp4\"}")]
    [InlineData("{\"uploadUrl\":\"https://example.com/video.mp4#fragment\"}")]
    [InlineData("{\"upload_url\":\"https://example.com/video.mp4\"}")]
    [InlineData("{\"uploadUrl\":42}")]
    [InlineData("{}")]
    [InlineData("null")]
    [UpstreamTest(
        Id + "rejects invalid upload configuration before HTTP: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task RejectsInvalidUploadConfigurationBeforeHttp(string output)
    {
        var server = new XaiVideoServer();

        var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateModel(server)
                .DoStartAsync(Call(xai: "{\"output\":" + output + "}"), CancellationToken.None)
        );

        Assert.Contains(
            "Invalid xAI video provider options for upload delivery.",
            exception.Message
        );
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Id + "omits signed URLs from validation error causes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task OmitsSignedUrlsFromValidationErrorCauses()
    {
        var server = new XaiVideoServer();

        var caught = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateModel(server)
                .DoStartAsync(
                    Call(
                        xai: "{\"output\":{\"uploadUrl\":\"https://storage.example.com/video.mp4?signature=secret\"},\"resolution\":\"invalid\"}"
                    ),
                    CancellationToken.None
                )
        );

        Assert.Null(caught.InnerException);
        Assert.Null(caught.Value);
        Assert.DoesNotContain("signature=secret", caught.ToString());
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Id + "redacts signed URLs from start API errors while preserving retryability",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task RedactsSignedUrlsFromStartApiErrorsWhilePreservingRetryability()
    {
        var server = new XaiVideoServer();
        const string uploadUrl = "https://storage.example.com/video.mp4?signature=secret";
        server.Respond(
            TestBaseUrl + "/videos/generations",
            429,
            "{\"error\":{\"message\":\"Failed to upload to " + uploadUrl + "\"}}"
        );

        var caught = await Assert.ThrowsAsync<ApiException>(() =>
            CreateModel(server)
                .DoStartAsync(
                    Call(xai: "{\"output\":{\"uploadUrl\":\"" + uploadUrl + "\"}}"),
                    CancellationToken.None
                )
        );

        Assert.Equal(429, caught.StatusCode);
        Assert.True(caught.IsRetryable);
        AssertMatches(
            "{\"output\":{\"upload_url\":\"[REDACTED]\"}}",
            System.Text.Json.Nodes.JsonNode.Parse(
                ((JsonElement)caught.RequestBodyValues!).GetRawText()
            )
        );
        Assert.Null(caught.ResponseBody);
        Assert.Null(caught.InnerException);
        Assert.Null(caught.ErrorData);
        Assert.DoesNotContain(
            "signature=secret",
            JsonSerializer.Serialize(
                new
                {
                    caught.Message,
                    caught.Url,
                    caught.RequestBodyValues,
                    caught.ResponseBody,
                }
            )
        );
        Assert.DoesNotContain("signature=secret", caught.ToString());
    }

    [Fact]
    [UpstreamTest(
        Id + "omits provider error bodies from upload status request failures",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task OmitsProviderErrorBodiesFromUploadStatusRequestFailures()
    {
        var server = new XaiVideoServer();
        server.Respond(
            Req123Url,
            500,
            "{\"error\":{\"message\":\"https://storage.example.com/video.mp4?signature=secret\"}}"
        );

        var caught = await Assert.ThrowsAsync<ApiException>(() =>
            CreateModel(server)
                .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None)
        );

        Assert.Equal("xAI video upload status request failed.", caught.Message);
        Assert.Equal(500, caught.StatusCode);
        Assert.True(caught.IsRetryable);
        Assert.Null(caught.RequestBodyValues);
        Assert.Null(caught.ResponseBody);
        Assert.Null(caught.ErrorData);
        Assert.Null(caught.InnerException);
    }

    [Fact]
    [UpstreamTest(
        Id + "does not echo signed destinations from terminal upload failures",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task DoesNotEchoSignedDestinationsFromTerminalUploadFailures()
    {
        var server = new XaiVideoServer();
        server.Json(
            Req123Url,
            "{\"status\":\"failed\",\"error\":{\"message\":\"https://storage.example.com/video.mp4?signature=secret\"}}"
        );

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123(hasUploadUrl: true)), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation failed.", result.Error);
    }

    [Fact]
    [UpstreamTest(
        Id + "preserves the missing URL error otherwise",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task PreservesTheMissingUrlErrorOtherwise()
    {
        var server = new XaiVideoServer();
        server.Json(Req123Url, "{\"status\":\"done\",\"video\":null}");

        var result = await CreateModel(server)
            .DoStatusAsync(StatusCall(Req123()), CancellationToken.None);

        Assert.Equal("error", result.Status);
        Assert.Equal("Video generation completed but no video URL was returned.", result.Error);
    }

    private static System.Text.Json.Nodes.JsonObject JsonNodeObject(string json)
    {
        return System.Text.Json.Nodes.JsonNode.Parse(json)!.AsObject();
    }
}
