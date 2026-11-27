// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

/// <summary>Builders and snapshot checks shared by the tests of <c>anthropic-prepare-tools.test.ts</c>.</summary>
internal static class AnthropicPrepareToolsSupport
{
    internal const string EmptyObjectSchema = "{\"type\":\"object\",\"properties\":{}}";

    internal static AnthropicToolDefinition FunctionTool(
        string name,
        string description,
        string inputSchema = EmptyObjectSchema,
        string? providerOptions = null,
        bool? strict = null,
        string? inputExamples = null
    ) =>
        new()
        {
            Name = name,
            Description = description,
            InputSchema = AnthropicParity.Json(inputSchema),
            ProviderOptions =
                providerOptions == null ? null : AnthropicParity.Json(providerOptions),
            Strict = strict,
            InputExamples = inputExamples == null ? null : AnthropicParity.Json(inputExamples),
        };

    internal static AnthropicToolDefinition ProviderTool(string id, string name, string args) =>
        new()
        {
            Type = "provider",
            Id = id,
            Name = name,
            Args = AnthropicParity.Json(args),
        };

    internal static AnthropicPreparedTools Prepare(
        IReadOnlyList<AnthropicToolDefinition>? tools,
        bool supportsStructuredOutput = true,
        bool supportsStrictTools = true,
        AnthropicCacheControlValidator? cacheControl = null
    ) =>
        AnthropicToolPreparer.Prepare(
            tools,
            null,
            null,
            null,
            cacheControl,
            supportsStructuredOutput,
            supportsStrictTools
        );

    /// <summary>Checks what an upstream inline snapshot of the result checks: betas, no tool choice, no warnings and the tools.</summary>
    internal static void AssertSnapshot(
        AnthropicPreparedTools result,
        string[] betas,
        string toolsJson
    )
    {
        Assert.Equal(betas, result.Betas);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.Warnings);
        AssertTools(result, toolsJson);
    }

    internal static void AssertTools(AnthropicPreparedTools result, string toolsJson)
    {
        Assert.NotNull(result.Tools);
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(toolsJson), result.Tools),
            "Expected tools " + toolsJson + " but received " + result.Tools!.ToJsonString()
        );
    }
}
