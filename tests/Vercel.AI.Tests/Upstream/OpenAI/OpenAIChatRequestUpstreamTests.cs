// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

/// <summary>Chat Completions tools, generate, and stream request outcomes.</summary>
public sealed class OpenAIChatRequestUpstreamTests
{
    private const string Tools = "packages/openai/src/chat/openai-chat-prepare-tools.test.ts::prepareChatTools::";
    private const string Generate = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate::";
    private const string Format = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate > response format::";
    private const string Reasoning = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate > reasoning models::";
    private const string Stream = "packages/openai/src/chat/openai-chat-language-model.test.ts::doStream::";
    private const string StreamReasoning = "packages/openai/src/chat/openai-chat-language-model.test.ts::doStream > reasoning models::";
    private const string StreamRaw = "packages/openai/src/chat/openai-chat-language-model.test.ts::doStream > raw chunks::";
    private const string ToolInputNote = "The tool call, finish, and usage match. The .NET stream has no tool-input-start, tool-input-delta, or tool-input-end parts.";
    private const string NoCacheNote = "Parts and token counts match. Usage noCache stays null when cache_write_tokens is absent.";
    private const string SparkleHead = "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1711357598,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[{\"index\":0,\"delta\":";
    private const string SparkleUsage = "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1711357598,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[],\"usage\":{\"prompt_tokens\":53,\"completion_tokens\":17,\"total_tokens\":70}}";
    private const string TextStream = "data: {\"id\":\"c\",\"created\":1,\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}\n\ndata: [DONE]\n\n";

    private const string Schema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";

    [Fact]
    [UpstreamTest(Tools + "should return undefined tools and toolChoice when tools are null", Coverage = UpstreamCoverage.Covered)]
    public void OmitsNullTools()
    {
        var prepared = OpenAIChatTools.Prepare(null, null);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolChoice);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "should return undefined tools and toolChoice when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void OmitsEmptyTools()
    {
        var prepared = OpenAIChatTools.Prepare(Array.Empty<LanguageModelTool>(), null);
        Assert.Null(prepared.Tools);
        Assert.Null(prepared.ToolChoice);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void PreparesFunctionTools()
    {
        var prepared = OpenAIChatTools.Prepare(new[] { OpenAIUpstream.Tool("testFunction", "A test function") }, null);
        OpenAIUpstream.Equal(prepared.Tools, "[{\"type\":\"function\",\"function\":{\"name\":\"testFunction\",\"parameters\":{\"type\":\"object\",\"properties\":{}},\"description\":\"A test function\"}}]");
        Assert.Null(prepared.ToolChoice);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Tools + "should remove string propertyNames from function tools and warn", Coverage = UpstreamCoverage.Covered)]
    public void RemovesToolPropertyNames()
    {
        var prepared = OpenAIChatTools.Prepare(new[]
        {
            OpenAIUpstream.Tool("testFunction", null, schema: "{\"type\":\"object\",\"properties\":{\"values\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\",\"pattern\":\"^[A-Z_]+$\"}}}}"),
        }, null);
        OpenAIUpstream.Equal(prepared.Tools, "[{\"type\":\"function\",\"function\":{\"name\":\"testFunction\",\"parameters\":{\"type\":\"object\",\"properties\":{\"values\":{\"type\":\"object\"}}}}}]");
        Assert.Equal("JSON Schema propertyNames", prepared.Warnings[0].Feature);
        Assert.Equal(OpenAIJsonSchema.PropertyNamesDetails, prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Tools + "should remove regex lookaround patterns from function tools and warn", Coverage = UpstreamCoverage.Covered)]
    public void RemovesToolLookaround()
    {
        var prepared = OpenAIChatTools.Prepare(new[]
        {
            OpenAIUpstream.Tool("createContact", null, schema: "{\"type\":\"object\",\"properties\":{\"email\":{\"type\":\"string\",\"format\":\"email\",\"pattern\":\"^(?!\\\\.).+@.+$\"},\"username\":{\"type\":\"string\",\"pattern\":\"^@[a-zA-Z0-9_]+$\"}}}"),
        }, null);
        OpenAIUpstream.Equal(prepared.Tools, "[{\"type\":\"function\",\"function\":{\"name\":\"createContact\",\"parameters\":{\"type\":\"object\",\"properties\":{\"email\":{\"type\":\"string\",\"format\":\"email\"},\"username\":{\"type\":\"string\",\"pattern\":\"^@[a-zA-Z0-9_]+$\"}}}}}]");
        Assert.Equal("JSON Schema pattern with regex lookaround", prepared.Warnings[0].Feature);
    }

    [Fact]
    [UpstreamTest(Tools + "should add warnings for unsupported tools", Coverage = UpstreamCoverage.Covered)]
    public void WarnsForUnsupportedTools()
    {
        var prepared = OpenAIChatTools.Prepare(null, new[] { new OpenAIUnsupportedTool("provider", "unsupported_tool") }, null);
        Assert.Empty(prepared.Tools!);
        Assert.Null(prepared.ToolChoice);
        Assert.Equal("unsupported", prepared.Warnings[0].Type);
        Assert.Equal("tool type: provider", prepared.Warnings[0].Feature);
    }

