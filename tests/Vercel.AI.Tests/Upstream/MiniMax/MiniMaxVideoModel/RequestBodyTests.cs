// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; request body</c>.</summary>
public sealed class RequestBodyTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > request body::";

    [Theory]
    [InlineData("MiniMax-H3-Max", "480P")]
    [InlineData("MiniMax-H3", "768P")]
    [UpstreamTest(
        Prefix + "should send the %s resolution %s using MiniMax casing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_model_resolution_using_MiniMax_casing(
        string modelId,
        string resolution
    )
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall
            {
                ProviderOptions = MiniMaxOptions("{\"resolution\":\"" + resolution + "\"}"),
            },
            modelId
        );

        JsonAssert.Equal(RequestBody(handler), Body(modelId, resolution));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit reference inputs that MiniMax-H3-Max does not support",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_reference_inputs_that_MiniMax_H3_Max_does_not_support()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[] { ImageUrlFile, VideoUrlFile },
                ProviderOptions = MiniMaxOptions(
                    "{\"resolution\":\"480P\",\"referenceAudioUrls\":[\"https://cdn.example.com/ref.wav\"]}"
                ),
            },
            "MiniMax-H3-Max"
        );

        JsonAssert.Equal(RequestBody(handler), Body("MiniMax-H3-Max", "480P"));
        AssertWarning(
            result.Warnings,
            "inputReferences",
            "MiniMax-H3-Max does not support reference-to-video inputs. The references were ignored."
        );
        AssertWarning(
            result.Warnings,
            "referenceAudioUrls",
            "MiniMax-H3-Max does not support reference audio. The audio was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map aigcWatermark to aigc_watermark",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_aigcWatermark_to_aigc_watermark()
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall { ProviderOptions = MiniMaxOptions("{\"aigcWatermark\":true}") }
        );

        JsonAssert.Equal(RequestBody(handler), Body(extra: ",\"aigc_watermark\":true"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send aigc_watermark when it is explicitly disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_aigc_watermark_when_it_is_explicitly_disabled()
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall { ProviderOptions = MiniMaxOptions("{\"aigcWatermark\":false}") }
        );

        Assert.False(RequestBody(handler)!["aigc_watermark"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit aigc_watermark when it is unset",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_aigc_watermark_when_it_is_unset()
    {
        var handler = Server();

        await Generate(handler, new MiniMaxCall());

        Assert.False(RequestBody(handler)!.AsObject().ContainsKey("aigc_watermark"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer providerOptions.minimax.ratio over the top-level aspectRatio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_providerOptions_minimax_ratio_over_the_top_level_aspectRatio()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                AspectRatio = "16:9",
                ProviderOptions = MiniMaxOptions("{\"ratio\":\"9:16\"}"),
            }
        );

        JsonAssert.Equal(RequestBody(handler), Body(ratio: "9:16"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should support the adaptive ratio on reference-to-video, which has no top-level equivalent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_the_adaptive_ratio_on_reference_to_video_which_has_no_top_level_equivalent()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[] { ImageUrlFile },
                ProviderOptions = MiniMaxOptions("{\"ratio\":\"adaptive\"}"),
            }
        );

        Assert.Equal("adaptive", RequestBody(handler)!["ratio"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and omit ratio for an unsupported aspectRatio on reference-to-video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_omit_ratio_for_an_unsupported_aspectRatio_on_reference_to_video()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { InputReferences = new[] { ImageUrlFile }, AspectRatio = "5:3" }
        );

        Assert.False(RequestBody(handler)!.AsObject().ContainsKey("ratio"));
        AssertWarning(
            result.Warnings,
            "aspectRatio",
            "MiniMax-H3 does not support the aspect ratio \"5:3\". Using the provider default (adaptive)."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should warn and fall back to the default ratio for an unsupported aspectRatio on text-to-video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_fall_back_to_the_default_ratio_for_an_unsupported_aspectRatio_on_text_to_video()
    {
        var handler = Server();

        var result = await Generate(handler, new MiniMaxCall { AspectRatio = "5:3" });

        Assert.Equal("16:9", RequestBody(handler)!["ratio"]!.GetValue<string>());
        AssertWarning(
            result.Warnings,
            "aspectRatio",
            "MiniMax-H3 does not support the aspect ratio \"5:3\". Using the default (16:9)."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should warn and fall back to the default ratio when adaptive is requested for text-to-video",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_and_fall_back_to_the_default_ratio_when_adaptive_is_requested_for_text_to_video()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall { ProviderOptions = MiniMaxOptions("{\"ratio\":\"adaptive\"}") }
        );

        Assert.Equal("16:9", RequestBody(handler)!["ratio"]!.GetValue<string>());
        AssertWarning(
            result.Warnings,
            "aspectRatio",
            "MiniMax-H3 text-to-video does not support the adaptive aspect ratio. Using the default (16:9)."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should encode an inline Uint8Array frame image as a data URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_an_inline_Uint8Array_frame_image_as_a_data_URI()
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(
                        VideoModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
                        "first_frame"
                    ),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part("image_url", "data:image/png;base64,AQID", "first_frame")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass an inline base64 string frame image through as a data URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_an_inline_base64_string_frame_image_through_as_a_data_URI()
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall
            {
                FrameImages = new[]
                {
                    Frame(VideoModelFile.FromBase64("AQID", "image/jpeg"), "first_frame"),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler)!["content"],
            "[{\"type\":\"text\",\"text\":\""
                + PromptText
                + "\"}"
                + Part("image_url", "data:image/jpeg;base64,AQID", "first_frame")
                + "]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should encode an inline Uint8Array reference video as a data URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_an_inline_Uint8Array_reference_video_as_a_data_URI()
    {
        var handler = Server();

        var result = await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[]
                {
                    VideoModelFile.FromFile(new byte[] { 4, 5, 6 }, "video/mp4"),
                },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler),
            Body(
                ratio: null,
                extraContent: Part("video_url", "data:video/mp4;base64,BAUG", "reference_video")
            )
        );
        // Data URIs are not echoed back through resolvedInputs.
        JsonAssert.Equal(
            Resolve(result.ProviderMetadata, "resolvedInputs"),
            "{\"imageCount\":0,\"referenceVideoUrls\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass an inline base64 string reference video through as a data URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_an_inline_base64_string_reference_video_through_as_a_data_URI()
    {
        var handler = Server();

        await Generate(
            handler,
            new MiniMaxCall
            {
                InputReferences = new[] { VideoModelFile.FromBase64("BAUG", "video/webm") },
            }
        );

        JsonAssert.Equal(
            RequestBody(handler)!["content"],
            "[{\"type\":\"text\",\"text\":\""
                + PromptText
                + "\"}"
                + Part("video_url", "data:video/webm;base64,BAUG", "reference_video")
                + "]"
        );
    }
}
