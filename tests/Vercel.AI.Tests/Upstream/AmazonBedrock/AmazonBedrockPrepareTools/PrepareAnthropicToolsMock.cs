// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockPrepareTools;

/// <summary>
/// Stands in for upstream's <c>vi.mock('@ai-sdk/anthropic/internal')</c>. It records each call to
/// <c>prepareAnthropicTools</c> and returns no tool choice, no warnings and no betas.
/// </summary>
internal sealed class PrepareAnthropicToolsMock
{
    public List<Call> Calls { get; } = [];

    public Call LastCall => Calls[^1];

    public AnthropicPreparedTools Invoke(
        IReadOnlyList<AnthropicToolDefinition> tools,
        string? toolChoiceType,
        string? toolName,
        bool? disableParallelToolUse,
        bool supportsStructuredOutput,
        bool supportsStrictTools,
        bool rejectsForcedToolUse
    )
    {
        Calls.Add(
            new Call(
                tools,
                toolChoiceType,
                toolName,
                disableParallelToolUse,
                supportsStructuredOutput,
                supportsStrictTools,
                rejectsForcedToolUse
            )
        );
        return new AnthropicPreparedTools(null, null, [], []);
    }

    public sealed record Call(
        IReadOnlyList<AnthropicToolDefinition> Tools,
        string? ToolChoiceType,
        string? ToolName,
        bool? DisableParallelToolUse,
        bool SupportsStructuredOutput,
        bool SupportsStrictTools,
        bool RejectsForcedToolUse
    );
}
