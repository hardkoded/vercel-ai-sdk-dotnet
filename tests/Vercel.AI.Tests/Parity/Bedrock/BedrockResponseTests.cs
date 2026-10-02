// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Converse response parsing for text, tools, reasoning, and usage.</summary>
public sealed class BedrockResponseTests
{
    private const string Haiku = "anthropic.claude-3-haiku-20240307-v1:0";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > text::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_text()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello, World!\"}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal("Hello, World!", ((GeneratedText)parsed.Content[0]).Text);
        Assert.Equal(FinishReason.Stop, parsed.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > text::should extract text from citation content", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_text_from_citation_content()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"citationsContent\":{\"content\":[{\"text\":\"cited\"}]}}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal("cited", ((GeneratedText)parsed.Content[0]).Text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > text::should prefer regular text when citation content is also present", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_regular_text_over_citation_content()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"plain\",\"citationsContent\":{\"content\":[{\"text\":\"cited\"}]}}]}},\"stopReason\":\"end_turn\"}");

        Assert.Single(parsed.Content);
        Assert.Equal("plain", ((GeneratedText)parsed.Content[0]).Text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > text::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_usage()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":22,\"outputTokens\":57,\"totalTokens\":79,\"cacheReadInputTokens\":0,\"cacheWriteInputTokens\":0}}");

        Assert.Equal(22, parsed.Usage.InputTokens);
        Assert.Equal(57, parsed.Usage.OutputTokens);
        Assert.Equal(79, parsed.Usage.TotalTokens);
        Assert.Equal(0, parsed.Usage.CacheReadTokens);
        Assert.Equal(0, parsed.Usage.CacheWriteTokens);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should extract finish reason", Coverage = UpstreamCoverage.Covered)]
    public void Maps_end_turn_to_stop()
    {
        Assert.Equal(FinishReason.Stop, Parse("{\"stopReason\":\"end_turn\",\"output\":{\"message\":{\"content\":[]}}}").FinishReason);
        Assert.Equal(FinishReason.Stop, AmazonBedrockFinishReason.Map("stop_sequence", false));
        Assert.Equal(FinishReason.Length, AmazonBedrockFinishReason.Map("max_tokens", false));
        Assert.Equal(FinishReason.ContentFilter, AmazonBedrockFinishReason.Map("content_filtered", false));
        Assert.Equal(FinishReason.ContentFilter, AmazonBedrockFinishReason.Map("guardrail_intervened", false));
        Assert.Equal(FinishReason.ToolCalls, AmazonBedrockFinishReason.Map("tool_use", false));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public void Maps_an_unknown_stop_reason_to_other()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello, World!\"}]}},\"stopReason\":\"eos\",\"usage\":{\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38}}");

