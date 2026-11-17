// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel.MistralConversationFixtures;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertToMistralConversationInputs;

/// <summary>Port of <c>convert-to-mistral-conversation-inputs.test.ts</c> &gt; <c>convertToMistralConversationInputs</c>.</summary>
public sealed class ConvertToMistralConversationInputsTests
{
    private const string Prefix =
        "packages/mistral/src/convert-to-mistral-conversation-inputs.test.ts::convertToMistralConversationInputs::";

    private static readonly ToolNameMapping Mapping =
        Vercel.AI.Mistral.MistralConversationPrepareTools.CreateToolNameMapping(
            new[] { Search("mistral.web_search_premium", "news") }
        );

    private static JsonArray Convert(params ModelMessage[] prompt)
    {
        return Vercel.AI.Mistral.ConvertToMistralConversationInputs.Convert(prompt, Mapping).Inputs;
    }

    private static IReadOnlyDictionary<string, System.Text.Json.JsonElement> MistralFunction(
        string name
    )
    {
        return MistralOptions("{\"function\":\"" + name + "\"}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits entry IDs on replay while preserving function tool-call IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Omits_entry_IDs_on_replay_while_preserving_function_tool_call_IDs()
    {
        var inputs = Convert(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "tool_exec_search",
                            "news",
                            "{\"arguments\":\"{\\\"query\\\":\\\"news\\\"}\"}"
                        )
                        {
                            ProviderExecuted = true,
                        },
                        MistralFunction("web_search")
                    ),
                    new AssistantToolResultPart(
                        "tool_exec_search",
                        "news",
                        new ToolModelMessage(
                            "tool_exec_search",
                            "news",
                            "{\"info\":{\"result\":\"search context\"}}",
                            false,
                            "json"
                        )
                    ),
                    new AssistantToolCallPart(
                        new GeneratedToolCall("function-1", "weather", "{\"city\":\"Paris\"}")
                    ),
                }
            ),
            new ToolModelMessage("function-1", "weather", "Sunny", false, "text")
        );

        foreach (var input in inputs)
        {
            Assert.Null(input!["id"]);
        }

        AssertJson(
            "{\"type\":\"function.call\",\"tool_call_id\":\"tool_exec_search\",\"name\":\"web_search\",\"arguments\":\"{\\\"query\\\":\\\"news\\\"}\"}",
            inputs[0]
        );
        AssertJson(
            "{\"type\":\"function.result\",\"tool_call_id\":\"tool_exec_search\",\"result\":\"search context\"}",
            inputs[1]
        );
        AssertMatches("{\"type\":\"function.call\",\"tool_call_id\":\"function-1\"}", inputs[2]);
        AssertMatches("{\"type\":\"function.result\",\"tool_call_id\":\"function-1\"}", inputs[3]);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "preserves message order, reasoning, and function results within assistant messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_message_order_reasoning_and_function_results_within_assistant_messages()
    {
        var inputs = Convert(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart("Before"),
                    new AssistantReasoningPart("Think"),
                    new AssistantToolCallPart(
                        new GeneratedToolCall("function-1", "weather", "{\"city\":\"Paris\"}")
                    ),
                    new AssistantToolResultPart(
                        "function-1",
                        "weather",
                        new ToolModelMessage("function-1", "weather", "Sunny", false, "text")
                    ),
                    new AssistantTextPart("After"),
                }
            )
        );

        AssertJson(
            "[{\"type\":\"message.input\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Before\"},{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Think\"}],\"closed\":true}]},{\"type\":\"function.call\",\"tool_call_id\":\"function-1\",\"name\":\"weather\",\"arguments\":\"{\\\"city\\\":\\\"Paris\\\"}\"},{\"type\":\"function.result\",\"tool_call_id\":\"function-1\",\"result\":\"Sunny\"},{\"type\":\"message.input\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"After\"}]}]",
            inputs
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "replays renamed premium search as function calls and results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Replays_renamed_premium_search_as_function_calls_and_results()
    {
        var inputs = Convert(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall("search-1", "news", "{\"arguments\":\"news query\"}")
                        {
                            ProviderExecuted = true,
                        }
                    ),
                }
            ),
            new ToolModelMessage(
                "search-1",
                "news",
                "{\"info\":{\"documents\":[\"result\"]}}",
                false,
                "json"
            ),
            new AssistantModelMessage("The news", null, null)
        );

        AssertJson(
            "[{\"type\":\"function.call\",\"tool_call_id\":\"search-1\",\"name\":\"web_search_premium\",\"arguments\":\"news query\"},{\"type\":\"function.result\",\"tool_call_id\":\"search-1\",\"result\":\"{\\\"documents\\\":[\\\"result\\\"]}\"},{\"type\":\"message.input\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"The news\"}]}]",
            inputs
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps images and PDFs in user messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_images_and_PDFs_in_user_messages()
    {
        var inputs = Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Search for context"),
                    new FileContentPart("image/png", "https://example.com/image.png", null, null),
                    new FileContentPart(
                        "application/pdf",
                        "https://example.com/document.pdf",
                        null,
                        null
                    ),
                }
            )
        );

        AssertJson(
            "[{\"type\":\"message.input\",\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Search for context\"},{\"type\":\"image_url\",\"image_url\":\"https://example.com/image.png\"},{\"type\":\"document_url\",\"document_url\":\"https://example.com/document.pdf\"}]}]",
            inputs
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not silently drop unsupported assistant content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Does_not_silently_drop_unsupported_assistant_content()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            Convert(
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantFilePart("image/png", null, Array.Empty<byte>()),
                    }
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/convert-to-mistral-conversation-inputs.test.ts::preserves argument replay and serializes result graphs in conversation inputs",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
    )]
    public void Preserves_argument_replay_and_serializes_result_graphs_in_conversation_inputs()
    {
        var value = new Dictionary<string, object?>
        {
            ["count"] = System.Numerics.BigInteger.Parse("42"),
        };
        value["self"] = value;
        var inputs = Convert(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall("call1", "inspect", "{\"count\":42}")
                    ),
                }
            ),
            new ToolModelMessage(
                "call1",
                "inspect",
                Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(
                    value
                ),
                false
            )
        );
        Assert.Equal(2, inputs.Count);
        Assert.Equal("function.call", (string)inputs[0]!["type"]!);
        Assert.Equal("call1", (string)inputs[0]!["tool_call_id"]!);
        Assert.Equal("inspect", (string)inputs[0]!["name"]!);
        Assert.Equal("{\"count\":42}", (string)inputs[0]!["arguments"]!);
        Assert.Equal("function.result", (string)inputs[1]!["type"]!);
        Assert.Equal("call1", (string)inputs[1]!["tool_call_id"]!);
        Assert.Equal("{\"count\":\"42\",\"self\":\"[Circular]\"}", (string)inputs[1]!["result"]!);
    }
}
