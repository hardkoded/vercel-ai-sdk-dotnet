// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>Partial Mode</c>.
/// </summary>
public sealed class DoGeneratePartialModeTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "Partial Mode::";

    private static AssistantModelMessage Partial(string text, string options)
    {
        return new AssistantModelMessage([new AssistantTextPart(text)])
        {
            ProviderOptions = Opts("{\"moonshotai\":" + options + "}"),
        };
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send partial true on the final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_partial_true_on_the_final_assistant_message()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(
            Messages(
                new UserModelMessage("Continue this prefix."),
                Partial("The sky is", "{\"name\":\"writer\",\"partial\":true}")
            ),
            CancellationToken.None
        );

        Match(
            Body(handler),
            "{\"messages\":[{\"role\":\"user\",\"content\":\"Continue this prefix.\"},{\"role\":\"assistant\",\"content\":\"The sky is\",\"name\":\"writer\",\"partial\":true}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject Partial Mode with JSON object response format before the API call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_Partial_Mode_with_JSON_object_response_format_before_the_API_call()
    {
        var (model, handler) = Model("kimi-k3");
        var call = Messages(Partial("{", "{\"partial\":true}"));
        call.JsonResponse = true;

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            model.DoGenerateAsync(call, CancellationToken.None)
        );

        Assert.Contains(
            "Moonshot AI Partial Mode cannot be combined with JSON object response format.",
            error.Message
        );
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow Partial Mode with JSON schema response format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_allow_Partial_Mode_with_JSON_schema_response_format()
    {
        var (model, handler) = Model("kimi-k3");
        var call = Messages(Partial("{", "{\"partial\":true}"));
        call.JsonSchema = GoogleUpstream.Element(
            "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}}}"
        );
        call.JsonSchemaName = "result";

        await model.DoGenerateAsync(call, CancellationToken.None);

        var body = Body(handler);
        Match(body["messages"]![0], "{\"partial\":true}");
        Match(body["response_format"], "{\"type\":\"json_schema\"}");
    }
}
