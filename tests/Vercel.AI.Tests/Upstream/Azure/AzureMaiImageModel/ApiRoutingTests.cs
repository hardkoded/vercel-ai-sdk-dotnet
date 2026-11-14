// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Azure.AzureImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiImageModel;

/// <summary>Port of <c>azure-mai-image-model.test.ts</c> &gt; <c>API routing</c>.</summary>
public sealed class ApiRoutingTests
{
    private const string File = "packages/azure/src/azure-mai-image-model.test.ts::API routing::";

    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    [Theory]
    [InlineData("MAI-Image-2.6", null, "mai")]
    [InlineData("mai-image-2.6-flash", null, "mai")]
    [InlineData("MAI-Image-2.5-Pro", null, "mai")]
    [InlineData("gpt-image-1", null, "openai")]
    [InlineData("mai-image-custom", null, "openai")]
    [InlineData("MAI-Image-2.6", "openai", "openai")]
    [InlineData("my-mai-deployment", "mai", "mai")]
    [UpstreamTest(File + "routes %s with api=%s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task RoutesWithApiTo(string id, string? api, string expected)
    {
        var handler = new AzureImageHandler(() => AzureImageHandler.JsonResponse(expected == "mai" ? MaiResponse : OpenAIResponse));
        var provider = CreateProvider(handler);

        await provider.Image(id).DoGenerateAsync(CallOptions(api == null ? null : "{\"azure\":{\"api\":\"" + api + "\"}}"), CancellationToken.None);

        Assert.Equal(
            expected == "mai"
                ? GenerationsUrl
                : "https://test-resource.openai.azure.com/openai/v1/images/generations?api-version=v1",
            handler.Last.Url);
    }

    [Fact]
    [UpstreamTest(File + "advertises MAI capabilities by model ID", Coverage = UpstreamCoverage.Covered)]
    public void AdvertisesMaiCapabilitiesByModelId()
    {
        var provider = CreateProvider(new AzureImageHandler(() => AzureImageHandler.JsonResponse(MaiResponse)));
        var mai = (AzureImageModel)provider.ImageModel("MAI-Image-2.6");
        Assert.Equal(1, mai.MaxImagesPerCall);
        Assert.True(mai.SupportsFileInputs);
        Assert.False(mai.SupportsMaskInputs);

        var openai = (AzureImageModel)provider.ImageModel("my-dalle-deployment");
        Assert.Null(openai.SupportsFileInputs);
        Assert.Null(openai.SupportsMaskInputs);
    }

    [Fact]
    [UpstreamTest(File + "warns about MAI-only options on the OpenAI API", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAboutMaiOnlyOptionsOnTheOpenAIApi()
    {
        var provider = CreateProvider(new AzureImageHandler(() => AzureImageHandler.JsonResponse(OpenAIResponse)));

        var result = await provider.Image("gpt-image-1").DoGenerateAsync(CallOptions("{\"azure\":{\"webGrounding\":true}}"), CancellationToken.None);

        Assert.Contains(
            result.CallWarnings,
            warning => warning.Type == "unsupported"
                && warning.Feature == "providerOptions.azure.webGrounding"
                && warning.Message == "This option requires the MAI image API.");
    }

    [Fact]
    [UpstreamTest(File + "accepts OpenAI options under the azure key on the OpenAI API", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task AcceptsOpenAIOptionsUnderTheAzureKeyOnTheOpenAIApi()
    {
        var handler = new AzureImageHandler(() => AzureImageHandler.JsonResponse(OpenAIResponse));
        var provider = CreateProvider(handler);

        await provider.Image("gpt-image-1").DoGenerateAsync(
            CallOptions("{\"openai\":{\"quality\":\"low\",\"background\":\"opaque\"},\"azure\":{\"quality\":\"high\"}}"),
            CancellationToken.None);

        var body = handler.Last.Json();
        Assert.Equal("high", (string?)body["quality"]);
        Assert.Equal("opaque", (string?)body["background"]);
    }

    [Theory]
    [InlineData("alone", false)]
    [InlineData("overriding openai options", true)]
    [UpstreamTest(File + "accepts OpenAI image edit options under the azure key %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task AcceptsOpenAIImageEditOptionsUnderTheAzureKey(string label, bool overridesOpenAI)
    {
        Assert.NotEmpty(label);
        var handler = new AzureImageHandler(() => AzureImageHandler.JsonResponse(OpenAIResponse));
        var provider = CreateProvider(handler);
        var openai = overridesOpenAI
            ? "\"openai\":{\"inputFidelity\":\"low\",\"markup\":\"data:image/png;base64,AQID\",\"background\":\"opaque\"},"
            : string.Empty;
        var options = CallOptions("{" + openai + "\"azure\":{\"inputFidelity\":\"high\",\"markup\":\"data:image/png;base64,iVBORw==\"}}");
        options.Files = new[] { ImageModelFile.FromFile(new byte[] { 137, 80, 78, 71 }, "image/png") };

        var result = await provider.Image("gpt-image-2.6-flare").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("https://test-resource.openai.azure.com/openai/v1/images/edits?api-version=v1", handler.Last.Url);
        var parts = handler.Last.Parts();
        Assert.Equal("high", parts.Single(part => part.Name == "input_fidelity").Text);
        Assert.Equal(overridesOpenAI ? "opaque" : null, parts.SingleOrDefault(part => part.Name == "background")?.Text);
        var markup = parts.Single(part => part.Name == "markup");
        Assert.Equal("image/png", markup.ContentType);
        Assert.Equal(new byte[] { 137, 80, 78, 71 }, markup.Data);
        Assert.Empty(result.CallWarnings);
    }

    [Theory]
    [InlineData("MAI-Image-2.6")]
    [InlineData("gpt-image-1")]
    [UpstreamTest(File + "accepts unrelated azure provider options for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task AcceptsUnrelatedAzureProviderOptionsFor(string id)
    {
        var provider = CreateProvider(new AzureImageHandler(() => AzureImageHandler.JsonResponse(id.StartsWith("MAI", StringComparison.Ordinal) ? MaiResponse : OpenAIResponse)));

        var result = await provider.Image(id).DoGenerateAsync(CallOptions("{\"azure\":{\"someFutureOption\":\"value\"}}"), CancellationToken.None);

        Assert.Single(result.Images);
    }
}
