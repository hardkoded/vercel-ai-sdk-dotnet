// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; reasoning content</c>.</summary>
public sealed class DoGenerateReasoningContentTests
{
    private const string Prefix = TestFile + " > doGenerate > reasoning content::";

    [Fact]
    [UpstreamTest(Prefix + "should extract reasoning with encrypted content when store=false", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_with_encrypted_content_when_store_false()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[
               {"type":"reasoning","id":"rs_456","status":"completed","summary":[{"type":"summary_text","text":"First, analyze the question carefully."}],"encrypted_content":"abc123encryptedcontent"},
               {"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"The answer is 42.","annotations":[]}]}],
             "usage":{"input_tokens":10,"output_tokens":20,"output_tokens_details":{"reasoning_tokens":15}}}
            """);

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        var reasoning = Assert.IsType<XaiReasoning>(result.Content[0]);
        Assert.Equal("First, analyze the question carefully.", reasoning.Text);
        JsonAssert.Equal(reasoning.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456","reasoningEncryptedContent":"abc123encryptedcontent"}}""");
        Assert.Equal("The answer is 42.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(2, result.Content.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle reasoning without encrypted content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_reasoning_without_encrypted_content()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[
               {"type":"reasoning","id":"rs_456","status":"completed","summary":[{"type":"summary_text","text":"Thinking through the problem."}]},
               {"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Solution found.","annotations":[]}]}],
             "usage":{"input_tokens":10,"output_tokens":15}}
            """);

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        var reasoning = Assert.IsType<XaiReasoning>(result.Content[0]);
        Assert.Equal("Thinking through the problem.", reasoning.Text);
        JsonAssert.Equal(reasoning.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456"}}""");
        Assert.Equal("Solution found.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(2, result.Content.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract reasoning from content when summary is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_from_content_when_summary_is_empty()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-3-mini",
             "output":[
               {"type":"reasoning","id":"rs_456","status":"completed","summary":[],"content":[{"type":"reasoning_text","text":"Let me think step by step."}]},
               {"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"The answer is 444.","annotations":[]}]}],
             "usage":{"input_tokens":10,"output_tokens":15}}
            """);

        var result = await Model(capture, "grok-3-mini").DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        var reasoning = Assert.IsType<XaiReasoning>(result.Content[0]);
        Assert.Equal("Let me think step by step.", reasoning.Text);
        JsonAssert.Equal(reasoning.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456"}}""");
        Assert.Equal("The answer is 444.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(2, result.Content.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract reasoning with encrypted content but empty summary text", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_with_encrypted_content_but_empty_summary_text()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[
               {"type":"reasoning","id":"rs_789","status":"completed","summary":[],"encrypted_content":"encrypted_zdr_content_xyz"},
               {"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Here is my response.","annotations":[]}]}],
             "usage":{"input_tokens":10,"output_tokens":20,"output_tokens_details":{"reasoning_tokens":15}}}
            """);

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        var reasoning = Assert.IsType<XaiReasoning>(result.Content[0]);
        Assert.Equal(string.Empty, reasoning.Text);
        JsonAssert.Equal(reasoning.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_789","reasoningEncryptedContent":"encrypted_zdr_content_xyz"}}""");
        Assert.Equal("Here is my response.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal(2, result.Content.Count);
    }
}
