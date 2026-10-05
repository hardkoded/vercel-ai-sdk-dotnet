// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt;

public sealed class FilePartDataTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::wraps a Uint8Array as { type: \"data\", data }", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_bytes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromBytes(bytes));
        Assert.Equal("data", result.DataType);
        Assert.Same(bytes, result.Bytes);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::wraps an ArrayBuffer (converted to Uint8Array) as { type: \"data\", data }", Coverage = UpstreamCoverage.Covered)]
    public void Copies_array_buffer_bytes()
    {
        var bytes = new byte[] { 4, 5, 6 };
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromArrayBuffer(bytes));
        Assert.Equal("data", result.DataType);
        Assert.NotSame(bytes, result.Bytes);
        Assert.Equal(new byte[] { 4, 5, 6 }, result.Bytes);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::wraps a base64 string that is not a URL as { type: \"data\", data }", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_base64_text()
    {
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromString("aGVsbG8="));
        Assert.Equal("data", result.DataType);
        Assert.Equal("aGVsbG8=", result.Base64);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::converts a URL string into { type: \"url\", url }", Coverage = UpstreamCoverage.Covered)]
    public void Parses_https_url_string()
    {
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromString("https://example.com/file.pdf"));
        Assert.Equal("url", result.DataType);
        Assert.Equal("https://example.com/file.pdf", result.Url!.AbsoluteUri);
        Assert.Null(result.OriginalUrl);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::preserves the original string when URL parsing changes a non-HTTP URI", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_original_gs_url()
    {
        const string original = "gs://my-bucket/folder/My File.pdf";
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromString(original));
        Assert.Equal("url", result.DataType);
        Assert.Equal(original, result.OriginalUrl);
        Assert.Contains("My%20File.pdf", result.Url!.AbsoluteUri, StringComparison.Ordinal);
        Assert.StartsWith("gs://", result.Url.AbsoluteUri, StringComparison.Ordinal);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::passes through a URL instance as { type: \"url\", url }", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_url_instance()
    {
        var url = new Uri("https://example.com/file.pdf");
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromUrl(url));
        Assert.Equal("url", result.DataType);
        Assert.Same(url, result.Url);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::extracts base64 and media type from a data URL into { type: \"data\", data }", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_data_url()
    {
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromString("data:text/plain;base64,aGVsbG8="));
        Assert.Equal("data", result.DataType);
        Assert.Equal("aGVsbG8=", result.Base64);
        Assert.Equal("text/plain", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy bare shapes::wraps a provider reference as { type: \"reference\", reference }", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_provider_reference()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-123", ["anthropic"] = "file-abc" };
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromReference(reference));
        Assert.Equal("reference", result.DataType);
        Assert.Same(reference, result.Reference);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::unwraps { type: \"data\", data: Uint8Array }", Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_tagged_bytes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedData(bytes));
        Assert.Equal("data", result.DataType);
        Assert.Same(bytes, result.Bytes);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::unwraps { type: \"data\", data: ArrayBuffer }", Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_tagged_array_buffer()
    {
        var bytes = new byte[] { 4, 5, 6 };
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedDataBuffer(bytes));
        Assert.Equal("data", result.DataType);
        Assert.NotSame(bytes, result.Bytes);
        Assert.Equal(new byte[] { 4, 5, 6 }, result.Bytes);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::unwraps { type: \"data\", data: base64 string } that is not a URL", Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_tagged_base64()
    {
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedData("aGVsbG8="));
        Assert.Equal("data", result.DataType);
        Assert.Equal("aGVsbG8=", result.Base64);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::rejects { type: \"data\", data: data URL string } — data URLs are not inline data", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_inline_data_url()
    {
        var error = Assert.Throws<InvalidDataContentException>(() => FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedData("data:text/plain;base64,aGVsbG8=")));
        Assert.Contains("Data URLs are not valid inline data", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::unwraps { type: \"url\", url } into { type: \"url\", url }", Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_tagged_url()
    {
        var url = new Uri("https://example.com/file.pdf");
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedUrl(url));
        Assert.Equal("url", result.DataType);
        Assert.Same(url, result.Url);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::unwraps { type: \"url\", url } with data URL into base64 + mediaType", Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_tagged_data_url()
    {
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedUrl(new Uri("data:text/plain;base64,aGVsbG8=")));
        Assert.Equal("data", result.DataType);
        Assert.Equal("aGVsbG8=", result.Base64);
        Assert.Equal("text/plain", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::passes through { type: \"reference\", reference }", Coverage = UpstreamCoverage.Covered)]
    public void Passes_tagged_reference()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-123", ["anthropic"] = "file-abc" };
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedReference(reference));
        Assert.Equal("reference", result.DataType);
        Assert.Same(reference, result.Reference);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > tagged shapes::passes through { type: \"text\", text }", Coverage = UpstreamCoverage.Covered)]
    public void Passes_tagged_text()
    {
        var result = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedText("hello"));
        Assert.Equal("text", result.DataType);
        Assert.Equal("hello", result.Text);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy and tagged produce the same output::{ type: \"data\", data: bytes } equals bare bytes", Coverage = UpstreamCoverage.Covered)]
    public void Tagged_bytes_match_bare_bytes()
    {
        var bytes = new byte[] { 7, 8, 9 };
        var tagged = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedData(bytes));
        var bare = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromBytes(bytes));
        Assert.Equal(bare.DataType, tagged.DataType);
        Assert.Equal(bare.Bytes, tagged.Bytes);
        Assert.Equal(bare.MediaType, tagged.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy and tagged produce the same output::{ type: \"url\", url } equals bare URL", Coverage = UpstreamCoverage.Covered)]
    public void Tagged_url_matches_bare_url()
    {
        var url = new Uri("https://example.com/file.pdf");
        var tagged = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedUrl(url));
        var bare = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromUrl(url));
        Assert.Equal(bare.DataType, tagged.DataType);
        Assert.Same(bare.Url, tagged.Url);
        Assert.Equal(bare.MediaType, tagged.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/file-part-data.test.ts::convertToLanguageModelV4FilePart > legacy and tagged produce the same output::{ type: \"reference\", reference } equals bare reference", Coverage = UpstreamCoverage.Covered)]
    public void Tagged_reference_matches_bare_reference()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-123" };
        var tagged = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.TaggedReference(reference));
        var bare = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromReference(reference));
        Assert.Equal(bare.DataType, tagged.DataType);
        Assert.Same(bare.Reference, tagged.Reference);
        Assert.Equal(bare.MediaType, tagged.MediaType);
    }
}

