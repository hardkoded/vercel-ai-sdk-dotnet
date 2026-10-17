// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ProgrammaticToolCallingTests
{
    private const string Id = Suite + " > programmatic tool calling::";

    [Fact]
    [UpstreamTest(Id + "should serialize the hosted tool and function tool options", Coverage = UpstreamCoverage.Covered)]
    public void Should_serialize_the_hosted_tool_and_function_tool_options()
    {
        const string OutputSchema = """{"type":"object","properties":{"sku":{"type":"string"},"availableUnits":{"type":"number"}},"required":["sku","availableUnits"],"additionalProperties":false}""";
        const string InputSchema = """{"type":"object","properties":{"sku":{"type":"string"}},"required":["sku"],"additionalProperties":false}""";
        var result = Prepare(
            null,
            ProviderTool("openai.programmatic_tool_calling", "program", "{}"),
            Function("get_inventory", "Get inventory", InputSchema, $$"""{"allowedCallers":["programmatic"],"outputSchema":{{OutputSchema}}}"""));

        AssertTools(
            result,
            $$"""[{"type":"programmatic_tool_calling"},{"type":"function","name":"get_inventory","description":"Get inventory","parameters":{{InputSchema}},"strict":false,"allowed_callers":["programmatic"],"output_schema":{{OutputSchema}}}]""");
    }

    [Fact]
    [UpstreamTest(Id + "should support forcing the hosted tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_support_forcing_the_hosted_tool()
    {
        var tool = ProviderTool("openai.programmatic_tool_calling", "program", "{}");
        var result = Prepare(
            new PrepareResponsesToolsOptions { ToolNameMapping = Mapping(new[] { tool }, ("openai.programmatic_tool_calling", "programmatic_tool_calling")) },
            ToolChoice.Tool("program"),
            tool);

        AssertToolChoice(result, """{"type":"programmatic_tool_calling"}""");
    }
}
