// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using System.Runtime.CompilerServices;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaPrepareTools.AlibabaPrepareToolsSupport;
using PrepareTools = global::Vercel.AI.Alibaba.AlibabaPrepareTools;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaPrepareTools;

/// <summary>Port of <c>alibaba-prepare-tools.test.ts</c> &gt; <c>prepareTools</c>.</summary>
public sealed class PrepareToolsTests
{
    private const string Prefix = TestFile + "::prepareTools::";

    [Fact]
    [UpstreamTest(Prefix + "should return undefined tools and toolChoice when tools are null", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_tools_and_toolChoice_when_tools_are_null()
    {
        var result = PrepareTools.Prepare(null, null);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined tools and toolChoice when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_tools_and_toolChoice_when_tools_are_empty()
    {
        var result = PrepareTools.Prepare(Array.Empty<LanguageModelTool>(), null);

        Assert.Null(result.Tools);
        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_correctly_prepare_function_tools()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "A test function") }, null);

        Assert.Null(result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
        JsonAssert.Equal(result.Tools, """[{"function":{"description":"A test function","name":"testFunction","parameters":{"properties":{},"type":"object"}},"type":"function"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should add warnings for unsupported provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_warnings_for_unsupported_provider_defined_tools()
    {
        var result = PrepareTools.Prepare(
            new[] { LanguageModelTool.ProviderDefined("some.unsupported_tool", "unsupported_tool", UpstreamChat.Json("{}")) },
            null);

        JsonAssert.Equal(result.Tools, "[]");
        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool some.unsupported_tool", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_auto()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "Test", "{}") }, ToolChoice.Auto);

        JsonAssert.Equal(result.ToolChoice, "\"auto\"");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_required()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "Test", "{}") }, ToolChoice.Required);

        JsonAssert.Equal(result.ToolChoice, "\"required\"");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_none()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "Test", "{}") }, ToolChoice.None);

        JsonAssert.Equal(result.ToolChoice, "\"none\"");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_tool()
    {
        var result = PrepareTools.Prepare(new[] { Function("testFunction", "Test", "{}") }, ToolChoice.Tool("testFunction"));

        JsonAssert.Equal(result.ToolChoice, """{"type":"function","function":{"name":"testFunction"}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw for an unrecognized tool choice type", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_for_an_unrecognized_tool_choice_type()
    {
        // ToolChoice is a closed hierarchy, so the only way to get a choice with no recognized type is to skip its constructor.
        var unrecognized = (ToolChoice)RuntimeHelpers.GetUninitializedObject(typeof(ToolChoice).GetNestedType("AutoChoice", BindingFlags.NonPublic)!);

        Assert.Throws<AiSdkException>(() => PrepareTools.Prepare(new[] { Function("testFunction", "Test", "{}") }, unrecognized));
    }
}
