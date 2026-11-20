// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ConvertToAmazonBedrockChatMessages;

/// <summary>Port of <c>convert-to-amazon-bedrock-chat-messages.test.ts</c> &gt; <c>foreign provider history</c>.</summary>
public sealed class ForeignProviderHistoryTests
{
    private const string ModelId = "meta.llama3-70b-instruct-v1:0";
    private const string Prefix =
        "packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::foreign provider history::";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document
            .RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    private static JsonArray Convert(params AssistantContentPart[] parts)
    {
        AmazonBedrockMessages.Convert(
            ModelId,
            new ModelMessage[] { new AssistantModelMessage(parts) },
            out _,
            out var messages
        );
        return messages;
    }

    [Theory]
    [InlineData("google")]
    [InlineData("vertex")]
    [InlineData("googleVertex")]
    [UpstreamTest(
        Prefix + "trims prefill before empty %s signature text",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public void Trims_prefill_before_empty_signature_text(string ns)
    {
        var messages = Convert(
            new AssistantTextPart("Answer  "),
            new AssistantTextPart("", Options("{\"" + ns + "\":{\"thoughtSignature\":\"opaque\"}}"))
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("[{\"role\":\"assistant\",\"content\":[{\"text\":\"Answer\"}]}]"),
                messages
            ),
            messages.ToJsonString()
        );
    }

    [Theory]
    [InlineData("google")]
    [InlineData("vertex")]
    [InlineData("googleVertex")]
    [UpstreamTest(
        Prefix + "omits empty %s signature text when reasoning is present",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public void Omits_empty_signature_text_when_reasoning_is_present(string ns)
    {
        var messages = Convert(
            new AssistantReasoningPart(
                "Foreign reasoning",
                Options("{\"" + ns + "\":{\"thoughtSignature\":\"reasoning\"}}")
            ),
            new AssistantTextPart("A"),
            new AssistantTextPart(
                "",
                Options("{\"" + ns + "\":{\"thoughtSignature\":\"opaque\"}}")
            ),
            new AssistantTextPart("B")
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "[{\"role\":\"assistant\",\"content\":[{\"text\":\"A\"},{\"text\":\"B\"}]}]"
                ),
                messages
            ),
            messages.ToJsonString()
        );
    }
}
