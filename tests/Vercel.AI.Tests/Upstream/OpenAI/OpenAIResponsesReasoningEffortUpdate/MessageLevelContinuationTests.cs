// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate.ReasoningEffortUpdateSupport;

namespace Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate;

/// <summary>Port of <c>openai-responses-reasoning-effort-update.test.ts</c> &gt; <c>message-level continuation with $field</c>.</summary>
public sealed class MessageLevelContinuationTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-reasoning-effort-update.test.ts::positioned reasoning effort updates (%s) > message-level continuation with $field::";

    public static TheoryData<string, string, string> Cases()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "previousResponseId", "previous_response_id");
            data.Add(method, "conversation", "conversation");
        }

        return data;
    }

    private static JsonObject Options(string option) =>
        new() { [option] = option == "previousResponseId" ? "resp_previous" : "conv_test" };

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "preserves an update after filtering reasoning already stored in history", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_an_update_after_filtering_reasoning_already_stored_in_history(string method, string option, string field)
    {
        var options = Options(option);
        options["reasoningEffort"] = "low";

        var (body, warnings) = await Request(
            method,
            new ModelMessage[] { PreviousReasoning(), Update("high"), User() },
            options,
            "gpt-6-astra");

        Assert.Equal(option == "previousResponseId" ? "resp_previous" : "conv_test", body[field]!.GetValue<string>());
        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("high") + "," + WireUser + "]");
        Assert.Equal("low", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "rejects updates made adjacent by filtering stored reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_updates_made_adjacent_by_filtering_stored_reasoning(string method, string option, string field)
    {
        var capture = new OpenAICapture();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => Request(
                method,
                new ModelMessage[] { Update("high"), PreviousReasoning(), Update("low"), User() },
                Options(option),
                "gpt-6-astra",
                capture: capture));

        Assert.Equal("Adjacent reasoning effort configuration updates", error.Functionality);
        Assert.Equal(0, capture.Calls);
        Assert.NotEmpty(field);
    }
}
