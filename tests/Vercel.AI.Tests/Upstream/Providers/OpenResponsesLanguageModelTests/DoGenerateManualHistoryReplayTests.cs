// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; manual history replay</c>.</summary>
public sealed class DoGenerateManualHistoryReplayTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > manual history replay::";

    private const string Annotation = """{"type":"url_citation","start_index":0,"end_index":7,"url":"https://example.com/source","title":"Example source"}""";

    [Fact]
    [UpstreamTest(Prefix + "should preserve output item order and ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_output_item_order_and_ids()
    {
        var capture = OutputResponse("""
            [
              {"id":"rs_1","type":"reasoning","status":"completed","summary":[],"content":[{"type":"reasoning_text","text":"reasoning"}]},
              {"id":"fc_1","type":"function_call","status":"completed","call_id":"call_1","name":"search","arguments":"{}"},
              {"id":"msg_1","type":"message","role":"assistant","status":"completed","content":[{"type":"output_text","text":"answer after the call","annotations":[]}]}
            ]
            """);

        var model = Model(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        JsonAssert.Equal(SecondInput(capture), """
            [
              {"id":"rs_1","type":"reasoning","summary":[],"content":[{"type":"reasoning_text","text":"reasoning"}]},
              {"id":"fc_1","type":"function_call","call_id":"call_1","name":"search","arguments":"{}"},
              {"id":"msg_1","type":"message","role":"assistant","content":[{"type":"output_text","text":"answer after the call"}]}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve summary and encrypted-only reasoning items", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_summary_and_encrypted_only_reasoning_items()
    {
        var capture = OutputResponse("""
            [{"id":"rs_2","type":"reasoning","status":"completed","summary":[{"type":"summary_text","text":"safe summary"}],"encrypted_content":"opaque-provider-state"}]
            """);

        var model = Model(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var reasoning = Assert.IsType<GeneratedReasoning>(Assert.Single(first.Content));
        Assert.Equal("safe summary", reasoning.Text);
        JsonAssert.Equal(reasoning.ProviderMetadata!.Value, """
            {"lmstudio":{"itemId":"rs_2","reasoningContent":null,"reasoningSummary":[{"type":"summary_text","text":"safe summary"}],"reasoningEncryptedContent":"opaque-provider-state"}}
            """);

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        JsonAssert.Equal(SecondInput(capture), """
            [{"id":"rs_2","type":"reasoning","summary":[{"type":"summary_text","text":"safe summary"}],"encrypted_content":"opaque-provider-state"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve output text annotations", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_output_text_annotations()
    {
        var capture = OutputResponse("""
            [{"id":"msg_annotated","type":"message","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Sourced answer","annotations":[@ANNOTATION@]}]}]
            """.Replace("@ANNOTATION@", Annotation));

        var model = Model(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(first.Content));
        Assert.Equal("Sourced answer", text.Text);
        JsonAssert.Equal(text.ProviderMetadata!.Value, """{"lmstudio":{"itemId":"msg_annotated","annotations":[@ANNOTATION@]}}""".Replace("@ANNOTATION@", Annotation));

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        JsonAssert.Equal(SecondInput(capture), """
            [{"id":"msg_annotated","type":"message","role":"assistant","content":[{"type":"output_text","text":"Sourced answer","annotations":[@ANNOTATION@]}]}]
            """.Replace("@ANNOTATION@", Annotation));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve reasoning content part boundaries", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_reasoning_content_part_boundaries()
    {
        var capture = OutputResponse("""
            [{"id":"rs_multiple","type":"reasoning","status":"completed","summary":[],"content":[{"type":"reasoning_text","text":"First thought. "},{"type":"reasoning_text","text":"Second thought."}]}]
            """);

        var model = Model(capture);
        var first = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Equal(new[] { "reasoning", "reasoning" }, first.Content.Select(part => part.Type).ToArray());
        Assert.Equal(new[] { "First thought. ", "Second thought." }, first.Content.Select(part => ((GeneratedReasoning)part).Text).ToArray());

        await model.DoGenerateAsync(Replay(first.Content), CancellationToken.None);

        JsonAssert.Equal(SecondInput(capture), """
            [{"id":"rs_multiple","type":"reasoning","summary":[],"content":[{"type":"reasoning_text","text":"First thought. "},{"type":"reasoning_text","text":"Second thought."}]}]
            """);
    }

    private static JsonNode? SecondInput(UpstreamCapture capture)
    {
        return JsonNode.Parse(capture.Requests[1].Body)!["input"];
    }

    [Fact]
    [UpstreamTest(Prefix + "should decode and losslessly replay a registered hosted-tool receipt", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_decode_and_losslessly_replay_a_registered_hosted_tool_receipt()
    {
        const string receiptJson = "{\"id\":\"search_1\",\"type\":\"acme:document_search_receipt\",\"status\":\"completed\",\"call_id\":\"call_1\",\"name\":\"documentSearch\",\"provider_executed\":false,\"query\":{\"text\":\"climate\"},\"result\":{\"documents\":[{\"id\":\"doc_1\",\"score\":0.9}]},\"opaque_receipt\":{\"trace_id\":\"trace_1\",\"implementation_version\":3}}";
        const string reference = "{\"lmstudio\":{\"openResponsesExtension\":{\"id\":\"acme.document_search\",\"itemId\":\"search_1\"}}}";
        var capture = new UpstreamCapture();
        PrepareOutputResponse(capture, Obj(receiptJson));

        var model = CreateModel(capture, "gemma-7b-it", CreateDocumentSearchExtension(providerExecuted: true));
        var first = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(3, first.Content.Count);
        var replay = Assert.IsType<GeneratedCustom>(first.Content[0]);
        Assert.Equal("open-responses.extension-replay", replay.Kind);
        AssertMetadata("{\"lmstudio\":{\"openResponsesExtension\":{\"id\":\"acme.document_search\",\"item\":" + receiptJson + "}}}", replay.ProviderMetadata);
        var call = Assert.IsType<GeneratedToolCall>(first.Content[1]);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("documentSearch", call.ToolName);
        Assert.Equal("{\"text\":\"climate\"}", call.ArgumentsJson);
        Assert.False(call.ProviderExecuted);
        AssertMetadata(reference, call.ProviderMetadata);
        var result = Assert.IsType<GeneratedToolResult>(first.Content[2]);
        Assert.Equal("call_1", result.ToolCallId);
        Assert.Equal("documentSearch", result.ToolName);
        AssertJson(Obj("{\"documents\":[{\"id\":\"doc_1\",\"score\":0.9}]}"), JsonNode.Parse(result.ResultJson));
        AssertMetadata(reference, result.ProviderMetadata);

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new AssistantModelMessage(new AssistantContentPart[]
                    {
                        new AssistantCustomPart("open-responses.extension-replay", Options(replay.ProviderMetadata)),
                        new AssistantToolCallPart(new GeneratedToolCall("call_1", "documentSearch", "{\"text\":\"climate\"}"), Options(call.ProviderMetadata)),
                        new AssistantToolResultPart(
                            "call_1",
                            "documentSearch",
                            new ToolModelMessage("call_1", "documentSearch", "{\"documents\":[{\"id\":\"doc_1\",\"score\":0.9}]}", false, "json"),
                            Options(result.ProviderMetadata)),
                    }),
                },
            },
            CancellationToken.None);

        AssertJson(Arr("[" + receiptJson + "]"), RequestBody(capture, 1)["input"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should replay a source-only extension item through response history", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_replay_a_source_only_extension_item_through_response_history()
    {
        const string sourceItem = "{\"id\":\"source_1\",\"type\":\"acme:document_search_receipt\",\"status\":\"completed\",\"url\":\"https://example.com/documentation\",\"title\":\"Extension documentation\",\"opaque_receipt\":{\"trace_id\":\"trace_source_1\"}}";
        var capture = new UpstreamCapture();
        PrepareOutputResponse(capture, Obj(sourceItem));

        var extension = CreateDocumentSearchExtension(providerExecuted: true);
        extension.DecodeItem = context => new GeneratedContent[]
        {
            new GeneratedSource((string)context.Item["id"]!, (string)context.Item["url"]!, (string)context.Item["title"]!),
        };

        var model = CreateModel(capture, "gemma-7b-it", extension);
        var first = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(2, first.Content.Count);
        var replay = Assert.IsType<GeneratedCustom>(first.Content[0]);
        Assert.Equal("open-responses.extension-replay", replay.Kind);
        AssertMetadata("{\"lmstudio\":{\"openResponsesExtension\":{\"id\":\"acme.document_search\",\"item\":" + sourceItem + "}}}", replay.ProviderMetadata);
        var source = Assert.IsType<GeneratedSource>(first.Content[1]);
        Assert.Equal("source_1", source.Id);
        Assert.Equal("https://example.com/documentation", source.Url);
        Assert.Equal("Extension documentation", source.Title);
        AssertMetadata("{\"lmstudio\":{\"openResponsesExtension\":{\"id\":\"acme.document_search\",\"itemId\":\"source_1\"}}}", source.ProviderMetadata);

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new AssistantModelMessage(new AssistantContentPart[]
                    {
                        new AssistantCustomPart("open-responses.extension-replay", Options(replay.ProviderMetadata)),
                    }),
                },
            },
            CancellationToken.None);

        AssertJson(Arr("[" + sourceItem + "]"), RequestBody(capture, 1)["input"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should encode client-executed extension calls and results without original wire metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_client_executed_extension_calls_and_results_without_original_wire_metadata()
    {
        var capture = new UpstreamCapture();
        PrepareJsonFixtureResponse(capture, "lmstudio-basic.1");

        await CreateModel(capture, "gemma-7b-it", CreateDocumentSearchExtension(providerExecuted: false)).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new AssistantModelMessage(new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(new GeneratedToolCall("call_client", "documentSearch", "{\"text\":\"weather\"}")),
                    }),
                    new ToolModelMessage("call_client", "documentSearch", "{\"documents\":[\"forecast\"]}", false, "json"),
                },
                Tools = new[]
                {
                    LanguageModelTool.ProviderDefined("acme.document_search", "documentSearch", JsonDocumentElement("{}")),
                },
            },
            CancellationToken.None);

        AssertJson(
            Arr("[{\"type\":\"acme:document_search_call\",\"id\":\"call_item_call_client\",\"status\":\"completed\",\"call_id\":\"call_client\",\"name\":\"documentSearch\",\"query\":{\"text\":\"weather\"}},{\"type\":\"acme:document_search_result\",\"id\":\"result_item_call_client\",\"status\":\"completed\",\"call_id\":\"call_client\",\"name\":\"documentSearch\",\"result\":{\"type\":\"json\",\"value\":{\"documents\":[\"forecast\"]}}}]"),
            RequestBody(capture, 0)["input"]);
    }

    private static System.Text.Json.JsonElement JsonDocumentElement(string json)
    {
        return UpstreamChat.Json(json);
    }
}
