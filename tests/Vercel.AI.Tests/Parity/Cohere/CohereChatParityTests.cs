// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Cohere chat generate and stream mapping.</summary>
public sealed class CohereChatParityTests
{
    private const string Generate = "packages/cohere/src/cohere-chat-language-model.test.ts::doGenerate > ";

    private const string Stream = "packages/cohere/src/cohere-chat-language-model.test.ts::doStream > ";

    private const string TextEvents = """
        data: {"id":"321d178c-2c12-44d3-ae42-2f5510f6b1cc","type":"message-start","delta":{"message":{"role":"assistant"}}}

        data: {"type":"content-start","index":0,"delta":{"message":{"content":{"type":"text","text":""}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":"The"}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":" capital"}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":" of"}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":" France"}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":" is"}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":" Paris"}}}}

        data: {"type":"content-delta","index":0,"delta":{"message":{"content":{"text":"."}}}}

        data: {"type":"content-end","index":0}

        data: {"type":"message-end","delta":{"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":12,"output_tokens":7},"tokens":{"input_tokens":507,"output_tokens":10},"cached_tokens":448}}}

        """;

    [Fact]
    [UpstreamTest(Generate + "text::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text()
    {
        var result = await GenerateAsync(CohereParity.TextResponse, CohereParity.Prompt());

        Assert.Equal("The capital of France is Paris.", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("COMPLETE", result.RawFinishReason);
        Assert.Null(result.ResponseId);
        Assert.Equal(507, result.Usage.InputTokens);
        Assert.Equal(10, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Generate + "max tokens::should map MAX_TOKENS finish reason to length", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_max_tokens_to_length()
    {
        var result = await GenerateAsync(
            """
            {"message":{"content":[{"type":"text","text":"**The History of"}]},"finish_reason":"MAX_TOKENS","usage":{"billed_units":{"input_tokens":11,"output_tokens":4},"tokens":{"input_tokens":506,"output_tokens":5},"cached_tokens":448}}
            """,
            CohereParity.Prompt());

        Assert.Equal(FinishReason.Length, result.FinishReason);
        Assert.Equal("MAX_TOKENS", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Generate + "tool call::should extract tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_tool_calls()
    {
        var result = await GenerateAsync(
            """
            {"message":{"tool_calls":[{"id":"weather_dqgshstja6p9","type":"function","function":{"name":"weather","arguments":"{\"location\":\"San Francisco\"}"}},{"id":"cityAttractions_dcxfx4myvx68","type":"function","function":{"name":"cityAttractions","arguments":"{\"city\":\"San Francisco\"}"}}]},"finish_reason":"TOOL_CALL","usage":{"tokens":{"input_tokens":1549,"output_tokens":103},"cached_tokens":992}}
            """,
            CohereParity.Prompt());

        var weather = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        var attractions = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.Equal("weather_dqgshstja6p9", weather.ToolCallId);
        Assert.Equal("weather", weather.ToolName);
        Assert.Equal("{\"location\":\"San Francisco\"}", weather.ArgumentsJson);
        Assert.Equal("cityAttractions", attractions.ToolName);
        Assert.Equal("{\"city\":\"San Francisco\"}", attractions.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("TOOL_CALL", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Generate + "null tool call arguments::should handle string \"null\" tool call arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Replaces_null_tool_arguments_with_an_object()
    {
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("What is the current time?") },
        };
        var result = await GenerateAsync(
            """
            {"message":{"tool_calls":[{"id":"currentTime_tf4dywn8wgnk","type":"function","function":{"name":"currentTime","arguments":"null"}}]},"finish_reason":"TOOL_CALL","usage":{"tokens":{"input_tokens":1445,"output_tokens":43}}}
            """,
            call);

        var tool = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("currentTime_tf4dywn8wgnk", tool.ToolCallId);
        Assert.Equal("currentTime", tool.ToolName);
        Assert.Equal("{}", tool.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(Generate + "reasoning::should extract reasoning from response", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_reasoning_before_text()
    {
        var result = await GenerateAsync(
            """
            {"message":{"content":[{"type":"thinking","thinking":"Okay, so I need to figure out what 2 + 2 is."},{"type":"text","text":"2 + 2 = 4"}]},"finish_reason":"COMPLETE","usage":{"tokens":{"input_tokens":1394,"output_tokens":582}}}
            """,
            CohereParity.Prompt());

        Assert.Equal("Okay, so I need to figure out what 2 + 2 is.", Assert.IsType<GeneratedReasoning>(result.Content[0]).Text);
        Assert.Equal("2 + 2 = 4", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(Generate + "top-level reasoning::should map top-level reasoning to thinking enabled with budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_high_reasoning_to_a_token_budget()
    {
        var call = CohereParity.Prompt();
        call.Reasoning = "high";
        var handler = await Send(call);

        var thinking = JsonNode.Parse(handler.Body)!["thinking"]!;
        Assert.Equal("enabled", thinking["type"]!.GetValue<string>());
        Assert.Equal(19661, thinking["token_budget"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(Generate + "top-level reasoning::should map top-level reasoning none to thinking disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_reasoning_none_to_disabled_thinking()
    {
        var call = CohereParity.Prompt();
        call.Reasoning = "none";
        var handler = await Send(call);

        CohereParity.JsonEqual(JsonNode.Parse(handler.Body)!["thinking"]!.ToJsonString(), "{\"type\":\"disabled\"}");
    }

    [Fact]
    [UpstreamTest(Generate + "top-level reasoning::should prefer providerOptions over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_thinking_wins_over_top_level_reasoning()
    {
        var call = CohereParity.Prompt();
        call.Reasoning = "none";
        call.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
        {
            ["cohere"] = CohereParity.Element("{\"thinking\":{\"type\":\"enabled\"}}"),
        };
        var handler = await Send(call);

        var thinking = JsonNode.Parse(handler.Body)!["thinking"]!;
        Assert.Equal("enabled", thinking["type"]!.GetValue<string>());
        Assert.False(thinking.AsObject().ContainsKey("token_budget"));
    }

    [Fact]
    [UpstreamTest(Generate + "top-level reasoning::should not set thinking when reasoning is not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_thinking_when_reasoning_is_absent()
    {
        var handler = await Send(CohereParity.Prompt());

        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("thinking"));
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should extract citations from response", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_citations()
    {
        var handler = CohereParity.Handler(
            """
            {"message":{"content":[{"type":"text","text":"The key benefits mentioned in this document are:\n1. Automation of tasks\n2. Better decision-making\n3. Cost reduction"}],"citations":[{"start":52,"end":71,"text":"Automation of tasks","type":"TEXT_CONTENT","sources":[{"type":"document","id":"doc:0","document":{"id":"doc:0","title":"benefits.txt","text":"AI provides"}}]},{"start":75,"end":97,"text":"Better decision-making","type":"TEXT_CONTENT","sources":[{"document":{"title":"benefits.txt"}}]},{"start":101,"end":115,"text":"Cost reduction","type":"TEXT_CONTENT","sources":[{"document":{"title":"benefits.txt"}}]}]},"finish_reason":"COMPLETE","usage":{"tokens":{"input_tokens":1683,"output_tokens":62},"cached_tokens":992}}
            """);
        var provider = CohereParity.Provider(handler, options => options.GenerateId = () => "test-citation-id");
        var file = new CohereFilePart("text/plain")
        {
            Text = "AI provides automation and efficiency.",
            FileName = "ai-benefits.txt",
        };
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new TextContentPart("What are AI benefits?"), file }),
            },
        };
        var result = await ((CohereLanguageModel)provider.LanguageModel("command-r-plus")).DoGenerateAsync(call, CancellationToken.None);

        Assert.StartsWith("The key benefits", result.Text, StringComparison.Ordinal);
        var citations = new List<CohereCitation>();
        foreach (var part in result.Content)
        {
            if (part is CohereCitation citation)
            {
                citations.Add(citation);
            }
        }

        Assert.Equal(3, citations.Count);
        Assert.All(citations, citation => Assert.Equal("test-citation-id", citation.Id));
        Assert.All(citations, citation => Assert.Equal("benefits.txt", citation.Title));
        Assert.Equal("source", citations[0].Type);
        var cohere = citations[0].ProviderMetadata!.Value.GetProperty("cohere");
        Assert.Equal(52, cohere.GetProperty("start").GetInt32());
        Assert.Equal(71, cohere.GetProperty("end").GetInt32());
        Assert.Equal("Automation of tasks", cohere.GetProperty("text").GetString());
        Assert.Equal("TEXT_CONTENT", cohere.GetProperty("citationType").GetString());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("ai-benefits.txt", JsonNode.Parse(handler.Body)!["documents"]![0]!["data"]!["title"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should extract text documents and send to API", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_string_document()
    {
        var handler = await Send(UserWithFile("What does this say?", "This is a test document.", "test.txt", "text/plain", bytes: false));

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "command-r-plus",
              "messages": [{"role":"user","content":"What does this say?"}],
              "documents": [{"data":{"text":"This is a test document.","title":"test.txt"}}]
            }
            """);
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should extract multiple text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_multiple_utf8_documents()
    {
        var handler = await Send(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What do these documents say?"),
                    BytesFile("doc1.txt", "text/plain", "First document content"),
                    BytesFile("doc2.txt", "text/plain", "Second document content"),
                }),
            },
        });

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "command-r-plus",
              "messages": [{"role":"user","content":"What do these documents say?"}],
              "documents": [
                {"data":{"text":"First document content","title":"doc1.txt"}},
                {"data":{"text":"Second document content","title":"doc2.txt"}}
              ]
            }
            """);
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should support JSON files", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_file_text()
    {
        var handler = await Send(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What is in this JSON?"),
                    BytesFile("data.json", "application/json", "{\"key\": \"value\"}"),
                }),
            },
        });

        Assert.Equal("{\"key\": \"value\"}", JsonNode.Parse(handler.Body)!["documents"]![0]!["data"]!["text"]!.GetValue<string>());
        Assert.Equal("data.json", JsonNode.Parse(handler.Body)!["documents"]![0]!["data"]!["title"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should not include mediaType in the outgoing payload (category D)", Coverage = UpstreamCoverage.Covered)]
    public async Task Document_payload_omits_media_type()
    {
        var handler = await Send(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What is this?"),
                    BytesFile("document.pdf", "application/pdf", "Some file content"),
                }),
            },
        });

        CohereParity.JsonEqual(
            JsonNode.Parse(handler.Body)!["documents"]!.ToJsonString(),
            """[{"data":{"text":"Some file content","title":"document.pdf"}}]""");
        Assert.DoesNotContain("application/pdf", handler.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("mediaType", handler.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should successfully process supported text media types", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_plain_text_and_markdown_documents()
    {
        var handler = await Send(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What is this?"),
                    BytesFile("text.txt", "text/plain", "This is plain text content"),
                    BytesFile("doc.md", "text/markdown", "# Markdown Header\nContent"),
                }),
            },
        });

        Assert.Equal("This is plain text content", JsonNode.Parse(handler.Body)!["documents"]![0]!["data"]!["text"]!.GetValue<string>());
        Assert.Equal("# Markdown Header\nContent", JsonNode.Parse(handler.Body)!["documents"]![1]!["data"]!["text"]!.GetValue<string>());
        Assert.Equal("doc.md", JsonNode.Parse(handler.Body)!["documents"]![1]!["data"]!["title"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "citations::should not include documents parameter when no files present", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_documents_when_the_prompt_has_no_files()
    {
        var handler = await Send(CohereParity.Prompt());

        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("documents"));
    }

    [Fact]
    [UpstreamTest(Generate + "request::should pass model and messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_model_and_messages()
    {
        var handler = await Send(CohereParity.Prompt());

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "command-r-plus",
              "messages": [
                {"role":"system","content":"you are a friendly bot!"},
                {"role":"user","content":"Hello"}
              ]
            }
            """);
        Assert.Contains("https://api.cohere.com/v2/chat", handler.Uri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Generate + "request::should pass tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_tools_and_none_tool_choice()
    {
        var call = CohereParity.Prompt();
        call.ToolChoice = ToolChoice.None;
        call.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                CohereParity.Element("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}")),
        };
        var handler = await Send(call);

        Assert.Equal("NONE", JsonNode.Parse(handler.Body)!["tool_choice"]!.GetValue<string>());
        var function = JsonNode.Parse(handler.Body)!["tools"]![0]!["function"]!;
        Assert.Equal("test-tool", function["name"]!.GetValue<string>());
        Assert.False(function.AsObject().ContainsKey("description"));
        Assert.Equal("http://json-schema.org/draft-07/schema#", function["parameters"]!["$schema"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "request::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_generate_headers()
    {
        var handler = await Send(CohereParity.Prompt(), withHeaders: true);

        AssertCohereHeaders(handler);
    }

    [Fact]
    [UpstreamTest(Generate + "request::should pass response format", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_json_response_format()
    {
        var call = CohereParity.Prompt();
        call.JsonSchema = CohereParity.Element("{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"]}");
        var handler = await Send(call);

        CohereParity.JsonEqual(
            JsonNode.Parse(handler.Body)!["response_format"]!.ToJsonString(),
            """
            {"type":"json_object","json_schema":{"type":"object","properties":{"text":{"type":"string"}},"required":["text"]}}
            """);
    }

    [Fact]
    [UpstreamTest(Generate + "request::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_generate_response_headers()
    {
        var handler = CohereParity.Handler(CohereParity.TextResponse);
        handler.ResponseHeaders["test-header"] = "test-value";
        var result = await ((CohereLanguageModel)CohereParity.Provider(handler).LanguageModel("command-r-plus")).DoGenerateAsync(CohereParity.Prompt(), CancellationToken.None);

        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Contains("application/json", result.ResponseHeaders["Content-Type"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Generate + "request::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_token_usage_not_billed_units()
    {
        var usage = (await GenerateAsync(CohereParity.TextResponse, CohereParity.Prompt())).Usage;

        Assert.Equal(507, usage.InputTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Equal(517, usage.TotalTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal(12, usage.Raw!.Value.GetProperty("billed_units").GetProperty("input_tokens").GetInt32());
        Assert.Equal(7, usage.Raw.Value.GetProperty("billed_units").GetProperty("output_tokens").GetInt32());
        Assert.Equal(448, usage.Raw.Value.GetProperty("cached_tokens").GetInt32());
        Assert.Equal(507, usage.Raw.Value.GetProperty("tokens").GetProperty("input_tokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(Generate + "request::should preserve extra top-level and nested fields in raw usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_extra_usage_fields()
    {
        var usage = (await GenerateAsync(
            """
            {"message":{"content":[{"type":"text","text":"Hello"}]},"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":12,"output_tokens":7,"billing_tier":"standard"},"tokens":{"input_tokens":507,"output_tokens":10,"tokenizer":"command"},"cached_tokens":448,"provider_usage_id":"usage-123"}}
            """,
            CohereParity.Prompt())).Usage;

        Assert.Equal("standard", usage.Raw!.Value.GetProperty("billed_units").GetProperty("billing_tier").GetString());
        Assert.Equal("command", usage.Raw.Value.GetProperty("tokens").GetProperty("tokenizer").GetString());
        Assert.Equal("usage-123", usage.Raw.Value.GetProperty("provider_usage_id").GetString());
        Assert.Equal(507, usage.InputTokens);
        Assert.Equal(10, usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Generate + "request::should validate cached token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_string_cached_tokens()
    {
        var handler = CohereParity.Handler(
            """
            {"message":{"content":[{"type":"text","text":"Hello"}]},"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":12,"output_tokens":7},"tokens":{"input_tokens":507,"output_tokens":10},"cached_tokens":"448"}}
            """);
        var model = (CohereLanguageModel)CohereParity.Provider(handler).LanguageModel("command-r-plus");

        await Assert.ThrowsAsync<CohereUsageException>(() => model.DoGenerateAsync(CohereParity.Prompt(), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Generate + "request::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_treat_the_top_level_id_as_a_generation_id()
    {
        var result = await GenerateAsync(CohereParity.TextResponse, CohereParity.Prompt());

        Assert.Null(result.ResponseId);
        Assert.Null(result.ResponseModelId);
        Assert.Null(result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest(Stream + "text::should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text_deltas()
    {
        var parts = await StreamAsync(TextEvents, false);

        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        Assert.Equal("321d178c-2c12-44d3-ae42-2f5510f6b1cc", Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Id);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        var text = new StringBuilder();
        foreach (var part in parts)
        {
            if (part is TextDeltaStreamPart delta)
            {
                Assert.Equal("0", delta.Id);
                text.Append(delta.Delta);
            }
        }

        Assert.Equal("The capital of France is Paris.", text.ToString());
        Assert.Contains(parts, part => part is TextEndStreamPart end && end.Id == "0");
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Equal(507, finish.Usage.InputTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Stream + "text::should include raw chunks when includeRawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_raw_chunks_when_requested()
    {
        var parts = await StreamAsync(TextEvents, true);
        var raw = new List<RawStreamPart>();
        foreach (var part in parts)
        {
            if (part is RawStreamPart item)
            {
                raw.Add(item);
            }
        }

        Assert.Equal(11, raw.Count);
        Assert.Contains("321d178c-2c12-44d3-ae42-2f5510f6b1cc", raw[0].RawJson, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Stream + "text::should not include raw chunks when includeRawChunks is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_raw_chunks_by_default()
    {
        var parts = await StreamAsync(TextEvents, false);

        Assert.DoesNotContain(parts, part => part is RawStreamPart);
    }

    [Fact]
    [UpstreamTest(Stream + "text::should preserve extra top-level and nested fields in raw usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_preserves_extra_usage_fields()
    {
        var parts = await StreamAsync(
            CohereParity.Sse(
                """{"type":"message-end","delta":{"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":12,"output_tokens":7,"billing_tier":"standard"},"tokens":{"input_tokens":507,"output_tokens":10,"tokenizer":"command"},"cached_tokens":448,"provider_usage_id":"usage-123"}}}"""),
            false);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);

        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Equal("standard", finish.Usage.Raw!.Value.GetProperty("billed_units").GetProperty("billing_tier").GetString());
        Assert.Equal("command", finish.Usage.Raw.Value.GetProperty("tokens").GetProperty("tokenizer").GetString());
        Assert.Equal("usage-123", finish.Usage.Raw.Value.GetProperty("provider_usage_id").GetString());
    }

    [Fact]
    [UpstreamTest(Stream + "text::should finish successfully when billed units are absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_finishes_without_billed_units()
    {
        var parts = await StreamAsync(
            CohereParity.Sse("""{"type":"message-end","delta":{"finish_reason":"COMPLETE","usage":{"tokens":{"input_tokens":507,"output_tokens":10}}}}"""),
            false);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);

        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(507, finish.Usage.InputTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
        Assert.False(finish.Usage.Raw!.Value.TryGetProperty("billed_units", out _));
    }

    [Fact]
    [UpstreamTest(Stream + "text::should validate streamed $field", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_rejects_string_usage_fields()
    {
        await AssertUsageError("""{"type":"message-end","delta":{"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":"12","output_tokens":7},"tokens":{"input_tokens":507,"output_tokens":10},"cached_tokens":448}}}""");
        await AssertUsageError("""{"type":"message-end","delta":{"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":12,"output_tokens":7},"tokens":{"input_tokens":507,"output_tokens":10},"cached_tokens":"448"}}}""");
    }

    [Fact]
    [UpstreamTest(Stream + "reasoning::should stream reasoning deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_reasoning_then_text()
    {
        var parts = await StreamAsync(
            CohereParity.Sse(
                """{"id":"c9117d7f-a7e4-499f-b643-a2a1e139687b","type":"message-start","delta":{"message":{"role":"assistant"}}}""",
                """{"type":"content-start","index":0,"delta":{"message":{"content":{"type":"thinking","thinking":""}}}}""",
                """{"type":"content-delta","index":0,"delta":{"message":{"content":{"thinking":"The user is asking"}}}}""",
                """{"type":"content-end","index":0}""",
                """{"type":"content-start","index":1,"delta":{"message":{"content":{"type":"text","text":""}}}}""",
                """{"type":"content-delta","index":1,"delta":{"message":{"content":{"text":"The answer to 2 + 2 is 4."}}}}""",
                """{"type":"content-end","index":1}""",
                """{"type":"message-end","delta":{"finish_reason":"COMPLETE","usage":{"tokens":{"input_tokens":1394,"output_tokens":54}}}}"""),
            false);

        Assert.Equal("0", Assert.IsType<ReasoningStartStreamPart>(parts[2]).Id);
        Assert.Equal("The user is asking", Assert.IsType<ReasoningDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("0", Assert.IsType<ReasoningEndStreamPart>(parts[4]).Id);
        Assert.Equal("1", Assert.IsType<TextStartStreamPart>(parts[5]).Id);
        Assert.Equal("The answer to 2 + 2 is 4.", Assert.IsType<TextDeltaStreamPart>(parts[6]).Delta);
        Assert.Equal("1", Assert.IsType<TextEndStreamPart>(parts[7]).Id);
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[8]).FinishReason);
    }

    [Fact]
    [UpstreamTest(Stream + "tool call::should stream tool deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_tool_input_and_a_reserialized_call()
    {
        var parts = await StreamAsync(
            CohereParity.Sse(
                """{"id":"2941521a-b87a-45f6-9b0d-235fd66c3025","type":"message-start","delta":{"message":{"role":"assistant"}}}""",
                """{"type":"tool-call-start","index":0,"delta":{"message":{"tool_calls":{"id":"weather_e8p4pn45zt0t","type":"function","function":{"name":"weather","arguments":""}}}}}""",
                """{"type":"tool-call-delta","index":0,"delta":{"message":{"tool_calls":{"function":{"arguments":"{\"location\": \"San Francisco\"}"}}}}}""",
                """{"type":"tool-call-end","index":0}""",
                """{"type":"message-end","delta":{"finish_reason":"TOOL_CALL","usage":{"tokens":{"input_tokens":1549,"output_tokens":95}}}}"""),
            false);

        var start = Assert.IsType<CohereToolInputStartStreamPart>(parts[2]);
        Assert.Equal("weather_e8p4pn45zt0t", start.Id);
        Assert.Equal("weather", start.ToolName);
        Assert.Equal("{\"location\": \"San Francisco\"}", Assert.IsType<CohereToolInputDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("weather_e8p4pn45zt0t", Assert.IsType<CohereToolInputEndStreamPart>(parts[4]).Id);
        var call = Assert.IsType<ToolCallStreamPart>(parts[5]);
        Assert.Equal("weather_e8p4pn45zt0t", call.ToolCallId);
        Assert.Equal("{\"location\":\"San Francisco\"}", call.ArgumentsJson);
        var finish = Assert.IsType<FinishStreamPart>(parts[6]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("TOOL_CALL", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Stream + "tool call::rejects prototype keys in streamed tool call arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_prototype_keys_in_streamed_tool_arguments()
    {
        var handler = CohereParity.Handler(
            "{}",
            CohereParity.Sse(
                """{"type":"tool-call-start","index":0,"delta":{"message":{"tool_calls":{"id":"test-tool-call","type":"function","function":{"name":"test-tool","arguments":""}}}}}""",
                """{"type":"tool-call-delta","index":0,"delta":{"message":{"tool_calls":{"function":{"arguments":"{\"__proto__\":{\"polluted\":true}}"}}}}}""",
                """{"type":"tool-call-end","index":0}"""));
        var opened = await Model(handler).OpenStreamAsync(CohereParity.Prompt(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<FormatException>(async () => await CohereParity.Collect(opened.Parts));
        Assert.Equal("Object contains forbidden prototype property", exception.Message);
    }

    [Fact]
    [UpstreamTest(Stream + "empty tool call::should handle empty tool call arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_tool_arguments_become_an_object()
    {
        var parts = await StreamAsync(
            CohereParity.Sse(
                """{"id":"66dec7d7-45e6-427c-8fd9-7d6375d12046","type":"message-start","delta":{"message":{"role":"assistant"}}}""",
                """{"type":"tool-call-start","index":0,"delta":{"message":{"tool_calls":{"id":"currentTime_y46ar19t5gvw","type":"function","function":{"name":"currentTime","arguments":""}}}}}""",
                """{"type":"tool-call-end","index":0}""",
                """{"type":"message-end","delta":{"finish_reason":"TOOL_CALL","usage":{"tokens":{"input_tokens":1445,"output_tokens":43}}}}"""),
            false);

        var call = Assert.IsType<ToolCallStreamPart>(parts[4]);
        Assert.Equal("currentTime_y46ar19t5gvw", call.ToolCallId);
        Assert.Equal("currentTime", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(Stream + "error handling::should handle unparsable stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Unparsable_events_become_an_error_finish()
    {
        var parts = await StreamAsync("data: {unparsable}\n\n", false);

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.False(string.IsNullOrEmpty(Assert.IsType<ErrorStreamPart>(parts[1]).Message));
        var finish = Assert.IsType<FinishStreamPart>(parts[2]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Equal(0, finish.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Stream + "request::should pass the messages and the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_request_includes_messages_and_stream()
    {
        var handler = CohereParity.Handler("{}", TextEvents);
        await CohereParity.Collect((await Model(handler).OpenStreamAsync(CohereParity.Prompt(), CancellationToken.None)).Parts);

        CohereParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "command-r-plus",
              "messages": [
                {"role":"system","content":"you are a friendly bot!"},
                {"role":"user","content":"Hello"}
              ],
              "stream": true
            }
            """);
    }

    [Fact]
    [UpstreamTest(Stream + "request::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_passes_headers()
    {
        var handler = CohereParity.Handler("{}", TextEvents);
        var provider = CohereParity.Provider(handler, options =>
        {
            options.Headers = new Dictionary<string, string?>
            {
                ["Custom-Provider-Header"] = "provider-header-value",
            };
        });
        var call = CohereParity.Prompt();
        call.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await CohereParity.Collect((await ((CohereLanguageModel)provider.LanguageModel("command-r-plus")).OpenStreamAsync(call, CancellationToken.None)).Parts);

        AssertCohereHeaders(handler);
    }

    [Fact]
    [UpstreamTest(Stream + "request::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_exposes_response_headers()
    {
        var handler = CohereParity.Handler("{}", TextEvents);
        handler.ResponseHeaders["test-header"] = "test-value";
        handler.ResponseHeaders["cache-control"] = "no-cache";
        var opened = await Model(handler).OpenStreamAsync(CohereParity.Prompt(), CancellationToken.None);

        Assert.Equal("test-value", opened.Headers["test-header"]);
        Assert.Equal("no-cache", opened.Headers["cache-control"]);
        Assert.Contains("text/event-stream", opened.Headers["Content-Type"], StringComparison.Ordinal);
        await CohereParity.Collect(opened.Parts);
    }

    private static async Task AssertUsageError(string json)
    {
        var parts = await StreamAsync(CohereParity.Sse(json), false);

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.False(string.IsNullOrEmpty(Assert.IsType<ErrorStreamPart>(parts[1]).Message));
        var finish = Assert.IsType<FinishStreamPart>(parts[2]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
    }

    private static async Task<List<LanguageModelStreamPart>> StreamAsync(string events, bool raw)
    {
        var handler = CohereParity.Handler("{}", events);
        var call = CohereParity.Prompt();
        call.IncludeRawChunks = raw;
        return await CohereParity.Collect((await Model(handler).OpenStreamAsync(call, CancellationToken.None)).Parts);
    }

    private static async Task<LanguageModelGenerateResult> GenerateAsync(string body, LanguageModelCallOptions call)
    {
        var handler = CohereParity.Handler(body);
        return await Model(handler).DoGenerateAsync(call, CancellationToken.None);
    }

    private static async Task<CaptureHandler> Send(LanguageModelCallOptions call, bool withHeaders = false)
    {
        var handler = CohereParity.Handler(CohereParity.TextResponse);
        CohereProvider provider;
        if (withHeaders)
        {
            provider = CohereParity.Provider(handler, options =>
            {
                options.Headers = new Dictionary<string, string?>
                {
                    ["Custom-Provider-Header"] = "provider-header-value",
                };
            });
            call.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        }
        else
        {
            provider = CohereParity.Provider(handler);
        }

        await ((CohereLanguageModel)provider.LanguageModel("command-r-plus")).DoGenerateAsync(call, CancellationToken.None);
        return handler;
    }

    private static CohereLanguageModel Model(CaptureHandler handler)
    {
        return (CohereLanguageModel)CohereParity.Provider(handler).LanguageModel("command-r-plus");
    }

    private static LanguageModelCallOptions UserWithFile(string text, string data, string name, string mediaType, bool bytes)
    {
        UserContentPart file = bytes
            ? BytesFile(name, mediaType, data)
            : new CohereFilePart(mediaType) { Text = data, FileName = name };
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart(text), file }) },
        };
    }

    private static CohereFilePart BytesFile(string name, string mediaType, string text)
    {
        return new CohereFilePart(mediaType)
        {
            Bytes = Encoding.UTF8.GetBytes(text),
            FileName = name,
        };
    }

    private static void AssertCohereHeaders(CaptureHandler handler)
    {
        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/cohere/0.0.0-test", handler.RequestHeaders["User-Agent"], StringComparison.Ordinal);
    }
}
