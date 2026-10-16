// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using DownloadException = Vercel.AI.GenerateText.DownloadException;
using ImageModel = Vercel.AI.OpenAICompatible.OpenAICompatibleImageModel;

namespace Vercel.AI.Tests.Upstream.OpenAICompatible.OpenAICompatibleImageModel;

/// <summary>Port of <c>openai-compatible-image-model.test.ts</c> &gt; <c>URL image responses (%s)</c>.</summary>
public sealed class UrlImageResponsesTests
{
    private const string ImageUrl = "https://cdn.example.com/image.png";

    private static readonly byte[] ImageBytes = { 137, 80, 78, 71 };

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::downloads URL-only images using the custom fetch and abort signal", Coverage = UpstreamCoverage.Partial, Note = "HttpClient hands the handler a linked token, so the caller's token identity cannot be asserted. Cancellation reaching the download is asserted by 'propagates cancellation during download'. See #498.")]
    public async Task Downloads_url_only_images_using_the_custom_fetch_and_abort_signal(string endpoint)
    {
        var (model, fetch, options) = Setup(endpoint, "{\"b64_json\":null,\"url\":\"" + ImageUrl + "\"}");
        using var abort = new CancellationTokenSource();

        var result = await model.DoGenerateAsync(options, abort.Token);

        Assert.Equal(new[] { ImageBytes }, result.Images.Select(image => image.Data));
        Assert.Equal(2, fetch.Requests.Count);
        Assert.Equal("https://api.example.com/images/" + endpoint, fetch.Requests[0].Uri);
        Assert.Equal(ImageUrl, fetch.Requests[1].Uri);
        Assert.Equal("GET", fetch.Requests[1].Method);
        Assert.False(fetch.Requests[1].Headers.ContainsKey("Authorization"));
        Assert.False(fetch.Requests[1].Headers.ContainsKey("x-api-key"));
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::preserves order in mixed responses and prefers base64 when both are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_order_in_mixed_responses_and_prefers_base64_when_both_are_present(string endpoint)
    {
        var (model, fetch, options) = Setup(
            endpoint,
            "{\"url\":\"" + ImageUrl + "\"}",
            "{\"b64_json\":\"AQID\",\"url\":\"https://cdn.example.com/unused.png\"}",
            "{\"b64_json\":\"BAUG\",\"url\":null}");

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            new[] { ImageBytes, new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 } },
            result.Images.Select(image => image.Data));
        Assert.Equal(2, fetch.Requests.Count);
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::preserves base64 without downloading when both fields are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_base64_without_downloading_when_both_fields_are_present(string endpoint)
    {
        var (model, fetch, options) = Setup(endpoint, "{\"b64_json\":\"AQID\",\"url\":\"" + ImageUrl + "\"}");

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("AQID", result.Images.Single().Data is { } data ? Convert.ToBase64String(data) : null);
        Assert.Single(fetch.Requests);
    }

