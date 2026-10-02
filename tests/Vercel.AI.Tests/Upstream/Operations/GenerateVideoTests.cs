// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for <c>generateVideo</c>, <c>startVideo</c>, and <c>getVideoStatus</c>.</summary>
public sealed class GenerateVideoTests
{
    private const string Video = "packages/ai/src/generate-video/generate-video.test.ts::experimental_generateVideo";
    private const string Start = "packages/ai/src/generate-video/start-video.test.ts::experimental_startVideo::";
    private const string Status = "packages/ai/src/generate-video/start-video.test.ts::experimental_getVideoStatus::";
    private const string Mp4 = "AAAAIGZ0eXBpc29tAAACAGlzb20iaXNvMgAAAAhmcmVl";

    [Fact]
    [UpstreamTest(Video + "::should send args to doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_do_generate()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            N = 1,
            AspectRatio = "16:9",
            Resolution = "720p",
            Duration = 4,
            Fps = 24,
            Seed = 7,
            GenerateAudio = true,
            ProviderOptions = OperationJson.Parse("{\"p\":{\"style\":\"cinematic\"}}"),
            Headers = new Dictionary<string, string> { ["X-Test"] = "1" },
        });
        var call = model.Generates[0];
        Assert.Equal("a cat", call.Prompt);
        Assert.Equal(1, call.N);
        Assert.Equal("16:9", call.AspectRatio);
        Assert.Equal("720p", call.Resolution);
        Assert.Equal((double?)4, call.Duration);
        Assert.Equal((int?)24, call.Fps);
        Assert.Equal((int?)7, call.Seed);
        Assert.True(call.GenerateAudio);
        Assert.Equal("cinematic", call.ProviderOptions.GetProperty("p").GetProperty("style").GetString());
        Assert.Equal("1", call.Headers["x-test"]);
        Assert.Equal("ai/0.0.0-test", call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Video + "::should return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_warnings()
    {
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Result = Clip(OperationWarning.Other("note")) }, Prompt = new VideoPrompt("a cat") });
        Assert.Equal("note", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Video + "::should call logWarnings with the correct warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_video_warnings()
    {
        var seen = Watch();
        try
        {
            await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Provider = "video", ModelId = "clip", Result = Clip(OperationWarning.Other("note")) }, Prompt = new VideoPrompt("a cat") });
            Assert.Equal("note", seen[0].Warnings[0].Message);
            Assert.Equal("video", seen[0].Provider);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Video + "::should not call logWarnings when no warnings are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_log_an_empty_warning_list()
    {
        var seen = Watch();
        try
        {
            await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Result = Clip() }, Prompt = new VideoPrompt("a cat") });
            Assert.Empty(seen);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Video + " > base64 video data::should return generated videos with correct mime types", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_base64_videos()
    {
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }) }, Prompt = new VideoPrompt("a cat") });
        Assert.Equal("video/mp4", result.Video.MediaType);
        Assert.Equal(Mp4, result.Video.Base64);
    }

    [Fact]
    [UpstreamTest(Video + " > base64 video data::should return the first video", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_first_video()
    {
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4"), new VideoOutput("base64", data: Mp4, mediaType: "video/webm") }) },
            Prompt = new VideoPrompt("a cat"),
        });
        Assert.Equal("video/mp4", result.Video.MediaType);
        Assert.Equal(2, result.Videos.Count);
    }

    [Fact]
    [UpstreamTest(Video + " > binary video data::should return generated videos", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_binary_videos()
    {
        var bytes = Convert.FromBase64String(Mp4);
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("binary", data: bytes) }) }, Prompt = new VideoPrompt("a cat") });
        Assert.Equal(bytes, result.Video.Data);
        Assert.Equal("video/mp4", result.Video.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > URL video data::should fetch and return videos from URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Downloads_url_videos()
    {
        var bytes = Convert.FromBase64String(Mp4);
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("url", "https://example.com/cat.mp4", mediaType: "video/mp4") }) },
            Prompt = new VideoPrompt("a cat"),
            Download = (url, _) => Task.FromResult(new DownloadedMedia(bytes, "video/mp4")),
        });
        Assert.Equal(bytes, result.Video.Data);
        Assert.Equal("video/mp4", result.Video.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > URL video data::should throw DownloadError when fetch fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_a_video_download_fails()
    {
        var error = await Assert.ThrowsAsync<DownloadException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("url", "https://example.com/missing.mp4") }) },
            Prompt = new VideoPrompt("a cat"),
            Download = (url, _) => throw new DownloadException(url, 404, "missing"),
        }));
        Assert.Contains("https://example.com/missing.mp4", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Video + " > URL video data::should detect mediaType via signature when provider and download return application/octet-stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_a_video_signature_for_octet_stream()
    {
        var bytes = Convert.FromBase64String(Mp4);
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("url", "https://example.com/cat.mp4", mediaType: "application/octet-stream") }) },
            Prompt = new VideoPrompt("a cat"),
            Download = (_, _) => Task.FromResult(new DownloadedMedia(bytes, "application/octet-stream")),
        });
        Assert.Equal("video/mp4", result.Video.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > when several calls are required::should generate videos", Coverage = UpstreamCoverage.Covered)]
    public async Task Splits_video_generation_across_calls()
    {
        var model = new VideoFake { MaxVideosPerCall = 1, Result = Clip() };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 3 });
        Assert.Equal(new[] { 1, 1, 1 }, model.Generates.Select(item => item.N));
        Assert.Equal(3, result.Videos.Count);
    }

    [Fact]
    [UpstreamTest(Video + " > when several calls are required::should aggregate warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Aggregates_video_warnings()
    {
        var turn = 0;
        var model = new VideoFake { MaxVideosPerCall = 1, Next = () => Clip(OperationWarning.Other("w" + turn++)) };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 2 });
        Assert.Equal(new[] { "w0", "w1" }, result.Warnings.Select(item => item.Message));
    }

    [Fact]
    [UpstreamTest(Video + " > when several calls are required::should generate with maxVideosPerCall = %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_functional_video_limit()
    {
        foreach (var _ in new[] { "sync method", "async method" })
        {
            var model = new VideoFake { FunctionalMax = 2, Result = Clip() };
            await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 3 });
            Assert.Equal(new[] { 2, 1 }, model.Generates.Select(item => item.N));
        }
    }

    [Fact]
    [UpstreamTest(Video + " > error handling::should throw NoVideoGeneratedError when no videos are returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_no_videos_are_returned()
    {
        var error = await Assert.ThrowsAsync<NoVideoGeneratedException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Result = new VideoModelResult(Array.Empty<VideoOutput>()) }, Prompt = new VideoPrompt("a cat") }));
        Assert.Equal("No video generated.", error.Message);
    }

    [Fact]
    [UpstreamTest(Video + " > error handling::should include response headers in error when no videos generated", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_headers_when_no_video_is_generated()
    {
        var model = new VideoFake { Result = new VideoModelResult(Array.Empty<VideoOutput>(), response: new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "req" })) };
        var error = await Assert.ThrowsAsync<NoVideoGeneratedException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") }));
        Assert.Equal("req", error.Responses[0].Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Video + "::should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_response_metadata()
    {
        var model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }, providerMetadata: OperationJson.Parse("{\"p\":{\"id\":\"job\"}}"), response: new ProviderResponse(id: "response", modelId: "clip")) };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") });
        Assert.Equal("response", result.Responses[0].Id);
        Assert.Equal("job", result.Responses[0].ProviderMetadata!.Value.GetProperty("p").GetProperty("id").GetString());
    }

    [Fact]
    [UpstreamTest(Video + "::should return provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_provider_metadata()
    {
        var model = new VideoFake { Result = new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }, providerMetadata: OperationJson.Parse("{\"p\":{\"videos\":[{\"id\":\"a\"}]}}")) };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") });
        Assert.Equal("a", result.ProviderMetadata.GetProperty("p").GetProperty("videos")[0].GetProperty("id").GetString());
    }

    [Fact]
    [UpstreamTest(Video + " > provider metadata merging::should merge provider metadata from multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_video_provider_metadata()
    {
        var turn = 0;
        var model = new VideoFake { MaxVideosPerCall = 1, Next = () => new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }, providerMetadata: OperationJson.Parse("{\"p\":{\"videos\":[{\"id\":" + turn++ + "}]}}")) };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 2 });
        Assert.Equal(2, result.ProviderMetadata.GetProperty("p").GetProperty("videos").GetArrayLength());
    }

    [Fact]
    [UpstreamTest(Video + " > provider metadata merging::should handle gateway provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_gateway_video_metadata()
    {
        var turn = 0;
        var model = new VideoFake { MaxVideosPerCall = 1, Next = () => new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }, providerMetadata: OperationJson.Parse("{\"gateway\":{\"cost\":\"" + (turn++ == 0 ? "0.1" : "0.2") + "\"}}")) };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 2 });
        Assert.Equal("0.2", result.ProviderMetadata.GetProperty("gateway").GetProperty("cost").GetString());
    }

    [Fact]
    [UpstreamTest(Video + " > provider metadata merging::should handle undefined providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_omitted_video_metadata()
    {
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = new VideoFake { Result = Clip() }, Prompt = new VideoPrompt("a cat") });
        Assert.Equal(0, result.ProviderMetadata.EnumerateObject().Count());
    }

    [Fact]
    [UpstreamTest(Video + " > provider metadata merging::should preserve per-call providerMetadata in responses array", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_per_call_metadata_on_responses()
    {
        var turn = 0;
        var model = new VideoFake { MaxVideosPerCall = 1, Next = () => new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }, providerMetadata: OperationJson.Parse("{\"p\":{\"n\":" + turn++ + "}}")) };
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 2 });
        Assert.Equal(0, result.Responses[0].ProviderMetadata!.Value.GetProperty("p").GetProperty("n").GetInt32());
        Assert.Equal(1, result.Responses[1].ProviderMetadata!.Value.GetProperty("p").GetProperty("n").GetInt32());
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should handle string prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_a_string_prompt()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") });
        Assert.Equal("a cat", model.Generates[0].Prompt);
        Assert.Null(model.Generates[0].Image);
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should handle object prompt with text and image", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_a_prompt_image()
    {
        var png = Png();
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat", png) });
        Assert.Equal("file", model.Generates[0].Image!.Type);
        Assert.Equal("image/png", model.Generates[0].Image.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should handle URL image in prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_an_http_prompt_image_as_a_url()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat", "https://example.com/cat.png") });
        Assert.Equal("url", model.Generates[0].Image!.Type);
        Assert.Equal("https://example.com/cat.png", model.Generates[0].Image.Url);
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should handle data URL image in prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Decodes_a_data_url_prompt_image()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat", "data:image/png;base64," + Png()) });
        Assert.Equal("image/png", model.Generates[0].Image!.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should handle Uint8Array image in prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_binary_prompt_images()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat", Convert.FromBase64String(Png())) });
        Assert.Equal("image/png", model.Generates[0].Image!.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should detect image mediaType from raw base64 string via signature detection", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_a_base64_prompt_image()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat", Png()) });
        Assert.Equal("image/png", model.Generates[0].Image!.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > prompt normalization::should detect image mediaType from Uint8Array via signature detection", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_a_binary_prompt_image()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat", Convert.FromBase64String(Png())), null, null);
        Assert.Equal("image/png", inputs.Image!.MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use doStart/doStatus when poll is provided and model supports it", Coverage = UpstreamCoverage.Covered)]
    public async Task Polls_when_the_model_can_start()
    {
        var model = StartModel(new[] { "completed" });
        await GenerateVideo.GenerateVideoAsync(Poll(model));
        Assert.Empty(model.Generates);
        Assert.Single(model.Starts);
        Assert.Single(model.Statuses);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use doStart/doStatus when model only has doStart/doStatus (no doGenerate)", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_start_when_generate_is_missing()
    {
        var model = StartModel(new[] { "completed" });
        model.CanGenerate = false;
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), Delay = (_, _) => Task.CompletedTask, PollTimeoutMs = 1000, PollIntervalMs = 1 });
        Assert.Single(model.Starts);
        Assert.Empty(model.Generates);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should retry doStatus without restarting the operation", Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_status_without_restarting()
    {
        var model = StartModel(new[] { "completed" });
        model.StatusFailures = 1;
        await GenerateVideo.GenerateVideoAsync(Poll(model, maxRetries: 1));
        Assert.Single(model.Starts);
        Assert.Equal(2, model.Statuses.Count);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should send one stable idempotency key across doStart retries", Coverage = UpstreamCoverage.Covered)]
    public async Task Reuses_one_idempotency_key_across_start_retries()
    {
        var model = StartModel(new[] { "completed" });
        model.StartFailures = 1;
        await GenerateVideo.GenerateVideoAsync(Poll(model, maxRetries: 1, id: () => "one"));
        Assert.Equal(2, model.Starts.Count);
        Assert.Equal(model.Starts[0].Headers["idempotency-key"], model.Starts[1].Headers["idempotency-key"]);
        Assert.Equal("aisdk_vid_one", model.Starts[0].Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should mint a distinct idempotency key per generateVideo call", Coverage = UpstreamCoverage.Covered)]
    public async Task Mints_a_distinct_idempotency_key_per_call()
    {
        var suffix = 0;
        var model = StartModel(new[] { "completed" });
        await GenerateVideo.GenerateVideoAsync(Poll(model, id: () => (suffix++).ToString()));
        await GenerateVideo.GenerateVideoAsync(Poll(model, id: () => (suffix++).ToString()));
        Assert.Equal("aisdk_vid_0", model.Starts[0].Headers["idempotency-key"]);
        Assert.Equal("aisdk_vid_1", model.Starts[1].Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should preserve a caller-supplied idempotency key instead of minting one", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_a_caller_idempotency_key()
    {
        var model = StartModel(new[] { "completed" });
        var request = Poll(model);
        request.Headers = new Dictionary<string, string> { ["idempotency-key"] = "caller" };
        await GenerateVideo.GenerateVideoAsync(request);
        Assert.Equal("caller", model.Starts[0].Headers["idempotency-key"]);
        Assert.False(model.Statuses[0].Headers.ContainsKey("idempotency-key"));
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should honor a caller-supplied idempotency key regardless of header casing", Coverage = UpstreamCoverage.Covered)]
    public async Task Honors_idempotency_key_casing()
    {
        var model = StartModel(new[] { "completed" });
        var request = Poll(model);
        request.Headers = new Dictionary<string, string> { ["Idempotency-Key"] = "Caller" };
        await GenerateVideo.GenerateVideoAsync(request);
        Assert.Equal("Caller", model.Starts[0].Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should fall back to doGenerate when poll is provided but model lacks doStart/doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_generate_when_start_is_missing()
    {
        var seen = Watch();
        try
        {
            var model = new VideoFake { Result = Clip() };
            var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), Poll = true });
            Assert.Single(model.Generates);
            Assert.Contains("Falling back to doGenerate", seen[0].Warnings[0].Message);
            Assert.Empty(result.Warnings);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should throw error when model lacks both doGenerate and doStart/doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_the_model_implements_neither_flow()
    {
        var model = new VideoFake { CanGenerate = false };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") }));
        Assert.Contains("does not implement doGenerate or doStart/doStatus", error.Message);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use custom intervalMs for polling", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_custom_poll_interval()
    {
        var delays = new List<int>();
        var model = StartModel(new[] { "pending", "completed" });
        await GenerateVideo.GenerateVideoAsync(Poll(model, interval: 25, delay: (ms, _) => { delays.Add(ms); return Task.CompletedTask; }));
        Assert.Contains(25, delays);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use a custom delay for polling", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_custom_poll_delay()
    {
        var used = false;
        var model = StartModel(new[] { "completed" });
        await GenerateVideo.GenerateVideoAsync(Poll(model, delay: (_, _) => { used = true; return Task.CompletedTask; }));
        Assert.True(used);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should throw timeout error when polling exceeds timeoutMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Times_out_when_polling_exceeds_the_limit()
    {
        var model = StartModel(new[] { "pending" });
        var error = await Assert.ThrowsAsync<TimeoutException>(() => GenerateVideo.GenerateVideoAsync(Poll(model, timeout: 0)));
        Assert.Equal("Video generation timed out after 0ms.", error.Message);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should abort an in-flight status request when polling times out", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_an_in_flight_status_request_on_timeout()
    {
        var model = StartModel(new[] { "pending" });
        model.HoldStatus = true;
        await Assert.ThrowsAsync<TimeoutException>(() => GenerateVideo.GenerateVideoAsync(Poll(model, timeout: 20, delay: (_, _) => Task.CompletedTask)));
        Assert.True(model.StatusCancelled);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should reject a completed status result returned after the polling timeout", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_a_status_that_completes_after_the_timeout()
    {
        var model = StartModel(new[] { "completed" });
        model.HoldStatus = true;
        var error = await Assert.ThrowsAsync<TimeoutException>(() => GenerateVideo.GenerateVideoAsync(Poll(model, timeout: 15, delay: (_, _) => Task.CompletedTask)));
        Assert.Contains("timed out", error.Message);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should merge warnings from doStart and doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_start_and_status_warnings()
    {
        var model = StartModel(new[] { "completed" });
        model.StartWarnings = new[] { OperationWarning.Other("start") };
        model.StatusWarnings = new[] { OperationWarning.Other("status") };
        var result = await GenerateVideo.GenerateVideoAsync(Poll(model));
        Assert.Equal(new[] { "start", "status" }, result.Warnings.Select(item => item.Message));
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should merge warnings and metadata from pending status results", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_pending_status_metadata()
    {
        var model = StartModel(new[] { "pending", "completed" });
        model.StatusMetadata = OperationJson.Parse("{\"p\":{\"videos\":[{\"id\":\"a\"}],\"step\":1}}");
        var result = await GenerateVideo.GenerateVideoAsync(Poll(model));
        Assert.True(result.ProviderMetadata.GetProperty("p").TryGetProperty("step", out _));
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use webhook flow when webhook is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Waits_for_a_webhook_before_status()
    {
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        var received = Task.CompletedTask;
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", received)),
            Delay = (_, _) => Task.CompletedTask,
            PollTimeoutMs = 1000,
        });
        Assert.Equal("https://example.com/hook", model.Starts[0].WebhookUrl);
        Assert.Single(model.Statuses);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow > webhook receiver rejection::should observe rejection $timing while preserving start precedence (startFails: $startFails)", Coverage = UpstreamCoverage.Covered)]
    public async Task Observes_a_rejected_webhook_receiver()
    {
        foreach (var (duringStart, startFails) in new[] { (true, false), (false, false), (true, true), (false, true) })
        {
            var model = StartModel(new[] { "completed" });
            model.CanWebhook = true;
            var hook = new InvalidOperationException("hook");
            var received = new TaskCompletionSource<bool>();
            model.WebhookFactory = _ =>
            {
                if (!duringStart)
                {
                    received.SetException(hook);
                }

                return Task.FromResult(new VideoWebhook("https://example.com/hook", received.Task));
            };
            model.OnStart = () =>
            {
                if (duringStart)
                {
                    received.TrySetException(hook);
                }

                if (startFails)
                {
                    throw new InvalidOperationException("start failed");
                }
            };
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
            {
                Model = model,
                Prompt = new VideoPrompt("a cat"),
                Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", received.Task)),
                MaxRetries = 0,
                Delay = (_, _) => Task.CompletedTask,
                PollTimeoutMs = 1000,
            }));
            Assert.Equal(startFails ? "start failed" : "hook", error.Message);
            Assert.Empty(model.Statuses);
            Assert.True(received.Task.IsFaulted);
        }
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow > webhook receiver rejection::should observe late webhook rejection after start fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Observes_a_late_webhook_rejection_after_start_fails()
    {
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        var received = new TaskCompletionSource<bool>();
        model.OnStart = () => throw new InvalidOperationException("start failed");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", received.Task)),
            MaxRetries = 0,
        }));
        received.SetException(new InvalidOperationException("late"));
        await Task.Yield();
        Assert.Equal("start failed", error.Message);
        Assert.True(received.Task.IsFaulted);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should assimilate a then-only webhook receiver once before start completes", Coverage = UpstreamCoverage.Covered)]
    public async Task Awaits_the_webhook_receiver_once()
    {
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        var calls = 0;
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => { calls++; return Task.FromResult(new VideoWebhook("https://example.com/hook", Task.CompletedTask)); },
            Delay = (_, _) => Task.CompletedTask,
            PollTimeoutMs = 1000,
        });
        Assert.Equal(1, calls);
        Assert.Equal(1, model.WebhookCalls);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use a custom delay for the webhook timeout", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_custom_webhook_delay()
    {
        var delays = new List<int>();
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        var received = new TaskCompletionSource<bool>();
        await Assert.ThrowsAsync<TimeoutException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", received.Task)),
            PollTimeoutMs = 40,
            Delay = (ms, _) => { delays.Add(ms); return Task.CompletedTask; },
        }));
        Assert.Contains(40, delays);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should use webhook over poll when both are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_a_webhook_over_polling()
    {
        var delays = new List<int>();
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Poll = true,
            PollIntervalMs = 5,
            PollTimeoutMs = 80,
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", Task.CompletedTask)),
            Delay = (ms, _) => { delays.Add(ms); return Task.CompletedTask; },
        });
        Assert.DoesNotContain(5, delays);
        Assert.Equal("https://example.com/hook", model.Starts[0].WebhookUrl);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should timeout when webhook notification is not received", Coverage = UpstreamCoverage.Covered)]
    public async Task Times_out_when_the_webhook_never_arrives()
    {
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        var error = await Assert.ThrowsAsync<TimeoutException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", new TaskCompletionSource<bool>().Task)),
            PollTimeoutMs = 10,
            Delay = (_, _) => Task.CompletedTask,
        }));
        Assert.Equal("Video generation timed out after 10ms.", error.Message);
        Assert.Empty(model.Statuses);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should abort while waiting for webhook notification", Coverage = UpstreamCoverage.Covered)]
    public async Task Aborts_while_waiting_for_a_webhook()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var reason = new InvalidOperationException("Cancelled");
        var model = StartModel(new[] { "completed" });
        model.CanWebhook = true;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", new TaskCompletionSource<bool>().Task)),
            CancellationToken = source.Token,
            AbortReason = reason,
            PollTimeoutMs = 1000,
        }));
        Assert.Same(reason, error);
        Assert.Empty(model.Starts);
        Assert.Empty(model.Statuses);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should fall back to polling when model has no handleWebhookOption", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_polling_when_webhooks_are_unsupported()
    {
        var model = StartModel(new[] { "completed" });
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Webhook = () => Task.FromResult(new VideoWebhook("https://example.com/hook", Task.CompletedTask)),
            Delay = (_, _) => Task.CompletedTask,
            PollIntervalMs = 1,
            PollTimeoutMs = 1000,
        });
        Assert.Contains("does not support webhooks", result.Warnings[0].Message);
        Assert.Null(model.Starts[0].WebhookUrl);
        Assert.Single(model.Statuses);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should handle n > maxVideosPerCall with doStart/doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task Splits_a_start_status_batch()
    {
        var model = StartModel(new[] { "completed" });
        model.MaxVideosPerCall = 1;
        var result = await GenerateVideo.GenerateVideoAsync(Poll(model, n: 2));
        Assert.Equal(2, model.Starts.Count);
        Assert.Equal(2, result.Videos.Count);
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should return provider metadata from doStart/doStatus flow", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_start_and_status_metadata()
    {
        var model = StartModel(new[] { "completed" });
        model.StartMetadata = OperationJson.Parse("{\"p\":{\"job\":\"1\"}}");
        model.StatusMetadata = OperationJson.Parse("{\"p\":{\"done\":true}}");
        var result = await GenerateVideo.GenerateVideoAsync(Poll(model));
        Assert.Equal("1", result.ProviderMetadata.GetProperty("p").GetProperty("job").GetString());
        Assert.True(result.ProviderMetadata.GetProperty("p").GetProperty("done").GetBoolean());
    }

    [Fact]
    [UpstreamTest(Video + " > doStart/doStatus flow::should pass headers and abortSignal to doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_headers_and_the_token_to_status()
    {
        using var source = new CancellationTokenSource();
        var model = StartModel(new[] { "completed" });
        var request = Poll(model);
        request.Headers = new Dictionary<string, string> { ["X-Test"] = "1" };
        request.CancellationToken = source.Token;
        await GenerateVideo.GenerateVideoAsync(request);
        Assert.Equal("1", model.Statuses[0].Headers["x-test"]);
        Assert.Equal("ai/0.0.0-test", model.Statuses[0].Headers["user-agent"]);
        Assert.False(model.Statuses[0].Headers.ContainsKey("idempotency-key"));
        Assert.Equal(source.Token, model.Statuses[0].CancellationToken);
    }

    [Fact]
    [UpstreamTest(Video + " > frameImages::should normalize and pass frameImages through to the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_frame_images()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), FrameImages = new[] { new VideoFrameImage(Png(), "first_frame"), new VideoFrameImage("https://example.com/last.png", "last_frame") } });
        Assert.Equal(new[] { "first_frame", "last_frame" }, model.Generates[0].FrameImages!.Select(item => item.FrameType));
        Assert.Equal("image/png", model.Generates[0].FrameImages![0].Image.MediaType);
        Assert.Equal("url", model.Generates[0].FrameImages![1].Image.Type);
    }

    [Fact]
    [UpstreamTest(Video + " > frameImages::should copy a first_frame entry into the image field when no image is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Copies_the_first_frame_into_the_image()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat"), new[] { new VideoFrameImage(Png(), "first_frame") }, null);
        Assert.Equal("image/png", inputs.Image!.MediaType);
        Assert.Empty(inputs.Warnings);
    }

    [Fact]
    [UpstreamTest(Video + " > frameImages::should prefer the first_frame over prompt.image and warn when both are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_the_first_frame_over_the_prompt_image()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat", "https://example.com/prompt.png"), new[] { new VideoFrameImage("https://example.com/first.png", "first_frame") }, null);
        Assert.Equal("https://example.com/first.png", inputs.Image!.Url);
        Assert.Contains("prompt.image was ignored", inputs.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Video + " > frameImages::should pass only last_frame in frameImages without setting image", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_the_image_empty_for_a_last_frame()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat"), new[] { new VideoFrameImage(Png(), "last_frame") }, null);
        Assert.Null(inputs.Image);
        Assert.Equal("last_frame", inputs.FrameImages![0].FrameType);
    }

    [Fact]
    [UpstreamTest(Video + " > frameImages::should keep the prompt image when frameImages only has a last_frame and prompt image is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_the_prompt_image_beside_a_last_frame()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat", "https://example.com/prompt.png"), new[] { new VideoFrameImage(Png(), "last_frame") }, null);
        Assert.Equal("https://example.com/prompt.png", inputs.Image!.Url);
        Assert.Empty(inputs.Warnings);
    }

    [Fact]
    [UpstreamTest(Video + " > inputReferences::should normalize and pass inputReferences through to the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_input_references()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), InputReferences = new[] { new VideoReference("https://example.com/ref.mp4"), new VideoReference(Mp4) } });
        Assert.Equal("url", model.Generates[0].InputReferences![0].Type);
        Assert.Equal("video/mp4", model.Generates[0].InputReferences![1].MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > inputReferences::should detect video media type from binary inputReferences without object form", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_binary_reference_media_types()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat"), null, new[] { new VideoReference(Convert.FromBase64String(Mp4)) });
        Assert.Equal("video/mp4", inputs.InputReferences![0].MediaType);
    }

    [Fact]
    [UpstreamTest(Video + " > inputReferences::should carry mediaType from the object form for URL references", Coverage = UpstreamCoverage.Covered)]
    public async Task Carries_an_explicit_reference_media_type()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat"), null, new[] { new VideoReference("https://example.com/ref.mp4", "video/webm") });
        Assert.Equal("video/webm", inputs.InputReferences![0].MediaType);
        Assert.Equal("url", inputs.InputReferences[0].Type);
    }

    [Fact]
    [UpstreamTest(Video + " > inputReferences::should pass inputReferences as undefined when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_references_when_they_are_not_provided()
    {
        var model = new VideoFake { Result = Clip() };
        await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") });
        Assert.Null(model.Generates[0].InputReferences);
    }

    [Fact]
    [UpstreamTest(Video + " > inputReferences::should ignore inputReferences when frameImages is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_references_when_frames_are_present()
    {
        var inputs = GenerateVideo.Normalize(new VideoPrompt("a cat"), new[] { new VideoFrameImage(Png(), "last_frame") }, new[] { new VideoReference("https://example.com/ref.mp4") });
        Assert.Null(inputs.InputReferences);
        Assert.Contains("inputReferences were ignored", inputs.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Start + "should call doStart with a fully populated spec call-options object", Coverage = UpstreamCoverage.Covered)]
    public async Task Populates_the_start_call()
    {
        var model = StartModel(Array.Empty<string>());
        await StartVideo.StartVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat", Png()),
            N = 1,
            AspectRatio = "16:9",
            Resolution = "720p",
            Duration = 5,
            Fps = 24,
            Seed = 3,
            GenerateAudio = false,
            ProviderOptions = OperationJson.Parse("{\"p\":true}"),
            Headers = new Dictionary<string, string> { ["X-Test"] = "1" },
        });
        var call = model.Starts[0];
        Assert.Equal("a cat", call.Prompt);
        Assert.Equal("image/png", call.Image!.MediaType);
        Assert.Equal("16:9", call.AspectRatio);
        Assert.Equal("ai/0.0.0-test", call.Headers["user-agent"]);
        Assert.StartsWith("aisdk_vid_", call.Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(Start + "should forward webhookUrl to doStart", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_the_webhook_url()
    {
        var model = StartModel(Array.Empty<string>());
        await StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), WebhookUrl = "https://example.com/hook" });
        Assert.Equal("https://example.com/hook", model.Starts[0].WebhookUrl);
    }

    [Fact]
    [UpstreamTest(Start + "should keep a caller-supplied idempotency key", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_the_start_idempotency_key()
    {
        var model = StartModel(Array.Empty<string>());
        await StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), Headers = new Dictionary<string, string> { ["Idempotency-Key"] = "keep" } });
        Assert.Equal("keep", model.Starts[0].Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(Start + "should reuse one idempotency key across start retries", Coverage = UpstreamCoverage.Covered)]
    public async Task Reuses_the_start_idempotency_key()
    {
        var model = StartModel(Array.Empty<string>());
        model.StartFailures = 1;
        await StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), MaxRetries = 1, GenerateId = () => "same" });
        Assert.Equal("aisdk_vid_same", model.Starts[0].Headers["idempotency-key"]);
        Assert.Equal(model.Starts[0].Headers["idempotency-key"], model.Starts[1].Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(Start + "should surface provider metadata (job id, signing secret) from the start response", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_start_provider_metadata()
    {
        var model = StartModel(Array.Empty<string>());
        model.StartMetadata = OperationJson.Parse("{\"jobId\":\"job\",\"signingSecret\":\"secret\"}");
        var result = await StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") });
        Assert.Equal("job", result.ProviderMetadata!.Value.GetProperty("jobId").GetString());
        Assert.Equal("op", result.Operation);
    }

    [Fact]
    [UpstreamTest(Start + "should combine input-normalization warnings with start warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Combines_normalization_and_start_warnings()
    {
        var model = StartModel(Array.Empty<string>());
        model.StartWarnings = new[] { OperationWarning.Other("started") };
        var result = await StartVideo.StartVideoAsync(new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat", "https://example.com/prompt.png"),
            FrameImages = new[] { new VideoFrameImage("https://example.com/first.png", "first_frame") },
        });
        Assert.Contains("prompt.image was ignored", result.Warnings[0].Message);
        Assert.Equal("started", result.Warnings[1].Message);
    }

    [Fact]
    [UpstreamTest(Start + "should throw when n is not a positive integer", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_non_positive_video_count()
    {
        foreach (var n in new[] { 0d, -1d, 1.5d })
        {
            var model = StartModel(Array.Empty<string>());
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = n }));
            Assert.Contains("Invalid n: expected a positive integer, received " + n.ToString(System.Globalization.CultureInfo.InvariantCulture), error.Message);
            Assert.Empty(model.Starts);
        }
    }

    [Fact]
    [UpstreamTest(Start + "should throw when n exceeds the known per-call limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_count_above_the_per_call_limit()
    {
        var model = StartModel(Array.Empty<string>());
        model.MaxVideosPerCall = 1;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 2 }));
        Assert.Contains("supports at most 1 video(s) per call, but 2 were requested", error.Message);
    }

    [Fact]
    [UpstreamTest(Start + "should invoke a functional maxVideosPerCall", Coverage = UpstreamCoverage.Covered)]
    public async Task Invokes_a_functional_start_limit()
    {
        var model = StartModel(Array.Empty<string>());
        model.FunctionalMax = 3;
        await StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat"), N = 3 });
        Assert.True(model.Resolved);
        Assert.Equal(3, model.Starts[0].N);
    }

    [Fact]
    [UpstreamTest(Start + "should throw when the model does not implement doStart", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_start_is_missing()
    {
        var model = new VideoFake();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => StartVideo.StartVideoAsync(new GenerateVideoRequest { Model = model, Prompt = new VideoPrompt("a cat") }));
        Assert.Contains("does not implement doStart", error.Message);
    }

    [Fact]
    [UpstreamTest(Status + "should call doStatus with the operation", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_do_status_with_the_operation()
    {
        var model = StartModel(new[] { "pending" });
        var operation = new object();
        await GetVideoStatus.GetVideoStatusAsync(new GetVideoStatusRequest { Model = model, Operation = operation, Headers = new Dictionary<string, string> { ["X-Test"] = "1" } });
        Assert.Same(operation, model.Statuses[0].Operation);
        Assert.Equal("1", model.Statuses[0].Headers["x-test"]);
        Assert.Contains("ai/", model.Statuses[0].Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Status + "should return the completed payload with videos", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_completed_status_payload()
    {
        var model = StartModel(new[] { "completed" });
        var result = await GetVideoStatus.GetVideoStatusAsync(new GetVideoStatusRequest { Model = model, Operation = "op" });
        Assert.Equal("completed", result.Status);
        Assert.Single(result.Videos);
    }

    [Fact]
    [UpstreamTest(Status + "should throw when the model does not implement doStatus", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_status_is_missing()
    {
        var model = new VideoFake { CanStart = true };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GetVideoStatus.GetVideoStatusAsync(new GetVideoStatusRequest { Model = model, Operation = "op" }));
        Assert.Contains("does not implement doStatus", error.Message);
    }

    private static VideoModelResult Clip(params OperationWarning[] warnings)
    {
        return new VideoModelResult(new[] { new VideoOutput("base64", data: Mp4, mediaType: "video/mp4") }, warnings);
    }

    private static string Png()
    {
        return "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAACklEQVR4nGMAAQAABQABDQottAAAAABJRU5ErkJggg==";
    }

    private static GenerateVideoRequest Poll(VideoFake model, int? maxRetries = null, Func<string>? id = null, int? interval = null, int? timeout = null, Func<int, CancellationToken, Task>? delay = null, double n = 1)
    {
        return new GenerateVideoRequest
        {
            Model = model,
            Prompt = new VideoPrompt("a cat"),
            Poll = true,
            N = n,
            MaxRetries = maxRetries,
            GenerateId = id,
            PollIntervalMs = interval ?? 1,
            PollTimeoutMs = timeout ?? 1000,
            Delay = delay ?? ((_, _) => Task.CompletedTask),
        };
    }

    private static VideoFake StartModel(string[] statuses)
    {
        return new VideoFake { CanGenerate = true, CanStart = true, CanStatus = true, StatusScript = statuses };
    }

    private static List<WarningLogContext> Watch()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        return seen;
    }

    private sealed class VideoFake : IVideoCaller
    {
        private int _statusIndex;

        public string Provider { get; set; } = "test-provider";

        public string ModelId { get; set; } = "test-model";

        public int? MaxVideosPerCall { get; set; }

        public int? FunctionalMax { get; set; }

        public bool CanGenerate { get; set; } = true;

        public bool CanStart { get; set; }

        public bool CanStatus { get; set; }

        public bool CanWebhook { get; set; }

        public bool Resolved { get; private set; }

        public VideoModelResult Result { get; set; } = new VideoModelResult(new[] { new VideoOutput("base64", data: "AAAAIGZ0eXBpc29tAAACAGlzb20iaXNvMgAAAAhmcmVl", mediaType: "video/mp4") });

        public Func<VideoModelResult>? Next { get; set; }

        public string[] StatusScript { get; set; } = Array.Empty<string>();

        public int StartFailures { get; set; }

        public int StatusFailures { get; set; }

        public bool HoldStatus { get; set; }

        public bool StatusCancelled { get; private set; }

        public IReadOnlyList<OperationWarning> StartWarnings { get; set; } = Array.Empty<OperationWarning>();

        public IReadOnlyList<OperationWarning> StatusWarnings { get; set; } = Array.Empty<OperationWarning>();

        public JsonElement? StartMetadata { get; set; }

        public JsonElement? StatusMetadata { get; set; }

        public Action? OnStart { get; set; }

        public Func<Func<Task<VideoWebhook>>, Task<VideoWebhook>>? WebhookFactory { get; set; }

        public int WebhookCalls { get; private set; }

        public List<VideoModelCall> Generates { get; } = new List<VideoModelCall>();

        public List<VideoModelCall> Starts { get; } = new List<VideoModelCall>();

        public List<VideoModelCall> Statuses { get; } = new List<VideoModelCall>();

        public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
        {
            Resolved = true;
            return Task.FromResult(FunctionalMax ?? MaxVideosPerCall);
        }

        public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
        {
            Generates.Add(call);
            return Task.FromResult(Next?.Invoke() ?? Result);
        }

        public Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
        {
            Starts.Add(call);
            OnStart?.Invoke();
            if (StartFailures > 0)
            {
                StartFailures--;
                throw new RetryableCallException("start limited", 429, new Dictionary<string, string> { ["retry-after-ms"] = "0" });
            }

            return Task.FromResult(new VideoStartResult("op", StartWarnings, StartMetadata));
        }

        public async Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
        {
            Statuses.Add(call);
            if (HoldStatus)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    StatusCancelled = true;
                    throw;
                }
            }

            if (StatusFailures > 0)
            {
                StatusFailures--;
                throw new RetryableCallException("status limited", 429, new Dictionary<string, string> { ["retry-after-ms"] = "0" });
            }

            var status = StatusScript.Length == 0 ? "completed" : StatusScript[Math.Min(_statusIndex++, StatusScript.Length - 1)];
            var videos = status == "completed" ? new[] { new VideoOutput("base64", data: "AAAAIGZ0eXBpc29tAAACAGlzb20iaXNvMgAAAAhmcmVl", mediaType: "video/mp4") } : Array.Empty<VideoOutput>();
            return new VideoStatusResult(status, videos, warnings: StatusWarnings, providerMetadata: StatusMetadata);
        }

        public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
        {
            WebhookCalls++;
            return WebhookFactory == null ? webhook() : WebhookFactory(webhook);
        }
    }
}
