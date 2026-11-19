// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.GetRequestOrigin;

/// <summary>Port of <c>get-request-origin.test.ts</c>.</summary>
public sealed class GetRequestOriginTests
{
    private const string Prefix = "packages/ai/src/telemetry/get-request-origin.test.ts::";

    [Fact]
    [UpstreamTest(
        Prefix + "preserves metadata and its input while removing sensitive URL components",
        Coverage = UpstreamCoverage.Covered,
        Commit = "04a8f3837e7cf350358f2a084507445ab3e15f11"
    )]
    public void Preserves_metadata_and_its_input_while_removing_sensitive_URL_components()
    {
        var body = System.Text.Json.JsonDocument.Parse("""{"model":"test"}""").RootElement;
        var request = new LanguageModelStreamRequest(body)
        {
            Url = "https://user:password@example.com:8443/private?key=secret#fragment",
        };

        var sanitized = RequestOrigin.SanitizeRequestMetadata(request)!;

        Assert.Equal(body.GetRawText(), sanitized.Body!.Value.GetRawText());
        Assert.Equal("https://example.com:8443", sanitized.Url);
        Assert.Contains("password", request.Url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a URL")]
    [InlineData("/relative")]
    [InlineData("file:///etc/passwd")]
    [InlineData("data:text/plain,secret")]
    [UpstreamTest(
        Prefix + "omits unavailable HTTP origins (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "04a8f3837e7cf350358f2a084507445ab3e15f11"
    )]
    public void Omits_unavailable_HTTP_origins(string url)
    {
        var sanitized = RequestOrigin.SanitizeRequestMetadata(
            new LanguageModelStreamRequest(
                System.Text.Json.JsonDocument.Parse("\"keep\"").RootElement
            )
            {
                Url = url,
            }
        )!;

        Assert.Equal("\"keep\"", sanitized.Body!.Value.GetRawText());
        Assert.Null(sanitized.Url);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves requests without endpoint metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "04a8f3837e7cf350358f2a084507445ab3e15f11"
    )]
    public void Preserves_requests_without_endpoint_metadata()
    {
        var request = new LanguageModelStreamRequest(
            System.Text.Json.JsonDocument.Parse("\"keep\"").RootElement
        );

        Assert.Null(RequestOrigin.SanitizeRequestMetadata(null));
        Assert.Same(request, RequestOrigin.SanitizeRequestMetadata(request));
    }
}
