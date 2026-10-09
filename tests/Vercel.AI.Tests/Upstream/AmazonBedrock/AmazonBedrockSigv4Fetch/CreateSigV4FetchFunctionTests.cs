// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockSigv4Fetch;

public sealed class CreateSigV4FetchFunctionTests
{
    private const string Prefix = "packages/amazon-bedrock/src/amazon-bedrock-sigv4-fetch.test.ts::createSigV4FetchFunction::";
    private const string Body = "{\"test\": \"data\"}";
    private static readonly DateTimeOffset Now = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    [UpstreamTest(Prefix + "should bypass signing for non-POST requests", Coverage = UpstreamCoverage.Covered)]
    public void Should_bypass_signing_for_non_POST_requests()
    {
        var call = AmazonBedrockFetch.Prepare("http://example.com", "GET", null, null, null, null, null, Creds());
        Assert.False(call.Signed);
        Assert.Equal("GET", call.Method);
        Assert.Equal("http://example.com", call.Url);
        Assert.Equal(new[] { "user-agent" }, call.Headers.Keys);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should bypass signing if POST request has no body", Coverage = UpstreamCoverage.Covered)]
    public void Should_bypass_signing_if_POST_request_has_no_body()
    {
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", null, null, null, null, null, Creds());
        Assert.False(call.Signed);
        Assert.Equal("POST", call.Method);
        Assert.Equal(new[] { "user-agent" }, call.Headers.Keys);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle a POST request with a string body and merge signed headers including user-agent", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_a_POST_request_with_a_string_body_and_merge_signed_headers_including_user_agent()
    {
        var headers = new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["Custom-Header"] = "value", ["empty-header"] = "" };
        var credentials = new AmazonBedrockCredentials("us-west-2", "test-access-key", "test-secret", "test-session-token");
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", Body, null, null, headers, null, credentials, utcNow: Now);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("value", call.Headers["custom-header"]);
        Assert.False(call.Headers.ContainsKey("empty-header"));
        Assert.Equal("20240315T000000Z", call.Headers["x-amz-date"]);
        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=test-access-key/20240315/us-west-2/bedrock/aws4_request", call.Headers["authorization"]);
        Assert.Equal("test-session-token", call.Headers["x-amz-security-token"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
        Assert.Equal(Body, call.Body);
        Assert.True(call.Signed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle a POST request with a Request object", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_a_POST_request_with_a_Request_object()
    {
        var requestHeaders = new Dictionary<string, string?> { ["X-From-Request"] = "from-request" };
        var headers = new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["Custom-Header"] = "value" };
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", Body, null, null, headers, requestHeaders, Creds(), utcNow: Now);
        Assert.Equal("http://example.com", call.Url);
        Assert.Equal(Body, call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("value", call.Headers["custom-header"]);
        Assert.Equal("from-request", call.Headers["x-from-request"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should sign when input is a POST Request with body and no init", Coverage = UpstreamCoverage.Covered)]
    public void Should_sign_when_input_is_a_POST_Request_with_body_and_no_init()
    {
        var requestHeaders = new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["X-From-Request"] = "from-request" };
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", Body, null, null, null, requestHeaders, Creds(), utcNow: Now);
        Assert.Equal(Body, call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("from-request", call.Headers["x-from-request"]);
        Assert.Equal("20240315T000000Z", call.Headers["x-amz-date"]);
        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=test-access-key/20240315/us-west-2/bedrock/aws4_request", call.Headers["authorization"]);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
        Assert.True(call.Signed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle non-string body by stringifying it", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_non_string_body_by_stringifying_it()
    {
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", null, null, JsonNode.Parse("{\"field\":\"value\"}"), new Dictionary<string, string?>(), null, Creds());
        Assert.Equal("{\"field\":\"value\"}", call.Body);
        Assert.True(call.Signed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle Uint8Array body", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_Uint8Array_body()
    {
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", null, Encoding.UTF8.GetBytes("binaryTest"), null, new Dictionary<string, string?>(), null, Creds());
        Assert.Equal("binaryTest", call.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle ArrayBuffer body", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_ArrayBuffer_body()
    {
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", null, Encoding.UTF8.GetBytes("bufferTest"), null, new Dictionary<string, string?>(), null, Creds());
        Assert.Equal("bufferTest", call.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract headers from a Headers instance", Coverage = UpstreamCoverage.Covered)]
    public void Should_extract_headers_from_a_Headers_instance()
    {
        var headers = new Dictionary<string, string?> { ["A"] = "value-a", ["B"] = "value-b" };
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", Body, null, null, headers, null, Creds());
        Assert.Equal("value-a", call.Headers["a"]);
        Assert.Equal("value-b", call.Headers["b"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle headers provided as an array", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_headers_provided_as_an_array()
    {
        var headers = new Dictionary<string, string?> { ["Array-Header"] = "array-value", ["Another-Header"] = "another-value" };
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", Body, null, null, headers, null, Creds(), utcNow: Now);
        Assert.Equal("array-value", call.Headers["array-header"]);
        Assert.Equal("another-value", call.Headers["another-header"]);
        Assert.Equal("20240315T000000Z", call.Headers["x-amz-date"]);
        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=test-access-key/20240315/us-west-2/bedrock/aws4_request", call.Headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call original fetch if init is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_call_original_fetch_if_init_is_undefined()
    {
        var call = AmazonBedrockFetch.Prepare("http://example.com", null, null, null, null, null, null, Creds());
        Assert.Equal("GET", call.Method);
        Assert.False(call.Signed);
        Assert.Equal(new[] { "user-agent" }, call.Headers.Keys);
        Assert.Equal(AmazonBedrockFetch.UserAgent, call.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should correctly handle async credential providers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_correctly_handle_async_credential_providers()
    {
        var fetch = new AmazonBedrockSigV4Fetch(async _ =>
        {
            await Task.Yield();
            return new AmazonBedrockCredentials("us-east-1", "async-access-key", "async-secret-key", "async-session-token");
        })
        {
            UtcNow = () => Now,
        };
        AmazonBedrockPreparedCall? sent = null;
        fetch.Transport = call => sent = call;

        await fetch.SendAsync("http://example.com", "POST", "{\"test\": \"async\"}", new Dictionary<string, string?> { ["Content-Type"] = "application/json" });

        var headers = Assert.IsAssignableFrom<AmazonBedrockPreparedCall>(sent).Headers;
        Assert.Equal("20240315T000000Z", headers["x-amz-date"]);
        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=async-access-key/20240315/us-east-1/bedrock/aws4_request", headers["authorization"]);
        Assert.Equal("async-session-token", headers["x-amz-security-token"]);
        Assert.Equal("application/json", headers["content-type"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle async credential providers that reject", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_async_credential_providers_that_reject()
    {
        var calls = 0;
        var fetch = new AmazonBedrockSigV4Fetch(async _ =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Failed to get credentials");
        });
        fetch.Transport = call =>
        {
            calls++;
            return call;
        };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => fetch.SendAsync("http://example.com", "POST", Body, null));

        Assert.Equal("Failed to get credentials", error.Message);
        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use default service name \"bedrock\" when no service parameter is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_default_service_name_bedrock_when_no_service_parameter_is_provided()
    {
        var fetch = new AmazonBedrockSigV4Fetch(_ => Task.FromResult(Creds()));
        var call = await fetch.SendAsync("http://example.com", "POST", Body, null);
        Assert.Contains("/us-west-2/bedrock/aws4_request", call.Headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use custom service name when service parameter is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_custom_service_name_when_service_parameter_is_provided()
    {
        var fetch = new AmazonBedrockSigV4Fetch(_ => Task.FromResult(Creds()), "bedrock-mantle");
        var call = await fetch.SendAsync("http://example.com", "POST", Body, null);
        Assert.Contains("/us-west-2/bedrock-mantle/aws4_request", call.Headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send non-ASCII header values without signing them", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_non_ASCII_header_values_without_signing_them()
    {
        var headers = new Dictionary<string, string?> { ["x-ascii"] = "plain", ["x-title"] = "Example · App" };
        var call = AmazonBedrockFetch.Prepare("http://example.com", "POST", Body, null, null, headers, null, Creds(), utcNow: Now);
        var signedHeaders = Regex.Match(call.Headers["authorization"], "SignedHeaders=([^,]+)").Groups[1].Value.Split(';');
        Assert.Equal(new[] { "host", "user-agent", "x-amz-date", "x-ascii" }, signedHeaders);
        Assert.Equal("plain", call.Headers["x-ascii"]);
        Assert.Equal("Example · App", call.Headers["x-title"]);
    }

    private static AmazonBedrockCredentials Creds() => new AmazonBedrockCredentials("us-west-2", "test-access-key", "test-secret");
}
