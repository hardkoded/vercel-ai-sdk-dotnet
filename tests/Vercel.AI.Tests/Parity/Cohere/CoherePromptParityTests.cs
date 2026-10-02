// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Cohere prompt conversion and tool preparation.</summary>
public sealed class CoherePromptParityTests
{
    private const string Convert = "packages/cohere/src/convert-to-cohere-chat-prompt.test.ts::convert to cohere chat prompt > ";

    private const string Tools = "packages/cohere/src/cohere-prepare-tools.test.ts::";

    [Fact]
    [UpstreamTest(Convert + "file processing::should extract documents from file parts", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_documents_from_file_parts()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new TextContentPart("Analyze this file: "),
                File("test.txt", "text/plain", "This is file content"),
            }),
        });

        CohereParity.JsonEqual(result.MessagesJson, """[{"role":"user","content":"Analyze this file: "}]""");
        CohereParity.JsonEqual(result.DocumentsJson, """[{"data":{"text":"This is file content","title":"test.txt"}}]""");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Convert + "file processing::should accept top-level-only mediaType without error (category D: mediaType not consumed)", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_a_top_level_text_media_type()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { File("test.txt", "text", "This is file content") }),
        });

        CohereParity.JsonEqual(result.MessagesJson, """[{"role":"user","content":""}]""");
        CohereParity.JsonEqual(result.DocumentsJson, """[{"data":{"text":"This is file content","title":"test.txt"}}]""");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Convert + "file processing::should not read mediaType (document payload carries only text + title)", Coverage = UpstreamCoverage.Covered)]
    public void Document_payload_carries_text_and_title_only()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { File("test.pdf", "application/pdf", "PDF-like content") }),
        });

        CohereParity.JsonEqual(result.DocumentsJson, """[{"data":{"text":"PDF-like content","title":"test.pdf"}}]""");
        var payload = result.MessagesJson + result.DocumentsJson;
        Assert.DoesNotContain("application/pdf", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("mediaType", payload, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Convert + "image processing::should convert image file with data bytes into image_url data URI", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_bytes_to_a_data_uri()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new TextContentPart("What is in this image?"),
                Image(new byte[] { 0, 1, 2, 3 }),
            }),
        });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """
            [{"role":"user","content":[
              {"type":"text","text":"What is in this image?"},
              {"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw=="}}
            ]}]
            """);
        Assert.Equal("[]", result.DocumentsJson);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Convert + "image processing::should convert image file with URL data into image_url URL", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_image_url()
    {
        var image = new CohereFilePart("image/png") { Url = "https://example.com/cat.png" };
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { image }),
        });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """[{"role":"user","content":[{"type":"image_url","image_url":{"url":"https://example.com/cat.png"}}]}]""");
        Assert.Equal("[]", result.DocumentsJson);
    }

    [Fact]
    [UpstreamTest(Convert + "image processing::should pass through detail provider option as image_url.detail", Coverage = UpstreamCoverage.Covered)]
    public void Passes_image_detail()
    {
        var image = Image(new byte[] { 0, 1, 2, 3 });
        image.Detail = "high";
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { image }),
        });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """[{"role":"user","content":[{"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw==","detail":"high"}}]}]""");
    }

    [Fact]
    [UpstreamTest(Convert + "image processing::should omit detail when no provider option is set", Coverage = UpstreamCoverage.Covered)]
    public void Omits_image_detail_when_unset()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { Image(new byte[] { 0, 1, 2, 3 }) }),
        });
        var image = JsonNode.Parse(result.MessagesJson)![0]!["content"]![0]!["image_url"]!;

        Assert.Equal("image_url", JsonNode.Parse(result.MessagesJson)![0]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.False(image.AsObject().ContainsKey("detail"));
    }

    [Fact]
    [UpstreamTest(Convert + "image processing::should send image inline and route non-image file to documents", Coverage = UpstreamCoverage.Covered)]
    public void Splits_an_image_from_a_document()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new TextContentPart("See attached:"),
                Image(new byte[] { 0, 1, 2, 3 }),
                File("note.txt", "text/plain", "Doc text"),
            }),
        });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """
            [{"role":"user","content":[
              {"type":"text","text":"See attached:"},
              {"type":"image_url","image_url":{"url":"data:image/png;base64,AAECAw=="}}
            ]}]
            """);
        CohereParity.JsonEqual(result.DocumentsJson, """[{"data":{"text":"Doc text","title":"note.txt"}}]""");
    }

    [Fact]
    [UpstreamTest(Convert + "image processing::should accept top-level \"image\" media type and detect full type from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_from_a_bare_image_media_type()
    {
        var image = new CohereFilePart("image")
        {
            Bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        };
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { image }),
        });
        var url = JsonNode.Parse(result.MessagesJson)![0]!["content"]![0]!["image_url"]!["url"]!.GetValue<string>();

        Assert.Equal("image_url", JsonNode.Parse(result.MessagesJson)![0]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("data:image/png;base64,iVBORw0KGgo=", url);
    }

    [Fact]
    [UpstreamTest(Convert + "tool messages::should convert a tool call into a cohere chatbot message", Coverage = UpstreamCoverage.Covered)]
    public void Assistant_tool_calls_omit_content()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new AssistantModelMessage(
                "Calling a tool",
                new[] { new GeneratedToolCall("tool-call-1", "tool-1", "{\"test\":\"This is a tool message\"}") },
                null),
        });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """
            [{
              "role": "assistant",
              "tool_calls": [{
                "id": "tool-call-1",
                "type": "function",
                "function": {"name":"tool-1","arguments":"{\"test\":\"This is a tool message\"}"}
              }]
            }]
            """);
        Assert.False(JsonNode.Parse(result.MessagesJson)![0]!.AsObject().ContainsKey("content"));
        Assert.Equal("[]", result.DocumentsJson);
    }

    [Fact]
    [UpstreamTest(Convert + "tool messages::should convert a single tool result into a cohere tool message", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_single_tool_result()
    {
        var result = CohereChatMapping.Convert(new ModelMessage[]
        {
            new ToolModelMessage("tool-call-1", "tool-1", "{\"test\":\"This is a tool message\"}", false),
        });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """[{"role":"tool","content":"{\"test\":\"This is a tool message\"}","tool_call_id":"tool-call-1"}]""");
    }

    [Fact]
    [UpstreamTest(Convert + "tool messages::should convert multiple tool results into a cohere tool message", Coverage = UpstreamCoverage.Covered)]
    public void Converts_multiple_tool_results_into_separate_messages()
    {
        var first = new CohereToolResultPart("tool-call-1", "json")
        {
            Json = CohereParity.Element("{\"test\":\"This is a tool message\"}"),
        };
        var second = new CohereToolResultPart("tool-call-2", "json")
        {
            Json = CohereParity.Element("{\"something\":\"else\"}"),
        };
        var result = CohereChatMapping.Convert(new ModelMessage[] { new CohereToolMessage(new[] { first, second }) });

        CohereParity.JsonEqual(
            result.MessagesJson,
            """
            [
              {"role":"tool","content":"{\"test\":\"This is a tool message\"}","tool_call_id":"tool-call-1"},
              {"role":"tool","content":"{\"something\":\"else\"}","tool_call_id":"tool-call-2"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Convert + "provider reference::should throw for file parts with provider references", Coverage = UpstreamCoverage.Covered)]
    public void Provider_file_references_throw()
    {
        var file = new CohereFilePart("text/plain") { ProviderReference = true };
        var exception = Assert.Throws<InvalidOperationException>(() => CohereChatMapping.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { file }),
        }));

        Assert.Contains("'file parts with provider references' functionality not supported", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Tools + "should return undefined tools when no tools are provided", Coverage = UpstreamCoverage.Covered)]
    public void Empty_tools_stay_unset()
    {
        var result = CohereChatMapping.PrepareTools(Array.Empty<CohereToolDefinition>(), null);

        Assert.Null(result.ToolsJson);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "should process function tools correctly", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_function_tool()
    {
        var result = CohereChatMapping.PrepareTools(
            new[] { CohereToolDefinition.Function("testFunction", "test description", CohereParity.Element("{\"type\":\"object\",\"properties\":{}}")) },
            null);

        CohereParity.JsonEqual(
            result.ToolsJson!,
            """[{"type":"function","function":{"name":"testFunction","description":"test description","parameters":{"type":"object","properties":{}}}}]""");
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "should add warnings for provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Warns_about_provider_defined_tools()
    {
        var result = CohereChatMapping.PrepareTools(new[] { CohereToolDefinition.Provider("provider.tool", "tool") }, null);

        Assert.Equal("[]", result.ToolsJson);
        Assert.Null(result.ToolChoice);
        Assert.Equal("unsupported", result.Warnings[0].Type);
        Assert.Equal("provider-defined tool provider.tool", result.Warnings[0].Feature);
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle auto tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Auto_tool_choice_stays_unset()
    {
        var result = PrepareBasic(ToolChoice.Auto);

        Assert.Null(result.ToolChoice);
        Assert.Contains("testFunction", result.ToolsJson!, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle none tool choice", Coverage = UpstreamCoverage.Covered)]
    public void None_tool_choice_is_none()
    {
        var result = PrepareBasic(ToolChoice.None);

        Assert.Equal("NONE", result.ToolChoice);
        Assert.Contains("testFunction", result.ToolsJson!, StringComparison.Ordinal);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle required tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Required_tool_choice_is_required()
    {
        var result = PrepareBasic(ToolChoice.Required);

        Assert.Equal("REQUIRED", result.ToolChoice);
        Assert.Contains("test description", result.ToolsJson!, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle tool type tool choice by filtering tools", Coverage = UpstreamCoverage.Covered)]
    public void Named_tool_choice_filters_and_requires()
    {
        var tools = new[]
        {
            CohereToolDefinition.Function("testFunction", "test description", CohereParity.Element("{\"type\":\"object\",\"properties\":{}}")),
            CohereToolDefinition.Function("other", "other description", CohereParity.Element("{\"type\":\"object\"}")),
        };
        var result = CohereChatMapping.PrepareTools(tools, ToolChoice.Tool("testFunction"));

        Assert.Equal("REQUIRED", result.ToolChoice);
        var parsed = JsonNode.Parse(result.ToolsJson!)!.AsArray();
        Assert.Single(parsed);
        Assert.Equal("testFunction", parsed[0]!["function"]!["name"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    private static CoherePreparedTools PrepareBasic(ToolChoice choice)
    {
        return CohereChatMapping.PrepareTools(
            new[] { CohereToolDefinition.Function("testFunction", "test description", CohereParity.Element("{\"type\":\"object\",\"properties\":{}}")) },
            choice);
    }

    private static CohereFilePart File(string name, string mediaType, string text)
    {
        return new CohereFilePart(mediaType)
        {
            Bytes = Encoding.UTF8.GetBytes(text),
            FileName = name,
        };
    }

    private static CohereFilePart Image(byte[] bytes)
    {
        return new CohereFilePart("image/png") { Bytes = bytes };
    }
}