public sealed class CreateToolModelOutputTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > error cases::should return error type with string value when isError is true and output is string", Coverage = UpstreamCoverage.Covered)]
    public void Error_text_keeps_a_string()
    {
        var result = ErrorText("Error message");
        Assert.Equal("error-text", result.Type);
        Assert.Equal("Error message", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > error cases::should return error type with JSON stringified value when isError is true and output is not string", Coverage = UpstreamCoverage.Covered)]
    public void Error_text_stringifies_an_object()
    {
        var output = new Dictionary<string, object?> { ["error"] = "Something went wrong", ["code"] = 500 };
        var result = (ErrorTextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), output, true, null, "text");
        Assert.Equal("{\"error\":\"Something went wrong\",\"code\":500}", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > error cases::should return error type with JSON stringified value for complex objects", Coverage = UpstreamCoverage.Covered)]
    public void Error_text_stringifies_a_nested_object()
    {
        var output = new Dictionary<string, object?>
        {
            ["message"] = "Complex error",
            ["details"] = new Dictionary<string, object?>
            {
                ["timestamp"] = "2023-01-01T00:00:00Z",
                ["stack"] = new[] { "line1", "line2" },
            },
        };
        var result = (ErrorTextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), output, true, null, "text");
        Assert.Equal("{\"message\":\"Complex error\",\"details\":{\"timestamp\":\"2023-01-01T00:00:00Z\",\"stack\":[\"line1\",\"line2\"]}}", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > tool with toModelOutput::should use tool.toModelOutput when available", Coverage = UpstreamCoverage.Covered)]
    public void Uses_to_model_output()
    {
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput(
            "123",
            Empty(),
            "test output",
            true,
            call => new TextToolModelOutput("Custom output: " + call.Output),
            "none");
        Assert.Equal("Custom output: test output", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > tool with toModelOutput::should use tool.toModelOutput with complex output", Coverage = UpstreamCoverage.Covered)]
    public void Uses_to_model_output_for_json()
    {
        var output = Json("{\"data\":[1,2,3],\"status\":\"success\"}");
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput(
            "123",
            Empty(),
            output,
            true,
            call => new JsonToolModelOutput(Json("{\"processed\":" + ((JsonElement)call.Output!).GetRawText() + ",\"timestamp\":\"2023-01-01\"}")),
            "none");
        Assert.Equal("success", result.Value.GetProperty("processed").GetProperty("status").GetString());
        Assert.Equal(1, result.Value.GetProperty("processed").GetProperty("data")[0].GetInt32());
        Assert.Equal("2023-01-01", result.Value.GetProperty("timestamp").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > tool with toModelOutput::should use tool.toModelOutput returning content type", Coverage = UpstreamCoverage.Covered)]
    public void Uses_content_to_model_output()
    {
        var result = (ContentToolModelOutput)ToolOutputs.CreateToolModelOutput(
            "123",
            Empty(),
            "any output",
            true,
            _ => new ContentToolModelOutput(new[]
            {
                new ToolModelContentPart("text", "Here is the result:"),
                new ToolModelContentPart("text", "Additional information"),
            }),
            "none");
        Assert.Equal("text", result.Value[0].Type);
        Assert.Equal("Here is the result:", result.Value[0].Text);
        Assert.Equal("Additional information", result.Value[1].Text);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > this binding::should preserve `this` when calling a class-based tool.toModelOutput", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_delegate_target()
    {
        var tool = new WeatherTool();
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), "21", true, tool.ToModelOutput, "none");
        Assert.Equal("21°C", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > string output without toModelOutput::should return text type for string output", Coverage = UpstreamCoverage.Covered)]
    public void String_output_is_text()
    {
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), "Simple string output", true, null, "none");
        Assert.Equal("Simple string output", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > string output without toModelOutput::should return text type for string output even with tool that has no toModelOutput", Coverage = UpstreamCoverage.Covered)]
    public void String_output_stays_text_without_converter()
    {
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), "String output", true, null, "none");
        Assert.Equal("String output", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > string output without toModelOutput::should return text type for empty string", Coverage = UpstreamCoverage.Covered)]
    public void Empty_string_is_text()
    {
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), string.Empty, true, null, "none");
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should return json type for object output", Coverage = UpstreamCoverage.Covered)]
    public void Object_output_is_json()
    {
        var output = new Dictionary<string, object?> { ["result"] = "success", ["data"] = new object[] { 1, 2, 3 } };
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), output, true, null, "none");
        Assert.Equal("success", result.Value.GetProperty("result").GetString());
        Assert.Equal(3, result.Value.GetProperty("data").GetArrayLength());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should JSON serialize object output", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_to_json_and_omits_undefined()
    {
        var output = new Dictionary<string, object?>
        {
            ["id"] = new ObjectIdLike(),
            ["omitted"] = ToolOutputs.Undefined.Value,
        };
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), output, true, null, "none");
        Assert.Equal("{\"id\":\"507f1f77bcf86cd799439011\"}", result.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should preserve %s properties", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_prototype_key_names()
    {
        foreach (var serialized in new[]
        {
            "{\"rows\":[{\"__proto__\":\"value\"}]}",
            "{\"rows\":[{\"constructor\":{\"prototype\":{\"value\":true}}}]}",
        })
        {
            var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), Json(serialized), true, null, "none");
            Assert.Equal("json", result.Type);
            Assert.Equal(serialized, result.Value.GetRawText());
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should return json type for array output", Coverage = UpstreamCoverage.Covered)]
    public void Array_output_is_json()
    {
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), new object[] { 1, 2, 3, "test" }, true, null, "none");
        Assert.Equal("[1,2,3,\"test\"]", result.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should return json type for number output", Coverage = UpstreamCoverage.Covered)]
    public void Number_output_is_json()
    {
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), 42, true, null, "none");
        Assert.Equal(42, result.Value.GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should return json type for boolean output", Coverage = UpstreamCoverage.Covered)]
    public void Boolean_output_is_json()
    {
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), true, true, null, "none");
        Assert.True(result.Value.GetBoolean());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should return json type for null output", Coverage = UpstreamCoverage.Covered)]
    public void Null_output_is_json_null()
    {
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), null, true, null, "none");
        Assert.Equal(JsonValueKind.Null, result.Value.ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > non-string output without toModelOutput::should return json type for complex nested object", Coverage = UpstreamCoverage.Covered)]
    public void Nested_object_is_json()
    {
        var output = Json("{\"user\":{\"id\":123,\"name\":\"John Doe\",\"preferences\":{\"theme\":\"dark\",\"notifications\":true}},\"metadata\":{\"timestamp\":\"2023-01-01T00:00:00Z\",\"version\":\"1.0.0\"},\"items\":[{\"id\":1,\"name\":\"Item 1\"},{\"id\":2,\"name\":\"Item 2\"}]}");
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), output, true, null, "none");
        Assert.Equal("John Doe", result.Value.GetProperty("user").GetProperty("name").GetString());
        Assert.Equal("dark", result.Value.GetProperty("user").GetProperty("preferences").GetProperty("theme").GetString());
        Assert.True(result.Value.GetProperty("user").GetProperty("preferences").GetProperty("notifications").GetBoolean());
        Assert.Equal("1.0.0", result.Value.GetProperty("metadata").GetProperty("version").GetString());
        Assert.Equal("Item 2", result.Value.GetProperty("items")[1].GetProperty("name").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > edge cases::should prioritize isError over tool.toModelOutput", Coverage = UpstreamCoverage.Covered)]
    public void Error_mode_skips_to_model_output()
    {
        var result = (ErrorTextToolModelOutput)ToolOutputs.CreateToolModelOutput(
            "123",
            Empty(),
            "Error occurred",
            true,
            _ => throw new InvalidOperationException("toModelOutput should not be called"),
            "text");
        Assert.Equal("Error occurred", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > edge cases::should handle undefined output in error text case", Coverage = UpstreamCoverage.Covered)]
    public void Undefined_error_text_is_unknown()
    {
        var result = (ErrorTextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), null, false, null, "text");
        Assert.Equal("unknown error", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > edge cases::should use null for undefined output in error json case", Coverage = UpstreamCoverage.Covered)]
    public void Undefined_error_json_is_null()
    {
        var result = (ErrorJsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), null, false, null, "json");
        Assert.Equal(JsonValueKind.Null, result.Value.ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > edge cases::should use null for undefined output in non-error case", Coverage = UpstreamCoverage.Covered)]
    public void Undefined_output_is_json_null()
    {
        var result = (JsonToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), null, false, null, "none");
        Assert.Equal(JsonValueKind.Null, result.Value.ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > arguments::should pass toolCallId to tool.toModelOutput", Coverage = UpstreamCoverage.Covered)]
    public void Passes_tool_call_id()
    {
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput(
            "2344",
            Empty(),
            "test",
            true,
            call => new TextToolModelOutput("Tool call ID: " + call.ToolCallId),
            "none");
        Assert.Equal("Tool call ID: 2344", result.Value);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/create-tool-model-output.test.ts::createToolModelOutput > arguments::should pass input to tool.toModelOutput", Coverage = UpstreamCoverage.Covered)]
    public void Passes_input()
    {
        var result = (TextToolModelOutput)ToolOutputs.CreateToolModelOutput(
            "2344",
            Json("{\"number\":8877}"),
            "test",
            true,
            call => new TextToolModelOutput("Input: " + call.Input!.Value.GetProperty("number").GetInt32().ToString()),
            "none");
        Assert.Equal("Input: 8877", result.Value);
    }

    private static ErrorTextToolModelOutput ErrorText(string output)
    {
        return (ErrorTextToolModelOutput)ToolOutputs.CreateToolModelOutput("123", Empty(), output, true, null, "text");
    }

    private static JsonElement? Empty()
    {
        return Json("{}");
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private sealed class WeatherTool
    {
        private readonly string _unit = "°C";

        public ToolModelOutput ToModelOutput(ToolModelOutputCall call)
        {
            return new TextToolModelOutput(call.Output + _unit);
        }
    }

    private sealed class ObjectIdLike : IToolJsonValue
    {
        public object? ToJson()
        {
            return "507f1f77bcf86cd799439011";
        }
    }
}

public sealed class NormalizeStreamProviderErrorTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::normalizes a typed overloaded provider error", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_typed_provider_error()
    {
        var data = new Dictionary<string, object?>
        {
            ["message"] = "Overloaded",
            ["type"] = "overloaded_error",
            ["code"] = "provider_overloaded",
        };
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new ProviderStreamError("Overloaded", data, "overloaded_error", "provider_overloaded", 529, true))!;
        Assert.Equal("Overloaded", error.Message);
        Assert.Equal("overloaded_error", error.Type);
        Assert.Equal("provider_overloaded", error.Code);
        Assert.Equal(529, error.StatusCode);
        Assert.True(error.IsRetryable);
        Assert.Same(data, error.Data);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::uses provider-owned metadata without exposing the metadata wrapper as data", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_provider_data_payload()
    {
        var data = new Dictionary<string, object?>
        {
            ["error"] = new Dictionary<string, object?>
            {
                ["message"] = "Request too large",
                ["type"] = "request_too_large",
            },
        };
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new ProviderStreamError("Request too large", data, "request_too_large", null, 413, false))!;
        Assert.Equal("Request too large", error.Message);
        Assert.Equal("request_too_large", error.Type);
        Assert.Equal(413, error.StatusCode);
        Assert.False(error.IsRetryable);
        Assert.Same(data, error.Data);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::normalizes a message-only \"%s\" provider error", Coverage = UpstreamCoverage.Covered)]
    public void Infers_status_from_known_messages()
    {
        foreach (var (message, status) in new[] { ("Internal server error", 500), ("Overloaded", 503) })
        {
            var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new Dictionary<string, object?> { ["message"] = message })!;
            Assert.Equal(message, error.Message);
            Assert.Null(error.Type);
            Assert.Null(error.Code);
            Assert.Equal(status, error.StatusCode);
            Assert.True(error.IsRetryable);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::uses explicit status and retry metadata when available", Coverage = UpstreamCoverage.Covered)]
    public void Uses_explicit_status_and_retry()
    {
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new Dictionary<string, object?>
        {
            ["message"] = "Request rejected",
            ["type"] = "provider_rejection",
            ["status_code"] = 422,
            ["is_retryable"] = true,
        })!;
        Assert.Equal("Request rejected", error.Message);
        Assert.Equal("provider_rejection", error.Type);
        Assert.Equal(422, error.StatusCode);
        Assert.True(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::normalizes nested response failure payloads", Coverage = UpstreamCoverage.Covered)]
    public void Reads_nested_response_error()
    {
        var data = new Dictionary<string, object?>
        {
            ["type"] = "response.failed",
            ["response"] = new Dictionary<string, object?>
            {
                ["error"] = new Dictionary<string, object?>
                {
                    ["code"] = "rate_limit_exceeded",
                    ["message"] = "Try again later",
                },
            },
        };
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(data)!;
        Assert.Equal("Try again later", error.Message);
        Assert.Equal("response.failed", error.Type);
        Assert.Equal("rate_limit_exceeded", error.Code);
        Assert.Null(error.StatusCode);
        Assert.False(error.IsRetryable);
        Assert.Same(data, error.Data);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::preserves provider type and code as separate discriminators", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_type_and_code()
    {
        var data = new Dictionary<string, object?>
        {
            ["type"] = "error",
            ["code"] = "rate_limit_exceeded",
            ["message"] = "Rate limit reached",
        };
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(data)!;
        Assert.Equal("Rate limit reached", error.Message);
        Assert.Equal("error", error.Type);
        Assert.Equal("rate_limit_exceeded", error.Code);
        Assert.Null(error.StatusCode);
        Assert.False(error.IsRetryable);
        Assert.Same(data, error.Data);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::preserves the provider type and code when code is HTTP status %p", Coverage = UpstreamCoverage.Covered)]
    public void Http_status_code_sets_status_and_stays_on_the_error()
    {
        foreach (object code in new object[] { "429", 429 })
        {
            var data = new Dictionary<string, object?>
            {
                ["message"] = "Rate limit reached",
                ["type"] = "rate_limit_error",
                ["code"] = code,
            };
            var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(data)!;
            Assert.Equal("Rate limit reached", error.Message);
            Assert.Equal("rate_limit_error", error.Type);
            Assert.Equal(code, error.Code);
            Assert.Equal(429, error.StatusCode);
            Assert.True(error.IsRetryable);
            Assert.Same(data, error.Data);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::uses explicit status metadata for non-retryable provider errors", Coverage = UpstreamCoverage.Covered)]
    public void Explicit_424_is_not_retryable()
    {
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new Dictionary<string, object?>
        {
            ["message"] = "A required provider dependency is unavailable",
            ["type"] = "failed_dependency",
            ["statusCode"] = 424,
        })!;
        Assert.Equal(424, error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::uses conservative metadata for unknown message-only errors", Coverage = UpstreamCoverage.Covered)]
    public void Unknown_message_has_no_status()
    {
        var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new Dictionary<string, object?>
        {
            ["message"] = "Provider-specific failure",
        })!;
        Assert.Null(error.StatusCode);
        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::does not infer metadata from arbitrary provider type \"%s\"", Coverage = UpstreamCoverage.Covered)]
    public void Provider_type_does_not_imply_status()
    {
        foreach (var type in new[] { "timeout_warning", "not_found_in_cache" })
        {
            var error = (StreamProviderError)StreamErrors.NormalizeStreamProviderError(new Dictionary<string, object?>
            {
                ["message"] = "Provider-specific failure",
                ["type"] = type,
            })!;
            Assert.Equal(type, error.Type);
            Assert.Null(error.StatusCode);
            Assert.False(error.IsRetryable);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::preserves existing Error instances", Coverage = UpstreamCoverage.Covered)]
    public void Returns_exceptions_unchanged()
    {
        var error = new InvalidOperationException("existing error");
        Assert.Same(error, StreamErrors.NormalizeStreamProviderError(error));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::preserves cross-realm AI SDK errors", Coverage = UpstreamCoverage.Covered)]
    public void Returns_preserved_sdk_errors_unchanged()
    {
        var error = new PreservedStreamError("existing SDK error", true);
        Assert.Same(error, StreamErrors.NormalizeStreamProviderError(error));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::preserves non-normalizable values", Coverage = UpstreamCoverage.Covered)]
    public void Returns_non_normalizable_values()
    {
        const string text = "plain string";
        Assert.Same(text, StreamErrors.NormalizeStreamProviderError(text));
        var typeOnly = new Dictionary<string, object?> { ["type"] = "overloaded_error" };
        Assert.Same(typeOnly, StreamErrors.NormalizeStreamProviderError(typeOnly));
        var numericMessage = new Dictionary<string, object?> { ["message"] = 123 };
        Assert.Same(numericMessage, StreamErrors.NormalizeStreamProviderError(numericMessage));
        Assert.Null(StreamErrors.NormalizeStreamProviderError(null));
    }
}
