// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of the tests of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel</c> that sit outside a nested <c>describe</c>.</summary>
public sealed class AnthropicLanguageModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel::ignores typed ping opt-in for non-streaming requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = "97540149254736e8d566d432ccb7abf7b1cebbb3"
    )]
    public async Task Ignores_typed_ping_opt_in_for_non_streaming_requests()
    {
        var handler = AnthropicParity.Ok();

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            AnthropicParity.WithProvider("{\"includePings\":true}")
        );

        Assert.Contains(result.Content, part => part is GeneratedText);
        using var body = JsonDocument.Parse(handler.Body);
        Assert.False(body.RootElement.TryGetProperty("includePings", out _));
    }
}
