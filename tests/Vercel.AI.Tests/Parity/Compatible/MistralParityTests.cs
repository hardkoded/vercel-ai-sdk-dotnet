// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class MistralParityTests
{
    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-prepare-tools.test.ts::prepareTools::should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_true()
    {
        var prepared = MistralTools.Prepare(new[] { Tool("testFunction", true) }, null);
        Assert.True(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-prepare-tools.test.ts::prepareTools::should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_false()
    {
        var prepared = MistralTools.Prepare(new[] { Tool("testFunction", false) }, null);
        Assert.False(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-prepare-tools.test.ts::prepareTools::should not include strict mode when strict is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Omits_unset_strict()
    {
        var prepared = MistralTools.Prepare(new[] { Tool("testFunction", null) }, null);
        Assert.Null(prepared.Tools![0]!["function"]!["strict"]);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-prepare-tools.test.ts::prepareTools::should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void Passes_mixed_strict_settings()
    {
        var prepared = MistralTools.Prepare(new[] { Tool("strictTool", true), Tool("nonStrictTool", false), Tool("defaultTool", null) }, null);
        Assert.True(prepared.Tools![0]!["function"]!["strict"]!.GetValue<bool>());
        Assert.False(prepared.Tools[1]!["function"]!["strict"]!.GetValue<bool>());
        Assert.Null(prepared.Tools[2]!["function"]!["strict"]);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > warnings::should warn about unsupported reasoning for non-supporting models", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_the_model_cannot_configure_reasoning()
    {
        var (model, handler) = Create("mistral-large-latest");
        var options = Hello();
        options.Reasoning = "high";
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.False(JsonDocument.Parse(handler.Body).RootElement.TryGetProperty("reasoning_effort", out _));
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message.IndexOf("This model does not support reasoning configuration.", StringComparison.Ordinal) >= 0);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > warnings::should emit compatibility warning for reasoning medium on supporting model", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_medium_maps_to_high()
    {
        var (model, _) = Create("mistral-small-latest");
        var options = Hello();
        options.Reasoning = "medium";
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Message.IndexOf("mapped to effort \"high\"", StringComparison.Ordinal) >= 0);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > warnings::should not warn for reasoning high on supporting model", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_warn_for_high_reasoning()
    {
        var (model, _) = Create("mistral-small-latest");
        var options = Hello();
        options.Reasoning = "high";
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > warnings::should not warn about reasoning for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_warn_for_supporting_models()
    {
        foreach (var modelId in new[] { "mistral-medium-3-5", "mistral-medium-latest", "mistral-vibe-cli-fast", "zai-glm-5-2", "glm-5-2", "labs-leanstral-1-5" })
        {
            var (model, _) = Create(modelId);
            var options = Hello();
            options.Reasoning = "high";
            var result = await model.DoGenerateAsync(options, CancellationToken.None);
            Assert.Empty(result.Warnings);
        }
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should send reasoning_effort high for reasoning high", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_high_for_high()
    {
        await AssertEffort("mistral-small-latest", "high", "high");
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should send reasoning_effort high for reasoning medium", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_high_for_medium()
    {
        await AssertEffort("mistral-small-latest", "medium", "high");
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should send reasoning_effort high for reasoning minimal", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_high_for_minimal()
    {
        await AssertEffort("mistral-small-latest", "minimal", "high");
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should send reasoning_effort none for reasoning none", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_none_for_none()
    {
        await AssertEffort("mistral-small-latest", "none", "none");
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should allow provider option to override reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_option_overrides_reasoning()
    {
        var (model, handler) = Create("mistral-small-latest");
        var options = Hello();
        options.Reasoning = "none";
        options.ProviderOptions = new Dictionary<string, JsonElement> { ["mistral"] = Parse("{\"reasoningEffort\":\"high\"}") };
        var result = await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("high", JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should not send reasoning_effort for non-supporting models", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_reasoning_effort_for_unsupported_models()
    {
        var (model, handler) = Create("mistral-large-latest");
        var options = Hello();
        options.Reasoning = "high";
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.False(JsonDocument.Parse(handler.Body).RootElement.TryGetProperty("reasoning_effort", out _));
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > reasoning_effort::should send reasoning_effort for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_reasoning_effort_for_supporting_models()
    {
        foreach (var modelId in new[] { "mistral-medium-3-5", "mistral-medium-latest", "mistral-vibe-cli-fast", "zai-glm-5-2", "glm-5-2", "labs-leanstral-1-5" })
        {
            await AssertEffort(modelId, "high", "high");
        }
    }

    private static async Task AssertEffort(string modelId, string reasoning, string expected)
    {
        var (model, handler) = Create(modelId);
        var options = Hello();
        options.Reasoning = reasoning;
        await model.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal(expected, JsonDocument.Parse(handler.Body).RootElement.GetProperty("reasoning_effort").GetString());
    }

    private static (MistralChatLanguageModel Model, CaptureHandler Handler) Create(string modelId)
    {
        var handler = new CaptureHandler();
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler);
        return ((MistralChatLanguageModel)provider.LanguageModel(modelId), handler);
    }

    private static LanguageModelCallOptions Hello()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };
    }

    private static LanguageModelTool Tool(string name, bool? strict)
    {
        return new LanguageModelTool(name, "A test function", Parse("{\"type\":\"object\",\"properties\":{}}"), strict);
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
