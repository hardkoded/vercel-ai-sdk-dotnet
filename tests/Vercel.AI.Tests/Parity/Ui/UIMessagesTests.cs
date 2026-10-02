// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class UIMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::getStaticToolName::should return the tool name after the \"tool-\" prefix",
        Coverage = UpstreamCoverage.Covered)]
    public void Static_tool_name_drops_the_tool_prefix()
    {
        Assert.Equal("getLocation", UIMessages.GetStaticToolName(Part("tool-getLocation", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::getStaticToolName::should return the tool name for tools that contains a dash",
        Coverage = UpstreamCoverage.Covered)]
    public void Static_tool_name_keeps_later_dashes()
    {
        Assert.Equal("get-location", UIMessages.GetStaticToolName(Part("tool-get-location", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isCustomContentUIPart::should return true for a custom part",
        Coverage = UpstreamCoverage.Covered)]
    public void Custom_part_is_custom_content()
    {
        Assert.True(UIMessages.IsCustomContentPart(new UIMessagePart("custom")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isCustomContentUIPart::should return true for a custom part without providerMetadata",
        Coverage = UpstreamCoverage.Covered)]
    public void Custom_part_without_provider_metadata_is_custom_content()
    {
        Assert.True(UIMessages.IsCustomContentPart(new UIMessagePart("custom")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isCustomContentUIPart::should return false for a text part",
        Coverage = UpstreamCoverage.Covered)]
    public void Text_part_is_not_custom_content()
    {
        Assert.False(UIMessages.IsCustomContentPart(new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isDataUIPart::should return true if the part is a data part",
        Coverage = UpstreamCoverage.Covered)]
    public void Data_part_is_recognized()
    {
        Assert.True(UIMessages.IsDataPart(new UIMessagePart("data-someDataPart")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isDataUIPart::should return false if the part is not a data part",
        Coverage = UpstreamCoverage.Covered)]
    public void Text_part_is_not_a_data_part()
    {
        Assert.False(UIMessages.IsDataPart(new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isToolOutputErrorUIPart::should return true for a static tool output error part",
        Coverage = UpstreamCoverage.Covered)]
    public void Static_tool_output_error_is_recognized()
    {
        Assert.True(UIMessages.IsToolOutputErrorPart(Part("tool-weather", "output-error")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isToolOutputErrorUIPart::should return true for a dynamic tool output error part",
        Coverage = UpstreamCoverage.Covered)]
    public void Dynamic_tool_output_error_is_recognized()
    {
        var part = Part("dynamic-tool", "output-error");
        part.ToolName = "weather";
        Assert.True(UIMessages.IsToolOutputErrorPart(part));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isToolOutputErrorUIPart::should return false for a successful tool output part",
        Coverage = UpstreamCoverage.Covered)]
    public void Successful_tool_output_is_not_an_error_part()
    {
        Assert.False(UIMessages.IsToolOutputErrorPart(Part("tool-weather", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/ui-messages.test.ts::isToolOutputErrorUIPart::should return false for a non-tool part",
        Coverage = UpstreamCoverage.Covered)]
    public void Non_tool_part_is_not_a_tool_output_error()
    {
        Assert.False(UIMessages.IsToolOutputErrorPart(new UIMessagePart("text")));
    }

    private static UIMessagePart Part(string type, string state)
    {
        return new UIMessagePart(type)
        {
            State = state,
        };
    }
}
