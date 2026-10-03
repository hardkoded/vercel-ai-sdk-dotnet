// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>One method per in-scope Anthropic upstream unit test.</summary>
public sealed partial class AnthropicUpstreamTests
{

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should limit max output tokens to the model max and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0185()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should limit max output tokens to the model max and warn").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should not limit max output tokens for unknown models", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0186()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should not limit max output tokens for unknown models").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0188()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should pass tools and toolChoice").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should support cache control", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0191()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should support cache control").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should throw an api error when the server is returning a 529 overloaded error", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0260()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::should throw an api error when the server is returning a 529 overloaded error").ConfigureAwait(false);
    }

}
