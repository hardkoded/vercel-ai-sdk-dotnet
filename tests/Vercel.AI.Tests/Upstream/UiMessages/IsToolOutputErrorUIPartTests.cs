// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.UiMessages;

/// <summary>Port of <c>ui-messages.test.ts</c> &gt; <c>isToolOutputErrorUIPart</c>.</summary>
public sealed class IsToolOutputErrorUIPartTests
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
        Prefix + "isToolOutputErrorUIPart::should return true for a static tool output error part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsToolOutputErrorUIPart_should_return_true_for_a_static_tool_output_error_part()
    {
        var part = Tool(
            "weather",
            ToolUIPartState.OutputError,
            errorText: "Weather service unavailable"
        );

        Assert.True(UIMessageParts.IsToolOutputErrorUIPart(part));
    }

    [UpstreamTest(
        Prefix + "isToolOutputErrorUIPart::should return true for a dynamic tool output error part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsToolOutputErrorUIPart_should_return_true_for_a_dynamic_tool_output_error_part()
    {
        var part = Tool(
            "weather",
            ToolUIPartState.OutputError,
            isDynamic: true,
            errorText: "Weather service unavailable"
        );

        Assert.Equal("dynamic-tool", part.Type);
        Assert.True(UIMessageParts.IsToolOutputErrorUIPart(part));
    }

    [UpstreamTest(
        Prefix + "isToolOutputErrorUIPart::should return false for a successful tool output part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsToolOutputErrorUIPart_should_return_false_for_a_successful_tool_output_part()
    {
        Assert.False(
            UIMessageParts.IsToolOutputErrorUIPart(Tool("weather", ToolUIPartState.OutputAvailable))
        );
    }

    [UpstreamTest(
        Prefix + "isToolOutputErrorUIPart::should return false for a non-tool part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsToolOutputErrorUIPart_should_return_false_for_a_non_tool_part()
    {
        Assert.False(
            UIMessageParts.IsToolOutputErrorUIPart(new TextUIPart("Weather service unavailable"))
        );
    }
}
