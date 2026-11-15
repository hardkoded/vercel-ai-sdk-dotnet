// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesApi;

/// <summary>Port of <c>openai-responses-api.test.ts</c> &gt; <c>openaiResponsesResponseSchema</c>.</summary>
public sealed class OpenAIResponsesResponseSchemaTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-api.test.ts::openaiResponsesResponseSchema::";

    private static bool IsValid(string json) => OpenAIResponsesSchema.IsValidResponse(OpenAIUpstream.Json(json));

    [Fact]
    [UpstreamTest(Prefix + "accepts refusal content parts in message output items", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Accepts_refusal_content_parts_in_message_output_items()
    {
        Assert.True(IsValid("""
            {
              "id": "resp_1",
              "output": [
                {
                  "type": "message",
                  "id": "msg_1",
                  "role": "assistant",
                  "content": [{ "type": "refusal", "refusal": "I'm sorry, I can't assist with that." }]
                }
              ]
            }
            """));
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts text and refusal content parts in the same message", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Accepts_text_and_refusal_content_parts_in_the_same_message()
    {
        Assert.True(IsValid("""
            {
              "id": "resp_1",
              "output": [
                {
                  "type": "message",
                  "id": "msg_1",
                  "role": "assistant",
                  "content": [
                    { "type": "output_text", "text": "partial", "annotations": [] },
                    { "type": "refusal", "refusal": "I can't assist with that." }
                  ]
                }
              ]
            }
            """));
    }
}
