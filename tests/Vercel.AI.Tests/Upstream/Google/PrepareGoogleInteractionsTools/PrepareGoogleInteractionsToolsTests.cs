// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.PrepareGoogleInteractionsTools;

/// <summary>Port of <c>prepare-google-interactions-tools.test.ts</c> &gt; <c>prepareGoogleInteractionsTools</c>.</summary>
public sealed class PrepareGoogleInteractionsToolsTests
{
    private const string Prefix =
        "packages/google/src/interactions/prepare-google-interactions-tools.test.ts::prepareGoogleInteractionsTools::";
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\",\"description\":\"City name\"}},\"required\":[\"location\"]}";

    internal static LanguageModelTool FunctionTool(
        string? description = "Get the current weather in a location"
    )
    {
        return new LanguageModelTool(
            "getWeather",
            description,
            JsonDocument.Parse(Schema).RootElement.Clone()
        );
    }

    internal static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }

    internal static void AssertEmpty(PrepareGoogleInteractionsToolsResult result)
    {
        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns undefined tools/toolChoice and no warnings when no tools are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Returns_undefined_tools_toolChoice_and_no_warnings_when_no_tools_are_provided()
    {
        AssertEmpty(GoogleInteractionsTools.Prepare(null));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "treats an empty tools array as no tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Treats_an_empty_tools_array_as_no_tools()
    {
        AssertEmpty(GoogleInteractionsTools.Prepare([]));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "maps an AI SDK function tool to an Interactions function tool with passthrough JSON Schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_an_AI_SDK_function_tool_to_an_Interactions_function_tool_with_passthrough_JSON_Schema()
    {
        var result = GoogleInteractionsTools.Prepare([FunctionTool()]);

        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        AssertJson(
            "[{\"type\":\"function\",\"name\":\"getWeather\",\"description\":\"Get the current weather in a location\",\"parameters\":"
                + Schema
                + "}]",
            result.Tools
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits an empty description string when the tool has none",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_an_empty_description_string_when_the_tool_has_none()
    {
        var result = GoogleInteractionsTools.Prepare([FunctionTool(null)]);

        AssertJson(
            "{\"type\":\"function\",\"name\":\"getWeather\",\"description\":\"\",\"parameters\":"
                + Schema
                + "}",
            result.Tools![0]
        );
    }
}
