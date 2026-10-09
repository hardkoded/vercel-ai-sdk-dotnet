// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockSigv4Fetch;

public sealed class CreateApiKeyFetchFunctionTests
{
    private const string Prefix = "packages/amazon-bedrock/src/amazon-bedrock-sigv4-fetch.test.ts::createApiKeyFetchFunction::";
    private const string Body = "{\"test\": \"data\"}";

    [Fact]
    [UpstreamTest(Prefix + "should add Authorization header with Bearer token and user-agent", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_Authorization_header_with_Bearer_token_and_user_agent()
    {
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-123", "http://example.com", "POST", Body, Headers(("Content-Type", "application/json")));
        Assert.False(call.Signed);
        Assert.Equal("POST", call.Method);
        Assert.Equal(Body, call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("Bearer test-api-key-123", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge Authorization header with existing headers", Coverage = UpstreamCoverage.Covered)]
    public void Should_merge_Authorization_header_with_existing_headers()
    {
        var call = AmazonBedrockFetch.PrepareApiKey(
            "test-api-key-456",
            "http://example.com",
            "POST",
            Body,
            Headers(("Content-Type", "application/json"), ("Custom-Header", "custom-value"), ("X-Request-ID", "req-123")));
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("custom-value", call.Headers["custom-header"]);
        Assert.Equal("req-123", call.Headers["x-request-id"]);
        Assert.Equal("Bearer test-api-key-456", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work with Headers instance", Coverage = UpstreamCoverage.Covered)]
    public void Should_work_with_Headers_instance()
    {
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-789", "http://example.com", "POST", Body, Headers(("Content-Type", "application/json"), ("X-Custom", "value")));
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("value", call.Headers["x-custom"]);
        Assert.Equal("Bearer test-api-key-789", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work with headers as array", Coverage = UpstreamCoverage.Covered)]
    public void Should_work_with_headers_as_array()
    {
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-array", "http://example.com", "POST", Body, Headers(("Content-Type", "application/json"), ("X-Array-Header", "array-value")));
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("array-value", call.Headers["x-array-header"]);
        Assert.Equal("Bearer test-api-key-array", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work with GET requests", Coverage = UpstreamCoverage.Covered)]
    public void Should_work_with_GET_requests()
    {
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-get", "http://example.com", "GET", null, Headers(("Accept", "application/json")));
        Assert.False(call.Signed);
        Assert.Equal("GET", call.Method);
        Assert.Equal("application/json", call.Headers["accept"]);
        Assert.Equal("Bearer test-api-key-get", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work when no headers are provided", Coverage = UpstreamCoverage.Covered)]
    public void Should_work_when_no_headers_are_provided()
    {
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-no-headers", "http://example.com", "POST", Body, null);
        Assert.Equal("POST", call.Method);
        Assert.Equal(Body, call.Body);
        Assert.Equal(new[] { "user-agent", "authorization" }, call.Headers.Keys);
        Assert.Equal("Bearer test-api-key-no-headers", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work when init is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_work_when_init_is_undefined()
    {
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-undefined", "http://example.com", null, null, null);
        Assert.Equal(string.Empty, call.Method);
        Assert.Null(call.Body);
        Assert.Equal("Bearer test-api-key-undefined", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
        Assert.False(call.Signed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should override existing Authorization header", Coverage = UpstreamCoverage.Covered, Note = "Header names are case-insensitive in .NET, so the old token is replaced instead of kept under a second key.")]
    public void Should_override_existing_Authorization_header()
    {
        var call = AmazonBedrockFetch.PrepareApiKey(
            "test-api-key-override",
            "http://example.com",
            "POST",
            Body,
            Headers(("Content-Type", "application/json"), ("Authorization", "Bearer old-token")));
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("Bearer test-api-key-override", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use default fetch when no custom fetch provided", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_default_fetch_when_no_custom_fetch_provided()
    {
        var fetch = new AmazonBedrockApiKeyFetch("test-api-key-default");
        Assert.Null(fetch.Transport);
        var call = fetch.Send("http://example.com", "POST", Body, null);
        Assert.Equal("POST", call.Method);
        Assert.Equal(Body, call.Body);
        Assert.Equal("Bearer test-api-key-default", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve default fetch lazily when no custom fetch provided", Coverage = UpstreamCoverage.Covered)]
    public void Should_resolve_default_fetch_lazily_when_no_custom_fetch_provided()
    {
        var fetch = new AmazonBedrockApiKeyFetch("test-api-key-lazy");
        var patched = 0;
        fetch.Transport = call =>
        {
            patched++;
            return call;
        };
        var call = fetch.Send("http://example.com", "POST", Body, null);
        Assert.Equal(1, patched);
        Assert.Equal(Body, call.Body);
        Assert.Equal("Bearer test-api-key-lazy", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty string API key", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_empty_string_API_key()
    {
        var call = AmazonBedrockFetch.PrepareApiKey(string.Empty, "http://example.com", "POST", Body, null);
        Assert.Equal("Bearer ", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
        Assert.Equal(Body, call.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve request body and other properties", Coverage = UpstreamCoverage.Covered, Note = "The credentials and cache init properties have no .NET equivalent. Method, body, and headers are preserved.")]
    public void Should_preserve_request_body_and_other_properties()
    {
        var body = "{\"data\":\"test\"}";
        var call = AmazonBedrockFetch.PrepareApiKey("test-api-key-preserve", "http://example.com", "PUT", body, Headers(("Content-Type", "application/json")));
        Assert.Equal("PUT", call.Method);
        Assert.Equal(body, call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("Bearer test-api-key-preserve", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    private static Dictionary<string, string?> Headers(params (string Name, string Value)[] pairs)
    {
        var headers = new Dictionary<string, string?>();
        foreach (var (name, value) in pairs)
        {
            headers[name] = value;
        }

        return headers;
    }
}
