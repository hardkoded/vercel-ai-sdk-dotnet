// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Chat Completions settings, reasoning limits, and parsed generate content.</summary>
public sealed class OpenAIChatSettingsUpstreamTests
{
    private const string Generate = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate::";
    private const string Format = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate > response format::";
    private const string Reasoning = "packages/openai/src/chat/openai-chat-language-model.test.ts::doGenerate > reasoning models::";
    private const string Usage = "packages/openai/src/chat/convert-openai-chat-usage.test.ts::convertOpenAIChatUsage::";
    private const string ToolSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";

    [UpstreamTest(Usage + "clamps text tokens at 0 when reasoning exceeds completion", Coverage = UpstreamCoverage.Partial, Note = "Text tokens clamp to 0 and reasoning is 6001. cacheWrite is absent, so noCache stays null instead of 891.")]
    public void ClampsTextTokens()
    {
        var usage = OpenAIJson.ChatUsage(OpenAIUpstream.Json("{\"prompt_tokens\":951,\"completion_tokens\":6000,\"total_tokens\":6952,\"prompt_tokens_details\":{\"cached_tokens\":60},\"completion_tokens_details\":{\"reasoning_tokens\":6001}}"));
        Assert.Equal(951, usage.InputTokens);
        Assert.Equal(60, usage.CacheReadTokens);
        Assert.Equal(6000, usage.OutputTokens);
        Assert.Equal(6001, usage.ReasoningTokens);
        Assert.Equal(0, usage.TextTokens);
        Assert.Equal(6952, usage.Raw!.Value.GetProperty("total_tokens").GetInt32());
    }

