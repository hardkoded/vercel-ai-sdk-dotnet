// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; doGenerate &gt; frameImages and inputReferences encoding</c>.</summary>
public sealed class GatewayVideoModelDoGenerateFrameImagesAndInputReferencesEncodingTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > doGenerate > frameImages and inputReferences encoding::";

    [Fact]
    [UpstreamTest(
        Prefix + "should encode frameImages with Uint8Array data to base64",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_frameImages_with_Uint8Array_data_to_base64()
    {
        var handler = PrepareJsonResponse();
        var binaryData = new byte[] { 72, 101, 108, 108, 111 };

        await DoGenerate(
            CreateTestModel(handler),
            Call(
                prompt: "Animate between frames",
                frameImages: new[]
                {
                    new VideoFrameFile(
                        VideoModelFile.FromFile(binaryData, "image/png"),
                        "first_frame"
                    ),
                    new VideoFrameFile(
                        VideoModelFile.FromUrl("https://example.com/last-frame.png"),
                        "last_frame"
                    ),
                }
            )
        );

        Assert.True(
            Equal(
                "[{\"frameType\":\"first_frame\",\"image\":{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\"}},{\"frameType\":\"last_frame\",\"image\":{\"type\":\"url\",\"url\":\"https://example.com/last-frame.png\"}}]",
                Body(handler)!["frameImages"]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should encode inputReferences with Uint8Array data to base64",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_inputReferences_with_Uint8Array_data_to_base64()
    {
        var handler = PrepareJsonResponse();
        var binaryData = new byte[] { 72, 101, 108, 108, 111 };

        await DoGenerate(
            CreateTestModel(handler),
            Call(
                prompt: "Generate from references",
                inputReferences: new[]
                {
                    VideoModelFile.FromFile(binaryData, "image/png"),
                    VideoModelFile.FromUrl("https://example.com/reference.png"),
                }
            )
        );

        Assert.True(
            Equal(
                "[{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"SGVsbG8=\"},{\"type\":\"url\",\"url\":\"https://example.com/reference.png\"}]",
                Body(handler)!["inputReferences"]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit frameImages and inputReferences when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_frameImages_and_inputReferences_when_not_provided()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), Call(prompt: "Text to video"));

        var requestBody = Body(handler)!.AsObject();
        Assert.False(requestBody.ContainsKey("frameImages"));
        Assert.False(requestBody.ContainsKey("inputReferences"));
    }
}
