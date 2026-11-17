// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekInternalOptions;

/// <summary>Port of <c>deepseek-internal-options.test.ts</c> &gt; <c>DeepSeek internal schema compatibility</c>.</summary>
public sealed class DeepSeekInternalSchemaCompatibilityTests
{
    private const string Prefix =
        "packages/deepseek/src/chat/deepseek-internal-options.test.ts::DeepSeek internal schema compatibility::";

    /// <summary>The upstream clone this test was ported from. The parity catalog does not list the file.</summary>
    private const string Commit = "3bea041f28e65f371e640d2c70c302776a46387f";

    [Fact]
    [UpstreamTest(
        Prefix + "keeps chat options as a composable Zod object",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no Zod schemas. Only the parse of topLogprobs 20 and the rejection of 21 apply. The Zod object check and extend are not ported.",
        Commit = Commit
    )]
    public void Validates_chat_option_top_logprobs_without_Zod_composition()
    {
        var parsed = new DeepSeekLanguageModelChatOptions { TopLogprobs = 20 }.ToProviderOptions();

        JsonAssert.Equal(parsed["deepseek"], """{ "topLogprobs": 20 }""");
        Assert.Throws<InvalidArgumentError>(() =>
            new DeepSeekLanguageModelChatOptions { TopLogprobs = 21 }.ToProviderOptions()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps message schemas and their shared field composable",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no Zod schemas. The assistant options derive from the message options, which shares the name field. The Zod object checks and extend are not ported.",
        Commit = Commit
    )]
    public void Shares_the_name_option_between_message_provider_options_without_Zod_composition()
    {
        var assistant = new DeepSeekAssistantMessageProviderOptions
        {
            Name = "assistant",
            Prefix = true,
        };
        var user = new DeepSeekMessageProviderOptions { Name = "user" };

        Assert.IsAssignableFrom<DeepSeekMessageProviderOptions>(assistant);
        JsonAssert.Equal(
            assistant.ToProviderOptions()["deepseek"],
            """{ "name": "assistant", "prefix": true }"""
        );
        JsonAssert.Equal(user.ToProviderOptions()["deepseek"], """{ "name": "user" }""");
    }
}
