// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.UiMessages;

/// <summary>Port of <c>ui-messages.test.ts</c> &gt; <c>getStaticToolName</c>.</summary>
public sealed class GetStaticToolNameTests
{
    private const string Prefix = "packages/ai/src/ui/ui-messages.test.ts::";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static ToolUIPart Tool(
        string name,
        ToolUIPartState state,
        bool isDynamic = false,
        string? errorText = null
    ) =>
        new(name, "tool1", state, isDynamic)
        {
            Input = Json("{}"),
            Output = state == ToolUIPartState.OutputAvailable ? Json("\"some result\"") : null,
            ErrorText = errorText,
        };

    [UpstreamTest(
        Prefix + "getStaticToolName::should return the tool name after the \"tool-\" prefix",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void GetStaticToolName_should_return_the_tool_name_after_the_tool_prefix()
    {
        var part = Tool("getLocation", ToolUIPartState.OutputAvailable);

        Assert.Equal("tool-getLocation", part.Type);
        Assert.Equal("getLocation", UIMessageParts.GetStaticToolName(part));
    }

    [UpstreamTest(
        Prefix + "getStaticToolName::should return the tool name for tools that contains a dash",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void GetStaticToolName_should_return_the_tool_name_for_tools_that_contains_a_dash()
    {
        var part = Tool("get-location", ToolUIPartState.OutputAvailable);

        Assert.Equal("get-location", UIMessageParts.GetStaticToolName(part));
    }
}
