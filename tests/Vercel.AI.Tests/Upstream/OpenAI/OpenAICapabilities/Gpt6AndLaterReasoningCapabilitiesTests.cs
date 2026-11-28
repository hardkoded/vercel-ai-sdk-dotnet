// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAICapabilities;

/// <summary>Port of <c>openai-language-model-capabilities.test.ts</c> &gt; <c>getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities</c>.</summary>
public sealed class Gpt6AndLaterReasoningCapabilitiesTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::supports the documented GPT-6.1 Sol capabilities",
        Coverage = UpstreamCoverage.Covered
    )]
    public void SupportsTheDocumentedGpt61SolCapabilities()
    {
        var capabilities = OpenAILanguageModelCapabilities.ForModel("gpt-6.1-sol");

        Assert.True(capabilities.IsReasoningModel);
        Assert.Equal("developer", capabilities.SystemMessageMode);
        Assert.True(capabilities.SupportsFlexProcessing);
        Assert.True(capabilities.SupportsPriorityProcessing);
        Assert.True(capabilities.SupportsConfigurationUpdate);
        Assert.True(capabilities.SupportsAsyncToolCalling);
        Assert.Equal(
            new[] { "low", "medium", "high", "xhigh", "max" },
            capabilities.SupportedReasoningEfforts
        );
        Assert.False(capabilities.SupportsNonReasoningParameters);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::%s supports async tool calling: %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void AsyncToolCallingMatrix()
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel("gpt-5.6").SupportsAsyncToolCalling);
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("custom-model").SupportsAsyncToolCalling
        );
        foreach (var id in new[] { "gpt-6-astra", "gpt-6.1", "gpt-7", "gpt-99" })
        {
            Assert.True(OpenAILanguageModelCapabilities.ForModel(id).SupportsAsyncToolCalling, id);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::%s supports configuration updates: %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ConfigurationUpdateMatrix()
    {
        Assert.False(
            OpenAILanguageModelCapabilities.ForModel("gpt-5.6").SupportsConfigurationUpdate
        );
        foreach (var id in new[] { "gpt-6-astra", "gpt-6.1", "gpt-99" })
        {
            Assert.True(
                OpenAILanguageModelCapabilities.ForModel(id).SupportsConfigurationUpdate,
                id
            );
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/openai-language-model-capabilities.test.ts::getOpenAILanguageModelCapabilities > GPT-6 and later reasoning capabilities::%s supports the expected reasoning efforts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ReasoningEffortMatrix()
    {
        var expected = new[] { "low", "medium", "high", "xhigh", "max" };
        Assert.Equal(
            expected,
            OpenAILanguageModelCapabilities.ForModel("gpt-6-astra").SupportedReasoningEfforts
        );
        Assert.Equal(
            expected,
            OpenAILanguageModelCapabilities.ForModel("gpt-99").SupportedReasoningEfforts
        );
    }
}
