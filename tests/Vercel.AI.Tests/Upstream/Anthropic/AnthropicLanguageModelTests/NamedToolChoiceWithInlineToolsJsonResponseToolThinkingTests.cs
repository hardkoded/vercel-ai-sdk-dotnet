// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; named tool choice with inline tools (%s) &gt; JSON response tool (thinking %s)</c>.</summary>
public sealed class NamedToolChoiceWithInlineToolsJsonResponseToolThinkingTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > named tool choice with inline tools (%s) > JSON response tool (thinking %s)::";

    private const string ProviderTool = "{\"type\":\"provider\",\"id\":\"anthropic.web_fetch_20250910\",\"name\":\"selected\",\"args\":{}}";

    public static IEnumerable<object[]> RejectCases()
    {
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var useThinking in new[] { false, true })
            {
                yield return new object[] { method, useThinking, "tool", false, false };
                yield return new object[] { method, useThinking, "none", false, false };
                yield return new object[] { method, useThinking, "tool", true, false };
                yield return new object[] { method, useThinking, "tool", false, true };
                yield return new object[] { method, useThinking, "tool", true, true };
            }
        }
    }

    public static IEnumerable<object[]> PreserveCases()
    {
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var useThinking in new[] { false, true })
            {
                yield return new object[] { method, useThinking, "tool", false, false };
                yield return new object[] { method, useThinking, "auto", false, false };
                yield return new object[] { method, useThinking, "required", false, false };
                yield return new object[] { method, useThinking, "tool", true, false };
                yield return new object[] { method, useThinking, "tool", false, true };
                yield return new object[] { method, useThinking, "tool", true, true };
            }
        }
    }

    private static LanguageModelCallOptions Options(IReadOnlyList<ModelMessage> prompt, string providerToolsJson, LanguageModelTool[] tools, string choice, bool useThinking)
    {
        return new LanguageModelCallOptions
        {
            Prompt = prompt,
            Tools = tools,
            ToolChoice = choice == "tool" ? ToolChoice.Tool("selected") : choice == "none" ? ToolChoice.None : choice == "auto" ? ToolChoice.Auto : ToolChoice.Required,
            JsonResponse = true,
            JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}"),
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["anthropic"] = AnthropicParity.Json(
                    "{\"structuredOutputMode\":\"jsonTool\""
                    + (useThinking ? ",\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1024}" : string.Empty)
                    + (providerToolsJson.Length > 0 ? ",\"providerTools\":[" + providerToolsJson + "]" : string.Empty)
                    + "}"),
            },
        };
    }

    [Theory]
    [MemberData(nameof(RejectCases))]
    [UpstreamTest(Prefix + "should reject conflicting inline tools with $choice choice, providerAlias=$providerAlias and reuseSelectedName=$reuseSelectedName before sending a request", Coverage = UpstreamCoverage.Covered,
        Commit = "ba302a14fcd98fbfa363130a53d46d7b61d22cff")]
    public async Task Should_reject_conflicting_inline_tools_with_choice_choice_providerAlias_and_reuseSelectedName_before_sending_a_request(string method, bool useThinking, string choice, bool providerAlias, bool reuseSelectedName)
    {
        var inlineTool = JsonNode.Parse(ProviderTool)!.AsObject();
        if (reuseSelectedName)
        {
            inlineTool["id"] = "anthropic.web_search_20250305";
        }

        inlineTool["name"] = reuseSelectedName ? "selected" : providerAlias ? "other" : "web_fetch";
        var prompt = new ModelMessage[] { new UserModelMessage("Hello"), InlineToolHarness.ToolChange(inlineTool.ToJsonString()) };
        var options = Options(
            prompt,
            providerAlias ? ProviderTool : string.Empty,
            providerAlias ? Array.Empty<LanguageModelTool>() : new[] { InlineToolHarness.SelectedTool() },
            choice,
            useThinking);

        var handler = AnthropicParity.Ok();
        var model = AnthropicParity.Client(handler).LanguageModel("claude-opus-4-8");

        await Assert.ThrowsAsync<InvalidArgumentException>(async () =>
        {
            if (method == "generate")
            {
                await model.DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                await model.DoStreamAsync(options, CancellationToken.None).ConfigureAwait(false);
            }
        });
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(PreserveCases))]
    [UpstreamTest(Prefix + "should preserve allowed inline tools with $choice choice, providerAlias=$providerAlias and inlineOnly=$inlineOnly", Coverage = UpstreamCoverage.Covered,
        Commit = "ba302a14fcd98fbfa363130a53d46d7b61d22cff")]
    public async Task Should_preserve_allowed_inline_tools_with_choice_choice_providerAlias_and_inlineOnly(string method, bool useThinking, string choice, bool providerAlias, bool inlineOnly)
    {
        var inlineTool = providerAlias
            ? "{\"type\":\"provider\",\"id\":\"anthropic.web_search_20250305\",\"name\":\"selected\",\"args\":{}}"
            : InlineToolHarness.Function(choice == "tool" ? "selected" : "other");
        var prompt = new ModelMessage[] { new UserModelMessage("Hello"), InlineToolHarness.ToolChange(inlineTool) };
        var tools = inlineOnly || providerAlias ? Array.Empty<LanguageModelTool>() : new[] { InlineToolHarness.SelectedTool() };
        var options = Options(prompt, !inlineOnly && providerAlias ? inlineTool : string.Empty, tools, choice, useThinking);

        var (handler, _) = await InlineToolHarness.Run(method, "claude-opus-4-8", options, jsonTool: true);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"" + (useThinking ? "auto" : "any") + "\",\"disable_parallel_tool_use\":true}"),
            body["tool_choice"]));
        Assert.Equal(
            inlineOnly ? new[] { "json" } : new[] { providerAlias ? "web_search" : "selected", "json" },
            body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray());
        var part = body["messages"]![1]!["content"]![0]!;
        Assert.Equal("tool_addition", part["type"]!.GetValue<string>());
        Assert.Equal("tool_definition", part["tool"]!["type"]!.GetValue<string>());
        Assert.Equal(
            providerAlias ? "web_search" : JsonNode.Parse(inlineTool)!["name"]!.GetValue<string>(),
            part["tool"]!["definition"]!["name"]!.GetValue<string>());
    }
}
