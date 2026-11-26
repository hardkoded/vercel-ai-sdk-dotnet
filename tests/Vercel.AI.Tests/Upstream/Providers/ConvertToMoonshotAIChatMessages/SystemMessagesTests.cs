// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToMoonshotAIChatMessages.Helpers;

namespace Vercel.AI.Tests.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>system messages</c>.</summary>
public sealed class SystemMessagesTests
{
    private const string Prefix = Source + "system messages::";

    private const string CalculatorTools =
        "{\"moonshotai\":{\"tools\":[{\"type\":\"function\",\"name\":\"calculator\","
        + "\"inputSchema\":{\"type\":\"object\",\"properties\":{}}}]}}";

    private static MoonshotChatMessages.Result Convert(string modelId, params ModelMessage[] prompt)
    {
        return MoonshotChatMessages.ConvertToMoonshotAIChatMessages(prompt, modelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass system messages through",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_system_messages_through()
    {
        AssertJson(
            "[{\"role\":\"system\",\"content\":\"You are Kimi.\"}]",
            ConvertMessages(new SystemModelMessage("You are Kimi."))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize and normalize K3 dynamic tools without content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_and_normalize_K3_dynamic_tools_without_content()
    {
        var result = Convert(
            "kimi-k3",
            new SystemModelMessage(
                "",
                Options(
                    "{\"moonshotai\":{\"tools\":[{\"type\":\"function\",\"name\":\"locate\","
                        + "\"description\":\"Locate coordinates\",\"inputSchema\":{\"type\":\"object\","
                        + "\"properties\":{\"coordinates\":{\"type\":\"array\","
                        + "\"items\":[{\"type\":\"number\"},{\"type\":\"number\"}]}}}}]}}"
                )
            )
        );

        AssertJson(
            "[{\"role\":\"system\",\"tools\":[{\"type\":\"function\",\"function\":{\"name\":\"locate\","
                + "\"description\":\"Locate coordinates\",\"parameters\":{\"type\":\"object\","
                + "\"properties\":{\"coordinates\":{\"type\":\"array\","
                + "\"prefixItems\":[{\"type\":\"number\"},{\"type\":\"number\"}]}}}}}]}]",
            result.Messages
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit dynamic tools and warn for known non-K3 models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_dynamic_tools_and_warn_for_known_non_K3_models()
    {
        var result = Convert(
            "kimi-k2.6",
            new UserModelMessage("Start"),
            new SystemModelMessage("", Options(CalculatorTools))
        );

        AssertJson("[{\"role\":\"user\",\"content\":\"Start\"}]", result.Messages);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("dynamic tool loading for model \"kimi-k2.6\"", warning.Feature);
        Assert.Equal(
            "Moonshot documents dynamic tool loading only for Kimi K3. The dynamic system message has been omitted.",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve dynamic tools for unknown custom models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_dynamic_tools_for_unknown_custom_models()
    {
        var result = Convert("custom-model", new SystemModelMessage("", Options(CalculatorTools)));

        Assert.Empty(result.Warnings);
        Assert.Equal("system", result.Messages[0]!["role"]!.GetValue<string>());
        Assert.Equal(
            "calculator",
            result.Messages[0]!["tools"]![0]!["function"]!["name"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve an ordinary system message when tools is empty",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_an_ordinary_system_message_when_tools_is_empty()
    {
        var result = Convert(
            "kimi-k3",
            new SystemModelMessage("You are Kimi.", Options("{\"moonshotai\":{\"tools\":[]}}"))
        );

        AssertJson("[{\"role\":\"system\",\"content\":\"You are Kimi.\"}]", result.Messages);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject incomplete dynamic tool definitions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_incomplete_dynamic_tool_definitions()
    {
        var error = Assert.Throws<InvalidArgumentException>(() =>
            Convert(
                "kimi-k3",
                new SystemModelMessage(
                    "",
                    Options(
                        "{\"moonshotai\":{\"tools\":[{\"type\":\"function\",\"name\":\"calculator\"}]}}"
                    )
                )
            )
        );
        Assert.Equal("providerOptions", error.Parameter);
        Assert.Contains("invalid moonshotai provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject content alongside dynamic tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_content_alongside_dynamic_tools()
    {
        var error = Assert.Throws<InvalidPromptException>(() =>
            Convert(
                "kimi-k3",
                new SystemModelMessage("Do not send this.", Options(CalculatorTools))
            )
        );
        Assert.Contains(
            "A Moonshot dynamic-tool system message must use empty content because the API forbids content alongside tools.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject dynamic tools on non-system messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_dynamic_tools_on_non_system_messages()
    {
        var error = Assert.Throws<InvalidPromptException>(() =>
            Convert(
                "kimi-k3",
                new AssistantModelMessage("Hello", null, null)
                {
                    ProviderOptions = Options(CalculatorTools),
                }
            )
        );
        Assert.Contains(
            "Moonshot dynamic tools must be configured on a system message.",
            error.Message
        );
    }
}
