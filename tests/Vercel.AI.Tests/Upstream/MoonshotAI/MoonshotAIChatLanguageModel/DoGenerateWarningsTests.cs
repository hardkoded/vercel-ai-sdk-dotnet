// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>warnings</c>.
/// </summary>
public sealed class DoGenerateWarningsTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "warnings::";

    [Fact]
    [UpstreamTest(
        Prefix + "should warn for unsupported topK and seed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_for_unsupported_topK_and_seed()
    {
        var (model, _) = Model("kimi-k3");
        var call = TestPrompt();
        call.TopK = 5;
        call.Seed = 42;

        var result = await model.DoGenerateAsync(call, CancellationToken.None);

        AssertWarnings(
            result.Warnings,
            ("unsupported", "topK", null),
            ("unsupported", "seed", null)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn when a message name is set on a tool message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_warn_when_a_message_name_is_set_on_a_tool_message()
    {
        var (model, handler) = Model("kimi-k3");

        var result = await model.DoGenerateAsync(
            Messages(
                new ToolModelMessage("call-1", "weather", "sunny", false, "text")
                {
                    MessageProviderOptions = Opts("{\"moonshotai\":{\"name\":\"weather_tool\"}}"),
                }
            ),
            CancellationToken.None
        );

        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "message name on tool messages"
        );
        Assert.False(Body(handler)["messages"]![0]!.AsObject().ContainsKey("name"));
    }
}
