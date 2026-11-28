// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>dynamic tool loading</c>.
/// </summary>
public sealed class DoGenerateDynamicToolLoadingTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "dynamic tool loading::";

    private const string Calculator =
        "{\"moonshotai\":{\"tools\":[{\"type\":\"function\",\"name\":\"calculator\",\"inputSchema\":{\"type\":\"object\",\"properties\":{}}}]}}";

    [Fact]
    [UpstreamTest(
        Prefix + "should send normalized dynamic tools in a K3 system message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_normalized_dynamic_tools_in_a_K3_system_message()
    {
        var (model, handler) = Model("kimi-k3", "moonshotai-tool-call");

        await model.DoGenerateAsync(
            Messages(
                new UserModelMessage("Calculate."),
                new SystemModelMessage(
                    "",
                    Opts(
                        "{\"moonshotai\":{\"tools\":[{\"type\":\"function\",\"name\":\"calculator\",\"description\":\"Evaluate an expression\",\"inputSchema\":{\"type\":\"object\",\"properties\":{\"values\":{\"type\":\"array\",\"items\":[{\"type\":\"number\"},{\"type\":\"number\"}]}}}}]}}"
                    )
                )
            ),
            CancellationToken.None
        );

        var messages = Body(handler)["messages"]!.AsArray();
        Strict(
            messages[messages.Count - 1],
            "{\"role\":\"system\",\"tools\":[{\"type\":\"function\",\"function\":{\"name\":\"calculator\",\"description\":\"Evaluate an expression\",\"parameters\":{\"type\":\"object\",\"properties\":{\"values\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"number\"},{\"type\":\"number\"}]}}}}}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit dynamic messages and warn for non-K3 official models",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_dynamic_messages_and_warn_for_non_K3_official_models()
    {
        var (model, handler) = Model("kimi-k2.6", "moonshotai-tool-call");

        var result = await model.DoGenerateAsync(
            Messages(new SystemModelMessage("", Opts(Calculator))),
            CancellationToken.None
        );

        Match(Body(handler), "{\"messages\":[]}");
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "unsupported"
            && warning.Feature == "dynamic tool loading for model \"kimi-k2.6\""
            && warning.Message
                == "Moonshot documents dynamic tool loading only for Kimi K3. The dynamic system message has been omitted."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve dynamic messages for unknown custom models",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_dynamic_messages_for_unknown_custom_models()
    {
        var (model, handler) = Model("custom-model", "moonshotai-tool-call");

        await model.DoGenerateAsync(
            Messages(new SystemModelMessage("", Opts(Calculator))),
            CancellationToken.None
        );

        Match(
            Body(handler)["messages"]![0],
            "{\"role\":\"system\",\"tools\":[{\"function\":{\"name\":\"calculator\"}}]}"
        );
    }
}
