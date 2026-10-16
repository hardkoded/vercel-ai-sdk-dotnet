// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using KlingAIAuthType = Vercel.AI.KlingAI.KlingAIAuth;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIAuth;

/// <summary>Port of <c>klingai-auth.test.ts</c> &gt; <c>generateKlingAIAuthToken</c>.</summary>
[Collection(KlingAIEnvironmentCollection.Name)]
public sealed class GenerateKlingAIAuthTokenTests : IDisposable
{
    private const string Prefix = "packages/klingai/src/klingai-auth.test.ts::generateKlingAIAuthToken::";

    private readonly KlingAIEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "should generate a valid JWT token structure", Coverage = UpstreamCoverage.Covered)]
    public void Should_generate_a_valid_JWT_token_structure()
    {
        var token = KlingAIAuthType.GenerateAuthToken("test-access-key", "test-secret-key");

        // JWT should have 3 parts separated by dots
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include correct header with HS256 algorithm", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_correct_header_with_HS256_algorithm()
    {
        var token = KlingAIAuthType.GenerateAuthToken("test-access-key", "test-secret-key");

        var header = KlingAIJwt.Decode(token, 0);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"), header));
        Assert.Equal(2, header.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include issuer (iss) matching the access key", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_issuer_iss_matching_the_access_key()
    {
        var token = KlingAIAuthType.GenerateAuthToken("my-access-key-123", "my-secret-key");

        Assert.Equal("my-access-key-123", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include exp and nbf claims", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_exp_and_nbf_claims()
    {
        var token = KlingAIAuthType.GenerateAuthToken("test-ak", "test-sk");

        var payload = KlingAIJwt.Decode(token, 1);

        Assert.NotNull(payload["exp"]);
        Assert.NotNull(payload["nbf"]);
        // exp should be approximately 30 minutes from now
        var span = (long)payload["exp"]! - (long)payload["nbf"]!;
        Assert.True(span > 1800 - 10);
        Assert.True(span < 1800 + 10);
    }

    [Fact]
    [UpstreamTest(Prefix + "should load access key from environment variable", Coverage = UpstreamCoverage.Covered)]
    public void Should_load_access_key_from_environment_variable()
    {
        _env.Stub("KLINGAI_ACCESS_KEY", "env-access-key");

        var token = KlingAIAuthType.GenerateAuthToken(secretKey: "test-sk");

        Assert.Equal("env-access-key", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer explicit accessKey over environment variable", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_explicit_accessKey_over_environment_variable()
    {
        _env.Stub("KLINGAI_ACCESS_KEY", "env-access-key");

        var token = KlingAIAuthType.GenerateAuthToken("explicit-access-key", "test-sk");

        Assert.Equal("explicit-access-key", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when access key is not available", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_when_access_key_is_not_available()
    {
        var error = Assert.Throws<AiSdkException>(() => KlingAIAuthType.GenerateAuthToken(secretKey: "test-sk"));

        Assert.Contains("KlingAI access key", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when secret key is not available", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_when_secret_key_is_not_available()
    {
        var error = Assert.Throws<AiSdkException>(() => KlingAIAuthType.GenerateAuthToken(accessKey: "test-ak"));

        Assert.Contains("KlingAI secret key", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should produce different tokens for different secret keys", Coverage = UpstreamCoverage.Covered)]
    public void Should_produce_different_tokens_for_different_secret_keys()
    {
        var token1 = KlingAIAuthType.GenerateAuthToken("same-ak", "secret-key-1");
        var token2 = KlingAIAuthType.GenerateAuthToken("same-ak", "secret-key-2");

        // Signatures should differ
        Assert.NotEqual(token1.Split('.')[2], token2.Split('.')[2]);
    }
}
