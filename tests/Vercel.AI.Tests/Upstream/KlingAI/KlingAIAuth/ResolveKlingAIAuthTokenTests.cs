// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using KlingAIAuthType = Vercel.AI.KlingAI.KlingAIAuth;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIAuth;

/// <summary>Port of <c>klingai-auth.test.ts</c> &gt; <c>resolveKlingAIAuthToken</c>.</summary>
[Collection(KlingAIEnvironmentCollection.Name)]
public sealed class ResolveKlingAIAuthTokenTests : IDisposable
{
    private const string Prefix = "packages/klingai/src/klingai-auth.test.ts::resolveKlingAIAuthToken::";

    private readonly KlingAIEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "should return the api key verbatim when passed explicitly", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_the_api_key_verbatim_when_passed_explicitly()
    {
        Assert.Equal("test-api-key", KlingAIAuthType.ResolveAuthToken(apiKey: "test-api-key"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should trim the api key", Coverage = UpstreamCoverage.Covered)]
    public void Should_trim_the_api_key()
    {
        Assert.Equal("test-api-key", KlingAIAuthType.ResolveAuthToken(apiKey: "  test-api-key  "));
    }

    [Fact]
    [UpstreamTest(Prefix + "should load the api key from the environment", Coverage = UpstreamCoverage.Covered)]
    public void Should_load_the_api_key_from_the_environment()
    {
        _env.Stub("KLINGAI_API_KEY", "env-api-key");

        Assert.Equal("env-api-key", KlingAIAuthType.ResolveAuthToken());
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer an explicit api key over the environment variable", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_an_explicit_api_key_over_the_environment_variable()
    {
        _env.Stub("KLINGAI_API_KEY", "env-api-key");

        Assert.Equal("explicit-api-key", KlingAIAuthType.ResolveAuthToken(apiKey: "explicit-api-key"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer an explicit api key over explicit legacy credentials", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_an_explicit_api_key_over_explicit_legacy_credentials()
    {
        var token = KlingAIAuthType.ResolveAuthToken("explicit-api-key", "test-ak", "test-sk");

        Assert.Equal("explicit-api-key", token);
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer explicit legacy credentials over the api key environment variable", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_explicit_legacy_credentials_over_the_api_key_environment_variable()
    {
        _env.Stub("KLINGAI_API_KEY", "env-api-key");

        var token = KlingAIAuthType.ResolveAuthToken(accessKey: "test-ak", secretKey: "test-sk");

        Assert.Equal(3, token.Split('.').Length);
        Assert.Equal("test-ak", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should prefer the api key environment variable over legacy environment variables", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_the_api_key_environment_variable_over_legacy_environment_variables()
    {
        _env.Stub("KLINGAI_API_KEY", "env-api-key");
        _env.Stub("KLINGAI_ACCESS_KEY", "env-access-key");
        _env.Stub("KLINGAI_SECRET_KEY", "env-secret-key");

        Assert.Equal("env-api-key", KlingAIAuthType.ResolveAuthToken());
    }

    [Fact]
    [UpstreamTest(Prefix + "should fall back to a signed JWT when only legacy environment variables are set", Coverage = UpstreamCoverage.Covered)]
    public void Should_fall_back_to_a_signed_JWT_when_only_legacy_environment_variables_are_set()
    {
        _env.Stub("KLINGAI_ACCESS_KEY", "env-access-key");
        _env.Stub("KLINGAI_SECRET_KEY", "env-secret-key");

        var token = KlingAIAuthType.ResolveAuthToken();

        Assert.Equal(3, token.Split('.').Length);
        Assert.Equal("env-access-key", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should ignore a blank api key and fall back to legacy credentials", Coverage = UpstreamCoverage.Covered)]
    public void Should_ignore_a_blank_api_key_and_fall_back_to_legacy_credentials()
    {
        _env.Stub("KLINGAI_API_KEY", "   ");
        _env.Stub("KLINGAI_ACCESS_KEY", "env-access-key");
        _env.Stub("KLINGAI_SECRET_KEY", "env-secret-key");

        var token = KlingAIAuthType.ResolveAuthToken();

        Assert.Equal("env-access-key", (string?)KlingAIJwt.Decode(token, 1)["iss"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw an api-key-centric error when no credentials are available", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_an_api_key_centric_error_when_no_credentials_are_available()
    {
        var error = Assert.Throws<AiSdkException>(() => KlingAIAuthType.ResolveAuthToken());

        Assert.Matches(new Regex("KlingAI API key is missing.*'apiKey'.*KLINGAI_API_KEY", RegexOptions.Singleline, TimeSpan.FromSeconds(1)), error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should still report the missing secret key when only an access key is set", Coverage = UpstreamCoverage.Covered)]
    public void Should_still_report_the_missing_secret_key_when_only_an_access_key_is_set()
    {
        _env.Stub("KLINGAI_ACCESS_KEY", "env-access-key");

        var error = Assert.Throws<AiSdkException>(() => KlingAIAuthType.ResolveAuthToken());

        Assert.Contains("KlingAI secret key", error.Message);
    }
}
