// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexDownloadAuth;

/// <summary>Port of <c>google-vertex-download-auth.test.ts</c> &gt; <c>%s fetch</c>.</summary>
[Collection("DownloadFetch")]
public sealed class FetchTests
{
    private const string Commit = "2ebc78b65435926718879f85a1a85687b053c542";

    private const string Response = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"done\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}";

    [Theory]
    [InlineData("custom", "generate")]
    [InlineData("custom", "stream")]
    [InlineData("global", "generate")]
    [InlineData("global", "stream")]
    [UpstreamTest("packages/google-vertex/src/google-vertex-download-auth.test.ts::%s fetch::keeps the Express Mode API key off tool-result downloads during %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task KeepsTheExpressModeApiKeyOffToolResultDownloadsDuringMode(string transport, string mode)
    {
        var downloads = new List<(string Url, DownloadRequest Request)>();
        DownloadFetch download = (url, request) =>
        {
            downloads.Add((url, request));
            if (url == "https://files.example.com/image.png")
            {
                var redirect = new DownloadResponse(302, "Found", null, null);
                redirect.Headers.Add("location", "https://cdn.example.com/image.png");
                return Task.FromResult(redirect);
            }

            return Task.FromResult(DownloadResponse.Bytes(new byte[] { 0x89, 0x50, 0x4e, 0x47 }, "image/png"));
        };
        var handler = new RecordingHandler
        {
            ResponseText = mode == "generate" ? Response : "data: " + Response + "\n\n",
            MediaType = mode == "generate" ? "application/json" : "text/event-stream",
        };
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = transport == "global"
                ? download
                : (url, request) => throw new InvalidOperationException("Expected the custom fetch transport");
            var provider = GoogleVertexProvider.Create(new VertexOptions
            {
                ApiKey = "vertex-api-key",
                UpstreamRoutes = true,
                DownloadFetch = transport == "custom" ? download : null,
            }, handler);
            var model = provider.LanguageModel("gemini-3.7-flash");
            var options = new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new ToolModelMessage("tool-call-id", "get-image", new UserContentPart[]
                    {
                        new FileContentPart("image/png", "https://files.example.com/image.png", null, null),
                    }),
                },
            };

            if (mode == "generate")
            {
                await model.DoGenerateAsync(options, CancellationToken.None);
            }
            else
            {
                var result = await model.DoStreamAsync(options, CancellationToken.None);
                await GoogleUpstream.Collect(result.Stream);
            }
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(new[] { "https://files.example.com/image.png", "https://cdn.example.com/image.png" }, downloads.Select(d => d.Url));
        foreach (var (_, request) in downloads)
        {
            Assert.Null(request.Headers.Get("x-goog-api-key"));
            Assert.Null(request.Headers.Get("authorization"));
            Assert.Equal("manual", request.Redirect);
        }

        Assert.Single(handler.Uris);
        Assert.Equal(
            "https://aiplatform.googleapis.com/v1/publishers/google/models/gemini-3.7-flash:" + (mode == "generate" ? "generateContent" : "streamGenerateContent?alt=sse"),
            handler.Uris[0]);
        Assert.Equal("vertex-api-key", handler.RequestHeaders["x-goog-api-key"]);
    }
}
