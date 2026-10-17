// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class AsyncToolsTests
{
    private const string Id = Suite + " > async tools::";

    [Fact]
    [UpstreamTest(Id + "should pass through async mode for function tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_async_mode_for_function_tools()
    {
        var result = Prepare(null, Function("get_weather", "Get the weather", """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"],"additionalProperties":false}""", """{"async":true}"""));

        AssertTools(result, """[{"type":"function","name":"get_weather","description":"Get the weather","parameters":{"type":"object","properties":{"city":{"type":"string"}},"required":["city"],"additionalProperties":false},"async":true,"strict":false}]""");
    }

    [Fact]
    [UpstreamTest(Id + "should pass through async mode for custom tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_async_mode_for_custom_tools()
    {
        var result = Prepare(null, ProviderTool("openai.custom", "write_sql", """{"description":"Write a SQL query","async":true,"format":{"type":"text"}}"""));

        AssertTools(result, """[{"type":"custom","name":"write_sql","description":"Write a SQL query","async":true,"format":{"type":"text"}}]""");
    }

    [Fact]
    [UpstreamTest(Id + "should omit async mode and warn for unsupported models", Coverage = UpstreamCoverage.Covered)]
    public void Should_omit_async_mode_and_warn_for_unsupported_models()
    {
        var result = Prepare(
            new PrepareResponsesToolsOptions { SupportsAsyncToolCalling = false },
            null,
            Function("get_weather", null, """{"type":"object","properties":{}}""", """{"async":true}"""),
            ProviderTool("openai.custom", "write_sql", """{"async":true}"""));

        AssertTools(result, """[{"type":"function","name":"get_weather","parameters":{"type":"object","properties":{}},"strict":false},{"type":"custom","name":"write_sql"}]""", expectedWarnings: true);
        AssertWarnings(
            result,
            ("unsupported", "async tool calling for \"get_weather\"", "Async tool calling is only supported by GPT-6 and later models."),
            ("unsupported", "async tool calling for \"write_sql\"", "Async tool calling is only supported by GPT-6 and later models."));
    }
}
