// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning summary text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_summary_text_deltas()
    {
        var parts = await Collect(Model(ChunksFixture("openai-reasoning-summary-text.1")), UpstreamChat.Prompt());

        JsonAssert.Equal(ToJson(parts.Where(part => part.Type.StartsWith("reasoning", StringComparison.Ordinal))), """
            [
              {"type":"reasoning-start","id":"rs_1"},
              {"type":"reasoning-delta","id":"rs_1","delta":"Think"},
              {"type":"reasoning-delta","id":"rs_1","delta":"ing."},
              {"type":"reasoning-end","id":"rs_1","providerMetadata":{"lmstudio":{"itemId":"rs_1","reasoningSummary":[{"type":"summary_text","text":"Thinking."}],"reasoningContent":null}}}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves $expectedType stream errors", Coverage = UpstreamCoverage.Partial, Note = "Checks both cases: the error message and the error finish. ErrorStreamPart carries only a message, so the error type, code, and data are not asserted. Issue #366.")]
    public async Task Preserves_expectedType_stream_errors()
    {
        foreach (var (json, message) in new[]
        {
            ("{\"type\":\"response.failed\",\"sequence_number\":1,\"response\":{\"status\":\"failed\",\"error\":{\"code\":\"429\",\"message\":\"Rate limit reached\"}}}", "Rate limit reached"),
            ("{\"type\":\"error\",\"sequence_number\":1,\"error\":{\"code\":\"503\",\"message\":\"Service unavailable\"}}", "Service unavailable"),
        })
        {
            var parts = await Collect(Model(StreamResponse(json)), UpstreamChat.Prompt());

            Assert.Equal(message, parts.OfType<ErrorStreamPart>().First().Message);
            Assert.Equal(FinishReason.Error, Assert.IsType<FinishStreamPart>(parts[^1]).FinishReason);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve output text annotations in stream metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_output_text_annotations_in_stream_metadata()
    {
        const string annotation = """{"type":"url_citation","start_index":0,"end_index":7,"url":"https://example.com/source","title":"Example source"}""";
        var capture = StreamResponse(
            """{"type":"response.output_item.added","sequence_number":0,"output_index":0,"item":{"id":"msg_annotated","type":"message","role":"assistant","status":"in_progress","content":[]}}""",
            """{"type":"response.output_text.delta","sequence_number":1,"item_id":"msg_annotated","output_index":0,"content_index":0,"delta":"Sourced answer"}""",
            """{"type":"response.output_item.done","sequence_number":2,"output_index":0,"item":{"id":"msg_annotated","type":"message","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Sourced answer","annotations":[ANNOTATION]}]}}""".Replace("ANNOTATION", annotation));

        var parts = await Collect(Model(capture), UpstreamChat.Prompt());

        JsonAssert.Equal(
            ToJson(parts.OfType<TextEndStreamPart>().Single()),
            """{"type":"text-end","id":"msg_annotated","providerMetadata":{"lmstudio":{"itemId":"msg_annotated","annotations":[ANNOTATION]}}}""".Replace("ANNOTATION", annotation));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send provider-native reasoning effort when streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_provider_native_reasoning_effort_when_streaming()
    {
        var capture = ChunksFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningEffort\":\"max\"}");

        await UpstreamChat.Read(Model(capture).DoStreamAsync(options, CancellationToken.None));

        Assert.Equal("{\"effort\":\"max\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should close unfinished reasoning items with their original ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_close_unfinished_reasoning_items_with_their_original_ids()
    {
        var capture = StreamResponse(
            """{"type":"response.output_item.added","sequence_number":0,"output_index":0,"item":{"id":"rs_reasoning_item","type":"reasoning","summary":[]}}""",
            """{"type":"response.incomplete","sequence_number":1,"response":{"status":"incomplete","incomplete_details":{"reason":"max_output_tokens"},"usage":{"input_tokens":1,"input_tokens_details":{"cached_tokens":0},"output_tokens":1,"output_tokens_details":{"reasoning_tokens":1},"total_tokens":2}}}""");

        var parts = await Collect(Model(capture), UpstreamChat.Prompt());

        var json = ToJson(parts).Select(part => part!.ToJsonString()).ToList();
        Assert.Contains("""{"type":"reasoning-start","id":"rs_reasoning_item"}""", json);
        Assert.Contains("""{"type":"reasoning-end","id":"rs_reasoning_item"}""", json);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Length, finish.FinishReason);
        Assert.Equal("max_output_tokens", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not pollute Object.prototype from tool call item ids", Coverage = UpstreamCoverage.Covered, Note = "C# has no object prototype. The case checks that a __proto__ item id is an ordinary key: the stream finishes and emits no tool call.")]
    public async Task Should_not_pollute_Object_prototype_from_tool_call_item_ids()
    {
        var capture = StreamResponse(
            """{"type":"response.function_call_arguments.done","item_id":"__proto__","output_index":0,"arguments":"polluted","sequence_number":0}""",
            """{"type":"response.completed","response":{"incomplete_details":null,"status":"completed","usage":{"input_tokens":0,"input_tokens_details":{"cached_tokens":0},"output_tokens":0,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":0}},"sequence_number":1}""");

        var parts = await Collect(Model(capture), UpstreamChat.Prompt());

        Assert.Empty(parts.OfType<ToolCallStreamPart>());
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[^1]).FinishReason);
    }
}
