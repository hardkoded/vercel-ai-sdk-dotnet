// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution errors</c>. Only the UI message stream tests are ported here.</summary>
public sealed class ToolExecutionErrorsTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution errors::";
    private const string RedactPrefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution errors > redactToolErrors: true::";

    [Fact]
    [UpstreamTest(Prefix + "should add tool-error parts to ui message stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_tool_error_parts_to_ui_message_stream()
    {
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(Run());

        // The .NET stream always sends a generated messageId on start.
        Assert.NotNull(chunks[0]!["messageId"]);
        chunks[0]!.AsObject().Remove("messageId");
        var expected = JsonNode.Parse("""
            [
              { "type": "start" },
              { "type": "start-step" },
              { "input": { "value": "value" }, "toolCallId": "call-1", "toolName": "tool1", "type": "tool-input-available" },
              { "errorText": "Error: test error", "toolCallId": "call-1", "type": "tool-output-error" },
              { "type": "finish-step" },
              { "finishReason": "stop", "type": "finish" }
            ]
            """);
        Assert.True(JsonNode.DeepEquals(expected, chunks), chunks.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        RedactPrefix + "should redact the tool error in toUIMessageStream",
        Coverage = UpstreamCoverage.Partial,
        Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc",
        Note = "Uses UIMessageStreamResult.WriteAsync, the .NET counterpart of toUIMessageStream.")]
    public async Task Should_redact_the_tool_error_in_toUIMessageStream()
    {
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(Run(), redactToolErrors: true);

        var errors = chunks.Where(chunk => (string?)chunk!["type"] == "tool-output-error").ToArray();
        var error = Assert.Single(errors)!;
        Assert.Equal("An error occurred.", (string?)error["errorText"]);
        Assert.Equal("call-1", (string?)error["toolCallId"]);
    }

    [Fact]
    [UpstreamTest(RedactPrefix + "should redact the tool error in toUIMessageStreamResponse", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_redact_the_tool_error_in_toUIMessageStreamResponse()
    {
        var body = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = body;

        await Run().ToUIMessageStreamResult(redactToolErrors: true).ExecuteAsync(context);

        var text = Encoding.UTF8.GetString(body.ToArray());
        Assert.Contains("\"errorText\":\"An error occurred.\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("test error", text, StringComparison.Ordinal);
    }

    private static StreamTextResult Run()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"value\" }"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10), "stop"),
            },
        };
        var tool = Tool.Function(
            "tool1",
            null,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
            (_, _) => throw new JsError("test error"));
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { tool },
            NowMs = () => 0,
        });
    }
}
