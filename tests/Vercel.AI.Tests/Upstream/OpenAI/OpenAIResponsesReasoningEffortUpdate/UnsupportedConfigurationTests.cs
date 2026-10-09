// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate.ReasoningEffortUpdateSupport;

namespace Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate;

/// <summary>Port of <c>openai-responses-reasoning-effort-update.test.ts</c> &gt; <c>unsupported configuration: $model $options</c>.</summary>
public sealed class UnsupportedConfigurationTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-reasoning-effort-update.test.ts::positioned reasoning effort updates (%s) > unsupported configuration: $model $options::";

    private const string PartialNote = "The port has no reasoningMode or contextManagement, so those three cases are not ported. Needs a new ticket.";

    public static TheoryData<string, string, string> Cases()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "gpt-5.6", "{}");
            data.Add(method, "custom-model", "{\"forceReasoning\":true}");
            data.Add(method, "gpt-6-astra", "{\"truncation\":\"auto\"}");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "rejects historical updates before sending and preserves caller input", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Rejects_historical_updates_before_sending_and_preserves_caller_input(string method, string model, string optionsJson)
    {
        var prompt = Build(new[] { "high", "user" });
        var originalPrompt = prompt.ToArray();
        var options = JsonNode.Parse(optionsJson)!.AsObject();
        var capture = new OpenAICapture();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => Request(method, prompt, options, model, capture: capture));

        Assert.Equal("Message-level reasoningEffortUpdate", error.Functionality);
        Assert.Equal(string.Empty, capture.Body);
        Assert.Equal(originalPrompt, prompt);
        Assert.Equal(optionsJson, options.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "preserves legacy request-level warn-and-omit behavior", Coverage = UpstreamCoverage.Partial, Note = PartialNote)]
    public async Task Preserves_legacy_request_level_warn_and_omit_behavior(string method, string model, string optionsJson)
    {
        var options = JsonNode.Parse(optionsJson)!.AsObject();
        options["reasoningEffortUpdate"] = "low";

        var (body, warnings) = await Request(method, new ModelMessage[] { User() }, options, model);

        OpenAIUpstream.Equal(body["input"], "[" + WireUser + "]");
        Assert.Single(warnings, w => w.Type == "unsupported" && w.Message.StartsWith("reasoningEffortUpdate", StringComparison.Ordinal));
    }
}
