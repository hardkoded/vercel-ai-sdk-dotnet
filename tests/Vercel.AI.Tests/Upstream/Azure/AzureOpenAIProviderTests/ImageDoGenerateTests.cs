// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>image</c> &gt; <c>doGenerate</c>.</summary>
public sealed class ImageDoGenerateTests
{
    private const string Prompt = "A cute baby sea otter";

    // Upstream returns the literal strings base64-image-1 and base64-image-2. The port decodes the payload to bytes, so the
    // fixture holds the base64 of two short byte strings.
    private static readonly string Image1 = Convert.ToBase64String("image-1"u8.ToArray());
    private static readonly string Image2 = Convert.ToBase64String("image-2"u8.ToArray());

    private static readonly string ImageResponse =
        "{\"created\":1733837122,\"data\":[{\"revised_prompt\":\"A charming visual illustration of a baby sea otter swimming joyously.\",\"b64_json\":\""
        + Image1
        + "\"},{\"b64_json\":\""
        + Image2
        + "\"}]}";

    private static ImageCallOptions Options(
        int count = 1,
        IReadOnlyDictionary<string, JsonElement>? providerOptions = null,
        IReadOnlyDictionary<string, string?>? headers = null
    ) =>
        new(Prompt)
        {
            Count = count,
            Size = "1024x1024",
            ProviderOptions = providerOptions ?? new Dictionary<string, JsonElement>(),
            Headers = headers,
        };

    private static AzureProvider Create(
        UpstreamRecordingHandler handler,
        Action<AzureOpenAIOptions>? configure = null
    )
    {
        var options = new AzureOpenAIOptions
        {
            ApiKey = "test-api-key",
            ResourceName = "test-resource",
        };
        configure?.Invoke(options);
        return AzureProvider.Create(options, handler);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::should set the correct default api version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_the_correct_default_api_version()
    {
        var handler = new UpstreamRecordingHandler(ImageResponse);

        await Create(handler)
            .Image("dalle-deployment")
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Contains("api-version=v1", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::should set the correct modified api version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_the_correct_modified_api_version()
    {
        var handler = new UpstreamRecordingHandler(ImageResponse);

        await Create(handler, options => options.ApiVersion = "2025-04-01-preview")
            .Image("dalle-deployment")
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Contains("api-version=2025-04-01-preview", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers()
    {
        var handler = new UpstreamRecordingHandler(ImageResponse);
        var provider = Create(handler, options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );

        await provider
            .Image("dalle-deployment")
            .DoGenerateAsync(
                Options(
                    headers: new Dictionary<string, string?>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    }
                ),
                CancellationToken.None
            );

        AzureResponsesToolSupport.AssertRequestHeaders(
            handler.Headers,
            AzureResponsesToolSupport.ProviderAndRequestHeaders
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::should use the baseURL correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_the_baseURL_correctly()
    {
        var handler = new UpstreamRecordingHandler(ImageResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                BaseUrl = "https://test-resource.openai.azure.com/openai",
                ApiKey = "test-api-key",
            },
            handler
        );

        await provider.Image("dalle-deployment").DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/images/generations?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::should extract the generated images",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_the_generated_images()
    {
        var handler = new UpstreamRecordingHandler(ImageResponse);

        var result = await Create(handler)
            .Image("dalle-deployment")
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal(
            new[] { Image1, Image2 },
            result.Images.Select(image => Convert.ToBase64String(image.Data!))
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::should send the correct request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_correct_request_body()
    {
        var handler = new UpstreamRecordingHandler(ImageResponse);

        await Create(handler)
            .Image("dalle-deployment")
            .DoGenerateAsync(
                Options(
                    count: 2,
                    providerOptions: new Dictionary<string, JsonElement>
                    {
                        ["openai"] = JsonSerializer.Deserialize<JsonElement>(
                            "{\"style\":\"natural\"}"
                        ),
                    }
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            JsonNode.Parse(handler.Body),
            "{\"model\":\"dalle-deployment\",\"n\":2,\"prompt\":\"A cute baby sea otter\",\"response_format\":\"b64_json\",\"size\":\"1024x1024\",\"style\":\"natural\"}"
        );
    }
}