        Assert.Equal(FinishReason.Other, parsed.FinishReason);
        Assert.Equal("eos", parsed.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should generate an ID when toolUseId is empty", Coverage = UpstreamCoverage.Covered)]
    public void Generates_an_id_when_the_tool_use_id_is_empty()
    {
        var parsed = Parse(
            "{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"toolUseId\":\"\",\"name\":\"lookup\",\"input\":{}}}]}},\"stopReason\":\"tool_use\"}",
            usesJsonResponseTool: false,
            () => "generated-id");
        var call = (GeneratedToolCall)parsed.Content[0];

        Assert.Equal("generated-id", call.ToolCallId);
        Assert.Equal("lookup", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should support tool calls with empty input (no arguments)", Coverage = UpstreamCoverage.Covered)]
    public void Treats_a_missing_tool_input_as_an_empty_object()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"toolUseId\":\"call-1\",\"name\":\"lookup\"}}]}},\"stopReason\":\"tool_use\"}");
        var call = (GeneratedToolCall)parsed.Content[0];

        Assert.Equal("{}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, parsed.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > reasoning::should extract reasoning and text response", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_reasoning_and_text()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"reasoningContent\":{\"reasoningText\":{\"text\":\"because\"}}},{\"text\":\"answer\"}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal("because", ((GeneratedReasoning)parsed.Content[0]).Text);
        Assert.Equal("answer", ((GeneratedText)parsed.Content[1]).Text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should extract reasoning text with signature", Coverage = UpstreamCoverage.Covered)]
    public void Stores_the_reasoning_signature_in_provider_metadata()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"reasoningContent\":{\"reasoningText\":{\"text\":\"because\",\"signature\":\"sig\"}}}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal("sig", parsed.ProviderMetadata!.Value.GetProperty("amazonBedrock").GetProperty("reasoning")[0].GetProperty("signature").GetString());
        Assert.Equal("sig", parsed.ProviderMetadata.Value.GetProperty("bedrock").GetProperty("reasoning")[0].GetProperty("signature").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should extract reasoning text without signature", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_reasoning_text_when_the_signature_is_absent()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"reasoningContent\":{\"reasoningText\":{\"text\":\"because\"}}}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal("because", ((GeneratedReasoning)parsed.Content[0]).Text);
        Assert.Equal(JsonValueKind.Object, parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("reasoning")[0].ValueKind);
        Assert.False(parsed.ProviderMetadata.Value.GetProperty("bedrock").GetProperty("reasoning")[0].TryGetProperty("signature", out _));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should extract redacted reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_redacted_reasoning_data()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"reasoningContent\":{\"redactedReasoning\":{\"data\":\"opaque\"}}}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal(string.Empty, ((GeneratedReasoning)parsed.Content[0]).Text);
        Assert.Equal("opaque", parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("reasoning")[0].GetProperty("redactedData").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should expose reasoning redacted as `redactedContent` for replay", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_redacted_content_for_replay()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"reasoningContent\":{\"redactedContent\":\"block\"}}]}},\"stopReason\":\"end_turn\"}");

        Assert.Equal("block", parsed.ProviderMetadata!.Value.GetProperty("amazonBedrock").GetProperty("reasoning")[0].GetProperty("redactedContent").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with json tool response::should return the json response as text", Coverage = UpstreamCoverage.Covered)]
    public void Returns_a_json_tool_call_as_text()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"toolUseId\":\"json-1\",\"name\":\"json\",\"input\":{\"name\":\"Ada\"}}}]}},\"stopReason\":\"tool_use\"}", true);

        Assert.Equal("{\"name\":\"Ada\"}", ((GeneratedText)parsed.Content[0]).Text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with json tool response::should send stop finish reason when json tool is used", Coverage = UpstreamCoverage.Covered)]
    public void Finishes_as_stop_when_the_json_tool_is_used()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"name\":\"json\",\"input\":{}}}]}},\"stopReason\":\"tool_use\"}", true);

        Assert.Equal(FinishReason.Stop, parsed.FinishReason);
        Assert.Equal("tool_use", parsed.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with json tool response::should set isJsonResponseFromTool in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public void Marks_a_json_tool_response_in_provider_metadata()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"name\":\"json\",\"input\":{}}}]}},\"stopReason\":\"tool_use\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1}}", true);

        Assert.True(parsed.ProviderMetadata!.Value.GetProperty("amazonBedrock").GetProperty("isJsonResponseFromTool").GetBoolean());
        Assert.True(parsed.ProviderMetadata.Value.GetProperty("bedrock").GetProperty("isJsonResponseFromTool").GetBoolean());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with other tool response::should return the regular tool call", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_regular_tool_call_when_json_mode_is_active()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"toolUseId\":\"call-1\",\"name\":\"lookup\",\"input\":{\"q\":\"x\"}}}]}},\"stopReason\":\"tool_use\"}", true);
        var call = (GeneratedToolCall)parsed.Content[0];

        Assert.Equal("lookup", call.ToolName);
        Assert.Equal("{\"q\":\"x\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with other tool response::should send tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public void Finishes_as_tool_calls_for_a_regular_tool()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"toolUseId\":\"call-1\",\"name\":\"lookup\",\"input\":{}}}]}},\"stopReason\":\"tool_use\"}", true);

        Assert.Equal(FinishReason.ToolCalls, parsed.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include text content before JSON tool call in doGenerate", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_text_that_precedes_a_json_tool_call()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"intro\"},{\"toolUse\":{\"name\":\"json\",\"input\":{\"ok\":true}}}]}},\"stopReason\":\"tool_use\"}", true);

        Assert.Equal("intro", ((GeneratedText)parsed.Content[0]).Text);
        Assert.Equal("{\"ok\":true}", ((GeneratedText)parsed.Content[1]).Text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include stop_sequence in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_stop_sequence()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"stop_sequence\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1},\"additionalModelResponseFields\":{\"delta\":{\"stop_sequence\":\"END\"}}}");

        Assert.Equal("END", parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("stopSequence").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should handle stop_sequence: null when stopReason is tool_use", Coverage = UpstreamCoverage.Covered)]
    public void Records_a_null_stop_sequence()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[]}},\"stopReason\":\"tool_use\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1},\"additionalModelResponseFields\":{\"delta\":{\"stop_sequence\":null}}}");

        Assert.Equal(JsonValueKind.Null, parsed.ProviderMetadata!.Value.GetProperty("amazonBedrock").GetProperty("stopSequence").ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include trace information in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public void Includes_trace_metadata()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1},\"trace\":{\"guardrail\":{\"action\":\"BLOCKED\"}}}");

        Assert.Equal("BLOCKED", parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("trace").GetProperty("guardrail").GetProperty("action").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include serviceTier in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_response_service_tier()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1},\"serviceTier\":{\"type\":\"priority\"}}");

