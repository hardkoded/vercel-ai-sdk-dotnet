// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    // Stands for a message_delta event that is not sent at all.
    private const string Undefined = "<undefined>";

    [Theory]
    [InlineData("empty")]
    [InlineData("metadata-only")]
    [InlineData("empty-text")]
    [InlineData("empty-reasoning")]
    [InlineData("empty-tool-input")]
    [InlineData("later-message")]
    [InlineData("partial-text")]
    [InlineData("partial-reasoning")]
    [InlineData("partial-tool-input")]
    [InlineData("tool-call")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::only emits an interrupted finish after output (%s)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Only_emits_an_interrupted_finish_after_output(string scenario)
    {
        static string Event(string json) => "data: " + json + "\n\n";
        static string MessageStart(string id, string content = "[]") =>
            Event(
                "{\"type\":\"message_start\",\"message\":{\"id\":\""
                    + id
                    + "\",\"type\":\"message\",\"role\":\"assistant\",\"content\":"
                    + content
                    + ",\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":1}}}"
            );
        const string TextStart =
            "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n";
        static string TextDelta(string text) =>
            Event(
                "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\""
                    + text
                    + "\"}}"
            );

        var sse = string.Empty;
        if (scenario == "later-message")
        {
            sse +=
                MessageStart("msg-previous")
                + TextStart
                + TextDelta("Previous answer")
                + Event("{\"type\":\"content_block_stop\",\"index\":0}")
                + Event(
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":3}}"
                )
                + Event("{\"type\":\"message_stop\"}");
        }

        if (scenario != "empty")
        {
            sse += MessageStart(
                "msg-current",
                scenario == "tool-call"
                    ? "[{\"type\":\"tool_use\",\"id\":\"tool-1\",\"name\":\"test-tool\",\"input\":{}}]"
                    : "[]"
            );
        }

        if (scenario == "empty-text" || scenario == "partial-text")
        {
            sse += TextStart + TextDelta(scenario == "partial-text" ? "Partial" : string.Empty);
        }

        if (scenario == "empty-reasoning" || scenario == "partial-reasoning")
        {
            sse +=
                Event(
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"thinking\",\"thinking\":\"\"}}"
                )
                + Event(
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"thinking_delta\",\"thinking\":\""
                        + (scenario == "partial-reasoning" ? "Thinking" : string.Empty)
                        + "\"}}"
                );
        }

        if (scenario == "empty-tool-input" || scenario == "partial-tool-input")
        {
            sse +=
                Event(
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"tool_use\",\"id\":\"tool-1\",\"name\":\"test-tool\",\"input\":{}}}"
                )
                + Event(
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\""
                        + (scenario == "partial-tool-input" ? "{" : string.Empty)
                        + "\"}}"
                );
        }

        var parts = await StreamAsync(sse);

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finishes = parts
            .OfType<FinishStreamPart>()
            .Select(part => (part.FinishReason, part.RawFinishReason))
            .ToList();
        if (scenario == "later-message")
        {
            Assert.Equal(new[] { (FinishReason.Stop, (string?)"end_turn") }, finishes);
        }
        else if (
            scenario is "partial-text" or "partial-reasoning" or "partial-tool-input" or "tool-call"
        )
        {
            Assert.Equal(new[] { (FinishReason.Interrupted, (string?)null) }, finishes);
        }
        else
        {
            Assert.Empty(finishes);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::reports interruption for a later message even when an earlier message completed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_interruption_for_a_later_message_even_when_an_earlier_message_completed()
    {
        static string MessageStart(string id) =>
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\""
            + id
            + "\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":1}}}\n\n";

        var parts = await StreamAsync(
            MessageStart("msg1")
                + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":3}}\n\n"
                + "data: {\"type\":\"message_stop\"}\n\n"
                + MessageStart("msg2")
                + "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n"
                + "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Partial answer\"}}\n\n"
        );

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finishes = parts.OfType<FinishStreamPart>().ToList();
        Assert.Equal(2, finishes.Count);
        Assert.Equal(FinishReason.Stop, finishes[0].FinishReason);
        Assert.Equal("end_turn", finishes[0].RawFinishReason);
        Assert.Equal(FinishReason.Interrupted, finishes[1].FinishReason);
        Assert.Null(finishes[1].RawFinishReason);
        Assert.Equal(finishes[0].Usage, finishes[1].Usage);
        Assert.Equal(finishes[0].ProviderMetadata, finishes[1].ProviderMetadata);
    }

    [Theory]
    [InlineData(Undefined, false, "Interrupted")]
    [InlineData(null, false, "Interrupted")]
    [InlineData("end_turn", false, "Interrupted")]
    [InlineData("end_turn", true, "Stop")]
    [InlineData(Undefined, true, "Other")]
    [InlineData("unknown_reason", true, "Other")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::maps stop_reason %j and message_stop %s to %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_stop_reason_and_message_stop_to_a_finish_reason(
        string? stopReason,
        bool messageStop,
        string unified
    )
    {
        var sse =
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg1\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":1}}}\n\n"
            + "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n"
            + "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Partial answer\"}}\n\n"
            + (
                stopReason == Undefined
                    ? string.Empty
                    : "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":"
                        + (stopReason == null ? "null" : "\"" + stopReason + "\"")
                        + ",\"stop_sequence\":null},\"usage\":{\"output_tokens\":3}}\n\n"
            )
            + (messageStop ? "data: {\"type\":\"message_stop\"}\n\n" : string.Empty);

        var parts = await StreamAsync(sse);

        Assert.Contains(parts, part =>
            part is TextDeltaStreamPart delta && delta.Delta == "Partial answer"
        );
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason.ToString());
        Assert.Equal(
            messageStop && stopReason != Undefined ? stopReason : null,
            finish.RawFinishReason
        );
        Assert.Equal(messageStop ? 10 : null, finish.Usage.InputTokens);
        if (messageStop)
        {
            Assert.Equal(
                10,
                finish
                    .ProviderMetadata!.Value.GetProperty("anthropic")
                    .GetProperty("usage")
                    .GetProperty("input_tokens")
                    .GetInt32()
            );
        }
        else
        {
            Assert.Null(finish.ProviderMetadata);
        }
    }

    [Fact]
    public async Task Finish_part_carries_stop_sequence_and_container_metadata()
    {
        var parts = await StreamAsync(
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg1\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":1}}}\n\n"
                + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"stop_sequence\",\"stop_sequence\":\"END\",\"container\":{\"id\":\"c1\",\"expires_at\":\"2030-01-01T00:00:00Z\",\"skills\":[{\"type\":\"anthropic\",\"skill_id\":\"pptx\",\"version\":\"1\"}]}},\"usage\":{\"output_tokens\":3}}\n\n"
                + "data: {\"type\":\"message_stop\"}\n\n"
        );

        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        JsonAssert.Equal(
            finish.ProviderMetadata!.Value,
            "{\"anthropic\":{\"usage\":{\"input_tokens\":10,\"output_tokens\":3},\"stopSequence\":\"END\",\"iterations\":null,\"container\":{\"expiresAt\":\"2030-01-01T00:00:00Z\",\"id\":\"c1\",\"skills\":[{\"type\":\"anthropic\",\"skillId\":\"pptx\",\"version\":\"1\"}]},\"contextManagement\":null}}"
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should stream compaction content blocks with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_compaction_content_blocks_with_provider_metadata()
    {
        var lines = File.ReadAllLines(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Fixtures",
                    "anthropic-compaction.1.chunks.txt"
                )
            )
            .Where(line => line.Trim().Length > 0);
        var parts = await StreamAsync(
            string.Concat(lines.Select(line => "data: " + line + "\n\n"))
        );

        var compactionStart = Assert.Single(parts.OfType<TextStartStreamPart>(), part =>
            part.Id == "0"
        );
        JsonAssert.Equal(
            compactionStart.ProviderMetadata!.Value,
            "{\"anthropic\":{\"type\":\"compaction\"}}"
        );

        var compactionText = string.Concat(
            parts
                .OfType<TextDeltaStreamPart>()
                .Where(part => part.Id == "0")
                .Select(part => part.Delta)
        );
        Assert.NotEmpty(compactionText);
        Assert.Contains("## Summary of Conversation", compactionText);
        Assert.Contains("React vs Vue.js", compactionText);

        var regularStart = Assert.Single(parts.OfType<TextStartStreamPart>(), part =>
            part.Id == "1"
        );
        Assert.Null(regularStart.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should stream complete signed on-demand compaction blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_complete_signed_on_demand_compaction_blocks()
    {
        var parts = await StreamAsync(
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_compaction\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-opus-5\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":0,\"output_tokens\":0}}}\n\n"
                + "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"compaction\",\"content\":\"Summary of the conversation.\",\"signature\":\"compaction-signature\"}}\n\n"
                + "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n"
                + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"compaction\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":0,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":120,\"output_tokens\":30}]}}\n\n"
                + "data: {\"type\":\"message_stop\"}\n\n"
        );

        var start = Assert.Single(parts.OfType<TextStartStreamPart>());
        Assert.Equal("0", start.Id);
        JsonAssert.Equal(
            start.ProviderMetadata!.Value,
            "{\"anthropic\":{\"type\":\"compaction\",\"signature\":\"compaction-signature\"}}"
        );
        var delta = Assert.Single(parts.OfType<TextDeltaStreamPart>());
        Assert.Equal("0", delta.Id);
        Assert.Equal("Summary of the conversation.", delta.Delta);
        Assert.Null(delta.ProviderMetadata);
        Assert.Equal("0", Assert.Single(parts.OfType<TextEndStreamPart>()).Id);
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Other, finish.FinishReason);
        Assert.Equal("compaction", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should handle compaction_delta with null content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_compaction_delta_with_null_content()
    {
        var parts = await StreamAsync(
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-3-haiku-20240307\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":100,\"output_tokens\":0},\"content\":[],\"stop_reason\":null}}\n\n"
                + "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"compaction\",\"content\":null}}\n\n"
                + "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"compaction_delta\",\"content\":null}}\n\n"
                + "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"compaction_delta\",\"content\":\"Summary of conversation.\"}}\n\n"
                + "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n"
                + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":10}}\n\n"
                + "data: {\"type\":\"message_stop\"}\n\n"
        );

        // No parsing errors should occur, and the null content delta is skipped.
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var delta = Assert.Single(parts.OfType<TextDeltaStreamPart>(), part => part.Id == "0");
        Assert.Equal("Summary of conversation.", delta.Delta);
    }

    private const string StartWithContainer =
        "{\"type\":\"message_start\",\"message\":{\"id\":\"msg1\",\"model\":\"m\",\"container\":{\"id\":\"c1\",\"expires_at\":\"2030-01-01T00:00:00Z\"},\"usage\":{\"input_tokens\":10,\"output_tokens\":1}}}";

    private static JsonElement FinishMetadata(AnthropicStream stream, params string[] events)
    {
        var parts = events.SelectMany(stream.Push).ToList();
        return parts.OfType<FinishStreamPart>().Last().ProviderMetadata!.Value;
    }

    [Fact]
    public void Finish_metadata_is_mirrored_under_a_custom_provider_key()
    {
        var metadata = FinishMetadata(
            new AnthropicStream(false, false, "custom", true),
            StartWithContainer,
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":3}}",
            "{\"type\":\"message_stop\"}"
        );

        Assert.Equal(
            metadata.GetProperty("anthropic").GetRawText(),
            metadata.GetProperty("custom").GetRawText()
        );
    }

    [Fact]
    public void Container_from_message_start_has_null_skills_and_is_cleared_by_a_delta_without_one()
    {
        var withoutDelta = FinishMetadata(
            new AnthropicStream(false, false),
            StartWithContainer,
            "{\"type\":\"message_stop\"}"
        );
        JsonAssert.Equal(
            withoutDelta.GetProperty("anthropic").GetProperty("container"),
            "{\"expiresAt\":\"2030-01-01T00:00:00Z\",\"id\":\"c1\",\"skills\":null}"
        );

        var withDelta = FinishMetadata(
            new AnthropicStream(false, false),
            StartWithContainer,
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":3}}",
            "{\"type\":\"message_stop\"}"
        );
        Assert.Equal(
            JsonValueKind.Null,
            withDelta.GetProperty("anthropic").GetProperty("container").ValueKind
        );
    }

    [Fact]
    public void Stop_sequence_is_null_when_the_delta_has_none()
    {
        var metadata = FinishMetadata(
            new AnthropicStream(false, false),
            StartWithContainer,
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":3}}",
            "{\"type\":\"message_stop\"}"
        );

        Assert.Equal(
            JsonValueKind.Null,
            metadata.GetProperty("anthropic").GetProperty("stopSequence").ValueKind
        );
    }

    [Fact]
    public void A_new_message_resets_the_stop_sequence_and_container()
    {
        var metadata = FinishMetadata(
            new AnthropicStream(false, false),
            StartWithContainer,
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"stop_sequence\",\"stop_sequence\":\"END\",\"container\":{\"id\":\"c1\",\"expires_at\":\"2030-01-01T00:00:00Z\"}},\"usage\":{\"output_tokens\":3}}",
            "{\"type\":\"message_stop\"}",
            "{\"type\":\"message_start\",\"message\":{\"id\":\"msg2\",\"model\":\"m\",\"usage\":{\"input_tokens\":5,\"output_tokens\":1}}}",
            "{\"type\":\"message_stop\"}"
        );

        var anthropic = metadata.GetProperty("anthropic");
        Assert.Equal(JsonValueKind.Null, anthropic.GetProperty("stopSequence").ValueKind);
        Assert.Equal(JsonValueKind.Null, anthropic.GetProperty("container").ValueKind);
    }

    [Theory]
    [InlineData(null, false, "anthropic")]
    [InlineData(null, true, "anthropic")]
    [InlineData(null, false, "vertex")]
    [InlineData(null, true, "vertex")]
    [InlineData(false, false, "anthropic")]
    [InlineData(false, true, "anthropic")]
    [InlineData(false, false, "vertex")]
    [InlineData(false, true, "vertex")]
    [InlineData(true, false, "anthropic")]
    [InlineData(true, true, "anthropic")]
    [InlineData(true, false, "vertex")]
    [InlineData(true, true, "vertex")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::only emits typed pings when enabled ($includePings, raw: $includeRawChunks, key: $providerKey)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "97540149254736e8d566d432ccb7abf7b1cebbb3"
    )]
    public async Task Only_emits_typed_pings_when_enabled(
        bool? includePings,
        bool includeRawChunks,
        string providerKey
    )
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody =
                "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}\n\n"
                + "data: {\"type\":\"ping\"}\n\n"
                + "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n"
                + "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello\"}}\n\n"
                + "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n"
                + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":2}}\n\n"
                + "data: {\"type\":\"message_stop\"}\n\n",
        };
        var provider = AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                Name = providerKey == "anthropic" ? null : "vertex.anthropic",
            },
            handler
        );
        var options = AnthropicParity.Hello();
        options.IncludeRawChunks = includeRawChunks;
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            [providerKey] = AnthropicParity.Json(
                includePings == null
                    ? "{}"
                    : "{\"includePings\":" + (includePings.Value ? "true" : "false") + "}"
            ),
        };

        var parts = new List<LanguageModelStreamPart>();
        await foreach (
            var part in await provider
                .LanguageModel("claude-3-haiku-20240307")
                .DoStreamAsync(options, CancellationToken.None)
        )
        {
            parts.Add(part);
        }

        Assert.Equal(includePings == true ? 1 : 0, parts.OfType<PingStreamPart>().Count());
        using var body = JsonDocument.Parse(handler.Body);
        Assert.False(body.RootElement.TryGetProperty("includePings", out _));
        Assert.Equal(
            includeRawChunks ? 1 : 0,
            parts.OfType<RawStreamPart>().Count(raw => raw.RawJson == "{\"type\":\"ping\"}")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use input_tokens from message_delta when different from message_start",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_input_tokens_from_message_delta_when_different_from_message_start()
    {
        // Fixture has message_start.usage.input_tokens=43, message_delta.usage.input_tokens=61
        // The final usage should use the value from message_delta (61)
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-message-delta-input-tokens")
            ),
            "claude-3-haiku-20240307"
        );

        var finish = DoStreamSupport.Finish(parts);
        Assert.Equal(61, finish.Usage.InputTokens);
        Assert.Equal(2, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map a streamed classifier refusal to content-filter and expose stop details",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_a_streamed_classifier_refusal_to_content_filter_and_expose_stop_details()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-refusal")),
            "claude-fable-5"
        );

        var finish = DoStreamSupport.Finish(parts);
        Assert.Equal(FinishReason.ContentFilter, finish.FinishReason);
        Assert.Equal("refusal", finish.RawFinishReason);
        DoStreamSupport.AssertSnapshot(
            DoStreamSupport.AnthropicMetadata(finish, "stopDetails"),
            "refusal-stop-details"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should preserve the streamed fallback content block and surface the fallback iteration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_streamed_fallback_content_block_and_surface_the_fallback_iteration()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-fallback")),
            "claude-fable-5"
        );

        var fallback = Assert.Single(parts.OfType<CustomStreamPart>(), part =>
            part.Kind == "anthropic.fallback"
        );
        JsonAssert.Equal(
            fallback.ProviderMetadata!.Value,
            "{\"anthropic\":{\"type\":\"fallback\",\"from\":{\"model\":\"claude-fable-5\"},\"to\":{\"model\":\"claude-opus-4-8\"}}}"
        );

        var textDeltas = string.Concat(parts.OfType<TextDeltaStreamPart>().Select(p => p.Delta));
        Assert.Equal(
            "The printing press was invented by Johannes Gutenberg around 1440.",
            textDeltas
        );

        DoStreamSupport.AssertSnapshot(
            DoStreamSupport.AnthropicMetadata(DoStreamSupport.Finish(parts), "iterations"),
            "fallback-iterations"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve a mid-output fallback boundary between reasoning blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_a_mid_output_fallback_boundary_between_reasoning_blocks()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-fallback-mid-output")
            ),
            "claude-opus-5-5"
        );

        var firstReasoningEnd = parts.FindIndex(part => part is ReasoningEndStreamPart { Id: "0" });
        var fallback = parts.FindIndex(part =>
            part is CustomStreamPart { Kind: "anthropic.fallback" }
        );
        var secondReasoningStart = parts.FindIndex(part =>
            part is ReasoningStartStreamPart { Id: "2" }
        );

        Assert.True(firstReasoningEnd > -1);
        Assert.True(fallback > firstReasoningEnd);
        Assert.True(secondReasoningStart > fallback);
        Assert.Equal("custom", parts[fallback].Type);
        JsonAssert.Equal(
            ((CustomStreamPart)parts[fallback]).ProviderMetadata!.Value,
            "{\"anthropic\":{\"type\":\"fallback\",\"from\":{\"model\":\"claude-opus-5-5\"},\"to\":{\"model\":\"claude-opus-4-8\"}}}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_deltas()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}",
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"thinking\",\"thinking\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"thinking_delta\",\"thinking\":\"I am\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"thinking_delta\",\"thinking\":\"thinking...\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"signature_delta\",\"signature\":\"1234567890\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":0}",
                    "{\"type\":\"content_block_start\",\"index\":1,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":1,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello, World!\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":1}",
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":227}}",
                    "{\"type\":\"message_stop\"}"
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "stream-reasoning-deltas");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream thinking updates between tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_thinking_updates_between_tool_calls()
    {
        var options = AnthropicParity.WithProvider(
            "{\"thinking\":{\"type\":\"adaptive\",\"display\":\"updates\"}}"
        );
        options.Tools = new LanguageModelTool[]
        {
            new LanguageModelTool(
                "lookup",
                null,
                AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}")
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_updates\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-fable-5\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":1}}}",
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"thinking\",\"thinking\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"thinking_delta\",\"thinking\":\"First update\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"signature_delta\",\"signature\":\"sig-1\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":0}",
                    "{\"type\":\"content_block_start\",\"index\":1,\"content_block\":{\"type\":\"tool_use\",\"id\":\"tool-1\",\"name\":\"lookup\",\"input\":{}}}",
                    "{\"type\":\"content_block_delta\",\"index\":1,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"{}\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":1}",
                    "{\"type\":\"content_block_start\",\"index\":2,\"content_block\":{\"type\":\"thinking\",\"thinking\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":2,\"delta\":{\"type\":\"thinking_delta\",\"thinking\":\"Second update\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":2,\"delta\":{\"type\":\"signature_delta\",\"signature\":\"sig-2\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":2}",
                    "{\"type\":\"content_block_start\",\"index\":3,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":3,\"delta\":{\"type\":\"text_delta\",\"text\":\"Done\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":3}",
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":20}}",
                    "{\"type\":\"message_stop\"}"
                )
            ),
            "claude-fable-5",
            options
        );

        var updates = parts.SelectMany(part =>
            part switch
            {
                ReasoningStartStreamPart start => new[] { "reasoning-start|" + start.Id + "|" },
                ReasoningDeltaStreamPart { Delta: not "" } delta => new[]
                {
                    "reasoning-delta|" + delta.Id + "|" + delta.Delta,
                },
                _ => Array.Empty<string>(),
            }
        );
        Assert.Equal(
            new[]
            {
                "reasoning-start|0|",
                "reasoning-delta|0|First update",
                "reasoning-start|2|",
                "reasoning-delta|2|Second update",
            },
            updates
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream redacted reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_redacted_reasoning()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}",
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"redacted_thinking\",\"data\":\"redacted-thinking-data\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":0}",
                    "{\"type\":\"content_block_start\",\"index\":1,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":1,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello, World!\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":1}",
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":227}}",
                    "{\"type\":\"message_stop\"}"
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "stream-redacted-reasoning");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore signatures on text deltas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_ignore_signatures_on_text_deltas()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}",
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello, World!\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"signature_delta\",\"signature\":\"1234567890\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":0}",
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":227}}",
                    "{\"type\":\"message_stop\"}"
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "ignore-signatures-on-text-deltas");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse context_management from streaming response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_context_management_from_streaming_response()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}",
                    "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}",
                    "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello\"}}",
                    "{\"type\":\"content_block_stop\",\"index\":0}",
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":50},\"context_management\":{\"applied_edits\":[{\"type\":\"clear_tool_uses_20250919\",\"cleared_tool_uses\":5,\"cleared_input_tokens\":10000}]}}",
                    "{\"type\":\"message_stop\"}"
                )
            ),
            "claude-3-haiku-20240307"
        );

        JsonAssert.Equal(
            DoStreamSupport.AnthropicMetadata(DoStreamSupport.Finish(parts), "contextManagement"),
            "{\"appliedEdits\":[{\"type\":\"clear_tool_uses_20250919\",\"clearedToolUses\":5,\"clearedInputTokens\":10000}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the last non-null safeguard_results from message_delta",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_last_non_null_safeguard_results_from_message_delta()
    {
        const string safeguardResults =
            "[{\"type\":\"dangerous_tool_use\",\"status\":{\"type\":\"available\",\"tool_uses\":{\"toolu_01\":{\"type\":\"evaluated\",\"outcome\":\"not_flagged\"}}}}]";
        var handler = DoStreamSupport.Stream(
            "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}",
            "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"tool_use\",\"id\":\"toolu_01\",\"name\":\"Bash\",\"input\":{}}}",
            "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"{\\\"command\\\":\\\"echo hello\\\"}\"}}",
            "{\"type\":\"content_block_stop\",\"index\":0}",
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"safeguard_results\":null},\"usage\":{\"output_tokens\":50}}",
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"safeguard_results\":"
                + safeguardResults
                + "},\"usage\":{\"output_tokens\":50}}",
            "{\"type\":\"message_stop\"}"
        );

        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(handler),
            "claude-3-haiku-20240307",
            AnthropicParity.WithProvider("{\"safeguards\":[{\"type\":\"dangerous_tool_use\"}]}")
        );

        var finish = DoStreamSupport.Finish(parts);
        using var body = JsonDocument.Parse(handler.Body);
        JsonAssert.Equal(
            body.RootElement.GetProperty("safeguards"),
            "[{\"type\":\"dangerous_tool_use\"}]"
        );
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("tool_use", finish.RawFinishReason);
        JsonAssert.Equal(
            DoStreamSupport.AnthropicMetadata(finish, "safeguardResults"),
            safeguardResults
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit safeguardResults when no message_delta carries them",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_safeguardResults_when_no_message_delta_carries_them()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}",
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"safeguard_results\":null},\"usage\":{\"output_tokens\":50}}",
                    "{\"type\":\"message_stop\"}"
                )
            ),
            "claude-3-haiku-20240307"
        );

        var anthropic = DoStreamSupport
            .Finish(parts)
            .ProviderMetadata!.Value.GetProperty("anthropic");
        Assert.False(anthropic.TryGetProperty("safeguardResults", out _));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse iterations from streaming compaction response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_iterations_from_streaming_compaction_response()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-compaction.1")),
            "claude-3-haiku-20240307"
        );

        var finish = DoStreamSupport.Finish(parts);
        JsonAssert.Equal(
            DoStreamSupport.AnthropicMetadata(finish, "iterations"),
            "[{\"type\":\"compaction\",\"inputTokens\":60385,\"outputTokens\":522},{\"type\":\"message\",\"inputTokens\":612,\"outputTokens\":2819}]"
        );
        Assert.Equal(60385 + 612, finish.Usage.InputTokens);
        Assert.Equal(522 + 2819, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream clear_tool_uses response with context_management",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_clear_tool_uses_response_with_context_management()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-clear-tool-uses.1")),
            "claude-3-haiku-20240307"
        );

        var textDeltas = parts.OfType<TextDeltaStreamPart>().Where(part => part.Id == "0").ToList();
        Assert.True(textDeltas.Count > 0);

        var fullText = string.Concat(textDeltas.Select(delta => delta.Delta));
        Assert.Contains("Here's a comparison of the weather in both cities", fullText);

        var finish = DoStreamSupport.Finish(parts);
        JsonAssert.Equal(
            DoStreamSupport.AnthropicMetadata(finish, "contextManagement"),
            "{\"appliedEdits\":[]}"
        );

        Assert.Equal(859, finish.Usage.InputTokens);
        Assert.Equal(122, finish.Usage.OutputTokens);
        JsonAssert.Equal(
            DoStreamSupport.SnapshotOf(finish)["usage"],
            """
            {
              "inputTokens": { "cacheRead": 0, "cacheWrite": 0, "noCache": 859, "total": 859 },
              "outputTokens": { "total": 122 },
              "raw": {
                "cache_creation": {
                  "ephemeral_1h_input_tokens": 0,
                  "ephemeral_5m_input_tokens": 0
                },
                "cache_creation_input_tokens": 0,
                "cache_read_input_tokens": 0,
                "inference_geo": "not_available",
                "input_tokens": 859,
                "output_tokens": 122,
                "service_tier": "standard"
              }
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream clear_thinking response with thinking and text blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_clear_thinking_response_with_thinking_and_text_blocks()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(DoStreamSupport.FixtureStream("anthropic-clear-thinking.1")),
            "claude-3-haiku-20240307"
        );

        Assert.Contains(parts, part => part is ReasoningStartStreamPart);

        var reasoningDeltas = parts.OfType<ReasoningDeltaStreamPart>().ToList();
        Assert.True(reasoningDeltas.Count > 0);
        var fullReasoning = string.Concat(reasoningDeltas.Select(delta => delta.Delta));
        Assert.Contains("925", fullReasoning);
        Assert.Contains("185", fullReasoning);

        var signatureParts = reasoningDeltas
            .Where(part =>
                part.ProviderMetadata is { } metadata && metadata.TryGetProperty("anthropic", out _)
            )
            .ToList();
        Assert.Single(signatureParts);
        Assert.False(
            string.IsNullOrEmpty(
                signatureParts[0]
                    .ProviderMetadata!.Value.GetProperty("anthropic")
                    .GetProperty("signature")
                    .GetString()
            )
        );

        var textDeltas = parts.OfType<TextDeltaStreamPart>().Where(part => part.Id == "1").ToList();
        Assert.True(textDeltas.Count > 0);
        Assert.Equal("925 ÷ 5 = 185", string.Concat(textDeltas.Select(delta => delta.Delta)));

        var finish = DoStreamSupport.Finish(parts);
        JsonAssert.Equal(
            DoStreamSupport.AnthropicMetadata(finish, "contextManagement"),
            "{\"appliedEdits\":[]}"
        );

        Assert.Equal(69, finish.Usage.InputTokens);
        Assert.Equal(53, finish.Usage.OutputTokens);
        JsonAssert.Equal(
            DoStreamSupport.SnapshotOf(finish)["usage"],
            """
            {
              "inputTokens": { "cacheRead": 0, "cacheWrite": 0, "noCache": 69, "total": 69 },
              "outputTokens": { "total": 53 },
              "raw": {
                "cache_creation": {
                  "ephemeral_1h_input_tokens": 0,
                  "ephemeral_5m_input_tokens": 0
                },
                "cache_creation_input_tokens": 0,
                "cache_read_input_tokens": 0,
                "inference_geo": "not_available",
                "input_tokens": 69,
                "output_tokens": 53,
                "service_tier": "standard"
              }
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream combined clear_thinking and clear_tool_uses response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_combined_clear_thinking_and_clear_tool_uses_response()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-combined-context-editing.1")
            ),
            "claude-3-haiku-20240307"
        );

        var reasoningDeltas = parts
            .OfType<ReasoningDeltaStreamPart>()
            .Where(part =>
                part.ProviderMetadata is not { } metadata
                || !metadata.TryGetProperty("anthropic", out _)
            )
            .ToList();
        Assert.True(reasoningDeltas.Count > 0);
        var fullReasoning = string.Concat(reasoningDeltas.Select(delta => delta.Delta));
        Assert.Contains("25 * 37", fullReasoning);
        Assert.Contains("925", fullReasoning);

        var signatureParts = parts
            .OfType<ReasoningDeltaStreamPart>()
            .Where(part =>
                part.ProviderMetadata is { } metadata && metadata.TryGetProperty("anthropic", out _)
            )
            .ToList();
        Assert.Single(signatureParts);

        var textDeltas = parts.OfType<TextDeltaStreamPart>().Where(part => part.Id == "1").ToList();
        Assert.True(textDeltas.Count > 0);
        Assert.Contains("925", string.Concat(textDeltas.Select(delta => delta.Delta)));

        JsonAssert.Equal(
            DoStreamSupport.AnthropicMetadata(DoStreamSupport.Finish(parts), "contextManagement"),
            "{\"appliedEdits\":[]}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream tool deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_deltas()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                AnthropicParity.Json(
                    """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}"""
                )
            ),
        };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01GouTqNCGXzrj5LQ5jEkw67","type":"message","role":"assistant","model":"claude-3-haiku-20240307","stop_sequence":null,"usage":{"input_tokens":441,"output_tokens":2},"content":[],"stop_reason":null}            }""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}      }""",
                    """{"type": "ping"}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Okay"}    }""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"!"}   }""",
                    """{"type":"content_block_stop","index":0    }""",
                    """{"type":"content_block_start","index":1,"content_block":{"type":"tool_use","id":"toolu_01DBsB4vvYLnBDzZ5rBSxSLs","name":"test-tool","input":{}}      }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":""}           }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"{\"value"}              }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"\":"}      }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"\"Spark"}          }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":"le"}          }""",
                    """{"type":"content_block_delta","index":1,"delta":{"type":"input_json_delta","partial_json":" Day\"}"}               }""",
                    """{"type":"content_block_stop","index":1              }""",
                    """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":65}           }""",
                    """{"type":"message_stop"           }"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "stream-tool-deltas");
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward error chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_error_chunks()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}      }""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}          }""",
                    """{"type": "ping"}""",
                    """{"type":"error","error":{"type":"error","message":"test error"}}"""
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "forward-error-chunks");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the messages and the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_messages_and_the_model()
    {
        var handler = DoStreamSupport.Stream(
            """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
            """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
            """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello, World!"}}""",
            """{"type":"content_block_stop","index":0}""",
            """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
            """{"type":"message_stop"}"""
        );

        await DoStreamSupport
            .Provider(handler)
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None);

        JsonAssert.Equal(
            JsonNode.Parse(handler.Body),
            """
            {
              "stream": true,
              "model": "claude-3-haiku-20240307",
              "max_tokens": 4096,
              "messages": [{ "role": "user", "content": [{ "type": "text", "text": "Hello" }] }]
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = DoStreamSupport.SseBody(
                """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello, World!"}}""",
                """{"type":"content_block_stop","index":0}""",
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
                """{"type":"message_stop"}"""
            ),
        };
        var provider = AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                Headers = { ["Custom-Provider-Header"] = "provider-header-value" },
            },
            handler
        );
        var options = AnthropicParity.Hello();
        options.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };

        await provider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(options, CancellationToken.None);

        DoStreamSupport.AssertSnapshot(DoStreamSupport.RequestHeaders(handler), "pass-headers");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should merge custom anthropic-beta headers without legacy fine-grained-tool-streaming beta",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_custom_anthropic_beta_headers_without_legacy_fine_grained_tool_streaming_beta()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = DoStreamSupport.SseBody(
                """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello, World!"}}""",
                """{"type":"content_block_stop","index":0}""",
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
                """{"type":"message_stop"}"""
            ),
        };
        var provider = AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                Headers = { ["anthropic-beta"] = "CONFIG-beta1,config-beta2" },
            },
            handler
        );
        var options = AnthropicParity.Hello();
        options.Headers = new Dictionary<string, string?>
        {
            ["anthropic-beta"] = "REQUEST-beta1,request-beta2",
        };

        await provider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(options, CancellationToken.None);

        DoStreamSupport.AssertSnapshot(
            JsonValue.Create(handler.Headers["anthropic-beta"]),
            "merge-anthropic-beta-headers"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should default to per-tool eager_input_streaming on streaming requests and not send the legacy beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_to_per_tool_eager_input_streaming_on_streaming_requests_and_not_send_the_legacy_beta_header()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = DoStreamSupport.SseBody(
                """{"type":"message_start","message":{"id":"msg_1","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":1}}}""",
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":1}}""",
                """{"type":"message_stop"}"""
            ),
        };
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-api-key" },
            handler
        );
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get_weather",
                "Get weather",
                AnthropicParity.Json("""{"type":"object","properties":{}}""")
            ),
        };

        await provider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.True((bool?)body["tools"]![0]!["eager_input_streaming"]);
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not add eager_input_streaming when toolStreaming is explicitly false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_add_eager_input_streaming_when_toolStreaming_is_explicitly_false()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = DoStreamSupport.SseBody(
                """{"type":"message_start","message":{"id":"msg_1","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":1}}}""",
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":1}}""",
                """{"type":"message_stop"}"""
            ),
        };
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-api-key" },
            handler
        );
        var options = AnthropicParity.WithProvider("{\"toolStreaming\":false}");
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get_weather",
                "Get weather",
                AnthropicParity.Json("""{"type":"object","properties":{}}""")
            ),
        };

        await provider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["tools"]![0]!["eager_input_streaming"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not default eager_input_streaming on non-streaming (generate) calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_default_eager_input_streaming_on_non_streaming_generate_calls()
    {
        var handler = AnthropicParity.Ok(
            """{"id":"msg_1","type":"message","role":"assistant","content":[{"type":"text","text":"ok"}],"model":"claude-3-haiku-20240307","stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":1}}"""
        );
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get_weather",
                "Get weather",
                AnthropicParity.Json("""{"type":"object","properties":{}}""")
            ),
        };

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["tools"]![0]!["eager_input_streaming"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include providerOptions.anthropic.anthropicBeta in anthropic-beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_providerOptions_anthropic_anthropicBeta_in_anthropic_beta_header()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = DoStreamSupport.SseBody(
                """{"type":"message_start","message":{"id":"msg_1","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
                """{"type":"content_block_stop","index":0}""",
                """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":5}}""",
                """{"type":"message_stop"}"""
            ),
        };
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-api-key" },
            handler
        );
        var options = AnthropicParity.WithProvider(
            "{\"anthropicBeta\":[\"my-beta-2025-01-01\",\"another-beta-2025-06-01\"]}"
        );

        await provider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(options, CancellationToken.None);

        Assert.Contains("my-beta-2025-01-01", handler.Headers["anthropic-beta"]);
        Assert.Contains("another-beta-2025-06-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support cache control", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_cache_control()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1,"cache_creation_input_tokens":10,"cache_read_input_tokens":5}}      }""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}          }""",
                    """{"type": "ping"}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}              }""",
                    """{"type":"content_block_stop","index":0             }""",
                    """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}          }""",
                    """{"type":"message_stop"           }"""
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "stream-support-cache-control");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support cache control and return extra fields in provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_cache_control_and_return_extra_fields_in_provider_metadata()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1,"cache_creation_input_tokens":10,"cache_read_input_tokens":5,"cache_creation":{"ephemeral_5m_input_tokens":0,"ephemeral_1h_input_tokens":10}}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}          }""",
                    """{"type": "ping"}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}              }""",
                    """{"type":"content_block_stop","index":0             }""",
                    """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227}          }""",
                    """{"type":"message_stop"           }"""
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "stream-cache-control-extra-fields");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support cache tokens in message_delta event",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_cache_tokens_in_message_delta_event()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                    """{"type": "ping"}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":227,"cache_creation_input_tokens":10,"cache_read_input_tokens":5}}""",
                    """{"type":"message_stop"}"""
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(parts, "cache-tokens-in-message-delta");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle handle stop_reason:pause_turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_handle_stop_reason_pause_turn()
    {
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":", "}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"World!"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"pause_turn","stop_sequence":null},"usage":{"output_tokens":227}}""",
                    """{"type":"message_stop"}"""
                )
            ),
            "claude-3-haiku-20240307"
        );

        DoStreamSupport.AssertStreamSnapshot(
            parts.Where(part => part is FinishStreamPart).ToList(),
            "pause-turn"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include stop_sequence in provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_stop_sequence_in_provider_metadata()
    {
        var options = AnthropicParity.Hello();
        options.StopSequences = new[] { "STOP" };
        var parts = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.Stream(
                    """{"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}""",
                    """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
                    """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
                    """{"type":"content_block_stop","index":0}""",
                    """{"type":"message_delta","delta":{"stop_reason":"stop_sequence","stop_sequence":"STOP"},"usage":{"output_tokens":227}}""",
                    """{"type":"message_stop"}"""
                )
            ),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertStreamSnapshot(
            parts.Where(part => part is FinishStreamPart).ToList(),
            "stop-sequence-metadata"
        );
    }

    [Theory]
    [InlineData("overloaded_error", "Overloaded", 529, true)]
    [InlineData("api_error", "Internal server error", 500, true)]
    [InlineData("request_too_large", "Request too large", 413, false)]
    [UpstreamTest(
        Prefix + "should attach provider-owned metadata to a mid-stream $type error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_attach_provider_owned_metadata_to_a_mid_stream_type_error(
        string type,
        string message,
        int statusCode,
        bool isRetryable
    )
    {
        var handler = new SseStreamHandler([
            """data: {"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}"""
                + "\n\n",
            """data: {"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}"""
                + "\n\n",
            """data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}"""
                + "\n\n",
            "event: error\n",
            "data: "
                + new JsonObject
                {
                    ["type"] = "error",
                    ["error"] = new JsonObject { ["type"] = type, ["message"] = message },
                }.ToJsonString()
                + "\n\n",
        ]);

        var chunks = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(handler),
            "claude-3-haiku-20240307"
        );
        var errorPart = chunks.OfType<ErrorStreamPart>().FirstOrDefault();

        Assert.Equal(6, chunks.Count);
        var start = Assert.IsType<StreamStartStreamPart>(chunks[0]);
        Assert.Empty(start.Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(chunks[1]);
        Assert.Equal("msg_01KfpJoAEabmH2iHRRFjQMAG", metadata.Id);
        Assert.Equal("claude-3-haiku-20240307", metadata.ModelId);
        Assert.Equal("anthropic.message_start", Assert.IsType<CustomStreamPart>(chunks[2]).Kind);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(chunks[3]).Id);
        var delta = Assert.IsType<TextDeltaStreamPart>(chunks[4]);
        Assert.Equal("0", delta.Id);
        Assert.Equal("Hello", delta.Delta);
        Assert.IsType<ErrorStreamPart>(chunks[5]);
        Assert.NotNull(errorPart);
        var error = errorPart!.Error;
        Assert.NotNull(error);
        Assert.Equal(message, error!.Message);
        Assert.Equal(type, error.Type);
        Assert.Equal(statusCode, error.StatusCode);
        Assert.Equal(isRetryable, error.IsRetryable);
        JsonAssert.Equal(
            (JsonNode)error.Data!,
            new JsonObject { ["message"] = message, ["type"] = type }.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve explicit metadata from an Anthropic-compatible transport",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_explicit_metadata_from_an_Anthropic_compatible_transport()
    {
        var data = new JsonObject
        {
            ["message"] = "The model stream failed",
            ["originalStatusCode"] = 503,
        };
        var error = new JsonObject
        {
            ["type"] = "modelStreamErrorException",
            ["code"] = "bedrock_stream_error",
            ["message"] = "The model stream failed",
            ["statusCode"] = 424,
            ["isRetryable"] = true,
            ["data"] = data.DeepClone(),
        };
        var handler = new SseStreamHandler([
            """data: {"type":"message_start","message":{"id":"msg_01KfpJoAEabmH2iHRRFjQMAG","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":17,"output_tokens":1}}}"""
                + "\n\n",
            "event: error\n",
            "data: "
                + new JsonObject { ["type"] = "error", ["error"] = error }.ToJsonString()
                + "\n\n",
        ]);

        var chunks = await DoStreamSupport.Collect(
            DoStreamSupport.Provider(handler),
            "claude-3-haiku-20240307"
        );
        var errorPart = Assert.Single(chunks.OfType<ErrorStreamPart>());

        var providerError = errorPart.Error;
        Assert.NotNull(providerError);
        Assert.Equal("The model stream failed", providerError!.Message);
        Assert.Equal("modelStreamErrorException", providerError.Type);
        Assert.Equal("bedrock_stream_error", providerError.Code);
        Assert.Equal(424, providerError.StatusCode);
        Assert.True(providerError.IsRetryable);
        JsonAssert.Equal((JsonNode)providerError.Data!, data.ToJsonString());
    }

    private static async Task<List<LanguageModelStreamPart>> StreamAsync(string sse)
    {
        var handler = new CaptureHandler { ContentType = "text/event-stream", ResponseBody = sse };
        var parts = new List<LanguageModelStreamPart>();
        await foreach (
            var part in await AnthropicParity
                .Client(handler)
                .LanguageModel("claude-opus-5")
                .DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None)
        )
        {
            parts.Add(part);
        }

        return parts;
    }

    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::";

    private static readonly string[] Chunks =
    {
        "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}\n\n",
        "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n",
        "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello, World!\"}}\n\n",
        "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n",
        "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":227}}\n\n",
        "data: {\"type\":\"message_stop\"}\n\n",
    };

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var result = await Stream(
            new SseStreamHandler(
                Chunks,
                new Dictionary<string, string> { ["test-header"] = "test-value" }
            )
        );

        SseStreamHandler.AssertDefaultHeaders(result.Response!.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body()
    {
        var result = await Stream(
            new SseStreamHandler(
                Chunks,
                new Dictionary<string, string> { ["test-header"] = "test-value" }
            )
        );

        var expected = JsonNode.Parse(
            """
            {"model":"claude-3-haiku-20240307","max_tokens":4096,"messages":[{"role":"user","content":[{"type":"text","text":"Hello"}]}],"stream":true}
            """
        );
        Assert.True(
            JsonNode.DeepEquals(expected, JsonNode.Parse(result.Request!.Body!.Value.GetRawText()))
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "preserves startup error handling after pings (raw: %s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "97540149254736e8d566d432ccb7abf7b1cebbb3"
    )]
    public async Task Preserves_startup_error_handling_after_pings(bool includeRawChunks)
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "data: {\"type\":\"ping\"}\n\n",
                "data: {\"type\":\"ping\"}\n\n",
                "data: {\"type\":\"error\",\"error\":{\"type\":\"overloaded_error\",\"message\":\"Overloaded\"}}\n\n",
            }
        );
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-api-key" },
            handler
        );
        var options = AnthropicParity.WithProvider("{\"includePings\":true}");
        options.IncludeRawChunks = includeRawChunks;

        var exception = await Assert.ThrowsAsync<InternalServerException>(() =>
            provider
                .LanguageModel("claude-3-haiku-20240307")
                .DoStreamAsync(options, CancellationToken.None)
        );

        Assert.Equal("Overloaded", exception.Message);
        Assert.True(ProviderHttp.IsRetryable(exception.StatusCode));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw an api error when the first stream chunk is an overloaded error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_an_api_error_when_the_first_stream_chunk_is_an_overloaded_error()
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "event: error\n",
                "data: {\"type\":\"error\",\"error\":{\"details\":null,\"type\":\"overloaded_error\",\"message\":\"Overloaded\"}}\n",
                "\n",
            }
        );
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-api-key" },
            handler
        );

        var exception = await Assert.ThrowsAsync<InternalServerException>(() =>
            provider
                .LanguageModel("claude-3-haiku-20240307")
                .DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None)
        );

        Assert.Equal("Overloaded", exception.Message);
        Assert.Equal("https://api.anthropic.com/v1/messages", exception.Url);
        var requestBody = Assert.IsAssignableFrom<JsonNode>(exception.RequestBodyValues);
        Assert.Equal("claude-3-haiku-20240307", (string?)requestBody["model"]);
        Assert.Equal(4096, (int?)requestBody["max_tokens"]);
        Assert.True((bool?)requestBody["stream"]);
        Assert.Equal("Hello", (string?)requestBody["messages"]![0]!["content"]![0]!["text"]);
        Assert.Equal(529, exception.StatusCode);
        Assert.True(ProviderHttp.IsRetryable(exception.StatusCode));
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"overloaded_error\",\"message\":\"Overloaded\"}"),
                JsonNode.Parse(exception.ResponseBody!)
            )
        );
        Assert.Equal("no-cache", exception.ResponseHeaders!["cache-control"]);
        Assert.Equal("keep-alive", exception.ResponseHeaders["connection"]);
        Assert.Equal("text/event-stream", exception.ResponseHeaders["content-type"]);
    }

    private static async Task<Vercel.AI.Provider.LanguageModelStreamResult> Stream(
        SseStreamHandler handler
    )
    {
        var provider = AnthropicProvider.Create(
            new AnthropicOptions { ApiKey = "test-api-key" },
            handler
        );
        var result = await provider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None);
        await foreach (var _ in result) { }

        return result;
    }
}
