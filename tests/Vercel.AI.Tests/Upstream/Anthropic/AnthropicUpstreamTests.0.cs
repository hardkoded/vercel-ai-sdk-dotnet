// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>One method per in-scope Anthropic upstream unit test.</summary>
[Collection("AnthropicParity")]
public sealed partial class AnthropicUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should bypass signing for non-POST requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0001()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should bypass signing for non-POST requests").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should bypass signing if POST request has no body", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0002()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should bypass signing if POST request has no body").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle a POST request with a string body and merge signed headers including user-agent", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0003()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle a POST request with a string body and merge signed headers including user-agent").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::shold handle a POST request with a Request object", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0004()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::shold handle a POST request with a Request object").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should sign when input is a POST Request with body and no init", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0005()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should sign when input is a POST Request with body and no init").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle non-string body by stringifying it", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0006()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle non-string body by stringifying it").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle Uint8Array body", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0007()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle Uint8Array body").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle ArrayBuffer body", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0008()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle ArrayBuffer body").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should extract headers from a Headers instance", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0009()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should extract headers from a Headers instance").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle headers provided as an array", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0010()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle headers provided as an array").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should call original fetch if init is undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0011()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should call original fetch if init is undefined").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should correctly handle async credential providers", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0012()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should correctly handle async credential providers").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle async credential providers that reject", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0013()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should handle async credential providers that reject").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createSigV4FetchFunction::should send non-ASCII header values without signing them", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_non_ASCII_header_values_without_signing_them()
    {
        AnthropicAssertions.FetchNonAsciiHeader();
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should add x-api-key header with user-agent", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0014()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should add x-api-key header with user-agent").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should merge x-api-key header with existing headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0015()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should merge x-api-key header with existing headers").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work with Headers instance", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0016()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work with Headers instance").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work with headers as array", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0017()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work with headers as array").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work with GET requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0018()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work with GET requests").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work when no headers are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0019()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work when no headers are provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work when init is undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0020()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should work when init is undefined").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should override existing x-api-key header", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0021()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should override existing x-api-key header").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should use default fetch when no custom fetch provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0022()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should use default fetch when no custom fetch provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should resolve default fetch lazily when no custom fetch provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0023()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should resolve default fetch lazily when no custom fetch provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should handle empty string API key", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0024()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should handle empty string API key").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should preserve request body and other properties", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0025()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-fetch.test.ts::createApiKeyFetchFunction::should preserve request body and other properties").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws > baseURL configuration::uses the default Claude Platform on AWS base URL with region templating", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0026()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws > baseURL configuration::uses the default Claude Platform on AWS base URL with region templating").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws > baseURL configuration::reads AWS_REGION from the environment when region option is omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0027()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws > baseURL configuration::reads AWS_REGION from the environment when region option is omitted").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws > baseURL configuration::prefers the baseURL option over the default template", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0028()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws > baseURL configuration::prefers the baseURL option over the default template").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > apiKey option::sends x-api-key header when apiKey is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0029()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > apiKey option::sends x-api-key header when apiKey is provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > apiKey option::reads apiKey from ANTHROPIC_AWS_API_KEY when option is omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0030()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > apiKey option::reads apiKey from ANTHROPIC_AWS_API_KEY when option is omitted").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::signs requests with SigV4 when apiKey is not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0031()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::signs requests with SigV4 when apiKey is not provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::honors a credentialProvider for dynamic SigV4 credentials", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0032()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::honors a credentialProvider for dynamic SigV4 credentials").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::throws a guided error when SigV4 credentials are missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0033()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::throws a guided error when SigV4 credentials are missing").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::wraps credentialProvider rejections with a guided message", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0034()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - authentication > SigV4 path::wraps credentialProvider rejections with a guided message").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::sends the anthropic-version header on every request", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0035()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::sends the anthropic-version header on every request").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::sends the anthropic-workspace-id header on every request", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0036()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::sends the anthropic-workspace-id header on every request").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::reads workspaceId from ANTHROPIC_AWS_WORKSPACE_ID when option is omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0037()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::reads workspaceId from ANTHROPIC_AWS_WORKSPACE_ID when option is omitted").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::throws when workspaceId is not resolvable at request time", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0038()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - workspaceId::throws when workspaceId is not resolvable at request time").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - region::throws when region is not resolvable at model creation time", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0039()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - region::throws when region is not resolvable at model creation time").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - headers::merges custom headers with the workspace-id header", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0040()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - headers::merges custom headers with the workspace-id header").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - supportedUrls::should support image/* URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0041()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - supportedUrls::should support image/* URLs").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - supportedUrls::should support application/pdf URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0042()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - supportedUrls::should support application/pdf URLs").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::sets the provider name to anthropic-aws.messages on the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0043()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::sets the provider name to anthropic-aws.messages on the model").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::throws NoSuchModelError when embeddingModel is invoked", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0044()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::throws NoSuchModelError when embeddingModel is invoked").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::throws NoSuchModelError when imageModel is invoked", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0045()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::throws NoSuchModelError when imageModel is invoked").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::exposes files() returning an AnthropicFiles instance", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0046()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::exposes files() returning an AnthropicFiles instance").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::exposes skills() returning an AnthropicSkills instance", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0047()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::exposes skills() returning an AnthropicSkills instance").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::throws if the provider function is called with new", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0048()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - model identity::throws if the provider function is called with new").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - auth precedence::prefers the API-key path when both apiKey and AWS SigV4 creds are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0049()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - auth precedence::prefers the API-key path when both apiKey and AWS SigV4 creds are present").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - streaming::forwards doStream through the SigV4 / api-key fetch wrapper and yields stream events", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0050()
    {
        await AnthropicCases.Run("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::anthropicAws provider - streaming::forwards doStream through the SigV4 / api-key fetch wrapper and yields stream events").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported request types before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0051()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported request types before making an API request").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported $feature", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0059()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects unsupported $feature").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects invalid request IDs before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0060()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects invalid request IDs before making an API request").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects duplicate request IDs before making an API request", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0061()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects duplicate request IDs before making an API request").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::maps status %s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0062()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::maps status %s to %s").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::normalizes request counts", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0063()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::normalizes request counts").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects result retrieval after Anthropic archives the result file", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0067()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch::rejects result retrieval after Anthropic archives the result file").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::rejects result retrieval while the batch is pending", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0079()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::rejects result retrieval while the batch is pending").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::rejects a completed batch without output", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0084()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::rejects a completed batch without output").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-error.test.ts::anthropicError > anthropicErrorDataSchema::should parse overloaded error", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0086()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-error.test.ts::anthropicError > anthropicErrorDataSchema::should parse overloaded error").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::uses native Messages output with portable constraints and configured settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0087()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::uses native Messages output with portable constraints and configured settings").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::allows provider options to enable thinking for evaluations", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0088()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::allows provider options to enable thinking for evaluations").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::selects native structured output automatically for supported models", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0089()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::selects native structured output automatically for supported models").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::supports the existing JSON-tool fallback through provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0090()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::supports the existing JSON-tool fallback through provider options").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::rejects %s even with valid JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0091()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::rejects %s even with valid JSON").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::validates score bounds locally", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0092()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::validates score bounds locally").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-evaluation.test.ts::evaluates Boolean alongside Choice and Score using %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0093()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-evaluation.test.ts::evaluates Boolean alongside Choice and Score using %s").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::sends POST to /v1/files with correct beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0094()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::sends POST to /v1/files with correct beta header").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::threads per-call headers and abortSignal", Coverage = UpstreamCoverage.Partial)]
    public async Task Case_0096()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::threads per-call headers and abortSignal").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::sends x-api-key header", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0097()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::sends x-api-key header").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::sends multipart form data with file", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0098()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::sends multipart form data with file").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::uses default filename \"blob\" when not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0099()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::uses default filename \"blob\" when not specified").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::uses custom filename from spec options", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0100()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::uses custom filename from spec options").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::uses mediaType from spec options", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0101()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::uses mediaType from spec options").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::returns providerReference with anthropic key set to file ID", Coverage = UpstreamCoverage.Partial)]
    public async Task Case_0102()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::returns providerReference with anthropic key set to file ID").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::returns providerMetadata with response data", Coverage = UpstreamCoverage.Partial)]
    public async Task Case_0103()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::returns providerMetadata with response data").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::omits downloadable from providerMetadata when null", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0104()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::omits downloadable from providerMetadata when null").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::handles base64 string data", Coverage = UpstreamCoverage.Partial)]
    public async Task Case_0105()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::handles base64 string data").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::has specificationVersion v4", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0106()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::has specificationVersion v4").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::has correct provider name", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0107()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-files.test.ts::AnthropicFiles > uploadFile::has correct provider name").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should pass thinking config; add budget tokens; clear out temperature, top_p, top_k; and return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0108()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should pass thinking config; add budget tokens; clear out temperature, top_p, top_k; and return warnings").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should extract reasoning response", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0109()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should extract reasoning response").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should preserve container upload response blocks as custom content", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0110()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should preserve container upload response blocks as custom content").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should use default budget when thinking type is enabled without budgetTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0111()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking enabled)::should use default budget when thinking type is enabled without budgetTokens").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking disabled)::should forward thinking { type: \"disabled\" } to the API instead of stripping it", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0114()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking disabled)::should forward thinking { type: \"disabled\" } to the API instead of stripping it").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking disabled)::should not strip temperature / topP / topK when thinking is disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0115()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > reasoning (thinking disabled)::should not strip temperature / topP / topK when thinking is disabled").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"none\" to thinking disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0117()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"none\" to thinking disabled").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"low\" to adaptive thinking with effort \"low\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0118()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"low\" to adaptive thinking with effort \"low\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"medium\" to adaptive thinking with effort \"medium\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0119()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"medium\" to adaptive thinking with effort \"medium\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"high\" to adaptive thinking with effort \"high\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0120()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"high\" to adaptive thinking with effort \"high\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"xhigh\" to adaptive thinking with effort \"max\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0121()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"xhigh\" to adaptive thinking with effort \"max\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"minimal\" to adaptive thinking with effort \"low\" and emit compatibility warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0122()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (newer models with adaptive thinking)::should map reasoning \"minimal\" to adaptive thinking with effort \"low\" and emit compatibility warning").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"none\" to thinking disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0125()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"none\" to thinking disabled").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"minimal\" to enabled thinking with ~2% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0126()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"minimal\" to enabled thinking with ~2% budget").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"low\" to enabled thinking with ~10% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0127()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"low\" to enabled thinking with ~10% budget").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"medium\" to enabled thinking with ~30% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0128()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"medium\" to enabled thinking with ~30% budget").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"high\" to enabled thinking with ~60% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0129()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"high\" to enabled thinking with ~60% budget").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"xhigh\" to enabled thinking with ~90% budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0130()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should map reasoning \"xhigh\" to enabled thinking with ~90% budget").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should clamp budget to minimum 1024 tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0131()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > top-level reasoning (older models with budget-based thinking)::should clamp budget to minimum 1024 tokens").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0167()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should extract text response").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0168()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should extract usage").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0169()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should send additional response information").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should include stop_sequence in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0170()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should include stop_sequence in provider metadata").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should expose the raw response headers", Coverage = UpstreamCoverage.Partial)]
    public async Task Case_0178()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should expose the raw response headers").ConfigureAwait(false);
    }

}
