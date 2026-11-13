// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>system messages</c> &gt; <c>effort-only system messages</c>.</summary>
public sealed class EffortOnlySystemMessagesTests
{
    private const string PrefixEffort = "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages > effort-only system messages::";

    private const string BetaSystem = "mid-conversation-system-2026-04-07";
    private const string BetaOutputConfig = "mid-conversation-output-config-2026-07-01";

    private static readonly SystemModelMessage Instruction = new("initial");

    private const string UserJson = "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"hi\"}]}";

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), "Expected " + expected + " but was " + (actual?.ToJsonString() ?? "null"));

    private static AnthropicPromptResult Run(params ModelMessage[] prompt) => AnthropicPrompt.Convert(prompt);

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    private static SystemModelMessage Effort(string effort) =>
        new SystemModelMessage(string.Empty, Options("{\"anthropic\":{\"effort\":\"" + effort + "\"}}"));

    private static string EffortMessage(string effort) =>
        "{\"role\":\"system\",\"content\":[],\"output_config\":{\"effort\":\"" + effort + "\"}}";

    public static IEnumerable<object[]> InitialEffortCases()
    {
        yield return ["alone", new ModelMessage[] { Effort("low") }, false, new[] { "low" }];
        yield return ["after initial instructions", new ModelMessage[] { Instruction, Effort("low") }, true, new[] { "low" }];
        yield return ["before initial instructions", new ModelMessage[] { Effort("low"), Instruction }, true, new[] { "low" }];
        yield return ["consecutively", new ModelMessage[] { Effort("low"), Effort("high") }, false, new[] { "low", "high" }];
        yield return ["around initial instructions", new ModelMessage[] { Effort("low"), Instruction, Effort("high") }, true, new[] { "low", "high" }];
    }

    [Theory]
    [MemberData(nameof(InitialEffortCases))]
    [UpstreamTest(PrefixEffort + "should preserve initial effort messages $name", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_initial_effort_messages(string name, ModelMessage[] initial, bool hasText, string[] efforts)
    {
        Assert.NotEmpty(name);
        var result = Run([.. initial, new UserModelMessage("hi")]);

        AssertJson(hasText ? "[{\"type\":\"text\",\"text\":\"initial\"}]" : "[]", result.System);
        AssertJson("[" + string.Join(",", efforts.Select(EffortMessage)) + "," + UserJson + "]", result.Messages);
        Assert.Equal([BetaOutputConfig], result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(PrefixEffort + "should preserve later consecutive effort messages with initial instructions: %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_later_consecutive_effort_messages_with_initial_instructions(bool hasInitial)
    {
        var prompt = new List<ModelMessage>();
        if (hasInitial)
        {
            prompt.Add(Instruction);
        }

        prompt.AddRange([new UserModelMessage("hi"), Effort("low"), Effort("high"), new SystemModelMessage("later instructions")]);
        var result = AnthropicPrompt.Convert(prompt);

        if (hasInitial)
        {
            AssertJson("[{\"type\":\"text\",\"text\":\"initial\"}]", result.System);
        }
        else
        {
            Assert.Null(result.System);
        }

        AssertJson(
            "[" + UserJson + "," + EffortMessage("low") + "," + EffortMessage("high")
            + ",{\"role\":\"system\",\"content\":[{\"type\":\"text\",\"text\":\"later instructions\"}]}]",
            result.Messages);
        Assert.Equal(new[] { BetaSystem, BetaOutputConfig }.OrderBy(b => b), result.Betas.OrderBy(b => b));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("initial", "{\"effort\":\"low\"}")]
    [InlineData("", "{\"clearAt\":\"next_user_message\"}")]
    [InlineData("", "{\"clearAt\":\"next_user_message\",\"effort\":\"low\"}")]
    [UpstreamTest(PrefixEffort + "should warn and ignore unsupported initial system options: $options", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_and_ignore_unsupported_initial_system_options(string content, string options)
    {
        var result = Run(new SystemModelMessage(content, Options("{\"anthropic\":" + options + "}")), new UserModelMessage("hi"));

        AssertJson("[" + UserJson + "]", result.Messages);
        Assert.Empty(result.Betas);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Contains("These options have been ignored.", warning.Message);
    }
}
