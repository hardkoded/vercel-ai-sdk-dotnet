// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleImageModel.GoogleImageModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleImageModel;

/// <summary>Port of <c>google-image-model.test.ts</c> &gt; <c>GoogleImageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should reject non-Gemini model IDs before sending a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_non_Gemini_model_IDs_before_sending_a_request()
    {
        var handler = new RecordingHandler();
        var nonGeminiModel = Model(handler, "legacy-image-model");

        var error = await Assert.ThrowsAsync<AiSdkException>(() => nonGeminiModel.DoGenerateAsync(Call("A beautiful sunset"), CancellationToken.None));

        Assert.Equal("Google image models other than Gemini are no longer supported. Use a model ID that starts with `gemini-`.", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should use the language model endpoint and extract generated images", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_language_model_endpoint_and_extract_generated_images()
    {
        var handler = Response();

        var result = await Model(handler).DoGenerateAsync(Call("A beautiful sunset"), CancellationToken.None);

        Assert.Equal(TestUrl, handler.Uris[0]);
        Assert.Equal(new object?[] { new byte[] { 1, 2, 3 } }, result.Images);
        GoogleUpstream.JsonEqual(result.ProviderMetadata!.Value, """
            {
              "google": {
                "finishMessage": null,
                "finishReason": "STOP",
                "groundingMetadata": null,
                "images": [{}],
                "promptFeedback": null,
                "safetyRatings": null,
                "serviceTier": null,
                "urlContextMetadata": null,
                "usageMetadata": { "candidatesTokenCount": 100, "promptTokenCount": 10, "totalTokenCount": 110 }
              }
            }
            """);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should preserve prompt feedback when a prompt block returns no candidates", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_prompt_feedback_when_a_prompt_block_returns_no_candidates()
    {
        var handler = new RecordingHandler
        {
            ResponseText = """
                {
                  "promptFeedback": { "blockReason": "PROHIBITED_CONTENT" },
                  "usageMetadata": { "promptTokenCount": 9, "totalTokenCount": 9, "serviceTier": "standard" }
                }
                """,
        };

        var result = await Model(handler).DoGenerateAsync(Call("A blocked image prompt"), CancellationToken.None);

        Assert.Empty(result.Images);
        Assert.False(result.IsRetryable);
        var google = result.ProviderMetadata!.Value.GetProperty("google");
        GoogleUpstream.JsonEqual(google.GetProperty("promptFeedback"), """{ "blockReason": "PROHIBITED_CONTENT" }""");
        GoogleUpstream.JsonEqual(google.GetProperty("images"), "[]");
        GoogleUpstream.JsonEqual(google.GetProperty("usageMetadata"), """{ "promptTokenCount": 9, "totalTokenCount": 9, "serviceTier": "standard" }""");
        Assert.Equal("standard", google.GetProperty("serviceTier").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should expose the candidate finish reason in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_candidate_finish_reason_in_provider_metadata()
    {
        var handler = Response(images: "[]", finishReason: "IMAGE_SAFETY");

        var result = await Model(handler).DoGenerateAsync(Call("A blocked image prompt"), CancellationToken.None);

        var google = result.ProviderMetadata!.Value.GetProperty("google");
        Assert.Equal("IMAGE_SAFETY", google.GetProperty("finishReason").GetString());
        GoogleUpstream.JsonEqual(google.GetProperty("images"), "[]");
    }

    [Theory]
    [InlineData("")]
    [InlineData("BLOCK_REASON_UNSPECIFIED")]
    [InlineData("BLOCKED_REASON_UNSPECIFIED")]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should leave empty responses with default prompt block reason %j retryable", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_leave_empty_responses_with_default_prompt_block_reason_retryable(string blockReason)
    {
        var handler = new RecordingHandler
        {
            ResponseText = new JsonObject
            {
                ["candidates"] = new JsonArray(),
                ["promptFeedback"] = new JsonObject { ["blockReason"] = blockReason },
                ["usageMetadata"] = new JsonObject { ["promptTokenCount"] = 9, ["totalTokenCount"] = 9 },
            }.ToJsonString(),
        };

        var result = await Model(handler).DoGenerateAsync(Call("An image prompt with an empty response"), CancellationToken.None);

        Assert.Empty(result.Images);
        Assert.Null(result.IsRetryable);
        var feedback = result.ProviderMetadata!.Value.GetProperty("google").GetProperty("promptFeedback");
        Assert.Equal(blockReason, feedback.GetProperty("blockReason").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should send response modalities, aspect ratio, seed, and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_response_modalities_aspect_ratio_seed_and_headers()
    {
        var handler = Response();
        var modelWithHeaders = Model(handler, configure: options => options.Headers["Custom-Provider-Header"] = "provider-header-value");

        await modelWithHeaders.DoGenerateAsync(
            Call(
                "A beautiful sunset",
                aspectRatio: "21:9",
                seed: 12345,
                providerOptions: """{ "google": { "imageConfig": { "imageSize": "4K" } } }""",
                headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }),
            CancellationToken.None);

        Assert.Equal("application/json; charset=utf-8", handler.RequestHeaders["content-type"]);
        Assert.Equal("provider-header-value", handler.RequestHeaders["custom-provider-header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["custom-request-header"]);
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body), """
            {
              "contents": [{ "parts": [{ "text": "A beautiful sunset" }], "role": "user" }],
              "generationConfig": {
                "imageConfig": { "aspectRatio": "21:9", "imageSize": "4K" },
                "responseModalities": ["IMAGE"],
                "seed": 12345
              }
            }
            """);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should include usage and response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_usage_and_response_metadata()
    {
        var handler = Response(usage: """{ "promptTokenCount": 20, "candidatesTokenCount": 200, "totalTokenCount": 220 }""");
        handler.ResponseHeaders["request-id"] = "test-request-id";
        var testDate = new DateTimeOffset(2024, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var googleProvider = CreateProvider(handler);
        googleProvider.Clock = () => testDate;
        var customModel = new Vercel.AI.Google.GoogleImageModel(googleProvider, "gemini-2.5-flash-image");

        var result = await customModel.DoGenerateAsync(Call("A beautiful sunset"), CancellationToken.None);

        Assert.Equal(20, result.Usage!.InputTokens);
        Assert.Equal(200, result.Usage.OutputTokens);
        Assert.Equal(220, result.Usage.TotalTokens);
        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("gemini-2.5-flash-image", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["request-id"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should return a warning for the unsupported size option", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_a_warning_for_the_unsupported_size_option()
    {
        var handler = Response();

        var result = await Model(handler).DoGenerateAsync(Call("A beautiful sunset", size: "1024x1024"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("size", warning.Feature);
        Assert.Equal("This model does not support the `size` option. Use `aspectRatio` instead.", warning.Details);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should forward Google Search grounding and its metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_Google_Search_grounding_and_its_metadata()
    {
        var handler = Response(groundingMetadata: """
            {
              "webSearchQueries": ["example query"],
              "groundingChunks": [{ "web": { "uri": "https://example.com/source", "title": "Example" } }]
            }
            """);

        var result = await Model(handler).DoGenerateAsync(
            Call("A beautiful sunset", providerOptions: """{ "google": { "googleSearch": { "searchTypes": { "imageSearch": {} } } } }"""),
            CancellationToken.None);

        var requestBody = JsonNode.Parse(handler.Body)!;
        GoogleUpstream.JsonEqual(requestBody["tools"], """[{ "googleSearch": { "searchTypes": { "imageSearch": {} } } }]""");
        Assert.Null(requestBody["generationConfig"]!["googleSearch"]);
        GoogleUpstream.JsonEqual(result.ProviderMetadata!.Value.GetProperty("google"), """
            {
              "finishMessage": null,
              "finishReason": "STOP",
              "groundingMetadata": {
                "groundingChunks": [{ "web": { "title": "Example", "uri": "https://example.com/source" } }],
                "webSearchQueries": ["example query"]
              },
              "images": [{}],
              "promptFeedback": null,
              "safetyRatings": null,
              "serviceTier": null,
              "urlContextMetadata": null,
              "usageMetadata": { "candidatesTokenCount": 100, "promptTokenCount": 10, "totalTokenCount": 110 }
            }
            """);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should include input images for editing", Coverage = UpstreamCoverage.Partial, Note = "ImageModelFile holds bytes, not a base64 string. The test sends the bytes and checks their base64.")]
    public async Task Should_include_input_images_for_editing()
    {
        var handler = Response();

        await Model(handler).DoGenerateAsync(
            Call("Add a hat to this cat", files: new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png") }),
            CancellationToken.None);

        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body)!["contents"], """
            [{
              "role": "user",
              "parts": [
                { "text": "Add a hat to this cat" },
                { "inlineData": { "mimeType": "image/png", "data": "AQID" } }
              ]
            }]
            """);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should reject unsupported URL editing input and masks", Coverage = UpstreamCoverage.Partial, Note = "The mask is bytes, not a base64 string.")]
    public async Task Should_reject_unsupported_URL_editing_input_and_masks()
    {
        var handler = Response();
        var model = Model(handler);

        var urlError = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoGenerateAsync(
            Call("Add a hat to this cat", files: new[] { ImageModelFile.FromUrl("https://example.com/cat.png") }),
            CancellationToken.None));
        Assert.Matches("media type \"image/\\*\".*not passed as inline bytes", urlError.Message);

        var maskError = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(
            Call("Edit this image", mask: ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png")),
            CancellationToken.None));
        Assert.Equal("Gemini image models do not support mask-based image editing.", maskError.Message);
    }
}
