// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt.PromptTestSupport;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; system message</c>.</summary>
public sealed class SystemMessageTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > system message::";

    private static async Task<IReadOnlyList<ModelMessage>> ConvertAsync(Instructions instructions)
    {
        var messages = new List<ModelMessage>();
        messages.AddRange(Generation.InstructionMessages(instructions));
        messages.Add(new UserModelMessage("Hello, world!"));
        return await PromptTestSupport.ConvertAsync(messages);
    }

    private static void AssertUserHello(ModelMessage message)
    {
        var user = Assert.IsType<UserModelMessage>(message);
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text
        );
        Assert.Null(user.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert a string system message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_a_string_system_message()
    {
        var result = await ConvertAsync("INSTRUCTIONS");

        Assert.Equal(2, result.Count);
        var system = Assert.IsType<SystemModelMessage>(result[0]);
        Assert.Equal("INSTRUCTIONS", system.Content);
        Assert.Null(system.ProviderOptions);
        AssertUserHello(result[1]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert a SystemModelMessage system message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_a_SystemModelMessage_system_message()
    {
        var result = await ConvertAsync(
            new SystemModelMessage(
                "INSTRUCTIONS",
                new Dictionary<string, JsonElement>
                {
                    ["test"] = JsonDocument.Parse("{\"value\":\"test\"}").RootElement.Clone(),
                }
            )
        );

        Assert.Equal(2, result.Count);
        var system = Assert.IsType<SystemModelMessage>(result[0]);
        Assert.Equal("INSTRUCTIONS", system.Content);
        Assert.Equal("{\"value\":\"test\"}", system.ProviderOptions!["test"].GetRawText());
        AssertUserHello(result[1]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert an array of SystemModelMessage system messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_an_array_of_SystemModelMessage_system_messages()
    {
        var result = await ConvertAsync(
            new[]
            {
                new SystemModelMessage("INSTRUCTIONS"),
                new SystemModelMessage("INSTRUCTIONS 2"),
            }
        );

        Assert.Equal(3, result.Count);
        var first = Assert.IsType<SystemModelMessage>(result[0]);
        Assert.Equal("INSTRUCTIONS", first.Content);
        Assert.Null(first.ProviderOptions);
        var second = Assert.IsType<SystemModelMessage>(result[1]);
        Assert.Equal("INSTRUCTIONS 2", second.Content);
        Assert.Null(second.ProviderOptions);
        AssertUserHello(result[2]);
    }
}
