// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.DeepInfra;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class DeepInfraUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should pass the correct parameters including aspect ratio and seed", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_send_aspect_ratio_seed_and_count()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        var model = Image(capture, "stabilityai/sdxl");
        model.Seed = 3;
        await model.DoGenerateAsync(new ImageCallOptions("a cat") { AspectRatio = "16:9", Count = 2 }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("a cat", body["prompt"]!.GetValue<string>());
        Assert.Equal("16:9", body["aspect_ratio"]!.GetValue<string>());
        Assert.Equal(2, body["num_images"]!.GetValue<int>());
        Assert.Equal(3, body["seed"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should call the correct url", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_requests_use_the_inference_route()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        await Image(capture, "stabilityai/sdxl").DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None);
        Assert.Equal("https://api.deepinfra.com/v1/inference/stabilityai/sdxl", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-image-model.test.ts::DeepInfraImageModel > doGenerate::should handle size parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Size_is_sent_as_width_and_height()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        await Image(capture, "stabilityai/sdxl").DoGenerateAsync(new ImageCallOptions("a cat") { Size = "1024x768" }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("1024", body["width"]!.GetValue<string>());
        Assert.Equal("768", body["height"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > chatModel::should set supportsStructuredOutputs so response_format json_schema is forwarded", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_structured_outputs_send_json_schema()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\"}");
        options.ProviderOptions = UpstreamChat.Bag("deepinfra", "{\"responseFormat\":\"json\"}");
        var model = (OpenAICompatibleLanguageModel)DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("m");
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_schema", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal("api.deepinfra.com", capture.Requests[0].Uri!.Host);
    }

    [Fact]
    [UpstreamTest("packages/deepinfra/src/deepinfra-provider.test.ts::DeepInfraProvider > image::should respect custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task A_custom_base_url_appends_inference()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"images\":[]}" };
        var provider = DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret", BaseUrl = "https://custom.api.deepinfra.com" }, capture);
        await ((DeepInfraImageModel)provider.ImageModel("deepinfra-image-model")).DoGenerateAsync(new ImageCallOptions("draw"), CancellationToken.None);
        Assert.Equal("https://custom.api.deepinfra.com/inference/deepinfra-image-model", capture.Requests[0].Uri!.AbsoluteUri);
    }

    private static DeepInfraImageModel Image(UpstreamCapture capture, string modelId)
    {
        return (DeepInfraImageModel)DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel(modelId);
    }
}
