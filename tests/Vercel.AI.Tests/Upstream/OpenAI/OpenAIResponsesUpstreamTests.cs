// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Responses API request bodies, tool strictness, and parsed results.</summary>
public sealed class OpenAIResponsesUpstreamTests
{
    private const string Generate = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::";
    private const string Tools = "packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > function tools strict mode::";
    private const string ToolCalls = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::";

    [UpstreamTest(Tools + "should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void PassesStrictTrue()
    {
        Assert.True(Strict(true));
    }

    [UpstreamTest(Tools + "should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void PassesStrictFalse()
    {
        Assert.False(Strict(false));
    }

    [UpstreamTest(Tools + "should not include strict mode when strict is undefined", Coverage = UpstreamCoverage.Partial, Note = "The port sends strict false when a tool leaves strict unset.")]
    public void SendsFalseWhenStrictIsUnset()
    {
        var tool = Prepared(null);
        Assert.False(tool["strict"]!.GetValue<bool>());
        Assert.Equal(5, tool.Count);
    }

    [UpstreamTest(Tools + "should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void PassesMixedStrict()
    {
        var prepared = OpenAIResponsesLanguageModel.PrepareResponsesTools(new[]
        {
            OpenAIUpstream.Tool("strictTool", "Strict", true),
            OpenAIUpstream.Tool("nonStrictTool", "Not strict", false),
            OpenAIUpstream.Tool("defaultTool", "Default"),
        }, null);
        Assert.True(prepared.Tools![0]!["strict"]!.GetValue<bool>());
        Assert.False(prepared.Tools[1]!["strict"]!.GetValue<bool>());
        Assert.False(prepared.Tools[2]!["strict"]!.GetValue<bool>());
    }

    [UpstreamTest(Generate + "should remove unsupported settings for o1", Coverage = UpstreamCoverage.Covered)]
    public async Task RemovesUnsupportedO1Settings()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIUpstream.ResponsesEmpty };
        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("o1").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
            Temperature = 0.5,
            TopP = 0.3,
        }, CancellationToken.None);
        OpenAIUpstream.Equal(
            JsonNode.Parse(capture.Body),
            "{\"model\":\"o1\",\"input\":[{\"role\":\"developer\",\"content\":\"You are a helpful assistant.\"},{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}]}");
        Assert.Equal("temperature is not supported for reasoning models", result.Warnings[0].Message);
        Assert.Equal("topP is not supported for reasoning models", result.Warnings[1].Message);
        Assert.DoesNotContain("stream", capture.Body, StringComparison.Ordinal);
    }

    [UpstreamTest(Generate + "should keep temperature and topP for gpt-5.1 models when reasoning effort is none", Coverage = UpstreamCoverage.Covered)]
    public void KeepsSamplingForGpt51None()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-5.1", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Temperature = 0.5,
            TopP = 0.3,
            Reasoning = "none",
        }, false);
        Assert.Equal(0.5, prepared.Body["temperature"]!.GetValue<double>());
        Assert.Equal(0.3, prepared.Body["top_p"]!.GetValue<double>());
        Assert.Equal("none", prepared.Body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
        Assert.Null(prepared.Body["stream"]);
    }

