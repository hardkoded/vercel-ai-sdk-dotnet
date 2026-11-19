// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.ExpandedParallelToolCallPrompt;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; compaction</c>.</summary>
public sealed class CompactionTests
{
    private const string Prefix =
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > compaction::";

    private static IReadOnlyDictionary<string, JsonElement> Options(string json) =>
        new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse(json).RootElement };

    private static AssistantCustomPart Compaction(string itemId, string encryptedContent) =>
        new(
            "openai.compaction",
            Options(
                "{\"type\":\"compaction\",\"itemId\":\""
                    + itemId
                    + "\",\"encryptedContent\":\""
                    + encryptedContent
                    + "\"}"
            )
        );

    private static System.Text.Json.Nodes.JsonArray Run(
        IReadOnlyList<ModelMessage> prompt,
        bool store,
        bool hasConversation
    ) =>
        OpenAIResponsesLanguageModel
            .ConvertInput(
                prompt,
                "system",
                false,
                null,
                "openai",
                hasConversation,
                store,
                hasConversation: hasConversation
            )
            .Input;

    [Fact]
    [UpstreamTest(
        Prefix + "should convert compaction to item_reference when store is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_compaction_to_item_reference_when_store_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[] { Compaction("cmp_123", "encrypted_data_here") }
            ),
        };

        AssertJson(
            """[{"id":"cmp_123","type":"item_reference"}]""",
            Run(prompt, store: true, hasConversation: false)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert compaction to full compaction item when store is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_compaction_to_full_compaction_item_when_store_is_false()
    {
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[] { Compaction("cmp_456", "encrypted_compaction_state") }
            ),
        };

        AssertJson(
            """[{"encrypted_content":"encrypted_compaction_state","id":"cmp_456","type":"compaction"}]""",
            Run(prompt, store: false, hasConversation: false)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should skip compaction when hasConversation is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_compaction_when_hasConversation_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(
                new AssistantContentPart[] { Compaction("cmp_789", "encrypted_data") }
            ),
        };

        AssertJson(
            """[{"content":[{"text":"Hello","type":"input_text"}],"role":"user"}]""",
            Run(prompt, store: true, hasConversation: true)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle compaction alongside regular text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_compaction_alongside_regular_text_content()
    {
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart(
                        "Here is my response.",
                        Options("{\"itemId\":\"msg_001\"}")
                    ),
                    Compaction("cmp_001", "encrypted_state"),
                }
            ),
        };

        AssertJson(
            """[{"content":"Here is my response.","role":"assistant"},{"encrypted_content":"encrypted_state","id":"cmp_001","type":"compaction"}]""",
            Run(prompt, store: false, hasConversation: false)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle compaction with store: true alongside other content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_compaction_with_store_true_alongside_other_content()
    {
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart("Response text", Options("{\"itemId\":\"msg_002\"}")),
                    Compaction("cmp_002", "encrypted_data"),
                }
            ),
        };

        AssertJson(
            """[{"id":"msg_002","type":"item_reference"},{"id":"cmp_002","type":"item_reference"}]""",
            Run(prompt, store: true, hasConversation: false)
        );
    }
}
