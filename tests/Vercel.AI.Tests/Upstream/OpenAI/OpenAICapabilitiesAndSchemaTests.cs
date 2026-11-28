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
    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > isReasoningModel::%s reasoning model: %s",
        Coverage = UpstreamCoverage.Covered
    )]
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
            "custom-model"
        );
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
            "gpt-99-mini"
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > supportsNonReasoningParameters::%s supports non-reasoning parameters: %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void NonReasoningParameterMatrix()
    {
        foreach (
            var id in new[]
            {
                "gpt-5.1",
                "gpt-5.1-chat-latest",
                "gpt-5.1-codex-mini",
                "gpt-5.1-codex",
                "gpt-5.2",
                "gpt-5.2-pro",
                "gpt-5.2-chat-latest",
                "gpt-5.3-chat-latest",
                "gpt-5.4",
                "gpt-5.4-mini",
                "gpt-5.4-nano",
                "gpt-5.4-pro",
                "gpt-5.4-2026-03-05",
                "gpt-5.4-mini-2026-03-17",
                "gpt-5.4-nano-2026-03-17",
                "gpt-5.5",
                "gpt-5.5-2026-04-23",
                "gpt-5.6",
                "gpt-5.6-luna",
                "gpt-5.6-sol",
                "gpt-5.6-terra",
                "gpt-5.99",
                "gpt-5.100",
                "gpt-6-sol",
                "gpt-6-luna",
                "gpt-6.1-terra",
            }
        )
        {
            Assert.True(
                OpenAILanguageModelCapabilities.ForModel(id).SupportsNonReasoningParameters,
                id
            );
        }

        foreach (
            var id in new[]
            {
                "gpt-6-astra",
                "gpt-99",
                "gpt-5",
                "gpt-5.0",
                "gpt-5-mini",
                "gpt-5-nano",
                "gpt-5-pro",
                "gpt-5-chat-latest",
                "ft:gpt-99:org:custom:abc123",
                "acme-gpt-99-proxy",
            }
        )
        {
            Assert.False(
                OpenAILanguageModelCapabilities.ForModel(id).SupportsNonReasoningParameters,
                id
            );
        }
    }

    [Theory]
    [InlineData("gpt-5", "minimal,low,medium,high")]
    [InlineData("gpt-5-nano-2025-08-07", "minimal,low,medium,high")]
    [InlineData("gpt-5.1", "none,low,medium,high")]
    [InlineData("gpt-5.2", "none,low,medium,high,xhigh")]
    [InlineData("gpt-5.4-mini", "none,low,medium,high,xhigh")]
    [InlineData("gpt-5.5", "none,low,medium,high,xhigh")]
    [InlineData("gpt-5.6", "none,low,medium,high,xhigh,max")]
    [InlineData("gpt-5.6-luna", "none,low,medium,high,xhigh,max")]
    [InlineData("gpt-5-pro", null)]
    [InlineData("gpt-5-codex", null)]
    [InlineData("gpt-5-chat-latest", null)]
    [InlineData("gpt-5.2-pro", null)]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-5 reasoning efforts::%s supports the expected reasoning efforts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Gpt5ReasoningEfforts(string modelId, string? expected)
    {
        var actual = OpenAILanguageModelCapabilities.ForModel(modelId).SupportedReasoningEfforts;
        Assert.Equal(expected, actual == null ? null : string.Join(",", actual));
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > supportsFlexProcessing::%s supports flex processing: %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void FlexProcessingMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("o1").SupportsFlexProcessing);
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-4.1").SupportsFlexProcessing);
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("gpt-99-chat-latest").SupportsFlexProcessing
        );
        Assert.False(
            OpenAILanguageModelCapabilities
                .ForModel("ft:gpt-99:org:custom:abc123")
                .SupportsFlexProcessing
        );
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("acme-gpt-99-proxy").SupportsFlexProcessing
        );
        foreach (
            var id in new[] { "o3", "o4", "o99", "gpt-5", "gpt-5.99", "gpt-6-astra", "gpt-99" }
        )
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsFlexProcessing, id);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > supportsPriorityProcessing::%s supports priority processing: %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void PriorityProcessingMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("o1").SupportsPriorityProcessing);
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("gpt-5.4-nano").SupportsPriorityProcessing
        );
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("gpt-99-nano").SupportsPriorityProcessing
        );
        Assert.False(
            OpenAILanguageModelCapabilities
                .ForModel("gpt-99-chat-latest")
                .SupportsPriorityProcessing
        );
        Assert.False(
            OpenAILanguageModelCapabilities
                .ForModel("ft:gpt-99:org:custom:abc123")
                .SupportsPriorityProcessing
        );
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("acme-gpt-99-proxy").SupportsPriorityProcessing
        );
        foreach (
            var id in new[]
            {
                "o3",
                "o4",
                "o99",
                "gpt-4.1",
                "gpt-5",
                "gpt-5.99",
                "gpt-6-astra",
                "gpt-99",
            }
        )
        {
            Assert.True(
                OpenAILanguageModelCapabilities.ForModel(id).SupportsPriorityProcessing,
                id
            );
        }
    }

    private static void AssertPair(bool expected, params string[] ids)
    {
        foreach (var id in ids)
        {
            Assert.Equal(expected, OpenAILanguageModelCapabilities.ForModel(id).IsReasoningModel);
        }
    }
}
