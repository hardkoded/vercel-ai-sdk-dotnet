// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.ConvertToOpenAIChatMessages;

/// <summary>Port of <c>convert-to-openai-chat-messages.test.ts</c> &gt; <c>message names</c>.</summary>
public sealed class MessageNamesTests
{
    private const string Prefix = "packages/openai/src/chat/convert-to-openai-chat-messages.test.ts::";

    [Fact]
    [UpstreamTest(Prefix + "message names::preserves names on plain user and assistant messages", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void PreservesNamesOnPlainUserAndAssistantMessages()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("user") { Parts = new[] { Text("Hello") }, ProviderOptions = Named("alice") },
            new OpenAIChatPromptMessage("assistant") { Parts = new[] { Text("Hi") }, ProviderOptions = Named("bot") },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"name\":\"alice\",\"content\":\"Hello\"},{\"role\":\"assistant\",\"name\":\"bot\",\"content\":\"Hi\"}]");
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "message names::preserves names on user parts and assistant tool-only messages", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void PreservesNamesOnUserPartsAndAssistantToolOnlyMessages()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("user") { Parts = new[] { Text("Hello"), Text(" there") }, ProviderOptions = Named("alice") },
            new OpenAIChatPromptMessage("assistant")
            {
                ToolCalls = new[] { new GeneratedToolCall("call-1", "lookup", "{}") },
                ProviderOptions = Named("bot"),
            },
        });
        OpenAIUpstream.Equal(
            result.Messages,
            "[{\"role\":\"user\",\"name\":\"alice\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"},{\"type\":\"text\",\"text\":\" there\"}]},{\"role\":\"assistant\",\"name\":\"bot\",\"content\":null,\"tool_calls\":[{\"id\":\"call-1\",\"type\":\"function\",\"function\":{\"name\":\"lookup\",\"arguments\":\"{}\"}}]}]");
    }

    [Theory]
    [InlineData("system")]
    [InlineData("developer")]
    [UpstreamTest(Prefix + "message names::preserves names and cache breakpoints in %s mode", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void PreservesNamesAndCacheBreakpointsInSystemMode(string systemMessageMode)
    {
        var result = OpenAIChatMessages.Convert(
            new[]
            {
                new OpenAIChatPromptMessage("system")
                {
                    Text = "Be helpful",
                    ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"name\":\"instructions\",\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}"),
                },
                new OpenAIChatPromptMessage("assistant") { Parts = new[] { Text("Hi", Breakpoint()) }, ProviderOptions = Named("bot") },
            },
            systemMessageMode);
        OpenAIUpstream.Equal(
            result.Messages,
            "[{\"role\":\"" + systemMessageMode + "\",\"name\":\"instructions\",\"content\":[{\"type\":\"text\",\"text\":\"Be helpful\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]},{\"role\":\"assistant\",\"name\":\"bot\",\"content\":[{\"type\":\"text\",\"text\":\"Hi\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "message names::removes named system messages in remove mode", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void RemovesNamedSystemMessagesInRemoveMode()
    {
        var result = OpenAIChatMessages.Convert(
            new[] { new OpenAIChatPromptMessage("system") { Text = "Be helpful", ProviderOptions = Named("instructions") } },
            "remove");
        Assert.Empty(result.Messages);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("system messages are removed for this model", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "message names::warns and omits names on tool messages", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void WarnsAndOmitsNamesOnToolMessages()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("tool") { ToolCallId = "call-1", ToolOutput = "result", MessageProviderOptions = new Dictionary<string, JsonElement> { ["openai"] = OpenAIUpstream.Json("{\"name\":\"ignored\"}") } },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"tool\",\"tool_call_id\":\"call-1\",\"content\":\"result\"}]");
        Assert.Single(result.Warnings);
        Assert.Equal("unsupported", result.Warnings[0].Type);
        Assert.Equal("message name on tool messages", result.Warnings[0].Feature);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("{}")]
    [InlineData("[]")]
    [UpstreamTest(Prefix + "message names::rejects invalid names: %j", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void RejectsInvalidNames(string name)
    {
        var error = Assert.Throws<InvalidArgumentException>(() => OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("user")
            {
                Parts = new[] { Text("Hello") },
                ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"name\":" + name + "}}"),
            },
        }));
        Assert.Contains("invalid openai provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "message names::ignores names from unrelated namespaces and content parts", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public void IgnoresNamesFromUnrelatedNamespacesAndContentParts()
    {
        var result = OpenAIChatMessages.Convert(new[]
        {
            new OpenAIChatPromptMessage("user")
            {
                Parts = new[] { Text("Hello", Named("part-name")) },
                ProviderOptions = OpenAIUpstream.Json("{\"other\":{\"name\":\"other-name\"}}"),
            },
        });
        OpenAIUpstream.Equal(result.Messages, "[{\"role\":\"user\",\"content\":\"Hello\"}]");
    }

    private static JsonElement Named(string name)
    {
        return OpenAIUpstream.Json("{\"openai\":{\"name\":\"" + name + "\"}}");
    }

    private static OpenAIChatPromptPart Text(string text, JsonElement? providerOptions = null)
    {
        return new OpenAIChatPromptPart("text") { Text = text, ProviderOptions = providerOptions };
    }

    private static JsonElement Breakpoint()
    {
        return OpenAIUpstream.Json("{\"openai\":{\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}");
    }
}
