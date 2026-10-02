// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Runtime URLs, bearer tokens, and Signature Version 4 credentials.</summary>
public sealed class BedrockProviderTests
{
    private const string Response = "{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::resolves the Bedrock Runtime endpoint for %s", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_partition_runtime_hosts()
    {
        Assert.Equal("https://bedrock-runtime.cn-north-1.amazonaws.com.cn", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "cn-north-1" }));
        Assert.Equal("https://bedrock-runtime.us-gov-west-1.amazonaws.com", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "us-gov-west-1" }));
        Assert.Equal("https://bedrock-runtime.us-iso-east-1.c2s.ic.gov", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "us-iso-east-1" }));
        Assert.Equal("https://bedrock-runtime.us-isob-east-1.sc2s.sgov.gov", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "us-isob-east-1" }));
        Assert.Equal("https://bedrock-runtime.eu-isoe-west-1.cloud.adc-e.uk", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "eu-isoe-west-1" }));
        Assert.Equal("https://bedrock-runtime.us-isof-south-1.csp.hci.ic.gov", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "us-isof-south-1" }));
        Assert.Equal("https://bedrock-runtime.eusc-de-east-1.amazonaws.eu", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { Region = "eusc-de-east-1" }));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::uses an explicit base URL without loading a region", Coverage = UpstreamCoverage.Covered)]
    public void Uses_an_explicit_base_url()
    {
        var url = AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions
        {
            Region = "not a region",
            BaseUrl = "https://explicit.example.com/",
        });

        Assert.Equal("https://explicit.example.com", url);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::prefers the service-specific Bedrock Runtime endpoint over the global endpoint", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_runtime_endpoint_environment_variable()
    {
        WithEnvironment(
            () =>
            {
                Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com/");
                Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://global.example.com");
                Assert.Equal("https://runtime.example.com", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { ApiKey = "test-api-key" }));
            },
            "AWS_ENDPOINT_URL_BEDROCK_RUNTIME",
            "AWS_ENDPOINT_URL");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::uses the global endpoint override when no service-specific endpoint is set", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_endpoint_override()
    {
        WithEnvironment(
            () =>
            {
                Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", null);
                Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://global.example.com/");
                Assert.Equal("https://global.example.com", AmazonBedrockEndpoints.ResolveRuntimeBaseUrl(new AmazonBedrockOptions { ApiKey = "test-api-key" }));
            },
            "AWS_ENDPOINT_URL_BEDROCK_RUNTIME",
            "AWS_ENDPOINT_URL");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::uses a separate service-specific endpoint for Bedrock Agent Runtime", Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_separate_agent_runtime_endpoint()
    {
        WithEnvironment(
            () =>
            {
                Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com");
                Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME", "https://agent-runtime.example.com");
                Assert.Equal("https://agent-runtime.example.com", AmazonBedrockEndpoints.ResolveAgentRuntimeBaseUrl(new AmazonBedrockOptions { Region = "us-east-1" }));
            },
            "AWS_ENDPOINT_URL_BEDROCK_RUNTIME",
            "AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::resolves the Bedrock Agent Runtime endpoint for a non-standard AWS partition", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_the_agent_runtime_host_for_an_iso_partition()
    {
        Assert.Equal(
            "https://bedrock-agent-runtime.eu-isoe-west-1.cloud.adc-e.uk",
            AmazonBedrockEndpoints.ResolveAgentRuntimeBaseUrl(new AmazonBedrockOptions { Region = "eu-isoe-west-1" }));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::should use API key when provided in options", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_bearer_token_from_options()
    {
        var handler = new BedrockJsonHandler(Response);
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", new AmazonBedrockOptions { ApiKey = " test-api-key " });
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.DoesNotContain("AWS4-HMAC-SHA256", handler.Headers["Authorization"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::should use API key from environment variable", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_bearer_token_from_the_environment()
    {
        await WithEnvironmentAsync(
            async () =>
            {
                Environment.SetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK", " env-token ");
                var handler = new BedrockJsonHandler(Response);
                var model = BedrockParity.Model(handler, "anthropic.claude-v2", new AmazonBedrockOptions());
                await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);
                Assert.Equal("Bearer env-token", handler.Headers["Authorization"]);
            },
            "AWS_BEARER_TOKEN_BEDROCK");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::should prioritize options.apiKey over environment variable", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_the_options_api_key()
    {
        await WithEnvironmentAsync(
            async () =>
            {
                Environment.SetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK", "env-token");
                var handler = new BedrockJsonHandler(Response);
                var model = BedrockParity.Model(handler, "anthropic.claude-v2", new AmazonBedrockOptions { ApiKey = "option-token" });
                await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);
                Assert.Equal("Bearer option-token", handler.Headers["Authorization"]);
            },
            "AWS_BEARER_TOKEN_BEDROCK");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::should fall back to SigV4 when no API key provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_signature_version_4()
    {
        var handler = new BedrockJsonHandler(Response);
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", Credentials());
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=AKIA/20200101/us-east-1/bedrock/aws4_request", handler.Headers["Authorization"], StringComparison.Ordinal);
        Assert.Equal("20200101T000000Z", handler.Headers["x-amz-date"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-sigv4-fetch.test.ts::createApiKeyFetchFunction::should handle empty string API key", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_an_empty_api_key_as_absent()
    {
        var handler = new BedrockJsonHandler(Response);
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", Credentials(string.Empty));
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.StartsWith("AWS4-HMAC-SHA256", handler.Headers["Authorization"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::does not use AWS_SESSION_TOKEN from env when both access keys are passed as options", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_the_environment_session_token_when_both_keys_are_explicit()
    {
        await WithEnvironmentAsync(
            async () =>
            {
                Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "env-session");
                var handler = new BedrockJsonHandler(Response);
                var model = BedrockParity.Model(handler, "anthropic.claude-v2", Credentials());
                await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);
                Assert.False(handler.Headers.ContainsKey("x-amz-security-token"));
                Assert.DoesNotContain("x-amz-security-token", handler.Headers["Authorization"], StringComparison.Ordinal);
            },
            "AWS_SESSION_TOKEN");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::uses options.sessionToken when both access keys are passed as options", Coverage = UpstreamCoverage.Covered)]
    public async Task Signs_with_the_options_session_token()
    {
        var options = Credentials();
        options.SessionToken = "option-session";
        var handler = new BedrockJsonHandler(Response);
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", options);
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Equal("option-session", handler.Headers["x-amz-security-token"]);
        Assert.Contains("x-amz-security-token", handler.Headers["Authorization"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::should work with credential provider when no API key is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Signs_with_credentials_from_the_provider()
    {
        var handler = new BedrockJsonHandler(Response);
        var options = new AmazonBedrockOptions
        {
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialProvider = () => new AmazonBedrockCredentials("dynamic-access-key", "dynamic-secret-key", "dynamic-session-token"),
        };
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", options);
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Contains("Credential=dynamic-access-key/", handler.Headers["Authorization"], StringComparison.Ordinal);
        Assert.Equal("dynamic-session-token", handler.Headers["x-amz-security-token"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::should prioritize credentialProvider over static credentials", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_the_credential_provider_over_static_keys()
    {
        var handler = new BedrockJsonHandler(Response);
        var options = Credentials();
        options.CredentialProvider = () => new AmazonBedrockCredentials("dynamic-access-key", "dynamic-secret-key");
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", options);
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Contains("Credential=dynamic-access-key/", handler.Headers["Authorization"], StringComparison.Ordinal);
        Assert.DoesNotContain("Credential=AKIA/", handler.Headers["Authorization"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-sigv4-fetch.test.ts::createSigV4FetchFunction::should use default service name \"bedrock\" when no service parameter is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Signs_converse_requests_for_the_bedrock_service()
    {
        var handler = new BedrockJsonHandler(Response);
        var model = BedrockParity.Model(handler, "anthropic.claude-v2", Credentials());
        await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Contains("/us-east-1/bedrock/aws4_request", handler.Headers["Authorization"], StringComparison.Ordinal);
        Assert.Equal("POST", handler.Method);
        Assert.Equal("bedrock-runtime.us-east-1.amazonaws.com", new Uri(handler.Uri).Host);
    }

    private static AmazonBedrockOptions Credentials(string? apiKey = null)
    {
        return new AmazonBedrockOptions
        {
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            ApiKey = apiKey,
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
    }

    private static void WithEnvironment(Action action, params string[] names)
    {
        var previous = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            previous[name] = Environment.GetEnvironmentVariable(name);
        }

        try
        {
            action();
        }
        finally
        {
            foreach (var pair in previous)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }

    private static async Task WithEnvironmentAsync(Func<Task> action, params string[] names)
    {
        var previous = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            previous[name] = Environment.GetEnvironmentVariable(name);
        }

        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            foreach (var pair in previous)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }
}
