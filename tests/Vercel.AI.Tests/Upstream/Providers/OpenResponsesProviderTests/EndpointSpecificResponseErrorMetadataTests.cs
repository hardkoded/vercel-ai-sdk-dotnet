// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenResponsesProviderTests;

/// <summary>Port of <c>open-responses-provider.test.ts</c> &gt; <c>endpoint-specific response error metadata</c>.</summary>
public sealed class EndpointSpecificResponseErrorMetadataTests
{
    private const string Prefix = "packages/open-responses/src/open-responses-provider.test.ts::endpoint-specific response error metadata::";

    private const string Error = """{"code":"busy","message":"Try again later.","http_status":503,"retry":false}""";

    [Fact]
    [UpstreamTest(Prefix + "maps generation failures without assuming a vendor status field", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_generation_failures_without_assuming_a_vendor_status_field()
    {
        var capture = new UpstreamCapture { ResponseBody = """{"error":ERROR,"status":"failed","output":[]}""".Replace("ERROR", Error) };

        var error = await Assert.ThrowsAsync<ApiException>(() => Model(capture, ReadMetadata).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));

        Assert.Equal(503, error.StatusCode);
        Assert.False(error.IsRetryable);
        JsonAssert.Equal((JsonElement)error.ErrorData!, Error);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps %s stream errors with the same callback", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_s_stream_errors_with_the_same_callback()
    {
        foreach (var json in new[]
        {
            """{"type":"response.failed","sequence_number":1,"response":{"status":"failed","error":ERROR,"output":[]}}""".Replace("ERROR", Error),
            """{"type":"error","sequence_number":1,"error":ERROR}""".Replace("ERROR", Error),
        })
        {
            var capture = new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(json) };

            var parts = await UpstreamChat.Read(await Model(capture, ReadMetadata).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));

            var error = Assert.Single(parts.OfType<ErrorStreamPart>()).Error;
            Assert.NotNull(error);
            Assert.Equal(503, error.StatusCode);
            Assert.False(error.IsRetryable);
            JsonAssert.Equal((JsonElement)error.Data!, json);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "does not interpret vendor error fields unless configured", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_interpret_vendor_error_fields_unless_configured()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = """{"error":{"code":"busy","message":"Try again later.","http_status":503,"retry":false,"status_code":503},"status":"failed","output":[]}""",
        };

        var error = await Assert.ThrowsAsync<ApiException>(() => Model(capture, null).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));

        Assert.Equal(400, error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    private static OpenResponsesErrorMetadata ReadMetadata(JsonElement error)
    {
        return new OpenResponsesErrorMetadata
        {
            StatusCode = error.GetProperty("http_status").GetInt32(),
            IsRetryable = error.GetProperty("retry").GetBoolean(),
        };
    }

    private static ILanguageModel Model(UpstreamCapture capture, Func<JsonElement, OpenResponsesErrorMetadata?>? getResponseErrorMetadata)
    {
        var options = new OpenResponsesOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://example.com",
            Name = "acme",
            GetResponseErrorMetadata = getResponseErrorMetadata,
        };
        return OpenResponsesProvider.Create(options, capture).LanguageModel("model");
    }
}
