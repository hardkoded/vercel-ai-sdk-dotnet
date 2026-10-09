// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate.ReasoningEffortUpdateSupport;

namespace Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate;

/// <summary>Port of <c>openai-responses-reasoning-effort-update.test.ts</c> &gt; <c>positioned reasoning effort updates (%s)</c>.</summary>
public sealed class PositionedReasoningEffortUpdatesTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-reasoning-effort-update.test.ts::positioned reasoning effort updates (%s)::";

    private const string AstraEfforts = "gpt-6-astra only supports the following reasoning efforts: low, medium, high, xhigh, max";

    public static TheoryData<string> Methods()
    {
        var data = new TheoryData<string>();
        data.Add("generate");
        data.Add("stream");
        return data;
    }

    public static TheoryData<string, string> MethodAndModel()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var model in new[] { "gpt-6-astra", "gpt-6-sol", "gpt-6-luna" })
            {
                data.Add(method, model);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(MethodAndModel))]
    [UpstreamTest(Prefix + "retains multiple updates among messages for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Retains_multiple_updates_among_messages(string method, string modelId)
    {
        var (body, warnings) = await Request(
            method,
            new ModelMessage[]
            {
                User(),
                new AssistantModelMessage("Answer", null, null),
                Update("high"),
                User(),
                Update("low"),
                User(),
            },
            new JsonObject { ["reasoningEffort"] = "medium", ["store"] = false },
            modelId);

        OpenAIUpstream.Equal(
            body["input"],
            "[" + WireUser + ",{\"role\":\"assistant\",\"content\":\"Answer\"}," + WireUpdate("high") + "," + WireUser + "," + WireUpdate("low") + "," + WireUser + "]");
        Assert.Equal("medium", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "rejects historical none updates for Astra before sending", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_historical_none_updates_for_Astra_before_sending(string method)
    {
        var prompt = Build(new[] { "user", "none", "user" });
        var original = prompt.ToArray();
        var capture = new OpenAICapture();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => Request(method, prompt, new JsonObject(), "gpt-6-astra", capture: capture));

        Assert.Equal("Message-level reasoningEffortUpdate", error.Functionality);
        Assert.Equal(AstraEfforts, error.Message);
        Assert.Equal(string.Empty, capture.Body);
        Assert.Equal(original, prompt);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "warns and omits request-level none updates for Astra while retaining valid history", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_and_omits_request_level_none_updates_for_Astra_while_retaining_valid_history(string method)
    {
        var (body, warnings) = await Request(
            method,
            Build(new[] { "user", "low", "user" }),
            new JsonObject { ["reasoningEffort"] = "medium", ["reasoningEffortUpdate"] = "none" },
            "gpt-6-astra");

        OpenAIUpstream.Equal(body["input"], "[" + WireUser + "," + WireUpdate("low") + "," + WireUser + "]");
        Assert.Equal("medium", body["reasoning"]!["effort"]!.GetValue<string>());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(AstraEfforts, warning.Message);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "keeps the request-level update prepended and historical updates positioned", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_the_request_level_update_prepended_and_historical_updates_positioned(string method)
    {
        var (body, warnings) = await Request(
            method,
            Build(new[] { "user", "high", "user" }),
            new JsonObject { ["reasoningEffort"] = "low", ["reasoningEffortUpdate"] = "medium" },
            "gpt-6-astra");

        OpenAIUpstream.Equal(
            body["input"],
            "[" + WireUpdate("medium") + "," + WireUser + "," + WireUpdate("high") + "," + WireUser + "]");
        Assert.Equal("low", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "uses an identical first historical update without prepending a duplicate", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_an_identical_first_historical_update_without_prepending_a_duplicate(string method)
    {
        var prompt = Build(new[] { "high", "user" });
        var original = prompt.ToArray();

        var (body, warnings) = await Request(
            method,
            prompt,
            new JsonObject { ["reasoningEffort"] = "low", ["reasoningSummary"] = "concise", ["reasoningEffortUpdate"] = "high" },
            "gpt-6-astra");

        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("high") + "," + WireUser + "]");
        OpenAIUpstream.Equal(body["reasoning"], "{\"effort\":\"low\",\"summary\":\"concise\"}");
        Assert.Empty(warnings);
        Assert.Equal(original, prompt);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "checks the first item after system text has been removed", Coverage = UpstreamCoverage.Covered)]
    public async Task Checks_the_first_item_after_system_text_has_been_removed(string method)
    {
        var (body, warnings) = await Request(
            method,
            new ModelMessage[] { new SystemModelMessage("Removed"), Update("high"), User() },
            new JsonObject { ["reasoningEffortUpdate"] = "high", ["systemMessageMode"] = "remove" },
            "gpt-6-astra");

        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("high") + "," + WireUser + "]");
        var warning = Assert.Single(warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("system messages are removed for this model", warning.Message);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "prepends when an identical historical update is not the first item", Coverage = UpstreamCoverage.Covered)]
    public async Task Prepends_when_an_identical_historical_update_is_not_the_first_item(string method)
    {
        var (body, warnings) = await Request(
            method,
            Build(new[] { "user", "high", "user" }),
            new JsonObject { ["reasoningEffortUpdate"] = "high" },
            "gpt-6-astra");

        OpenAIUpstream.Equal(
            body["input"],
            "[" + WireUpdate("high") + "," + WireUser + "," + WireUpdate("high") + "," + WireUser + "]");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "preserves request-level continuation behavior", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_request_level_continuation_behavior(string method)
    {
        var (body, warnings) = await Request(
            method,
            new ModelMessage[] { User() },
            new JsonObject { ["previousResponseId"] = "resp_previous", ["reasoningEffort"] = "low", ["reasoningEffortUpdate"] = "high" },
            "gpt-6-astra");

        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("high") + "," + WireUser + "]");
        Assert.Equal("resp_previous", body["previous_response_id"]!.GetValue<string>());
        Assert.Equal("low", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    public static TheoryData<string, string> MixedText()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "Instructions");
            data.Add(method, " ");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(MixedText))]
    [UpstreamTest(Prefix + "rejects mixed text %j before sending", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_mixed_text_before_sending(string method, string content)
    {
        var capture = new OpenAICapture();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => Request(method, new ModelMessage[] { Update("high", content), User() }, new JsonObject(), "gpt-6-astra", capture: capture));

        Assert.Equal("Message-level reasoningEffortUpdate", error.Functionality);
        Assert.Equal("Message-level reasoningEffortUpdate requires empty system message content.", error.Message);
        Assert.Equal(string.Empty, capture.Body);
    }

    public static TheoryData<string, string> SystemModes()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var mode in new[] { "system", "developer", "remove" })
            {
                data.Add(method, mode);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SystemModes))]
    [UpstreamTest(Prefix + "emits controls independently of systemMessageMode=%s", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_controls_independently_of_systemMessageMode(string method, string systemMessageMode)
    {
        var (body, warnings) = await Request(
            method,
            Build(new[] { "high", "user" }),
            new JsonObject { ["systemMessageMode"] = systemMessageMode },
            "gpt-6-astra");

        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("high") + "," + WireUser + "]");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "preserves ordinary empty system messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_ordinary_empty_system_messages(string method)
    {
        var (body, _) = await Request(
            method,
            new ModelMessage[] { new SystemModelMessage(string.Empty), User() },
            new JsonObject(),
            "gpt-6-astra");

        OpenAIUpstream.Equal(body["input"], "[{\"role\":\"developer\",\"content\":\"\"}," + WireUser + "]");
    }

    public static TheoryData<string, string, string[]> AdjacentCases()
    {
        var data = new TheoryData<string, string, string[]>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "{}", new[] { "high", "low", "user" });
            data.Add(method, "{}", new[] { "high", "high", "user" });
            data.Add(method, "{\"reasoningEffortUpdate\":\"high\"}", new[] { "high", "high", "user" });
            data.Add(method, "{\"reasoningEffortUpdate\":\"low\"}", new[] { "high", "user" });
            data.Add(method, "{\"systemMessageMode\":\"remove\"}", new[] { "high", "system", "low", "user" });
            data.Add(method, "{\"systemMessageMode\":\"remove\"}", new[] { "high", "system", "high", "user" });
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AdjacentCases))]
    [UpstreamTest(Prefix + "rejects adjacent serialized updates before sending: $options", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_adjacent_serialized_updates_before_sending(string method, string optionsJson, string[] kinds)
    {
        var prompt = new List<ModelMessage>();
        foreach (var kind in kinds)
        {
            prompt.Add(kind switch
            {
                "user" => User(),
                "system" => new SystemModelMessage("Removed"),
                _ => Update(kind),
            });
        }

        var capture = new OpenAICapture();

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => Request(method, prompt, JsonNode.Parse(optionsJson)!.AsObject(), "gpt-6-astra", capture: capture));

        Assert.Equal("Adjacent reasoning effort configuration updates", error.Functionality);
        Assert.Equal(string.Empty, capture.Body);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "rejects minimal message-level updates even on Luna", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_minimal_message_level_updates_even_on_Luna(string method)
    {
        var capture = new OpenAICapture();
        var minimal = new SystemModelMessage(string.Empty, new Dictionary<string, JsonElement>
        {
            ["openai"] = OpenAIUpstream.Json("{\"reasoningEffortUpdate\":\"minimal\"}"),
        });

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(
            () => Request(method, new ModelMessage[] { minimal, User() }, new JsonObject(), "gpt-6-luna", capture: capture));

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Equal("Invalid argument for parameter providerOptions: invalid openai provider options", error.Message);
        Assert.Equal(string.Empty, capture.Body);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "rejects minimal request-level updates even on Luna", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_minimal_request_level_updates_even_on_Luna(string method)
    {
        var capture = new OpenAICapture();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(
            () => Request(method, new ModelMessage[] { User() }, new JsonObject { ["reasoningEffortUpdate"] = "minimal" }, "gpt-6-luna", capture: capture));

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Equal("Invalid argument for parameter providerOptions: invalid openai provider options", error.Message);
        Assert.Equal(string.Empty, capture.Body);
    }
}
