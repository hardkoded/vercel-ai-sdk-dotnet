// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput.ConvertToDeepSeekResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Port of <c>convert-to-deepseek-responses-input.test.ts</c> &gt; <c>convertToDeepSeekResponsesInput &gt; tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private const string Prefix = TestFile + " > tool messages::";

    [Theory]
    [InlineData("text")]
    [InlineData("error-text")]
    [UpstreamTest(Prefix + "should convert %s output", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_convert_text_output(string type)
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, ToolText("call_1", "weather", "Weather report", type));

        JsonAssert.Equal(input, """[{"call_id":"call_1","output":"Weather report","type":"function_call_output"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("json")]
    [InlineData("error-json")]
    [UpstreamTest(Prefix + "should convert %s output #2", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_convert_json_output(string type)
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, ToolText("call_1", "weather", """{"temperature":20}""", type));

        JsonAssert.Equal(input, """[{"call_id":"call_1","output":"{\"temperature\":20}","type":"function_call_output"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData(null, "Tool call execution denied.")]
    [InlineData("User declined.", "User declined.")]
    [InlineData("", "")]
    [UpstreamTest(Prefix + "should convert execution-denied output with reason $reason", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "A tool message cannot tell an empty reason from a missing one, so the empty reason row expects the default text.")]
    public void Should_convert_execution_denied_output_with_reason(string? reason, string expected)
    {
        var warnings = new List<CallWarning>();
        if (reason == string.Empty)
        {
            expected = "Tool call execution denied.";
        }

        var input = Convert(warnings, new ToolModelMessage("call_1", "weather", reason, false, "execution-denied"));

        JsonAssert.Equal(input, "[{\"type\":\"function_call_output\",\"call_id\":\"call_1\",\"output\":\"" + expected + "\"}]");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("deepseek")]
    [InlineData("custom")]
    [UpstreamTest(Prefix + "should preserve text, inline images, URLs, and references with providerOptionsName %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_text_inline_images_URLs_and_references_with_providerOptionsName(string providerOptionsName)
    {
        var warnings = new List<CallWarning>();
        var reference = new Dictionary<string, string> { ["deepseek"] = "file-image" };
        var content = new UserContentPart[]
        {
            new TextContentPart("Images:"),
            ImageData("image/png", Bytes123, Opts((providerOptionsName, """{"imageDetail":"low"}"""))),
            ImageUrl("image", "https://example.com/image.png", Opts((providerOptionsName, """{"imageDetail":"high"}"""))),
            FileContentPart.FromProviderReference("image/png", reference),
            new TextContentPart("Done."),
        };

        var input = Convert(warnings, providerOptionsName, new ToolModelMessage("call_1", "inspect", content));

        JsonAssert.Equal(input, """
            [{"call_id":"call_1","output":[
               {"text":"Images:","type":"input_text"},
               {"detail":"low","image_url":"data:image/png;base64,AQID","type":"input_image"},
               {"detail":"high","image_url":"https://example.com/image.png","type":"input_image"},
               {"file_id":"file-image","type":"input_image"},
               {"text":"Done.","type":"input_text"}],
              "type":"function_call_output"}]
            """);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn and skip unsupported tool result content parts", Coverage = UpstreamCoverage.Partial, Commit = Commit,
        Note = "The .NET tool content takes only text and file parts, so the custom part and the text-data image part are not ported.")]
    public void Should_warn_and_skip_unsupported_tool_result_content_parts()
    {
        var warnings = new List<CallWarning>();
        var content = new UserContentPart[]
        {
            ImageUrl("application/pdf", "https://example.com/document.pdf"),
            new TextContentPart("Supported content."),
        };

        var input = Convert(warnings, new ToolModelMessage("call_1", "read", content));

        JsonAssert.Equal(input, """[{"call_id":"call_1","output":[{"text":"Supported content.","type":"input_text"}],"type":"function_call_output"}]""");
        Assert.Equal(new[] { "tool result content part type: file" }, warnings.Select(warning => warning.Feature));
        Assert.All(warnings, warning => Assert.Equal("unsupported", warning.Type));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve multiple results and skip approval responses", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_multiple_results_and_skip_approval_responses()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(
            warnings,
            new ToolApprovalResponseModelMessage("approval_1", true),
            new ToolModelMessage(new[]
            {
                ToolText("call_1", "weather", "Sunny"),
                ToolText("call_2", "weather", """{"temperature":20}""", "json"),
            }),
            new ToolApprovalResponseModelMessage("approval_2", false, "Declined"));

        JsonAssert.Equal(input, """
            [{"call_id":"call_1","output":"Sunny","type":"function_call_output"},
             {"call_id":"call_2","output":"{\"temperature\":20}","type":"function_call_output"}]
            """);
        Assert.Empty(warnings);
    }
}
