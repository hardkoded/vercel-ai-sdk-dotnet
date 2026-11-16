// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.PrepareGoogleInteractionsTools;

/// <summary>Port of <c>prepare-google-interactions-tools.test.ts</c> &gt; <c>prepareGoogleInteractionsTools</c> &gt; <c>toolChoice mapping</c>.</summary>
public sealed class PrepareGoogleInteractionsToolsToolChoiceMappingTests
{
    private const string Prefix = "packages/google/src/interactions/prepare-google-interactions-tools.test.ts::prepareGoogleInteractionsTools > toolChoice mapping::";

    [Fact]
    [UpstreamTest(Prefix + "maps \"auto\" to \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_auto_to_auto()
    {
        var result = GoogleInteractionsTools.Prepare([PrepareGoogleInteractionsToolsTests.FunctionTool()], ToolChoice.Auto);

        Assert.Equal("auto", (string?)result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps \"required\" to \"any\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_required_to_any()
    {
        var result = GoogleInteractionsTools.Prepare([PrepareGoogleInteractionsToolsTests.FunctionTool()], ToolChoice.Required);

        Assert.Equal("any", (string?)result.ToolChoice);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits function tools when toolChoice is none", Coverage = UpstreamCoverage.Covered)]
    public void Omits_function_tools_when_toolChoice_is_none()
    {
        PrepareGoogleInteractionsToolsTests.AssertEmpty(GoogleInteractionsTools.Prepare([PrepareGoogleInteractionsToolsTests.FunctionTool()], ToolChoice.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "maps { type: \"tool\", toolName } to { allowed_tools: { mode: \"validated\", tools: [name] } }", Coverage = UpstreamCoverage.Covered)]
    public void Maps_type_tool_toolName_to_allowed_tools_mode_validated_tools_name()
    {
        var result = GoogleInteractionsTools.Prepare([PrepareGoogleInteractionsToolsTests.FunctionTool()], ToolChoice.Tool("getWeather"));

        PrepareGoogleInteractionsToolsTests.AssertJson("{\"allowed_tools\":{\"mode\":\"validated\",\"tools\":[\"getWeather\"]}}", result.ToolChoice);
    }
}