    [Fact]
    [UpstreamTest(Tools + "should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void MapsAutoToolChoice()
    {
        Assert.Equal("auto", Choice(ToolChoice.Auto)!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Tools + "should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void MapsRequiredToolChoice()
    {
        Assert.Equal("required", Choice(ToolChoice.Required)!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Tools + "should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void MapsNoneToolChoice()
    {
        Assert.Equal("none", Choice(ToolChoice.None)!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Tools + "should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void MapsNamedToolChoice()
    {
        OpenAIUpstream.Equal(Choice(ToolChoice.Tool("testFunction")), "{\"type\":\"function\",\"function\":{\"name\":\"testFunction\"}}");
    }

    [Fact]
    [UpstreamTest(Tools + "should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void PassesStrictTrue()
    {
        Assert.True(Strict(true));
    }

    [Fact]
    [UpstreamTest(Tools + "should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void PassesStrictFalse()
    {
        Assert.False(Strict(false));
    }

    [Fact]
    [UpstreamTest(Tools + "should not include strict mode when strict is undefined", Coverage = UpstreamCoverage.Covered)]
    public void OmitsUnsetStrict()
    {
        var function = Function(OpenAIUpstream.Tool("testFunction", "Test"));
        Assert.Null(function["strict"]);
    }

    [Fact]
    [UpstreamTest(Tools + "should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void PassesMixedStrict()
    {
        var prepared = OpenAIChatTools.Prepare(new[]
        {
            OpenAIUpstream.Tool("strictTool", "Strict", true),
            OpenAIUpstream.Tool("nonStrictTool", "Not strict", false),
            OpenAIUpstream.Tool("defaultTool", "Default"),
        }, null);
        Assert.True(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
        Assert.False(prepared.Tools[1]!["function"]!["strict"]!.GetValue<bool>());
        Assert.Null(prepared.Tools[2]!["function"]!["strict"]);
    }

    [Fact]
    [UpstreamTest(Generate + "should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsText()
    {
        var result = await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Hello, World!\"},\"finish_reason\":\"stop\"}]}");
        Assert.Equal("Hello, World!", result.Text);
    }

    [Fact]
    [UpstreamTest(Generate + "should extract an audio transcript alongside tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsAudioTranscriptAndToolCall()
    {
        var result = await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null,\"audio\":{\"transcript\":\"Fix the login bug\"},\"tool_calls\":[{\"id\":\"call-1\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"{\\\"value\\\":\\\"Spark\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}]}");
        Assert.Equal("Fix the login bug", ((GeneratedText)result.Content[0]).Text);
        var call = (GeneratedToolCall)result.Content[1];
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(Generate + "should reject a response without choices", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsEmptyChoices()
    {
        var exception = await Assert.ThrowsAsync<AiSdkException>(() => GenerateText("{\"choices\":[]}"));
        Assert.Equal("Response did not contain any choices.", exception.Message);
    }

    [Fact]
    [UpstreamTest(Generate + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsUsage()
    {
        var usage = (await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":20,\"total_tokens\":25,\"completion_tokens\":5}}")).Usage;
        Assert.Equal(20, usage.InputTokens);
        Assert.Equal(20, usage.NoCacheInputTokens);
        Assert.Equal(5, usage.OutputTokens);
        Assert.Equal(25, usage.TotalTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(0, usage.ReasoningTokens);
        Assert.Equal(5, usage.TextTokens);
        Assert.Equal("{\"prompt_tokens\":20,\"total_tokens\":25,\"completion_tokens\":5}", usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Generate + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsRequestBody()
    {
        var capture = await Send("gpt-3.5-turbo", OpenAIUpstream.Hello());
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}");
    }

    [Fact]
    [UpstreamTest(Generate + "should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsResponseInformation()
    {
        var result = await GenerateText("{\"id\":\"test-id\",\"created\":123,\"model\":\"test-model\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}");
        Assert.Equal("test-id", result.ResponseId);
        Assert.Equal("test-model", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(123), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest(Generate + "should support partial usage", Coverage = UpstreamCoverage.Covered)]
    public async Task SupportsPartialUsage()
    {
        var usage = (await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":20,\"total_tokens\":20}}")).Usage;
        Assert.Equal(20, usage.InputTokens);
        Assert.Equal(20, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(0, usage.OutputTokens);
        Assert.Equal(0, usage.TextTokens);
        Assert.Equal(0, usage.ReasoningTokens);
        Assert.Equal(20, usage.TotalTokens);
        Assert.Equal("{\"prompt_tokens\":20,\"total_tokens\":20}", usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Generate + "should extract logprobs", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsLogprobs()
    {
        var result = await GenerateText(
            "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"logprobs\":{\"content\":[{\"token\":\"Hi\",\"logprob\":-0.1}]},\"finish_reason\":\"stop\"}]}",
            OpenAIUpstream.Hello(),
            OpenAIUpstream.OpenAIOptionsJson("{\"logprobs\":1}"));
        Assert.Equal("Hi", result.ProviderMetadata!.Value.GetProperty("openai").GetProperty("logprobs")[0].GetProperty("token").GetString());
    }

    [Fact]
    [UpstreamTest(Generate + "should extract finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsFinishReason()
    {
        var result = await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}]}");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Generate + "should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task SupportsUnknownFinishReason()
    {
        var result = await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"eos\"}]}");
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Generate + "should expose the raw response headers", Coverage = UpstreamCoverage.Partial, Note = "The custom header is returned. Content-length follows the scripted body.")]
    public async Task ExposesResponseHeaders()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}]}" };
        capture.ResponseHeaders["test-header"] = "test-value";
        var result = await OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest(Generate + "should pass the model and the messages", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesModelAndMessages()
    {
        var capture = await Send("gpt-3.5-turbo", OpenAIUpstream.Hello());
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}");
    }

    [Fact]
    [UpstreamTest(Generate + "should pass settings", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesSettings()
    {
        var capture = await Send("gpt-3.5-turbo", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"logitBias\":{\"50256\":-100},\"parallelToolCalls\":false,\"user\":\"test-user-id\"}"),
        });
        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal(-100, body["logit_bias"]!["50256"]!.GetValue<int>());
        Assert.False(body["parallel_tool_calls"]!.GetValue<bool>());
        Assert.Equal("test-user-id", body["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "should not set reasoning_effort when reasoning is \"provider-default\"", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsProviderDefaultReasoning()
    {
        var capture = await Send("o3", new LanguageModelCallOptions { Prompt = OpenAIUpstream.Hello().Prompt, Reasoning = "provider-default" });
        Assert.Null(JsonNode.Parse(capture.Body)!["reasoning_effort"]);
    }

    [Fact]
    [UpstreamTest(Generate + "should pass top-level reasoning as reasoning_effort", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesTopLevelReasoning()
    {
        var capture = await Send("o3", new LanguageModelCallOptions { Prompt = OpenAIUpstream.Hello().Prompt, Reasoning = "low" });
        Assert.Equal("low", JsonNode.Parse(capture.Body)!["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "should prefer providerOptions reasoningEffort over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersProviderReasoningEffort()
    {
        var capture = await Send("o3", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Reasoning = "low",
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"reasoningEffort\":\"high\"}"),
        });
        Assert.Equal("high", JsonNode.Parse(capture.Body)!["reasoning_effort"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "should pass textVerbosity setting from provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesTextVerbosity()
    {
        var capture = await Send("gpt-3.5-turbo", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"textVerbosity\":\"low\"}"),
        });
        Assert.Equal("low", JsonNode.Parse(capture.Body)!["verbosity"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Generate + "should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesToolsAndToolChoice()
    {
        var capture = await Send("gpt-3.5-turbo", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Tools = new[] { OpenAIUpstream.Tool("test-tool", null, schema: Schema) },
            ToolChoice = ToolChoice.Tool("test-tool"),
        });
        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal("test-tool", body["tool_choice"]!["function"]!["name"]!.GetValue<string>());
        Assert.Equal("http://json-schema.org/draft-07/schema#", body["tools"]![0]!["function"]!["parameters"]!["$schema"]!.GetValue<string>());
        Assert.Null(body["tools"]![0]!["function"]!["strict"]);
    }

    [Fact]
    [UpstreamTest(Generate + "should pass headers", Coverage = UpstreamCoverage.Partial, Note = "Authorization, organization, project, and custom headers match. User-Agent is ai-sdk/openai/4.0.73 and content-type includes a charset.")]
    public async Task PassesHeaders()
    {
        var capture = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(capture, options =>
        {
            options.Organization = "test-organization";
            options.Project = "test-project";
            options.Headers["Custom-Provider-Header"] = "provider-header-value";
        });
        await provider.ChatModel("gpt-3.5-turbo").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
        }, CancellationToken.None);
        OpenAIUpstream.AssertStandardHeaders(capture);
        Assert.Contains("ai-sdk/openai/4.0.73", OpenAIUpstream.Header(capture, "user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Generate + "should return cached_tokens in prompt_details_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsCachedTokens()
    {
        var usage = (await GenerateText("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":2000,\"completion_tokens\":20,\"total_tokens\":2020,\"prompt_tokens_details\":{\"cached_tokens\":1152,\"cache_write_tokens\":256}}}")).Usage;
        Assert.Equal(2000, usage.InputTokens);
        Assert.Equal(1152, usage.CacheReadTokens);
        Assert.Equal(256, usage.CacheWriteTokens);
        Assert.Equal(592, usage.NoCacheInputTokens);
        Assert.Equal(20, usage.OutputTokens);
        Assert.Equal(20, usage.TextTokens);
        Assert.Equal(0, usage.ReasoningTokens);
        Assert.Equal(256, usage.Raw!.Value.GetProperty("prompt_tokens_details").GetProperty("cache_write_tokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(Generate + "should use developer messages for o1", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesDeveloperMessagesForO1()
    {
        var capture = await Send("o1", Prompt(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello")));
        Assert.Equal("developer", JsonNode.Parse(capture.Body)!["messages"]![0]!["role"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Reasoning + "should allow temperature when top-level reasoning is none on gpt-5.1", Coverage = UpstreamCoverage.Covered)]
    public async Task KeepsTemperatureForGpt51None()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-5.1", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Temperature = 0.5,
            Reasoning = "none",
        });
        Assert.Equal(0.5, prepared.Body["temperature"]!.GetValue<double>());
        Assert.Equal("none", prepared.Body["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "should still clear temperature when top-level reasoning is none on o4-mini", Coverage = UpstreamCoverage.Covered)]
    public async Task ClearsTemperatureForO4MiniNone()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o4-mini", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Temperature = 0.5,
            Reasoning = "none",
        });
        Assert.Null(prepared.Body["temperature"]);
        Assert.Equal("none", prepared.Body["reasoning_effort"]!.GetValue<string>());
        Assert.Equal("temperature is not supported for reasoning models", prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Reasoning + "should convert maxOutputTokens to max_completion_tokens", Coverage = UpstreamCoverage.Covered)]
    public void ConvertsMaxOutputTokens()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o1", new LanguageModelCallOptions { Prompt = OpenAIUpstream.Hello().Prompt, MaxOutputTokens = 64 });
        Assert.Null(prepared.Body["max_tokens"]);
        Assert.Equal(64, prepared.Body["max_completion_tokens"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(Generate + "should remove temperature setting for gpt-4o-search-preview and add warning", Coverage = UpstreamCoverage.Covered)]
    public void RemovesSearchPreviewTemperature()
    {
        AssertSearchPreview("gpt-4o-search-preview");
    }

    [Fact]
    [UpstreamTest(Generate + "should remove temperature setting for gpt-4o-mini-search-preview and add warning", Coverage = UpstreamCoverage.Covered)]
    public void RemovesMiniSearchPreviewTemperature()
    {
        AssertSearchPreview("gpt-4o-mini-search-preview");
    }

    [Fact]
    [UpstreamTest(Generate + "should remove temperature setting for gpt-4o-mini-search-preview-2025-03-11 and add warning", Coverage = UpstreamCoverage.Covered)]
    public void RemovesDatedSearchPreviewTemperature()
    {
        AssertSearchPreview("gpt-4o-mini-search-preview-2025-03-11");
    }

    [Fact]
    [UpstreamTest(Generate + "should show warning when using flex processing with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public void WarnsForUnsupportedFlex()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o", Options("{\"serviceTier\":\"flex\"}"));
        Assert.Null(prepared.Body["service_tier"]);
        Assert.Equal("flex processing is only available for o3, o4-mini, and gpt-5 models", prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Generate + "should send serviceTier ultrafast processing setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsUltrafast()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-5.6-sol", Options("{\"serviceTier\":\"ultrafast\"}"));
        Assert.Equal("ultrafast", prepared.Body["service_tier"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Generate + "should omit legacy prompt cache retention for GPT-6 models", Coverage = UpstreamCoverage.Covered)]
    public void OmitsPromptCacheRetentionForGpt6()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-6-astra", Options("{\"promptCacheRetention\":\"24h\"}"));
        Assert.Null(prepared.Body["prompt_cache_retention"]);
        Assert.Equal("promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead", prepared.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Format + "should not send a response_format when response format is text", Coverage = UpstreamCoverage.Covered)]
    public void OmitsTextResponseFormat()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", OpenAIUpstream.Hello());
        Assert.Null(prepared.Body["response_format"]);
    }

    [Fact]
    [UpstreamTest(Format + "should forward json response format as \"json_object\" without schema", Coverage = UpstreamCoverage.Covered)]
    public void ForwardsJsonObject()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", Options("{\"responseFormat\":{\"type\":\"json\"}}"));
        OpenAIUpstream.Equal(prepared.Body["response_format"], "{\"type\":\"json_object\"}");
    }

    [Fact]
    [UpstreamTest(Format + "should forward json response format as \"json_object\" and include schema", Coverage = UpstreamCoverage.Covered)]
    public void ForwardsJsonSchema()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", SchemaOptions(null, null));
        Assert.Equal("json_schema", prepared.Body["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal("response", prepared.Body["response_format"]!["json_schema"]!["name"]!.GetValue<string>());
        Assert.True(prepared.Body["response_format"]!["json_schema"]!["strict"]!.GetValue<bool>());
        Assert.Equal("http://json-schema.org/draft-07/schema#", prepared.Body["response_format"]!["json_schema"]!["schema"]!["$schema"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Format + "should set name & description with responseFormat json", Coverage = UpstreamCoverage.Covered)]
    public void SetsSchemaNameAndDescription()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", SchemaOptions("test-name", "test description"));
        Assert.Equal("test-name", prepared.Body["response_format"]!["json_schema"]!["name"]!.GetValue<string>());
        Assert.Equal("test description", prepared.Body["response_format"]!["json_schema"]!["description"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Format + "should allow for undefined schema with responseFormat json when structuredOutputs are enabled", Coverage = UpstreamCoverage.Covered)]
    public void UsesJsonObjectWithoutSchema()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", Options("{\"responseFormat\":{\"type\":\"json\",\"name\":\"test-name\",\"description\":\"test description\"}}"));
        OpenAIUpstream.Equal(prepared.Body["response_format"], "{\"type\":\"json_object\"}");
    }

    [Fact]
    [UpstreamTest(Stream + "should stream text after Azure content filter chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamsAfterAzureContentFilter()
    {
        var parts = await StreamChunks(
            "data: {\"choices\":[],\"created\":0,\"id\":\"\",\"model\":\"\"}\n\n"
            + "data: {\"id\":\"chatcmpl-test\",\"created\":1,\"model\":\"gpt-4o\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"role\":\"assistant\"}}]}\n\n"
            + "data: {\"id\":\"chatcmpl-test\",\"created\":1,\"model\":\"gpt-4o\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\"}}]}\n\n"
            + "data: {\"id\":\"chatcmpl-test\",\"created\":1,\"model\":\"gpt-4o\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n"
            + "data: {\"choices\":[{\"finish_reason\":null,\"index\":0}],\"created\":0,\"id\":\"\",\"model\":\"\"}\n\n"
            + "data: [DONE]\n\n");
        var deltas = new List<string>();
        foreach (var part in parts)
        {
            if (part is TextDeltaStreamPart delta)
            {
                deltas.Add(delta.Delta);
                Assert.Equal("0", delta.Id);
            }
        }

        Assert.Equal(new[] { string.Empty, "Hello" }, deltas);
    }

    [Fact]
    [UpstreamTest(Stream + "should stream text deltas", Coverage = UpstreamCoverage.Partial, Note = "Text, metadata, finish reason, and usage match. The finish chunk has no logprobs, so the logprobs provider metadata is not checked.")]
    public async Task StreamsTextDeltas()
    {
        var parts = await StreamChunks(
            "data: {\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\"}}]}\n\n"
            + "data: {\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[{\"index\":1,\"delta\":{\"content\":\"Hello\"}}]}\n\n"
            + "data: {\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[{\"index\":1,\"delta\":{\"content\":\", \"}}]}\n\n"
            + "data: {\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[{\"index\":1,\"delta\":{\"content\":\"World!\"}}]}\n\n"
            + "data: {\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n"
            + "data: {\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[],\"usage\":{\"prompt_tokens\":17,\"total_tokens\":244,\"completion_tokens\":227}}\n\n"
            + "data: [DONE]\n\n");
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP", metadata.Id);
        Assert.Equal("gpt-3.5-turbo-0613", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2023-12-15T16:17:00Z"), metadata.Timestamp);
        Assert.Equal(string.Empty, Assert.IsType<TextDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(parts[4]).Delta);
        Assert.Equal(", ", Assert.IsType<TextDeltaStreamPart>(parts[5]).Delta);
        Assert.Equal("World!", Assert.IsType<TextDeltaStreamPart>(parts[6]).Delta);
        var finish = parts.OfType<FinishStreamPart>().Single();
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(17, finish.Usage.InputTokens);
        Assert.Equal(17, finish.Usage.NoCacheInputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Equal(227, finish.Usage.OutputTokens);
        Assert.Equal(227, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(Stream + "should throw an api error when the first stream chunk is an error", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsEarlyStreamError()
    {
        var exception = await Assert.ThrowsAsync<InternalServerException>(() => StreamChunks("data: {\"error\":{\"message\":\"The server had an error processing your request. Sorry about that! You can retry your request, or contact us through our help center at help.openai.com if you keep seeing this error.\",\"type\":\"server_error\",\"param\":null,\"code\":null}}\n\ndata: [DONE]\n\n"));
        Assert.Equal("The server had an error processing your request. Sorry about that! You can retry your request, or contact us through our help center at help.openai.com if you keep seeing this error.", exception.Message);
        Assert.Equal(500, exception.StatusCode);
        Assert.True(ProviderHttp.IsRetryable(exception.StatusCode));
    }

    [Fact]
    [UpstreamTest(Stream + "should preserve numeric status codes from early stream errors", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesNumericStreamStatus()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => StreamChunks("data: {\"error\":{\"message\":\"bad request\",\"type\":\"provider_error\",\"param\":null,\"code\":400}}\n\ndata: [DONE]\n\n"));
        Assert.Equal("bad request", exception.Message);
        Assert.Equal(400, exception.StatusCode);
        Assert.False(ProviderHttp.IsRetryable(exception.StatusCode));
    }

    [Fact]
    [UpstreamTest(Stream + "should forward error stream parts after output has started", Coverage = UpstreamCoverage.Partial, Note = "The error part carries the message string. Upstream wraps a structured error object.")]
    public async Task ForwardsErrorAfterOutput()
    {
        var parts = await StreamChunks(
            "data: {\"id\":\"chatcmpl-error-after-output\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\"}}]}\n\n"
            + "data: {\"error\":{\"message\":\"stream failed after output\",\"type\":\"server_error\",\"param\":null,\"code\":null}}\n\n"
            + "data: [DONE]\n\n");
        Assert.Equal("stream failed after output", parts.OfType<ErrorStreamPart>().Single().Message);
        Assert.Equal(FinishReason.Error, parts.OfType<FinishStreamPart>().Single().FinishReason);
    }

    [Fact]
    [UpstreamTest(Stream + "should handle unparsable stream parts", Coverage = UpstreamCoverage.Partial, Note = "The error message is JSON parsing failed: Text: {data}. Upstream also appends a SyntaxError.")]
    public async Task HandlesUnparsableStreamParts()
    {
        var parts = await StreamChunks("data: {\"id\":\"chatcmpl\",\"created\":1,\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"Hi\"}}]}\n\ndata: not-json\n\ndata: [DONE]\n\n");
        Assert.Equal("JSON parsing failed: Text: not-json.", parts.OfType<ErrorStreamPart>().Single().Message);
    }

    [Fact]
    [UpstreamTest(Stream + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsStreamRequestBody()
    {
        var capture = new OpenAICapture { ServerSentEvents = "data: {\"id\":\"c\",\"created\":1,\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}\n\ndata: [DONE]\n\n" };
        await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(body["stream"]!.GetValue<bool>());
        Assert.True(body["stream_options"]!["include_usage"]!.GetValue<bool>());
        Assert.Equal("gpt-3.5-turbo", body["model"]!.GetValue<string>());
    }

    private static void AssertSearchPreview(string modelId)
    {
        var prepared = OpenAIChatLanguageModel.Prepare(modelId, new LanguageModelCallOptions { Prompt = OpenAIUpstream.Hello().Prompt, Temperature = 0.5 });
        Assert.Null(prepared.Body["temperature"]);
        Assert.Equal("temperature is not supported for the search preview models and has been removed.", prepared.Warnings[0].Details);
    }

    private static LanguageModelCallOptions Options(string openai)
    {
        return new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(openai),
        };
    }

    [Fact]
    [UpstreamTest(Stream + "should stream annotations/citations", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamsAnnotationsAsSources()
    {
        var parts = await StreamChunks(UpstreamChat.Sse(
            "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[{\"index\":1,\"delta\":{\"content\":\"Based on search results\"},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[{\"index\":1,\"delta\":{\"annotations\":[{\"type\":\"url_citation\",\"url_citation\":{\"start_index\":24,\"end_index\":29,\"url\":\"https://example.com/doc1.pdf\",\"title\":\"Document 1\"}}]},\"finish_reason\":null}]}",
            "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}",
            "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0125\",\"choices\":[],\"usage\":{\"prompt_tokens\":17,\"completion_tokens\":227,\"total_tokens\":244}}"));
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        Assert.Equal("chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP", Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Id);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        Assert.Equal(string.Empty, Assert.IsType<TextDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("Based on search results", Assert.IsType<TextDeltaStreamPart>(parts[4]).Delta);
        var source = Assert.IsType<SourceStreamPart>(parts[5]);
        Assert.False(string.IsNullOrEmpty(source.Id));
        Assert.Equal("https://example.com/doc1.pdf", source.Url);
        Assert.Equal("Document 1", source.Title);
        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(parts[6]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[7]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Stream + "should stream tool deltas", Coverage = UpstreamCoverage.Partial, Note = ToolInputNote)]
    public async Task StreamsToolDeltas()
    {
        var parts = await StreamTool("test-tool", SparkleStream(string.Empty, "{\"", "value", "\":\"", "Spark", "le", " Day", "\"}"));
        AssertSparkleToolCall(parts);
    }

    [Fact]
    [UpstreamTest(Stream + "should stream tool call deltas when tool call arguments are passed in the first chunk", Coverage = UpstreamCoverage.Partial, Note = ToolInputNote)]
    public async Task StreamsToolDeltasWithArgumentsInTheFirstChunk()
    {
        var parts = await StreamTool("test-tool", SparkleStream("{\"", "va", "lue", "\":\"", "Spark", "le", " Day", "\"}"));
        AssertSparkleToolCall(parts);
    }

    [Fact]
    [UpstreamTest(Stream + "should not duplicate tool calls when there is an additional empty chunk after the tool call has been completed", Coverage = UpstreamCoverage.Partial, Note = ToolInputNote)]
    public async Task EmptyArgumentsAfterACompletedToolCallDoNotDuplicateIt()
    {
        const string Head = "{\"id\":\"chat-2267f7e2910a4254bac0650ba74cfc1c\",\"created\":1733162241,\"model\":\"meta/llama-3.1-8b-instruct:fp8\",\"choices\":[{\"index\":0,\"delta\":";
        var parts = await StreamTool("searchGoogle", UpstreamChat.Sse(
            Head + "{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":226,\"completion_tokens\":0}}",
            Head + "{\"tool_calls\":[{\"id\":\"chatcmpl-tool-b3b307239370432d9910d4b79b4dbbaa\",\"type\":\"function\",\"index\":0,\"function\":{\"name\":\"searchGoogle\"}}]},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":233,\"completion_tokens\":7}}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"{\\\"query\\\": \\\"\"}}]},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":241,\"completion_tokens\":15}}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"latest\"}}]},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":242,\"completion_tokens\":16}}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\" news\"}}]},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":243,\"completion_tokens\":17}}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\" on\"}}]},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":244,\"completion_tokens\":18}}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\" ai\\\"}\"}}]},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":245,\"completion_tokens\":19}}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"\"}}]},\"finish_reason\":\"tool_calls\",\"stop_reason\":128008}],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":246,\"completion_tokens\":20}}",
            "{\"id\":\"chat-2267f7e2910a4254bac0650ba74cfc1c\",\"created\":1733162241,\"model\":\"meta/llama-3.1-8b-instruct:fp8\",\"choices\":[],\"usage\":{\"prompt_tokens\":226,\"total_tokens\":246,\"completion_tokens\":20}}"));
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("meta/llama-3.1-8b-instruct:fp8", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2024-12-02T17:57:21Z"), metadata.Timestamp);
        Assert.Equal(string.Empty, Assert.IsType<TextDeltaStreamPart>(parts[3]).Delta);
        Assert.IsType<TextEndStreamPart>(parts[4]);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("chatcmpl-tool-b3b307239370432d9910d4b79b4dbbaa", call.ToolCallId);
        Assert.Equal("searchGoogle", call.ToolName);
        Assert.Equal("{\"query\": \"latest news on ai\"}", call.ArgumentsJson);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal(226, finish.Usage.InputTokens);
        Assert.Equal(20, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Stream + "should not finalize tool call early when partial JSON is coincidentally parsable", Coverage = UpstreamCoverage.Covered)]
    public async Task ParsableArgumentsMidStreamDoNotFinishTheToolCall()
    {
        const string Head = "{\"id\":\"chatcmpl-early\",\"created\":1733162241,\"model\":\"gpt-4\",\"choices\":[{\"index\":0,\"delta\":";
        var parts = await StreamTool("search", UpstreamChat.Sse(
            Head + "{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"index\":0,\"id\":\"call_early123\",\"type\":\"function\",\"function\":{\"name\":\"search\",\"arguments\":\"\"}}]},\"finish_reason\":null}]}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"{\\\"query\\\": \\\"test\\\"}\"}}]},\"finish_reason\":null}]}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"\"}}]},\"finish_reason\":null}]}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\", \\\"limit\\\": 10}\"}}]},\"finish_reason\":null}]}",
            Head + "{},\"finish_reason\":\"tool_calls\"}]}"));
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_early123", call.ToolCallId);
        Assert.Equal("search", call.ToolName);
        Assert.Equal("{\"query\": \"test\"}, \"limit\": 10}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(Stream + "should stream tool call with missing type field (Azure AI Foundry / Mistral)", Coverage = UpstreamCoverage.Partial, Note = ToolInputNote)]
    public async Task StreamsToolCallsWithoutATypeField()
    {
        const string Head = "{\"id\":\"chatcmpl-azure-001\",\"created\":1711357598,\"model\":\"mistral-large\",\"choices\":[{\"index\":0,\"delta\":";
        var parts = await StreamTool("test-tool", UpstreamChat.Sse(
            Head + "{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"index\":0,\"id\":\"call_abc123\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"\"}}]},\"finish_reason\":null}]}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"{\\\"value\\\"\"}}]},\"finish_reason\":null}]}",
            Head + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\":\\\"hello\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5,\"total_tokens\":15}}"));
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_abc123", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"hello\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(Stream + "should stream tool call that is sent in one chunk", Coverage = UpstreamCoverage.Partial, Note = ToolInputNote)]
    public async Task StreamsAToolCallSentInOneChunk()
    {
        var parts = await StreamTool("test-tool", UpstreamChat.Sse(
            SparkleHead + "{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"index\":0,\"id\":\"call_O17Uplv4lJvD6DVdIvFFeRMw\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"{\\\"value\\\":\\\"Sparkle Day\\\"}\"}}]},\"logprobs\":null,\"finish_reason\":null}]}",
            SparkleHead + "{},\"logprobs\":null,\"finish_reason\":\"tool_calls\"}]}",
            SparkleUsage));
        AssertSparkleToolCall(parts);
    }

    [Fact]
    [UpstreamTest(Stream + "should pass the messages and the model", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsMessagesAndModel()
    {
        var capture = new OpenAICapture { ServerSentEvents = TextStream };
        await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
    }

    [Fact]
    [UpstreamTest(Stream + "should pass headers", Coverage = UpstreamCoverage.Partial, Note = "Authorization, organization, project, and custom headers match. Content-type includes a charset.")]
    public async Task StreamPassesHeaders()
    {
        var capture = new OpenAICapture { ServerSentEvents = TextStream };
        var provider = OpenAIUpstream.Provider(capture, options =>
        {
            options.Organization = "test-organization";
            options.Project = "test-project";
            options.Headers["Custom-Provider-Header"] = "provider-header-value";
        });
        var call = OpenAIUpstream.Hello();
        call.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await OpenAIUpstream.Read(provider.ChatModel("gpt-3.5-turbo").DoStreamAsync(call, CancellationToken.None));
        Assert.Equal("Bearer test-api-key", OpenAIUpstream.Header(capture, "authorization"));
        Assert.StartsWith("application/json", OpenAIUpstream.Header(capture, "content-type"), StringComparison.Ordinal);
        Assert.Equal("provider-header-value", OpenAIUpstream.Header(capture, "custom-provider-header"));
        Assert.Equal("request-header-value", OpenAIUpstream.Header(capture, "custom-request-header"));
        Assert.Equal("test-organization", OpenAIUpstream.Header(capture, "openai-organization"));
        Assert.Equal("test-project", OpenAIUpstream.Header(capture, "openai-project"));
    }

    [Fact]
    [UpstreamTest(Stream + "should return cached tokens in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamReturnsCachedTokens()
    {
        var capture = new OpenAICapture { ServerSentEvents = UsageStream("{\"prompt_tokens\":2000,\"completion_tokens\":20,\"total_tokens\":2020,\"prompt_tokens_details\":{\"cached_tokens\":1152,\"cache_write_tokens\":256}}") };
        var parts = await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal("{\"openai\":{}}", finish.ProviderMetadata!.Value.GetRawText());
        Assert.Equal(2000, finish.Usage.InputTokens);
        Assert.Equal(1152, finish.Usage.CacheReadTokens);
        Assert.Equal(256, finish.Usage.CacheWriteTokens);
        Assert.Equal(592, finish.Usage.NoCacheInputTokens);
        Assert.Equal(20, finish.Usage.OutputTokens);
        Assert.Equal(20, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
        OpenAIUpstream.Equal(JsonNode.Parse(finish.Usage.Raw!.Value.GetRawText()), "{\"prompt_tokens\":2000,\"completion_tokens\":20,\"total_tokens\":2020,\"prompt_tokens_details\":{\"cached_tokens\":1152,\"cache_write_tokens\":256}}");
    }

    [Fact]
    [UpstreamTest(Stream + "should return accepted_prediction_tokens and rejected_prediction_tokens in providerMetadata", Coverage = UpstreamCoverage.Partial, Note = NoCacheNote)]
    public async Task StreamReturnsPredictionTokens()
    {
        var parts = await StreamChunks(UsageStream("{\"prompt_tokens\":15,\"completion_tokens\":20,\"total_tokens\":35,\"completion_tokens_details\":{\"accepted_prediction_tokens\":123,\"rejected_prediction_tokens\":456}}"));
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal("{\"openai\":{\"acceptedPredictionTokens\":123,\"rejectedPredictionTokens\":456}}", finish.ProviderMetadata!.Value.GetRawText());
        Assert.Equal(15, finish.Usage.InputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Equal(20, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(Stream + "should send store extension setting", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsStore()
    {
        var capture = await StreamWithOptions("gpt-3.5-turbo", "{\"store\":true}");
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"store\":true,\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
    }

    [Fact]
    [UpstreamTest(Stream + "should send metadata extension values", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsMetadata()
    {
        var capture = await StreamWithOptions("gpt-3.5-turbo", "{\"metadata\":{\"custom\":\"value\"}}");
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"metadata\":{\"custom\":\"value\"},\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
    }

    [Fact]
    [UpstreamTest(Stream + "should send serviceTier flex processing setting in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsFlexServiceTier()
    {
        var capture = await StreamWithOptions("o4-mini", "{\"serviceTier\":\"flex\"}");
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"o4-mini\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"service_tier\":\"flex\",\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
    }

    [Fact]
    [UpstreamTest(Stream + "should send serviceTier priority processing setting in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsPriorityServiceTier()
    {
        var capture = await StreamWithOptions("gpt-4o-mini", "{\"serviceTier\":\"priority\"}");
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-4o-mini\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"service_tier\":\"priority\",\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
    }

    [Fact]
    [UpstreamTest(Stream + "should set .modelId for model-router request", Coverage = UpstreamCoverage.Partial, Note = NoCacheNote)]
    public async Task StreamReportsTheRoutedModelId()
    {
        var chunks = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "azure-model-router.1.chunks.txt")).Where(line => line.Trim().Length > 0).ToArray();
        var capture = new OpenAICapture { ServerSentEvents = UpstreamChat.Sse(chunks) };
        var parts = await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("test-azure-model-router").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-CYPS1lijGoK8gd9lYzY3r9Sx50nbt", metadata.Id);
        Assert.Equal("gpt-5-nano-2025-08-07", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2025-11-05T04:30:21Z"), metadata.Timestamp);
        Assert.IsType<TextStartStreamPart>(parts[2]);
        Assert.Equal(new[] { string.Empty, "Capital", " of", " Denmark", "." }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.IsType<TextEndStreamPart>(parts[^2]);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("{\"openai\":{\"acceptedPredictionTokens\":0,\"rejectedPredictionTokens\":0}}", finish.ProviderMetadata!.Value.GetRawText());
        Assert.Equal(15, finish.Usage.InputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Equal(78, finish.Usage.OutputTokens);
        Assert.Equal(64, finish.Usage.ReasoningTokens);
        Assert.Equal(14, finish.Usage.TextTokens);
    }

    [Fact]
    [UpstreamTest(StreamReasoning + "should stream text delta", Coverage = UpstreamCoverage.Partial, Note = NoCacheNote)]
    public async Task ReasoningModelsStreamTextDeltas()
    {
        var parts = await StreamReasoningModel("{\"prompt_tokens\":17,\"total_tokens\":244,\"completion_tokens\":227}");
        AssertReasoningText(parts);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(227, finish.Usage.OutputTokens);
        Assert.Equal(227, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(StreamReasoning + "should send reasoning tokens", Coverage = UpstreamCoverage.Partial, Note = NoCacheNote)]
    public async Task ReasoningModelsStreamReasoningTokens()
    {
        var parts = await StreamReasoningModel("{\"prompt_tokens\":15,\"completion_tokens\":20,\"total_tokens\":35,\"completion_tokens_details\":{\"reasoning_tokens\":10}}");
        AssertReasoningText(parts);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(20, finish.Usage.OutputTokens);
        Assert.Equal(10, finish.Usage.TextTokens);
        Assert.Equal(10, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(StreamRaw + "should include raw chunks when includeRawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamIncludesRawChunksWhenAsked()
    {
        var chunks = RawChunks();
        var capture = new OpenAICapture { ServerSentEvents = UpstreamChat.Sse(chunks) };
        var call = OpenAIUpstream.Hello();
        call.IncludeRawChunks = true;
        var parts = await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoStreamAsync(call, CancellationToken.None));
        Assert.Equal(chunks, parts.OfType<RawStreamPart>().Select(part => part.RawJson));
    }

    [Fact]
    [UpstreamTest(StreamRaw + "should not include raw chunks when includeRawChunks is false", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamOmitsRawChunksByDefault()
    {
        var parts = await StreamChunks(UpstreamChat.Sse(RawChunks()));
        Assert.Empty(parts.OfType<RawStreamPart>());
    }

    private static async Task<List<LanguageModelStreamPart>> StreamTool(string toolName, string events)
    {
        var capture = new OpenAICapture { ServerSentEvents = events };
        var call = OpenAIUpstream.Hello();
        call.Tools = new[] { OpenAIUpstream.Tool(toolName, null, schema: Schema) };
        return await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoStreamAsync(call, CancellationToken.None)).ConfigureAwait(false);
    }

    private static string SparkleStream(params string[] argumentDeltas)
    {
        var events = new List<string>
        {
            SparkleHead + "{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"index\":0,\"id\":\"call_O17Uplv4lJvD6DVdIvFFeRMw\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":" + JsonSerializer.Serialize(argumentDeltas[0]) + "}}]},\"logprobs\":null,\"finish_reason\":null}]}",
        };
        foreach (var delta in argumentDeltas.Skip(1))
        {
            events.Add(SparkleHead + "{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":" + JsonSerializer.Serialize(delta) + "}}]},\"logprobs\":null,\"finish_reason\":null}]}");
        }

        events.Add(SparkleHead + "{},\"logprobs\":null,\"finish_reason\":\"tool_calls\"}]}");
        events.Add(SparkleUsage);
        return UpstreamChat.Sse(events.ToArray());
    }

    private static void AssertSparkleToolCall(List<LanguageModelStreamPart> parts)
    {
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP", metadata.Id);
        Assert.Equal("gpt-3.5-turbo-0125", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2024-03-25T09:06:38Z"), metadata.Timestamp);
        var call = Assert.IsType<ToolCallStreamPart>(parts[2]);
        Assert.Equal("call_O17Uplv4lJvD6DVdIvFFeRMw", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Sparkle Day\"}", call.ArgumentsJson);
        var finish = Assert.IsType<FinishStreamPart>(parts[3]);
        Assert.Equal(4, parts.Count);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("tool_calls", finish.RawFinishReason);
        Assert.Equal("{\"openai\":{}}", finish.ProviderMetadata!.Value.GetRawText());
        Assert.Equal(53, finish.Usage.InputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Equal(17, finish.Usage.OutputTokens);
        Assert.Equal(17, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
    }

    private static string UsageStream(string usage)
    {
        const string Head = "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",";
        return UpstreamChat.Sse(
            Head + "\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":null}]}",
            Head + "\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\",\"logprobs\":null}]}",
            Head + "\"choices\":[],\"usage\":" + usage + "}");
    }

    private static async Task<OpenAICapture> StreamWithOptions(string modelId, string openai)
    {
        var capture = new OpenAICapture { ServerSentEvents = TextStream };
        var call = OpenAIUpstream.Hello();
        call.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(openai);
        await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel(modelId).DoStreamAsync(call, CancellationToken.None)).ConfigureAwait(false);
        return capture;
    }

    private static async Task<List<LanguageModelStreamPart>> StreamReasoningModel(string usage)
    {
        const string Head = "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"created\":1702657020,\"model\":\"o4-mini\",";
        var capture = new OpenAICapture
        {
            ServerSentEvents = UpstreamChat.Sse(
                Head + "\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":null}]}",
                Head + "\"choices\":[{\"index\":1,\"delta\":{\"content\":\"Hello, World!\"},\"finish_reason\":null}]}",
                Head + "\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\",\"logprobs\":null}]}",
                Head + "\"choices\":[],\"usage\":" + usage + "}"),
        };
        return await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("o4-mini").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None)).ConfigureAwait(false);
    }

    private static void AssertReasoningText(List<LanguageModelStreamPart> parts)
    {
        Assert.Equal(7, parts.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP", metadata.Id);
        Assert.Equal("o4-mini", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2023-12-15T16:17:00Z"), metadata.Timestamp);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        Assert.Equal(string.Empty, Assert.IsType<TextDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("Hello, World!", Assert.IsType<TextDeltaStreamPart>(parts[4]).Delta);
        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(parts[5]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[6]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal("{\"openai\":{}}", finish.ProviderMetadata!.Value.GetRawText());
    }

    private static string[] RawChunks()
    {
        const string Head = "{\"id\":\"chatcmpl-96aZqmeDpA9IPD6tACY8djkMsJCMP\",\"object\":\"chat.completion.chunk\",\"created\":1702657020,\"model\":\"gpt-3.5-turbo-0613\",";
        return new[]
        {
            Head + "\"system_fingerprint\":null,\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":null}]}",
            Head + "\"system_fingerprint\":null,\"choices\":[{\"index\":1,\"delta\":{\"content\":\"Hello\"},\"finish_reason\":null}]}",
            Head + "\"system_fingerprint\":null,\"choices\":[{\"index\":1,\"delta\":{\"content\":\" World!\"},\"finish_reason\":null}]}",
            Head + "\"system_fingerprint\":null,\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\",\"logprobs\":null}]}",
            Head + "\"system_fingerprint\":\"fp_3bc1b5746c\",\"choices\":[],\"usage\":{\"prompt_tokens\":17,\"total_tokens\":244,\"completion_tokens\":227}}",
        };
    }

    private static LanguageModelCallOptions SchemaOptions(string? name, string? description)
    {
        var format = name == null
            ? "{\"responseFormat\":{\"type\":\"json\"}}"
            : "{\"responseFormat\":{\"type\":\"json\",\"name\":\"" + name + "\",\"description\":\"" + description + "\"}}";
        return new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            JsonSchema = OpenAIUpstream.Json(Schema),
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(format),
        };
    }

    private static LanguageModelCallOptions Prompt(params ModelMessage[] messages)
    {
        return new LanguageModelCallOptions { Prompt = messages };
    }

    private static JsonNode? Choice(ToolChoice choice)
    {
        return OpenAIChatTools.Prepare(new[] { OpenAIUpstream.Tool("testFunction", "Test") }, choice).ToolChoice;
    }

    private static bool Strict(bool value)
    {
        return Function(OpenAIUpstream.Tool("testFunction", "Test", value))["strict"]!.GetValue<bool>();
    }

    private static JsonObject Function(LanguageModelTool tool)
    {
        return (JsonObject)OpenAIChatTools.Prepare(new[] { tool }, null).Tools![0]!["function"]!;
    }

    private static async Task<OpenAICapture> Send(string modelId, LanguageModelCallOptions options)
    {
        var capture = new OpenAICapture();
        await OpenAIUpstream.Provider(capture).ChatModel(modelId).DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        return capture;
    }

    private static async Task<LanguageModelGenerateResult> GenerateText(string json, LanguageModelCallOptions? options = null, IReadOnlyDictionary<string, System.Text.Json.JsonElement>? providerOptions = null)
    {
        var capture = new OpenAICapture { ResponseJson = json, Status = HttpStatusCode.OK };
        var call = options ?? OpenAIUpstream.Hello();
        if (providerOptions != null)
        {
            call.ProviderOptions = providerOptions;
        }

        return await OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
    }

    private static async Task<List<LanguageModelStreamPart>> StreamChunks(string events)
    {
        var capture = new OpenAICapture { ServerSentEvents = events };
        return await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).ChatModel("gpt-3.5-turbo").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None)).ConfigureAwait(false);
    }
}
