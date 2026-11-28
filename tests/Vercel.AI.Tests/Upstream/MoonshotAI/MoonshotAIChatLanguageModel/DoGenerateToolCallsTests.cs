// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>tool calls</c>.
/// </summary>
public sealed class DoGenerateToolCallsTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "tool calls::";

    private const string City =
        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}";

    private const string Coords =
        "{\"properties\":{\"lat\":{\"type\":\"number\"}},\"required\":[\"lat\"]}";

    private static LanguageModelCallOptions WeatherCall(ToolChoice choice)
    {
        var call = TestPrompt();
        call.Tools =
        [
            Tool("get_weather", "Get the weather", "{\"type\":\"object\",\"properties\":{}}"),
        ];
        call.ToolChoice = choice;
        return call;
    }

    [Theory]
    [InlineData("kimi-k2.6")]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [UpstreamTest(
        Prefix + "should omit required tool choice and warn for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_required_tool_choice_and_warn_for(string modelId)
    {
        var (model, handler) = Model(modelId, "moonshotai-tool-call");

        var result = await model.DoGenerateAsync(
            WeatherCall(ToolChoice.Required),
            CancellationToken.None
        );

        Assert.False(Body(handler).ContainsKey("tool_choice"));
        AssertWarnings(
            result.Warnings,
            (
                "unsupported",
                "tool choice \"required\" for model \"" + modelId + "\"",
                "Moonshot AI rejects required tool choice for this model. The setting has been omitted; use \"auto\" or select a specific tool instead."
            )
        );
    }

    [Theory]
    [InlineData("kimi-k3")]
    [InlineData("custom-kimi-model")]
    [UpstreamTest(
        Prefix + "should preserve required tool choice for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_required_tool_choice_for(string modelId)
    {
        var (model, handler) = Model(modelId, "moonshotai-tool-call");

        var result = await model.DoGenerateAsync(
            WeatherCall(ToolChoice.Required),
            CancellationToken.None
        );

        Assert.Equal("required", (string?)Body(handler)["tool_choice"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should normalize tuple tool schemas to prefixItems on the wire",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_normalize_tuple_tool_schemas_to_prefixItems_on_the_wire()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-tool-call");
        var call = TestPrompt();
        call.Tools =
        [
            Tool(
                "probe",
                "probe",
                "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"items\":[{\"type\":\"number\"},{\"type\":\"number\"}]}},\"required\":[\"a\"]}"
            ),
        ];

        await model.DoGenerateAsync(call, CancellationToken.None);

        Strict(
            Body(handler)["tools"]![0]!["function"]!["parameters"],
            "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"number\"},{\"type\":\"number\"}]}},\"required\":[\"a\"]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send root unions of objects with type object on the wire",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_root_unions_of_objects_with_type_object_on_the_wire()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-tool-call");
        var call = TestPrompt();
        call.Tools =
        [
            Tool("untyped", null, "{\"anyOf\":[" + City + "," + Coords + "]}"),
            Tool("typed", null, "{\"type\":\"object\",\"anyOf\":[" + City + "," + Coords + "]}"),
        ];

        await model.DoGenerateAsync(call, CancellationToken.None);

        var tools = Body(handler)["tools"]!.AsArray();
        Assert.Equal(2, tools.Count);
        foreach (var tool in tools)
        {
            Strict(
                tool!["function"]!["parameters"],
                "{\"type\":\"object\",\"anyOf\":[" + City + "," + Coords + "]}"
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract tool calls and finish reason",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_tool_calls_and_finish_reason()
    {
        var (model, _) = Model("kimi-k3", "moonshotai-tool-call");

        var result = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_abc123", call.ToolCallId);
        Assert.Equal("get_weather", call.ToolName);
        Assert.Equal("{\"city\":\"Paris\"}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"moonshotai\":{\"responseObject\":\"chat.completion\",\"choiceIndex\":0,\"messageRole\":\"assistant\",\"toolCallTypes\":[\"function\"]}}"
        );
    }
}
