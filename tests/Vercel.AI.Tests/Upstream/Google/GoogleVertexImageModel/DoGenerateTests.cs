// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexImageModel.GoogleVertexImageModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexImageModel;

/// <summary>Port of <c>google-vertex-image-model.test.ts</c> &gt; <c>GoogleVertexImageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-image-model.test.ts::GoogleVertexImageModel > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should reject non-Gemini model IDs before sending a request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_non_Gemini_model_IDs_before_sending_a_request()
    {
        var handler = new RecordingHandler();
        var nonGeminiModel = Model(handler, "legacy-image-model");

        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            nonGeminiModel.DoGenerateAsync(Call("A beautiful sunset"), CancellationToken.None)
        );

        Assert.Equal(
            "Google image models other than Gemini are no longer supported. Use a model ID that starts with `gemini-`.",
            error.Message
        );
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use the language model endpoint and extract generated images",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_the_language_model_endpoint_and_extract_generated_images()
    {
        var handler = Response();

        var result = await Model(handler)
            .DoGenerateAsync(Call("A beautiful sunset"), CancellationToken.None);

        Assert.Equal(TestUrl, handler.Uris[0]);
        Assert.Equal(new object?[] { new byte[] { 1, 2, 3 } }, result.Images);
        GoogleUpstream.JsonEqual(
            result.ProviderMetadata!.Value,
            "{\"googleVertex\":{\"images\":[{}]},\"vertex\":{\"images\":[{}]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify prompt blocks as terminal",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_classify_prompt_blocks_as_terminal()
    {
        var handler = new RecordingHandler
        {
            ResponseText =
                "{\"promptFeedback\":{\"blockReason\":\"PROHIBITED_CONTENT\"},\"usageMetadata\":{\"promptTokenCount\":9,\"totalTokenCount\":9}}",
        };

        var result = await Model(handler)
            .DoGenerateAsync(Call("A blocked image prompt"), CancellationToken.None);

        Assert.Empty(result.Images);
        Assert.False(result.IsRetryable);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send response modalities, aspect ratio, seed, and headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_response_modalities_aspect_ratio_seed_and_headers()
    {
        var handler = Response();
        var modelWithHeaders = Model(handler, configure: options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );

        await modelWithHeaders.DoGenerateAsync(
            Call(
                "A beautiful sunset",
                aspectRatio: "21:9",
                seed: 12345,
                headers: new Dictionary<string, string>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                }
            ),
            CancellationToken.None
        );

        Assert.Equal("provider-header-value", handler.RequestHeaders["custom-provider-header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["custom-request-header"]);
        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            """
            {
              "contents": [{ "parts": [{ "text": "A beautiful sunset" }], "role": "user" }],
              "generationConfig": {
                "imageConfig": { "aspectRatio": "21:9" },
                "responseModalities": ["IMAGE"],
                "seed": 12345
              }
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept the legacy vertex options key and prefer googleVertex",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_the_legacy_vertex_options_key_and_prefer_googleVertex()
    {
        var handler = Response();

        await Model(handler)
            .DoGenerateAsync(
                Call(
                    "A beautiful sunset",
                    aspectRatio: "16:9",
                    providerOptions: """
                    {
                      "googleVertex": { "imageConfig": { "imageSize": "4K" } },
                      "vertex": { "imageConfig": { "imageSize": "2K" } }
                    }
                    """
                ),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body)!["generationConfig"]!["imageConfig"],
            """{ "aspectRatio": "16:9", "imageSize": "4K" }"""
        );

        var second = Response();
        await Model(second)
            .DoGenerateAsync(
                Call(
                    "A beautiful sunset",
                    providerOptions: """{ "vertex": { "imageConfig": { "imageSize": "2K" } } }"""
                ),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(second.Body)!["generationConfig"]!["imageConfig"],
            """{ "imageSize": "2K" }"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include usage and response metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_usage_and_response_metadata()
    {
        var handler = Response(
            usage: """{ "promptTokenCount": 20, "candidatesTokenCount": 200, "totalTokenCount": 220 }"""
        );
        handler.ResponseHeaders["request-id"] = "test-request-id";
        var testDate = new DateTimeOffset(2024, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var provider = CreateProvider(handler);
        provider.Clock = () => testDate;
        var customModel = new Vercel.AI.Google.GoogleVertexImageModel(
            provider,
            "gemini-2.5-flash-image"
        );

        var result = await customModel.DoGenerateAsync(
            Call("A beautiful sunset"),
            CancellationToken.None
        );

        Assert.Equal(20, result.Usage!.InputTokens);
        Assert.Equal(200, result.Usage.OutputTokens);
        Assert.Equal(220, result.Usage.TotalTokens);
        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("gemini-2.5-flash-image", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["request-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return a warning for the unsupported size option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_a_warning_for_the_unsupported_size_option()
    {
        var result = await Model(Response())
            .DoGenerateAsync(Call("A beautiful sunset", size: "1024x1024"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("size", warning.Feature);
        Assert.Equal(
            "This model does not support the `size` option. Use `aspectRatio` instead.",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include input images for editing",
        Coverage = UpstreamCoverage.Partial,
        Note = "ImageModelFile holds bytes, not an arbitrary base64 string. The test sends the bytes and checks their base64."
    )]
    public async Task Should_include_input_images_for_editing()
    {
        var handler = Response();

        await Model(handler)
            .DoGenerateAsync(
                Call(
                    "Add a hat to this cat",
                    files: new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png") }
                ),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body)!["contents"],
            """
            [{
              "role": "user",
              "parts": [
                { "text": "Add a hat to this cat" },
                { "inlineData": { "mimeType": "image/png", "data": "AQID" } }
              ]
            }]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported URL editing input and masks",
        Coverage = UpstreamCoverage.Partial,
        Note = "The mask is bytes, not a base64 string."
    )]
    public async Task Should_reject_unsupported_URL_editing_input_and_masks()
    {
        var model = Model(Response());

        var urlError = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            model.DoGenerateAsync(
                Call(
                    "Add a hat to this cat",
                    files: new[] { ImageModelFile.FromUrl("https://example.com/cat.png") }
                ),
                CancellationToken.None
            )
        );
        Assert.Matches("media type \"image/\\*\".*not passed as inline bytes", urlError.Message);

        var maskError = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoGenerateAsync(
                Call(
                    "Edit this image",
                    mask: ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png")
                ),
                CancellationToken.None
            )
        );
        Assert.Equal(
            "Gemini image models do not support mask-based image editing.",
            maskError.Message
        );
    }
}
