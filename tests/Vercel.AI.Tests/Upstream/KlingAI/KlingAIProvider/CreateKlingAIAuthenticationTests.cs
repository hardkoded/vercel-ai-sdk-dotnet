// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using Vercel.AI.KlingAI;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider.KlingAIProviderSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider;

/// <summary>Port of <c>klingai-provider.test.ts</c> &gt; <c>createKlingAI &gt; authentication</c>.</summary>
[Collection(KlingAIEnvironmentCollection.Name)]
public sealed class CreateKlingAIAuthenticationTests : IDisposable
{
    private const string Prefix = "packages/klingai/src/klingai-provider.test.ts::createKlingAI > authentication::";

    private readonly KlingAIEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the resolved token as a bearer token", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_the_resolved_token_as_a_bearer_token()
    {
        // Upstream mocks the resolver; here the real resolver runs: an api key is resolved verbatim.
        var call = await SendAsync(new KlingAIOptions { ApiKey = "test-api-key" });

        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward legacy credentials to the token resolver", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_legacy_credentials_to_the_token_resolver()
    {
        var call = await SendAsync(new KlingAIOptions { AccessKey = "test-ak", SecretKey = "test-sk" });

        var token = call.Header("Authorization")!.Substring("Bearer ".Length);
        Assert.Equal("test-ak", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
        var parts = token.Split('.');
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("test-sk"));
        var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(parts[0] + "." + parts[1]))).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        Assert.Equal(expected, parts[2]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge custom headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_custom_headers()
    {
        var options = new KlingAIOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Header"] = "custom-value";

        var call = await SendAsync(options);

        Assert.Equal("custom-value", call.Header("custom-header"));
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
    }
}
