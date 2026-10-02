// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mcp;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.Mcp;

public sealed class McpProtocolTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::validates JSON-RPC requests",
        Coverage = UpstreamCoverage.Covered)]
    public void Json_rpc_accepts_a_request()
    {
        var message = JsonRpcMessages.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\"}");
        Assert.Equal("2.0", message.GetProperty("jsonrpc").GetString());
        Assert.Equal(1, message.GetProperty("id").GetInt32());
        Assert.Equal("tools/list", message.GetProperty("method").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::validates JSON-RPC responses",
        Coverage = UpstreamCoverage.Covered)]
    public void Json_rpc_accepts_a_response()
    {
        var message = JsonRpcMessages.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"tools\":[]}}");
        Assert.Equal(0, message.GetProperty("result").GetProperty("tools").GetArrayLength());
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::rejects invalid JSON-RPC messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Json_rpc_rejects_the_wrong_version()
    {
        using var document = JsonDocument.Parse("{\"jsonrpc\":\"1.0\",\"id\":1,\"result\":{\"tools\":[]}}");
        var error = Assert.Throws<AiSdkException>(() => JsonRpcMessages.Validate(document.RootElement));
        Assert.Contains("2.0", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::does not export JSON-RPC schemas",
        Coverage = UpstreamCoverage.Covered)]
    public void Json_rpc_schemas_are_not_exported()
    {
        var names = typeof(JsonRpcMessages).Assembly.GetExportedTypes().Select(type => type.Name).ToArray();
        Assert.DoesNotContain("JSONRPCMessageSchema", names);
        Assert.DoesNotContain("JSONRPCRequestSchema", names);
        Assert.DoesNotContain("JSONRPCResponseSchema", names);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::normalizes structured-only results with %s content",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_synthesizes_text_for_structured_content()
    {
        AssertStructured("{\"value\":42}", "{\"value\":42}");
        AssertStructured("[1,\"two\",false]", "[1,\"two\",false]");
        AssertStructured("\"result\"", "\"result\"");
        AssertStructured("42", "42");
        AssertStructured("true", "true");
        AssertStructured("null", "null");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::preserves structured-only error results",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_keeps_a_structured_error()
    {
        using var document = JsonDocument.Parse("{\"structuredContent\":{\"code\":\"NOT_FOUND\"},\"isError\":true}");
        JsonAssert.Equal(
            CallToolResults.Normalize(document.RootElement),
            "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"code\\\":\\\"NOT_FOUND\\\"}\"}],\"structuredContent\":{\"code\":\"NOT_FOUND\"},\"isError\":true}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::preserves results that already contain content",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_adds_is_error_when_content_is_present()
    {
        using var document = JsonDocument.Parse("{\"content\":[{\"type\":\"text\",\"text\":\"Existing content\"}],\"structuredContent\":{\"value\":42}}");
        JsonAssert.Equal(
            CallToolResults.Normalize(document.RootElement),
            "{\"content\":[{\"type\":\"text\",\"text\":\"Existing content\"}],\"structuredContent\":{\"value\":42},\"isError\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects results without content, structuredContent, or toolResult",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_rejects_an_empty_object()
    {
        Assert.Throws<AiSdkException>(() => Normalize("{}"));
        Assert.Throws<AiSdkException>(() => Normalize("{\"_meta\":{}}"));
        Assert.Throws<AiSdkException>(() => Normalize("{\"value\":42}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::accepts legacy toolResult responses",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_returns_a_legacy_tool_result()
    {
        using var document = JsonDocument.Parse("{\"toolResult\":{\"value\":42}}");
        JsonAssert.Equal(CallToolResults.Normalize(document.RootElement), "{\"toolResult\":{\"value\":42}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects malformed known content types",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_rejects_malformed_content()
    {
        Assert.Throws<AiSdkException>(() => Normalize("{\"content\":[{\"type\":\"text\"}]}"));
        Assert.Throws<AiSdkException>(() => Normalize("{\"content\":[{\"type\":\"image\",\"data\":\"not-base64\",\"mimeType\":\"image/png\"}]}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects non-JSON structured content",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_rejects_values_that_are_not_json()
    {
        Assert.False(CallToolResults.IsJsonValue(CallToolResults.Undefined));
        Assert.False(CallToolResults.IsJsonValue(double.NaN));
        Assert.False(CallToolResults.IsJsonValue(double.PositiveInfinity));
        Assert.False(CallToolResults.IsJsonValue(new System.Numerics.BigInteger(1)));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects cyclic structured content",
        Coverage = UpstreamCoverage.Covered)]
    public void Call_tool_result_rejects_a_cycle()
    {
        Assert.ThrowsAny<Exception>(() =>
        {
            var structured = new JsonObject();
            structured["self"] = structured;
            CallToolResults.Normalize(structured);
        });
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::extracts statically reachable header bindings",
        Coverage = UpstreamCoverage.Covered)]
    public void Header_bindings_follow_object_properties()
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"},\"options\":{\"type\":\"object\",\"properties\":{\"dryRun\":{\"type\":\"boolean\",\"x-mcp-header\":\"Dry-Run\"}}}}}");
        var result = McpToolHeaders.GetBindings(document.RootElement);
        Assert.True(result.Success);
        Assert.Equal("Region", result.Bindings[0].HeaderName);
        Assert.Equal(new[] { "region" }, result.Bindings[0].Path);
        Assert.Equal("string", result.Bindings[0].ValueType);
        Assert.Equal("Dry-Run", result.Bindings[1].HeaderName);
        Assert.Equal(new[] { "options", "dryRun" }, result.Bindings[1].Path);
        Assert.Equal("boolean", result.Bindings[1].ValueType);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::rejects invalid x-mcp-header schemas",
        Coverage = UpstreamCoverage.Covered)]
    public void Header_bindings_reject_unreachable_and_duplicate_annotations()
    {
        AssertHeaderError("{\"type\":\"object\",\"items\":{\"type\":\"string\",\"x-mcp-header\":\"Invalid\"}}", "not on a statically reachable property");
        AssertHeaderError("{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"}}}}", "not on a statically reachable property");
        AssertHeaderError("{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"}}}}", "not on a statically reachable property");
        AssertHeaderError("{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"number\",\"x-mcp-header\":\"Count\"}}}", "can only annotate boolean, integer, or string");
        AssertHeaderError("{\"type\":\"object\",\"properties\":{\"first\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"},\"second\":{\"type\":\"string\",\"x-mcp-header\":\"region\"}}}", "is not unique");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::creates encoded headers from tool arguments",
        Coverage = UpstreamCoverage.Covered)]
    public void Header_bindings_encode_argument_values()
    {
        using var schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"},\"count\":{\"type\":\"integer\",\"x-mcp-header\":\"Count\"},\"options\":{\"type\":\"object\",\"properties\":{\"enabled\":{\"type\":\"boolean\",\"x-mcp-header\":\"Enabled\"}}}}}");
        var bindings = McpToolHeaders.GetBindings(schema.RootElement);
        using var arguments = JsonDocument.Parse("{\"region\":\"Hello, 世界\",\"count\":42,\"options\":{\"enabled\":false}}");
        var headers = McpToolHeaders.CreateHeaders(bindings.Bindings, arguments.RootElement);
        Assert.Equal("=?base64?SGVsbG8sIOS4lueVjA==?=", headers["Mcp-Param-Region"]);
        Assert.Equal("42", headers["Mcp-Param-Count"]);
        Assert.Equal("false", headers["Mcp-Param-Enabled"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::encodes MCP header values safely",
        Coverage = UpstreamCoverage.Covered)]
    public void Header_values_stay_plain_or_use_the_base64_sentinel()
    {
        Assert.Equal("plain-ascii", McpToolHeaders.EncodeValue("plain-ascii"));
        Assert.Equal("=?base64?IHBhZGRlZCA=?=", McpToolHeaders.EncodeValue(" padded "));
        Assert.Equal("=?base64?PT9iYXNlNjQ/bGl0ZXJhbD89?=", McpToolHeaders.EncodeValue("=?base64?literal?="));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::fingerprintMCPAppResource::produces a stable digest for equal resources",
        Coverage = UpstreamCoverage.Covered)]
    public void Fingerprint_is_stable()
    {
        using var csp = JsonDocument.Parse("{\"connect\":[\"self\"]}");
        using var permissions = JsonDocument.Parse("{\"camera\":true}");
        var first = McpAppFingerprint.Fingerprint("<p>hi</p>", csp.RootElement, permissions.RootElement);
        var second = McpAppFingerprint.Fingerprint("<p>hi</p>", csp.RootElement, permissions.RootElement);
        Assert.Equal(first, second);
        Assert.False(McpAppFingerprint.DetectDrift(first, second));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::fingerprintMCPAppResource::ignores key ordering in csp / permissions",
        Coverage = UpstreamCoverage.Covered)]
    public void Fingerprint_ignores_object_key_order()
    {
        using var leftCsp = JsonDocument.Parse("{\"a\":1,\"b\":2}");
        using var rightCsp = JsonDocument.Parse("{\"b\":2,\"a\":1}");
        using var leftPermissions = JsonDocument.Parse("{\"camera\":true,\"mic\":false}");
        using var rightPermissions = JsonDocument.Parse("{\"mic\":false,\"camera\":true}");
        Assert.Equal(
            McpAppFingerprint.Fingerprint("<p></p>", leftCsp.RootElement, leftPermissions.RootElement),
            McpAppFingerprint.Fingerprint("<p></p>", rightCsp.RootElement, rightPermissions.RootElement));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::fingerprintMCPAppResource::changes when html, csp, or permissions mutate",
        Coverage = UpstreamCoverage.Covered)]
    public void Fingerprint_changes_when_the_resource_changes()
    {
        using var csp = JsonDocument.Parse("{\"a\":1}");
        using var otherCsp = JsonDocument.Parse("{\"a\":2}");
        using var permissions = JsonDocument.Parse("{\"camera\":true}");
        using var otherPermissions = JsonDocument.Parse("{\"camera\":false}");
        var baseline = McpAppFingerprint.Fingerprint("<p>a</p>", csp.RootElement, permissions.RootElement);
        Assert.NotEqual(baseline, McpAppFingerprint.Fingerprint("<p>b</p>", csp.RootElement, permissions.RootElement));
        Assert.NotEqual(baseline, McpAppFingerprint.Fingerprint("<p>a</p>", otherCsp.RootElement, permissions.RootElement));
        Assert.NotEqual(baseline, McpAppFingerprint.Fingerprint("<p>a</p>", csp.RootElement, otherPermissions.RootElement));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::detectMCPAppResourceDrift::flags a changed fingerprint",
        Coverage = UpstreamCoverage.Covered)]
    public void Fingerprint_drift_is_a_string_difference()
    {
        Assert.True(McpAppFingerprint.DetectDrift("current", "baseline"));
        Assert.False(McpAppFingerprint.DetectDrift("same", "same"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceUrlFromServerUrl::should remove fragments",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_url_drops_the_fragment()
    {
        Assert.Equal("https://example.com/path", OAuthResources.ResourceUrlFromServerUrl("https://example.com/path#fragment"));
        Assert.Equal("https://example.com/", OAuthResources.ResourceUrlFromServerUrl("https://example.com#fragment"));
        Assert.Equal("https://example.com/path?query=1", OAuthResources.ResourceUrlFromServerUrl("https://example.com/path?query=1#fragment"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceUrlFromServerUrl::should return URL unchanged if no fragment",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_url_keeps_a_url_without_a_fragment()
    {
        Assert.Equal("https://example.com/", OAuthResources.ResourceUrlFromServerUrl("https://example.com"));
        Assert.Equal("https://example.com/path", OAuthResources.ResourceUrlFromServerUrl("https://example.com/path"));
        Assert.Equal("https://example.com/path?query=1", OAuthResources.ResourceUrlFromServerUrl("https://example.com/path?query=1"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceUrlFromServerUrl::should keep everything else unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_url_normalizes_host_and_default_port()
    {
        Assert.Equal("https://example.com/PATH", OAuthResources.ResourceUrlFromServerUrl("https://EXAMPLE.COM/PATH"));
        Assert.Equal("https://example.com/path", OAuthResources.ResourceUrlFromServerUrl("https://example.com:443/path"));
        Assert.Equal("https://example.com:8080/path", OAuthResources.ResourceUrlFromServerUrl("https://example.com:8080/path"));
        Assert.Equal("https://example.com/?foo=bar&baz=qux", OAuthResources.ResourceUrlFromServerUrl("https://example.com?foo=bar&baz=qux"));
        Assert.Equal("https://example.com/", OAuthResources.ResourceUrlFromServerUrl("https://example.com/"));
        Assert.Equal("https://example.com/path/", OAuthResources.ResourceUrlFromServerUrl("https://example.com/path/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should match identical URLs",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_match_accepts_the_same_url()
    {
        Assert.True(OAuthResources.CheckResourceAllowed("https://example.com/path", "https://example.com/path"));
        Assert.True(OAuthResources.CheckResourceAllowed("https://example.com/", "https://example.com/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs with different paths",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_match_rejects_a_different_path()
    {
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com/path1", "https://example.com/path2"));
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com/", "https://example.com/path"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs with different domains",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_match_rejects_a_different_host()
    {
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com/path", "https://example.org/path"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs with different ports",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_match_rejects_a_different_port()
    {
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com:8080/path", "https://example.com/path"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs where one path is a sub-path of another",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_match_requires_a_path_boundary()
    {
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com/mcpxxxx", "https://example.com/mcp"));
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com/folder", "https://example.com/folder/subfolder"));
        Assert.True(OAuthResources.CheckResourceAllowed("https://example.com/api/v1", "https://example.com/api"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should handle trailing slashes vs no trailing slashes",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_match_treats_a_trailing_slash_as_the_same_path()
    {
        Assert.True(OAuthResources.CheckResourceAllowed("https://example.com/mcp/", "https://example.com/mcp"));
        Assert.False(OAuthResources.CheckResourceAllowed("https://example.com/folder", "https://example.com/folder/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > message handling::should handle partial messages correctly",
        Coverage = UpstreamCoverage.Covered)]
    public void Ndjson_reassembles_a_split_line()
    {
        var message = "{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"method\":\"test\",\"params\":{}}";
        var buffer = new McpNdjson.Buffer();
        buffer.Append(Encoding.UTF8.GetBytes(message.Substring(0, 10)));
        Assert.Null(buffer.ReadLine());
        buffer.Append(Encoding.UTF8.GetBytes(message.Substring(10) + "\n"));
        Assert.Equal(message, buffer.ReadLine());
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > deserializeMessage::should reject payloads containing __proto__ (prototype pollution)",
        Coverage = UpstreamCoverage.Covered)]
    public void Ndjson_rejects_proto()
    {
        var text = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"__proto__\":{\"polluted\":true}}}";
        var error = Assert.Throws<JsonParseException>(() => JsonRpcMessages.Parse(text));
        Assert.Contains(text, error.Message);
        Assert.Contains("Object contains forbidden prototype property", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > deserializeMessage::should reject payloads containing constructor.prototype",
        Coverage = UpstreamCoverage.Covered)]
    public void Ndjson_rejects_constructor_prototype()
    {
        var text = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"constructor\":{\"prototype\":{\"polluted\":true}}}}";
        var error = Assert.Throws<JsonParseException>(() => JsonRpcMessages.Parse(text));
        Assert.Contains("Object contains forbidden prototype property", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > send::should successfully send a message",
        Coverage = UpstreamCoverage.Covered)]
    public void Ndjson_writes_one_json_line()
    {
        using var document = JsonDocument.Parse("{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"method\":\"test\",\"params\":{}}");
        using var stream = new MemoryStream();
        McpNdjson.Write(stream, document.RootElement);
        Assert.Equal(document.RootElement.GetRawText() + "\n", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > send::should throw error if transport is not connected",
        Coverage = UpstreamCoverage.Covered)]
    public void Ndjson_write_requires_a_stream()
    {
        using var document = JsonDocument.Parse("{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"method\":\"test\",\"params\":{}}");
        var error = Assert.Throws<AiSdkException>(() => McpNdjson.Write(null, document.RootElement));
        Assert.Contains("StdioClientTransport not connected", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/get-environment.test.ts::getEnvironment::should not mutate the original custom environment object",
        Coverage = UpstreamCoverage.Covered)]
    public void Stdio_environment_copies_the_custom_dictionary()
    {
        var custom = new Dictionary<string, string> { ["CUSTOM_VAR"] = "custom_value" };
        var result = StdioEnvironment.GetEnvironment(custom);
        Assert.Equal("custom_value", custom["CUSTOM_VAR"]);
        Assert.Equal(1, custom.Count);
        Assert.NotSame(custom, result);
        Assert.Equal("custom_value", result["CUSTOM_VAR"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/create-child-process.test.ts::createChildProcess::should reject line breaks in arguments on Windows",
        Coverage = UpstreamCoverage.Covered)]
    public void Stdio_command_rejects_line_breaks_on_windows()
    {
        var error = Assert.Throws<ArgumentException>(() => StdioEnvironment.ValidateCommand("npx", new[] { "safe\r\necho unsafe" }, windows: true));
        Assert.Equal("Stdio MCP commands and arguments must not contain line breaks on Windows.", error.Message);
        StdioEnvironment.ValidateCommand("npx", new[] { "safe\r\necho unsafe" }, windows: false);
    }

    private static void AssertStructured(string value, string text)
    {
        using var document = JsonDocument.Parse("{\"structuredContent\":" + value + "}");
        var normalized = CallToolResults.Normalize(document.RootElement);
        Assert.Equal(text, normalized.GetProperty("content")[0].GetProperty("text").GetString());
        Assert.False(normalized.GetProperty("isError").GetBoolean());
        JsonAssert.Equal(normalized.GetProperty("structuredContent"), value);
    }

    private static JsonElement Normalize(string json)
    {
        using var document = JsonDocument.Parse(json);
        return CallToolResults.Normalize(document.RootElement);
    }

    private static void AssertHeaderError(string json, string expected)
    {
        using var document = JsonDocument.Parse(json);
        var result = McpToolHeaders.GetBindings(document.RootElement);
        Assert.False(result.Success);
        Assert.Contains(expected, result.Error ?? string.Empty);
    }
}
