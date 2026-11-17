// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; warnings</c>.</summary>
public sealed class WarningsTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > warnings::";

    private const string Wav = "https://cdn.example.com/ref.wav";

    [Fact]
    [UpstreamTest(
        Prefix + "should return no warnings for a plain text-to-video request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_no_warnings_for_a_plain_text_to_video_request()
    {
        var result = await Generate(Server(), new MiniMaxCall());

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should identify MiniMax-H3-Max in warnings shared by both models",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_identify_MiniMax_H3_Max_in_warnings_shared_by_both_models()
    {
        var result = await Generate(
            Server(),
            new MiniMaxCall
            {
                Image = VideoUrlFile,
                AspectRatio = "5:3",
                Fps = 30,
                Seed = 42,
                N = 2,
                GenerateAudio = false,
            },
            "MiniMax-H3-Max"
        );

        Assert.Equal(
            new[]
            {
                ("fps", "MiniMax-H3-Max does not support a custom frame rate."),
                ("seed", "MiniMax-H3-Max does not support a seed."),
                (
                    "n",
                    "MiniMax-H3-Max generates a single video per call. Only 1 video will be generated."
                ),
                (
                    "generateAudio",
                    "The MiniMax-H3-Max API does not expose an audio parameter. The generateAudio option was ignored."
                ),
                (
                    "image",
                    "MiniMax-H3-Max does not accept a video as a frame image. The video was ignored."
                ),
                (
                    "aspectRatio",
                    "MiniMax-H3-Max does not support the aspect ratio \"5:3\". Using the default (16:9)."
                ),
            },
            result.Warnings.Select(w =>
            {
                Assert.Equal("unsupported", w.Type);
                return (w.Feature!, w.Details!);
            })
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should warn when generateAudio is %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_generateAudio_is(bool generateAudio)
    {
        var result = await Generate(Server(), new MiniMaxCall { GenerateAudio = generateAudio });

        AssertWarning(
            result.Warnings,
            "generateAudio",
            "The MiniMax-H3 API does not expose an audio parameter. The generateAudio option was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should warn when an unrecognized resolution is passed alongside providerOptions.minimax.resolution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_an_unrecognized_resolution_is_passed_alongside_providerOptions_minimax_resolution()
    {
        var result = await Generate(
            Server(),
            new MiniMaxCall
            {
                Resolution = "1280x720",
                ProviderOptions = MiniMaxOptions("{\"resolution\":\"2K\"}"),
            }
        );

        AssertWarning(
            result.Warnings,
            "resolution",
            "Unrecognized resolution \"1280x720\". MiniMax-H3 supports \"768P\" or \"2K\", so providerOptions.minimax.resolution (\"2K\") was used instead."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not warn when a recognized resolution agrees with the provider option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_warn_when_a_recognized_resolution_agrees_with_the_provider_option()
    {
        var result = await Generate(
            Server(),
            new MiniMaxCall
            {
                Resolution = "2048x1536",
                ProviderOptions = MiniMaxOptions("{\"resolution\":\"2K\"}"),
            }
        );

        Assert.False(HasWarning(result.Warnings, "resolution"));
    }

    [Theory]
    [InlineData("MiniMax-H3-Max", "480P", "480P")]
    [InlineData("MiniMax-H3-Max", "480p", "480P")]
    [InlineData("MiniMax-H3", "768P", "768P")]
    [InlineData("MiniMax-H3", "768p", "768P")]
    [InlineData("MiniMax-H3", "2K", "2K")]
    [InlineData("MiniMax-H3", "2k", "2K")]
    [UpstreamTest(
        Prefix + "should accept the %s resolution tier %s without warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_the_model_resolution_tier_without_warning(
        string modelId,
        string resolution,
        string expectedResolution
    )
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Resolution = resolution }, modelId);

        Assert.False(HasWarning(result.Warnings, "resolution"));
        Assert.Equal(expectedResolution, RequestBody(handler)!["resolution"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should drop reference audio that is not paired with an image or video reference",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_drop_reference_audio_that_is_not_paired_with_an_image_or_video_reference()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                ProviderOptions = MiniMaxOptions("{\"referenceAudioUrls\":[\"" + Wav + "\"]}"),
            }
        );

        JsonAssert.Equal(RequestBody(handler), Body());
        AssertWarning(
            result.Warnings,
            "referenceAudioUrls",
            "MiniMax-H3 reference audio must be paired with at least one reference image or video. The audio was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cap reference audio at 3 entries and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cap_reference_audio_at_3_entries_and_warn()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[] { ImageUrlFile },
                ProviderOptions = MiniMaxOptions(
                    "{\"referenceAudioUrls\":[\"https://cdn.example.com/a1.wav\",\"https://cdn.example.com/a2.wav\",\"https://cdn.example.com/a3.wav\",\"https://cdn.example.com/a4.wav\"]}"
                ),
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part("image_url", ImageUrl, "reference_image")
                    + Part("audio_url", "https://cdn.example.com/a1.wav", "reference_audio")
                    + Part("audio_url", "https://cdn.example.com/a2.wav", "reference_audio")
                    + Part("audio_url", "https://cdn.example.com/a3.wav", "reference_audio")
            )
        );
        AssertWarning(
            result.Warnings,
            "referenceAudioUrls",
            "MiniMax-H3 accepts at most 3 reference audios. Extra audios were ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cap reference images at 9 and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cap_reference_images_at_9_and_warn()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = Enumerable.Range(0, 12).Select(ImageReference).ToList(),
            }
        );

        var content = RequestBody(handler)!["content"]!.AsArray();
        Assert.Equal(10, content.Count);
        Assert.Equal(
            9,
            content.Count(part => part!["role"]?.GetValue<string>() == "reference_image")
        );
        Assert.Equal(
            "https://cdn.example.com/img-8.png",
            content[^1]!["image_url"]!["url"]!.GetValue<string>()
        );
        AssertWarning(
            result.Warnings,
            "inputReferences",
            "MiniMax-H3 accepts at most 9 reference images. Extra images were ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cap reference videos at 3 and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cap_reference_videos_at_3_and_warn()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = Enumerable.Range(0, 5).Select(VideoReference).ToList(),
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part(
                    "video_url",
                    "https://cdn.example.com/vid-0.mp4",
                    "reference_video"
                )
                    + Part("video_url", "https://cdn.example.com/vid-1.mp4", "reference_video")
                    + Part("video_url", "https://cdn.example.com/vid-2.mp4", "reference_video")
            )
        );
        AssertWarning(
            result.Warnings,
            "inputReferences",
            "MiniMax-H3 accepts at most 3 reference videos. Extra videos were ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should drop inputReferences when frame images are present and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_drop_inputReferences_when_frame_images_are_present_and_warn()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                FrameImages = new[] { Frame(ImageUrlFile, "first_frame") },
                InputReferences = new[] { VideoUrlFile },
                ProviderOptions = MiniMaxOptions("{\"referenceAudioUrls\":[\"" + Wav + "\"]}"),
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(ratio: null, extraContent: Part("image_url", ImageUrl, "first_frame"))
        );
        AssertWarning(
            result.Warnings,
            "inputReferences",
            "MiniMax-H3 cannot combine frame images with reference inputs. The references were ignored."
        );
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(15)]
    [UpstreamTest(
        Prefix + "should not warn for a duration of %i seconds, which is in range",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_warn_for_a_duration_of_i_seconds_which_is_in_range(int duration)
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Duration = duration });

        Assert.Equal(duration, RequestBody(handler)!["duration"]!.GetValue<int>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should clamp MiniMax-H3-Max duration to its 5-second minimum",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_clamp_MiniMax_H3_Max_duration_to_its_5_second_minimum()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { Duration = 4 }, "MiniMax-H3-Max");

        Assert.Equal(5, RequestBody(handler)!["duration"]!.GetValue<int>());
        AssertWarning(
            result.Warnings,
            "duration",
            "MiniMax-H3-Max requires at least 5 seconds. The requested duration of 4 was clamped to 5."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should attribute a video first_frame from frameImages to the frameImages feature",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_attribute_a_video_first_frame_from_frameImages_to_the_frameImages_feature()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { FrameImages = new[] { Frame(VideoUrlFile, "first_frame") } }
        );

        JsonAssert.Equal(RequestBody(handler), Body());
        AssertWarning(
            result.Warnings,
            "frameImages",
            "MiniMax-H3 does not accept a video as a frame image. The video was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep references and the aspect ratio when the only frame image is a video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_references_and_the_aspect_ratio_when_the_only_frame_image_is_a_video()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                Image = VideoUrlFile,
                AspectRatio = "16:9",
                InputReferences = new[] { ImageUrlFile },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(extraContent: Part("image_url", ImageUrl, "reference_image"))
        );
        Assert.False(HasWarning(result.Warnings, "inputReferences", "aspectRatio"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a frame image whose media type is neither image nor video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_a_frame_image_whose_media_type_is_neither_image_nor_video()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(
                        VideoModelFile.FromUrl("https://cdn.example.com/voice.mp3", "audio/mpeg"),
                        "first_frame"
                    ),
                },
            }
        );

        JsonAssert.Equal(RequestBody(handler), Body());
        AssertWarning(
            result.Warnings,
            "frameImages",
            "MiniMax-H3 only accepts an image as a frame image; the \"audio/mpeg\" file was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a last_frame whose media type is neither image nor video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_a_last_frame_whose_media_type_is_neither_image_nor_video()
    {
        var result = await Generate(
            Server(),
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(ImageUrlFile, "first_frame"),
                    Frame(
                        VideoModelFile.FromUrl(
                            "https://cdn.example.com/doc.pdf",
                            "application/pdf"
                        ),
                        "last_frame"
                    ),
                },
            }
        );

        AssertWarning(
            result.Warnings,
            "frameImages",
            "MiniMax-H3 only accepts an image as a frame image; the \"application/pdf\" last_frame was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept a frame image that has no media type",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_a_frame_image_that_has_no_media_type()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { Image = VideoModelFile.FromUrl("https://cdn.example.com/first.png") }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(ratio: null, extraContent: Part("image_url", ImageUrl, "first_frame"))
        );
        Assert.False(HasWarning(result.Warnings, "image", "frameImages"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should attribute a video standalone image to the image feature",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_attribute_a_video_standalone_image_to_the_image_feature()
    {
        var result = await Generate(Server(), new MiniMaxCall { Image = VideoUrlFile });

        AssertWarning(
            result.Warnings,
            "image",
            "MiniMax-H3 does not accept a video as a frame image. The video was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn when the last_frame is a video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_the_last_frame_is_a_video()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(ImageUrlFile, "first_frame"),
                    Frame(VideoUrlFile, "last_frame"),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler)!["content"],
            "[{\"type\":\"text\",\"text\":\""
                + PromptText
                + "\"}"
                + Part("image_url", ImageUrl, "first_frame")
                + "]"
        );
        AssertWarning(
            result.Warnings,
            "frameImages",
            "MiniMax-H3 does not accept a video as a frame image. The last_frame video was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn when a last_frame is given without a first_frame",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_a_last_frame_is_given_without_a_first_frame()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { FrameImages = new[] { Frame(ImageUrlFile, "last_frame") } }
        );

        JsonAssert.Equal(RequestBody(handler), Body());
        AssertWarning(
            result.Warnings,
            "frameImages",
            "MiniMax-H3 requires a first_frame when a last_frame is provided. The last_frame was ignored."
        );
    }
}
