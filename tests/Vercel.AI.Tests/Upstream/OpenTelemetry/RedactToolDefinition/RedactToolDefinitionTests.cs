// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.RedactToolDefinition;

public sealed class RedactToolDefinitionTests
{
    private const string Test =
        "packages/otel/src/redact-tool-definition.test.ts::redactToolDefinition::";

    public static TheoryData<string> DefinitionsWithoutDomainSecrets =>
        new()
        {
            """{"type":"provider","id":"openai.code_interpreter","args":null}""",
            """{"type":"provider","id":"openai.code_interpreter","args":{}}""",
            """{"type":"provider","id":"openai.code_interpreter","args":{"container":"cntr_existing"}}""",
            """{"type":"provider","id":"openai.code_interpreter","args":{"container":{}}}""",
            """{"type":"provider","id":"openai.code_interpreter","args":{"container":{"networkPolicy":{"type":"disabled"}}}}""",
            """{"type":"provider","id":"openai.code_interpreter","args":{"container":{"networkPolicy":{"type":"allowlist","domainSecrets":[]}}}}""",
        };

    private static JsonObject Obj(string json) => (JsonObject)JsonNode.Parse(json)!;

    private static void AssertRedacted(string tool, string expected)
    {
        var input = Obj(tool);
        var original = input.ToJsonString();
        var result = ToolDefinitionRedaction.RedactToolDefinition(input);
        Assert.True(JsonNode.DeepEquals(Obj(expected), result), "Actual: " + result.ToJsonString());
        Assert.Equal(original, input.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(DefinitionsWithoutDomainSecrets))]
    [UpstreamTest(
        Test + "preserves definitions without domain secrets: $args",
        Coverage = UpstreamCoverage.Partial,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71",
        Note = "The upstream case with undefined arguments has no JSON form."
    )]
    public void Preserves_definitions_without_domain_secrets(string tool)
    {
        var input = Obj(tool);
        Assert.True(
            JsonNode.DeepEquals(input, ToolDefinitionRedaction.RedactToolDefinition(input))
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "redacts shell environment domain secrets without mutating provider arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Redacts_shell_environment_domain_secrets_without_mutating_provider_arguments()
    {
        AssertRedacted(
            """
            {"type":"provider","id":"openai.shell","name":"terminal","args":{"environment":{"type":"containerAuto","memoryLimit":"4g","networkPolicy":{"type":"allowlist","allowedDomains":["example.com"],"domainSecrets":[{"domain":"example.com","name":"KEY","value":"shell-private-key"}]}}}}
            """,
            """
            {"type":"provider","id":"openai.shell","name":"terminal","args":{"environment":{"type":"containerAuto","memoryLimit":"4g","networkPolicy":{"type":"allowlist","allowedDomains":["example.com"],"domainSecrets":[{"domain":"example.com","name":"KEY","value":"[redacted]"}]}}}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Test
            + "redacts MCP authorization and all header values without mutating provider arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Redacts_MCP_authorization_and_all_header_values_without_mutating_provider_arguments()
    {
        AssertRedacted(
            """
            {"type":"provider","id":"openai.mcp","name":"remote_tools","args":{"serverLabel":"example","serverUrl":"https://example.com/mcp","allowedTools":["search"],"requireApproval":"always","authorization":"mcp-private-token","headers":{"Authorization":"Bearer mcp-private-token","X-Custom-Key":"mcp-private-key"}}}
            """,
            """
            {"type":"provider","id":"openai.mcp","name":"remote_tools","args":{"serverLabel":"example","serverUrl":"https://example.com/mcp","allowedTools":["search"],"requireApproval":"always","authorization":"[redacted]","headers":{"Authorization":"[redacted]","X-Custom-Key":"[redacted]"}}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves MCP definitions without credentials",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Preserves_MCP_definitions_without_credentials()
    {
        var tool = Obj(
            """{"type":"provider","id":"openai.mcp","args":{"serverLabel":"example","serverUrl":"https://example.com/mcp"}}"""
        );
        Assert.True(JsonNode.DeepEquals(tool, ToolDefinitionRedaction.RedactToolDefinition(tool)));
    }

    [Fact]
    [UpstreamTest(
        Test + "redacts Google MCP server header values without mutating provider arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Redacts_Google_MCP_server_header_values_without_mutating_provider_arguments()
    {
        AssertRedacted(
            """
            {"type":"provider","id":"google.mcp_server","name":"connector","args":{"name":"local-mcp","url":"https://example.com/mcp","allowedTools":["search"],"headers":{"Authorization":"Bearer mcp-private-token","X-Custom-Key":"mcp-private-key"}}}
            """,
            """
            {"type":"provider","id":"google.mcp_server","name":"connector","args":{"name":"local-mcp","url":"https://example.com/mcp","allowedTools":["search"],"headers":{"Authorization":"[redacted]","X-Custom-Key":"[redacted]"}}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves Google MCP definitions without credentials",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Preserves_Google_MCP_definitions_without_credentials()
    {
        var tool = Obj(
            """{"type":"provider","id":"google.mcp_server","args":{"url":"https://example.com/mcp"}}"""
        );
        Assert.Same(tool, ToolDefinitionRedaction.RedactToolDefinition(tool));
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves unrelated tool definitions",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Preserves_unrelated_tool_definitions()
    {
        const string args =
            """{"container":{"networkPolicy":{"domainSecrets":[{"value":"ordinary-input"}]}}}""";
        foreach (
            var json in new[]
            {
                """{"type":"function","name":"code_interpreter","args":""" + args + "}",
                """{"type":"provider","id":"other.tool","args":""" + args + "}",
            }
        )
        {
            var tool = Obj(json);
            Assert.Same(tool, ToolDefinitionRedaction.RedactToolDefinition(tool));
        }
    }

    [Fact]
    [UpstreamTest(
        Test + "redacts multiple credentials without mutating provider arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Redacts_multiple_credentials_without_mutating_provider_arguments()
    {
        AssertRedacted(
            """
            {"type":"provider","id":"openai.code_interpreter","name":"renamed_python","args":{"container":{"fileIds":["file-123"],"memoryLimit":"4g","networkPolicy":{"type":"allowlist","allowedDomains":["example.com"],"domainSecrets":[{"domain":"example.com","name":"API_KEY","value":"private-key"},{"domain":"example.com","name":"TOKEN","value":"private-token"}]}}}}
            """,
            """
            {"type":"provider","id":"openai.code_interpreter","name":"renamed_python","args":{"container":{"fileIds":["file-123"],"memoryLimit":"4g","networkPolicy":{"type":"allowlist","allowedDomains":["example.com"],"domainSecrets":[{"domain":"example.com","name":"API_KEY","value":"[redacted]"},{"domain":"example.com","name":"TOKEN","value":"[redacted]"}]}}}}
            """
        );
    }
}
