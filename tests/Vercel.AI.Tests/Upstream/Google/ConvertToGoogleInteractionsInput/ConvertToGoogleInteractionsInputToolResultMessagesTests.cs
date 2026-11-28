// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>tool-result messages</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputToolResultMessagesTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "tool-result messages::";

    [Fact]
    [UpstreamTest(
        Prefix + "maps a text tool result to a function_result block on a user turn",
        Coverage = UpstreamCoverage.Partial,
        Note = "The parity snapshot title. Upstream later renamed it to the top-level function_result it below, and the result is no longer nested in a user turn."
    )]
    [UpstreamTest(
        Prefix + "maps a text tool result to a top-level function_result input",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3d45b091f3107bbd4685ca8aaacdbed4ccf71b54"
    )]
    public void Maps_a_text_tool_result_to_a_top_level_function_result_input()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("What is the weather?"),
            S.Assistant(S.ToolCall("call_abc", "getWeather", "{\"location\":\"NYC\"}")),
            new ToolModelMessage("call_abc", "getWeather", "It is sunny.", false, "text")
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"What is the weather?\"}]},{\"type\":\"function_call\",\"id\":\"call_abc\",\"name\":\"getWeather\",\"arguments\":{\"location\":\"NYC\"}},{\"type\":\"function_result\",\"call_id\":\"call_abc\",\"name\":\"getWeather\",\"result\":\"It is sunny.\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes a json tool result to a stringified payload",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_a_json_tool_result_to_a_stringified_payload()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("q"),
            S.Assistant(S.ToolCall("call_x", "getWeather", "{}")),
            new ToolModelMessage(
                "call_x",
                "getWeather",
                "{\"temperature\":72,\"condition\":\"sunny\"}",
                false,
                "json"
            )
        );

        Assert.Equal(
            "{\"temperature\":72,\"condition\":\"sunny\"}",
            (string?)result.Input[2]!["result"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps an error-text tool result to is_error: true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_an_error_text_tool_result_to_is_error_true()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("q"),
            S.Assistant(S.ToolCall("call_x", "getWeather", "{}")),
            new ToolModelMessage("call_x", "getWeather", "API timeout", true, "error-text")
        );

        GoogleUpstream.JsonEqual(
            result.Input[2],
            "{\"type\":\"function_result\",\"call_id\":\"call_x\",\"name\":\"getWeather\",\"is_error\":true,\"result\":\"API timeout\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps a content output with mixed text and image parts to an array result",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_a_content_output_with_mixed_text_and_image_parts_to_an_array_result()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("q"),
            S.Assistant(S.ToolCall("call_x", "lookup", "{}")),
            new ToolModelMessage(
                "call_x",
                "lookup",
                new UserContentPart[]
                {
                    new TextContentPart("Here is the result:"),
                    new FileContentPart("image/png", null, new byte[] { 1, 2, 3 }, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input[2],
            "{\"type\":\"function_result\",\"call_id\":\"call_x\",\"name\":\"lookup\",\"result\":[{\"type\":\"text\",\"text\":\"Here is the result:\"},{\"type\":\"image\",\"data\":\"AQID\",\"mime_type\":\"image/png\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "nests a document inside its top-level function result",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3d45b091f3107bbd4685ca8aaacdbed4ccf71b54"
    )]
    public void Nests_a_document_inside_its_top_level_function_result()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("q"),
            S.Assistant(S.ToolCall("call_x", "lookup", "{}")),
            new ToolModelMessage(
                "call_x",
                "lookup",
                new UserContentPart[]
                {
                    new FileContentPart("application/pdf", null, new byte[] { 1, 2, 3 }, null),
                }
            )
        );

        Assert.Empty(result.Warnings);
        GoogleUpstream.JsonEqual(
            result.Input[2],
            "{\"type\":\"function_result\",\"call_id\":\"call_x\",\"name\":\"lookup\",\"result\":[{\"type\":\"document\",\"data\":\"AQID\",\"mime_type\":\"application/pdf\"}]}"
        );
    }
}
