// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should encode the task ID as a single URL path segment",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_the_task_ID_as_a_single_URL_path_segment()
    {
        var handler = Server("{\"task_id\":\"abc/../../internal\"}");

        await Generate(handler, new MiniMaxCall());

        Assert.Contains(handler.Calls, call =>
            call.Uri.AbsoluteUri == BaseUrl + "/v2/query/video_generation/abc%2F..%2F..%2Finternal"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send a text-to-video request with required fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_a_text_to_video_request_with_required_fields()
    {
        var handler = Server();

        await Generate(handler, new MiniMaxCall());

        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal(CreateUrl, handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(RequestBody(handler), Body());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the documented default payload for MiniMax-H3-Max",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_documented_default_payload_for_MiniMax_H3_Max()
    {
        var handler = Server();

        await Generate(handler, new MiniMaxCall(), "MiniMax-H3-Max");

        JsonAssert.Equal(RequestBody(handler), Body("MiniMax-H3-Max", "768P"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the video URL from the completed task",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_the_video_URL_from_the_completed_task()
    {
        var result = await Generate(Server(), new MiniMaxCall());

        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://cdn.minimax.io/output/video-001.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Equal(TaskId, Resolve(result.ProviderMetadata, "taskId")!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map aspectRatio and duration into the request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_aspectRatio_and_duration_into_the_request()
    {
        var handler = Server();

        await Generate(handler, new MiniMaxCall { AspectRatio = "16:9", Duration = 10 });

        JsonAssert.Equal(RequestBody(handler), Body(duration: 10));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should clamp a duration above the maximum and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_clamp_a_duration_above_the_maximum_and_warn()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Duration = 20 });

        JsonAssert.Equal(RequestBody(handler), Body(duration: 15));
        AssertWarning(
            result.Warnings,
            "duration",
            "MiniMax-H3 supports at most 15 seconds. The requested duration of 20 was clamped to 15."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should clamp a duration below the minimum and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_clamp_a_duration_below_the_minimum_and_warn()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Duration = 3 });

        JsonAssert.Equal(RequestBody(handler), Body(duration: 4));
        AssertWarning(
            result.Warnings,
            "duration",
            "MiniMax-H3 requires at least 4 seconds. The requested duration of 3 was clamped to 4."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should round a fractional duration and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_round_a_fractional_duration_and_warn()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Duration = 7.5 });

        JsonAssert.Equal(RequestBody(handler), Body(duration: 8));
        AssertWarning(
            result.Warnings,
            "duration",
            "MiniMax-H3 requires a whole number of seconds. The requested duration of 7.5 was rounded to 8."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should round and then clamp a fractional out-of-range duration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_round_and_then_clamp_a_fractional_out_of_range_duration()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Duration = 3.2 });

        JsonAssert.Equal(RequestBody(handler), Body(duration: 4));
        Assert.Equal(
            new[] { "duration", "duration" },
            result.Warnings.Where(w => w.Type == "unsupported").Select(w => w.Feature)
        );
    }

    [Theory]
    [InlineData("2048x2048", "1:1")]
    [InlineData("2560x1080", "21:9")]
    [InlineData("2560x1440", "16:9")]
    [InlineData("2048x1536", "4:3")]
    [InlineData("1440x2560", "9:16")]
    [InlineData("1536x2048", "3:4")]
    [UpstreamTest(
        Prefix + "should map the top-level resolution %s (%s) onto the named tier",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_the_top_level_resolution_onto_the_named_tier(
        string resolution,
        string aspectRatio
    )
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { Resolution = resolution, AspectRatio = aspectRatio }
        );

        JsonAssert.Equal(RequestBody(handler), Body(resolution: "2K", ratio: aspectRatio));
        Assert.False(HasWarning(result.Warnings, "resolution"));
    }

    [Theory]
    [InlineData("768x768", "1:1")]
    [InlineData("1792x768", "21:9")]
    [InlineData("1366x768", "16:9")]
    [InlineData("1024x768", "4:3")]
    [InlineData("768x1366", "9:16")]
    [InlineData("768x1024", "3:4")]
    [UpstreamTest(
        Prefix + "should map the top-level resolution %s (%s) onto the 768P tier",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_the_top_level_resolution_onto_the_768P_tier(
        string resolution,
        string aspectRatio
    )
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { Resolution = resolution, AspectRatio = aspectRatio }
        );

        JsonAssert.Equal(RequestBody(handler), Body(resolution: "768P", ratio: aspectRatio));
        Assert.False(HasWarning(result.Warnings, "resolution"));
    }

    [Theory]
    [InlineData("480x480", "1:1")]
    [InlineData("1120x480", "21:9")]
    [InlineData("854x480", "16:9")]
    [InlineData("640x480", "4:3")]
    [InlineData("480x854", "9:16")]
    [InlineData("480x640", "3:4")]
    [UpstreamTest(
        Prefix + "should map the top-level resolution %s (%s) onto the 480P tier",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_the_top_level_resolution_onto_the_480P_tier(
        string resolution,
        string aspectRatio
    )
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { Resolution = resolution, AspectRatio = aspectRatio },
            "MiniMax-H3-Max"
        );

        JsonAssert.Equal(RequestBody(handler), Body("MiniMax-H3-Max", "480P", ratio: aspectRatio));
        Assert.False(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map a 768P frame size on MiniMax-H3-Max, not just fall back to its default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_a_768P_frame_size_on_MiniMax_H3_Max_not_just_fall_back_to_its_default()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { Resolution = "1366x768", AspectRatio = "16:9" },
            "MiniMax-H3-Max"
        );

        Assert.Equal("MiniMax-H3-Max", RequestBody(handler)!["model"]!.GetValue<string>());
        Assert.Equal("768P", RequestBody(handler)!["resolution"]!.GetValue<string>());
        Assert.False(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should warn when a mapped top-level resolution names a different tier than the provider option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_a_mapped_top_level_resolution_names_a_different_tier_than_the_provider_option()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                Resolution = "1366x768",
                ProviderOptions = MiniMaxOptions("{\"resolution\":\"2K\"}"),
            }
        );

        Assert.Equal("2K", RequestBody(handler)!["resolution"]!.GetValue<string>());
        AssertWarning(
            result.Warnings,
            "resolution",
            "The resolution \"1366x768\" selects 768P, but providerOptions.minimax.resolution (\"2K\") was used instead."
        );
    }

    [Theory]
    [InlineData("MiniMax-H3", "854x480", "480P", "2K")]
    [InlineData("MiniMax-H3-Max", "2560x1440", "2K", "768P")]
    [UpstreamTest(
        Prefix + "should warn that %s does not support the %s tier and use %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_that_model_does_not_support_the_tier_and_use_default(
        string modelId,
        string resolution,
        string tier,
        string expectedResolution
    )
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Resolution = resolution }, modelId);

        Assert.Equal(expectedResolution, RequestBody(handler)!["resolution"]!.GetValue<string>());
        Assert.Contains(result.Warnings, w =>
            w.Type == "unsupported"
            && w.Feature == "resolution"
            && w.Details!.Contains(modelId + " does not support the resolution \"" + tier + "\".")
        );
    }

    [Theory]
    [InlineData("MiniMax-H3", "480P", "2K")]
    [InlineData("MiniMax-H3-Max", "2K", "768P")]
    [UpstreamTest(
        Prefix + "should reject %s resolution %s and use %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_model_resolution_and_use_default(
        string modelId,
        string resolution,
        string expectedResolution
    )
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                ProviderOptions = MiniMaxOptions("{\"resolution\":\"" + resolution + "\"}"),
            },
            modelId
        );

        Assert.Equal(expectedResolution, RequestBody(handler)!["resolution"]!.GetValue<string>());
        Assert.Contains(result.Warnings, w =>
            w.Type == "unsupported"
            && w.Feature == "resolution"
            && w.Details!.Contains("The provider resolution \"" + resolution + "\" was ignored.")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and use the default for an unrecognized resolution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_use_the_default_for_an_unrecognized_resolution()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Resolution = "1280x720" });

        JsonAssert.Equal(RequestBody(handler), Body());
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer providerOptions.resolution over the top-level value",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_providerOptions_resolution_over_the_top_level_value()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                Resolution = "1280x720",
                ProviderOptions = MiniMaxOptions("{\"resolution\":\"2K\"}"),
            }
        );

        JsonAssert.Equal(RequestBody(handler), Body());
        Assert.True(HasWarning(result.Warnings, "resolution"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send first/last frame images with roles and ignore ratio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_first_last_frame_images_with_roles_and_ignore_ratio()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                AspectRatio = "16:9",
                FrameImages = new[]
                {
                    Frame(ImageUrlFile, "first_frame"),
                    Frame(
                        VideoModelFile.FromUrl("https://cdn.example.com/last.png", "image/png"),
                        "last_frame"
                    ),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part("image_url", ImageUrl, "first_frame")
                    + Part("image_url", "https://cdn.example.com/last.png", "last_frame")
            )
        );
        Assert.True(HasWarning(result.Warnings, "aspectRatio"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should route inputReferences into reference_image/reference_video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_route_inputReferences_into_reference_image_reference_video()
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[]
                {
                    ImageUrlFile,
                    VideoModelFile.FromUrl("https://cdn.example.com/ref.mp4", "video/mp4"),
                },
                ProviderOptions = MiniMaxOptions(
                    "{\"referenceAudioUrls\":[\"https://cdn.example.com/ref.wav\"]}"
                ),
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part("image_url", ImageUrl, "reference_image")
                    + Part("video_url", "https://cdn.example.com/ref.mp4", "reference_video")
                    + Part("audio_url", "https://cdn.example.com/ref.wav", "reference_audio")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should treat a URL reference without a mediaType as an image and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_treat_a_URL_reference_without_a_mediaType_as_an_image_and_warn()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[]
                {
                    VideoModelFile.FromUrl("https://cdn.example.com/ref.mp4"),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part(
                    "image_url",
                    "https://cdn.example.com/ref.mp4",
                    "reference_image"
                )
            )
        );
        AssertWarning(
            result.Warnings,
            "inputReferences",
            "MiniMax-H3 requires an explicit mediaType to route URL references as video or image. Pass { data: url, mediaType: \"video/mp4\" } for video references. The reference was treated as an image."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore a reference that is neither an image nor a video and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_ignore_a_reference_that_is_neither_an_image_nor_a_video_and_warn()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[]
                {
                    ImageUrlFile,
                    VideoModelFile.FromUrl("https://cdn.example.com/ref.mp3", "audio/mp3"),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(ratio: null, extraContent: Part("image_url", ImageUrl, "reference_image"))
        );
        AssertWarning(
            result.Warnings,
            "inputReferences",
            "MiniMax-H3 only accepts image and video references; the \"audio/mp3\" reference was ignored. Pass reference audio via providerOptions.minimax.referenceAudioUrls."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn about unsupported fps, seed, and n",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_about_unsupported_fps_seed_and_n()
    {
        var result = await Generate(
            Server(),
            new MiniMaxCall
            {
                Fps = 30,
                Seed = 42,
                N = 2,
            }
        );

        var features = result.Warnings.Where(w => w.Type == "unsupported").Select(w => w.Feature);
        Assert.Contains("fps", features);
        Assert.Contains("seed", features);
        Assert.Contains("n", features);
    }
}
