// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiImageModel;

/// <summary>Port of <c>azure-mai-image-model.test.ts</c> &gt; <c>generations</c>.</summary>
public sealed class GenerationsTests
{
    private const string File = "packages/azure/src/azure-mai-image-model.test.ts::generations::";

    private static AzureImageHandler MaiHandler()
    {
        return new AzureImageHandler(() => AzureImageHandler.JsonResponse(MaiResponse));
    }

    [Fact]
    [UpstreamTest(File + "sends width, height, and MAI options", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsWidthHeightAndMaiOptions()
    {
        var handler = MaiHandler();
        var provider = CreateProvider(handler);

        await provider.Image("MAI-Image-2.6-Flash").DoGenerateAsync(
            new ImageCallOptions("A red apple")
            {
                Size = "1536x1024",
                ProviderOptions = ProviderOptionsOf("{\"azure\":{\"autoAspectRatio\":true,\"webGrounding\":false}}"),
            },
            CancellationToken.None);

        var request = handler.Last;
        Assert.Equal(GenerationsUrl, request.Url);
        Assert.Equal("test-key", request.Headers["api-key"]);
        Assert.Contains(Vercel.AI.Operations.AiSdkVersion.Version, request.Headers["user-agent"], StringComparison.Ordinal);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"model\":\"MAI-Image-2.6-Flash\",\"prompt\":\"A red apple\",\"width\":1536,\"height\":1024,\"auto_aspect_ratio\":true,\"web_grounding\":false}"),
            request.Json()));
    }

    [Fact]
    [UpstreamTest(File + "omits dimensions when neither size nor aspect ratio is set", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsDimensionsWhenNeitherSizeNorAspectRatioIsSet()
    {
        var handler = MaiHandler();

        await CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(CallOptions(), CancellationToken.None);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"model\":\"MAI-Image-2.6\",\"prompt\":\"A red apple\"}"), handler.Last.Json()));
    }

    [Fact]
    [UpstreamTest(File + "maps aspect ratio to dimensions", Coverage = UpstreamCoverage.Covered)]
    public async Task MapsAspectRatioToDimensions()
    {
        var handler = MaiHandler();
        var options = CallOptions();
        options.AspectRatio = "16:9";

        await CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(options, CancellationToken.None);

        var body = handler.Last.Json();
        Assert.Equal(1360, (int?)body["width"]);
        Assert.Equal(768, (int?)body["height"]);
    }

    [Fact]
    [UpstreamTest(File + "prefers size over aspect ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersSizeOverAspectRatio()
    {
        var handler = MaiHandler();
        var options = CallOptions();
        options.Size = "1024x1024";
        options.AspectRatio = "16:9";

        var result = await CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(options, CancellationToken.None);

        var body = handler.Last.Json();
        Assert.Equal(1024, (int?)body["width"]);
        Assert.Equal(1024, (int?)body["height"]);
        Assert.Contains(
            result.CallWarnings,
            warning => warning.Type == "unsupported" && warning.Feature == "aspectRatio" && warning.Message == "aspectRatio is ignored when size is set.");
    }

    [Fact]
    [UpstreamTest(File + "warns about unsupported call options", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAboutUnsupportedCallOptions()
    {
        var options = CallOptions();
        options.Seed = 42;

        var result = await CreateProvider(MaiHandler()).Image("MAI-Image-2.6").DoGenerateAsync(options, CancellationToken.None);

        var warning = Assert.Single(result.CallWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("seed", warning.Feature);
        Assert.Equal("seed", warning.Message);
    }

    [Fact]
    [UpstreamTest(File + "sends one request per image when called with n > 1", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsOneRequestPerImageWhenCalledWithNGreaterThanOne()
    {
        var handler = MaiHandler();
        var options = CallOptions();
        options.Count = 2;

        var result = await CreateProvider(handler).Image("MAI-Image-2.6").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(2, result.Images.Count);
        Assert.All(result.Images, image => Assert.Equal(Convert.FromBase64String("iVBORw0KGgo="), image.Data));
        Assert.Equal(20, result.Usage!.InputTokens);
        Assert.Equal(1152, result.Usage.OutputTokens);
        Assert.Equal(1172, result.Usage.TotalTokens);
        Assert.Equal(2, result.ProviderMetadata!.Value.GetProperty("azure").GetProperty("images").GetArrayLength());
        Assert.Empty(result.CallWarnings);
    }

    [Fact]
    [UpstreamTest(File + "returns images, usage, and provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsImagesUsageAndProviderMetadata()
    {
        var result = await CreateProvider(MaiHandler()).Image("MAI-Image-2.6-Flash").DoGenerateAsync(CallOptions(), CancellationToken.None);

        Assert.Equal(Convert.FromBase64String("iVBORw0KGgo="), Assert.Single(result.Images).Data);
        Assert.Equal(10, result.Usage!.InputTokens);
        Assert.Equal(576, result.Usage.OutputTokens);
        Assert.Equal(586, result.Usage.TotalTokens);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"azure\":{\"images\":[{\"created\":1791229019,\"size\":\"768x768\",\"textTokens\":10,\"imageTokens\":0}]}}"),
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
        Assert.Equal("MAI-Image-2.6-Flash", result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(File + "tolerates responses without usage", Coverage = UpstreamCoverage.Covered)]
    public async Task ToleratesResponsesWithoutUsage()
    {
        var provider = CreateProvider(new AzureImageHandler(() => AzureImageHandler.JsonResponse(OpenAIResponse)));

        var result = await provider.Image("MAI-Image-2.6").DoGenerateAsync(CallOptions(), CancellationToken.None);

        Assert.Null(result.Usage);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"azure\":{\"images\":[{}]}}"), JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(File + "surfaces API error messages", Coverage = UpstreamCoverage.Covered)]
    public async Task SurfacesApiErrorMessages()
    {
        const string message = "Model does not support request parameter value supplied: 'width' must be at least 768 pixels.";
        var provider = CreateProvider(new AzureImageHandler(() => AzureImageHandler.JsonResponse(
            new { error = new { code = "unsupported_request_value", message } },
            HttpStatusCode.BadRequest)));
        var options = CallOptions();
        options.Size = "512x512";

        var error = await Assert.ThrowsAnyAsync<ApiException>(() => provider.Image("MAI-Image-2.6").DoGenerateAsync(options, CancellationToken.None));

        Assert.Equal(400, error.StatusCode);
        Assert.Equal(message, error.Message);
    }

    [Theory]
    [InlineData("an HTML", "<html><body>Bad Gateway</body></html>", "text/html", 502)]
    [InlineData("an empty", "", "text/plain", 500)]
    [UpstreamTest(File + "keeps the status and raw body for %s error body", Coverage = UpstreamCoverage.Covered)]
    public async Task KeepsTheStatusAndRawBodyForErrorBody(string kind, string body, string contentType, int status)
    {
        Assert.NotEmpty(kind);
        var provider = CreateProvider(new AzureImageHandler(() => new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(body, Encoding.UTF8, contentType),
        }));

        var error = await Assert.ThrowsAnyAsync<ApiException>(() => provider.Image("MAI-Image-2.6").DoGenerateAsync(CallOptions(), CancellationToken.None));

        Assert.Equal(status, error.StatusCode);
        Assert.Equal(body, error.ResponseBody ?? string.Empty);
    }

    [Fact]
    [UpstreamTest(File + "uses maiBaseURL when set", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesMaiBaseUrlWhenSet()
    {
        var handler = MaiHandler();
        var provider = CreateProvider(handler, options => options.MaiBaseUrl = "https://proxy.example.com/mai/v1/");

        await provider.Image("MAI-Image-2.6").DoGenerateAsync(CallOptions(), CancellationToken.None);

        Assert.Equal("https://proxy.example.com/mai/v1/images/generations", handler.Last.Url);
    }

    [Fact]
    [UpstreamTest(File + "authenticates with an Entra ID token provider", Coverage = UpstreamCoverage.Covered)]
    public async Task AuthenticatesWithAnEntraIdTokenProvider()
    {
        var handler = MaiHandler();
        var provider = CreateProvider(handler, options =>
        {
            options.ApiKey = null;
            options.TokenProvider = () => "entra-token";
        });

        await provider.Image("MAI-Image-2.6").DoGenerateAsync(CallOptions(), CancellationToken.None);

        Assert.Equal("Bearer entra-token", handler.Last.Headers["authorization"]);
        Assert.False(handler.Last.Headers.ContainsKey("api-key"));
    }
}
