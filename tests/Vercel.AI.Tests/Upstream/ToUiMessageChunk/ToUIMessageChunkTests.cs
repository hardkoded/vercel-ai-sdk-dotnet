// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.ToUiMessageChunk;

/// <summary>Port of <c>to-ui-message-chunk.test.ts</c> &gt; <c>toUIMessageChunk</c>.</summary>
public sealed class ToUIMessageChunkTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::";
    private const string Commit = "ceeee29f9a7b7ca8c12a9270a542581bd760bdc7";

    private const string MetadataJson = """{"testProvider":{"signature":"sig-1"}}""";

    private static readonly JsonElement ProviderMetadata = JsonDocument
        .Parse(MetadataJson)
        .RootElement.Clone();

    private static readonly IReadOnlyDictionary<string, JsonElement> ToolMetadata = new Dictionary<
        string,
        JsonElement
    >
    {
        ["clientName"] = JsonDocument.Parse("\"test-client\"").RootElement.Clone(),
    };

    private static readonly IReadOnlyList<Tool> Tools =
    [
        Tool.Function(
            "staticTool",
            null,
            """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}""",
            null
        ),
        Tool.DynamicTool("dynamicTool", null, "{}", null),
    ];

    private static readonly GeneratedFile File = new([72, 101, 108, 108, 111], "text/plain");

    [Fact]
    [UpstreamTest(
        Prefix + "maps text parts and preserves provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_text_parts_and_preserves_provider_metadata()
    {
        const string metadata = """{"testProvider":{"signature":"sig-1"}}""";

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    $$"""{"type":"text-start","id":"text-1","providerMetadata":{{metadata}}}"""
                ),
                UIMessageStreamConverter.ToUIMessageChunk(
                    new TextStartPart("text-1", ProviderMetadata)
                )
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    $$"""{"type":"text-delta","id":"text-1","delta":"hello","providerMetadata":{{metadata}}}"""
                ),
                UIMessageStreamConverter.ToUIMessageChunk(
                    new TextDeltaPart("text-1", "hello", ProviderMetadata)
                )
            )
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    $$"""{"type":"text-end","id":"text-1","providerMetadata":{{metadata}}}"""
                ),
                UIMessageStreamConverter.ToUIMessageChunk(
                    new TextEndPart("text-1", providerMetadata: ProviderMetadata)
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps reasoning parts by default and suppresses them when disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_reasoning_parts_by_default_and_suppresses_them_when_disabled()
    {
        TextStreamPart[] reasoningParts =
        [
            new ReasoningStartPart("reasoning-1", ProviderMetadata),
            new ReasoningDeltaPart("reasoning-1", "thinking", ProviderMetadata),
            new ReasoningEndPart("reasoning-1", ProviderMetadata),
        ];
        const string metadata = """{"testProvider":{"signature":"sig-1"}}""";
        string[] expected =
        [
            $$"""{"type":"reasoning-start","id":"reasoning-1","providerMetadata":{{metadata}}}""",
            $$"""{"type":"reasoning-delta","id":"reasoning-1","delta":"thinking","providerMetadata":{{metadata}}}""",
            $$"""{"type":"reasoning-end","id":"reasoning-1","providerMetadata":{{metadata}}}""",
        ];

        for (var i = 0; i < reasoningParts.Length; i++)
        {
            Assert.True(
                JsonNode.DeepEquals(
                    JsonNode.Parse(expected[i]),
                    UIMessageStreamConverter.ToUIMessageChunk(reasoningParts[i])
                )
            );
            Assert.Null(
                UIMessageStreamConverter.ToUIMessageChunk(
                    reasoningParts[i],
                    new ToUIMessageChunkOptions { SendReasoning = false }
                )
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps files and suppresses reasoning files when reasoning is disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_files_and_suppresses_reasoning_files_when_reasoning_is_disabled()
    {
        AssertJson(
            $$"""{"type":"file","mediaType":"text/plain","url":"data:text/plain;base64,SGVsbG8=","providerMetadata":{{MetadataJson}}}""",
            UIMessageStreamConverter.ToUIMessageChunk(new FilePart(File, ProviderMetadata))
        );

        var reasoningFilePart = new ReasoningFilePart(File, ProviderMetadata);

        AssertJson(
            $$"""{"type":"reasoning-file","mediaType":"text/plain","url":"data:text/plain;base64,SGVsbG8=","providerMetadata":{{MetadataJson}}}""",
            UIMessageStreamConverter.ToUIMessageChunk(reasoningFilePart)
        );

        Assert.Null(
            UIMessageStreamConverter.ToUIMessageChunk(
                reasoningFilePart,
                new ToUIMessageChunkOptions { SendReasoning = false }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps steering messages with text content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_steering_messages_with_text_content()
    {
        AssertJson(
            """{"type":"steering-message","id":"steer-1","parts":[{"type":"text","text":"I meant Paris, Texas."}]}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new SteeringMessagePart("steer-1", new UserModelMessage("I meant Paris, Texas."), 1)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps provider options of steering message parts as provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_provider_options_of_steering_message_parts_as_provider_metadata()
    {
        var message = new UserModelMessage([
            new TextContentPart("I meant Paris, Texas.")
            {
                ProviderOptions = Options(
                    """{"anthropic":{"cacheControl":{"type":"ephemeral"}}}"""
                ),
            },
            new FileContentPart("image/png", "https://example.com/map.png", null, null)
            {
                ProviderOptions = Options("""{"openai":{"imageDetail":"low"}}"""),
            },
        ]);

        AssertJson(
            """
            {"type":"steering-message","id":"steer-1","parts":[
              {"type":"text","text":"I meant Paris, Texas.","providerMetadata":{"anthropic":{"cacheControl":{"type":"ephemeral"}}}},
              {"type":"file","mediaType":"image/png","url":"https://example.com/map.png","providerMetadata":{"openai":{"imageDetail":"low"}}}
            ]}
            """,
            UIMessageStreamConverter.ToUIMessageChunk(
                new SteeringMessagePart("steer-1", message, 1)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps files in steering messages to ui file parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_files_in_steering_messages_to_ui_file_parts()
    {
        var message = new UserModelMessage([
            new TextContentPart("Look at these."),
            new FileContentPart("image/png", null, [1, 2, 3], null),
            new FileContentPart("image", "https://example.com/cat.png", null, null),
            new FileContentPart("application/pdf", null, [1, 2, 3], "report.pdf"),
            FileContentPart.FromText("text/plain", "Hello"),
            FileContentPart.FromProviderReference(
                "application/pdf",
                new Dictionary<string, string> { ["openai"] = "file-1" }
            ),
        ]);

        AssertJson(
            """
            {"type":"steering-message","id":"steer-1","parts":[
              {"type":"text","text":"Look at these."},
              {"type":"file","mediaType":"image/png","url":"data:image/png;base64,AQID"},
              {"type":"file","mediaType":"image","url":"https://example.com/cat.png"},
              {"type":"file","mediaType":"application/pdf","filename":"report.pdf","url":"data:application/pdf;base64,AQID"},
              {"type":"file","mediaType":"text/plain","url":"data:text/plain;base64,SGVsbG8="},
              {"type":"file","mediaType":"application/pdf","url":"","providerReference":{"openai":"file-1"}}
            ]}
            """,
            UIMessageStreamConverter.ToUIMessageChunk(
                new SteeringMessagePart("steer-1", message, 1)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "skips sources by default and sends them when enabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Skips_sources_by_default_and_sends_them_when_enabled()
    {
        var urlSourcePart = new SourcePart(
            new GeneratedSource("source-1", "https://example.com", "Example", ProviderMetadata)
        );
        var documentSourcePart = new DocumentSourcePart(
            new GeneratedDocumentSource(
                "source-2",
                "application/pdf",
                "Document",
                "document.pdf",
                ProviderMetadata
            )
        );
        var sendSources = new ToUIMessageChunkOptions { SendSources = true };

        Assert.Null(UIMessageStreamConverter.ToUIMessageChunk(urlSourcePart));
        Assert.Null(UIMessageStreamConverter.ToUIMessageChunk(documentSourcePart));

        AssertJson(
            $$"""{"type":"source-url","sourceId":"source-1","url":"https://example.com","title":"Example","providerMetadata":{{MetadataJson}}}""",
            UIMessageStreamConverter.ToUIMessageChunk(urlSourcePart, sendSources)
        );
        AssertJson(
            $$"""{"type":"source-document","sourceId":"source-2","mediaType":"application/pdf","title":"Document","filename":"document.pdf","providerMetadata":{{MetadataJson}}}""",
            UIMessageStreamConverter.ToUIMessageChunk(documentSourcePart, sendSources)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps custom and lifecycle parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_custom_and_lifecycle_parts()
    {
        AssertJson(
            $$"""{"type":"custom","kind":"openai.compaction","providerMetadata":{{MetadataJson}}}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new CustomPart("openai.compaction", ProviderMetadata)
            )
        );

        var messageMetadata = JsonDocument.Parse("""{"model":"test-model"}""").RootElement.Clone();

        AssertJson(
            """{"type":"start","messageId":"msg-1","messageMetadata":{"model":"test-model"}}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new StartPart(),
                new ToUIMessageChunkOptions
                {
                    MessageMetadata = messageMetadata,
                    ResponseMessageId = "msg-1",
                }
            )
        );

        Assert.Null(
            UIMessageStreamConverter.ToUIMessageChunk(
                new StartPart(),
                new ToUIMessageChunkOptions { SendStart = false }
            )
        );

        var finish = new FinishPart(FinishReason.Stop, new LanguageModelUsage(1, 2, 3));

        AssertJson(
            """{"type":"finish","finishReason":"stop","messageMetadata":{"model":"test-model"}}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                finish,
                new ToUIMessageChunkOptions { MessageMetadata = messageMetadata }
            )
        );

        Assert.Null(
            UIMessageStreamConverter.ToUIMessageChunk(
                finish,
                new ToUIMessageChunkOptions { SendFinish = false }
            )
        );

        AssertJson(
            """{"type":"start-step"}""",
            UIMessageStreamConverter.ToUIMessageChunk(new StartStepPart())
        );

        var finishStep = new StepFinishPart(
            new StepResult(
                "",
                null,
                [],
                [],
                FinishReason.Stop,
                new LanguageModelUsage(1, 2, 3),
                [],
                ProviderMetadata
            ),
            new LanguageModelResponseMetadata(
                "response-id",
                DateTimeOffset.FromUnixTimeMilliseconds(0),
                "model-id"
            ),
            "stop"
        );

        AssertJson(
            """{"type":"finish-step"}""",
            UIMessageStreamConverter.ToUIMessageChunk(finishStep)
        );

        AssertJson(
            """{"type":"abort","reason":"user"}""",
            UIMessageStreamConverter.ToUIMessageChunk(new AbortPart("user"))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps tool input streaming parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_tool_input_streaming_parts()
    {
        AssertJson(
            $$"""{"type":"tool-input-start","toolCallId":"call-1","toolName":"dynamicTool","inputEncoding":"text","providerExecuted":true,"providerMetadata":{{MetadataJson}},"toolMetadata":{"clientName":"test-client"},"dynamic":true,"title":"Dynamic Tool"}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolInputStartPart(
                    "call-1",
                    "dynamicTool",
                    ProviderMetadata,
                    true,
                    false,
                    ToolInputEncoding.Text,
                    "Dynamic Tool"
                )
                {
                    ToolMetadata = ToolMetadata,
                },
                new ToUIMessageChunkOptions { Tools = Tools }
            )
        );

        AssertJson(
            """{"type":"tool-input-start","toolCallId":"call-2","toolName":"providerTool","dynamic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolInputStartPart("call-2", "providerTool", null, null, true),
                new ToUIMessageChunkOptions { Tools = Tools }
            )
        );

        AssertJson(
            """{"type":"tool-input-delta","toolCallId":"call-1","inputTextDelta":"{\"value\""}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolInputDeltaPart("call-1", "{\"value\"", null)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps valid and invalid tool call parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_valid_and_invalid_tool_call_parts()
    {
        AssertJson(
            $$"""{"type":"tool-input-available","toolCallId":"call-1","toolName":"staticTool","input":{"value":"input"},"providerExecuted":true,"providerMetadata":{{MetadataJson}},"toolMetadata":{"clientName":"test-client"},"title":"Static Tool"}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolCallPart(
                    new GeneratedToolCall(
                        "call-1",
                        "staticTool",
                        """{"value":"input"}""",
                        ProviderMetadata
                    )
                    {
                        ProviderExecuted = true,
                        ToolMetadata = ToolMetadata,
                        Title = "Static Tool",
                    }
                ),
                new ToUIMessageChunkOptions { Tools = Tools }
            )
        );

        AssertJson(
            """{"type":"tool-input-available","toolCallId":"call-2","toolName":"runtimeTool","input":{"value":"input"},"dynamic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolCallPart(
                    new GeneratedToolCall("call-2", "runtimeTool", """{"value":"input"}""")
                    {
                        Dynamic = true,
                    }
                )
            )
        );

        AssertJson(
            $$"""{"type":"tool-input-error","toolCallId":"call-3","toolName":"runtimeTool","input":"{broken","providerExecuted":true,"providerMetadata":{{MetadataJson}},"toolMetadata":{"clientName":"test-client"},"dynamic":true,"errorText":"handled: invalid input","title":"Invalid Tool"}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolCallPart(
                    new GeneratedToolCall("call-3", "runtimeTool", "{broken", ProviderMetadata)
                    {
                        Dynamic = true,
                        Invalid = true,
                        Error = new JsError("invalid input"),
                        ProviderExecuted = true,
                        ToolMetadata = ToolMetadata,
                        Title = "Invalid Tool",
                    }
                ),
                new ToUIMessageChunkOptions { OnError = error => "handled: " + error.Message }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves schema input for transformed tool approval requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Preserves_schema_input_for_transformed_tool_approval_requests()
    {
        var toolCall = new GeneratedToolCall("call-1", "staticTool", """{"value":"trimmed"}""")
        {
            InputSchemaInput = JsonDocument.Parse("""{"value":" trimmed "}""").RootElement.Clone(),
        };

        AssertJson(
            """{"type":"tool-approval-request","approvalId":"approval-1","toolCallId":"call-1","inputSchemaInput":{"value":" trimmed "}}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolApprovalRequestPart(
                    new GeneratedToolApprovalRequest("approval-1", "call-1") { ToolCall = toolCall }
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps tool result, tool error, tool denial, and approval parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_tool_result_tool_error_tool_denial_and_approval_parts()
    {
        var options = new ToUIMessageChunkOptions { Tools = Tools };

        AssertJson(
            $$"""{"type":"tool-output-available","toolCallId":"call-1","output":{"value":"output"},"providerExecuted":true,"providerMetadata":{{MetadataJson}},"toolMetadata":{"clientName":"test-client"},"dynamic":true,"preliminary":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolResultPart(
                    new ExecutedTool("call-1", "dynamicTool", """{"value":"output"}""", false)
                    {
                        InputJson = """{"value":"input"}""",
                        ProviderExecuted = true,
                        ProviderMetadata = ProviderMetadata,
                        ToolMetadata = ToolMetadata,
                        Dynamic = true,
                        Preliminary = true,
                    }
                ),
                options
            )
        );

        AssertJson(
            """{"type":"tool-output-available","toolCallId":"call-undefined","output":null,"dynamic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolResultPart(
                    new ExecutedTool("call-undefined", "dynamicTool", string.Empty, false)
                    {
                        InputJson = """{"value":"input"}""",
                    }
                ),
                options
            )
        );

        AssertJson(
            $$"""{"type":"tool-output-error","toolCallId":"call-2","errorText":"{\"code\":\"provider-error\"}","providerExecuted":true,"providerMetadata":{{MetadataJson}},"toolMetadata":{"clientName":"test-client"},"dynamic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolErrorPart(
                    new ExecutedTool("call-2", "dynamicTool", """{"code":"provider-error"}""", true)
                    {
                        InputJson = """{"value":"input"}""",
                        ProviderExecuted = true,
                        ProviderMetadata = ProviderMetadata,
                        ToolMetadata = ToolMetadata,
                        Dynamic = true,
                    }
                ),
                new ToUIMessageChunkOptions
                {
                    OnError = _ => "should not be used for provider-executed errors",
                    Tools = Tools,
                }
            )
        );

        AssertJson(
            """{"type":"tool-output-error","toolCallId":"call-string-error","errorText":"provider string error","providerExecuted":true,"dynamic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolErrorPart(
                    new ExecutedTool(
                        "call-string-error",
                        "dynamicTool",
                        "\"provider string error\"",
                        true
                    )
                    {
                        InputJson = """{"value":"input"}""",
                        ProviderExecuted = true,
                        Dynamic = true,
                    }
                )
            )
        );

        AssertJson(
            """{"type":"tool-output-error","toolCallId":"call-3","errorText":"handled: tool failed"}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolErrorPart(
                    new ExecutedTool("call-3", "staticTool", "{}", true)
                    {
                        InputJson = """{"value":"input"}""",
                        Error = new JsError("tool failed"),
                    }
                ),
                new ToUIMessageChunkOptions
                {
                    OnError = error => "handled: " + error.Message,
                    Tools = Tools,
                }
            )
        );

        AssertJson(
            """{"type":"tool-output-denied","toolCallId":"call-4"}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolOutputDeniedPart("call-4", "staticTool")
            )
        );

        var approvalToolCall = new GeneratedToolCall(
            "call-5",
            "staticTool",
            """{"value":"input"}"""
        );

        AssertJson(
            """{"type":"tool-approval-request","approvalId":"approval-1","toolCallId":"call-5","reason":"requires operator review","isAutomatic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolApprovalRequestPart(
                    new GeneratedToolApprovalRequest("approval-1", "call-5")
                    {
                        ToolCall = approvalToolCall,
                        Reason = "requires operator review",
                        IsAutomatic = true,
                    }
                )
            )
        );

        AssertJson(
            """{"type":"tool-approval-response","approvalId":"approval-1","approved":false,"reason":"not allowed","providerExecuted":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(
                new ToolApprovalResponsePart(
                    new GeneratedToolApprovalResponse(
                        "approval-1",
                        approvalToolCall,
                        false,
                        "not allowed",
                        true
                    )
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps error parts through onError",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Maps_error_parts_through_onError()
    {
        var error = new JsError("boom");
        var handled = new List<Exception>();

        var chunk = UIMessageStreamConverter.ToUIMessageChunk(
            new ErrorPart("boom") { Error = error },
            new ToUIMessageChunkOptions
            {
                OnError = e =>
                {
                    handled.Add(e);
                    return "handled error";
                },
            }
        );

        AssertJson("""{"type":"error","errorText":"handled error"}""", chunk);
        Assert.Same(error, Assert.Single(handled));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "masks error parts by default",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Masks_error_parts_by_default()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(new ErrorPart("boom"));

        Assert.Equal(
            """{"type":"error","errorText":"An error occurred."}""",
            chunk!.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "masks error parts when redactToolErrors is false",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Masks_error_parts_when_redactToolErrors_is_false()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(
            new ErrorPart("boom"),
            new ToUIMessageChunkOptions { RedactToolErrors = false }
        );

        Assert.Equal(
            """{"type":"error","errorText":"An error occurred."}""",
            chunk!.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns undefined for parts that do not produce UI message chunks",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Returns_undefined_for_parts_that_do_not_produce_UI_message_chunks()
    {
        Assert.Null(
            UIMessageStreamConverter.ToUIMessageChunk(new ToolInputEndPart("call-1", null))
        );

        Assert.Null(
            UIMessageStreamConverter.ToUIMessageChunk(
                new RawPart(JsonDocument.Parse("""{"provider":"raw"}""").RootElement.Clone())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "throws for unknown part types",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Throws_for_unknown_part_types()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            UIMessageStreamConverter.ToUIMessageChunk(new UnknownPart())
        );

        Assert.Contains("Unknown chunk type: unknown", exception.Message);
    }

    private static void AssertJson(string expected, JsonObject? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    private static IReadOnlyDictionary<string, JsonElement> Options(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

    private sealed class UnknownPart : TextStreamPart
    {
        public UnknownPart()
            : base("unknown") { }
    }
}
