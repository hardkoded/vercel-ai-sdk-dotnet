// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::";

    private static string ToolResult(string toolName, string toolCallId, string output) =>
        "{\"type\":\"tool-result\",\"toolName\":\""
        + toolName
        + "\",\"toolCallId\":\""
        + toolCallId
        + "\",\"output\":"
        + output
        + "}";

    private static string ToolMessage(params string[] results) =>
        "{\"role\":\"tool\",\"content\":[" + string.Join(",", results) + "]}";

    private static AnthropicPromptResult RunTool(string toolName, string id, string output) =>
        Run("[" + ToolMessage(ToolResult(toolName, id, output)) + "]", sendReasoning: true);

    private static void AssertResult(
        string expectedMessages,
        AnthropicPromptResult result,
        params string[] betas
    )
    {
        AssertJson(expectedMessages, result.Messages);
        Assert.Null(result.System);
        Assert.Equal(betas.OrderBy(b => b), result.Betas.OrderBy(b => b));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert a single tool result into an anthropic user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_single_tool_result_into_an_anthropic_user_message()
    {
        var result = RunTool(
            "tool-1",
            "tool-call-1",
            "{\"type\":\"json\",\"value\":{\"test\":\"This is a tool message\"}}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"tool-call-1\",\"content\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\"}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert multiple tool results into an anthropic user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_multiple_tool_results_into_an_anthropic_user_message()
    {
        var result = Run(
            "["
                + ToolMessage(
                    ToolResult(
                        "tool-1",
                        "tool-call-1",
                        "{\"type\":\"json\",\"value\":{\"test\":\"This is a tool message\"}}"
                    ),
                    ToolResult(
                        "tool-2",
                        "tool-call-2",
                        "{\"type\":\"json\",\"value\":{\"something\":\"else\"}}"
                    )
                )
                + "]"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"tool-call-1\",\"content\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\"},{\"type\":\"tool_result\",\"tool_use_id\":\"tool-call-2\",\"content\":\"{\\\"something\\\":\\\"else\\\"}\"}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle tool result with content parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_result_with_content_parts()
    {
        var result = RunTool(
            "image-generator",
            "image-gen-1",
            "{\"type\":\"content\",\"value\":[{\"type\":\"text\",\"text\":\"Image generated successfully\"},{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"AAECAw==\"},\"mediaType\":\"image/png\"}]}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"image-gen-1\",\"content\":[{\"type\":\"text\",\"text\":\"Image generated successfully\"},{\"type\":\"image\",\"source\":{\"type\":\"base64\",\"media_type\":\"image/png\",\"data\":\"AAECAw==\"}}]}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle tool result with PDF content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_result_with_PDF_content()
    {
        var result = RunTool(
            "pdf-generator",
            "pdf-gen-1",
            "{\"type\":\"content\",\"value\":[{\"type\":\"text\",\"text\":\"PDF generated successfully\"},{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"JVBERi0xLjQKJeLjz9MKNCAwIG9iago=\"},\"mediaType\":\"application/pdf\"}]}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"pdf-gen-1\",\"content\":[{\"type\":\"text\",\"text\":\"PDF generated successfully\"},{\"type\":\"document\",\"source\":{\"type\":\"base64\",\"media_type\":\"application/pdf\",\"data\":\"JVBERi0xLjQKJeLjz9MKNCAwIG9iago=\"}}]}]}]",
            result,
            "pdfs-2024-09-25"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle tool result with a provider-referenced file",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_result_with_a_provider_referenced_file()
    {
        var result = RunTool(
            "get-file",
            "get-file-1",
            "{\"type\":\"content\",\"value\":[{\"type\":\"file\",\"data\":{\"type\":\"reference\",\"reference\":{\"anthropic\":\"file-txt-12345\"}},\"mediaType\":\"text/plain\"}]}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"get-file-1\",\"content\":[{\"type\":\"document\",\"source\":{\"type\":\"file\",\"file_id\":\"file-txt-12345\"}}]}]}]",
            result,
            "files-api-2025-04-14"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should handle tool result with custom tool-reference content for custom tool search",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_result_with_custom_tool_reference_content_for_custom_tool_search()
    {
        var result = RunTool(
            "searchTools",
            "search-1",
            "{\"type\":\"content\",\"value\":[{\"type\":\"custom\",\"providerOptions\":{\"anthropic\":{\"type\":\"tool-reference\",\"toolName\":\"get_weather\"}}},{\"type\":\"custom\",\"providerOptions\":{\"anthropic\":{\"type\":\"tool-reference\",\"toolName\":\"get_forecast\"}}}]}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"search-1\",\"content\":[{\"type\":\"tool_reference\",\"tool_name\":\"get_weather\"},{\"type\":\"tool_reference\",\"tool_name\":\"get_forecast\"}]}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle tool result with url-based PDF content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_result_with_url_based_PDF_content()
    {
        var result = RunTool(
            "get-pdf",
            "get-pdf-1",
            "{\"type\":\"content\",\"value\":[{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/document.pdf\"},\"mediaType\":\"application/pdf\"}]}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"get-pdf-1\",\"content\":[{\"type\":\"document\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/document.pdf\"}}]}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle tool result with url-based image content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_result_with_url_based_image_content()
    {
        var result = RunTool(
            "image-generator",
            "image-gen-1",
            "{\"type\":\"content\",\"value\":[{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"},\"mediaType\":\"image/png\"}]}"
        );

        AssertResult(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"image-gen-1\",\"content\":[{\"type\":\"image\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"}}]}]}]",
            result
        );
    }
}
