// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Replicate;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class ReplicateParityTests
{
    private const string ImagePrefix = "packages/replicate/src/replicate-image-model.test.ts::doGenerate::";
    private const string EditingPrefix = "packages/replicate/src/replicate-image-model.test.ts::doGenerate > Image Editing::";
    private const string Flux2Prefix = "packages/replicate/src/replicate-image-model.test.ts::doGenerate > Flux-2 Models::";
    private const string ProviderPrefix = "packages/replicate/src/replicate-provider.test.ts::createReplicate::";
    private const string Prompt = "The Loch Ness monster getting a manicure";
    private const string OutputUrl = "https://replicate.delivery/xezq/abc/out-0.webp";
    private const string PollUrl = "https://api.replicate.com/v1/predictions/pending-prediction";
    private static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("test-binary-content");

    [Fact]
    [UpstreamTest(ImagePrefix + "should pass the model and the settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Body_has_the_input_settings_and_replicate_options()
    {
        var handler = Server();
        await Generate(handler, Call(size: "1024x768", aspectRatio: "3:4", seed: 123, providerOptions: "{\"replicate\":{\"style\":\"realistic_image\"},\"other\":{\"something\":\"else\"}}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"prompt\":\"The Loch Ness monster getting a manicure\",\"num_outputs\":1,\"aspect_ratio\":\"3:4\",\"size\":\"1024x768\",\"seed\":123,\"style\":\"realistic_image\"}}");
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should call the correct url", Coverage = UpstreamCoverage.Covered)]
    public async Task Unversioned_models_post_to_the_model_predictions_route()
    {
        var handler = Server();
        await Generate(handler, Call());
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal("https://api.replicate.com/v1/models/black-forest-labs/flux-schnell/predictions", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should pass headers and set the prefer header", Coverage = UpstreamCoverage.Covered)]
    public async Task Request_sends_bearer_custom_headers_and_prefer_wait()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-token" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = ReplicateProvider.Create(options, handler).ImageModel("black-forest-labs/flux-schnell");
        await ((ReplicateImageModel)model).DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-token", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Equal("wait", call.Header("Prefer"));
        Assert.Contains("ai-sdk/replicate/" + AiSdkVersion.Version, call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should set custom wait time in prefer header when maxWaitTimeInSeconds is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Max_wait_time_sets_the_prefer_wait_duration()
    {
        var handler = Server();
        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"maxWaitTimeInSeconds\":120}}"));
        Assert.Equal("wait=120", handler.Calls[0].Header("Prefer"));
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should not include maxWaitTimeInSeconds in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Call_settings_are_not_sent_as_input()
    {
        var handler = Server();
        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"maxWaitTimeInSeconds\":120,\"pollIntervalMillis\":10,\"maxPollAttempts\":100,\"guidance_scale\":7.5}}"));
        var input = JsonNode.Parse(handler.Calls[0].Text)!["input"]!.AsObject();
        Assert.False(input.ContainsKey("maxWaitTimeInSeconds"));
        Assert.False(input.ContainsKey("pollIntervalMillis"));
        Assert.False(input.ContainsKey("maxPollAttempts"));
        Assert.Equal(7.5, input["guidance_scale"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should extract the generated image from array response", Coverage = UpstreamCoverage.Covered)]
    public async Task Array_output_is_downloaded()
    {
        var handler = Server();
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal(OutputUrl, handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should extract the generated image from string response", Coverage = UpstreamCoverage.Covered)]
    public async Task String_output_is_downloaded()
    {
        var handler = Server(Prediction("\"" + OutputUrl + "\""));
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal(OutputUrl, handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should poll until output is available when the sync wait expires", Coverage = UpstreamCoverage.Covered)]
    public async Task Pending_predictions_are_polled_with_credentials()
    {
        var predictions = 0;
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.AbsoluteUri == OutputUrl)
            {
                return ParityHandler.Bytes(ImageBytes, "image/webp");
            }

            var number = predictions++;
            var status = number == 0 ? "starting" : number == 1 ? "processing" : "succeeded";
            return ParityHandler.Json(Pending(status, number < 2 ? "null" : "[\"" + OutputUrl + "\"]", PollUrl));
        });
        var result = await Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":100}}"));
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        Assert.Equal(new[] { HttpMethod.Post, HttpMethod.Get, HttpMethod.Get, HttpMethod.Get }, handler.Calls.Select(call => call.Method));
        Assert.Equal(PollUrl, handler.Calls[1].Uri.AbsoluteUri);
        Assert.Equal("Bearer test-api-token", handler.Calls[1].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should not send credentials to a polling URL on a different origin", Coverage = UpstreamCoverage.Covered)]
    public async Task Polls_on_another_origin_omit_credentials()
    {
        var otherOrigin = "https://example.com/predictions/pending-prediction";
        var handler = new ParityHandler(call => call.Uri.AbsoluteUri switch
        {
            OutputUrl => ParityHandler.Bytes(ImageBytes, "image/webp"),
            "https://example.com/predictions/pending-prediction" => ParityHandler.Json(Pending("succeeded", "[\"" + OutputUrl + "\"]", otherOrigin)),
            _ => ParityHandler.Json(Pending("starting", "null", otherOrigin)),
        });
        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":100}}"));
        Assert.Equal(otherOrigin, handler.Calls[1].Uri.AbsoluteUri);
        Assert.Null(handler.Calls[1].Header("Authorization"));
    }

    [Theory]
    [InlineData("failed")]
    [InlineData("canceled")]
    [UpstreamTest(ImagePrefix + "should throw when polling returns %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Failed_or_canceled_polls_throw(string status)
    {
        var predictions = 0;
        var handler = new ParityHandler(_ =>
        {
            var first = predictions++ == 0;
            return ParityHandler.Json("{\"id\":\"pending-prediction\",\"status\":\"" + (first ? "starting" : status) + "\",\"output\":null,\"error\":" + (first ? "null" : "\"Prediction did not complete\"") + ",\"urls\":{\"get\":\"" + PollUrl + "\"}}");
        });
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":100}}")));
        Assert.Equal("Replicate image generation " + status + ": Prediction did not complete", error.Message);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should throw when a succeeded prediction has no output", Coverage = UpstreamCoverage.Covered)]
    public async Task A_succeeded_prediction_without_output_throws()
    {
        var handler = Server(Pending("succeeded", "null", "https://api.replicate.com/v1/predictions/completed-prediction"));
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Generate(handler, Call()));
        Assert.Equal("Replicate image generation completed without output.", error.Message);
        Assert.Single(handler.Calls);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should stop when the maximum polling attempts are reached", Coverage = UpstreamCoverage.Covered)]
    public async Task Polling_stops_after_the_maximum_attempts()
    {
        var handler = Server(Pending("processing", "null", PollUrl));
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":2}}")));
        Assert.Equal("Replicate image generation did not complete after 2 polling attempts.", error.Message);
        Assert.Equal(3, handler.Calls.Count);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_timestamp_model_id_and_headers()
    {
        var date = new DateTime(2024, 1, 1);
        var result = await Generate(Server(), Call(), () => date, "https://api.replicate.com");
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("black-forest-labs/flux-schnell", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should include response headers in metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_headers_come_from_the_prediction_response()
    {
        var date = new DateTime(2024, 1, 1);
        var handler = new ParityHandler(call => call.Uri.AbsoluteUri == OutputUrl
            ? ParityHandler.Bytes(ImageBytes, "image/webp")
            : ParityHandler.Json(Prediction("[\"" + OutputUrl + "\"]"), ("custom-response-header", "response-header-value")));
        var result = await Generate(handler, Call(), () => date, "https://api.replicate.com");
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("black-forest-labs/flux-schnell", result.Response.ModelId);
        Assert.Equal("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal("response-header-value", result.Response.Headers["custom-response-header"]);
    }

    [Fact]
    [UpstreamTest(ImagePrefix + "should set version in request body for versioned models", Coverage = UpstreamCoverage.Covered)]
    public async Task Versioned_models_post_the_version_to_predictions()
    {
        var handler = Server();
        await Generate(handler, Call(), modelId: "bytedance/sdxl-lightning-4step:5599ed30703defd1d160a25a63321b4dec97101d98b4674bcc56e41f62f35637");
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal("https://api.replicate.com/v1/predictions", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"prompt\":\"The Loch Ness monster getting a manicure\",\"num_outputs\":1},\"version\":\"5599ed30703defd1d160a25a63321b4dec97101d98b4674bcc56e41f62f35637\"}");
    }

    [Fact]
    [UpstreamTest(EditingPrefix + "should send image when URL file is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_url_file_is_sent_as_image()
    {
        var handler = Server();
        await Generate(handler, Call("Add a hat to the person", files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") }));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"image\":\"https://example.com/input.jpg\",\"num_outputs\":1,\"prompt\":\"Add a hat to the person\"}}");
    }

    [Fact]
    [UpstreamTest(EditingPrefix + "should convert Uint8Array file to data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task A_byte_file_is_sent_as_a_data_uri()
    {
        var handler = Server();
        await Generate(handler, Call("Transform this image", files: new[] { ImageModelFile.FromFile(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, "image/png") }));
        var input = JsonNode.Parse(handler.Calls[0].Text)!["input"]!;
        Assert.StartsWith("data:image/png;base64,", input["image"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.Equal("Transform this image", input["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(EditingPrefix + "should send mask for inpainting", Coverage = UpstreamCoverage.Covered)]
    public async Task A_mask_is_sent()
    {
        var handler = Server();
        await Generate(handler, Call("Replace the masked area with a tree", files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") }, mask: ImageModelFile.FromUrl("https://example.com/mask.png")));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"image\":\"https://example.com/input.jpg\",\"mask\":\"https://example.com/mask.png\",\"num_outputs\":1,\"prompt\":\"Replace the masked area with a tree\"}}");
    }

    [Fact]
    [UpstreamTest(EditingPrefix + "should warn when multiple files are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Extra_files_warn_and_only_the_first_is_sent()
    {
        var handler = Server();
        var result = await Generate(handler, Call("Edit multiple images", files: new[] { ImageModelFile.FromUrl("https://example.com/input1.jpg"), ImageModelFile.FromUrl("https://example.com/input2.jpg") }));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("This Replicate model only supports a single input image. Additional images are ignored.", warning.Message);
        Assert.Equal("https://example.com/input1.jpg", JsonNode.Parse(handler.Calls[0].Text)!["input"]!["image"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(EditingPrefix + "should pass provider options with image editing", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_are_sent_with_an_edit()
    {
        var handler = Server();
        await Generate(handler, Call(
            "Inpaint this area",
            files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") },
            mask: ImageModelFile.FromUrl("https://example.com/mask.png"),
            providerOptions: "{\"replicate\":{\"guidance_scale\":7.5,\"num_inference_steps\":30,\"negative_prompt\":\"blurry, low quality\"}}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"guidance_scale\":7.5,\"image\":\"https://example.com/input.jpg\",\"mask\":\"https://example.com/mask.png\",\"negative_prompt\":\"blurry, low quality\",\"num_inference_steps\":30,\"num_outputs\":1,\"prompt\":\"Inpaint this area\"}}");
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should report maxImagesPerCall as 8 for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public void Flux2_models_accept_eight_images_per_call()
    {
        Assert.Equal(8, Model(Server(), "black-forest-labs/flux-2-pro").MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should report maxImagesPerCall as 1 for non-Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public void Other_models_return_one_image_per_call()
    {
        Assert.Equal(1, Model(Server(), "black-forest-labs/flux-schnell").MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should send single image as input_image for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_sends_one_file_as_input_image()
    {
        var handler = Server();
        await Generate(handler, Call("Generate image in similar style", files: new[] { ImageModelFile.FromUrl("https://example.com/reference.jpg") }), modelId: "black-forest-labs/flux-2-pro");
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"input_image\":\"https://example.com/reference.jpg\",\"num_outputs\":1,\"prompt\":\"Generate image in similar style\"}}");
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should send multiple images as input_image, input_image_2, etc. for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_numbers_extra_input_images()
    {
        var handler = Server();
        var files = new[] { "reference1", "reference2", "reference3" }.Select(name => ImageModelFile.FromUrl("https://example.com/" + name + ".jpg")).ToList();
        await Generate(handler, Call("Combine styles from reference images", files: files), modelId: "black-forest-labs/flux-2-pro");
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"input_image\":\"https://example.com/reference1.jpg\",\"input_image_2\":\"https://example.com/reference2.jpg\",\"input_image_3\":\"https://example.com/reference3.jpg\",\"num_outputs\":1,\"prompt\":\"Combine styles from reference images\"}}");
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should warn when more than 8 images are provided for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_warns_and_drops_images_past_eight()
    {
        var handler = Server();
        var files = Enumerable.Range(1, 9).Select(i => ImageModelFile.FromUrl("https://example.com/img" + i + ".jpg")).ToList();
        var result = await Generate(handler, Call("Too many images", files: files), modelId: "black-forest-labs/flux-2-pro");
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("Flux-2 models support up to 8 input images. Additional images are ignored.", warning.Message);
        var input = JsonNode.Parse(handler.Calls[0].Text)!["input"]!.AsObject();
        Assert.Equal("https://example.com/img1.jpg", input["input_image"]!.GetValue<string>());
        Assert.Equal("https://example.com/img8.jpg", input["input_image_8"]!.GetValue<string>());
        Assert.False(input.ContainsKey("input_image_9"));
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should warn and ignore mask for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_warns_and_drops_the_mask()
    {
        var handler = Server();
        var result = await Generate(handler, Call("Edit with mask", files: new[] { ImageModelFile.FromUrl("https://example.com/input.jpg") }, mask: ImageModelFile.FromUrl("https://example.com/mask.png")), modelId: "black-forest-labs/flux-2-pro");
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("Flux-2 models do not support mask input. The mask will be ignored.", warning.Message);
        Assert.False(JsonNode.Parse(handler.Calls[0].Text)!["input"]!.AsObject().ContainsKey("mask"));
    }

    [Fact]
    [UpstreamTest(Flux2Prefix + "should call correct URL for Flux-2 models", Coverage = UpstreamCoverage.Covered)]
    public async Task Flux2_posts_to_the_model_predictions_route()
    {
        var handler = Server();
        await Generate(handler, Call("Generate something"), modelId: "black-forest-labs/flux-2-pro");
        Assert.Equal("https://api.replicate.com/v1/models/black-forest-labs/flux-2-pro/predictions", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(ProviderPrefix + "creates a provider with required settings", Coverage = UpstreamCoverage.Covered)]
    public void Provider_with_a_token_creates_image_models()
    {
        Assert.NotNull(ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token" }).ImageModel("black-forest-labs/flux-schnell"));
    }

    [Fact]
    [UpstreamTest(ProviderPrefix + "creates a provider with custom settings", Coverage = UpstreamCoverage.Covered)]
    public void Provider_with_a_custom_base_url_creates_image_models()
    {
        var provider = ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token", BaseUrl = "https://custom.replicate.com" });
        Assert.NotNull(provider.ImageModel("black-forest-labs/flux-schnell"));
        Assert.Equal("https://custom.replicate.com", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest(ProviderPrefix + "rejects an empty baseURL during provider creation", Coverage = UpstreamCoverage.Covered)]
    public void Provider_rejects_an_empty_base_url()
    {
        var error = Assert.Throws<ArgumentException>(() => ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token", BaseUrl = string.Empty }));
        Assert.Equal("baseURL", error.ParamName);
        Assert.StartsWith("baseURL must be a non-empty string.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ProviderPrefix + "creates an image model instance", Coverage = UpstreamCoverage.Covered)]
    public void Provider_image_models_are_replicate_image_models()
    {
        Assert.IsType<ReplicateImageModel>(ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token" }).ImageModel("black-forest-labs/flux-schnell"));
    }

    private static string Prediction(string output)
    {
        return "{\"id\":\"s7x1e3dcmhrmc0cm8rbatcneec\",\"model\":\"black-forest-labs/flux-schnell\",\"version\":\"dp-4d0bcc010b3049749a251855f12800be\",\"input\":{\"num_outputs\":1,\"prompt\":\"The Loch Ness Monster getting a manicure\"},"
            + "\"logs\":\"\",\"output\":" + output + ",\"data_removed\":false,\"error\":null,\"status\":\"processing\",\"created_at\":\"2025-01-08T13:24:38.692Z\","
            + "\"urls\":{\"cancel\":\"https://api.replicate.com/v1/predictions/s7x1e3dcmhrmc0cm8rbatcneec/cancel\",\"get\":\"https://api.replicate.com/v1/predictions/s7x1e3dcmhrmc0cm8rbatcneec\",\"stream\":\"https://stream.replicate.com/v1/files/bcwr-3okdfv3o2wehstv5f2okyftwxy57hhypqsi6osiim5iaq5k7u24a\"}}";
    }

    private static string Pending(string status, string output, string getUrl)
    {
        return "{\"id\":\"pending-prediction\",\"status\":\"" + status + "\",\"output\":" + output + ",\"error\":null,\"urls\":{\"get\":\"" + getUrl + "\"}}";
    }

    private static ParityHandler Server(string? prediction = null)
    {
        var body = prediction ?? Prediction("[\"" + OutputUrl + "\"]");
        return new ParityHandler(call => call.Uri.AbsoluteUri == OutputUrl ? ParityHandler.Bytes(ImageBytes, "image/webp") : ParityHandler.Json(body));
    }

    private static ReplicateImageModel Model(ParityHandler handler, string modelId, Func<DateTime>? clock = null, string? baseUrl = null)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-token" };
        if (baseUrl != null)
        {
            options.BaseUrl = baseUrl;
        }

        return new ReplicateImageModel(ReplicateProvider.Create(options, handler), modelId, clock);
    }

    private static Task<ImageModelResult> Generate(ParityHandler handler, ImageModelCall call, Func<DateTime>? clock = null, string? baseUrl = null, string modelId = "black-forest-labs/flux-schnell")
    {
        return Model(handler, modelId, clock, baseUrl).DoGenerateAsync(call, CancellationToken.None);
    }

    private static ImageModelCall Call(string prompt = Prompt, string? size = null, string? aspectRatio = null, int? seed = null, string? providerOptions = null, IReadOnlyList<ImageModelFile>? files = null, ImageModelFile? mask = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        using var options = JsonDocument.Parse(providerOptions ?? "{}");
        return new ImageModelCall(prompt, files, mask, 1, size, aspectRatio, seed, options.RootElement.Clone(), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }
}
