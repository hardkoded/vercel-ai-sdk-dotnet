// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; structured-outputs-2025-11-13 beta header</c>.</summary>
public sealed class DoGenerateStructuredOutputs20251113BetaHeaderTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > structured-outputs-2025-11-13 beta header::";

    private const string NameSchema =
        "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}";

    // Maps the requestHeaders snapshot: the headers the provider sets, without the ones the HTTP client adds.
    private static void AssertRequestHeaders(CaptureHandler handler, string? beta)
    {
        var expected = new Dictionary<string, string>
        {
            ["anthropic-version"] = "2023-06-01",
            ["content-type"] = "application/json",
            ["x-api-key"] = "test-api-key",
        };
        if (beta != null)
        {
            expected["anthropic-beta"] = beta;
        }

        var actual = handler
            .Headers.Where(header =>
                !string.Equals(header.Key, "user-agent", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(header.Key, "content-length", StringComparison.OrdinalIgnoreCase)
            )
            .ToDictionary(
                header => header.Key.ToLowerInvariant(),
                header => header.Value.Split(';')[0]
            );
        Assert.Equal(expected.OrderBy(pair => pair.Key), actual.OrderBy(pair => pair.Key));
    }

    private static LanguageModelCallOptions WithTool(bool? strict)
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "testTool",
                "A test tool",
                AnthropicParity.Json(
                    "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}"
                ),
                strict
            ),
        };
        return options;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should NOT include beta header for simple text generation with supported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_NOT_include_beta_header_for_simple_text_generation_with_supported_model()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            AnthropicParity.Hello()
        );

        AssertRequestHeaders(handler, null);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should NOT include beta header when using json schema response format (structured outputs are GA)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_NOT_include_beta_header_when_using_json_schema_response_format_structured_outputs_are_GA()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-output-format.1");
        var options = AnthropicParity.Hello();
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(NameSchema);

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            options
        );

        AssertRequestHeaders(handler, null);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should NOT include beta header when using json schema response format with unsupported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_NOT_include_beta_header_when_using_json_schema_response_format_with_unsupported_model()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(NameSchema);

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        AssertRequestHeaders(handler, null);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include beta header when using tools with strict: true on supported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_beta_header_when_using_tools_with_strict_true_on_supported_model()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            WithTool(true)
        );

        AssertRequestHeaders(handler, "structured-outputs-2025-11-13");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should include beta header when using tools with strict: false on supported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_beta_header_when_using_tools_with_strict_false_on_supported_model()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            WithTool(false)
        );

        AssertRequestHeaders(handler, "structured-outputs-2025-11-13");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include beta header when using tools without strict on supported model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_beta_header_when_using_tools_without_strict_on_supported_model()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            WithTool(null)
        );

        AssertRequestHeaders(handler, "structured-outputs-2025-11-13");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should NOT include beta header when using json response tool (jsonTool mode)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_NOT_include_beta_header_when_using_json_response_tool_jsonTool_mode()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-tool.1");
        var options = AnthropicParity.WithProvider("{\"structuredOutputMode\":\"jsonTool\"}");
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(NameSchema);

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            options
        );

        AssertRequestHeaders(handler, null);
    }
}
