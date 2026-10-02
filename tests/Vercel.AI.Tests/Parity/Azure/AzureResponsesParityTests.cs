// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Azure Responses request bodies, parsed content, and the file-citation stream.</summary>
public sealed class AzureResponsesParityTests
{
    private const string Generate = "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate::";

    private const string Stream = "packages/azure/src/azure-openai-provider.test.ts::responses > doStream::";

    private const string TextBody = """
        {
          "id": "resp_0d6bb044bb6ff37200698c51948054819385e24e2ad931ae6e",
          "created_at": 1770803604,
          "model": "gpt-5.1",
          "service_tier": "default",
          "output": [
            {
              "id": "msg_0d6bb044bb6ff37200698c51952e288193beb9044db4d8c810",
              "type": "message",
              "content": [{"type": "output_text", "text": "Word", "annotations": []}]
            }
          ],
          "usage": {
            "input_tokens": 11,
            "input_tokens_details": {"cached_tokens": 0},
            "output_tokens": 11,
            "output_tokens_details": {"reasoning_tokens": 0},
            "total_tokens": 22
          }
        }
        """;

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > text::should extract text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text_content()
    {
        var result = await GenerateText();

        var text = Assert.IsType<AzureGeneratedText>(result.Content[0]);
        Assert.Equal("Word", text.Text);
        Assert.Equal("text", text.Type);
        Assert.Equal("msg_0d6bb044bb6ff37200698c51952e288193beb9044db4d8c810", text.ProviderMetadata!.Value.GetProperty("azure").GetProperty("itemId").GetString());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Null(result.RawFinishReason);
        Assert.Equal("resp_0d6bb044bb6ff37200698c51948054819385e24e2ad931ae6e", result.ProviderMetadata!.Value.GetProperty("azure").GetProperty("responseId").GetString());
        Assert.Equal("default", result.ProviderMetadata.Value.GetProperty("azure").GetProperty("serviceTier").GetString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > tool call::should extract tool call content", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_a_function_tool_call()
    {
        var handler = AzureParity.Handler(
            """
            {
              "id": "resp_0a2fa1b539ba14ba00698c519df7a88194874af28c8bfccb12",
              "output": [
                {
                  "id": "fc_0a2fa1b539ba14ba00698c519ebab0819494302fc0b5c31440",
                  "type": "function_call",
                  "arguments": "{\"location\":\"San Francisco\"}",
                  "call_id": "call_YunNGbIwdVJ2i0y0Mybva4Pw",
                  "name": "weather"
                }
              ]
            }
            """);
        var result = await AzureParity.Provider(handler).Responses("test-deployment").DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("call_YunNGbIwdVJ2i0y0Mybva4Pw", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"location\":\"San Francisco\"}", call.ArgumentsJson);
        Assert.Equal("fc_0a2fa1b539ba14ba00698c519ebab0819494302fc0b5c31440", call.ProviderMetadata!.Value.GetProperty("azure").GetProperty("itemId").GetString());
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Generate + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_response_usage()
    {
        var usage = (await GenerateText()).Usage;

        Assert.Equal(11, usage.InputTokens);
        Assert.Equal(11, usage.OutputTokens);
        Assert.Equal(22, usage.TotalTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(0, usage.ReasoningTokens);
        Assert.Equal(11, usage.TextTokens);
        Assert.Equal(11, usage.Raw!.Value.GetProperty("input_tokens").GetInt32());
        Assert.Equal(0, usage.Raw.Value.GetProperty("input_tokens_details").GetProperty("cached_tokens").GetInt32());
        Assert.Equal(0, usage.Raw.Value.GetProperty("output_tokens_details").GetProperty("reasoning_tokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(Generate + "should extract response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_response_metadata()
    {
        var result = await GenerateText();

        Assert.Equal("resp_0d6bb044bb6ff37200698c51948054819385e24e2ad931ae6e", result.ResponseId);
        Assert.Equal("gpt-5.1", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1770803604), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest(Generate + "should extract response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_response_headers()
    {
        var handler = AzureParity.Handler(TextBody);
        handler.ResponseHeaders["test-header"] = "test-value";
        var result = await AzureParity.Provider(handler).Responses("test-deployment").DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);

        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Contains("application/json", result.ResponseHeaders["Content-Type"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Generate + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = AzureParity.Handler(TextBody);
        var provider = AzureParity.Provider(handler, options =>
        {
            options.Headers = new Dictionary<string, string?>
            {
                ["Custom-Provider-Header"] = "provider-header-value",
            };
        });
        var call = AzureParity.Hello();
        call.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };
        await provider.Responses("test-deployment").DoGenerateAsync(call, CancellationToken.None);

        Assert.Equal("test-api-key", handler.RequestHeaders["api-key"]);
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/azure/0.0.0-test", handler.RequestHeaders["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Generate + "should handle Azure file IDs with assistant- prefix", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_assistant_image_ids_as_file_ids()
    {
        await AssertInput(
            new UserContentPart[]
            {
                new TextContentPart("Analyze this image"),
                new AzureInputFile("image/jpeg", "assistant-abc123"),
            },
            """
            [{
              "role": "user",
              "content": [
                {"type": "input_text", "text": "Analyze this image"},
                {"type": "input_image", "file_id": "assistant-abc123"}
              ]
            }]
            """);
    }

    [Fact]
    [UpstreamTest(Generate + "should handle PDF files with assistant- prefix", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_assistant_pdf_ids_as_file_ids()
    {
        await AssertInput(
            new UserContentPart[]
            {
                new TextContentPart("Analyze this PDF"),
                new AzureInputFile("application/pdf", "assistant-pdf123"),
            },
            """
            [{
              "role": "user",
              "content": [
                {"type": "input_text", "text": "Analyze this PDF"},
                {"type": "input_file", "file_id": "assistant-pdf123"}
              ]
            }]
            """);
    }

    [Fact]
    [UpstreamTest(Generate + "should fall back to base64 for non-assistant file IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_non_assistant_image_strings_in_a_data_uri()
    {
        await AssertInput(
            new UserContentPart[]
            {
                new TextContentPart("Analyze this image"),
                new AzureInputFile("image/jpeg", "file-abc123"),
            },
            """
            [{
              "role": "user",
              "content": [
                {"type": "input_text", "text": "Analyze this image"},
                {"type": "input_image", "image_url": "data:image/jpeg;base64,file-abc123"}
              ]
            }]
            """);
    }

    [Fact]
    [UpstreamTest(Generate + "should send include provider option for file search results", Coverage = UpstreamCoverage.Covered)]
    public async Task File_search_tool_omits_include_unless_requested()
    {
        var (handler, result) = await Respond(
            TextBody,
            new[]
            {
                new AzureProviderTool(
                    "openai.file_search",
                    "file_search",
                    AzureParity.Element("{\"vectorStoreIds\":[\"vs_123\",\"vs_456\"],\"maxNumResults\":10,\"ranking\":{\"ranker\":\"auto\"}}")),
            },
            null);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "test-deployment",
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "tools": [{
                "type": "file_search",
                "vector_store_ids": ["vs_123", "vs_456"],
                "max_num_results": 10,
                "ranking_options": {"ranker": "auto"}
              }]
            }
            """);
        Assert.Empty(result.Warnings);
        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("include"));
    }

    [Fact]
    [UpstreamTest(Generate + "should forward include provider options to request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_the_include_provider_option()
    {
        var (handler, result) = await Respond(
            TextBody,
            null,
            AzureParity.Element("{\"include\":[\"file_search_call.results\"]}"));

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "test-deployment",
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "include": ["file_search_call.results"]
            }
            """);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > code interpreter tool::should send request body with include and tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Code_interpreter_adds_outputs_to_include()
    {
        var (handler, _) = await Respond(
            "{\"output\":[]}",
            new[] { new AzureProviderTool("openai.code_interpreter", "code_interpreter", AzureParity.Element("{}")) },
            null);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "test-deployment",
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "include": ["code_interpreter_call.outputs"],
              "tools": [{"type":"code_interpreter","container":{"type":"auto"}}]
            }
            """);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > code interpreter tool::should include code interpreter tool call and result in content", Coverage = UpstreamCoverage.Covered)]
    public async Task Code_interpreter_content_includes_the_call_and_result()
    {
        var (_, result) = await Respond(
            """
            {
              "output": [
                {"id":"rs_1","type":"reasoning","summary":[]},
                {"id":"ci_1","type":"code_interpreter_call","code":"import random","container_id":"cntr_1","outputs":[{"type":"logs","logs":"1: 74.07"}]},
                {"id":"msg_1","type":"message","content":[{"type":"output_text","text":"I ran the program"}]}
              ]
            }
            """,
            new[] { new AzureProviderTool("openai.code_interpreter", "code_interpreter", AzureParity.Element("{}")) },
            null);

        Assert.IsType<AzureGeneratedReasoning>(result.Content[0]);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.Equal("ci_1", call.ToolCallId);
        Assert.Equal("code_interpreter", call.ToolName);
        AzureParity.JsonEqual(call.ArgumentsJson, "{\"code\":\"import random\",\"containerId\":\"cntr_1\"}");
        var toolResult = Assert.IsType<AzureToolResult>(result.Content[2]);
        Assert.True(toolResult.ProviderExecuted);
        AzureParity.JsonEqual(toolResult.ResultJson, "{\"outputs\":[{\"type\":\"logs\",\"logs\":\"1: 74.07\"}]}");
        Assert.Equal("I ran the program", Assert.IsType<AzureGeneratedText>(result.Content[3]).Text);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > file search tool > without results include::should send request body with tool", Coverage = UpstreamCoverage.Covered)]
    public async Task File_search_without_include_sends_the_tool()
    {
        var (handler, _) = await Respond("{\"output\":[]}", new[] { FileSearchTool() }, null);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "test-deployment",
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "tools": [{
                "type": "file_search",
                "vector_store_ids": ["vs_68caad8bd5d88191ab766cf043d89a18"],
                "max_num_results": 5,
                "ranking_options": {"ranker": "auto", "score_threshold": 0.5},
                "filters": {"key": "author", "type": "eq", "value": "Jane Smith"}
              }]
            }
            """);
        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("include"));
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > file search tool > without results include::should include file search tool call and result in content", Coverage = UpstreamCoverage.Covered)]
    public async Task File_search_without_results_keeps_results_null()
    {
        var (_, result) = await Respond(
            """
            {
              "output": [
                {"id":"fs_1","type":"file_search_call","queries":["What is an embedding model?"],"results":null},
                {"id":"msg_1","type":"message","content":[{"type":"output_text","text":"According to the document, an embedding model converts data."}]}
              ]
            }
            """,
            new[] { FileSearchTool() },
            null);

        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("file_search", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
        AzureParity.JsonEqual(
            Assert.IsType<AzureToolResult>(result.Content[1]).ResultJson,
            "{\"queries\":[\"What is an embedding model?\"],\"results\":null}");
        Assert.StartsWith("According to the document", Assert.IsType<AzureGeneratedText>(result.Content[2]).Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > file search tool > with results include::should send request body with tool", Coverage = UpstreamCoverage.Covered)]
    public async Task File_search_with_include_sends_the_tool_and_include()
    {
        var (handler, _) = await Respond(
            "{\"output\":[]}",
            new[] { FileSearchTool() },
            AzureParity.Element("{\"include\":[\"file_search_call.results\"]}"));

        Assert.Equal("file_search_call.results", JsonNode.Parse(handler.Body)!["include"]![0]!.GetValue<string>());
        Assert.Equal("file_search", JsonNode.Parse(handler.Body)!["tools"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("Jane Smith", JsonNode.Parse(handler.Body)!["tools"]![0]!["filters"]!["value"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > file search tool > with results include::should include file search tool call and result in content", Coverage = UpstreamCoverage.Covered)]
    public async Task File_search_with_results_passes_the_result_array()
    {
        var (_, result) = await Respond(
            """
            {
              "output": [
                {
                  "id": "fs_2",
                  "type": "file_search_call",
                  "queries": ["What is an embedding model?"],
                  "results": [{"file_id":"file-Ebzhf8H4DPGPr9pUhr7n7v","filename":"ai.pdf","score":0.9311,"text":"embedding model"}]
                }
              ]
            }
            """,
            new[] { FileSearchTool() },
            AzureParity.Element("{\"include\":[\"file_search_call.results\"]}"));

        var toolResult = Assert.IsType<AzureToolResult>(result.Content[1]);
        var parsed = JsonNode.Parse(toolResult.ResultJson)!;
        Assert.Equal("ai.pdf", parsed["results"]![0]!["filename"]!.GetValue<string>());
        Assert.Equal(0.9311, parsed["results"]![0]!["score"]!.GetValue<double>());
        Assert.Equal("What is an embedding model?", parsed["queries"]![0]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > web search preview tool::should stream web search preview results include", Coverage = UpstreamCoverage.Covered)]
    public async Task Web_search_preview_content_includes_actions()
    {
        var (_, result) = await Respond(
            """
            {
              "output": [
                {"id":"ws_1","type":"web_search_call","action":{"type":"search","query":"top news stories November 19, 2025"}},
                {"id":"ws_2","type":"web_search_call","action":{"type":"search","query":"major news stories November 19, 2025"}},
                {"id":"msg_1","type":"message","content":[{"type":"output_text","text":"Here are three major news stories"}]}
              ]
            }
            """,
            new[] { new AzureProviderTool("openai.web_search_preview", "web_search_preview", AzureParity.Element("{}")) },
            null);

        Assert.Equal("web_search_preview", Assert.IsType<GeneratedToolCall>(result.Content[0]).ToolName);
        AzureParity.JsonEqual(
            Assert.IsType<AzureToolResult>(result.Content[1]).ResultJson,
            "{\"action\":{\"type\":\"search\",\"query\":\"top news stories November 19, 2025\"}}");
        Assert.Equal("ws_2", Assert.IsType<GeneratedToolCall>(result.Content[2]).ToolCallId);
        Assert.Equal("Here are three major news stories", Assert.IsType<AzureGeneratedText>(result.Content[4]).Text);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > reasoning::should generate with reasoning encrypted content", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_request_includes_encrypted_content_and_the_summary_is_kept()
    {
        var call = AzureParity.Hello();
        call.Tools = new[]
        {
            new LanguageModelTool(
                "calculator",
                null,
                AzureParity.Element("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"},\"b\":{\"type\":\"number\"},\"op\":{\"type\":\"string\"}},\"required\":[\"a\",\"b\"],\"additionalProperties\":false}")),
        };
        call.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["azure"] = AzureParity.Element("{\"reasoningEffort\":\"high\",\"maxCompletionTokens\":32000,\"store\":false,\"include\":[\"reasoning.encrypted_content\"],\"reasoningSummary\":\"auto\",\"forceReasoning\":true}"),
        };
        var handler = AzureParity.Handler(
            """
            {
              "id": "resp_reason",
              "output": [
                {
                  "id": "rs_enc",
                  "type": "reasoning",
                  "encrypted_content": "gAAAAA-encrypted",
                  "summary": [{"type":"summary_text","text":"Final result: 570"}]
                },
                {"id":"msg_1","type":"message","content":[{"type":"output_text","text":"(12 + 7) = 19"}]}
              ]
            }
            """);
        var result = await AzureParity.Provider(handler).Responses("test-deployment").GenerateAsync(call, null, CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal(32000, body["max_output_tokens"]!.GetValue<int>());
        Assert.False(body["store"]!.GetValue<bool>());
        Assert.Equal("reasoning.encrypted_content", body["include"]![0]!.GetValue<string>());
        Assert.Single(body["include"]!.AsArray());
        Assert.Equal("high", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Equal("auto", body["reasoning"]!["summary"]!.GetValue<string>());
        Assert.Equal("calculator", body["tools"]![0]!["name"]!.GetValue<string>());
        Assert.False(body["tools"]![0]!["strict"]!.GetValue<bool>());
        var reasoning = Assert.IsType<AzureGeneratedReasoning>(result.Content[0]);
        Assert.Equal("Final result: 570", reasoning.Text);
        Assert.Equal("gAAAAA-encrypted", reasoning.ProviderMetadata!.Value.GetProperty("azure").GetProperty("reasoningEncryptedContent").GetString());
        Assert.Equal("rs_enc", reasoning.ProviderMetadata.Value.GetProperty("azure").GetProperty("itemId").GetString());
        Assert.Equal("(12 + 7) = 19", Assert.IsType<AzureGeneratedText>(result.Content[1]).Text);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > image generation tool::should send request body with include and tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_sends_the_tool_without_an_automatic_include()
    {
        var (handler, _) = await Respond(
            "{\"output\":[]}",
            new[]
            {
                new AzureProviderTool(
                    "openai.image_generation",
                    "image_generation",
                    AzureParity.Element("{\"outputFormat\":\"webp\",\"quality\":\"low\",\"size\":\"1024x1024\",\"partialImages\":2}")),
            },
            null);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "test-deployment",
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "tools": [{
                "type": "image_generation",
                "output_format": "webp",
                "quality": "low",
                "size": "1024x1024",
                "partial_images": 2
              }]
            }
            """);
        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("include"));
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > image generation tool::should include generate image tool call and result in content", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_content_includes_the_result()
    {
        var (_, result) = await Respond(
            """
            {
              "output": [
                {"id":"ig_1","type":"image_generation_call","result":"iVBORw0KGgo"},
                {"id":"msg_1","type":"message","content":[{"type":"output_text","text":"Here is an anime-like image of a cute cat"}]}
              ]
            }
            """,
            new[] { new AzureProviderTool("openai.image_generation", "image_generation", AzureParity.Element("{}")) },
            null);

        Assert.Equal("image_generation", Assert.IsType<GeneratedToolCall>(result.Content[0]).ToolName);
        AzureParity.JsonEqual(Assert.IsType<AzureToolResult>(result.Content[1]).ResultJson, "{\"result\":\"iVBORw0KGgo\"}");
        Assert.StartsWith("Here is an anime-like image", Assert.IsType<AzureGeneratedText>(result.Content[2]).Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Stream + "should extract response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_exposes_response_headers()
    {
        var handler = AzureParity.Handler(TextBody, AzureParity.Sse("{\"type\":\"response.completed\",\"response\":{}}"));
        handler.ResponseHeaders["test-header"] = "test-value";
        handler.ResponseHeaders["cache-control"] = "no-cache";
        var opened = await AzureParity.Provider(handler).Responses("test-deployment").OpenStreamAsync(AzureParity.Hello(), null, CancellationToken.None);

        Assert.Equal("test-value", opened.Headers["test-header"]);
        Assert.Equal("no-cache", opened.Headers["cache-control"]);
        Assert.Contains("text/event-stream", opened.Headers["Content-Type"], StringComparison.Ordinal);
        await AzureParity.Collect(opened.Parts);
    }

    [Fact]
    [UpstreamTest(Stream + "should handle file_citation annotations without optional fields in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task File_citation_stream_emits_document_sources_and_a_null_response_id()
    {
        var handler = AzureParity.Handler(
            TextBody,
            AzureParity.Sse(
                """{"type":"response.content_part.added","item_id":"msg_456","part":{"type":"output_text","text":"","annotations":[]}}""",
                """{"type":"response.output_text.annotation.added","item_id":"msg_456","annotation":{"type":"file_citation","file_id":"assistant-YRcoCqn3Fo2K4JgraG","filename":"resource1.json","index":145}}""",
                """{"type":"response.output_text.annotation.added","item_id":"msg_456","annotation":{"type":"file_citation","file_id":"assistant-YRcoCqn3Fo2K4JgraG","filename":"resource1.json","index":192}}""",
                """{"type":"response.output_item.done","item":{"id":"msg_456","type":"message","content":[{"type":"output_text","text":"Answer for the specified years....","annotations":[{"type":"file_citation","file_id":"assistant-YRcoCqn3Fo2K4JgraG","filename":"resource1.json","index":145},{"type":"file_citation","file_id":"assistant-YRcoCqn3Fo2K4JgraG","filename":"resource1.json","index":192}]}]}}""",
                """{"type":"response.completed","response":{"id":"resp_456","usage":{"input_tokens":50,"input_tokens_details":{"cached_tokens":0},"output_tokens":25,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":75}}}"""));
        var opened = await AzureParity.Provider(handler).Responses("test-deployment").OpenStreamAsync(AzureParity.Hello(), null, CancellationToken.None);
        var parts = await AzureParity.Collect(opened.Parts);

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Empty(((StreamStartStreamPart)parts[0]).Warnings);
        Assert.DoesNotContain(parts, part => part is ResponseMetadataStreamPart);
        var first = Assert.IsType<AzureDocumentSourceStreamPart>(parts[1]);
        Assert.Equal("id-0", first.Id);
        Assert.Equal("resource1.json", first.Filename);
        Assert.Equal("resource1.json", first.Title);
        Assert.Equal("text/plain", first.MediaType);
        Assert.Equal("document", first.SourceType);
        Assert.Equal("assistant-YRcoCqn3Fo2K4JgraG", first.ProviderMetadata!.Value.GetProperty("azure").GetProperty("fileId").GetString());
        Assert.Equal(145, first.ProviderMetadata.Value.GetProperty("azure").GetProperty("index").GetInt32());
        var second = Assert.IsType<AzureDocumentSourceStreamPart>(parts[2]);
        Assert.Equal("id-1", second.Id);
        Assert.Equal(192, second.ProviderMetadata!.Value.GetProperty("azure").GetProperty("index").GetInt32());
        var end = Assert.IsType<AzureAnnotatedTextEndStreamPart>(parts[3]);
        Assert.Equal("msg_456", end.Id);
        Assert.Equal(2, end.ProviderMetadata!.Value.GetProperty("azure").GetProperty("annotations").GetArrayLength());
        var finish = Assert.IsType<FinishStreamPart>(parts[4]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Equal(JsonValueKind.Null, finish.ProviderMetadata!.Value.GetProperty("azure").GetProperty("responseId").ValueKind);
        Assert.Equal(50, finish.Usage.InputTokens);
        Assert.Equal(25, finish.Usage.OutputTokens);
        Assert.Equal(75, finish.Usage.TotalTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
        Assert.Equal(25, finish.Usage.TextTokens);
    }

    private static async Task<LanguageModelGenerateResult> GenerateText()
    {
        var handler = AzureParity.Handler(TextBody);
        return await AzureParity.Provider(handler).Responses("test-deployment").DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);
    }

    private static async Task AssertInput(IReadOnlyList<UserContentPart> content, string expectedInput)
    {
        var handler = AzureParity.Handler(TextBody);
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(content) },
        };
        await AzureParity.Provider(handler).Responses("test-deployment").DoGenerateAsync(call, CancellationToken.None);
        ParityAssert.JsonEqual(JsonNode.Parse(handler.Body)!["input"], expectedInput);
    }

    private static async Task<(CaptureHandler Handler, LanguageModelGenerateResult Result)> Respond(
        string responseBody,
        IReadOnlyList<AzureProviderTool>? tools,
        JsonElement? azure)
    {
        var handler = AzureParity.Handler(responseBody);
        var call = AzureParity.Hello();
        if (azure is { } options)
        {
            call.ProviderOptions = new Dictionary<string, JsonElement> { ["azure"] = options };
        }

        var result = await AzureParity.Provider(handler).Responses("test-deployment").GenerateAsync(call, tools, CancellationToken.None);
        return (handler, result);
    }

    private static AzureProviderTool FileSearchTool()
    {
        return new AzureProviderTool(
            "openai.file_search",
            "file_search",
            AzureParity.Element(
                """
                {
                  "vectorStoreIds": ["vs_68caad8bd5d88191ab766cf043d89a18"],
                  "maxNumResults": 5,
                  "filters": {"key":"author","type":"eq","value":"Jane Smith"},
                  "ranking": {"ranker":"auto","scoreThreshold":0.5}
                }
                """));
    }
}
