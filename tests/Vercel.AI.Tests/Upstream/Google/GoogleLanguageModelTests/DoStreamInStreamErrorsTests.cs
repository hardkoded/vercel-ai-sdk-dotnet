// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>in-stream errors</c>.</summary>
public sealed class DoStreamInStreamErrorsTests
{
    [Theory]
    [InlineData(false, 503, "UNAVAILABLE", true)]
    [InlineData(true, 503, "UNAVAILABLE", true)]
    [InlineData(false, 400, "INVALID_ARGUMENT", false)]
    [InlineData(false, null, "UNKNOWN", false)]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > in-stream errors::should surface $status and finish with error (partial text: $partialText)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_surface_status_and_finish_with_error_partial_text(bool partialText, int? code, string status, bool isRetryable)
    {
        var error = $"{{\"code\":{(code?.ToString() ?? "null")},\"message\":\"Provider error\",\"status\":\"{status}\",\"details\":[{{\"@type\":\"type.googleapis.com/google.rpc.RetryInfo\"}}]}}";
        var sse = string.Empty;
        if (partialText)
        {
            sse += "data: {\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Partial answer\"}]}}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":2,\"totalTokenCount\":12}}\n\n";
        }

        sse += "data: {\"error\":" + error + "}\n\n";
        var handler = new RecordingHandler { ResponseText = sse, MediaType = "text/event-stream" };
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler);

        var events = await GoogleUpstream.Collect(await provider.LanguageModel("gemini-pro").DoStreamAsync(GoogleUpstream.Hello(), CancellationToken.None));

        var expected = new List<string> { "stream-start" };
        if (partialText)
        {
            expected.AddRange(["text-start", "text-delta"]);
        }

        expected.Add("error");
        if (partialText)
        {
            expected.Add("text-end");
        }

        expected.Add("finish");
        Assert.Equal(expected, events.Select(part => part.Type).ToList());

        var errorEvent = Assert.IsType<ErrorStreamPart>(events.Single(part => part is ErrorStreamPart));
        var providerError = errorEvent.Error!;
        Assert.Equal("Provider error", providerError.Message);
        Assert.Equal(status, providerError.Type);
        Assert.Equal(code, providerError.Code);
        Assert.Equal(code, providerError.StatusCode);
        Assert.Equal(isRetryable, providerError.IsRetryable);
        GoogleUpstream.JsonEqual((JsonElement)providerError.Data!, error);

        var finish = Assert.IsType<FinishStreamPart>(events[^1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        if (partialText)
        {
            Assert.Equal(10, finish.Usage.InputTokens);
            Assert.Equal(2, finish.Usage.OutputTokens);
        }
    }
}
