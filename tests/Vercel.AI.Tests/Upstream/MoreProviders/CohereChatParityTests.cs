// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>Cohere v2 chat requests, responses, and streams matched to the upstream catalog.</summary>
public sealed class CohereChatParityTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";
    private const string Prompt = "packages/cohere/src/convert-to-cohere-chat-prompt.test.ts::convert to cohere chat prompt > ";
    private const string Tools = "packages/cohere/src/cohere-prepare-tools.test.ts::";
    private const string UsageNote = "NoCacheInputTokens and TextTokens stay null because Cohere reports no cache-write or reasoning counts. Upstream reports them equal to the totals.";
    private const string Messages = "[{\"role\":\"system\",\"content\":\"you are a friendly bot!\"},{\"role\":\"user\",\"content\":\"Hello\"}]";
    private const string TestToolSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";
    private const string TextUsage = "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":448}";
    private const string BasicTools = "[{\"type\":\"function\",\"function\":{\"name\":\"testFunction\",\"description\":\"test description\",\"parameters\":{\"type\":\"object\",\"properties\":{}}}}]";
    private const string ExtraUsage = "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7,\"billing_tier\":\"standard\"},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10,\"tokenizer\":\"command\"},\"cached_tokens\":448,\"provider_usage_id\":\"usage-123\"}";

    [Fact]
    [UpstreamTest(Chat + "doGenerate > text::should extract text response", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Generate_extracts_text()
    {
        var result = await Generate(JsonFixture("cohere-text"), TestPrompt()).ConfigureAwait(false);
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("The capital of France is Paris.", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("COMPLETE", result.RawFinishReason);
        AssertUsage(result.Usage, 507, 10, TextUsage);
        Assert.Null(result.ResponseId);
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(JsonNode.Parse(result.RawResponse!), File.ReadAllText(Fixture("cohere-text.json")));
        Assert.Equal("application/json", result.ResponseHeaders["Content-Type"]);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > max tokens::should map MAX_TOKENS finish reason to length", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_maps_max_tokens_to_length()
    {
        var result = await Generate(JsonFixture("cohere-max-tokens"), TestPrompt()).ConfigureAwait(false);
        Assert.Equal(FinishReason.Length, result.FinishReason);
        Assert.Equal("MAX_TOKENS", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > tool call::should extract tool calls", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Generate_extracts_tool_calls()
    {
        var options = TestPrompt();
        options.Tools = new[] { Tool("test-tool", TestToolSchema) };
        var result = await Generate(JsonFixture("cohere-tool-call"), options).ConfigureAwait(false);
        Assert.Collection(
            result.Content,
            part => AssertToolCall(part, "weather_dqgshstja6p9", "weather", "{\"location\":\"San Francisco\"}"),
            part => AssertToolCall(part, "cityAttractions_dcxfx4myvx68", "cityAttractions", "{\"city\":\"San Francisco\"}"));
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("TOOL_CALL", result.RawFinishReason);
        AssertUsage(result.Usage, 1549, 103, "{\"billed_units\":{\"input_tokens\":119,\"output_tokens\":52},\"tokens\":{\"input_tokens\":1549,\"output_tokens\":103},\"cached_tokens\":992}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > null tool call arguments::should handle string \"null\" tool call arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_turns_null_tool_arguments_into_an_empty_object()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("What is the current time?") },
            Tools = new[] { Tool("currentTime", "{\"type\":\"object\",\"properties\":{},\"required\":[],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}") },
        };
        var result = await Generate(JsonFixture("cohere-null-args"), options).ConfigureAwait(false);
        AssertToolCall(Assert.Single(result.Content), "currentTime_tf4dywn8wgnk", "currentTime", "{}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > reasoning::should extract reasoning from response", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Generate_extracts_reasoning()
    {
        var result = await Generate(JsonFixture("cohere-reasoning"), TestPrompt()).ConfigureAwait(false);
        Assert.Collection(
            result.Content,
            part => Assert.Equal("Okay, so I need to figure out what 2 + 2 is. Let me start by recalling what addition means.", Assert.IsType<GeneratedReasoning>(part).Text),
            part => Assert.Equal("2 + 2 = 4", Assert.IsType<GeneratedText>(part).Text));
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        AssertUsage(result.Usage, 1394, 582, "{\"billed_units\":{\"input_tokens\":8,\"output_tokens\":578},\"tokens\":{\"input_tokens\":1394,\"output_tokens\":582},\"cached_tokens\":1344}");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > top-level reasoning::should map top-level reasoning to thinking enabled with budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Top_level_reasoning_enables_thinking_with_a_budget()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Reasoning = "high";
        await Generate(handler, options).ConfigureAwait(false);
        var thinking = Body(handler)["thinking"]!;
        Assert.Equal("enabled", thinking["type"]!.GetValue<string>());
        Assert.True(thinking["token_budget"]!.GetValue<int>() > 0);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > top-level reasoning::should map top-level reasoning none to thinking disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Top_level_reasoning_none_disables_thinking()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Reasoning = "none";
        await Generate(handler, options).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["thinking"], "{\"type\":\"disabled\"}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > top-level reasoning::should prefer providerOptions over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_thinking_wins_over_top_level_reasoning()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Reasoning = "none";
        options.ProviderOptions = new Dictionary<string, JsonElement> { ["cohere"] = JsonSerializer.Deserialize<JsonElement>("{\"thinking\":{\"type\":\"enabled\"}}") };
        await Generate(handler, options).ConfigureAwait(false);
        Assert.Equal("enabled", Body(handler)["thinking"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > top-level reasoning::should not set thinking when reasoning is not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Thinking_is_omitted_without_reasoning()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, TestPrompt()).ConfigureAwait(false);
        Assert.False(Body(handler).ContainsKey("thinking"));
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > citations::should extract text documents and send to API", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_files_are_sent_as_documents()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new TextContentPart("What does this say?"), TextFile("This is a test document.", "text/plain", "test.txt"))).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler),
            "{\"documents\":[{\"data\":{\"text\":\"This is a test document.\",\"title\":\"test.txt\"}}],\"messages\":[{\"content\":\"What does this say?\",\"role\":\"user\"}],\"model\":\"command-r-plus\"}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > citations::should extract multiple text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Multiple_text_files_are_sent_as_documents()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(
            handler,
            UserPrompt(
                new TextContentPart("What do these documents say?"),
                TextFile("First document content", "text/plain", "doc1.txt"),
                TextFile("Second document content", "text/plain", "doc2.txt"))).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler),
            "{\"documents\":[{\"data\":{\"text\":\"First document content\",\"title\":\"doc1.txt\"}},{\"data\":{\"text\":\"Second document content\",\"title\":\"doc2.txt\"}}],\"messages\":[{\"content\":\"What do these documents say?\",\"role\":\"user\"}],\"model\":\"command-r-plus\"}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > citations::should support JSON files", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_files_are_sent_as_documents()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new TextContentPart("What is in this JSON?"), TextFile("{\"key\": \"value\"}", "application/json", "data.json"))).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler),
            "{\"documents\":[{\"data\":{\"text\":\"{\\\"key\\\": \\\"value\\\"}\",\"title\":\"data.json\"}}],\"messages\":[{\"content\":\"What is in this JSON?\",\"role\":\"user\"}],\"model\":\"command-r-plus\"}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > citations::should not include mediaType in the outgoing payload (category D)", Coverage = UpstreamCoverage.Covered)]
    public async Task Document_media_type_is_not_sent()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new TextContentPart("What is this?"), TextFile("Some file content", "application/pdf", "document.pdf"))).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["documents"], "[{\"data\":{\"text\":\"Some file content\",\"title\":\"document.pdf\"}}]");
        Assert.DoesNotContain("application/pdf", handler.Calls[0].Text, StringComparison.Ordinal);
        Assert.DoesNotContain("mediaType", handler.Calls[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > citations::should successfully process supported text media types", Coverage = UpstreamCoverage.Covered)]
    public async Task Plain_text_and_markdown_files_are_sent_as_documents()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(
            handler,
            UserPrompt(
                new TextContentPart("What is this?"),
                TextFile("This is plain text content", "text/plain", "text.txt"),
                TextFile("# Markdown Header\nContent", "text/markdown", "doc.md"))).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler)["documents"],
            "[{\"data\":{\"text\":\"This is plain text content\",\"title\":\"text.txt\"}},{\"data\":{\"text\":\"# Markdown Header\\nContent\",\"title\":\"doc.md\"}}]");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > citations::should not include documents parameter when no files present", Coverage = UpstreamCoverage.Covered)]
    public async Task Documents_are_omitted_without_files()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, TestPrompt()).ConfigureAwait(false);
        Assert.False(Body(handler).ContainsKey("documents"));
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should pass model and messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_model_and_messages()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, TestPrompt()).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), "{\"messages\":" + Messages + ",\"model\":\"command-r-plus\"}");
        Assert.Equal("https://api.cohere.com/v2/chat", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should pass tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_tools()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.ToolChoice = ToolChoice.None;
        options.Tools = new[] { Tool("test-tool", TestToolSchema) };
        await Generate(handler, options).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler),
            "{\"messages\":" + Messages + ",\"model\":\"command-r-plus\",\"tool_choice\":\"NONE\",\"tools\":[{\"function\":{\"name\":\"test-tool\",\"parameters\":" + TestToolSchema + "},\"type\":\"function\"}]}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_provider_and_call_headers()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await Model(handler, provider => provider.Headers["Custom-Provider-Header"] = "provider-header-value").DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        AssertHeaders(handler.Calls[0]);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should pass response format", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_json_response_format()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.JsonSchema = JsonSerializer.Deserialize<JsonElement>("{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"]}");
        await Generate(handler, options).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler),
            "{\"messages\":" + Messages + ",\"model\":\"command-r-plus\",\"response_format\":{\"json_schema\":{\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"],\"type\":\"object\"},\"type\":\"json_object\"}}");
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_leaves_unset_settings_out_of_the_body()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, TestPrompt()).ConfigureAwait(false);
        Assert.Equal("{\"model\":\"command-r-plus\",\"messages\":" + Messages + "}", handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_exposes_response_headers()
    {
        var result = await Generate(JsonFixture("cohere-text", ("test-header", "test-value")), TestPrompt()).ConfigureAwait(false);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Equal("application/json", result.ResponseHeaders["Content-Type"]);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should extract usage", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Generate_extracts_usage()
    {
        var result = await Generate(JsonFixture("cohere-text"), TestPrompt()).ConfigureAwait(false);
        AssertUsage(result.Usage, 507, 10, TextUsage);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should preserve extra top-level and nested fields in raw usage", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Generate_keeps_unknown_usage_fields_in_raw_usage()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":" + ExtraUsage + "}"));
        var result = await Generate(handler, TestPrompt()).ConfigureAwait(false);
        AssertUsage(result.Usage, 507, 10, ExtraUsage);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should validate cached token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_rejects_string_cached_tokens()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":\"448\"}}"));
        await Assert.ThrowsAsync<TypeValidationException>(() => Generate(handler, TestPrompt())).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Chat + "doGenerate > request::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_has_no_response_id_model_or_timestamp()
    {
        var result = await Generate(JsonFixture("cohere-text"), TestPrompt()).ConfigureAwait(false);
        Assert.Null(result.ResponseId);
        Assert.Null(result.ResponseModelId);
        Assert.Null(result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > text::should stream text deltas", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Stream_emits_text_deltas()
    {
        var parts = await Stream(ChunksFixture("cohere-text"), TestPrompt()).ConfigureAwait(false);
        Assert.Equal(
            new[]
            {
                "stream-start", "response-metadata:321d178c-2c12-44d3-ae42-2f5510f6b1cc", "text-start:0",
                "text-delta:0:The", "text-delta:0: capital", "text-delta:0: of", "text-delta:0: France", "text-delta:0: is", "text-delta:0: Paris", "text-delta:0:.",
                "text-end:0", "finish:Stop:COMPLETE",
            },
            parts.Select(Describe));
        AssertUsage(((FinishStreamPart)parts[^1]).Usage, 507, 10, TextUsage);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > text::should include raw chunks when includeRawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_includes_raw_chunks_when_asked()
    {
        var options = TestPrompt();
        options.IncludeRawChunks = true;
        var raw = (await Stream(ChunksFixture("cohere-text"), options).ConfigureAwait(false)).OfType<RawStreamPart>().Select(part => part.RawJson);
        Assert.Equal(File.ReadAllLines(Fixture("cohere-text.chunks.txt")).Where(line => line.Trim().Length > 0), raw);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > text::should not include raw chunks when includeRawChunks is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_leaves_out_raw_chunks_by_default()
    {
        var parts = await Stream(ChunksFixture("cohere-text"), TestPrompt()).ConfigureAwait(false);
        Assert.DoesNotContain(parts, part => part is RawStreamPart);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > text::should preserve extra top-level and nested fields in raw usage", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Stream_keeps_unknown_usage_fields_in_raw_usage()
    {
        var parts = await Stream(ChunkLines("{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":" + ExtraUsage + "}}"), TestPrompt()).ConfigureAwait(false);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        AssertUsage(finish.Usage, 507, 10, ExtraUsage);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > text::should finish successfully when billed units are absent", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Stream_finishes_without_billed_units()
    {
        const string usage = "{\"tokens\":{\"input_tokens\":507,\"output_tokens\":10}}";
        var parts = await Stream(ChunkLines("{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":" + usage + "}}"), TestPrompt()).ConfigureAwait(false);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        AssertUsage(finish.Usage, 507, 10, usage);
    }

    [Theory]
    [InlineData("{\"billed_units\":{\"input_tokens\":\"12\",\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":448}")]
    [InlineData("{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":\"448\"}")]
    [UpstreamTest(Chat + "doStream > text::should validate streamed $field", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_reports_invalid_usage_as_an_error(string usage)
    {
        var parts = await Stream(ChunkLines("{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":" + usage + "}}"), TestPrompt()).ConfigureAwait(false);
        Assert.Equal(new[] { "stream-start", "error", "finish:Error:" }, parts.Select(Describe));
        AssertNoUsage(((FinishStreamPart)parts[^1]).Usage);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > reasoning::should stream reasoning deltas", Coverage = UpstreamCoverage.Partial, Note = UsageNote)]
    public async Task Stream_emits_reasoning_then_text()
    {
        var parts = await Stream(ChunksFixture("cohere-reasoning"), TestPrompt()).ConfigureAwait(false);
        var described = parts.Select(Describe).ToList();
        Assert.Equal(new[] { "stream-start", "response-metadata:c9117d7f-a7e4-499f-b643-a2a1e139687b", "reasoning-start:0" }, described.Take(3));
        Assert.Equal(
            "The user is asking for the sum of 2 and 2. Since this is a straightforward arithmetic problem, I don't need to use any tools. I can calculate the answer directly.",
            string.Concat(parts.OfType<ReasoningDeltaStreamPart>().Select(part => part.Id == "0" ? part.Delta : "?")));
        Assert.Equal(
            new[] { "reasoning-end:0", "text-start:1" },
            described.SkipWhile(part => !part.StartsWith("reasoning-end", StringComparison.Ordinal)).Take(2));
        Assert.Equal("The answer to 2 + 2 is 4.", string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Id == "1" ? part.Delta : "?")));
        Assert.Equal(new[] { "text-end:1", "finish:Stop:COMPLETE" }, described.Skip(described.Count - 2));
        AssertUsage(((FinishStreamPart)parts[^1]).Usage, 1394, 54, "{\"billed_units\":{\"input_tokens\":8,\"output_tokens\":50},\"tokens\":{\"input_tokens\":1394,\"output_tokens\":54},\"cached_tokens\":1360}");
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > tool call::should stream tool deltas", Coverage = UpstreamCoverage.Partial, Note = "No tool-input-start, tool-input-delta, or tool-input-end parts. " + UsageNote)]
    public async Task Stream_emits_completed_tool_calls()
    {
        var options = TestPrompt();
        options.Tools = new[] { Tool("test-tool", TestToolSchema) };
        var parts = await Stream(ChunksFixture("cohere-tool-call"), options).ConfigureAwait(false);
        Assert.Equal(
            new[]
            {
                "stream-start", "response-metadata:2941521a-b87a-45f6-9b0d-235fd66c3025",
                "tool-call:weather_e8p4pn45zt0t:weather:{\"location\":\"San Francisco\"}",
                "tool-call:cityAttractions_pyxssbwnq9fq:cityAttractions:{\"city\":\"San Francisco\"}",
                "finish:ToolCalls:TOOL_CALL",
            },
            parts.Select(Describe));
        AssertUsage(((FinishStreamPart)parts[^1]).Usage, 1549, 95, "{\"billed_units\":{\"input_tokens\":119,\"output_tokens\":44},\"tokens\":{\"input_tokens\":1549,\"output_tokens\":95},\"cached_tokens\":1504}");
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > tool call::rejects prototype keys in streamed tool call arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_rejects_prototype_keys_in_tool_arguments()
    {
        var handler = ChunkLines(
            "{\"type\":\"tool-call-start\",\"index\":0,\"delta\":{\"message\":{\"tool_calls\":{\"id\":\"test-tool-call\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"\"}}}}}",
            "{\"type\":\"tool-call-delta\",\"index\":0,\"delta\":{\"message\":{\"tool_calls\":{\"function\":{\"arguments\":\"{\\\"__proto__\\\":{\\\"polluted\\\":true}}\"}}}}}",
            "{\"type\":\"tool-call-end\",\"index\":0}");
        var options = TestPrompt();
        options.Tools = new[] { Tool("test-tool", "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":true,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}") };
        var error = await Assert.ThrowsAsync<JsonException>(() => Stream(handler, options)).ConfigureAwait(false);
        Assert.Contains("Object contains forbidden prototype property", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > empty tool call::should handle empty tool call arguments", Coverage = UpstreamCoverage.Partial, Note = "No tool-input-start or tool-input-end parts. " + UsageNote)]
    public async Task Stream_turns_empty_tool_arguments_into_an_empty_object()
    {
        var options = TestPrompt();
        options.Tools = new[] { Tool("test-tool", "{\"type\":\"object\",\"properties\":{},\"required\":[],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}") };
        var parts = await Stream(ChunksFixture("cohere-empty-tool-call"), options).ConfigureAwait(false);
        Assert.Equal(
            new[] { "stream-start", "response-metadata:66dec7d7-45e6-427c-8fd9-7d6375d12046", "tool-call:currentTime_y46ar19t5gvw:currentTime:{}", "finish:ToolCalls:TOOL_CALL" },
            parts.Select(Describe));
        AssertUsage(((FinishStreamPart)parts[^1]).Usage, 1445, 43, "{\"billed_units\":{\"input_tokens\":46,\"output_tokens\":14},\"tokens\":{\"input_tokens\":1445,\"output_tokens\":43},\"cached_tokens\":704}");
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > error handling::should handle unparsable stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_reports_unparsable_chunks_as_errors()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("event: foo-message\ndata: {unparsable}\n\n"), "text/event-stream"));
        var parts = await Stream(handler, TestPrompt()).ConfigureAwait(false);
        Assert.Equal(new[] { "stream-start", "error", "finish:Error:" }, parts.Select(Describe));
        Assert.StartsWith("JSON parsing failed: Text: {unparsable}.", ((ErrorStreamPart)parts[1]).Message, StringComparison.Ordinal);
        AssertNoUsage(((FinishStreamPart)parts[^1]).Usage);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > request::should pass the messages and the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_model_messages_and_stream_flag()
    {
        var handler = ChunksFixture("cohere-text");
        await Stream(handler, TestPrompt()).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler), "{\"messages\":" + Messages + ",\"model\":\"command-r-plus\",\"stream\":true}");
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > request::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_provider_and_call_headers()
    {
        var handler = ChunksFixture("cohere-text");
        var options = TestPrompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await UpstreamChat.Read(Model(handler, provider => provider.Headers["Custom-Provider-Header"] = "provider-header-value").DoStreamAsync(options, CancellationToken.None)).ConfigureAwait(false);
        AssertHeaders(handler.Calls[0]);
    }

    [Fact]
    [UpstreamTest(Chat + "doStream > request::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_leaves_unset_settings_out_of_the_body()
    {
        var handler = ChunksFixture("cohere-text");
        await Stream(handler, TestPrompt()).ConfigureAwait(false);
        Assert.Equal("{\"model\":\"command-r-plus\",\"messages\":" + Messages + ",\"stream\":true}", handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(Prompt + "file processing::should extract documents from file parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Files_become_documents_and_text_stays_a_string()
    {
        var handler = JsonFixture("cohere-text");
        var result = await Generate(handler, UserPrompt(new TextContentPart("Analyze this file: "), TextFile("This is file content", "text/plain", "test.txt"))).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["messages"], "[{\"role\":\"user\",\"content\":\"Analyze this file: \"}]");
        JsonAssert.Equal(Body(handler)["documents"], "[{\"data\":{\"text\":\"This is file content\",\"title\":\"test.txt\"}}]");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prompt + "file processing::should accept top-level-only mediaType without error (category D: mediaType not consumed)", Coverage = UpstreamCoverage.Covered)]
    public async Task Top_level_text_media_type_becomes_a_document()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(TextFile("This is file content", "text", "test.txt"))).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["messages"], "[{\"role\":\"user\",\"content\":\"\"}]");
        JsonAssert.Equal(Body(handler)["documents"], "[{\"data\":{\"text\":\"This is file content\",\"title\":\"test.txt\"}}]");
    }

    [Fact]
    [UpstreamTest(Prompt + "file processing::should not read mediaType (document payload carries only text + title)", Coverage = UpstreamCoverage.Covered)]
    public async Task Document_payload_has_only_text_and_title()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(TextFile("PDF-like content", "application/pdf", "test.pdf"))).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["documents"], "[{\"data\":{\"text\":\"PDF-like content\",\"title\":\"test.pdf\"}}]");
        Assert.DoesNotContain("application/pdf", handler.Calls[0].Text, StringComparison.Ordinal);
        Assert.DoesNotContain("mediaType", handler.Calls[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prompt + "image processing::should convert image file with data bytes into image_url data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_bytes_become_a_data_uri()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new TextContentPart("What is in this image?"), new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null))).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler)["messages"],
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"What is in this image?\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\"}}]}]");
        Assert.False(Body(handler).ContainsKey("documents"));
    }

    [Fact]
    [UpstreamTest(Prompt + "image processing::should convert image file with URL data into image_url URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_urls_are_sent_as_is()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new FileContentPart("image/png", "https://example.com/cat.png", null, null))).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["messages"], "[{\"role\":\"user\",\"content\":[{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/cat.png\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prompt + "image processing::should omit detail when no provider option is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_detail_is_omitted()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null))).ConfigureAwait(false);
        var part = Body(handler)["messages"]![0]!["content"]![0]!;
        Assert.Equal("image_url", part["type"]!.GetValue<string>());
        Assert.False(part["image_url"]!.AsObject().ContainsKey("detail"));
    }

    [Fact]
    [UpstreamTest(Prompt + "image processing::should send image inline and route non-image file to documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Images_stay_inline_while_other_files_become_documents()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(
            handler,
            UserPrompt(
                new TextContentPart("See attached:"),
                new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null),
                TextFile("Doc text", "text/plain", "note.txt"))).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler)["messages"],
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"See attached:\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AAECAw==\"}}]}]");
        JsonAssert.Equal(Body(handler)["documents"], "[{\"data\":{\"text\":\"Doc text\",\"title\":\"note.txt\"}}]");
    }

    [Fact]
    [UpstreamTest(Prompt + "image processing::should accept top-level \"image\" media type and detect full type from bytes", Coverage = UpstreamCoverage.Covered)]
    public async Task Top_level_image_media_type_is_detected_from_bytes()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, UserPrompt(new FileContentPart("image", null, new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, null))).ConfigureAwait(false);
        var part = Body(handler)["messages"]![0]!["content"]![0]!;
        Assert.Equal("image_url", part["type"]!.GetValue<string>());
        Assert.StartsWith("data:image/png;base64,", part["image_url"]!["url"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prompt + "tool messages::should convert a tool call into a cohere chatbot message", Coverage = UpstreamCoverage.Covered)]
    public async Task Assistant_tool_calls_drop_the_text()
    {
        var handler = JsonFixture("cohere-text");
        var call = new GeneratedToolCall("tool-call-1", "tool-1", "{\"test\":\"This is a tool message\"}");
        var result = await Generate(handler, new LanguageModelCallOptions { Prompt = new ModelMessage[] { new AssistantModelMessage("Calling a tool", new[] { call }, null) } }).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler)["messages"],
            "[{\"role\":\"assistant\",\"tool_calls\":[{\"id\":\"tool-call-1\",\"type\":\"function\",\"function\":{\"name\":\"tool-1\",\"arguments\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\"}}]}]");
        Assert.False(Body(handler).ContainsKey("documents"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prompt + "tool messages::should convert a single tool result into a cohere tool message", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_result_becomes_a_tool_message()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(handler, new LanguageModelCallOptions { Prompt = new ModelMessage[] { new ToolModelMessage("tool-call-1", "tool-1", "{\"test\":\"This is a tool message\"}", false) } }).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["messages"], "[{\"role\":\"tool\",\"content\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\",\"tool_call_id\":\"tool-call-1\"}]");
    }

    [Fact]
    [UpstreamTest(Prompt + "tool messages::should convert multiple tool results into a cohere tool message", Coverage = UpstreamCoverage.Covered)]
    public async Task Each_tool_result_becomes_a_tool_message()
    {
        var handler = JsonFixture("cohere-text");
        await Generate(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new ToolModelMessage("tool-call-1", "tool-1", "{\"test\":\"This is a tool message\"}", false),
                    new ToolModelMessage("tool-call-2", "tool-2", "{\"something\":\"else\"}", false),
                },
            }).ConfigureAwait(false);
        JsonAssert.Equal(
            Body(handler)["messages"],
            "[{\"role\":\"tool\",\"content\":\"{\\\"test\\\":\\\"This is a tool message\\\"}\",\"tool_call_id\":\"tool-call-1\"},{\"role\":\"tool\",\"content\":\"{\\\"something\\\":\\\"else\\\"}\",\"tool_call_id\":\"tool-call-2\"}]");
    }

    [Fact]
    [UpstreamTest(Tools + "should return undefined tools when no tools are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_tools_are_left_out()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Tools = Array.Empty<LanguageModelTool>();
        var result = await Generate(handler, options).ConfigureAwait(false);
        Assert.False(Body(handler).ContainsKey("tools"));
        Assert.False(Body(handler).ContainsKey("tool_choice"));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "should process function tools correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Function_tools_are_sent_with_description_and_parameters()
    {
        var body = await ToolBody(null).ConfigureAwait(false);
        JsonAssert.Equal(body["tools"], BasicTools);
        Assert.False(body.ContainsKey("tool_choice"));
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle auto tool choice", Coverage = UpstreamCoverage.Covered)]
    public async Task Auto_tool_choice_is_left_out()
    {
        Assert.False((await ToolBody(ToolChoice.Auto).ConfigureAwait(false)).ContainsKey("tool_choice"));
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle none tool choice", Coverage = UpstreamCoverage.Covered)]
    public async Task None_tool_choice_is_sent_as_none()
    {
        var body = await ToolBody(ToolChoice.None).ConfigureAwait(false);
        JsonAssert.Equal(body["tools"], BasicTools);
        Assert.Equal("NONE", body["tool_choice"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle required tool choice", Coverage = UpstreamCoverage.Covered)]
    public async Task Required_tool_choice_is_sent_as_required()
    {
        var body = await ToolBody(ToolChoice.Required).ConfigureAwait(false);
        JsonAssert.Equal(body["tools"], BasicTools);
        Assert.Equal("REQUIRED", body["tool_choice"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Tools + "tool choice handling::should handle tool type tool choice by filtering tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Named_tool_choice_keeps_only_that_tool()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Tools = new[] { BasicTool(), Tool("otherFunction", "{\"type\":\"object\",\"properties\":{}}") };
        options.ToolChoice = ToolChoice.Tool("testFunction");
        await Generate(handler, options).ConfigureAwait(false);
        JsonAssert.Equal(Body(handler)["tools"], BasicTools);
        Assert.Equal("REQUIRED", Body(handler)["tool_choice"]!.GetValue<string>());
    }

    private static LanguageModelTool BasicTool()
    {
        return new LanguageModelTool("testFunction", "test description", JsonSerializer.Deserialize<JsonElement>("{\"type\":\"object\",\"properties\":{}}"));
    }

    private static async Task<JsonObject> ToolBody(ToolChoice? choice)
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Tools = new[] { BasicTool() };
        options.ToolChoice = choice;
        await Generate(handler, options).ConfigureAwait(false);
        return Body(handler);
    }

    private static ILanguageModel Model(ParityHandler handler, Action<CohereOptions>? configure = null)
    {
        var options = new CohereOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return CohereProvider.Create(options, handler).LanguageModel("command-r-plus");
    }

    private static Task<LanguageModelGenerateResult> Generate(ParityHandler handler, LanguageModelCallOptions options)
    {
        return Model(handler).DoGenerateAsync(options, CancellationToken.None);
    }

    private static Task<List<LanguageModelStreamPart>> Stream(ParityHandler handler, LanguageModelCallOptions options)
    {
        return UpstreamChat.Read(Model(handler).DoStreamAsync(options, CancellationToken.None));
    }

    private static LanguageModelCallOptions TestPrompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("you are a friendly bot!"), new UserModelMessage("Hello") },
        };
    }

    private static LanguageModelCallOptions UserPrompt(params UserContentPart[] parts)
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage(parts) } };
    }

    private static FileContentPart TextFile(string text, string mediaType, string fileName)
    {
        return new FileContentPart(mediaType, null, Encoding.UTF8.GetBytes(text), fileName);
    }

    private static LanguageModelTool Tool(string name, string schema)
    {
        return new LanguageModelTool(name, null, JsonSerializer.Deserialize<JsonElement>(schema));
    }

    private static string Fixture(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
    }

    private static ParityHandler JsonFixture(string name, params (string Name, string Value)[] headers)
    {
        var json = File.ReadAllText(Fixture(name + ".json"));
        return new ParityHandler(_ => ParityHandler.Json(json, headers));
    }

    private static ParityHandler ChunksFixture(string name)
    {
        return ChunkLines(File.ReadAllLines(Fixture(name + ".chunks.txt")).Where(line => line.Trim().Length > 0).ToArray());
    }

    // Each line becomes one SSE event named after its type, like the Cohere API sends them.
    private static ParityHandler ChunkLines(params string[] lines)
    {
        var sse = new StringBuilder();
        foreach (var line in lines)
        {
            sse.Append("event: ").Append(JsonNode.Parse(line)!["type"]!.GetValue<string>()).Append("\ndata: ").Append(line).Append("\n\n");
        }

        var bytes = Encoding.UTF8.GetBytes(sse.ToString());
        return new ParityHandler(_ => ParityHandler.Bytes(bytes, "text/event-stream"));
    }

    private static JsonObject Body(ParityHandler handler)
    {
        return JsonNode.Parse(handler.Calls[0].Text)!.AsObject();
    }

    private static void AssertHeaders(ParityCall call)
    {
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.StartsWith("application/json", call.Header("Content-Type"), StringComparison.Ordinal);
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
    }

    private static void AssertToolCall(GeneratedContent part, string id, string name, string arguments)
    {
        var call = Assert.IsType<GeneratedToolCall>(part);
        Assert.Equal(id, call.ToolCallId);
        Assert.Equal(name, call.ToolName);
        Assert.Equal(arguments, call.ArgumentsJson);
    }

    private static void AssertUsage(LanguageModelUsage usage, int input, int output, string raw)
    {
        Assert.Equal(input, usage.InputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(input + output, usage.TotalTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Null(usage.ReasoningTokens);
        JsonAssert.Equal(usage.Raw!.Value, raw);
    }

    private static void AssertNoUsage(LanguageModelUsage usage)
    {
        Assert.Null(usage.InputTokens);
        Assert.Null(usage.OutputTokens);
        Assert.Null(usage.Raw);
    }

    private static string Describe(LanguageModelStreamPart part)
    {
        return part switch
        {
            ResponseMetadataStreamPart metadata => "response-metadata:" + metadata.Id,
            TextStartStreamPart start => "text-start:" + start.Id,
            TextDeltaStreamPart delta => "text-delta:" + delta.Id + ":" + delta.Delta,
            TextEndStreamPart end => "text-end:" + end.Id,
            ReasoningStartStreamPart start => "reasoning-start:" + start.Id,
            ReasoningDeltaStreamPart delta => "reasoning-delta:" + delta.Id,
            ReasoningEndStreamPart end => "reasoning-end:" + end.Id,
            ToolCallStreamPart call => "tool-call:" + call.ToolCallId + ":" + call.ToolName + ":" + call.ArgumentsJson,
            FinishStreamPart finish => "finish:" + finish.FinishReason + ":" + finish.RawFinishReason,
            _ => part.Type,
        };
    }
}
