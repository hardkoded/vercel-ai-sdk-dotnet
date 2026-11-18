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

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>when tool execution fails</c>.</summary>
public sealed class WhenToolExecutionFailsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > when tool execution fails::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return tool-error with the error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_tool_error_with_the_error()
    {
        var toolError = new InvalidOperationException("execution failed");

        var result = await Execute(CreateToolCall(), [FailingTool(toolError)]);

        var output = result!.Value.Output;
        Assert.True(output.IsError);
        Assert.Equal("call-1", output.ToolCallId);
        Assert.Equal("testTool", output.ToolName);
        Assert.Equal("{\"value\":\"test\"}", output.InputJson);
        Assert.Same(toolError, output.Error);
        Assert.False(output.Dynamic);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve providerMetadata from toolCall on error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_providerMetadata_from_toolCall_on_error()
    {
        var result = await Execute(
            CreateToolCall(providerMetadata: Json("{\"custom\":{\"key\":\"value\"}}")),
            [FailingTool(new InvalidOperationException("execution failed"))]
        );

        var output = result!.Value.Output;
        Assert.True(output.IsError);
        Assert.Equal(
            "{\"custom\":{\"key\":\"value\"}}",
            output.ProviderMetadata!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve toolMetadata from toolCall on error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_toolMetadata_from_toolCall_on_error()
    {
        var toolError = new InvalidOperationException("execution failed");

        var result = await Execute(
            CreateToolCall(
                toolMetadata: new Dictionary<string, JsonElement>
                {
                    ["clientName"] = Json("\"MyMCPClient\""),
                }
            ),
            [FailingTool(toolError)]
        );

        var output = result!.Value.Output;
        Assert.True(output.IsError);
        Assert.False(output.Dynamic);
        Assert.Same(toolError, output.Error);
        Assert.Equal("{\"value\":\"test\"}", output.InputJson);
        Assert.Equal("call-1", output.ToolCallId);
        Assert.Equal("testTool", output.ToolName);
        var metadata = Assert.Single(output.ToolMetadata!);
        Assert.Equal("clientName", metadata.Key);
        Assert.Equal("MyMCPClient", metadata.Value.GetString());
    }
}
