// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateText;

namespace Vercel.AI.Tests.Upstream.ToUiMessageStream;

/// <summary>Port of <c>to-ui-message-stream.test.ts</c> &gt; <c>toUIMessageStream</c>.</summary>
public sealed class ToUIMessageStreamTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/to-ui-message-stream.test.ts::toUIMessageStream::";

    [Fact]
    [UpstreamTest(
        Prefix + "routes an invalid tool call through onError once",
        Coverage = UpstreamCoverage.Partial,
        Note = "Runs through streamText with an unknown tool instead of feeding hand-built stream parts, because the .NET writer takes a StreamTextResult. The onError argument is the formatted error string, not the error object.")]
    public async Task Routes_an_invalid_tool_call_through_onError_once()
    {
        var errors = new List<string>();
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(
            Run("unknown-tool", "{\"value\":\"invalid\"}", []),
            error =>
            {
                errors.Add(error);
                return "formatted tool error";
            });

        var inputError = Assert.Single(chunks, chunk => (string?)chunk!["type"] == "tool-input-error")!;
        Assert.Equal("formatted tool error", (string?)inputError["errorText"]);
        Assert.DoesNotContain(chunks, chunk => (string?)chunk!["type"] == "tool-output-error");
        Assert.Single(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forwards tool errors for valid tool calls",
        Coverage = UpstreamCoverage.Partial,
        Note = "Runs through streamText with a tool that throws instead of feeding hand-built stream parts.")]
    public async Task Forwards_tool_errors_for_valid_tool_calls()
    {
        var errors = new List<string>();
        var tool = Tool.Function(
            "test-tool",
            null,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}",
            (_, _) => throw new InvalidOperationException("execution failed"));
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(
            Run("test-tool", "{\"value\":\"valid\"}", [tool]),
            error =>
            {
                errors.Add(error);
                return "formatted tool error";
            });

        Assert.Contains(chunks, chunk => (string?)chunk!["type"] == "tool-input-available");
        var outputError = Assert.Single(chunks, chunk => (string?)chunk!["type"] == "tool-output-error")!;
        Assert.Equal("formatted tool error", (string?)outputError["errorText"]);
        Assert.Single(errors);
    }

    private static StreamTextResult Run(string toolName, string input, Tool[] tools)
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new ToolCallStreamPart("call-1", toolName, input),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10), "stop"),
            },
        };
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = tools,
            NowMs = () => 0,
        });
    }
}
