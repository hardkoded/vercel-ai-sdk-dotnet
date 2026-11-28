// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportB;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = TestFile + "::doStream::";

    private const string ChunkHead =
        "{\"id\":\"response-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Partial answer\"},\"finish_reason\":null}]}";

    [Theory]
    [InlineData("missing", FinishReason.Interrupted)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [InlineData("invalid-json", FinishReason.Error)]
    [UpstreamTest(
        Prefix + "handles a stream ending with %s as %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_a_stream_ending_with_ending_as_unified(
        string ending,
        FinishReason unified
    )
    {
        var chunks = new List<string> { "data: " + ChunkHead + "\n\n" };
        if (ending == "invalid-json")
        {
            chunks.Add("data: {invalid-json}\n\n");
        }
        else if (ending != "missing")
        {
            chunks.Add(
                "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\""
                    + ending
                    + "\"}]}\n\n"
            );
        }

        var parts = await Stream(Chunks(chunks.ToArray()), TestPrompt(), "kimi-k2-0905-preview");

        Assert.Contains(parts, p => p is TextDeltaStreamPart { Delta: "Partial answer" });
        Assert.Equal(ending == "invalid-json" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(ending == "unknown_reason" ? ending : null, finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve a provider error envelope in stream errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_a_provider_error_envelope_in_stream_errors()
    {
        var parts = await GetStreamParts("moonshotai-error");

        var errorPart = Assert.Single(parts.OfType<ErrorStreamPart>());
        var error = Assert.IsType<ProviderStreamError>(errorPart.Error);
        Assert.Equal("Internal server error", error.Message);
        Assert.Equal("server_error", error.Type);
        Assert.Equal("upstream_failure", error.Code?.ToString());
        Assert.Equal(500, error.StatusCode);
        Assert.True(error.IsRetryable);
        var data = error.Data is JsonElement element
            ? JsonNode.Parse(element.GetRawText())
            : JsonNode.Parse(JsonSerializer.Serialize(error.Data));
        AssertJson(
            data,
            "{\"error\":{\"message\":\"Internal server error\",\"type\":\"server_error\",\"code\":\"upstream_failure\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream reasoning and text deltas with usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_reasoning_and_text_deltas_with_usage()
    {
        var parts = await GetStreamParts("moonshotai-stream");

        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Empty(start.Warnings);
        Assert.Contains(parts, p => p is ReasoningStartStreamPart { Id: "reasoning-0" });
        Assert.Contains(parts, p =>
            p is ReasoningDeltaStreamPart { Id: "reasoning-0", Delta: "Thinking " }
        );
        Assert.Contains(parts, p => p is ReasoningEndStreamPart { Id: "reasoning-0" });
        Assert.Contains(parts, p => p is TextStartStreamPart { Id: "txt-0" });
        Assert.Contains(parts, p => p is TextDeltaStreamPart { Id: "txt-0", Delta: "Hello" });
        Assert.Contains(parts, p => p is TextEndStreamPart { Id: "txt-0" });

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(9, finish.Usage.InputTokens);
        Assert.Equal(12, finish.Usage.OutputTokens);
        Assert.Equal(7, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep reasoning active when deltas include empty tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_reasoning_active_when_deltas_include_empty_tool_calls()
    {
        var handler = Chunks(
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\","
                + "\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\",\"reasoning_content\":\"Think \",\"tool_calls\":[]},\"finish_reason\":null}]}\n\n",
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\","
                + "\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"reasoning_content\":\"more...\",\"tool_calls\":[]},\"finish_reason\":null}]}\n\n",
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\","
                + "\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\",\"reasoning_content\":\"\",\"tool_calls\":[]},\"finish_reason\":\"stop\"}]}\n\n",
            "data: [DONE]\n\n"
        );

        var parts = await Stream(handler, TestPrompt(), "kimi-k2-thinking");

        var reasoning = parts
            .Where(p =>
                p is ReasoningStartStreamPart or ReasoningDeltaStreamPart or ReasoningEndStreamPart
            )
            .Select(p =>
                p switch
                {
                    ReasoningStartStreamPart s => "start:" + s.Id,
                    ReasoningDeltaStreamPart d => "delta:" + d.Id + ":" + d.Delta,
                    ReasoningEndStreamPart e => "end:" + e.Id,
                    _ => string.Empty,
                }
            )
            .ToArray();
        Assert.Equal(
            new[]
            {
                "start:reasoning-0",
                "delta:reasoning-0:Think ",
                "delta:reasoning-0:more...",
                "end:reasoning-0",
            },
            reasoning
        );
    }

    [Theory]
    [InlineData("choice-level finish chunk")]
    [InlineData("separate top-level usage chunk")]
    [UpstreamTest(
        Prefix + "should preserve extra fields in streamed raw usage from a %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_extra_fields_in_streamed_raw_usage(string usageLocation)
    {
        const string usage =
            "{\"prompt_tokens\":100,\"completion_tokens\":50,\"total_tokens\":150,\"cached_tokens\":20,\"provider_usage_id\":\"usage-123\","
            + "\"prompt_tokens_details\":{\"cached_tokens\":20,\"cache_type\":\"ephemeral\"},"
            + "\"completion_tokens_details\":{\"reasoning_tokens\":10,\"billable_tokens\":42}}";
        var choiceUsage = usageLocation == "choice-level finish chunk" ? ",\"usage\":" + usage : "";
        var chunks = new List<string>
        {
            "data: {\"id\":\"chatcmpl-raw-usage\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\""
                + choiceUsage
                + "}]}\n\n",
        };
        if (usageLocation == "separate top-level usage chunk")
        {
            chunks.Add(
                "data: {\"id\":\"chatcmpl-raw-usage\",\"object\":\"chat.completion.chunk\",\"choices\":[],\"usage\":"
                    + usage
                    + "}\n\n"
            );
        }

        chunks.Add("data: [DONE]\n\n");

        var parts = await Stream(Chunks(chunks.ToArray()), TestPrompt());

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(100, finish.Usage.InputTokens);
        Assert.Equal(80, finish.Usage.NoCacheInputTokens);
        Assert.Equal(20, finish.Usage.CacheReadTokens);
        Assert.Equal(50, finish.Usage.OutputTokens);
        Assert.Equal(40, finish.Usage.TextTokens);
        Assert.Equal(10, finish.Usage.ReasoningTokens);
        AssertJson(JsonNode.Parse(finish.Usage.Raw!.Value.GetRawText()), usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include stream options with usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_stream_options_with_usage()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(handler, TestPrompt());

        var body = RequestBody(handler);
        Assert.True((bool)body["stream"]!);
        AssertJson(body["stream_options"], "{\"include_usage\":true}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send dynamic tool messages when streaming",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_dynamic_tool_messages_when_streaming()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new SystemModelMessage(
                        "",
                        Options(
                            "{\"tools\":[{\"type\":\"function\",\"name\":\"calculator\",\"inputSchema\":{\"type\":\"object\",\"properties\":{}}}]}"
                        )
                    ),
                },
            }
        );

        var message = RequestBody(handler)["messages"]![0]!;
        Assert.Equal("system", (string)message["role"]!);
        Assert.Equal("calculator", (string)message["tools"]![0]!["function"]!["name"]!);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send partial true on the final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_partial_true_on_the_final_assistant_message()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new AssistantModelMessage(
                        new AssistantContentPart[] { new AssistantTextPart("The answer is") }
                    )
                    {
                        ProviderOptions = Options("{\"partial\":true}"),
                    },
                },
            }
        );

        var body = RequestBody(handler);
        var message = body["messages"]![0]!;
        Assert.Equal("assistant", (string)message["role"]!);
        Assert.Equal("The answer is", (string)message["content"]!);
        Assert.True((bool)message["partial"]!);
        Assert.True((bool)body["stream"]!);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send message names", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_message_names()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new SystemModelMessage(
                        "You are a helpful assistant.",
                        Options("{\"name\":\"guide\"}")
                    ),
                    new UserModelMessage("Hello")
                    {
                        ProviderOptions = Options("{\"name\":\"alice\"}"),
                    },
                },
            }
        );

        var body = RequestBody(handler);
        AssertJson(
            body["messages"],
            "[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\",\"name\":\"guide\"},{\"role\":\"user\",\"content\":\"Hello\",\"name\":\"alice\"}]"
        );
        Assert.True((bool)body["stream"]!);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include tool-message name warnings in the stream start",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_tool_message_name_warnings_in_the_stream_start()
    {
        var handler = ChunksFixture("moonshotai-stream");

        var parts = await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new ToolModelMessage("call-1", "weather", "sunny", false, "text")
                    {
                        MessageProviderOptions = Options("{\"name\":\"weather_tool\"}"),
                    },
                },
            }
        );

        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        var warning = Assert.Single(start.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("message name on tool messages", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should assemble tool calls without explicit indices",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_assemble_tool_calls_without_explicit_indices()
    {
        var parts = await GetStreamParts("moonshotai-stream-indexless-tool-calls");

        Assert.DoesNotContain(parts, p => p is ErrorStreamPart);
        var calls = parts
            .OfType<ToolCallStreamPart>()
            .Select(c => (c.ToolCallId, c.ToolName, c.ArgumentsJson))
            .ToArray();
        Assert.Equal(
            new[]
            {
                ("weather_0", "weather", "{\"location\":\"San Francisco\"}"),
                ("time_1", "time", "{\"zone\":\"UTC\"}"),
            },
            calls
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve explicit tool call indices",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_explicit_tool_call_indices()
    {
        var parts = await GetStreamParts("moonshotai-stream-explicit-tool-call-index");

        Assert.DoesNotContain(parts, p => p is ErrorStreamPart);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("weather_0", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"location\": \"San Francisco\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use choice-level usage when top-level usage is absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_choice_level_usage_when_top_level_usage_is_absent()
    {
        var parts = await GetStreamParts("moonshotai-stream-choice-usage");

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(12, finish.Usage.InputTokens);
        Assert.Equal(5, finish.Usage.OutputTokens);
        Assert.Equal(1, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer top-level usage over choice-level usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_top_level_usage_over_choice_level_usage()
    {
        var parts = await GetStreamParts("moonshotai-stream-usage-precedence");

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(99, finish.Usage.InputTokens);
        Assert.Equal(33, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject malformed tool call indices",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_malformed_tool_call_indices()
    {
        var parts = await GetStreamParts("moonshotai-stream-malformed-tool-call-index");

        Assert.Contains(parts, p => p is ErrorStreamPart);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should collect streamed logprobs in finish provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_collect_streamed_logprobs_in_finish_provider_metadata()
    {
        var handler = ChunksFixture("moonshotai-logprobs");

        var parts = await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = TestPrompt().Prompt,
                ProviderOptions = Options("{\"topLogprobs\":1}"),
            },
            "moonshot-v1-8k"
        );

        var body = RequestBody(handler);
        Assert.True((bool)body["logprobs"]!);
        Assert.Equal(1, (int)body["top_logprobs"]!);
        Assert.Equal(
            "OK!",
            string.Concat(parts.OfType<TextDeltaStreamPart>().Select(p => p.Delta))
        );
        AssertJson(
            FinishMetadata(parts),
            "{\"moonshotai\":{\"choiceIndex\":0,\"logprobs\":{\"content\":["
                + "{\"bytes\":[79,75],\"logprob\":-0.0004457433824427426,\"token\":\"OK\",\"top_logprobs\":[{\"bytes\":[79,75],\"logprob\":-0.0004457433824427426,\"token\":\"OK\"}]},"
                + "{\"bytes\":null,\"logprob\":-0.01,\"token\":\"!\",\"top_logprobs\":[{\"bytes\":null,\"logprob\":-0.01,\"token\":\"!\"}]}"
                + "]},\"messageRole\":\"assistant\",\"responseObject\":\"chat.completion.chunk\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward text-part-array predicted content unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_text_part_array_predicted_content_unchanged()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(
            handler,
            new LanguageModelCallOptions
            {
                Prompt = TestPrompt().Prompt,
                ProviderOptions = Options(
                    "{\"prediction\":{\"type\":\"content\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"},{\"type\":\"text\",\"text\":\", world!\"}]}}"
                ),
            }
        );

        AssertJson(
            RequestBody(handler)["prediction"],
            "{\"type\":\"content\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"},{\"type\":\"text\",\"text\":\", world!\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send maxOutputTokens as max_completion_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_maxOutputTokens_as_max_completion_tokens()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(
            handler,
            new LanguageModelCallOptions { Prompt = TestPrompt().Prompt, MaxOutputTokens = 17 }
        );

        var body = RequestBody(handler);
        Assert.Equal(17, (int)body["max_completion_tokens"]!);
        Assert.False(body.ContainsKey("max_tokens"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit max_completion_tokens when maxOutputTokens is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_max_completion_tokens_when_maxOutputTokens_is_undefined()
    {
        var handler = ChunksFixture("moonshotai-stream");

        await Stream(handler, TestPrompt());

        var body = RequestBody(handler);
        Assert.False(body.ContainsKey("max_completion_tokens"));
        Assert.False(body.ContainsKey("max_tokens"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve response metadata and raw tool-call chunks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_response_metadata_and_raw_tool_call_chunks()
    {
        var handler = ChunksFixture("moonshotai-tool-call");

        var parts = await Stream(
            handler,
            new LanguageModelCallOptions { Prompt = TestPrompt().Prompt, IncludeRawChunks = true }
        );

        var rawChunks = File.ReadAllLines(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Fixtures",
                    "moonshotai-tool-call.chunks.txt"
                )
            )
            .Where(line => line.Length > 0)
            .Select(line => JsonNode.Parse(line))
            .ToArray();
        var actualRaw = parts
            .OfType<RawStreamPart>()
            .Select(p => JsonNode.Parse(p.RawJson))
            .ToArray();
        Assert.Equal(rawChunks.Length, actualRaw.Length);
        for (var i = 0; i < rawChunks.Length; i++)
        {
            Assert.True(
                JsonNode.DeepEquals(rawChunks[i], actualRaw[i]),
                actualRaw[i]!.ToJsonString()
            );
        }

        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_abc123", call.ToolCallId);
        Assert.Equal("get_weather", call.ToolName);
        Assert.Equal("{\"city\":\"Paris\"}", call.ArgumentsJson);
        AssertJson(
            FinishMetadata(parts),
            "{\"moonshotai\":{\"responseObject\":\"chat.completion.chunk\",\"choiceIndex\":0,\"messageRole\":\"assistant\",\"toolCallTypes\":[\"function\"]}}"
        );
    }

    [Theory]
    [InlineData("null")]
    [InlineData("undefined")]
    [UpstreamTest(
        Prefix + "should tolerate %s streamed response metadata fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_tolerate_streamed_response_metadata_fields(string metadataValue)
    {
        var body =
            metadataValue == "null"
                ? "{\"id\":\"chatcmpl-stream\",\"object\":null,\"choices\":[{\"index\":null,\"delta\":{\"role\":null,\"content\":\"Hello\"},\"finish_reason\":\"stop\"}]}"
                : "{\"id\":\"chatcmpl-stream\",\"choices\":[{\"delta\":{\"content\":\"Hello\"},\"finish_reason\":\"stop\"}]}";
        var handler = Chunks("data: " + body + "\n\n", "data: [DONE]\n\n");

        var parts = await Stream(handler, TestPrompt());

        AssertJson(FinishMetadata(parts), "{\"moonshotai\":{}}");
    }
}
