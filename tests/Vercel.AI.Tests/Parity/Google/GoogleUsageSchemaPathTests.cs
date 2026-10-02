// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests;

/// <summary>Upstream usage, schema, model-path, and capability outcomes.</summary>
public sealed class GoogleUsageSchemaPathTests
{
    [Fact]
    [UpstreamTest("packages/google/src/convert-google-usage.test.ts::convertGoogleUsage::includes tool-use prompt tokens in input usage", Coverage = UpstreamCoverage.Covered)]
    public void Includes_tool_use_prompt_tokens_in_input_usage()
    {
        var usage = GoogleUsageConverter.Convert(GoogleParity.Json("""
            {"promptTokenCount":55,"toolUsePromptTokenCount":89,"candidatesTokenCount":51,"thoughtsTokenCount":56,"totalTokenCount":251}
            """));
        Assert.Equal(144, usage.InputTokens);
        Assert.Equal(107, usage.OutputTokens);
        Assert.Equal(51, usage.TextTokens);
        Assert.Equal(56, usage.ReasoningTokens);
        Assert.Equal(144, usage.NoCacheInputTokens);
        Assert.Equal(251, usage.TotalTokens);
        Assert.Equal(89, usage.Raw!.Value.GetProperty("toolUsePromptTokenCount").GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-google-usage.test.ts::convertGoogleUsage::includes tool-use prompt tokens when calculating uncached input", Coverage = UpstreamCoverage.Covered)]
    public void Includes_tool_use_tokens_when_calculating_uncached_input()
    {
        var usage = GoogleUsageConverter.Convert(GoogleParity.Json("""
            {"promptTokenCount":55,"toolUsePromptTokenCount":89,"cachedContentTokenCount":100}
            """));
        Assert.Equal(144, usage.InputTokens);
        Assert.Equal(100, usage.CacheReadTokens);
        Assert.Equal(44, usage.NoCacheInputTokens);
    }

    [Fact]
    [UpstreamTest("packages/google/src/sanitize-response-json-schema.test.ts::replaces const with enum while preserving JSON Schema", Coverage = UpstreamCoverage.Covered)]
    public void Replaces_const_with_enum_without_mutating_the_input()
    {
        var schema = GoogleParity.Json("""
            {"type":"object","properties":{"response":{"oneOf":[{"type":"object","properties":{"type":{"type":"string","const":"fruit"}},"required":["type"],"additionalProperties":false}]}},"required":["response"],"additionalProperties":false,"$defs":{"label":{"type":"string","const":"produce"}}}
            """);
        var sanitized = GoogleJsonSchema.Sanitize(schema);
        Assert.Equal("fruit", sanitized["properties"]!["response"]!["oneOf"]![0]!["properties"]!["type"]!["enum"]![0]!.GetValue<string>());
        Assert.Equal("produce", sanitized["$defs"]!["label"]!["enum"]![0]!.GetValue<string>());
        Assert.Equal("fruit", schema.GetProperty("properties").GetProperty("response").GetProperty("oneOf")[0].GetProperty("properties").GetProperty("type").GetProperty("const").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/get-model-path.test.ts::should pass through model path for models/*", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_a_models_path()
    {
        var capture = new GoogleCapture();
        await GoogleParity.Generate(capture, "models/some-model", GoogleParity.Prompt());
        Assert.EndsWith("/models/some-model:generateContent", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/google/src/get-model-path.test.ts::should pass through model path for tunedModels/*", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_a_tuned_model_path()
    {
        var capture = new GoogleCapture();
        await GoogleParity.Generate(capture, "tunedModels/some-model", GoogleParity.Prompt());
        Assert.EndsWith("/tunedModels/some-model:generateContent", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/google/src/get-model-path.test.ts::should add model path prefix to models without slash", Coverage = UpstreamCoverage.Covered)]
    public async Task Adds_the_models_prefix()
    {
        Assert.Equal("models/some-model", GoogleModelPath.Get("some-model"));
        var capture = new GoogleCapture();
        await GoogleParity.Generate(capture, "some-model", GoogleParity.Prompt());
        Assert.EndsWith("/models/some-model:generateContent", capture.Url!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-model-capabilities.test.ts::getGoogleModelCapabilities::classifies $modelId without falling back to legacy behavior", Coverage = UpstreamCoverage.Covered)]
    public void Classifies_model_ids_without_falling_back_to_legacy_behavior()
    {
        AssertCapabilities("gemini-pro", false, false, false);
        AssertCapabilities("gemini-pro-vision", false, false, false);
        AssertCapabilities("gemini-1.5-flash", false, false, false);
        AssertCapabilities("gemini-robotics-er-1.5-preview", false, false, false);
        AssertCapabilities("gemini-2.0-flash", true, false, false);
        AssertCapabilities("gemini-2.5-flash", true, true, false);
        AssertCapabilities("gemini-3.1-pro-preview", true, true, true);
        AssertCapabilities("gemini-99-pro-preview", true, true, true);
        AssertCapabilities("gemini-ultra-latest", true, true, true);
        AssertCapabilities("nano-banana-pro-preview", true, false, false);
    }

    private static void AssertCapabilities(string modelId, bool tools, bool fileSearch, bool gemini3)
    {
        var capabilities = GoogleModelCapabilitiesMap.Get(modelId);
        Assert.Equal(tools, capabilities.SupportsGemini2Tools);
        Assert.Equal(fileSearch, capabilities.SupportsFileSearch);
        Assert.Equal(gemini3, capabilities.UsesGemini3Features);
    }
}
