// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.UiMessages;

/// <summary>Port of <c>ui-messages.test.ts</c>.</summary>
public sealed class UiMessagesTests
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

    [UpstreamTest(
        Prefix + "isCustomContentUIPart::should return true for a custom part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsCustomContentUIPart_should_return_true_for_a_custom_part()
    {
        var part = new CustomContentUIPart("test-provider.compaction")
        {
            ProviderMetadata = new Dictionary<string, JsonElement>
            {
                ["openai"] = Json("{\"itemId\":\"cmp_123\"}"),
            },
        };

        Assert.True(UIMessageParts.IsCustomContentUIPart(part));
    }

    [UpstreamTest(
        Prefix
            + "isCustomContentUIPart::should return true for a custom part without providerMetadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsCustomContentUIPart_should_return_true_for_a_custom_part_without_providerMetadata()
    {
        Assert.True(
            UIMessageParts.IsCustomContentUIPart(new CustomContentUIPart("openai.compaction"))
        );
    }

    [UpstreamTest(
        Prefix + "isCustomContentUIPart::should return false for a text part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsCustomContentUIPart_should_return_false_for_a_text_part()
    {
        Assert.False(UIMessageParts.IsCustomContentUIPart(new TextUIPart("some text")));
    }

    [UpstreamTest(
        Prefix + "isDataUIPart::should return true if the part is a data part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsDataUIPart_should_return_true_if_the_part_is_a_data_part()
    {
        var part = new DataUIPart("someDataPart", Json("\"some data\""));

        Assert.Equal("data-someDataPart", part.Type);
        Assert.True(UIMessageParts.IsDataUIPart(part));
    }

    [UpstreamTest(
        Prefix + "isDataUIPart::should return false if the part is not a data part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsDataUIPart_should_return_false_if_the_part_is_not_a_data_part()
    {
        Assert.False(UIMessageParts.IsDataUIPart(new TextUIPart("some text")));
    }

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