        Assert.Equal("priority", parsed.ProviderMetadata!.Value.GetProperty("amazonBedrock").GetProperty("serviceTier").GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include cache token usage in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public void Includes_cache_write_tokens_in_provider_metadata()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":10,\"outputTokens\":2,\"cacheWriteInputTokens\":4}}");

        Assert.Equal(4, parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("usage").GetProperty("cacheWriteInputTokens").GetInt32());
        Assert.Equal(14, parsed.Usage.InputTokens);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should include cacheDetails in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public void Includes_cache_details_in_provider_metadata()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":10,\"outputTokens\":2,\"cacheDetails\":[{\"inputTokens\":2,\"ttl\":\"T5M\"}]}}");

        Assert.Equal("T5M", parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("usage").GetProperty("cacheDetails")[0].GetProperty("ttl").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > text::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_the_request_id_and_date_header()
    {
        var handler = new BedrockJsonHandler("{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}");
        handler.ResponseHeaders["x-amzn-requestid"] = "req-1";
        handler.ResponseHeaders["date"] = "Wed, 01 Jan 2020 00:00:00 GMT";
        var model = BedrockParity.Model(handler, Haiku);
        var result = await model.DoGenerateAsync(BedrockParity.Call("hi", null), CancellationToken.None);

        Assert.Equal("req-1", result.ResponseId);
        Assert.Equal(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), result.ResponseTimestamp);
        Assert.Equal("req-1", result.ResponseHeaders["x-amzn-requestid"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should surface guardrail intervention as a content-filter finish reason with trace metadata", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_guardrail_stop_to_content_filter_and_keeps_the_trace()
    {
        var parsed = Parse("{\"output\":{\"message\":{\"content\":[{\"text\":\"\"}]}},\"stopReason\":\"guardrail_intervened\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1},\"trace\":{\"guardrail\":{\"action\":\"INTERVENED\"}}}");

        Assert.Equal(FinishReason.ContentFilter, parsed.FinishReason);
        Assert.Equal("INTERVENED", parsed.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("trace").GetProperty("guardrail").GetProperty("action").GetString());
    }

    private static AmazonBedrockParsedResponse Parse(string json, bool usesJsonResponseTool = false, Func<string>? generateId = null)
    {
        using var document = JsonDocument.Parse(json);
        return AmazonBedrockResponseParser.Parse(document.RootElement, Haiku, usesJsonResponseTool, generateId, null);
    }
}
