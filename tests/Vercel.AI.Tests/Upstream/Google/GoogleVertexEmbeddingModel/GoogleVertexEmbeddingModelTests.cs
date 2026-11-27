// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel.GoogleVertexEmbeddingModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Port of <c>google-vertex-embedding-model.test.ts</c> &gt; <c>GoogleVertexEmbeddingModel</c>.</summary>
public sealed class GoogleVertexEmbeddingModelTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel::";

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var handler = Server();

        await Model(handler, configure: options =>
                options.Headers["X-Custom-Header"] = "custom-value"
            )
            .DoEmbedAsync(
                Call(
                    TestValues,
                    Options("google", MockProviderOptions),
                    headers: new Dictionary<string, string>
                    {
                        ["X-Request-Header"] = "request-value",
                    }
                ),
                CancellationToken.None
            );

        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"]);
        Assert.Equal("custom-value", handler.RequestHeaders["X-Custom-Header"]);
        Assert.Equal("request-value", handler.RequestHeaders["X-Request-Header"]);
        Assert.Contains("ai-sdk/google", handler.RequestHeaders["User-Agent"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw TooManyEmbeddingValuesForCallError when too many values provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_TooManyEmbeddingValuesForCallError_when_too_many_values_provided()
    {
        var tooManyValues = Enumerable.Repeat("test", 251).ToArray();

        await Assert.ThrowsAsync<TooManyEmbeddingValuesForCallException>(() =>
            Model(Server())
                .DoEmbedAsync(
                    Call(tooManyValues, Options("google", MockProviderOptions)),
                    CancellationToken.None
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom baseURL when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_baseURL_when_provided()
    {
        var handler = Server();

        var response = await Model(handler, configure: options =>
                options.BaseUrl = "https://custom-endpoint.com"
            )
            .DoEmbedAsync(
                Call(TestValues, Options("google", "{\"outputDimensionality\":768}")),
                CancellationToken.None
            );

        AssertEmbeddings(response.Embeddings);
        Assert.Equal(CustomUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom fetch when provided and include proper request content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_fetch_when_provided_and_include_proper_request_content()
    {
        // The custom fetch of the upstream test is the HttpMessageHandler of the provider.
        var handler = Server();

        var response = await Model(handler, configure: options =>
                options.BaseUrl = "https://custom-endpoint.com"
            )
            .DoEmbedAsync(
                Call(TestValues, Options("google", "{\"outputDimensionality\":768}")),
                CancellationToken.None
            );

        AssertEmbeddings(response.Embeddings);
        Assert.Equal(CustomUrl, handler.Uris[0]);
        GoogleUpstream.JsonEqual(
            Body(handler),
            "{\"instances\":[{\"content\":\"test text one\"},{\"content\":\"test text two\"}],\"parameters\":{\"outputDimensionality\":768}}"
        );
    }
}