    [UpstreamTest(Generate + "should pass reasoningEffort setting from provider metadata", Coverage = UpstreamCoverage.Covered)]
    public void PassesReasoningEffortFromProviderOptions()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o4-mini", Call("{\"reasoningEffort\":\"low\"}"));
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"o4-mini\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"reasoning_effort\":\"low\"}");
    }

    [UpstreamTest(Generate + "should pass reasoningEffort setting from settings", Coverage = UpstreamCoverage.Covered)]
    public void PassesReasoningEffortHigh()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o4-mini", Call("{\"reasoningEffort\":\"high\"}"));
        Assert.Equal("high", prepared.Body["reasoning_effort"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should pass reasoningEffort xhigh setting", Coverage = UpstreamCoverage.Covered)]
    public void PassesReasoningEffortXhigh()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-5.1-codex-max", Call("{\"reasoningEffort\":\"xhigh\"}"));
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"gpt-5.1-codex-max\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"reasoning_effort\":\"xhigh\"}");
    }

    [UpstreamTest(Generate + "should pass reasoningEffort max setting", Coverage = UpstreamCoverage.Covered)]
    public void PassesReasoningEffortMax()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-5.6", Call("{\"reasoningEffort\":\"max\"}"));
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"gpt-5.6\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"reasoning_effort\":\"max\"}");
    }

    [UpstreamTest(Generate + "should parse tool results", Coverage = UpstreamCoverage.Covered)]
    public async Task ParsesToolResults()
    {
        var result = await GenerateWithTool("gpt-3.5-turbo", null);
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_O17Uplv4lJvD6DVdIvFFeRMw", call.ToolCallId);
        Assert.Equal("test-tool", call.ToolName);
        Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
    }

    [UpstreamTest(Generate + "should parse annotations/citations", Coverage = UpstreamCoverage.Covered)]
    public async Task ParsesAnnotations()
    {
        const string json = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Based on the search results [doc1], I found information.\",\"annotations\":[{\"type\":\"url_citation\",\"url_citation\":{\"url\":\"https://example.com/doc1.pdf\",\"title\":\"Document 1\"}}]},\"finish_reason\":\"stop\"}]}";
        var result = await SendResult("gpt-3.5-turbo", OpenAIUpstream.Hello(), json);
        var text = Assert.IsType<GeneratedText>(result.Content[0]);
        var source = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.Equal("Based on the search results [doc1], I found information.", text.Text);
        Assert.Equal("https://example.com/doc1.pdf", source.Url);
        Assert.Equal("Document 1", source.Title);
        Assert.False(string.IsNullOrEmpty(source.Id));
    }

    [UpstreamTest(Format + "should remove string propertyNames from response schemas and warn", Coverage = UpstreamCoverage.Covered)]
    public void RemovesPropertyNamesFromResponseSchema()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", Call("{\"responseFormat\":{\"type\":\"json\",\"schema\":{\"type\":\"object\",\"properties\":{\"variables\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\",\"format\":\"uuid\"},\"additionalProperties\":{\"type\":\"string\"}}},\"required\":[\"variables\"],\"additionalProperties\":false}}}"));
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"gpt-4o-2024-08-06\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"response_format\":{\"type\":\"json_schema\",\"json_schema\":{\"schema\":{\"type\":\"object\",\"properties\":{\"variables\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"string\"}}},\"required\":[\"variables\"],\"additionalProperties\":false},\"strict\":true,\"name\":\"response\"}}}");
        Assert.Equal("compatibility", prepared.Warnings[0].Type);
        Assert.Equal("JSON Schema propertyNames", prepared.Warnings[0].Feature);
        Assert.Equal(OpenAIJsonSchema.PropertyNamesDetails, prepared.Warnings[0].Details);
    }

    [UpstreamTest(Format + "should use json_schema & strict with responseFormat json", Coverage = UpstreamCoverage.Covered)]
    public void UsesJsonSchemaStrict()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", Call("{\"responseFormat\":{\"type\":\"json\",\"schema\":" + ToolSchema + "}}"));
        Assert.Equal("json_schema", prepared.Body["response_format"]!["type"]!.GetValue<string>());
        Assert.True(prepared.Body["response_format"]!["json_schema"]!["strict"]!.GetValue<bool>());
        Assert.Equal("response", prepared.Body["response_format"]!["json_schema"]!["name"]!.GetValue<string>());
        Assert.Equal("http://json-schema.org/draft-07/schema#", prepared.Body["response_format"]!["json_schema"]!["schema"]!["$schema"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Format + "should set strict with tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task SetsToolChoiceRequired()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Tools = new[] { OpenAIUpstream.Tool("test-tool", "test description", schema: ToolSchema) },
            ToolChoice = ToolChoice.Required,
        };
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", options);
        Assert.Equal("required", prepared.Body["tool_choice"]!.GetValue<string>());
        Assert.Equal("test description", prepared.Body["tools"]![0]!["function"]!["description"]!.GetValue<string>());
        Assert.Null(prepared.Body["tools"]![0]!["function"]!["strict"]);
        var result = await GenerateWithTool("gpt-4o-2024-08-06", options);
        Assert.Equal("test-tool", Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content)).ToolName);
    }

    [UpstreamTest(Generate + "should set strict for tool usage", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsUnsetToolStrict()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Tools = new[] { OpenAIUpstream.Tool("test-tool", null, schema: ToolSchema) },
            ToolChoice = ToolChoice.Tool("test-tool"),
        };
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o-2024-08-06", options);
        Assert.Null(prepared.Body["tools"]![0]!["function"]!["strict"]);
        Assert.Null(prepared.Body["tools"]![0]!["function"]!["description"]);
        Assert.Equal("test-tool", prepared.Body["tool_choice"]!["function"]!["name"]!.GetValue<string>());
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single((await GenerateWithTool("gpt-4o-2024-08-06", options)).Content));
        Assert.Equal("{\"value\":\"Spark\"}", call.ArgumentsJson);
    }

    [UpstreamTest(Generate + "should return accepted_prediction_tokens and rejected_prediction_tokens in completion_details_tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsPredictionTokens()
    {
        const string json = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":15,\"completion_tokens\":20,\"total_tokens\":35,\"completion_tokens_details\":{\"accepted_prediction_tokens\":123,\"rejected_prediction_tokens\":456}}}";
        var result = await SendResult("gpt-4o-mini", OpenAIUpstream.Hello(), json);
        var openai = result.ProviderMetadata!.Value.GetProperty("openai");
        Assert.Equal(123, openai.GetProperty("acceptedPredictionTokens").GetInt32());
        Assert.Equal(456, openai.GetProperty("rejectedPredictionTokens").GetInt32());
    }

    [UpstreamTest(Reasoning + "should clear out temperature, top_p, frequency_penalty, presence_penalty and return warnings", Coverage = UpstreamCoverage.Covered)]
    public void ClearsSamplingForReasoningModels()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o4-mini", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Temperature = 0.5,
            TopP = 0.7,
            FrequencyPenalty = 0.2,
            PresencePenalty = 0.3,
        });
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"o4-mini\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}");
        Assert.Equal(4, prepared.Warnings.Count);
        Assert.Equal("temperature is not supported for reasoning models", prepared.Warnings[0].Details);
        Assert.Equal("topP is not supported for reasoning models", prepared.Warnings[1].Details);
        Assert.Equal("frequencyPenalty is not supported for reasoning models", prepared.Warnings[2].Details);
        Assert.Equal("presencePenalty is not supported for reasoning models", prepared.Warnings[3].Details);
    }

    [UpstreamTest(Reasoning + "should preserve disabled reasoning for %s", Coverage = UpstreamCoverage.Covered)]
    public void PreservesDisabledReasoningForSolAndLuna()
    {
        foreach (var modelId in new[] { "gpt-6-sol", "gpt-6-luna" })
        {
            var prepared = OpenAIChatLanguageModel.Prepare(modelId, Call("{\"reasoningEffort\":\"none\"}"));
            Assert.Equal("none", prepared.Body["reasoning_effort"]!.GetValue<string>());
            Assert.Equal(modelId, prepared.Body["model"]!.GetValue<string>());
            Assert.Empty(prepared.Warnings);
        }
    }

    [UpstreamTest(Reasoning + "should omit unsupported GPT-6 reasoning effort %s", Coverage = UpstreamCoverage.Covered)]
    public void OmitsUnsupportedGpt6Efforts()
    {
        foreach (var effort in new[] { "none", "minimal" })
        {
            var prepared = OpenAIChatLanguageModel.Prepare("gpt-6-astra", Call("{\"reasoningEffort\":\"" + effort + "\"}"));
            Assert.Null(prepared.Body["reasoning_effort"]);
            Assert.Equal("reasoningEffort", prepared.Warnings[0].Feature);
            Assert.Equal("gpt-6-astra only supports the following reasoning efforts: low, medium, high, xhigh, max", prepared.Warnings[0].Details);
        }
    }

    [UpstreamTest(Reasoning + "should strip sampling and logprob settings for GPT-6 models", Coverage = UpstreamCoverage.Covered)]
    public void StripsGpt6Sampling()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-6-astra", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Temperature = 0.5,
            TopP = 0.7,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"reasoningEffort\":\"low\",\"logprobs\":5}"),
        });
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"gpt-6-astra\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"reasoning_effort\":\"low\"}");
        Assert.Equal("temperature", prepared.Warnings[0].Feature);
        Assert.Equal("topP", prepared.Warnings[1].Feature);
        Assert.Equal("logprobs is not supported for reasoning models", prepared.Warnings[2].Message);
        Assert.Equal("topLogprobs is not supported for reasoning models", prepared.Warnings[3].Message);
    }

    [UpstreamTest(Generate + "should allow forcing reasoning behavior for unrecognized model IDs via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public void ForcesReasoningForUnknownModels()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("stealth-reasoning-model", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Temperature = 0.5,
            TopP = 0.7,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"forceReasoning\":true}"),
        });
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"stealth-reasoning-model\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}");
        Assert.Equal("temperature", prepared.Warnings[0].Feature);
        Assert.Equal("topP", prepared.Warnings[1].Feature);
    }

    [UpstreamTest(Generate + "should default systemMessageMode to developer when forcing reasoning", Coverage = UpstreamCoverage.Covered)]
    public void DefaultsDeveloperRoleWhenForcingReasoning()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("stealth-reasoning-model", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"forceReasoning\":true}"),
        });
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"stealth-reasoning-model\",\"messages\":[{\"role\":\"developer\",\"content\":\"You are a helpful assistant.\"},{\"role\":\"user\",\"content\":\"Hello\"}]}");
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should allow overriding systemMessageMode via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public void OverridesSystemMessageMode()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"systemMessageMode\":\"developer\"}"),
        });
        Assert.Equal("developer", prepared.Body["messages"]![0]!["role"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should use default systemMessageMode when not overridden", Coverage = UpstreamCoverage.Covered)]
    public void UsesDefaultSystemMessageMode()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
        });
        Assert.Equal("system", prepared.Body["messages"]![0]!["role"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should return the reasoning tokens in the provider metadata", Coverage = UpstreamCoverage.Partial, Note = "Reasoning tokens are 10 and text tokens are 10. cacheWrite is absent, so noCache stays null instead of 15.")]
    public async Task ReturnsReasoningTokens()
    {
        const string json = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":15,\"completion_tokens\":20,\"total_tokens\":35,\"completion_tokens_details\":{\"reasoning_tokens\":10}}}";
        var usage = (await SendResult("o4-mini", OpenAIUpstream.Hello(), json)).Usage;
        Assert.Equal(15, usage.InputTokens);
        Assert.Equal(20, usage.OutputTokens);
        Assert.Equal(10, usage.ReasoningTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Equal(10, usage.Raw!.Value.GetProperty("completion_tokens_details").GetProperty("reasoning_tokens").GetInt32());
    }

    [UpstreamTest(Generate + "should send max_completion_tokens extension setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsMaxCompletionTokens()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o4-mini", Call("{\"maxCompletionTokens\":255}"));
        Assert.Equal(255, prepared.Body["max_completion_tokens"]!.GetValue<int>());
        Assert.Null(prepared.Body["max_tokens"]);
    }

    [UpstreamTest(Generate + "should send prediction extension setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsPrediction()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"prediction\":{\"type\":\"content\",\"content\":\"Hello, World!\"}}"));
        OpenAIUpstream.Equal(prepared.Body["prediction"], "{\"type\":\"content\",\"content\":\"Hello, World!\"}");
    }

    [UpstreamTest(Generate + "should send store extension setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsStore()
    {
        Assert.True(OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"store\":true}")).Body["store"]!.GetValue<bool>());
    }

    [UpstreamTest(Generate + "should send metadata extension values", Coverage = UpstreamCoverage.Covered)]
    public void SendsMetadata()
    {
        OpenAIUpstream.Equal(OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"metadata\":{\"custom\":\"value\"}}")).Body["metadata"], "{\"custom\":\"value\"}");
    }

    [UpstreamTest(Generate + "should send promptCacheKey extension value", Coverage = UpstreamCoverage.Covered)]
    public void SendsPromptCacheKey()
    {
        Assert.Equal("test-cache-key-123", OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"promptCacheKey\":\"test-cache-key-123\"}")).Body["prompt_cache_key"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should send promptCacheRetention extension value", Coverage = UpstreamCoverage.Covered)]
    public void SendsPromptCacheRetention()
    {
        Assert.Equal("24h", OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"promptCacheRetention\":\"24h\"}")).Body["prompt_cache_retention"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should send promptCacheOptions extension value", Coverage = UpstreamCoverage.Covered)]
    public void SendsPromptCacheOptions()
    {
        OpenAIUpstream.Equal(
            OpenAIChatLanguageModel.Prepare("gpt-5.6", Call("{\"promptCacheOptions\":{\"mode\":\"explicit\",\"ttl\":\"30m\"}}")).Body["prompt_cache_options"],
            "{\"mode\":\"explicit\",\"ttl\":\"30m\"}");
    }

    [UpstreamTest(Generate + "should send safetyIdentifier extension value", Coverage = UpstreamCoverage.Covered)]
    public void SendsSafetyIdentifier()
    {
        Assert.Equal("test-safety-identifier-123", OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"safetyIdentifier\":\"test-safety-identifier-123\"}")).Body["safety_identifier"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should send serviceTier flex processing setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsFlexServiceTier()
    {
        Assert.Equal("flex", OpenAIChatLanguageModel.Prepare("o4-mini", Call("{\"serviceTier\":\"flex\"}")).Body["service_tier"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should allow flex processing with o4-mini model without warnings", Coverage = UpstreamCoverage.Covered)]
    public void AllowsFlexOnO4Mini()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o4-mini", Call("{\"serviceTier\":\"flex\"}"));
        Assert.Equal("flex", prepared.Body["service_tier"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should send serviceTier priority processing setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsPriorityServiceTier()
    {
        Assert.Equal("priority", OpenAIChatLanguageModel.Prepare("gpt-4o-mini", Call("{\"serviceTier\":\"priority\"}")).Body["service_tier"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should show warning when using priority processing with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public void WarnsForUnsupportedPriority()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"serviceTier\":\"priority\"}"));
        Assert.Null(prepared.Body["service_tier"]);
        Assert.Equal("priority processing is only available for supported models (gpt-4, gpt-5, gpt-5-mini, o3, o4-mini) and requires Enterprise access. gpt-5-nano is not supported", prepared.Warnings[0].Details);
    }

    [UpstreamTest(Generate + "should allow priority processing with gpt-4o model without warnings", Coverage = UpstreamCoverage.Covered)]
    public void AllowsPriorityOnGpt4o()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o", Call("{\"serviceTier\":\"priority\"}"));
        Assert.Equal("priority", prepared.Body["service_tier"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should allow priority processing with o3 model without warnings", Coverage = UpstreamCoverage.Covered)]
    public void AllowsPriorityOnO3()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("o3", Call("{\"serviceTier\":\"priority\"}"));
        Assert.Equal("priority", prepared.Body["service_tier"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    [UpstreamTest(Generate + "should send serviceTier fast processing setting", Coverage = UpstreamCoverage.Covered)]
    public void SendsFastServiceTier()
    {
        Assert.Equal("fast", OpenAIChatLanguageModel.Prepare("gpt-4o-mini", Call("{\"serviceTier\":\"fast\"}")).Body["service_tier"]!.GetValue<string>());
    }

    [UpstreamTest(Generate + "should show warning when using fast processing with unsupported model", Coverage = UpstreamCoverage.Covered)]
    public void WarnsForUnsupportedFast()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-3.5-turbo", Call("{\"serviceTier\":\"fast\"}"));
        Assert.Null(prepared.Body["service_tier"]);
        Assert.Equal("serviceTier", prepared.Warnings[0].Feature);
        Assert.Equal("priority processing is only available for supported models (gpt-4, gpt-5, gpt-5-mini, o3, o4-mini) and requires Enterprise access. gpt-5-nano is not supported", prepared.Warnings[0].Details);
    }

    [UpstreamTest(Generate + "should allow fast processing with gpt-4o model without warnings", Coverage = UpstreamCoverage.Covered)]
    public void AllowsFastOnGpt4o()
    {
        var prepared = OpenAIChatLanguageModel.Prepare("gpt-4o", Call("{\"serviceTier\":\"fast\"}"));
        Assert.Equal("fast", prepared.Body["service_tier"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    private static LanguageModelCallOptions Call(string openai)
    {
        return new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(openai),
        };
    }

    private static async Task<LanguageModelGenerateResult> GenerateWithTool(string modelId, LanguageModelCallOptions? options)
    {
        const string json = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"\",\"tool_calls\":[{\"id\":\"call_O17Uplv4lJvD6DVdIvFFeRMw\",\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"arguments\":\"{\\\"value\\\":\\\"Spark\\\"}\"}}]},\"finish_reason\":\"stop\"}]}";
        return await SendResult(modelId, options ?? OpenAIUpstream.Hello(), json).ConfigureAwait(false);
    }

    private static async Task<LanguageModelGenerateResult> SendResult(string modelId, LanguageModelCallOptions options, string json)
    {
        var capture = new OpenAICapture { ResponseJson = json };
        return await OpenAIUpstream.Provider(capture).ChatModel(modelId).DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
    }
}
