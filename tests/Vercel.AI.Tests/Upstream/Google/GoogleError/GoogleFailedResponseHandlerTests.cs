// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleError;

/// <summary>Port of <c>google-error.test.ts</c> &gt; <c>googleFailedResponseHandler</c>.</summary>
public sealed class GoogleFailedResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/google/src/google-error.test.ts::googleFailedResponseHandler::preserves google.rpc.RetryInfo in APICallError.data",
        Coverage = UpstreamCoverage.Partial,
        Note = "Google failures surface as ApiException subclasses that carry the raw ResponseBody and have no parsed Data property. The test reads the preserved details from ResponseBody."
    )]
    public async Task Preserves_google_rpc_RetryInfo_in_the_API_error_response()
    {
        var responseBody = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-429-retry-info.json")
        );
        var handler = new RecordingHandler
        {
            ResponseText = responseBody,
            Status = HttpStatusCode.TooManyRequests,
        };
        var provider = GoogleProvider.Create(
            new GoogleOptions { ApiKey = "test-api-key" },
            handler
        );

        var error = await Assert.ThrowsAnyAsync<ApiException>(async () =>
            await provider
                .LanguageModel("gemini-2.5-flash")
                .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None)
        );

        using var data = JsonDocument.Parse(error.ResponseBody!);
        var details = data.RootElement.GetProperty("error").GetProperty("details");
        Assert.Equal(
            "type.googleapis.com/google.rpc.QuotaFailure",
            details[0].GetProperty("@type").GetString()
        );
        Assert.Equal(
            "type.googleapis.com/google.rpc.RetryInfo",
            details[1].GetProperty("@type").GetString()
        );
        Assert.Equal("34.4s", details[1].GetProperty("retryDelay").GetString());
    }
}
