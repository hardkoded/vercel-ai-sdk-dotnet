// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexXaiProviderNode;

public sealed class GoogleVertexXaiProviderNodeTests
{
    private const string Prefix =
        "packages/google-vertex/src/xai/google-vertex-xai-provider-node.test.ts::google-vertex-xai-provider-node::";
    private const string ChatResponse =
        "{\"id\":\"chatcmpl-1\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1}}";
    private const string Endpoint =
        "https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi/chat/completions";

    [Fact]
    [UpstreamTest(
        Prefix + "should create provider with auth wrapper",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_create_provider_with_auth_wrapper()
    {
        var handler = new RecordingHandler { ResponseText = ChatResponse };
        var calls = 0;
        var options = Options(_ => Task.FromResult("mock-auth-token-" + ++calls));

        var provider = new Vercel.AI.Google.GoogleVertexXaiProvider(options, handler);
        Assert.NotNull(provider);
        Assert.Equal(0, handler.Calls);

        await provider
            .LanguageModel("xai/grok-4.1-fast-reasoning")
            .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(Endpoint, handler.Uris.Single());
        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate auth token and add to request headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_auth_token_and_add_to_request_headers()
    {
        var handler = new RecordingHandler { ResponseText = ChatResponse };
        var calls = 0;
        var options = Options(_ => Task.FromResult("mock-auth-token-" + ++calls));

        await Generate(options, handler).ConfigureAwait(false);

        Assert.Equal(Endpoint, handler.Uris.Single());
        Assert.Equal("Bearer mock-auth-token-1", handler.RequestHeaders["Authorization"]);
        Assert.StartsWith("application/json", handler.RequestHeaders["Content-Type"]);
        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge custom headers with auth header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_custom_headers_with_auth_header()
    {
        var handler = new RecordingHandler { ResponseText = ChatResponse };
        var options = Options(_ => Task.FromResult("mock-auth-token"));
        options.Headers["X-Custom"] = "header-value";
        options.Headers["Authorization"] = "Bearer user-value";

        await Generate(options, handler).ConfigureAwait(false);

        Assert.Equal("header-value", handler.RequestHeaders["X-Custom"]);
        Assert.Equal("Bearer mock-auth-token", handler.RequestHeaders["Authorization"]);
        Assert.StartsWith("application/json", handler.RequestHeaders["Content-Type"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom fetch when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_fetch_when_provided()
    {
        var handler = new RecordingHandler { ResponseText = ChatResponse };
        var options = Options(_ => Task.FromResult("mock-auth-token"));

        var result = await Generate(options, handler).ConfigureAwait(false);

        Assert.Equal(1, handler.Calls);
        Assert.Equal("Bearer mock-auth-token", handler.RequestHeaders["Authorization"]);
        Assert.NotEmpty(result.Content);
    }

    private static GoogleVertexPartnerOptions Options(Func<CancellationToken, Task<string>> token)
    {
        return new GoogleVertexPartnerOptions
        {
            Project = "test-project",
            GenerateAuthToken = token,
        };
    }

    private static Task<Vercel.AI.Provider.LanguageModelGenerateResult> Generate(
        GoogleVertexPartnerOptions options,
        RecordingHandler handler
    )
    {
        return new Vercel.AI.Google.GoogleVertexXaiProvider(options, handler)
            .LanguageModel("xai/grok-4.1-fast-reasoning")
            .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);
    }
}
