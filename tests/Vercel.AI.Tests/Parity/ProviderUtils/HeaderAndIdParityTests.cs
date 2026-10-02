// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class HeaderAndIdParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::returns empty object for undefined", Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_returns_empty_for_undefined()
    {
        Assert.Empty(HeaderFunctions.NormalizeHeaders(JsUndefined.Value));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts Headers instance to record", Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_lowercases_http_headers()
    {
        var request = new HttpRequestMessage();
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer token");
        request.Headers.TryAddWithoutValidation("X-Feature", "beta");
        var result = HeaderFunctions.NormalizeHeaders(request.Headers);
        Assert.Equal("Bearer token", result["authorization"]);
        Assert.Equal("beta", result["x-feature"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts tuple array", Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_reads_pairs_and_drops_undefined()
    {
        var pairs = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("Authorization", "Bearer token"),
            new KeyValuePair<string, string?>("X-Feature", "beta"),
            new KeyValuePair<string, string?>("X-Ignore", null),
        };
        var result = HeaderFunctions.NormalizeHeaders(pairs);
        Assert.Equal("Bearer token", result["authorization"]);
        Assert.Equal("beta", result["x-feature"]);
        Assert.False(result.ContainsKey("x-ignore"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts plain record and filters nullish values", Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_filters_undefined_record_values()
    {
        var headers = new Dictionary<string, object?>
        {
            ["Authorization"] = "Bearer token",
            ["X-Feature"] = JsUndefined.Value,
            ["Content-Type"] = "application/json",
        };
        var result = HeaderFunctions.NormalizeHeaders(headers);
        Assert.Equal("Bearer token", result["authorization"]);
        Assert.Equal("application/json", result["content-type"]);
        Assert.False(result.ContainsKey("x-feature"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::handles empty Headers instance", Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_handles_an_empty_header_collection()
    {
        Assert.Empty(HeaderFunctions.NormalizeHeaders(new HttpRequestMessage().Headers));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts uppercase keys to lowercase", Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_lowercases_keys()
    {
        var result = HeaderFunctions.NormalizeHeaders(new Dictionary<string, string>
        {
            ["CONTENT-TYPE"] = "application/json",
            ["X-CUSTOM-HEADER"] = "test-value",
        });
        Assert.Equal("application/json", result["content-type"]);
        Assert.Equal("test-value", result["x-custom-header"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should create a new user-agent header when no existing user-agent exists", Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_creates_a_header()
    {
        var result = HeaderFunctions.WithUserAgentSuffix(
            new Dictionary<string, string>
            {
                ["content-type"] = "application/json",
                ["authorization"] = "Bearer token123",
            },
            "ai-sdk/0.0.0-test",
            "provider/test-openai");
        Assert.Equal("ai-sdk/0.0.0-test provider/test-openai", result["user-agent"]);
        Assert.Equal("application/json", result["content-type"]);
        Assert.Equal("Bearer token123", result["authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should append suffix parts to existing user-agent header", Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_appends_to_the_current_value()
    {
        var result = HeaderFunctions.WithUserAgentSuffix(
            new Dictionary<string, string>
            {
                ["user-agent"] = "TestApp/0.0.0-test",
                ["accept"] = "application/json",
            },
            "ai-sdk/0.0.0-test",
            "provider/test-anthropic");
        Assert.Equal("TestApp/0.0.0-test ai-sdk/0.0.0-test provider/test-anthropic", result["user-agent"]);
        Assert.Equal("application/json", result["accept"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should automatically remove undefined entries from headers", Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_drops_nullish_headers()
    {
        var result = HeaderFunctions.WithUserAgentSuffix(
            new Dictionary<string, object?>
            {
                ["content-type"] = "application/json",
                ["authorization"] = JsUndefined.Value,
                ["user-agent"] = "TestApp/0.0.0-test",
                ["accept"] = "application/json",
                ["cache-control"] = null,
            },
            "ai-sdk/0.0.0-test");
        Assert.Equal("TestApp/0.0.0-test ai-sdk/0.0.0-test", result["user-agent"]);
        Assert.Equal("application/json", result["content-type"]);
        Assert.Equal("application/json", result["accept"]);
        Assert.False(result.ContainsKey("authorization"));
        Assert.False(result.ContainsKey("cache-control"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should preserve headers when given a Headers instance", Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_preserves_an_http_header_collection()
    {
        var request = new HttpRequestMessage();
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer token123");
        request.Headers.TryAddWithoutValidation("X-Custom", "value");
        var result = HeaderFunctions.WithUserAgentSuffix(request.Headers, "ai-sdk/0.0.0-test");
        Assert.Equal("Bearer token123", result["authorization"]);
        Assert.Equal("value", result["x-custom"]);
        Assert.Equal("ai-sdk/0.0.0-test", result["user-agent"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should handle array header entries", Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_reads_header_pairs()
    {
        var pairs = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("Authorization", "Bearer token123"),
            new KeyValuePair<string, string?>("X-Feature", "alpha"),
        };
        var result = HeaderFunctions.WithUserAgentSuffix(pairs, "ai-sdk/0.0.0-test");
        Assert.Equal("Bearer token123", result["authorization"]);
        Assert.Equal("alpha", result["x-feature"]);
        Assert.Equal("ai-sdk/0.0.0-test", result["user-agent"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/generate-id.test.ts::createIdGenerator::should generate an ID with the correct length", Coverage = UpstreamCoverage.Covered)]
    public void Id_generator_uses_the_requested_size()
    {
        Assert.Equal(10, IdGenerators.CreateIdGenerator(size: 10)().Length);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/generate-id.test.ts::createIdGenerator::should generate an ID with the correct default length", Coverage = UpstreamCoverage.Covered)]
    public void Id_generator_defaults_to_sixteen_characters()
    {
        Assert.Equal(16, IdGenerators.CreateIdGenerator()().Length);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/generate-id.test.ts::createIdGenerator::should throw an error if the separator is part of the alphabet", Coverage = UpstreamCoverage.Covered)]
    public void Id_generator_rejects_a_separator_that_is_in_the_alphabet()
    {
        var error = Assert.Throws<InvalidArgumentError>(() => IdGenerators.CreateIdGenerator(prefix: "b", separator: "a"));
        Assert.True(InvalidArgumentError.IsInstance(error));
        Assert.Equal("separator", error.Argument);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/generate-id.test.ts::generateId::should generate unique IDs", Coverage = UpstreamCoverage.Covered)]
    public void Generate_id_returns_distinct_values()
    {
        Assert.NotEqual(IdGenerators.GenerateId(), IdGenerators.GenerateId());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-base-url.test.ts::validateBaseURL::returns a valid baseURL", Coverage = UpstreamCoverage.Covered)]
    public void Validate_base_url_returns_the_value()
    {
        Assert.Equal("https://example.com/", BaseUrls.ValidateBaseUrl("https://example.com/"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-base-url.test.ts::validateBaseURL::returns undefined when the baseURL is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Validate_base_url_returns_null_when_omitted()
    {
        Assert.Null(BaseUrls.ValidateBaseUrl(null));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-base-url.test.ts::validateBaseURL::throws an InvalidArgumentError for an empty baseURL", Coverage = UpstreamCoverage.Covered)]
    public void Validate_base_url_rejects_empty_and_whitespace()
    {
        foreach (var value in new[] { "", "   " })
        {
            var error = Assert.Throws<InvalidArgumentError>(() => BaseUrls.ValidateBaseUrl(value));
            Assert.True(InvalidArgumentError.IsInstance(error));
            Assert.Equal("AI_InvalidArgumentError", error.Name);
            Assert.Equal("baseURL", error.Argument);
            Assert.Equal("baseURL must be a non-empty string.", error.Message);
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::returns the input unchanged when neither startLine nor endLine is set", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_returns_the_input_when_bounds_are_omitted()
    {
        Assert.Equal("a\nb\nc", Lines.ExtractLines("a\nb\nc"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::slices a 1-based inclusive range from a \\n file", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_slices_an_lf_range()
    {
        Assert.Equal("b\nc", Lines.ExtractLines("a\nb\nc\nd", 2, 3));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::preserves \\r\\n line endings", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_preserves_crlf()
    {
        Assert.Equal("b\r\nc", Lines.ExtractLines("a\r\nb\r\nc\r\nd", 2, 3));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::preserves \\r line endings", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_preserves_cr()
    {
        Assert.Equal("b\rc", Lines.ExtractLines("a\rb\rc\rd", 2, 3));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::treats endLine past EOF as the last line", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_clamps_the_end_to_the_last_line()
    {
        Assert.Equal("b\nc", Lines.ExtractLines("a\nb\nc", 2, 99));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::defaults startLine to 1 when only endLine is set", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_defaults_the_start()
    {
        Assert.Equal("a\nb", Lines.ExtractLines("a\nb\nc", endLine: 2));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::defaults endLine to the last line when only startLine is set", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_defaults_the_end()
    {
        Assert.Equal("b\nc", Lines.ExtractLines("a\nb\nc", startLine: 2));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/extract-lines.test.ts::extractLines::returns input unchanged when there are no line breaks", Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_returns_a_single_line()
    {
        Assert.Equal("one-liner", Lines.ExtractLines("one-liner", 1, 1));
    }
}
