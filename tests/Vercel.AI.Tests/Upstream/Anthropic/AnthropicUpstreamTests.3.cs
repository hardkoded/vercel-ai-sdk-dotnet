// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>One method per in-scope Anthropic upstream unit test.</summary>
public sealed partial class AnthropicUpstreamTests
{

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should combine user and tool messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0556()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should combine user and tool messages").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should omit empty compaction blocks", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0562()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should omit empty compaction blocks").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should preserve non-empty compaction blocks", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0564()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should preserve non-empty compaction blocks").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should remove trailing whitespace from last assistant message when there is no further user message", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0566()
    {
        await AnthropicCases.Run("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should remove trailing whitespace from last assistant message when there is no further user message").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::strips unsupported number constraints and adds readable descriptions", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0619()
    {
        await AnthropicCases.Run("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::strips unsupported number constraints and adds readable descriptions").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::strips unsupported string constraints and unsupported formats", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0620()
    {
        await AnthropicCases.Run("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::strips unsupported string constraints and unsupported formats").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::recursively sanitizes arrays, definitions, and composition schemas", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0621()
    {
        await AnthropicCases.Run("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::recursively sanitizes arrays, definitions, and composition schemas").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::converts oneOf to anyOf", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0622()
    {
        await AnthropicCases.Run("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::converts oneOf to anyOf").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::does not mutate the input schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0623()
    {
        await AnthropicCases.Run("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::does not mutate the input schema").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should preserve skill $skillId and version $version as URL path segments", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0624()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should preserve skill $skillId and version $version as URL path segments").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should send files as multipart form data", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0625()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should send files as multipart form data").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should include anthropic-beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0626()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should include anthropic-beta header").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should map response to providerReference", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0627()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should map response to providerReference").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should send display_title in form data when displayTitle is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0628()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should send display_title in form data when displayTitle is provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should not send display_title when displayTitle is not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0629()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should not send display_title when displayTitle is not provided").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should return no warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0630()
    {
        await AnthropicCases.Run("packages/anthropic/src/skills/anthropic-skills.test.ts::AnthropicSkills > uploadSkill::should return no warnings").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/tool/bash_20241022.test.ts::bash_20241022 tool::passes abort signal to sandbox command execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0631()
    {
        await AnthropicCases.Run("packages/anthropic/src/tool/bash_20241022.test.ts::bash_20241022 tool::passes abort signal to sandbox command execution").ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/tool/bash_20250124.test.ts::bash_20250124 tool::passes abort signal to sandbox command execution", Coverage = UpstreamCoverage.Covered)]
    public async Task Case_0632()
    {
        await AnthropicCases.Run("packages/anthropic/src/tool/bash_20250124.test.ts::bash_20250124 tool::passes abort signal to sandbox command execution").ConfigureAwait(false);
    }
}
