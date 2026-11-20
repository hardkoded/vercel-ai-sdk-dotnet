// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>foreign provider history</c>.</summary>
public sealed class ForeignProviderHistoryTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::foreign provider history::";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document
            .RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    [Theory]
    [InlineData("google")]
    [InlineData("vertex")]
    [InlineData("googleVertex")]
    [UpstreamTest(
        Prefix + "preserves message cache control and trims prefill before empty %s text",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public void Preserves_message_cache_control_and_trims_prefill_before_empty_text(string ns)
    {
        var result = AnthropicPrompt.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantTextPart("Answer  "),
                        new AssistantTextPart(
                            "",
                            Options("{\"" + ns + "\":{\"thoughtSignature\":\"opaque\"}}")
                        ),
                    }
                )
                {
                    ProviderOptions = Options(
                        "{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}"
                    ),
                },
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "[{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Answer\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]"
                ),
                result.Messages
            ),
            result.Messages.ToJsonString()
        );
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("google")]
    [InlineData("vertex")]
    [InlineData("googleVertex")]
    [UpstreamTest(
        Prefix + "omits empty %s signature text from assistant content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public void Omits_empty_signature_text_from_assistant_content(string ns)
    {
        var result = AnthropicPrompt.Convert(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantTextPart("A"),
                        new AssistantTextPart(
                            "",
                            Options("{\"" + ns + "\":{\"thoughtSignature\":\"opaque\"}}")
                        ),
                        new AssistantTextPart("B"),
                    }
                ),
            }
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "[{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"A\"},{\"type\":\"text\",\"text\":\"B\"}]}]"
                ),
                result.Messages
            ),
            result.Messages.ToJsonString()
        );
    }
}
