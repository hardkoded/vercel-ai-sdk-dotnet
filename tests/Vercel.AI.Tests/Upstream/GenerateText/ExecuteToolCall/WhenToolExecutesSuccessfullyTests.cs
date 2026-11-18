// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall.ExecuteToolCallSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>when tool executes successfully</c>.</summary>
public sealed class WhenToolExecutesSuccessfullyTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > when tool executes successfully::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return tool-result with correct data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_tool_result_with_correct_data()
    {
        var result = await Execute(CreateToolCall(), [ResultTool()]);

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.Equal("call-1", output.ToolCallId);
        Assert.Equal("testTool", output.ToolName);
        Assert.Equal("{\"value\":\"test\"}", output.InputJson);
        Assert.Equal("\"test-result\"", output.OutputJson);
        Assert.False(output.Dynamic);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass sandbox to tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_sandbox_to_tool_execution()
    {
        var sandbox = new Vercel.AI.Tests.Upstream.ToolLoopAgent.TestSandboxSession();
        ISandboxSession? receivedSandbox = null;
        var tool = Tool.Function("testTool", null, ValueSchema, null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            receivedSandbox = options.ExperimentalSandbox;
            return Task.FromResult(
                JsonSerializer.Serialize(input.GetProperty("value").GetString() + "-result")
            );
        };

        await Execute(CreateToolCall(), [tool], experimentalSandbox: sandbox);

        Assert.Same(sandbox, receivedSandbox);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve providerMetadata from toolCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_providerMetadata_from_toolCall()
    {
        var result = await Execute(
            CreateToolCall(providerMetadata: Json("{\"custom\":{\"key\":\"value\"}}")),
            [ResultTool()]
        );

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.Equal(
            "{\"custom\":{\"key\":\"value\"}}",
            output.ProviderMetadata!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve toolMetadata from toolCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_toolMetadata_from_toolCall()
    {
        var result = await Execute(
            CreateToolCall(
                toolMetadata: new Dictionary<string, JsonElement>
                {
                    ["clientName"] = Json("\"MyMCPClient\""),
                }
            ),
            [ResultTool()]
        );

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.False(output.Dynamic);
        Assert.Equal("{\"value\":\"test\"}", output.InputJson);
        Assert.Equal("\"test-result\"", output.OutputJson);
        Assert.Equal("call-1", output.ToolCallId);
        Assert.Equal("testTool", output.ToolName);
        var metadata = Assert.Single(output.ToolMetadata!);
        Assert.Equal("clientName", metadata.Key);
        Assert.Equal("MyMCPClient", metadata.Value.GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw TypeValidationError when tool context fails validation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_TypeValidationError_when_tool_context_fails_validation()
    {
        var error = await Assert.ThrowsAsync<Vercel.AI.GenerateText.TypeValidationException>(() =>
            Execute(
                CreateToolCall(),
                [ResultTool(Key1StringSchema)],
                toolsContext: new Dictionary<string, JsonElement>
                {
                    ["testTool"] = Json("{\"key1\":1}"),
                }
            )
        );

        Assert.Equal("{\"key1\":1}", ((JsonElement)error.Value!).GetRawText());
        Assert.Equal("tool context", error.Context.Field);
        Assert.Equal("testTool", error.Context.EntityName);
    }
}
