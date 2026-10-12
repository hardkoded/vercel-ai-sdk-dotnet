// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; manual history replay</c>.</summary>
public sealed class DoGenerateManualHistoryReplayTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > manual history replay::";

    [Fact]
    [UpstreamTest(Prefix + "should preserve output item order and ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_output_item_order_and_ids()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = OutputResponse("""
                [
                  {"id":"rs_1","type":"reasoning","status":"completed","summary":[],"content":[{"type":"reasoning_text","text":"reasoning"}]},
                  {"id":"fc_1","type":"function_call","status":"completed","call_id":"call_1","name":"search","arguments":"{}"},
                  {"id":"msg_1","type":"message","role":"assistant","status":"completed","content":[{"type":"output_text","text":"answer after the call","annotations":[]}]}
                ]
                """),
        };

        var model = CreateModel(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        Assert.True(JsonEquals("""
            [
              {"id":"rs_1","type":"reasoning","summary":[],"content":[{"type":"reasoning_text","text":"reasoning"}]},
              {"id":"fc_1","type":"function_call","call_id":"call_1","name":"search","arguments":"{}"},
              {"id":"msg_1","type":"message","role":"assistant","content":[{"type":"output_text","text":"answer after the call"}]}
            ]
            """, RequestInput(capture, 1)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve summary and encrypted-only reasoning items", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_summary_and_encrypted_only_reasoning_items()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = OutputResponse("""
                [{"id":"rs_2","type":"reasoning","status":"completed","summary":[{"type":"summary_text","text":"safe summary"}],"encrypted_content":"opaque-provider-state"}]
                """),
        };

        var model = CreateModel(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var reasoning = Assert.IsType<GeneratedReasoning>(Assert.Single(first.Content));
        Assert.Equal("safe summary", reasoning.Text);
        Assert.True(JsonEquals("""
            {"open-responses":{"itemId":"rs_2","reasoningContent":null,"reasoningSummary":[{"type":"summary_text","text":"safe summary"}],"reasoningEncryptedContent":"opaque-provider-state"}}
            """, System.Text.Json.Nodes.JsonNode.Parse(reasoning.ProviderMetadata!.Value.GetRawText())));

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        Assert.True(JsonEquals("""
            [{"id":"rs_2","type":"reasoning","summary":[{"type":"summary_text","text":"safe summary"}],"encrypted_content":"opaque-provider-state"}]
            """, RequestInput(capture, 1)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve output text annotations", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_output_text_annotations()
    {
        const string annotation = """{"type":"url_citation","start_index":0,"end_index":7,"url":"https://example.com/source","title":"Example source"}""";
        var capture = new UpstreamCapture
        {
            ResponseBody = OutputResponse("""
                [{"id":"msg_annotated","type":"message","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Sourced answer","annotations":[@ANNOTATION@]}]}]
                """.Replace("@ANNOTATION@", annotation)),
        };

        var model = CreateModel(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(first.Content));
        Assert.Equal("Sourced answer", text.Text);
        Assert.True(JsonEquals("""{"open-responses":{"itemId":"msg_annotated","annotations":[@ANNOTATION@]}}""".Replace("@ANNOTATION@", annotation), System.Text.Json.Nodes.JsonNode.Parse(text.ProviderMetadata!.Value.GetRawText())));

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        Assert.True(JsonEquals("""
            [{"id":"msg_annotated","type":"message","role":"assistant","content":[{"type":"output_text","text":"Sourced answer","annotations":[@ANNOTATION@]}]}]
            """.Replace("@ANNOTATION@", annotation), RequestInput(capture, 1)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve reasoning content part boundaries", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_reasoning_content_part_boundaries()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = OutputResponse("""
                [{"id":"rs_multiple","type":"reasoning","status":"completed","summary":[],"content":[{"type":"reasoning_text","text":"First thought. "},{"type":"reasoning_text","text":"Second thought."}]}]
                """),
        };

        var model = CreateModel(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Equal(2, first.Content.Count);
        Assert.Equal(new[] { "reasoning", "reasoning" }, first.Content.Select(part => part.Type).ToArray());
        Assert.Equal(new[] { "First thought. ", "Second thought." }, first.Content.Select(part => ((GeneratedReasoning)part).Text).ToArray());

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        Assert.True(JsonEquals("""
            [{"id":"rs_multiple","type":"reasoning","summary":[],"content":[{"type":"reasoning_text","text":"First thought. "},{"type":"reasoning_text","text":"Second thought."}]}]
            """, RequestInput(capture, 1)));
    }
}
