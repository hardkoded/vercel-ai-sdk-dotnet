// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.OpenAI.ExpandParallelToolCall;

/// <summary>Port of <c>expand-parallel-tool-call.test.ts</c> &gt; <c>expandParallelToolCall</c>.</summary>
public sealed class ExpandParallelToolCallTests
{
    private const string Id = "packages/openai/src/responses/expand-parallel-tool-call.test.ts::expandParallelToolCall::";

    private const string WrapperInput =
        """{"tool_uses":[{"recipient_name":"functions.weather","parameters":{"location":"San Francisco"}},{"recipient_name":"functions.cityAttractions","parameters":{"city":"Rome"}}]}""";

    private static readonly IReadOnlyCollection<string> ToolNames = OpenAIParallelToolCall.FunctionToolNames(OpenAIResponsesSupport.TestTools);

    private static List<OpenAIExpandedToolCall>? Expand(string input, IReadOnlyCollection<string>? toolNames = null) =>
        OpenAIParallelToolCall.Expand("call_parallel", "parallel", input, toolNames ?? ToolNames, "fc_parallel", "openai");

    [Fact]
    [UpstreamTest(Id + "expands an internal parallel wrapper into declared tool calls", Coverage = UpstreamCoverage.Covered)]
    public void Expands_an_internal_parallel_wrapper_into_declared_tool_calls()
    {
        var expanded = Expand(WrapperInput);

        Assert.NotNull(expanded);
        Assert.Collection(
            expanded,
            call =>
            {
                Assert.Equal("call_parallel_0", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
                Assert.Equal("""{"location":"San Francisco"}""", call.Input);
                JsonAssert.Equal(
                    call.ProviderMetadata,
                    """{"openai":{"parallelToolCall":{"itemId":"fc_parallel","toolCallId":"call_parallel","toolName":"parallel","input":""" + System.Text.Json.JsonSerializer.Serialize(WrapperInput) + ""","index":0,"count":2}}}""");
            },
            call =>
            {
                Assert.Equal("call_parallel_1", call.ToolCallId);
                Assert.Equal("cityAttractions", call.ToolName);
                Assert.Equal("""{"city":"Rome"}""", call.Input);
                JsonAssert.Equal(
                    call.ProviderMetadata,
                    """{"openai":{"parallelToolCall":{"itemId":"fc_parallel","toolCallId":"call_parallel","toolName":"parallel","index":1,"count":2}}}""");
            });
    }

    [Fact]
    [UpstreamTest(Id + "does not expand a declared tool named parallel", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_expand_a_declared_tool_named_parallel()
    {
        var names = ToolNames.Concat(new[] { "parallel" }).ToList();

        Assert.Null(Expand("""{"tool_uses":[{"recipient_name":"functions.weather","parameters":{"location":"Rome"}}]}""", names));
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("""{"tool_uses":[]}""")]
    [InlineData("""{"tool_uses":[{"recipient_name":"functions.unknown","parameters":{}}]}""")]
    [InlineData("""{"tool_uses":[{"recipient_name":"weather","parameters":{}}]}""")]
    [InlineData("""{"tool_uses":[{"recipient_name":"functions.weather","parameters":null}]}""")]
    [UpstreamTest(Id + "does not expand %s", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_expand_invalid_wrappers(string input) => Assert.Null(Expand(input));
}