    [UpstreamTest(Generate + "should send model id, settings, and input", Coverage = UpstreamCoverage.Covered)]
    public void SendsModelSettingsAndInput()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-4o", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
            Temperature = 0.5,
            TopP = 0.3,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"maxToolCalls\":10}"),
        }, false);
        OpenAIUpstream.Equal(
            prepared.Body,
            "{\"model\":\"gpt-4o\",\"input\":[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\"},{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}],\"temperature\":0.5,\"top_p\":0.3,\"max_tool_calls\":10}");
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should warn about GPT-5.6 reasoning controls on non-reasoning models", Coverage = UpstreamCoverage.Partial, Note = "reasoningEffort on a non-reasoning model warns and is omitted. The port warning text is reasoningEffort is not supported for non-reasoning models.")]
    public void WarnsForReasoningOnNonReasoningModels()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-4o", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"reasoningEffort\":\"medium\"}"),
        }, false);
        Assert.Null(prepared.Body["reasoning"]);
        Assert.Equal("reasoningEffort is not supported for non-reasoning models", prepared.Warnings[0].Details);
    }

    [UpstreamTest("packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate::should throw a descriptive error when the response has no output", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsWhenOutputIsMissing()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"resp_test\",\"error\":null,\"incomplete_details\":{\"reason\":\"content_filter\"}}" };
        var exception = await Assert.ThrowsAsync<InternalServerException>(() =>
            OpenAIUpstream.Provider(capture).ResponsesModel("gpt-4o").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None));
        Assert.Equal("Responses API returned no output (content_filter)", exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }

    [UpstreamTest(Generate + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsUsage()
    {
        var usage = (await Generate("{\"id\":\"resp_test\",\"output\":[],\"usage\":{\"future_usage_field\":{\"value\":true},\"input_tokens\":345,\"input_tokens_details\":{\"cache_write_tokens\":45,\"cached_tokens\":234,\"future_input_detail\":{\"tokens\":7}},\"output_tokens\":538,\"output_tokens_details\":{\"future_output_detail\":[\"preserved\"],\"reasoning_tokens\":123},\"total_tokens\":572}}")).Usage;
        Assert.Equal(345, usage.InputTokens);
        Assert.Equal(234, usage.CacheReadTokens);
        Assert.Equal(45, usage.CacheWriteTokens);
        Assert.Equal(66, usage.NoCacheInputTokens);
        Assert.Equal(538, usage.OutputTokens);
        Assert.Equal(123, usage.ReasoningTokens);
        Assert.Equal(415, usage.TextTokens);
        Assert.Equal(572, usage.TotalTokens);
        Assert.Equal(7, usage.Raw!.Value.GetProperty("input_tokens_details").GetProperty("future_input_detail").GetProperty("tokens").GetInt32());
        Assert.Equal("preserved", usage.Raw.Value.GetProperty("output_tokens_details").GetProperty("future_output_detail")[0].GetString());
    }

    [UpstreamTest(Generate + "should preserve orchestration usage fields in raw", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesOrchestrationUsage()
    {
        var usage = (await Generate("{\"id\":\"resp_test\",\"output\":[],\"usage\":{\"input_tokens\":120,\"input_tokens_details\":{\"cached_tokens\":0,\"orchestration_input_tokens\":40,\"orchestration_input_cached_tokens\":10},\"output_tokens\":80,\"output_tokens_details\":{\"reasoning_tokens\":0,\"orchestration_output_tokens\":25},\"total_tokens\":265}}")).Usage;
        Assert.Equal(40, usage.Raw!.Value.GetProperty("input_tokens_details").GetProperty("orchestration_input_tokens").GetInt32());
        Assert.Equal(10, usage.Raw.Value.GetProperty("input_tokens_details").GetProperty("orchestration_input_cached_tokens").GetInt32());
        Assert.Equal(25, usage.Raw.Value.GetProperty("output_tokens_details").GetProperty("orchestration_output_tokens").GetInt32());
    }

    [UpstreamTest(Generate + "should generate text", Coverage = UpstreamCoverage.Partial, Note = "The text is answer text. Generated text does not carry the Responses item id.")]
    public async Task GeneratesText()
    {
        var result = await Generate("{\"id\":\"resp_67c97c0203188190a025beb4a75242bc\",\"created_at\":1741257730,\"model\":\"gpt-4o\",\"output\":[{\"id\":\"msg_1\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"answer text\"}]}]}");
        Assert.Equal("answer text", result.Text);
        Assert.Equal("resp_67c97c0203188190a025beb4a75242bc", result.ResponseId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1741257730), result.ResponseTimestamp);
    }

    [UpstreamTest(ToolCalls + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesToolCallsFinishReason()
    {
        var result = await Generate("{\"id\":\"resp_tool\",\"incomplete_details\":null,\"output\":[{\"type\":\"function_call\",\"id\":\"fc_1\",\"call_id\":\"call_1\",\"name\":\"test-tool\",\"arguments\":\"{\\\"value\\\":\\\"Spark\\\"}\"}]}");
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Null(result.RawFinishReason);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
        Assert.Equal("fc_1", call.ProviderMetadata!.Value.GetProperty("openai").GetProperty("itemId").GetString());
    }

    private static bool Strict(bool value)
    {
        return Prepared(value)["strict"]!.GetValue<bool>();
    }

    private static JsonObject Prepared(bool? strict)
    {
        var prepared = OpenAIResponsesLanguageModel.PrepareResponsesTools(new[] { OpenAIUpstream.Tool("testFunction", "A test function", strict) }, null);
        return (JsonObject)prepared.Tools![0]!;
    }

    private static async Task<LanguageModelGenerateResult> Generate(string json)
    {
        var capture = new OpenAICapture { ResponseJson = json };
        return await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-4o").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
    }
}
