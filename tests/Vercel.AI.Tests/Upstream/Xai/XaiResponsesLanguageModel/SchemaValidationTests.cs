// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; schema validation</c>.</summary>
public sealed class SchemaValidationTests
{
    private const string Prefix = TestFile + " > schema validation::";

    [Fact]
    [UpstreamTest(Prefix + "should accept response.created with usage: null", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_response_created_with_usage_null()
    {
        var parts = await Collect(Model(NullUsageStream("response.created")), HelloPrompt());

        AssertHi(parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept response.in_progress with usage: null", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_response_in_progress_with_usage_null()
    {
        var parts = await Collect(Model(NullUsageStream("response.in_progress")), HelloPrompt());

        AssertHi(parts);
    }

    private static UpstreamCapture NullUsageStream(string firstEvent)
    {
        var response = """{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"in_progress","output":[],"usage":null}""";
        return StreamResponse(
            "{\"type\":\"" + firstEvent + "\",\"response\":" + response + "}",
            "{\"type\":\"response.in_progress\",\"response\":" + response + "}",
            """{"type":"response.output_item.added","item":{"id":"msg_001","type":"message","role":"assistant","content":[],"status":"in_progress"},"output_index":0}""",
            """{"type":"response.content_part.added","item_id":"msg_001","output_index":0,"content_index":0,"part":{"type":"output_text","text":""}}""",
            """{"type":"response.output_text.delta","item_id":"msg_001","output_index":0,"content_index":0,"delta":"Hi"}""",
            """{"type":"response.completed","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"completed","output":[{"id":"msg_001","type":"message","role":"assistant","content":[{"type":"output_text","text":"Hi"}],"status":"completed"}],"usage":{"input_tokens":5,"output_tokens":1,"total_tokens":6}}}""");
    }

    private static void AssertHi(List<LanguageModelStreamPart> parts)
    {
        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part => part.Delta == "Hi");
        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
    }
}
