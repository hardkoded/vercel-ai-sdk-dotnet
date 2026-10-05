// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for <c>generateImage</c>.</summary>
[Collection("WarningLog")]
public sealed class GenerateImageTests
{
    private const string Prefix = "packages/ai/src/generate-image/generate-image.test.ts::";
    private const string Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAACklEQVR4nGMAAQAABQABDQottAAAAABJRU5ErkJggg==";
    private const string Jpeg = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAb/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCdABmX/9k=";
    private const string Gif = "R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs=";

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should send args to doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_arguments_to_the_image_model()
    {
        using var source = new CancellationTokenSource();
        var model = new ImageFake { Images = new object?[] { Png } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = model,
            Prompt = new ImagePrompt("sunny day at the beach", new object?[] { Png }, Png),
            Size = "1024x1024",
            AspectRatio = "16:9",
            Seed = 12345,
            ProviderOptions = OperationJson.Parse("{\"mock-provider\":{\"style\":\"vivid\"}}"),
            Headers = new Dictionary<string, string> { ["custom-request-header"] = "request-header-value" },
            CancellationToken = source.Token,
        });
        var call = model.Calls[0];
        Assert.Equal(1, call.N);
        Assert.Equal("sunny day at the beach", call.Prompt);
        Assert.Equal("file", call.Mask!.Type);
        Assert.Equal("image/png", call.Mask.MediaType);
        Assert.Equal(PngBytes(), call.Files![0].Data);
        Assert.Equal("1024x1024", call.Size);
        Assert.Equal("16:9", call.AspectRatio);
        Assert.Equal((int?)12345, call.Seed);
        Assert.Equal("vivid", call.ProviderOptions!.Value.GetProperty("mock-provider").GetProperty("style").GetString());
        Assert.Equal("request-header-value", call.Headers["custom-request-header"]);
        Assert.Equal("ai/0.0.0-test", call.Headers["user-agent"]);
        Assert.Equal(source.Token, call.CancellationToken);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_warnings()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Warnings = new[] { OperationWarning.Other("Setting is not supported") } });
        Assert.Equal("Setting is not supported", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should call logWarnings with the correct warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_the_image_warnings()
    {
        var seen = Watch();
        try
        {
            await Generate(new ImageFake
            {
                Images = new object?[] { Png },
                Warnings = new[] { OperationWarning.Other("Setting is not supported"), OperationWarning.Unsupported("size", "Size parameter not supported") },
            });
            Assert.Equal(2, seen[0].Warnings.Count);
            Assert.Equal("size", seen[0].Warnings[1].Feature);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should call logWarnings with aggregated warnings from multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_aggregated_image_warnings()
    {
        var seen = Watch();
        try
        {
            var turn = 0;
            var model = new ImageFake { MaxImagesPerCall = 1, Next = () => new ImageModelResult(new object?[] { Png }, new[] { OperationWarning.Other((++turn).ToString()) }) };
            await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
            Assert.Equal(new[] { "1", "2" }, seen[0].Warnings.Select(item => item.Message));
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should call logWarnings with empty array when no warnings are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_an_empty_warning_list()
    {
        var seen = Watch();
        try
        {
            await Generate(new ImageFake { Images = new object?[] { Png } });
            Assert.Empty(seen[0].Warnings);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > base64 image data::should return generated images with correct mime types", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_images_with_detected_media_types()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png, Jpeg, Gif } });
        Assert.Equal(new[] { "image/png", "image/jpeg", "image/gif" }, result.Images.Select(item => item.MediaType));
        Assert.Equal(Png, result.Images[0].Base64);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > base64 image data::should return the first image with correct mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_first_image()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png, Jpeg } });
        Assert.Equal("image/png", result.Image.MediaType);
        Assert.Equal(PngBytes(), result.Image.Data);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > uint8array image data::should return generated images", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_binary_images()
    {
        var bytes = PngBytes();
        var result = await Generate(new ImageFake { Images = new object?[] { bytes } });
        Assert.Equal(bytes, result.Image.Data);
        Assert.Equal("image/png", result.Image.MediaType);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > when several calls are required::should generate images", Coverage = UpstreamCoverage.Covered)]
    public async Task Generates_images_across_calls()
    {
        var images = new object?[] { Png, Jpeg, Gif };
        var turn = 0;
        var model = new ImageFake { MaxImagesPerCall = 1, Next = () => new ImageModelResult(new[] { images[turn++] }) };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 3 });
        Assert.Equal(3, model.Calls.Count);
        Assert.Equal(new[] { 1, 1, 1 }, model.Calls.Select(item => item.N));
        Assert.Equal(3, result.Images.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > when several calls are required::should aggregate warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Aggregates_warnings_across_calls()
    {
        var turn = 0;
        var model = new ImageFake { MaxImagesPerCall = 1, Next = () => new ImageModelResult(new object?[] { Png }, new[] { OperationWarning.Other("w" + turn++) }) };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
        Assert.Equal(new[] { "w0", "w1" }, result.Warnings.Select(item => item.Message));
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > when several calls are required::should generate with maxImagesPerCall = %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_functional_per_call_limit()
    {
        foreach (var label in new[] { "sync method", "async method" })
        {
            var model = new ImageFake { FunctionalMax = 2, Images = new object?[] { Png, Jpeg } };
            var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 4 });
            Assert.Equal(label.Length > 0, model.Calls.Count == 2);
            Assert.Equal(new[] { 2, 2 }, model.Calls.Select(item => item.N));
            Assert.Equal(4, result.Images.Count);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > error handling::should retry when no images are returned and a later attempt succeeds", Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_an_empty_image_result()
    {
        var turn = 0;
        var model = new ImageFake { Next = () => turn++ == 0 ? new ImageModelResult(Array.Empty<object?>()) : new ImageModelResult(new object?[] { Png }) };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", MaxRetries = 2 });
        Assert.Equal(2, model.Calls.Count);
        Assert.Equal(Png, result.Image.Base64);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > error handling::should not retry provider-classified terminal empty results", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_retry_a_terminal_empty_result()
    {
        var model = new ImageFake { Next = () => new ImageModelResult(Array.Empty<object?>(), isRetryable: false) };
        await Assert.ThrowsAsync<NoImageGeneratedException>(() => GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", MaxRetries = 2 }));
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > error handling::should throw NoImageGeneratedError after no-image retries are exhausted", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_after_empty_retries_are_exhausted()
    {
        var model = new ImageFake { Next = () => new ImageModelResult(Array.Empty<object?>()) };
        var error = await Assert.ThrowsAsync<NoImageGeneratedException>(() => GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", MaxRetries = 2 }));
        Assert.Equal("No image generated.", error.Message);
        Assert.Equal(3, model.Calls.Count);
        Assert.Equal(3, error.Calls.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > error handling::should throw NoImageGeneratedError when no images are returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_no_images_are_returned()
    {
        var error = await Assert.ThrowsAsync<NoImageGeneratedException>(() => Generate(new ImageFake { Images = Array.Empty<object?>(), MaxRetries = 0 }));
        Assert.Equal("No image generated.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > error handling::should preserve per-call diagnostics when no images are returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_per_call_diagnostics()
    {
        var model = new ImageFake { Next = () => new ImageModelResult(Array.Empty<object?>(), new[] { OperationWarning.Other("empty") }, response: new ProviderResponse(id: "call")) };
        var error = await Assert.ThrowsAsync<NoImageGeneratedException>(() => GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", MaxRetries = 0 }));
        Assert.Equal("empty", error.Calls[0].Warnings[0].Message);
        Assert.Equal("call", error.Calls[0].Response.Id);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > error handling::should include response headers in error when no images generated", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_headers_when_no_image_is_generated()
    {
        var model = new ImageFake { Next = () => new ImageModelResult(Array.Empty<object?>(), response: new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "req" })) };
        var error = await Assert.ThrowsAsync<NoImageGeneratedException>(() => GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", MaxRetries = 0 }));
        Assert.Equal("req", error.Responses[0].Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_response_metadata()
    {
        var timestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var model = new ImageFake { Next = () => new ImageModelResult(new object?[] { Png }, response: new ProviderResponse(id: "response", timestamp: timestamp, modelId: "image-model")) };
        var result = await Generate(model);
        Assert.Equal("response", result.Responses[0].Id);
        Assert.Equal((DateTime?)timestamp, result.Responses[0].Timestamp);
        Assert.Equal("image-model", result.Responses[0].ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should return provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_provider_metadata()
    {
        var model = new ImageFake { Next = () => new ImageModelResult(new object?[] { Png }, providerMetadata: OperationJson.Parse("{\"testProvider\":{\"images\":[{\"revisedPrompt\":\"beach\"}]}}")) };
        var result = await Generate(model);
        Assert.Equal("beach", result.ProviderMetadata.GetProperty("testProvider").GetProperty("images")[0].GetProperty("revisedPrompt").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should preserve underlying calls and attach per-image provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Attaches_per_image_provider_metadata()
    {
        var metadata = OperationJson.Parse("{\"testProvider\":{\"images\":[{\"id\":\"one\"},{\"id\":\"two\"}]}}");
        var result = await Generate(new ImageFake { Images = new object?[] { Png, Jpeg }, Metadata = metadata });
        Assert.Equal("one", result.Images[0].ProviderMetadata!.Value.GetProperty("testProvider").GetProperty("id").GetString());
        Assert.Equal("two", result.Images[1].ProviderMetadata!.Value.GetProperty("testProvider").GetProperty("id").GetString());
        Assert.Single(result.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should expose empty usage when provider does not report usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_usage_empty_when_the_provider_omits_it()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png } });
        Assert.Null(result.Usage.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage::should aggregate usage across multiple provider calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Aggregates_usage_across_calls()
    {
        var turn = 0;
        var model = new ImageFake
        {
            MaxImagesPerCall = 1,
            Next = () => new ImageModelResult(new object?[] { Png }, usage: new OperationUsage(inputTokens: 10, outputTokens: turn++ == 0 ? 1 : 2, totalTokens: turn == 1 ? 11 : 12)),
        };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
        Assert.Equal((int?)20, result.Usage.InputTokens);
        Assert.Equal((int?)3, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should merge provider metadata from multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_provider_metadata_from_multiple_calls()
    {
        var turn = 0;
        var model = new ImageFake
        {
            MaxImagesPerCall = 1,
            Next = () => new ImageModelResult(new object?[] { Png }, providerMetadata: OperationJson.Parse("{\"openai\":{\"images\":[{\"id\":" + turn++ + "}]}}")),
        };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
        Assert.Equal(2, result.ProviderMetadata.GetProperty("openai").GetProperty("images").GetArrayLength());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should sum Gateway costs across multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Sums_gateway_costs()
    {
        var payloads = new[]
        {
            "{\"gateway\":{\"images\":[],\"routing\":{\"provider\":\"test1\"},\"cost\":\"0.01\",\"gatewayCost\":\"0.002\",\"inferenceCost\":\"0.003\",\"inputInferenceCost\":\"0.004\",\"marketCost\":\"0.02\",\"outputInferenceCost\":\"0.005\",\"surchargeCost\":\"0.006\"}}",
            "{\"gateway\":{\"images\":[],\"routing\":{\"provider\":\"test2\"},\"cost\":\"0.02\",\"gatewayCost\":\"0.020\",\"inferenceCost\":\"0.030\",\"inputInferenceCost\":\"0.040\",\"marketCost\":\"0.04\",\"outputInferenceCost\":\"0.050\",\"surchargeCost\":\"0.060\",\"generationId\":\"gen-123\"}}",
        };
        var turn = 0;
        var model = new ImageFake { MaxImagesPerCall = 1, Next = () => new ImageModelResult(new object?[] { turn == 0 ? Png : Jpeg }, providerMetadata: OperationJson.Parse(payloads[turn++])) };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
        var gateway = result.ProviderMetadata.GetProperty("gateway");
        Assert.Equal("0.03", gateway.GetProperty("cost").GetString());
        Assert.Equal("0.022", gateway.GetProperty("gatewayCost").GetString());
        Assert.Equal("0.033", gateway.GetProperty("inferenceCost").GetString());
        Assert.Equal("0.044", gateway.GetProperty("inputInferenceCost").GetString());
        Assert.Equal("0.06", gateway.GetProperty("marketCost").GetString());
        Assert.Equal("0.055", gateway.GetProperty("outputInferenceCost").GetString());
        Assert.Equal("0.066", gateway.GetProperty("surchargeCost").GetString());
        Assert.Equal("gen-123", gateway.GetProperty("generationId").GetString());
        Assert.Equal("test2", gateway.GetProperty("routing").GetProperty("provider").GetString());
        Assert.False(gateway.TryGetProperty("images", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should drop empty images array for gateway provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Drops_an_empty_gateway_images_array()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"gateway\":{\"images\":[],\"routing\":{\"provider\":\"vertex\"},\"cost\":\"0.04\"}}") });
        Assert.False(result.ProviderMetadata.GetProperty("gateway").TryGetProperty("images", out _));
        Assert.Equal("0.04", result.ProviderMetadata.GetProperty("gateway").GetProperty("cost").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should not drop empty images array for non-gateway providers", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_an_empty_images_array_for_other_providers()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"openai\":{\"images\":[],\"usage\":{\"tokens\":100}}}") });
        var openai = result.ProviderMetadata.GetProperty("openai");
        Assert.Equal(0, openai.GetProperty("images").GetArrayLength());
        Assert.False(openai.TryGetProperty("usage", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should handle provider metadata without images field", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_gateway_metadata_without_an_images_field()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"gateway\":{\"routing\":{\"provider\":\"vertex\"},\"cost\":\"0.04\"}}") });
        Assert.Equal("vertex", result.ProviderMetadata.GetProperty("gateway").GetProperty("routing").GetProperty("provider").GetString());
        Assert.False(result.ProviderMetadata.GetProperty("gateway").TryGetProperty("images", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should handle undefined providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_omitted_provider_metadata()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png } });
        Assert.Equal(JsonValueKind.Object, result.ProviderMetadata.ValueKind);
        Assert.Empty(result.ProviderMetadata.EnumerateObject());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should merge multiple providers from same call", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_multiple_providers_from_one_call()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"openai\":{\"images\":[{\"id\":\"a\"}]},\"gateway\":{\"cost\":\"0.01\"}}") });
        Assert.Equal("a", result.ProviderMetadata.GetProperty("openai").GetProperty("images")[0].GetProperty("id").GetString());
        Assert.Equal("0.01", result.ProviderMetadata.GetProperty("gateway").GetProperty("cost").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should merge multiple providers across multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_multiple_providers_across_calls()
    {
        var turn = 0;
        var model = new ImageFake
        {
            MaxImagesPerCall = 1,
            Next = () => new ImageModelResult(new object?[] { Png }, providerMetadata: OperationJson.Parse(turn++ == 0
                ? "{\"openai\":{\"images\":[{\"id\":\"a\"}]},\"gateway\":{\"cost\":\"0.01\"}}"
                : "{\"openai\":{\"images\":[{\"id\":\"b\"}]},\"gateway\":{\"cost\":\"0.02\"}}")),
        };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
        Assert.Equal(2, result.ProviderMetadata.GetProperty("openai").GetProperty("images").GetArrayLength());
        Assert.Equal("0.03", result.ProviderMetadata.GetProperty("gateway").GetProperty("cost").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should preserve null values in images array", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_null_image_metadata_entries()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"openai\":{\"images\":[null]}}") });
        Assert.Equal(JsonValueKind.Null, result.ProviderMetadata.GetProperty("openai").GetProperty("images")[0].ValueKind);
        Assert.Null(result.Image.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should handle complex nested metadata structures", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_nested_gateway_metadata()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"gateway\":{\"routing\":{\"provider\":\"vertex\",\"fallbacks\":[\"a\"]},\"cost\":\"1.5\"}}") });
        Assert.Equal("a", result.ProviderMetadata.GetProperty("gateway").GetProperty("routing").GetProperty("fallbacks")[0].GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should handle empty gateway images across multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Drops_empty_gateway_images_across_calls()
    {
        var model = new ImageFake { MaxImagesPerCall = 1, Metadata = OperationJson.Parse("{\"gateway\":{\"images\":[],\"cost\":\"0.01\"}}"), Images = new object?[] { Png } };
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny", N = 2 });
        Assert.False(result.ProviderMetadata.GetProperty("gateway").TryGetProperty("images", out _));
        Assert.Equal("0.02", result.ProviderMetadata.GetProperty("gateway").GetProperty("cost").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "generateImage > provider metadata merging::should keep images array for gateway if non-empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_a_non_empty_gateway_images_array()
    {
        var result = await Generate(new ImageFake { Images = new object?[] { Png }, Metadata = OperationJson.Parse("{\"gateway\":{\"images\":[{\"id\":\"kept\"}],\"cost\":\"0.01\"}}") });
        Assert.Equal("kept", result.ProviderMetadata.GetProperty("gateway").GetProperty("images")[0].GetProperty("id").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "data URL handling::should handle data URL with media type in prompt images", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_png_data_url()
    {
        var model = new ImageFake { Images = new object?[] { Png } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Prompt = new ImagePrompt("edit", new object?[] { "data:image/png;base64," + Png }) });
        Assert.Equal("image/png", model.Calls[0].Files![0].MediaType);
        Assert.Equal(PngBytes(), model.Calls[0].Files![0].Data);
    }

    [Fact]
    [UpstreamTest(Prefix + "data URL handling::should handle data URL with jpeg media type", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_jpeg_data_url()
    {
        var model = new ImageFake { Images = new object?[] { Jpeg } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Prompt = new ImagePrompt("edit", new object?[] { "data:image/jpeg;base64," + Jpeg }) });
        Assert.Equal("image/jpeg", model.Calls[0].Files![0].MediaType);
    }

    [Fact]
    [UpstreamTest(Prefix + "data URL handling::should handle data URL as mask", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_data_url_mask()
    {
        var model = new ImageFake { Images = new object?[] { Png } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Prompt = new ImagePrompt("edit", new object?[] { Png }, "data:image/png;base64," + Png) });
        Assert.Equal("image/png", model.Calls[0].Mask!.MediaType);
        Assert.Equal(PngBytes(), model.Calls[0].Mask!.Data);
    }

    [Fact]
    [UpstreamTest(Prefix + "data URL handling::should detect media type from data when data URL has no media type", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_the_media_type_when_the_data_url_omits_it()
    {
        var model = new ImageFake { Images = new object?[] { Png } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Prompt = new ImagePrompt("edit", new object?[] { "data:;base64," + Png }) });
        Assert.Equal("image/png", model.Calls[0].Files![0].MediaType);
    }

    [Fact]
    [UpstreamTest(Prefix + "data URL handling::should handle multiple data URLs in prompt images", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_multiple_data_urls()
    {
        var model = new ImageFake { Images = new object?[] { Png } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Prompt = new ImagePrompt("edit", new object?[] { "data:image/png;base64," + Png, "data:image/gif;base64," + Gif }) });
        Assert.Equal(new[] { "image/png", "image/gif" }, model.Calls[0].Files!.Select(item => item.MediaType));
    }

    [Fact]
    [UpstreamTest(Prefix + "data URL handling::should handle mix of data URLs and base64 strings", Coverage = UpstreamCoverage.Covered)]
    public async Task Mixes_data_urls_and_base64_strings()
    {
        var model = new ImageFake { Images = new object?[] { Png } };
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Prompt = new ImagePrompt("edit", new object?[] { "data:image/png;base64," + Png, Jpeg, "https://example.com/cat.png" }) });
        Assert.Equal("file", model.Calls[0].Files![0].Type);
        Assert.Equal("image/jpeg", model.Calls[0].Files![1].MediaType);
        Assert.Equal("url", model.Calls[0].Files![2].Type);
        Assert.Equal("https://example.com/cat.png", model.Calls[0].Files![2].Url);
    }

    private static Task<GenerateImageResult> Generate(ImageFake model)
    {
        return GenerateImage.GenerateImageAsync(new GenerateImageRequest { Model = model, Text = "sunny day at the beach", MaxRetries = model.MaxRetries });
    }

    private static List<WarningLogContext> Watch()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        return seen;
    }

    private static byte[] PngBytes()
    {
        return Convert.FromBase64String(Png);
    }

    private sealed class ImageFake : IImageCaller
    {
        public string Provider => "test-provider";

        public string ModelId => "test-model";

        public int? MaxImagesPerCall { get; set; }

        public int? FunctionalMax { get; set; }

        public int? MaxRetries { get; set; }

        public object?[] Images { get; set; } = new object?[] { "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAACklEQVR4nGMAAQAABQABDQottAAAAABJRU5ErkJggg==" };

        public IReadOnlyList<OperationWarning>? Warnings { get; set; }

        public JsonElement? Metadata { get; set; }

        public Func<ImageModelResult>? Next { get; set; }

        public List<ImageModelCall> Calls { get; } = new List<ImageModelCall>();

        public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(FunctionalMax ?? MaxImagesPerCall);
        }

        public Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
        {
            Calls.Add(call);
            if (Next != null)
            {
                return Task.FromResult(Next());
            }

            return Task.FromResult(new ImageModelResult(Images, Warnings, providerMetadata: Metadata));
        }
    }
}