    [Theory]
    [InlineData("generations", "{}")]
    [InlineData("generations", "{\"b64_json\":null,\"url\":null}")]
    [InlineData("edits", "{}")]
    [InlineData("edits", "{\"b64_json\":null,\"url\":null}")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::rejects responses with neither base64 nor a URL: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_responses_with_neither_base64_nor_a_url(string endpoint, string image)
    {
        var (model, fetch, options) = Setup(endpoint, image);

        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoGenerateAsync(options, CancellationToken.None));

        Assert.Equal("Expected b64_json or url in the image response", error.Message);
        Assert.Single(fetch.Requests);
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::propagates image download failures", Coverage = UpstreamCoverage.Partial, Note = "Asserts the 404 status. The download error carries no url. See #498.")]
    public async Task Propagates_image_download_failures(string endpoint)
    {
        var (model, fetch, options) = Setup(endpoint, "{\"url\":\"" + ImageUrl + "\"}");
        fetch.Once(() => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Not found") });

        var error = await Assert.ThrowsAsync<NotFoundException>(() => model.DoGenerateAsync(options, CancellationToken.None));

        Assert.Equal(404, error.StatusCode);
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::propagates cancellation during download", Coverage = UpstreamCoverage.Covered)]
    public async Task Propagates_cancellation_during_download(string endpoint)
    {
        var (model, fetch, options) = Setup(endpoint, "{\"url\":\"" + ImageUrl + "\"}");
        using var abort = new CancellationTokenSource();
        fetch.OnRequest = (request, token) =>
        {
            if (request.RequestUri!.AbsoluteUri == ImageUrl)
            {
                abort.Cancel();
                token.ThrowIfCancellationRequested();
            }
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoGenerateAsync(options, abort.Token));
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::rejects private download URLs before fetching them", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_private_download_urls_before_fetching_them(string endpoint)
    {
        var (model, fetch, options) = Setup(endpoint, "{\"url\":\"http://127.0.0.1/image.png\"}");

        await Assert.ThrowsAsync<DownloadException>(() => model.DoGenerateAsync(options, CancellationToken.None));

        Assert.Single(fetch.Requests);
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::rejects redirects to private download URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_redirects_to_private_download_urls(string endpoint)
    {
        var (model, fetch, options) = Setup(endpoint, "{\"url\":\"" + ImageUrl + "\"}");
        fetch.Once(() => Redirect("http://127.0.0.1/image.png"));

        await Assert.ThrowsAsync<DownloadException>(() => model.DoGenerateAsync(options, CancellationToken.None));

        Assert.Equal(2, fetch.Requests.Count);
    }

    [Theory]
    [InlineData("generations")]
    [InlineData("edits")]
    [UpstreamTest("packages/openai-compatible/src/image/openai-compatible-image-model.test.ts::URL image responses (%s)::supports authenticated self-hosted downloads and strips credentials on cross-origin redirects", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_authenticated_self_hosted_downloads_and_strips_credentials_on_cross_origin_redirects(string endpoint)
    {
        const string baseUrl = "http://127.0.0.1:8000";
        var (model, fetch, options) = Setup(endpoint, new[] { "{\"url\":\"" + baseUrl + "/image.png\"}" }, baseUrl);
        fetch.Once(() => Redirect(ImageUrl));

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(new[] { ImageBytes }, result.Images.Select(image => image.Data));
        var sameOrigin = fetch.Requests[1].Headers;
        Assert.Equal("Bearer test-key", sameOrigin["Authorization"]);
        Assert.Equal("custom-key", sameOrigin["x-api-key"]);
        var redirected = fetch.Requests[2].Headers;
        Assert.False(redirected.ContainsKey("Authorization"));
        Assert.False(redirected.ContainsKey("x-api-key"));
    }

    private static HttpResponseMessage Redirect(string location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Found);
        response.Headers.TryAddWithoutValidation("location", location);
        return response;
    }

    private static (ImageModel Model, Fetch Fetch, ImageCallOptions Options) Setup(string endpoint, params string[] images)
    {
        return Setup(endpoint, images, "https://api.example.com");
    }

    private static (ImageModel Model, Fetch Fetch, ImageCallOptions Options) Setup(string endpoint, string[] images, string baseUrl)
    {
        var fetch = new Fetch();
        fetch.Once(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"data\":[" + string.Join(",", images) + "]}", Encoding.UTF8, "application/json"),
        });
        var options = new OpenAICompatibleOptions
        {
            ProviderName = "test",
            BaseUrl = baseUrl,
            ApiKey = "test-key",
            SupportsImages = true,
            DownloadHttpClient = new HttpClient(fetch, disposeHandler: false),
        };
        var model = new ImageModel(OpenAICompatibleProvider.Create(options, fetch), "test-model");
        var call = new ImageCallOptions("A photorealistic astronaut riding a horse")
        {
            Headers = new Dictionary<string, string?> { ["x-api-key"] = "custom-key" },
        };
        if (endpoint == "edits")
        {
            call.Files = new[] { new OpenAICompatibleImageFile("image/png", ImageBytes, null, "image.png") };
        }

        return (model, fetch, call);
    }

    private sealed class Fetch : HttpMessageHandler
    {
        private readonly Queue<Func<HttpResponseMessage>> _once = new();

        public List<Seen> Requests { get; } = new();

        public Action<HttpRequestMessage, CancellationToken>? OnRequest { get; set; }

        public void Once(Func<HttpResponseMessage> response)
        {
            _once.Enqueue(response);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }

            Requests.Add(new Seen(request.RequestUri!.AbsoluteUri, request.Method.Method, headers));
            OnRequest?.Invoke(request, cancellationToken);
            return Task.FromResult(_once.Count > 0 ? _once.Dequeue()() : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(ImageBytes) });
        }
    }

    private sealed record Seen(string Uri, string Method, Dictionary<string, string> Headers);
}
