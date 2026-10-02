// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>One method per in-scope Anthropic upstream unit test.</summary>
public sealed partial class AnthropicUpstreamTests
{

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should throw an api error when the server is returning a 529 overloaded error", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0365()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should throw an api error when the server is returning a 529 overloaded error").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-4-8", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0381()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-4-8").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-fable-5", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0382()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-fable-5").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-fable-5-1", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0383()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-fable-5-1").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-4-7", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0384()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-4-7").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-sonnet-5", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0385()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-sonnet-5").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-4-6", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0386()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-4-6").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-sonnet-4-6", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0387()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-sonnet-4-6").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-5", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0388()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return correct capabilities for claude-opus-5").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return current-generation capabilities for an unknown Claude model", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0389()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return current-generation capabilities for an unknown Claude model").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should recognize an unknown platform-prefixed Claude model", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0390()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should recognize an unknown platform-prefixed Claude model").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should retain conservative capabilities for legacy Claude model %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0391()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should retain conservative capabilities for legacy Claude model %s").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should match a known model before the forward-compatible fallback", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0392()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should match a known model before the forward-compatible fallback").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should recognize the Vertex model ID %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0393()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should recognize the Vertex model ID %s").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return conservative capabilities for an unknown non-Claude model", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0394()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-language-model.test.ts::getModelCapabilities::should return conservative capabilities for an unknown non-Claude model").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-message-lifecycle.test.ts::Anthropic message lifecycle::fails when a different message starts while the previous message is open", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0429()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-message-lifecycle.test.ts::Anthropic message lifecycle::fails when a different message starts while the previous message is open").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-message-lifecycle.test.ts::Anthropic message lifecycle::ignores a duplicate message_start for the already-open message", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0430()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-message-lifecycle.test.ts::Anthropic message lifecycle::ignores a duplicate message_start for the already-open message").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should include strict and structured-outputs beta when supportsStructuredOutput is true and strict is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0435()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > strict mode for function tools::should include strict and structured-outputs beta when supportsStructuredOutput is true and strict is true").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20251124::should correctly prepare computer_20251124 tool with enableZoom", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0443()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20251124::should correctly prepare computer_20251124 tool with enableZoom").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should reject advisor_20260301 maxTokens below 1024", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0460()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::should reject advisor_20260301 maxTokens below 1024").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0470()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"auto\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0471()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"required\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0472()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"none\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0473()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should handle tool choice \"tool\"").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should set cache control", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0474()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::should set cache control").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::uses the default Anthropic base URL when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0497()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::uses the default Anthropic base URL when not provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::uses ANTHROPIC_BASE_URL when set", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0498()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::uses ANTHROPIC_BASE_URL when set").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::normalizes a bare Anthropic API URL from ANTHROPIC_BASE_URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0499()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::normalizes a bare Anthropic API URL from ANTHROPIC_BASE_URL").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::normalizes a bare Anthropic API URL from the baseURL option", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0500()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::normalizes a bare Anthropic API URL from the baseURL option").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::prefers the baseURL option over ANTHROPIC_BASE_URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0501()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::prefers the baseURL option over ANTHROPIC_BASE_URL").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::rejects an empty baseURL option during provider creation", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0502()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::createAnthropic > baseURL configuration::rejects an empty baseURL option during provider creation").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - authentication > authToken option::sends Authorization Bearer header when authToken is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0503()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - authentication > authToken option::sends Authorization Bearer header when authToken is provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - authentication > apiKey and authToken conflict::throws error when both apiKey and authToken options are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0504()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - authentication > apiKey and authToken conflict::throws error when both apiKey and authToken options are provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - custom provider name::should use custom provider name when specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0505()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - custom provider name::should use custom provider name when specified").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - custom provider name::should default to anthropic.messages when name not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0506()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - custom provider name::should default to anthropic.messages when name not specified").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - supportedUrls::should support image/* URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0507()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - supportedUrls::should support image/* URLs").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - supportedUrls::should support application/pdf URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0508()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-provider.test.ts::anthropic provider - supportedUrls::should support application/pdf URLs").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-unknown-model-max-output-tokens.test.ts::unknown model max output tokens::should warn when using the default max output token limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0509()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-unknown-model-max-output-tokens.test.ts::unknown model max output tokens::should warn when using the default max output token limit").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-unknown-model-max-output-tokens.test.ts::unknown model max output tokens::should not warn when max output tokens are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0510()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-unknown-model-max-output-tokens.test.ts::unknown model max output tokens::should not warn when max output tokens are provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-unknown-model-max-output-tokens.test.ts::unknown model max output tokens::should use the current-generation default and warn for an unknown Claude model", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0511()
    {
        await AnthropicCases.Run("packages/anthropic/src/anthropic-unknown-model-max-output-tokens.test.ts::unknown model max output tokens::should use the current-generation default and warn for an unknown Claude model").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should use usage as raw when rawUsage is not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0512()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should use usage as raw when rawUsage is not provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should use rawUsage as raw when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0513()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should use rawUsage as raw when provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should compute token totals correctly with cache tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0514()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should compute token totals correctly with cache tokens").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should handle null cache tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0515()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage::should handle null cache tokens").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should sum across all iterations when iterations array is present", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0516()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should sum across all iterations when iterations array is present").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should handle single iteration (message only, no compaction triggered)", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0517()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should handle single iteration (message only, no compaction triggered)").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should handle multiple compaction iterations (long-running task)", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0518()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should handle multiple compaction iterations (long-running task)").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should combine iterations with cache tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0519()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should combine iterations with cache tokens").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should use rawUsage as raw even when iterations are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0520()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > compaction usage with iterations::should use rawUsage as raw even when iterations are present").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should use top-level values when iterations is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0521()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should use top-level values when iterations is null").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should use top-level values when iterations is undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0522()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should use top-level values when iterations is undefined").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should use top-level values when iterations array is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0523()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should use top-level values when iterations array is empty").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should handle zero tokens in iterations", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0524()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > edge cases::should handle zero tokens in iterations").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > real-world scenarios from documentation::should match documentation example exactly", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0525()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > real-world scenarios from documentation::should match documentation example exactly").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > real-world scenarios from documentation::should handle re-applying previous compaction block (no new compaction)", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0526()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-anthropic-usage.test.ts::convertAnthropicUsage > real-world scenarios from documentation::should handle re-applying previous compaction block (no new compaction)").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should convert a single system message into an anthropic system message", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0527()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should convert a single system message into an anthropic system message").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add image parts for URL images", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0535()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add image parts for URL images").ConfigureAwait(false);
    }

}
