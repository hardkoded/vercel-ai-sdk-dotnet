// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Upstream <c>convert-to-google-messages</c> outcomes.</summary>
public sealed class GoogleMessageConversionTests
{
    private const string File = "packages/google/src/convert-to-google-messages.test.ts";

    [Fact]
    [UpstreamTest(File + "::system messages::should store system message in system instruction", Coverage = UpstreamCoverage.Covered)]
    public void Stores_system_message_in_system_instruction()
    {
        var result = GoogleMessages.Convert(new ModelMessage[] { new SystemModelMessage("Test") }, "gemini-2.0-flash", vertex: false);
        GoogleParity.Prompt(result, "[]", "{\"parts\":[{\"text\":\"Test\"}]}");
    }

    [Fact]
    [UpstreamTest(File + "::system messages::should throw error when there was already a user message", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_a_system_message_follows_a_user_message()
    {
        var error = Assert.Throws<InvalidOperationException>(() => GoogleMessages.Convert(
            new ModelMessage[]
            {
                new UserModelMessage("Test"),
                new SystemModelMessage("Test"),
            },
            "gemini-2.0-flash",
            vertex: false));
        Assert.Equal("system messages are only supported at the beginning of the conversation", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures::should preserve thought signatures in assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_thought_signatures_in_assistant_messages()
    {
        var result = Convert(Assistant(
            GooglePromptPart.TextPart("Regular text", Options("{\"thoughtSignature\":\"sig1\"}")),
            GooglePromptPart.ReasoningPart("Reasoning text", Options("{\"thoughtSignature\":\"sig2\"}")),
            GooglePromptPart.ToolCall("call1", "test", "{\"value\":\"test\"}", Options("{\"thoughtSignature\":\"sig3\"}"))));
        GoogleParity.Prompt(result, """
            [{"role":"model","parts":[
              {"text":"Regular text","thoughtSignature":"sig1"},
              {"text":"Reasoning text","thought":true,"thoughtSignature":"sig2"},
              {"functionCall":{"name":"test","args":{"value":"test"},"id":"call1"},"thoughtSignature":"sig3"}
            ]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures with vertex providerOptionsName::should resolve thoughtSignature from google namespace when using vertex providerOptionsName", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_a_google_signature_when_the_vertex_namespace_is_preferred()
    {
        var result = Convert(
            Assistant(
                GooglePromptPart.TextPart("Regular text", Google("{\"thoughtSignature\":\"sig1\"}")),
                GooglePromptPart.ReasoningPart("Reasoning text", Google("{\"thoughtSignature\":\"sig2\"}")),
                GooglePromptPart.ToolCall("call1", "getWeather", "{\"location\":\"London\"}", Google("{\"thoughtSignature\":\"sig3\"}"))),
            Names("googleVertex", "vertex"));
        GoogleParity.Equal(result.Contents[0]!["parts"]![2], """
            {"functionCall":{"name":"getWeather","args":{"location":"London"},"id":"call1"},"thoughtSignature":"sig3"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures with vertex providerOptionsName::should prefer vertex namespace over google namespace when both are present", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_vertex_namespace_over_google()
    {
        var options = Full("{\"vertex\":{\"thoughtSignature\":\"vertex_sig\"},\"google\":{\"thoughtSignature\":\"google_sig\"}}");
        var result = Convert(Assistant(GooglePromptPart.ToolCall("call1", "getWeather", "{\"location\":\"London\"}", options)), Names("googleVertex", "vertex"));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"functionCall":{"name":"getWeather","args":{"location":"London"},"id":"call1"},"thoughtSignature":"vertex_sig"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures with vertex providerOptionsName::should resolve thoughtSignature from vertex namespace directly", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_a_vertex_signature_directly()
    {
        var result = Convert(
            Assistant(GooglePromptPart.ToolCall("call1", "getWeather", "{\"location\":\"London\"}", Full("{\"vertex\":{\"thoughtSignature\":\"vertex_sig\"}}"))),
            Names("googleVertex", "vertex"));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"functionCall":{"name":"getWeather","args":{"location":"London"},"id":"call1"},"thoughtSignature":"vertex_sig"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures with google providerOptionsName (gateway failover)::should resolve thoughtSignature from vertex namespace when using google providerOptionsName", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_a_vertex_signature_when_google_is_the_preferred_namespace()
    {
        var result = Convert(Assistant(
            GooglePromptPart.TextPart("Regular text", Full("{\"vertex\":{\"thoughtSignature\":\"sig1\"}}")),
            GooglePromptPart.ReasoningPart("Reasoning text", Full("{\"vertex\":{\"thoughtSignature\":\"sig2\"}}")),
            GooglePromptPart.ToolCall("call1", "getWeather", "{\"location\":\"London\"}", Full("{\"vertex\":{\"thoughtSignature\":\"sig3\"}}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]![2], """
            {"functionCall":{"name":"getWeather","args":{"location":"London"},"id":"call1"},"thoughtSignature":"sig3"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures with google providerOptionsName (gateway failover)::should prefer google namespace over vertex namespace when both are present", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_google_namespace_over_vertex()
    {
        var options = Full("{\"google\":{\"thoughtSignature\":\"google_sig\"},\"vertex\":{\"thoughtSignature\":\"vertex_sig\"}}");
        var result = Convert(Assistant(GooglePromptPart.ToolCall("call1", "getWeather", "{\"location\":\"London\"}", options)));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"functionCall":{"name":"getWeather","args":{"location":"London"},"id":"call1"},"thoughtSignature":"google_sig"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::thought signatures with google providerOptionsName (gateway failover)::should resolve thoughtSignature from vertex namespace when google namespace is absent (default providerOptionsName)", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_a_vertex_signature_when_the_google_namespace_is_absent()
    {
        var result = Convert(Assistant(GooglePromptPart.ToolCall("call1", "getWeather", "{\"location\":\"London\"}", Full("{\"vertex\":{\"thoughtSignature\":\"vertex_sig\"}}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"functionCall":{"name":"getWeather","args":{"location":"London"},"id":"call1"},"thoughtSignature":"vertex_sig"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::Gemma model system instructions::should prepend system instruction to first user message for Gemma models", Coverage = UpstreamCoverage.Covered)]
    public void Prepends_the_system_instruction_for_Gemma()
    {
        var result = GoogleMessages.Convert(
            new ModelMessage[] { new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello") },
            "gemma-3-12b-it",
            vertex: false);
        Assert.Null(result.SystemInstruction);
        GoogleParity.Equal(result.Contents, """
            [{"role":"user","parts":[{"text":"You are a helpful assistant.\n\n"},{"text":"Hello"}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::Gemma model system instructions::should handle multiple system messages for Gemma models", Coverage = UpstreamCoverage.Covered)]
    public void Joins_multiple_Gemma_system_messages()
    {
        var result = GoogleMessages.Convert(
            new ModelMessage[]
            {
                new SystemModelMessage("You are helpful."),
                new SystemModelMessage("Be concise."),
                new UserModelMessage("Hi"),
            },
            "gemma-3-27b-it",
            vertex: false);
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], "{\"text\":\"You are helpful.\\n\\nBe concise.\\n\\n\"}");
    }

    [Fact]
    [UpstreamTest(File + "::Gemma model system instructions::should not affect non-Gemma models", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_system_instructions_on_non_Gemma_models()
    {
        var result = GoogleMessages.Convert(
            new ModelMessage[] { new SystemModelMessage("You are helpful."), new UserModelMessage("Hello") },
            "gemini-2.0-flash",
            vertex: false);
        GoogleParity.Prompt(result, "[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello\"}]}]", "{\"parts\":[{\"text\":\"You are helpful.\"}]}");
    }

    [Fact]
    [UpstreamTest(File + "::Gemma model system instructions::should handle Gemma model with system instruction but no user messages", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_Gemma_system_instruction_when_there_is_no_user_message()
    {
        var result = GoogleMessages.Convert(new ModelMessage[] { new SystemModelMessage("You are helpful.") }, "gemma-2-9b", vertex: false);
        GoogleParity.Prompt(result, "[]");
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should preserve original Google Cloud Storage file URIs", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_original_Google_Cloud_Storage_file_URIs()
    {
        var result = Convert(User(GooglePromptPart.FileUrl("gs://my-bucket/folder/My File.pdf", "application/pdf", "gs://my-bucket/folder/My File.pdf")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"fileData":{"mimeType":"application/pdf","fileUri":"gs://my-bucket/folder/My File.pdf"}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should add image parts", Coverage = UpstreamCoverage.Covered)]
    public void Adds_image_parts()
    {
        var result = Convert(User(GooglePromptPart.FileBase64("AAECAw==", "image/png")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], "{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AAECAw==\"}}");
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should add file parts for base64 encoded files", Coverage = UpstreamCoverage.Covered)]
    public void Adds_file_parts_for_base64_encoded_files()
    {
        var result = Convert(User(GooglePromptPart.FileBase64("AAECAw==", "image/png")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], "{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AAECAw==\"}}");
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should convert file parts with provider reference to fileData", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_provider_reference_to_file_data()
    {
        var result = Convert(User(GooglePromptPart.FileReference(
            "{\"google\":\"https://generativelanguage.googleapis.com/v1beta/files/abc123\",\"openai\":\"file-xyz789\"}",
            "image/png")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"fileData":{"mimeType":"image/png","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/abc123"}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should convert image file parts with provider reference to fileData", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_image_provider_reference_to_file_data()
    {
        var result = Convert(User(GooglePromptPart.FileReference(
            "{\"google\":\"https://generativelanguage.googleapis.com/v1beta/files/img456\"}",
            "image/jpeg")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"fileData":{"mimeType":"image/jpeg","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/img456"}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::user messages::should throw when provider reference is missing google key in user file part", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_a_user_file_reference_has_no_google_key()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Convert(User(GooglePromptPart.FileReference("{\"openai\":\"file-xyz789\"}", "image/png"))));
        Assert.Equal("No provider reference found for provider 'google'. Available providers: openai", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should convert tool result messages to function responses", Coverage = UpstreamCoverage.Covered)]
    public void Converts_tool_results_to_function_responses()
    {
        var result = Convert(Tool(GooglePromptPart.ToolResultJson("testCallId", "testFunction", "{\"someData\":\"test result\"}")));
        GoogleParity.Equal(result.Contents, """
            [{"role":"user","parts":[{"functionResponse":{"name":"testFunction","response":{"name":"testFunction","content":{"someData":"test result"}},"id":"testCallId"}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should convert tool result content with image-data into functionResponse parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_data_tool_results_into_function_response_parts()
    {
        var result = Convert(Tool(GooglePromptPart.ToolResultContent(
            "testCallId",
            "imageGenerator",
            new[]
            {
                GooglePromptPart.TextPart("Here is the generated image:"),
                GooglePromptPart.FileBase64("base64encodedimagedata", "image/jpeg"),
            })));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"functionResponse":{"name":"imageGenerator","response":{"name":"imageGenerator","content":"Here is the generated image:"},"id":"testCallId","parts":[{"inlineData":{"mimeType":"image/jpeg","data":"base64encodedimagedata"}}]}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should convert tool result content with file-data into functionResponse parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_file_data_tool_results_into_function_response_parts()
    {
        var result = Convert(Tool(GooglePromptPart.ToolResultContent(
            "testCallId",
            "documentReader",
            new[] { GooglePromptPart.FileBase64("base64pdfdata", "application/pdf") })));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0]!["functionResponse"]!, """
            {"name":"documentReader","response":{"name":"documentReader","content":"Tool executed successfully."},"id":"testCallId","parts":[{"inlineData":{"mimeType":"application/pdf","data":"base64pdfdata"}}]}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should convert tool result content with image-url data URL into functionResponse parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_data_url_tool_result_into_inline_data()
    {
        var result = Convert(Tool(GooglePromptPart.ToolResultContent(
            "testCallId",
            "imageGenerator",
            new[] { GooglePromptPart.FileUrl("data:image/png;base64,base64pngdata", "image/png") })));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0]!["functionResponse"]!["parts"]![0], "{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"base64pngdata\"}}");
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should forward non-data image-url tool result parts as text content", Coverage = UpstreamCoverage.Covered)]
    public void Forwards_a_non_data_image_url_as_text()
    {
        var result = Convert(Tool(GooglePromptPart.ToolResultContent(
            "testCallId",
            "imageGenerator",
            new[] { GooglePromptPart.FileUrl("https://example.com/image.png", "image/png") })));
        Assert.Equal(
            "{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"},\"mediaType\":\"image/png\"}",
            result.Contents[0]!["parts"]![0]!["functionResponse"]!["response"]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should forward non-data file-url tool result parts as text content", Coverage = UpstreamCoverage.Covered)]
    public void Forwards_a_non_data_file_url_as_text()
    {
        var result = Convert(Tool(GooglePromptPart.ToolResultContent(
            "testCallId",
            "documentReader",
            new[] { GooglePromptPart.FileUrl("https://example.com/report.pdf", "application/pdf") })));
        Assert.Equal(
            "{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/report.pdf\"},\"mediaType\":\"application/pdf\"}",
            result.Contents[0]!["parts"]![0]!["functionResponse"]!["response"]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should use legacy tool-result conversion when functionResponse parts are unsupported", Coverage = UpstreamCoverage.Covered)]
    public void Uses_legacy_tool_result_conversion_when_parts_are_unsupported()
    {
        var result = Convert(
            Tool(GooglePromptPart.ToolResultContent(
                "testCallId",
                "imageGenerator",
                new[]
                {
                    GooglePromptPart.TextPart("Here is the generated image:"),
                    GooglePromptPart.FileBase64("base64encodedimagedata", "image/jpeg"),
                    GooglePromptPart.FileBase64("base64pdfdata", "application/pdf"),
                })),
            new GoogleMessageConversionOptions { SupportsFunctionResponseParts = false });
        GoogleParity.Equal(result.Contents[0]!["parts"]!, """
            [
              {"functionResponse":{"name":"imageGenerator","response":{"name":"imageGenerator","content":"Here is the generated image:"},"id":"testCallId"}},
              {"inlineData":{"mimeType":"image/jpeg","data":"base64encodedimagedata"}},
              {"text":"Tool executed successfully and returned this image as a response"},
              {"inlineData":{"mimeType":"application/pdf","data":"base64pdfdata"}},
              {"text":"Tool executed successfully and returned this file as a response"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::issue #16072: should not serialize PDF file tool results as text on the non-Gemini-3 path", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_serialize_pdf_tool_results_as_text_on_the_legacy_path()
    {
        var result = GoogleMessages.Convert(
            new ModelMessage[]
            {
                new GooglePromptMessage(
                    "tool",
                    null,
                    new[]
                    {
                        GooglePromptPart.ToolResultContent(
                            "testCallId",
                            "catalogSearch",
                            new[]
                            {
                                GooglePromptPart.TextPart("metadata"),
                                GooglePromptPart.FileBase64("JVBERi0xLjQK", "application/pdf"),
                            }),
                    }),
            },
            "gemini-2.5-flash",
            vertex: false);
        var serialized = result.Contents.ToJsonString();
        Assert.DoesNotContain("\"text\":\"JVBERi0xLjQK\"", serialized, StringComparison.Ordinal);
        GoogleParity.Equal(result.Contents[0]!["parts"]![1], "{\"inlineData\":{\"mimeType\":\"application/pdf\",\"data\":\"JVBERi0xLjQK\"}}");
    }

    [Fact]
    [UpstreamTest(File + "::tool messages::should keep URL tool result parts on the legacy path", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_url_tool_results_as_text_on_the_legacy_path()
    {
        var result = Convert(
            Tool(GooglePromptPart.ToolResultContent(
                "testCallId",
                "documentReader",
                new[]
                {
                    GooglePromptPart.FileUrl("https://example.com/image.png", "image/png"),
                    GooglePromptPart.FileUrl("https://example.com/report.pdf", "application/pdf"),
                })),
            new GoogleMessageConversionOptions { SupportsFunctionResponseParts = false });
        Assert.Equal(
            "{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"},\"mediaType\":\"image/png\"}",
            result.Contents[0]!["parts"]![0]!["text"]!.GetValue<string>());
        Assert.Equal(
            "{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/report.pdf\"},\"mediaType\":\"application/pdf\"}",
            result.Contents[0]!["parts"]![1]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should add PNG image parts for base64 encoded files", Coverage = UpstreamCoverage.Covered)]
    public void Adds_png_image_parts_for_assistant_files()
    {
        var result = Convert(Assistant(GooglePromptPart.FileBase64("AAECAw==", "image/png")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], "{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AAECAw==\"}}");
        Assert.Equal("model", result.Contents[0]!["role"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should include thought flag on file parts when set in providerOptions", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_thought_flag_on_assistant_file_parts()
    {
        var result = Convert(Assistant(
            GooglePromptPart.FileBase64("AAECAw==", "image/png", Options("{\"thought\":true,\"thoughtSignature\":\"sig1\"}")),
            GooglePromptPart.FileBase64("BAUG", "image/jpeg")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"inlineData":{"mimeType":"image/png","data":"AAECAw=="},"thought":true,"thoughtSignature":"sig1"}
            """);
        Assert.Null(result.Contents[0]!["parts"]![1]!["thoughtSignature"]);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should convert reasoning-file parts with thought flag and signature", Coverage = UpstreamCoverage.Covered)]
    public void Converts_reasoning_files_with_a_signature()
    {
        var result = Convert(Assistant(GooglePromptPart.ReasoningFile("data", "AAECAw==", null, null, "image/png", Options("{\"thoughtSignature\":\"sig_reasoning_file\"}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"inlineData":{"mimeType":"image/png","data":"AAECAw=="},"thought":true,"thoughtSignature":"sig_reasoning_file"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should convert reasoning-file parts without thoughtSignature", Coverage = UpstreamCoverage.Covered)]
    public void Converts_reasoning_files_without_a_signature()
    {
        var result = Convert(Assistant(GooglePromptPart.ReasoningFile("data", "BAUG", null, null, "image/jpeg", null)));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], "{\"inlineData\":{\"mimeType\":\"image/jpeg\",\"data\":\"BAUG\"},\"thought\":true}");
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should throw error for URL file data in reasoning-file assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_url_data_in_reasoning_files()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Convert(Assistant(
            GooglePromptPart.ReasoningFile("url", null, null, "https://example.com/image.png", "image/png", null))));
        Assert.Equal("File data URLs in assistant messages are not supported", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should handle mixed reasoning, reasoning-file, text, and tool-call parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_mixed_reasoning_file_and_text_parts()
    {
        var result = Convert(Assistant(
            GooglePromptPart.ReasoningPart("Thinking about this...", Options("{\"thoughtSignature\":\"sig1\"}")),
            GooglePromptPart.ReasoningFile("data", "AAECAw==", null, null, "image/png", Options("{\"thoughtSignature\":\"sig2\"}")),
            GooglePromptPart.TextPart("Here is my response", Options("{\"thoughtSignature\":\"sig3\"}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]!, """
            [
              {"text":"Thinking about this...","thought":true,"thoughtSignature":"sig1"},
              {"inlineData":{"mimeType":"image/png","data":"AAECAw=="},"thought":true,"thoughtSignature":"sig2"},
              {"text":"Here is my response","thoughtSignature":"sig3"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should throw error for URL file data in assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_url_file_data_in_assistant_messages()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Convert(Assistant(GooglePromptPart.FileUrl("https://example.com/image.png", "image/png"))));
        Assert.Equal("File data URLs in assistant messages are not supported", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should convert assistant file parts with provider reference to fileData", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_assistant_provider_reference_to_file_data()
    {
        var result = Convert(Assistant(GooglePromptPart.FileReference(
            "{\"google\":\"https://generativelanguage.googleapis.com/v1beta/files/abc123\"}",
            "image/png")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"fileData":{"mimeType":"image/png","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/abc123"}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should convert assistant file parts with provider reference and thought flag", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_assistant_provider_reference_with_a_thought_flag()
    {
        var result = Convert(Assistant(GooglePromptPart.FileReference(
            "{\"google\":\"https://generativelanguage.googleapis.com/v1beta/files/abc123\"}",
            "image/png",
            Options("{\"thought\":true,\"thoughtSignature\":\"sig1\"}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"fileData":{"mimeType":"image/png","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/abc123"},"thought":true,"thoughtSignature":"sig1"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::assistant messages::should throw when provider reference is missing google key in assistant file part", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_an_assistant_file_reference_has_no_google_key()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Convert(Assistant(GooglePromptPart.FileReference("{\"openai\":\"file-xyz789\"}", "image/png"))));
        Assert.Equal("No provider reference found for provider 'google'. Available providers: openai", error.Message);
    }

    [Fact]
    [UpstreamTest(File + "::parallel tool calls::should include thought signature on functionCall when provided", Coverage = UpstreamCoverage.Covered)]
    public void Includes_a_thought_signature_on_a_function_call()
    {
        var result = Convert(Assistant(GooglePromptPart.ToolCall("call1", "test", "{\"value\":\"test\"}", Options("{\"thoughtSignature\":\"sig\"}"))));
        Assert.Equal("sig", result.Contents[0]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "::tool results with thought signatures::should include thought signature on functionCall but not on functionResponse", Coverage = UpstreamCoverage.Covered)]
    public void Puts_the_thought_signature_on_the_function_call_only()
    {
        var result = Convert(
            Assistant(GooglePromptPart.ToolCall("call1", "test", "{}", Options("{\"thoughtSignature\":\"sig\"}"))),
            Tool(GooglePromptPart.ToolResultJson("call1", "test", "{\"ok\":true}", Options("{\"thoughtSignature\":\"sig\"}"))));
        Assert.Equal("sig", result.Contents[0]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
        Assert.Null(result.Contents[1]!["parts"]![0]!["thoughtSignature"]);
    }

    [Fact]
    [UpstreamTest(File + "::server tool combination round-trip::should convert assistant tool-call with serverToolCallId to toolCall wire format", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_server_tool_call_to_the_tool_call_wire_format()
    {
        var result = Convert(Assistant(GooglePromptPart.ToolCall(
            "tc-1",
            "server:GOOGLE_SEARCH_WEB",
            "{\"query\":\"test\"}",
            Options("{\"serverToolCallId\":\"server-id-1\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\"}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], """
            {"toolCall":{"toolType":"GOOGLE_SEARCH_WEB","args":{"query":"test"},"id":"server-id-1"}}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::server tool combination round-trip::should convert assistant tool-call without serverToolCallId to functionCall wire format", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_client_tool_call_to_a_function_call()
    {
        var result = Convert(Assistant(GooglePromptPart.ToolCall("tc-1", "weather", "{\"location\":\"SF\"}")));
        Assert.NotNull(result.Contents[0]!["parts"]![0]!["functionCall"]);
        Assert.Null(result.Contents[0]!["parts"]![0]!["toolCall"]);
    }

    [Fact]
    [UpstreamTest(File + "::server tool combination round-trip::should convert tool result with serverToolCallId to toolResponse on last model content", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_server_tool_response_to_the_last_model_content()
    {
        var result = Convert(
            Assistant(GooglePromptPart.ToolCall(
                "tc-1",
                "server:GOOGLE_SEARCH_WEB",
                "{\"query\":\"test\"}",
                Options("{\"serverToolCallId\":\"server-id-1\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\"}"))),
            Tool(GooglePromptPart.ToolResultJson(
                "tc-1",
                "server:GOOGLE_SEARCH_WEB",
                "{\"results\":[\"a\"]}",
                Options("{\"serverToolCallId\":\"server-id-1\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\",\"thoughtSignature\":\"sig-resp\"}"))));
        Assert.Single(result.Contents);
        GoogleParity.Equal(result.Contents[0]!["parts"]![1], """
            {"toolResponse":{"toolType":"GOOGLE_SEARCH_WEB","response":{"results":["a"]},"id":"server-id-1"},"thoughtSignature":"sig-resp"}
            """);
    }

    [Fact]
    [UpstreamTest(File + "::server tool combination round-trip::should parse string input for server tool call args", Coverage = UpstreamCoverage.Covered)]
    public void Parses_string_input_for_server_tool_call_args()
    {
        var result = Convert(Assistant(GooglePromptPart.ToolCall(
            "tc-1",
            "server:GOOGLE_SEARCH_WEB",
            "{\"query\":\"hello\"}",
            Options("{\"serverToolCallId\":\"sid-1\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\"}"))));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0]!["toolCall"]!["args"]!, "{\"query\":\"hello\"}");
    }

    [Fact]
    [UpstreamTest(File + "::server tool combination round-trip::should pass object input directly for server tool call args", Coverage = UpstreamCoverage.Covered)]
    public void Passes_object_input_directly_for_server_tool_call_args()
    {
        var result = Convert(Assistant(GooglePromptPart.ToolCall(
            "tc-1",
            "server:GOOGLE_SEARCH_WEB",
            "{\"query\":\"hello\"}",
            Options("{\"serverToolCallId\":\"sid-1\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\"}"))));
        Assert.Equal("hello", result.Contents[0]!["parts"]![0]!["toolCall"]!["args"]!["query"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::injects skip_thought_signature_validator and emits a warning for Gemini 3 when a tool-call has no signature", Coverage = UpstreamCoverage.Covered)]
    public void Injects_the_thought_signature_sentinel_for_Gemini_3()
    {
        var result = GoogleMessages.Convert(UnsignedWeatherPrompt(), "gemini-3-flash", vertex: false);
        Assert.Equal(GoogleMessages.SkipThoughtSignatureValidator, result.Contents[1]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
        Assert.Contains("skip_thought_signature_validator", result.Warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("`weather`", result.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel or warn for unsigned parallel calls after a signed call", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_for_unsigned_calls_after_a_signed_call()
    {
        var result = Convert(
            Assistant(
                GooglePromptPart.ToolCall("tc_paris", "get_weather", "{\"city\":\"Paris\"}", Full("{\"vertex\":{\"thoughtSignature\":\"parallel_batch_signature\"}}")),
                GooglePromptPart.ToolCall("tc_tokyo", "get_weather", "{\"city\":\"Tokyo\"}"),
                GooglePromptPart.ToolCall("tc_new_york", "get_weather", "{\"city\":\"New York\"}")),
            new GoogleMessageConversionOptions { IsGemini3Model = true, ProviderOptionsNames = new[] { "googleVertex", "vertex" } });
        Assert.Equal("parallel_batch_signature", result.Contents[0]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
        Assert.Null(result.Contents[0]!["parts"]![1]!["thoughtSignature"]);
        Assert.Null(result.Contents[0]!["parts"]![2]!["thoughtSignature"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel when other response parts separate parallel function calls", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_when_text_separates_function_calls()
    {
        var result = Convert(
            Assistant(
                GooglePromptPart.ToolCall("tc_signed", "weather", "{\"location\":\"SF\"}", Options("{\"thoughtSignature\":\"signed_batch\"}")),
                GooglePromptPart.TextPart("Checking another city in the same response."),
                GooglePromptPart.ToolCall("tc_unsigned", "weather", "{\"location\":\"NYC\"}")),
            Gemini3());
        Assert.Null(result.Contents[0]!["parts"]![2]!["thoughtSignature"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel when server tool parts separate parallel function calls", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_when_a_server_tool_separates_function_calls()
    {
        var result = Convert(
            Assistant(
                GooglePromptPart.ToolCall("signed_function_call", "weather", "{\"location\":\"SF\"}", Options("{\"thoughtSignature\":\"function_signature\"}")),
                GooglePromptPart.ToolCall("server_call", "server:GOOGLE_SEARCH_WEB", "{\"query\":\"weather\"}", Options("{\"serverToolCallId\":\"server_call\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\",\"thoughtSignature\":\"server_call_signature\"}")),
                GooglePromptPart.ToolResultJson("server_call", "server:GOOGLE_SEARCH_WEB", "{\"results\":[]}", Options("{\"serverToolCallId\":\"server_call\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\",\"thoughtSignature\":\"server_response_signature\"}")),
                GooglePromptPart.ToolCall("unsigned_function_call", "weather", "{\"location\":\"NYC\"}")),
            Gemini3());
        Assert.Null(result.Contents[0]!["parts"]![3]!["thoughtSignature"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::injects the sentinel when a signed server tool call precedes an unsigned function call", Coverage = UpstreamCoverage.Covered)]
    public void Injects_the_sentinel_when_a_server_tool_precedes_an_unsigned_function_call()
    {
        var result = Convert(
            Assistant(
                GooglePromptPart.ToolCall("server_call", "server:GOOGLE_SEARCH_WEB", "{\"query\":\"weather\"}", Options("{\"serverToolCallId\":\"server_call\",\"serverToolType\":\"GOOGLE_SEARCH_WEB\",\"thoughtSignature\":\"server_signature\"}")),
                GooglePromptPart.ToolCall("function_call", "weather", "{\"location\":\"NYC\"}")),
            Gemini3());
        Assert.Equal(GoogleMessages.SkipThoughtSignatureValidator, result.Contents[0]!["parts"]![1]!["thoughtSignature"]!.GetValue<string>());
        Assert.Contains("`weather`", result.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel for non-Gemini-3 models", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_for_older_models()
    {
        var result = GoogleMessages.Convert(UnsignedWeatherPrompt(), "gemini-2.5-flash", vertex: false);
        Assert.Null(result.Contents[1]!["parts"]![0]!["thoughtSignature"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel when a real signature is present under `google`", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_when_a_google_signature_is_present()
    {
        var result = Convert(
            User(GooglePromptPart.TextPart("hi")),
            Assistant(GooglePromptPart.ToolCall("tc_1", "weather", "{\"location\":\"SF\"}", Options("{\"thoughtSignature\":\"real_sig\"}"))),
            Gemini3());
        Assert.Equal("real_sig", result.Contents[1]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel when a real signature is present under `vertex`", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_when_a_vertex_signature_is_present()
    {
        var result = Convert(
            User(GooglePromptPart.TextPart("hi")),
            Assistant(GooglePromptPart.ToolCall("tc_1", "weather", "{\"location\":\"SF\"}", Full("{\"vertex\":{\"thoughtSignature\":\"vertex_sig\"}}"))),
            new GoogleMessageConversionOptions { IsGemini3Model = true, ProviderOptionsNames = new[] { "googleVertex", "vertex" } });
        Assert.Equal("vertex_sig", result.Contents[1]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::does NOT inject the sentinel when a real signature is present under `googleVertex`", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_inject_the_sentinel_when_a_google_vertex_signature_is_present()
    {
        var result = Convert(
            User(GooglePromptPart.TextPart("hi")),
            Assistant(GooglePromptPart.ToolCall("tc_1", "weather", "{\"location\":\"SF\"}", Full("{\"googleVertex\":{\"thoughtSignature\":\"google_vertex_sig\"}}"))),
            Gemini3());
        Assert.Equal("google_vertex_sig", result.Contents[1]!["parts"]![0]!["thoughtSignature"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(File + "::Gemini 3 missing thoughtSignature mitigation::emits one warning per request listing each affected tool name", Coverage = UpstreamCoverage.Covered)]
    public void Emits_one_warning_listing_each_affected_tool()
    {
        var result = Convert(
            User(GooglePromptPart.TextPart("hi")),
            Assistant(
                GooglePromptPart.ToolCall("tc_1", "weather", "{\"location\":\"SF\"}"),
                GooglePromptPart.ToolCall("tc_2", "weather", "{\"location\":\"NYC\"}"),
                GooglePromptPart.ToolCall("tc_3", "search", "{\"query\":\"q\"}")),
            Gemini3());
        Assert.Single(result.Warnings);
        Assert.Contains("3 ", result.Warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("`weather`", result.Warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("`search`", result.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(File + "::top-level-only media type resolution::passes full image/png through unchanged for inline data", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_full_image_media_type_through_unchanged()
    {
        var result = Convert(User(GooglePromptPart.FileBase64("iVBORw0KGgo=", "image/png")));
        GoogleParity.Equal(result.Contents[0]!["parts"]![0], "{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"iVBORw0KGgo=\"}}");
    }

    private static ModelMessage[] UnsignedWeatherPrompt()
    {
        return new ModelMessage[]
        {
            new UserModelMessage("hi"),
            new GooglePromptMessage("assistant", null, new[] { GooglePromptPart.ToolCall("tc_1", "weather", "{\"location\":\"SF\"}") }),
            new GooglePromptMessage("tool", null, new[] { GooglePromptPart.ToolResultJson("tc_1", "weather", "{\"temperature\":72}") }),
        };
    }

    private static GooglePromptConversion Convert(params ModelMessage[] messages)
    {
        return GoogleMessages.Convert(messages, new GoogleMessageConversionOptions());
    }

    private static GooglePromptConversion Convert(ModelMessage message, GoogleMessageConversionOptions options)
    {
        return GoogleMessages.Convert(new ModelMessage[] { message }, options);
    }

    private static GooglePromptConversion Convert(ModelMessage first, ModelMessage second, GoogleMessageConversionOptions options)
    {
        return GoogleMessages.Convert(new ModelMessage[] { first, second }, options);
    }

    private static GoogleMessageConversionOptions Names(params string[] names)
    {
        return new GoogleMessageConversionOptions { ProviderOptionsNames = names };
    }

    private static GoogleMessageConversionOptions Gemini3()
    {
        return new GoogleMessageConversionOptions { IsGemini3Model = true };
    }

    private static GooglePromptMessage User(params GooglePromptPart[] parts)
    {
        return new GooglePromptMessage("user", null, parts);
    }

    private static GooglePromptMessage Assistant(params GooglePromptPart[] parts)
    {
        return new GooglePromptMessage("assistant", null, parts);
    }

    private static GooglePromptMessage Tool(params GooglePromptPart[] parts)
    {
        return new GooglePromptMessage("tool", null, parts);
    }

    private static JsonElement Options(string inner)
    {
        return GoogleParity.Json("{\"google\":" + inner + "}");
    }

    private static JsonElement Google(string inner)
    {
        return GoogleParity.Json("{\"google\":" + inner + "}");
    }

    private static JsonElement Full(string json)
    {
        return GoogleParity.Json(json);
    }
}
