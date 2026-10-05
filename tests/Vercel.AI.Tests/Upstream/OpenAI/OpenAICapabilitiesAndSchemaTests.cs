// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Model-family capabilities and JSON Schema normalization.</summary>
public sealed class OpenAICapabilitiesAndSchemaTests
{
    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > isReasoningModel::%s reasoning model: %s", Coverage = UpstreamCoverage.Covered)]
    public void ReasoningModelMatrix()
    {
        AssertPair(
            false,
            "gpt-4.1",
            "gpt-4.1-2025-04-14",
            "gpt-4.1-mini",
            "gpt-4.1-mini-2025-04-14",
            "gpt-4.1-nano",
            "gpt-4.1-nano-2025-04-14",
            "gpt-4o",
            "gpt-4o-2024-05-13",
            "gpt-4o-2024-08-06",
            "gpt-4o-2024-11-20",
            "gpt-4o-audio-preview",
            "gpt-4o-audio-preview-2024-12-17",
            "gpt-4o-search-preview",
            "gpt-4o-search-preview-2025-03-11",
            "gpt-4o-mini-search-preview",
            "gpt-4o-mini-search-preview-2025-03-11",
            "gpt-4o-mini",
            "gpt-4o-mini-2024-07-18",
            "gpt-3.5-turbo-0125",
            "gpt-3.5-turbo",
            "gpt-3.5-turbo-1106",
            "gpt-5-chat-latest",
            "new-unknown-model",
            "ft:gpt-4o-2024-08-06:org:custom:abc123",
            "ft:gpt-99:org:custom:abc123",
            "acme-gpt-99-proxy",
            "custom-model");
        AssertPair(
            true,
            "gpt-5.99-chat-latest",
            "o1",
            "o1-2024-12-17",
            "o3-mini",
            "o3-mini-2025-01-31",
            "o3",
            "o3-2025-04-16",
            "o4",
            "o4-mini",
            "o4-mini-2025-04-16",
            "o99",
            "o99-2099-01-01",
            "gpt-5",
            "gpt-5-2025-08-07",
            "gpt-5-codex",
            "gpt-5-mini",
            "gpt-5-mini-2025-08-07",
            "gpt-5-nano",
            "gpt-5-nano-2025-08-07",
            "gpt-5-pro",
            "gpt-5-pro-2025-10-06",
            "gpt-5.4-mini",
            "gpt-5.4-mini-2026-03-17",
            "gpt-5.4-nano",
            "gpt-5.4-nano-2026-03-17",
            "gpt-5.5",
            "gpt-5.5-2026-04-23",
            "gpt-5.6",
            "gpt-5.6-luna",
            "gpt-5.6-sol",
            "gpt-5.6-terra",
            "gpt-5.99",
            "gpt-6-astra",
            "gpt-99",
            "gpt-99-mini");
    }

    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > supportsNonReasoningParameters::%s supports non-reasoning parameters: %s", Coverage = UpstreamCoverage.Covered)]
    public void NonReasoningParameterMatrix()
    {
        foreach (var id in new[]
        {
            "gpt-5.1", "gpt-5.1-chat-latest", "gpt-5.1-codex-mini", "gpt-5.1-codex", "gpt-5.2", "gpt-5.2-pro",
            "gpt-5.2-chat-latest", "gpt-5.3-chat-latest", "gpt-5.4", "gpt-5.4-mini", "gpt-5.4-nano", "gpt-5.4-pro",
            "gpt-5.4-2026-03-05", "gpt-5.4-mini-2026-03-17", "gpt-5.4-nano-2026-03-17", "gpt-5.5", "gpt-5.5-2026-04-23",
            "gpt-5.6", "gpt-5.6-luna", "gpt-5.6-sol", "gpt-5.6-terra", "gpt-5.99", "gpt-5.100",
        })
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsNonReasoningParameters, id);
        }

        foreach (var id in new[] { "gpt-6-astra", "gpt-99", "gpt-5", "gpt-5.0", "gpt-5-mini", "gpt-5-nano", "gpt-5-pro", "gpt-5-chat-latest", "ft:gpt-99:org:custom:abc123", "acme-gpt-99-proxy" })
        {
            Assert.False(OpenAILanguageModelCapabilities.ForModel(id).SupportsNonReasoningParameters, id);
        }
    }

    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::%s supports async tool calling: %s", Coverage = UpstreamCoverage.Covered)]
    public void AsyncToolCallingMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-5.6").SupportsAsyncToolCalling);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("custom-model").SupportsAsyncToolCalling);
        foreach (var id in new[] { "gpt-6-astra", "gpt-6.1", "gpt-7", "gpt-99" })
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsAsyncToolCalling, id);
        }
    }

    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::%s supports configuration updates: %s", Coverage = UpstreamCoverage.Covered)]
    public void ConfigurationUpdateMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-5.6").SupportsConfigurationUpdate);
        foreach (var id in new[] { "gpt-6-astra", "gpt-6.1", "gpt-99" })
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsConfigurationUpdate, id);
        }
    }

    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::%s supports the expected reasoning efforts", Coverage = UpstreamCoverage.Covered)]
    public void ReasoningEffortMatrix()
    {
        Assert.Null(OpenAILanguageModelCapabilities.ForModel("gpt-5.6").SupportedReasoningEfforts);
        var expected = new[] { "low", "medium", "high", "xhigh", "max" };
        Assert.Equal(expected, OpenAILanguageModelCapabilities.ForModel("gpt-6-astra").SupportedReasoningEfforts);
        Assert.Equal(expected, OpenAILanguageModelCapabilities.ForModel("gpt-99").SupportedReasoningEfforts);
    }

    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > supportsFlexProcessing::%s supports flex processing: %s", Coverage = UpstreamCoverage.Covered)]
    public void FlexProcessingMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("o1").SupportsFlexProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-4.1").SupportsFlexProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-99-chat-latest").SupportsFlexProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("ft:gpt-99:org:custom:abc123").SupportsFlexProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("acme-gpt-99-proxy").SupportsFlexProcessing);
        foreach (var id in new[] { "o3", "o4", "o99", "gpt-5", "gpt-5.99", "gpt-6-astra", "gpt-99" })
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsFlexProcessing, id);
        }
    }

    [UpstreamTest("packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > supportsPriorityProcessing::%s supports priority processing: %s", Coverage = UpstreamCoverage.Covered)]
    public void PriorityProcessingMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("o1").SupportsPriorityProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-5.4-nano").SupportsPriorityProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-99-nano").SupportsPriorityProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-99-chat-latest").SupportsPriorityProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("ft:gpt-99:org:custom:abc123").SupportsPriorityProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("acme-gpt-99-proxy").SupportsPriorityProcessing);
        foreach (var id in new[] { "o3", "o4", "o99", "gpt-4.1", "gpt-5", "gpt-5.99", "gpt-6-astra", "gpt-99" })
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsPriorityProcessing, id);
        }
    }

    [UpstreamTest("packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::removes string propertyNames recursively and warns", Coverage = UpstreamCoverage.Covered)]
    public void RemovesStringPropertyNames()
    {
        var original = JsonNode.Parse("{\"type\":\"object\",\"properties\":{\"variables\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\",\"format\":\"uuid\"},\"additionalProperties\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\",\"pattern\":\"^[A-Z_]+$\"}}}},\"definitions\":{\"variable\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\"}}},\"$defs\":{\"conditional\":{\"if\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\"}}}}}")!;
        var result = OpenAIJsonSchema.Normalize(original);
        OpenAIUpstream.Equal(result.Schema, "{\"type\":\"object\",\"properties\":{\"variables\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\"}}},\"definitions\":{\"variable\":{\"type\":\"object\"}},\"$defs\":{\"conditional\":{\"if\":{\"type\":\"object\"}}}}");
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("JSON Schema propertyNames", result.Warnings[0].Feature);
        Assert.Equal(OpenAIJsonSchema.PropertyNamesDetails, result.Warnings[0].Details);
        Assert.Single(result.Warnings);
        Assert.NotNull(original["properties"]!["variables"]!["propertyNames"]);
    }

    [UpstreamTest("packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::rejects non-string propertyNames schemas", Coverage = UpstreamCoverage.Covered)]
    public void RejectsNonStringPropertyNames()
    {
        var exception = Assert.Throws<AiSdkException>(() => OpenAIJsonSchema.Normalize(JsonNode.Parse("{\"type\":\"object\",\"propertyNames\":{\"type\":\"number\"}}")!));
        Assert.Equal("JSON Schema propertyNames that does not use a string schema", exception.Message);
    }

    [UpstreamTest("packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::removes regex lookaround patterns recursively and warns", Coverage = UpstreamCoverage.Covered)]
    public void RemovesRegexLookaround()
    {
        var original = JsonNode.Parse("{\"type\":\"object\",\"properties\":{\"email\":{\"type\":\"string\",\"format\":\"email\",\"pattern\":\"^(?!\\\\.)(?!.*\\\\.\\\\.).+@.+$\"},\"username\":{\"type\":\"string\",\"pattern\":\"^@[a-zA-Z0-9_]+$\"},\"contacts\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\",\"pattern\":\"(?<=prefix)value\"}}}}},\"$defs\":{\"value\":{\"type\":\"string\",\"pattern\":\"value(?=suffix)\"}}}")!;
        var result = OpenAIJsonSchema.Normalize(original);
        OpenAIUpstream.Equal(result.Schema, "{\"type\":\"object\",\"properties\":{\"email\":{\"type\":\"string\",\"format\":\"email\"},\"username\":{\"type\":\"string\",\"pattern\":\"^@[a-zA-Z0-9_]+$\"},\"contacts\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}}},\"$defs\":{\"value\":{\"type\":\"string\"}}}");
        Assert.Equal("JSON Schema pattern with regex lookaround", result.Warnings[0].Feature);
        Assert.Equal(OpenAIJsonSchema.LookaroundDetails, result.Warnings[0].Details);
        Assert.NotNull(original["properties"]!["email"]!["pattern"]);
    }

    [UpstreamTest("packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::preserves escaped and character-class lookaround-like text", Coverage = UpstreamCoverage.Covered)]
    public void PreservesEscapedLookaroundText()
    {
        const string schema = "{\"type\":\"object\",\"properties\":{\"escaped\":{\"type\":\"string\",\"pattern\":\"\\\\(\\\\?=literal\\\\)\"},\"characterClass\":{\"type\":\"string\",\"pattern\":\"[(?=!)]\"}}}";
        var result = OpenAIJsonSchema.Normalize(JsonNode.Parse(schema)!);
        OpenAIUpstream.Equal(result.Schema, schema);
        Assert.Empty(result.Warnings);
    }

    private static void AssertPair(bool expected, params string[] ids)
    {
        foreach (var id in ids)
        {
            Assert.Equal(expected, OpenAILanguageModelCapabilities.ForModel(id).IsReasoningModel);
        }
    }
}
