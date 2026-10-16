// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Google;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.UnsupportedBox;

/// <summary>Port of <c>unsupported-box.test.ts</c>.</summary>
[Collection("DownloadFetch")]
public sealed class UnsupportedBoxTests
{
    private sealed class CountingHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("The provider must not be called.");
        }
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("google")]
    [InlineData("gateway")]
    [UpstreamTest("packages/ai/src/decide/unsupported-box.test.ts::rejects box questions in %s before downloading files or calling the provider", Coverage = UpstreamCoverage.Partial, Note = "The models are sealed, so the test checks that the HTTP handler is never reached instead of spying on DoDecideAsync.")]
    public async Task Rejects_box_questions_in_provider_before_downloading_files_or_calling_the_provider(string provider)
    {
        var handler = new CountingHandler();
        IDecisionCaller model = provider switch
        {
            "openai" => OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test" }, handler).DecisionModel("test"),
            "google" => GoogleProvider.Create(new GoogleOptions { ApiKey = "test" }, handler).DecisionModel("test"),
            _ => GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }, handler).DecisionModel("test"),
        };
        var downloads = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                downloads++;
                return Task.FromResult(DownloadResponse.Bytes(new byte[] { 1 }, "image/png"));
            };
            await Assert.ThrowsAnyAsync<DecisionUnsupportedQuestionTypeException>(() => DecideOperation.DecideAsync(new DecideRequest
            {
                Model = model,
                State = new List<object?> { new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = "image/png", ["data"] = new Uri("https://example.com/cat.png") } },
                Questions = new Dictionary<string, DecisionQuestion> { ["target"] = new DecisionQuestion("box", "The cat") },
            }));
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(0, handler.Calls);
        Assert.Equal(0, downloads);
    }
}
