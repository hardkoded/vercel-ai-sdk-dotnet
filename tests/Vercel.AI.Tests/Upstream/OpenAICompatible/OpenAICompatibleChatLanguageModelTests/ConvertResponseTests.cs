// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>convertResponse</c>.</summary>
public sealed class ConvertResponseTests
{
    private const string UpstreamCommit = "c9c6402cbdfce3f82b36b4073236043b1946dd06";
    private const string Prefix = "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::convertResponse::";

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture? capture = null, Action<OpenAICompatibleOptions>? configure = null)
    {
        return UpstreamChat.Model(capture ?? new UpstreamCapture(), "test-provider", "grok-3", configure);
    }

    private static string ReadFixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json"));
    }

    private static JsonElement CreateResponseBody(string message, string? overrides = null)
    {
        var json = "{\"id\":\"chatcmpl-test\",\"object\":\"chat.completion\",\"created\":1711115037,\"model\":\"grok-3\","
            + "\"choices\":[{\"index\":0,\"message\":" + message + ",\"finish_reason\":\"stop\"}],"
            + "\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}";
        if (overrides == null)
        {
            return UpstreamChat.Json(json);
        }

        var merged = System.Text.Json.Nodes.JsonNode.Parse(json)!.AsObject();
        foreach (var pair in System.Text.Json.Nodes.JsonNode.Parse(overrides)!.AsObject())
        {
            merged[pair.Key] = pair.Value?.DeepClone();
        }

        return UpstreamChat.Json(merged.ToJsonString());
    }

    private static string Describe(GeneratedContent part)
    {
        return part switch
        {
            GeneratedText text => "text:" + text.Text,
            GeneratedReasoning reasoning => "reasoning:" + reasoning.Text,
            GeneratedToolCall call => "tool-call:" + call.ToolCallId + ":" + call.ToolName + ":" + call.ArgumentsJson + ":" + call.ProviderMetadata?.GetRawText(),
            _ => part.Type,
        };
    }

    [Theory]
    [InlineData("xai-text")]
    [InlineData("xai-tool-call")]
    [UpstreamTest(Prefix + "should match doGenerate for the %s fixture", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_match_doGenerate_for_the_fixture(string fixture)
    {
        var capture = new UpstreamCapture { ResponseBody = ReadFixture(fixture) };
        var model = Model(capture);

        var generated = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        var converted = model.ConvertResponse(UpstreamChat.Json(ReadFixture(fixture)));

        Assert.Equal(generated.Content.Select(Describe), converted.Content.Select(Describe));
        Assert.Equal(generated.FinishReason, converted.FinishReason);
        Assert.Equal(generated.RawFinishReason, converted.RawFinishReason);
        Assert.Equal(generated.Usage.InputTokens, converted.Usage.InputTokens);
        Assert.Equal(generated.Usage.OutputTokens, converted.Usage.OutputTokens);
        Assert.Equal(generated.Usage.CacheReadTokens, converted.Usage.CacheReadTokens);
        Assert.Equal(generated.Usage.ReasoningTokens, converted.Usage.ReasoningTokens);
        Assert.Equal(generated.Usage.Raw?.GetRawText(), converted.Usage.Raw?.GetRawText());
        Assert.Equal(generated.ProviderMetadata?.GetRawText(), converted.ProviderMetadata?.GetRawText());
        Assert.Equal(generated.ResponseId, converted.ResponseId);
        Assert.Equal(generated.ResponseModelId, converted.ResponseModelId);
        Assert.Equal(generated.ResponseTimestamp, converted.ResponseTimestamp);
        Assert.Empty(converted.Warnings);
        Assert.Null(converted.RawResponse);
        Assert.Empty(converted.ResponseHeaders);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert reasoning, text, tool calls and prediction tokens", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_convert_reasoning_text_tool_calls_and_prediction_tokens()
    {
        var body = CreateResponseBody(
            "{\"role\":\"assistant\",\"content\":\"Hello!\",\"reasoning_content\":\"Thinking...\",\"tool_calls\":[{\"id\":\"call_1\",\"type\":\"function\","
            + "\"function\":{\"name\":\"weather\",\"arguments\":\"{\\\"location\\\":\\\"SF\\\"}\"},\"extra_content\":{\"google\":{\"thought_signature\":\"sig-123\"}}}]}",
            "{\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":30,\"total_tokens\":34,\"completion_tokens_details\":{\"accepted_prediction_tokens\":5,\"rejected_prediction_tokens\":2}}}");

        var result = Model().ConvertResponse(body);

        Assert.Collection(
            result.Content,
            part => Assert.Equal("Thinking...", Assert.IsType<GeneratedReasoning>(part).Text),
            part => Assert.Equal("Hello!", Assert.IsType<GeneratedText>(part).Text),
            part =>
            {
                var call = Assert.IsType<GeneratedToolCall>(part);
                Assert.Equal("{\"location\":\"SF\"}", call.ArgumentsJson);
                Assert.Equal("call_1", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
                Assert.Equal("sig-123", call.ProviderMetadata!.Value.GetProperty("test-provider").GetProperty("thoughtSignature").GetString());
            });
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        var metadata = result.ProviderMetadata!.Value.GetProperty("test-provider");
        Assert.Equal(5, metadata.GetProperty("acceptedPredictionTokens").GetInt32());
        Assert.Equal(2, metadata.GetProperty("rejectedPredictionTokens").GetInt32());
        Assert.Equal("chatcmpl-test", result.ResponseId);
        Assert.Equal("grok-3", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1711115037), result.ResponseTimestamp);
        Assert.Equal(4, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(4, result.Usage.NoCacheInputTokens);
        Assert.Equal(30, result.Usage.OutputTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
        Assert.Equal(30, result.Usage.TextTokens);
        Assert.Equal(34, result.Usage.Raw!.Value.GetProperty("total_tokens").GetInt32());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should generate an id for tool calls without one", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_generate_an_id_for_tool_calls_without_one()
    {
        var result = Model().ConvertResponse(CreateResponseBody(
            "{\"role\":\"assistant\",\"tool_calls\":[{\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"}}]}"));

        var toolCall = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.False(string.IsNullOrEmpty(toolCall.ToolCallId));
    }

    [Fact]
    [UpstreamTest(Prefix + "should key provider metadata by the raw provider name by default", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_key_provider_metadata_by_the_raw_provider_name_by_default()
    {
        var body = CreateResponseBody(
            "{\"role\":\"assistant\",\"tool_calls\":[{\"id\":\"call_1\",\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"},"
            + "\"extra_content\":{\"google\":{\"thought_signature\":\"sig-123\"}}}]}");

        var result = Model().ConvertResponse(body);

        Assert.Equal(new[] { "test-provider" }, result.ProviderMetadata == null ? Array.Empty<string>() : result.ProviderMetadata.Value.EnumerateObject().Select(p => p.Name));
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("sig-123", call.ProviderMetadata!.Value.GetProperty("test-provider").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should key provider metadata by the camelCase name when providerOptions use it", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_key_provider_metadata_by_the_camelCase_name_when_providerOptions_use_it()
    {
        var body = CreateResponseBody(
            "{\"role\":\"assistant\",\"tool_calls\":[{\"id\":\"call_1\",\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"},"
            + "\"extra_content\":{\"google\":{\"thought_signature\":\"sig-123\"}}}]}");

        var result = Model().ConvertResponse(body, UpstreamChat.Bag("testProvider", "{}"));

        Assert.Equal(new[] { "testProvider" }, result.ProviderMetadata == null ? Array.Empty<string>() : result.ProviderMetadata.Value.EnumerateObject().Select(p => p.Name));
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("sig-123", call.ProviderMetadata!.Value.GetProperty("testProvider").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should use a custom convertUsage", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_a_custom_convertUsage()
    {
        var usage = new LanguageModelUsage(1, 2, 3);
        var calls = 0;
        var model = Model(configure: options => options.ConvertUsage = _ =>
        {
            calls++;
            return usage;
        });

        var result = model.ConvertResponse(CreateResponseBody("{\"role\":\"assistant\",\"content\":\"Hello!\"}"));

        Assert.Equal(1, calls);
        Assert.Same(usage, result.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert content arrays with text and thinking parts", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_convert_content_arrays_with_text_and_thinking_parts()
    {
        var result = Model().ConvertResponse(CreateResponseBody(
            "{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Let me think.\"}]},{\"type\":\"text\",\"text\":\"Answer.\"}]}"));

        Assert.Equal(new[] { "reasoning:Let me think.", "text:Answer." }, result.Content.Select(Describe));
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw InvalidResponseDataError when there are no choices", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_throw_InvalidResponseDataError_when_there_are_no_choices()
    {
        var body = CreateResponseBody("{}", "{\"choices\":[]}");

        var error = Assert.Throws<InvalidResponseDataException>(() => Model().ConvertResponse(body));

        Assert.Equal("Response did not contain any choices.", error.Message);
        Assert.Equal(body.GetRawText(), ((JsonElement)error.Data!).GetRawText());
    }

    [Theory]
    [InlineData("{\"choices\":\"nope\"}")]
    [InlineData("null")]
    [UpstreamTest(Prefix + "should throw TypeValidationError for an invalid body (%s)", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_throw_TypeValidationError_for_an_invalid_body(string body)
    {
        Assert.Throws<TypeValidationException>(() => Model().ConvertResponse(UpstreamChat.Json(body)));
    }
}
