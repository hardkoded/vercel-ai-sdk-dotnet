// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class DecisionTypesTests
{
    [Theory]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("null")]
    public void DecisionInput_rejects_JSON_that_is_not_a_string_object_or_array(string json)
    {
        using var document = JsonDocument.Parse(json);
        Assert.Throws<InvalidArgumentException>(() => DecisionInput.FromJson(document.RootElement));
    }

    [Theory]
    [InlineData("\"text\"", "text")]
    [InlineData("{\"a\":1}", "{\"a\":1}")]
    [InlineData("[\"a\"]", "[\"a\"]")]
    public void DecisionInput_keeps_strings_objects_and_arrays(string json, string expected)
    {
        Assert.Equal(expected, DecisionInput.Parse(json).ToString());
    }

    [Fact]
    public void DecisionState_rejects_a_JSON_array()
    {
        using var document = JsonDocument.Parse("[1]");
        Assert.Throws<InvalidArgumentException>(() => DecisionState.FromJson(document.RootElement));
    }

    [Fact]
    public void Boolean_criteria_send_only_the_sides_that_were_set()
    {
        Assert.Equal("{}", new DecisionBooleanCriteria().ToNode().ToJsonString());
        Assert.Equal("{\"true\":null}", new DecisionBooleanCriteria { True = null }.ToNode().ToJsonString());
        Assert.Equal("{\"true\":\"yes\",\"false\":null}", new DecisionBooleanCriteria { True = "yes", False = null }.ToNode().ToJsonString());
    }

    [Fact]
    public async Task PrepareDecisionState_turns_text_and_a_JSON_object_into_one_part()
    {
        var text = Assert.IsType<DecisionModelTextPart>(Assert.Single(await DecisionState.PrepareDecisionStateAsync("hello")));
        Assert.Equal("hello", text.Text);

        using var document = JsonDocument.Parse("{\"a\":1}");
        var json = Assert.IsType<DecisionModelJsonPart>(Assert.Single(await DecisionState.PrepareDecisionStateAsync(DecisionState.FromJson(document.RootElement))));
        Assert.Equal("{\"a\":1}", json.Value.GetRawText());
    }

    [Fact]
    public void StandardizePrompt_rejects_a_prompt_together_with_messages()
    {
        Assert.Throws<InvalidPromptException>(() => Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Prompt = "hello",
            Messages = new ModelMessage[] { new UserModelMessage("hi") },
        }));
    }

    [Fact]
    public void StandardizePrompt_rejects_a_missing_prompt()
    {
        Assert.Throws<InvalidPromptException>(() => Prompts.StandardizePrompt(new StandardizePromptInput()));
    }

    [Fact]
    public void Instructions_reject_a_null_system_message_in_a_list()
    {
        Assert.Throws<InvalidPromptException>(() => Instructions.From(new SystemModelMessage[] { null! }));
    }
}
