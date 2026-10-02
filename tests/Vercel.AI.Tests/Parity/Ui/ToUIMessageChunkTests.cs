// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class ToUIMessageChunkTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps text parts and preserves provider metadata",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_text_parts_and_preserves_provider_metadata()
    {
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("text-start")
            {
                Id = "text-1",
                ProviderMetadata = Meta(),
            }),
            "{\"type\":\"text-start\",\"id\":\"text-1\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("text-delta")
            {
                Id = "text-1",
                Text = "hello",
                ProviderMetadata = Meta(),
            }),
            "{\"type\":\"text-delta\",\"id\":\"text-1\",\"delta\":\"hello\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("text-end")
            {
                Id = "text-1",
                ProviderMetadata = Meta(),
            }),
            "{\"type\":\"text-end\",\"id\":\"text-1\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps reasoning parts by default and suppresses them when disabled",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_reasoning_parts_unless_disabled()
    {
        var parts = new[]
        {
            new UITextStreamPart("reasoning-start") { Id = "reasoning-1", ProviderMetadata = Meta() },
            new UITextStreamPart("reasoning-delta") { Id = "reasoning-1", Text = "thinking", ProviderMetadata = Meta() },
            new UITextStreamPart("reasoning-end") { Id = "reasoning-1", ProviderMetadata = Meta() },
        };

        Expect(UIMessageChunkConverter.ToUIMessageChunk(parts[0]), "{\"type\":\"reasoning-start\",\"id\":\"reasoning-1\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Expect(UIMessageChunkConverter.ToUIMessageChunk(parts[1]), "{\"type\":\"reasoning-delta\",\"id\":\"reasoning-1\",\"delta\":\"thinking\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Expect(UIMessageChunkConverter.ToUIMessageChunk(parts[2]), "{\"type\":\"reasoning-end\",\"id\":\"reasoning-1\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");

        var suppressed = new ToUIMessageChunkOptions { SendReasoning = false };
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(parts[0], suppressed));
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(parts[1], suppressed));
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(parts[2], suppressed));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps files and suppresses reasoning files when reasoning is disabled",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_files_and_suppresses_reasoning_files()
    {
        var file = new UITextStreamPart("file")
        {
            File = new UIGeneratedFile("SGVsbG8=", "text/plain"),
            ProviderMetadata = Meta(),
        };
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(file),
            "{\"type\":\"file\",\"mediaType\":\"text/plain\",\"url\":\"data:text/plain;base64,SGVsbG8=\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");

        var reasoningFile = new UITextStreamPart("reasoning-file")
        {
            File = new UIGeneratedFile("SGVsbG8=", "text/plain"),
            ProviderMetadata = Meta(),
        };
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(reasoningFile),
            "{\"type\":\"reasoning-file\",\"mediaType\":\"text/plain\",\"url\":\"data:text/plain;base64,SGVsbG8=\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(reasoningFile, new ToUIMessageChunkOptions { SendReasoning = false }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::skips sources by default and sends them when enabled",
        Coverage = UpstreamCoverage.Covered)]
    public void Skips_sources_unless_enabled()
    {
        var url = new UITextStreamPart("source")
        {
            SourceType = "url",
            Id = "source-1",
            Url = "https://example.com",
            Title = "Example",
            ProviderMetadata = Meta(),
        };
        var document = new UITextStreamPart("source")
        {
            SourceType = "document",
            Id = "source-2",
            MediaType = "application/pdf",
            Title = "Document",
            Filename = "document.pdf",
            ProviderMetadata = Meta(),
        };

        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(url));
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(document));
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(url, new ToUIMessageChunkOptions { SendSources = true }),
            "{\"type\":\"source-url\",\"sourceId\":\"source-1\",\"url\":\"https://example.com\",\"title\":\"Example\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(document, new ToUIMessageChunkOptions { SendSources = true }),
            "{\"type\":\"source-document\",\"sourceId\":\"source-2\",\"mediaType\":\"application/pdf\",\"title\":\"Document\",\"filename\":\"document.pdf\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps custom and lifecycle parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_custom_and_lifecycle_parts()
    {
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("custom")
            {
                Kind = "openai.compaction",
                ProviderMetadata = Meta(),
            }),
            "{\"type\":\"custom\",\"kind\":\"openai.compaction\",\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}}}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("start"),
                new ToUIMessageChunkOptions
                {
                    MessageMetadata = JsonNode.Parse("{\"model\":\"test-model\"}"),
                    ResponseMessageId = "msg-1",
                }),
            "{\"type\":\"start\",\"messageMetadata\":{\"model\":\"test-model\"},\"messageId\":\"msg-1\"}");
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("start"), new ToUIMessageChunkOptions { SendStart = false }));
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("finish") { FinishReason = "stop" },
                new ToUIMessageChunkOptions { MessageMetadata = JsonNode.Parse("{\"model\":\"test-model\"}") }),
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"messageMetadata\":{\"model\":\"test-model\"}}");
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(
            new UITextStreamPart("finish") { FinishReason = "stop" },
            new ToUIMessageChunkOptions { SendFinish = false }));
        Expect(UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("start-step")), "{\"type\":\"start-step\"}");
        Expect(UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("finish-step")), "{\"type\":\"finish-step\"}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("abort") { Reason = "user" }),
            "{\"type\":\"abort\",\"reason\":\"user\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps tool input streaming parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_tool_input_streaming_parts()
    {
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-input-start")
                {
                    Id = "call-1",
                    ToolName = "dynamicTool",
                    ProviderExecuted = true,
                    ProviderMetadata = Meta(),
                    ToolMetadata = JsonNode.Parse("{\"clientName\":\"test-client\"}"),
                    Title = "Dynamic Tool",
                },
                Tools()),
            "{\"type\":\"tool-input-start\",\"toolCallId\":\"call-1\",\"toolName\":\"dynamicTool\",\"providerExecuted\":true,\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}},\"toolMetadata\":{\"clientName\":\"test-client\"},\"dynamic\":true,\"title\":\"Dynamic Tool\"}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-input-start")
                {
                    Id = "call-2",
                    ToolName = "providerTool",
                    Dynamic = true,
                },
                Tools()),
            "{\"type\":\"tool-input-start\",\"toolCallId\":\"call-2\",\"toolName\":\"providerTool\",\"dynamic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-input-delta")
            {
                Id = "call-1",
                Delta = "{\"value\"",
            }),
            "{\"type\":\"tool-input-delta\",\"toolCallId\":\"call-1\",\"inputTextDelta\":\"{\\\"value\\\"\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps valid and invalid tool call parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_valid_and_invalid_tool_calls()
    {
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-call")
                {
                    ToolCallId = "call-1",
                    ToolName = "staticTool",
                    Input = JsonNode.Parse("{\"value\":\"input\"}"),
                    ProviderExecuted = true,
                    ProviderMetadata = Meta(),
                    ToolMetadata = JsonNode.Parse("{\"clientName\":\"test-client\"}"),
                    Title = "Static Tool",
                },
                Tools()),
            "{\"type\":\"tool-input-available\",\"toolCallId\":\"call-1\",\"toolName\":\"staticTool\",\"input\":{\"value\":\"input\"},\"providerExecuted\":true,\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}},\"toolMetadata\":{\"clientName\":\"test-client\"},\"title\":\"Static Tool\"}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-call")
            {
                ToolCallId = "call-2",
                ToolName = "runtimeTool",
                Input = JsonNode.Parse("{\"value\":\"input\"}"),
                Dynamic = true,
            }),
            "{\"type\":\"tool-input-available\",\"toolCallId\":\"call-2\",\"toolName\":\"runtimeTool\",\"input\":{\"value\":\"input\"},\"dynamic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-call")
                {
                    ToolCallId = "call-3",
                    ToolName = "runtimeTool",
                    Input = JsonValue.Create("{broken"),
                    Dynamic = true,
                    Invalid = true,
                    Error = new InvalidOperationException("invalid input"),
                    ProviderExecuted = true,
                    ProviderMetadata = Meta(),
                    ToolMetadata = JsonNode.Parse("{\"clientName\":\"test-client\"}"),
                    Title = "Invalid Tool",
                },
                new ToUIMessageChunkOptions
                {
                    OnError = error => "handled: " + ((Exception)error!).Message,
                }),
            "{\"type\":\"tool-input-error\",\"toolCallId\":\"call-3\",\"toolName\":\"runtimeTool\",\"input\":\"{broken\",\"providerExecuted\":true,\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}},\"toolMetadata\":{\"clientName\":\"test-client\"},\"dynamic\":true,\"errorText\":\"handled: invalid input\",\"title\":\"Invalid Tool\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::preserves schema input for transformed tool approval requests",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_schema_input_for_transformed_approval_requests()
    {
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-approval-request")
            {
                ApprovalId = "approval-1",
                HasInputSchemaInput = true,
                InputSchemaInput = JsonNode.Parse("{\"value\":\" trimmed \"}"),
                ToolCall = new UITextStreamPart("tool-call")
                {
                    ToolCallId = "call-1",
                    ToolName = "staticTool",
                    Input = JsonNode.Parse("{\"value\":\"trimmed\"}"),
                },
            }),
            "{\"type\":\"tool-approval-request\",\"approvalId\":\"approval-1\",\"toolCallId\":\"call-1\",\"inputSchemaInput\":{\"value\":\" trimmed \"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps tool result, tool error, tool denial, and approval parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_tool_results_errors_denials_and_approvals()
    {
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-result")
                {
                    ToolCallId = "call-1",
                    ToolName = "dynamicTool",
                    Output = JsonNode.Parse("{\"value\":\"output\"}"),
                    HasOutput = true,
                    ProviderExecuted = true,
                    ProviderMetadata = Meta(),
                    ToolMetadata = JsonNode.Parse("{\"clientName\":\"test-client\"}"),
                    Dynamic = true,
                    Preliminary = true,
                },
                Tools()),
            "{\"type\":\"tool-output-available\",\"toolCallId\":\"call-1\",\"output\":{\"value\":\"output\"},\"providerExecuted\":true,\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}},\"toolMetadata\":{\"clientName\":\"test-client\"},\"preliminary\":true,\"dynamic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-result")
                {
                    ToolCallId = "call-undefined",
                    ToolName = "dynamicTool",
                    HasOutput = true,
                    Output = null,
                },
                Tools()),
            "{\"type\":\"tool-output-available\",\"toolCallId\":\"call-undefined\",\"output\":null,\"dynamic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-error")
                {
                    ToolCallId = "call-2",
                    ToolName = "dynamicTool",
                    Error = JsonNode.Parse("{\"code\":\"provider-error\"}"),
                    ProviderExecuted = true,
                    ProviderMetadata = Meta(),
                    ToolMetadata = JsonNode.Parse("{\"clientName\":\"test-client\"}"),
                    Dynamic = true,
                },
                new ToUIMessageChunkOptions
                {
                    Tools = ToolMap(),
                    OnError = _ => "should not be used for provider-executed errors",
                }),
            "{\"type\":\"tool-output-error\",\"toolCallId\":\"call-2\",\"errorText\":\"{\\\"code\\\":\\\"provider-error\\\"}\",\"providerExecuted\":true,\"providerMetadata\":{\"testProvider\":{\"signature\":\"sig-1\"}},\"toolMetadata\":{\"clientName\":\"test-client\"},\"dynamic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-error")
            {
                ToolCallId = "call-string-error",
                ToolName = "dynamicTool",
                Error = "provider string error",
                ProviderExecuted = true,
                Dynamic = true,
            }),
            "{\"type\":\"tool-output-error\",\"toolCallId\":\"call-string-error\",\"errorText\":\"provider string error\",\"providerExecuted\":true,\"dynamic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("tool-error")
                {
                    ToolCallId = "call-3",
                    ToolName = "staticTool",
                    Error = new InvalidOperationException("tool failed"),
                },
                new ToUIMessageChunkOptions
                {
                    Tools = ToolMap(),
                    OnError = error => "handled: " + ((Exception)error!).Message,
                }),
            "{\"type\":\"tool-output-error\",\"toolCallId\":\"call-3\",\"errorText\":\"handled: tool failed\"}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-output-denied")
            {
                ToolCallId = "call-4",
                ToolName = "staticTool",
            }),
            "{\"type\":\"tool-output-denied\",\"toolCallId\":\"call-4\"}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-approval-request")
            {
                ApprovalId = "approval-1",
                Reason = "requires operator review",
                IsAutomatic = true,
                ToolCall = new UITextStreamPart("tool-call")
                {
                    ToolCallId = "call-5",
                    ToolName = "staticTool",
                    Input = JsonNode.Parse("{\"value\":\"input\"}"),
                },
            }),
            "{\"type\":\"tool-approval-request\",\"approvalId\":\"approval-1\",\"toolCallId\":\"call-5\",\"reason\":\"requires operator review\",\"isAutomatic\":true}");
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-approval-response")
            {
                ApprovalId = "approval-1",
                Approved = false,
                Reason = "not allowed",
                ProviderExecuted = true,
            }),
            "{\"type\":\"tool-approval-response\",\"approvalId\":\"approval-1\",\"approved\":false,\"reason\":\"not allowed\",\"providerExecuted\":true}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::maps error parts through onError",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_error_parts_through_on_error()
    {
        var error = new InvalidOperationException("boom");
        object? seen = null;
        Expect(
            UIMessageChunkConverter.ToUIMessageChunk(
                new UITextStreamPart("error") { Error = error },
                new ToUIMessageChunkOptions
                {
                    OnError = value =>
                    {
                        seen = value;
                        return "handled error";
                    },
                }),
            "{\"type\":\"error\",\"errorText\":\"handled error\"}");
        Assert.Same(error, seen);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::returns undefined for parts that do not produce UI message chunks",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_parts_that_do_not_produce_chunks()
    {
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("tool-input-end") { Id = "call-1" }));
        Assert.Null(UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("raw")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::throws for unknown part types",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_unknown_part_types()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            UIMessageChunkConverter.ToUIMessageChunk(new UITextStreamPart("unknown")));
        Assert.Equal("Unknown chunk type: unknown", error.Message);
    }

    private static ToUIMessageChunkOptions Tools()
    {
        return new ToUIMessageChunkOptions { Tools = ToolMap() };
    }

    private static Dictionary<string, UIMessageTool> ToolMap()
    {
        return new Dictionary<string, UIMessageTool>
        {
            ["staticTool"] = new UIMessageTool(false),
            ["dynamicTool"] = new UIMessageTool(true),
        };
    }

    private static JsonNode Meta()
    {
        return JsonNode.Parse("{\"testProvider\":{\"signature\":\"sig-1\"}}")!;
    }

    private static void Expect(JsonObject? actual, string expected)
    {
        ParityAssert.JsonEqual(actual, expected);
    }
}
