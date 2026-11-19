// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class PrepareResponsesToolsTests
{
    [Fact]
    [UpstreamTest(
        Suite + "::should remove string propertyNames from function tools and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_remove_string_propertyNames_from_function_tools_and_warn()
    {
        var result = Prepare(
            null,
            Function(
                "get_weather",
                null,
                """{"type":"object","properties":{"values":{"type":"object","propertyNames":{"type":"string","format":"uuid"}}}}"""
            )
        );

        AssertTools(
            result,
            """[{"type":"function","name":"get_weather","parameters":{"type":"object","properties":{"values":{"type":"object"}}},"strict":false}]""",
            expectedWarnings: true
        );
        AssertNoToolChoice(result);
        AssertWarnings(
            result,
            (
                "compatibility",
                "JSON Schema propertyNames",
                "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "::should remove regex lookaround patterns from function tools and warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_remove_regex_lookaround_patterns_from_function_tools_and_warn()
    {
        var result = Prepare(
            null,
            Function(
                "create_contact",
                null,
                """{"type":"object","properties":{"email":{"type":"string","format":"email","pattern":"^(?!\\.).+@.+$"},"username":{"type":"string","pattern":"^@[a-zA-Z0-9_]+$"}}}"""
            )
        );

        AssertTools(
            result,
            """[{"type":"function","name":"create_contact","parameters":{"type":"object","properties":{"email":{"type":"string","format":"email"},"username":{"type":"string","pattern":"^@[a-zA-Z0-9_]+$"}}},"strict":false}]""",
            expectedWarnings: true
        );
        AssertNoToolChoice(result);
        AssertWarnings(
            result,
            (
                "compatibility",
                "JSON Schema pattern with regex lookaround",
                "OpenAI does not support regex lookaround in JSON Schema patterns. The pattern was removed before sending the schema, so OpenAI will not enforce that constraint."
            )
        );
    }

    [Theory]
    [InlineData("codeInterpreter")]
    [InlineData("shell")]
    [InlineData("mcp")]
    [UpstreamTest(
        Suite + "::%s supplies telemetry redaction for configuration and raw bodies",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9f37b45262a853f44b9ca84c8592169b38369826"
    )]
    public void Supplies_telemetry_redaction_for_configuration(string name)
    {
        JsonElement args;
        string toolId;
        Func<JsonElement, JsonElement>? redact;
        if (name == "mcp")
        {
            var mcp = OpenAITools.Mcp(
                new OpenAIMcpToolOptions
                {
                    ServerLabel = "remote",
                    ServerUrl = "https://example.com",
                    Authorization = "test-secret",
                    Headers = new Dictionary<string, string> { ["X-Key"] = "test-secret" },
                }
            );
            args = mcp.Args;
            toolId = mcp.ProviderToolId!;
            redact = mcp.RedactArgs;
        }
        else if (name == "shell")
        {
            var shell = OpenAITools.Shell(
                new OpenAIShellToolOptions
                {
                    Environment = new OpenAIShellEnvironment
                    {
                        Type = "containerAuto",
                        AllowedDomains = ["example.com"],
                        DomainSecrets =
                        [
                            new OpenAIShellDomainSecret
                            {
                                Domain = "example.com",
                                Name = "KEY",
                                Value = "test-secret",
                            },
                        ],
                    },
                }
            );
            args = shell.ProviderToolArgs;
            toolId = shell.ProviderToolId!;
            redact = shell.RedactArgs;
        }
        else
        {
            var interpreter = OpenAITools.CodeInterpreter(
                new OpenAICodeInterpreterToolOptions
                {
                    AllowedDomains = ["example.com"],
                    DomainSecrets =
                    [
                        new OpenAIShellDomainSecret
                        {
                            Domain = "example.com",
                            Name = "KEY",
                            Value = "test-secret",
                        },
                    ],
                }
            );
            args = interpreter.Args;
            toolId = interpreter.ProviderToolId!;
            redact = interpreter.RedactArgs;
        }

        var originalArgs = args.GetRawText();
        var result = Prepare(null, LanguageModelTool.ProviderDefined(toolId, "renamed", args));
        var body = RedactOpenAIToolCredentials.WithOpenAIResponseRedaction(
            JsonSerializer.SerializeToElement(new JsonObject { ["tools"] = result.Tools })
        );
        Assert.NotNull(redact);
        foreach (
            var (value, redacted) in new[]
            {
                (args.GetRawText(), redact!(args).GetRawText()),
                (body.Value.GetRawText(), TelemetryRedaction.RedactTelemetryValue(body)!.Value.GetRawText()),
            }
        )
        {
            Assert.True(value.Contains("test-secret"), value);
            Assert.Equal(value.Replace("test-secret", "[redacted]"), redacted);
        }

        Assert.Equal(originalArgs, args.GetRawText());
    }
}
