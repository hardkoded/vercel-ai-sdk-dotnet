// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; named tool choice with inline tools (%s) &gt; $name</c>.</summary>
public sealed class NamedToolChoiceWithInlineToolsNameTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > named tool choice with inline tools (%s) > $name::";

    private static readonly string[] InlineTools =
    {
        "{\"type\":\"function\",\"name\":\"other\",\"inputSchema\":{\"type\":\"object\"}}",
        "{\"type\":\"provider\",\"id\":\"anthropic.web_search_20250305\",\"name\":\"web_search\",\"args\":{}}",
        "{\"type\":\"provider\",\"id\":\"anthropic.web_fetch_20250910\",\"name\":\"web_fetch\",\"args\":{}}",
        "{\"type\":\"provider\",\"id\":\"anthropic.web_search_20250305\",\"name\":\"selected\",\"args\":{}}",
    };

    public static IEnumerable<object[]> Cases()
    {
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var tool in InlineTools)
            {
                yield return new object[] { method, tool, "claude-haiku-4-5", "providerOptions" };
                yield return new object[] { method, tool, "claude-haiku-4-5", "reasoning" };
                yield return new object[] { method, tool, "claude-opus-5-5", "model" };
                yield return new object[] { method, tool, "claude-haiku-4-5", "inlineOnly" };
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "should reject conflicting additions before sending a request for $modelId via $source", Coverage = UpstreamCoverage.Covered,
        Commit = "ba302a14fcd98fbfa363130a53d46d7b61d22cff")]
    public async Task Should_reject_conflicting_additions_before_sending_a_request_for_modelId_via_source(string method, string inlineTool, string modelId, string source)
    {
        var prompt = new List<ModelMessage>
        {
            new UserModelMessage("Hello"),
            InlineToolHarness.ToolChange(inlineTool),
        };
        // Also check definitions replayed from earlier turns.
        if (source != "providerOptions")
        {
            prompt.Add(new AssistantModelMessage("OK.", null, null));
            prompt.Add(new UserModelMessage("Hello"));
        }

        var options = new LanguageModelCallOptions
        {
            Prompt = prompt,
            ToolChoice = ToolChoice.Tool("selected"),
        };
        if (source != "inlineOnly")
        {
            options.Tools = new[] { InlineToolHarness.SelectedTool() };
        }

        if (source == "reasoning")
        {
            options.Reasoning = "low";
        }
        else if (source != "model")
        {
            options.ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["anthropic"] = AnthropicParity.Json("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1024}}"),
            };
        }

        var handler = AnthropicParity.Ok();
        var model = AnthropicParity.Client(handler).LanguageModel(modelId);

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
}
