// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.ByteDance;
using Vercel.AI.Fal;
using Vercel.AI.Luma;
using Vercel.AI.OpenAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Prodia;
using Vercel.AI.Provider;
using Vercel.AI.QuiverAI;
using Vercel.AI.Replicate;

namespace Vercel.AI.Tests;

/// <summary>
/// The one-argument <c>DoGenerateAsync(ImageCallOptions)</c> of each image model must pass the call options on to the provider request
/// and report the warnings of the call.
/// </summary>
public sealed class ImageCallOptionsForwardingTests
{
    private const string ImageUrl = "https://api.example.com/image.png";

    private static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("test-binary-content");

    [Fact]
    public async Task OpenAI_sends_the_call_headers_and_reports_the_seed_warning()
    {
        var handler = new Scripted(_ => Json("{\"data\":[{\"b64_json\":\"dGVzdA==\"}]}"));
        var model = new OpenAIImageModel(OpenAIProvider.Create(new OpenAIOptions { ApiKey = "k" }, handler), "dall-e-3");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        AssertRequest(handler, "/images/generations");
        Assert.Single(result.Images);
        Assert.Contains(result.CallWarnings, warning => warning.Feature == "seed");
    }

    [Fact]
    public async Task Fal_sends_the_call_headers_and_the_seed()
    {
        var handler = new Scripted(call => call.Method == HttpMethod.Post
            ? Json("{\"images\":[{\"url\":\"" + ImageUrl + "\",\"width\":1,\"height\":1,\"content_type\":\"image/png\"}]}")
            : Bytes(ImageBytes, "image/png"));
        var model = FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("fal-ai/flux/dev");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        AssertRequest(handler, "/fal-ai/flux/dev");
        Assert.Contains("\"seed\":4", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.Single(result.Images);
    }

    [Fact]
    public async Task ByteDance_sends_the_call_headers_and_reports_the_mask_and_seed_warnings()
    {
        var handler = new Scripted(_ => Json("{\"data\":[{\"b64_json\":\"dGVzdA==\"}]}"));
        var model = ByteDanceProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("seedream-4-0-250828");
        var call = Call();
        call.Mask = ImageModelFileFor();

        var result = await model.DoGenerateAsync(call, CancellationToken.None);

        AssertRequest(handler, "/images/generations");
        Assert.Single(result.Images);
        Assert.Contains(result.CallWarnings, warning => warning.Feature == "seed");
        Assert.Contains(result.CallWarnings, warning => warning.Feature == "mask");
    }

    [Fact]
    public async Task QuiverAI_sends_the_call_headers_and_reports_the_seed_warning()
    {
        var handler = new Scripted(_ => Json("{\"id\":\"svg-1\",\"created\":1713374400,\"data\":[{\"svg\":\"<svg/>\",\"mime_type\":\"image/svg+xml\"}],\"usage\":{\"total_tokens\":3,\"input_tokens\":1,\"output_tokens\":2}}"));
        var model = QuiverAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("arrow-1");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        AssertRequest(handler, "/svgs/generations");
        Assert.Single(result.Images);
        Assert.Contains(result.CallWarnings, warning => warning.Feature == "seed");
    }

    [Fact]
    public async Task Prodia_sends_the_call_headers_and_the_seed()
    {
        const string job = "{\"id\":\"job-1\",\"state\":{\"current\":\"completed\"},\"config\":{\"prompt\":\"p\"}}";
        var handler = new Scripted(_ => Multipart(job));
        var model = ProdiaProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("inference.flux.schnell.txt2img.v2");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        AssertRequest(handler, "/job");
        Assert.Contains("\"seed\":4", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.Single(result.Images);
    }

    [Fact]
    public async Task Replicate_sends_the_call_headers()
    {
        var handler = new Scripted(call => call.RequestUri!.AbsoluteUri == ImageUrl
            ? Bytes(ImageBytes, "image/webp")
            : Json("{\"id\":\"p1\",\"status\":\"processing\",\"output\":[\"" + ImageUrl + "\"],\"error\":null,\"urls\":{\"get\":\"https://api.replicate.com/v1/predictions/p1\"}}"));
        var model = ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("black-forest-labs/flux-schnell");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        AssertRequest(handler, "/predictions");
        Assert.Contains("\"seed\":4", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.Single(result.Images);
    }

    [Fact]
    public async Task Luma_sends_the_call_headers_and_reports_the_seed_warning()
    {
        const string completed = "{\"id\":\"g1\",\"generation_type\":\"image\",\"state\":\"completed\",\"created_at\":\"2024-01-01T00:00:00Z\",\"assets\":{\"image\":\"" + ImageUrl + "\"},\"model\":\"photon-1\",\"request\":{\"generation_type\":\"image\",\"model\":\"photon-1\",\"prompt\":\"p\"}}";
        var handler = new Scripted(call => call.RequestUri!.AbsoluteUri == ImageUrl
            ? Bytes(ImageBytes, "image/png")
            : Json(completed.Replace("completed", call.Method == HttpMethod.Post ? "queued" : "completed", StringComparison.Ordinal)));
        var model = LumaProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("photon-1");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        AssertRequest(handler, "/image");
        Assert.Single(result.Images);
        Assert.Contains(result.CallWarnings, warning => warning.Feature == "seed");
    }

    private static ImageCallOptions Call()
    {
        return new ImageCallOptions("p") { Seed = 4, Headers = new Dictionary<string, string?> { ["X-Call"] = "a" } };
    }

    private static Operations.ImageModelFile ImageModelFileFor()
    {
        return Operations.ImageModelFile.FromFile(new byte[] { 255, 255, 255, 0 }, "image/png");
    }

    private static void AssertRequest(Scripted handler, string pathSuffix)
    {
        var request = handler.Requests[0];
        Assert.Equal("a", request.Headers["X-Call"]);
        Assert.EndsWith(pathSuffix, request.Uri.AbsolutePath, StringComparison.Ordinal);
    }

    private static HttpResponseMessage Json(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    private static HttpResponseMessage Bytes(byte[] body, string mediaType)
    {
        var content = new ByteArrayContent(body);
        content.Headers.TryAddWithoutValidation("Content-Type", mediaType);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static HttpResponseMessage Multipart(string job)
    {
        const string boundary = "test-boundary-12345";
        var body = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"job\"; filename=\"job.json\"\r\nContent-Type: application/json\r\n\r\n" + job + "\r\n"
            + "--" + boundary + "\r\nContent-Disposition: form-data; name=\"output\"; filename=\"output.png\"\r\nContent-Type: image/png\r\n\r\ntest-image\r\n"
            + "--" + boundary + "--\r\n";
        return Bytes(Encoding.UTF8.GetBytes(body), "multipart/form-data; boundary=" + boundary);
    }

    /// <summary>Answers each request from the script and records what was sent.</summary>
    private sealed class Scripted(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(Uri Uri, Dictionary<string, string> Headers, string Body)> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            var headers = request.Headers.ToDictionary(pair => pair.Key, pair => string.Join(",", pair.Value), StringComparer.OrdinalIgnoreCase);
            if (request.RequestUri!.AbsoluteUri != ImageUrl)
            {
                Requests.Add((request.RequestUri, headers, body));
            }

            return respond(request);
        }
    }
}
