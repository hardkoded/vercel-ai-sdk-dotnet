// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class FunctionToolsStrictModeTests
{
    private const string Id = Suite + " > function tools strict mode::";

    private static LanguageModelTool Tool(string name, string description, bool? strict = null) =>
        Function(name, description, """{"type":"object","properties":{}}""", strict: strict);

    private static string Expected(string name, string description, bool strict) =>
        $$$"""{"type":"function","name":"{{{name}}}","description":"{{{description}}}","parameters":{"type":"object","properties":{}},"strict":{{{(strict ? "true" : "false")}}}}""";

    [Fact]
    [UpstreamTest(Id + "should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_strict_mode_when_strict_is_true()
    {
        var result = Prepare(null, Tool("testFunction", "A test function", true));

        AssertTools(result, "[" + Expected("testFunction", "A test function", true) + "]");
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should pass through strict mode when strict is false", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_strict_mode_when_strict_is_false()
    {
        var result = Prepare(null, Tool("testFunction", "A test function", false));

        AssertTools(result, "[" + Expected("testFunction", "A test function", false) + "]");
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should default strict mode to false when strict is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_default_strict_mode_to_false_when_strict_is_undefined()
    {
        var result = Prepare(null, Tool("testFunction", "A test function"));

        AssertTools(result, "[" + Expected("testFunction", "A test function", false) + "]");
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should pass through strict mode for multiple tools with different strict settings", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_strict_mode_for_multiple_tools_with_different_strict_settings()
    {
        var result = Prepare(
            null,
            Tool("strictTool", "A strict tool", true),
            Tool("nonStrictTool", "A non-strict tool", false),
            Tool("defaultTool", "A tool without strict setting"));

        AssertTools(
            result,
            "[" + Expected("strictTool", "A strict tool", true) + "," + Expected("nonStrictTool", "A non-strict tool", false) + "," + Expected("defaultTool", "A tool without strict setting", false) + "]");
        AssertNoToolChoice(result);
    }
}
