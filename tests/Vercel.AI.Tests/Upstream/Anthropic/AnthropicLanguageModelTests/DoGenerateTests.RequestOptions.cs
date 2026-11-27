// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate</c>. Other parts of the class sit in the sibling files.</summary>
public sealed partial class DoGenerateTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should set effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_effort()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(handler, Options("""{"effort":"medium"}"""));

        AssertJson(HelloBody("""{"output_config":{"effort":"medium"}}"""), RequestBody(handler));
        AssertRequestHeaders(
            handler,
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should set speed and fast-mode beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_speed_and_fast_mode_beta_header()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(handler, Options("""{"speed":"fast"}"""));

        AssertJson(HelloBody("""{"speed":"fast"}"""), RequestBody(handler));
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "fast-mode-2026-02-01"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should set speed=standard without fast-mode beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_speed_standard_without_fast_mode_beta_header()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(handler, Options("""{"speed":"standard"}"""));

        AssertJson(HelloBody("""{"speed":"standard"}"""), RequestBody(handler));
        AssertRequestHeaders(
            handler,
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should set inference_geo in request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_inference_geo_in_request_body()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(handler, Options("""{"inferenceGeo":"us"}"""));

        AssertJson(HelloBody("""{"inference_geo":"us"}"""), RequestBody(handler));
        AssertRequestHeaders(
            handler,
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass cache_control to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_cache_control_to_request_body()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(
            handler,
            Options("""{"cacheControl":{"type":"ephemeral"}}""")
        );

        AssertJson(HelloBody("""{"cache_control":{"type":"ephemeral"}}"""), RequestBody(handler));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass cache_control with ttl to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_cache_control_with_ttl_to_request_body()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(
            handler,
            Options("""{"cacheControl":{"type":"ephemeral","ttl":"1h"}}""")
        );

        AssertJson(
            HelloBody("""{"cache_control":{"ttl":"1h","type":"ephemeral"}}"""),
            RequestBody(handler)
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass metadata with user_id to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_metadata_with_user_id_to_request_body()
    {
        var handler = JsonFixture("anthropic-text");

        var result = await DoGenerate(
            handler,
            Options("""{"metadata":{"userId":"test-user-id"}}""")
        );

        AssertJson(HelloBody("""{"metadata":{"user_id":"test-user-id"}}"""), RequestBody(handler));
        Assert.Empty(result.Warnings);
    }
}
