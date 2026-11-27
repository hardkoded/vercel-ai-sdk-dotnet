// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelMessage &gt; assistant message</c>.</summary>
public sealed class MessageAssistantTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelMessage > assistant message";

    private static readonly IReadOnlyDictionary<string, DownloadResult> NoAssets =
        new Dictionary<string, DownloadResult>();

    private static IReadOnlyDictionary<string, JsonElement> Options(string json) =>
        new Dictionary<string, JsonElement>
        {
            ["test-provider"] = JsonDocument.Parse(json).RootElement.Clone(),
        };

    private static IReadOnlyList<AssistantContentPart> Convert(params AssistantContentPart[] parts)
    {
        var assistant = Assert.IsType<AssistantModelMessage>(
            LanguageModelPrompts.ConvertToLanguageModelMessage(
                new AssistantModelMessage(parts),
                NoAssets
            )
        );
        Assert.Null(assistant.ProviderOptions);
        return assistant.Parts;
    }

    private static AssistantToolCallPart ToolCall(bool providerExecuted = false) =>
        new(
            new GeneratedToolCall("toolCallId", "toolName", "{}")
            {
                ProviderExecuted = providerExecuted,
            }
        );

    [Fact]
    [UpstreamTest(Id + "::should include custom parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_custom_parts()
    {
        var options = new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonDocument.Parse("{\"itemId\":\"cmp_123\"}").RootElement.Clone(),
        };

        var part = Assert.IsType<AssistantCustomPart>(
            Assert.Single(Convert(new AssistantCustomPart("test-provider.compaction", options)))
        );
        Assert.Equal("test-provider.compaction", part.Kind);
        Assert.Equal("{\"itemId\":\"cmp_123\"}", part.ProviderOptions!["openai"].GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Id + " > text parts::should ignore empty text parts when there are no provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_ignore_empty_text_parts_when_there_are_no_provider_options()
    {
        var parts = Convert(new AssistantTextPart(string.Empty), ToolCall());

        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(parts));
        Assert.Equal("toolCallId", call.ToolCall.ToolCallId);
        Assert.Equal("toolName", call.ToolCall.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Id + " > text parts::should include empty text parts when there are provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_empty_text_parts_when_there_are_provider_options()
    {
        var parts = Convert(
            new AssistantTextPart(string.Empty, Options("{\"key-a\":\"test-value-1\"}")),
            ToolCall()
        );

        Assert.Equal(2, parts.Count);
        var text = Assert.IsType<AssistantTextPart>(parts[0]);
        Assert.Equal(string.Empty, text.Text);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\"}",
            text.ProviderOptions!["test-provider"].GetRawText()
        );
        var call = Assert.IsType<AssistantToolCallPart>(parts[1]);
        Assert.Equal("toolCallId", call.ToolCall.ToolCallId);
        Assert.False(call.ToolCall.ProviderExecuted);
        Assert.Null(call.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + " > reasoning parts::should support a mix of reasoning, redacted reasoning, and text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_support_a_mix_of_reasoning_redacted_reasoning_and_text_parts()
    {
        var parts = Convert(
            new AssistantReasoningPart("I'm thinking"),
            new AssistantReasoningPart("redacted-reasoning-data", Options("{\"redacted\":true}")),
            new AssistantReasoningPart("more thinking"),
            new AssistantTextPart("hello, world!")
        );

        Assert.Equal(4, parts.Count);
        Assert.Equal("I'm thinking", Assert.IsType<AssistantReasoningPart>(parts[0]).Text);
        var redacted = Assert.IsType<AssistantReasoningPart>(parts[1]);
        Assert.Equal("redacted-reasoning-data", redacted.Text);
        Assert.Equal(
            "{\"redacted\":true}",
            redacted.ProviderOptions!["test-provider"].GetRawText()
        );
        Assert.Equal("more thinking", Assert.IsType<AssistantReasoningPart>(parts[2]).Text);
        Assert.Equal("hello, world!", Assert.IsType<AssistantTextPart>(parts[3]).Text);
    }

    [Fact]
    [UpstreamTest(
        Id
            + " > file parts with provider reference::should pass through provider reference for assistant file parts without conversion",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_provider_reference_for_assistant_file_parts_without_conversion()
    {
        var reference = new Dictionary<string, string>
        {
            ["openai"] = "file-abc123",
            ["anthropic"] = "file-xyz789",
        };

        var file = Assert.IsType<AssistantFilePart>(
            Assert.Single(
                Convert(
                    AssistantFilePart.FromProviderReference("application/pdf", reference, "doc.pdf")
                )
            )
        );
        Assert.Equal(reference, file.ProviderReference);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Equal("doc.pdf", file.FileName);
    }

    [Fact]
    [UpstreamTest(
        Id + " > reasoning-file parts::should convert reasoning-file part with base64 data",
        Coverage = UpstreamCoverage.Partial,
        Note = "FileContentPart takes decoded bytes, so the base64 string input of upstream is decoded by the caller; the string handling is not exercised."
    )]
    public void Partial_Should_convert_reasoning_file_part_with_base64_data()
    {
        var part = Assert.IsType<AssistantReasoningFilePart>(
            Assert.Single(
                Convert(
                    new AssistantReasoningFilePart(
                        "image/png",
                        null,
                        System.Convert.FromBase64String("iVBORw0KGgo="),
                        Options("{\"key-a\":\"test-value-1\"}")
                    )
                )
            )
        );
        Assert.Equal(System.Convert.FromBase64String("iVBORw0KGgo="), part.Data);
        Assert.Equal("image/png", part.MediaType);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\"}",
            part.ProviderOptions!["test-provider"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + " > reasoning-file parts::should convert reasoning-file part with Uint8Array data",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_reasoning_file_part_with_Uint8Array_data()
    {
        byte[] data = [137, 80, 78, 71, 13, 10, 26, 10];

        var part = Assert.IsType<AssistantReasoningFilePart>(
            Assert.Single(Convert(new AssistantReasoningFilePart("image/png", null, data)))
        );
        Assert.Equal(data, part.Data);
        Assert.Equal("image/png", part.MediaType);
        Assert.Null(part.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id + " > tool call parts::should include providerExecuted flag",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Tool_call_should_include_providerExecuted_flag()
    {
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(Convert(ToolCall(true))));
        Assert.Equal("toolCallId", call.ToolCall.ToolCallId);
        Assert.Equal("toolName", call.ToolCall.ToolName);
        Assert.Equal("{}", call.ToolCall.ArgumentsJson);
        Assert.True(call.ToolCall.ProviderExecuted);
        Assert.Null(call.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id + " > tool result parts::should include providerExecuted flag",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Tool_result_should_include_providerExecuted_flag()
    {
        var result = Assert.IsType<AssistantToolResultPart>(
            Assert.Single(
                Convert(
                    new AssistantToolResultPart(
                        "toolCallId",
                        "toolName",
                        new ToolModelMessage(
                            "toolCallId",
                            "toolName",
                            "{\"some\":\"result\"}",
                            false
                        ),
                        Options("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}")
                    )
                )
            )
        );
        Assert.Equal("toolCallId", result.ToolCallId);
        Assert.Equal("toolName", result.ToolName);
        Assert.Equal("json", result.Output.OutputType);
        Assert.Equal("{\"some\":\"result\"}", result.Output.OutputJson);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            result.ProviderOptions!["test-provider"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + " > provider-executed tool calls and results::should include providerExecuted flag",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Provider_executed_tool_calls_and_results_should_include_providerExecuted_flag()
    {
        const string optionsJson = "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}";

        var parts = Convert(
            new AssistantToolCallPart(
                new GeneratedToolCall("toolCallId", "toolName", "{}") { ProviderExecuted = true },
                Options(optionsJson)
            ),
            new AssistantToolResultPart(
                "toolCallId",
                "toolName",
                new ToolModelMessage("toolCallId", "toolName", "{\"some\":\"result\"}", false),
                Options(optionsJson)
            )
        );

        Assert.Equal(2, parts.Count);
        var call = Assert.IsType<AssistantToolCallPart>(parts[0]);
        Assert.True(call.ToolCall.ProviderExecuted);
        Assert.Equal(optionsJson, call.ProviderOptions!["test-provider"].GetRawText());
        var result = Assert.IsType<AssistantToolResultPart>(parts[1]);
        Assert.Equal("{\"some\":\"result\"}", result.Output.OutputJson);
        Assert.Equal(optionsJson, result.ProviderOptions!["test-provider"].GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Id + " > file parts::should convert file data correctly",
        Coverage = UpstreamCoverage.Partial,
        Note = "FileContentPart takes decoded bytes, so the base64 string input of upstream is decoded by the caller; the string handling is not exercised."
    )]
    public void Partial_Should_convert_file_data_correctly()
    {
        var file = Assert.IsType<AssistantFilePart>(
            Assert.Single(
                Convert(new AssistantFilePart("application/pdf", null, "test"u8.ToArray()))
            )
        );
        Assert.Equal("test"u8.ToArray(), file.Data);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Null(file.FileName);
    }

    [Fact]
    [UpstreamTest(
        Id + " > file parts::should preserve filename when present",
        Coverage = UpstreamCoverage.Partial,
        Note = "FileContentPart takes decoded bytes, so the base64 string input of upstream is decoded by the caller; the string handling is not exercised."
    )]
    public void Partial_Should_preserve_filename_when_present()
    {
        var file = Assert.IsType<AssistantFilePart>(
            Assert.Single(
                Convert(
                    new AssistantFilePart(
                        "application/pdf",
                        null,
                        "test"u8.ToArray(),
                        "test-document.pdf"
                    )
                )
            )
        );
        Assert.Equal("test"u8.ToArray(), file.Data);
        Assert.Equal("test-document.pdf", file.FileName);
    }

    [Fact]
    [UpstreamTest(
        Id + " > file parts::should handle provider options",
        Coverage = UpstreamCoverage.Partial,
        Note = "FileContentPart takes decoded bytes, so the base64 string input of upstream is decoded by the caller; the string handling is not exercised."
    )]
    public void Partial_Should_handle_provider_options()
    {
        var file = Assert.IsType<AssistantFilePart>(
            Assert.Single(
                Convert(
                    new AssistantFilePart(
                        "application/pdf",
                        null,
                        "test"u8.ToArray(),
                        null,
                        Options("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}")
                    )
                )
            )
        );
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            file.ProviderOptions!["test-provider"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + " > reasoning parts::should pass through provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Reasoning_should_pass_through_provider_options()
    {
        var part = Assert.IsType<AssistantReasoningPart>(
            Assert.Single(
                Convert(
                    new AssistantReasoningPart(
                        "hello, world!",
                        Options("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}")
                    )
                )
            )
        );
        Assert.Equal("hello, world!", part.Text);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            part.ProviderOptions!["test-provider"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + " > tool call parts::should pass through provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Tool_call_should_pass_through_provider_options()
    {
        var call = Assert.IsType<AssistantToolCallPart>(
            Assert.Single(
                Convert(
                    new AssistantToolCallPart(
                        new GeneratedToolCall("toolCallId", "toolName", "{}"),
                        Options("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}")
                    )
                )
            )
        );
        Assert.Equal("toolCallId", call.ToolCall.ToolCallId);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            call.ProviderOptions!["test-provider"].GetRawText()
        );
    }
}
