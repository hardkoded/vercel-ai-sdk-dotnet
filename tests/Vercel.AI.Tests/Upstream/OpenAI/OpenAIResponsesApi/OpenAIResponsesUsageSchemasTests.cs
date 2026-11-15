// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesApi;

/// <summary>Port of <c>openai-responses-api.test.ts</c> &gt; <c>OpenAI Responses usage schemas</c>.</summary>
public sealed class OpenAIResponsesUsageSchemasTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-api.test.ts::OpenAI Responses usage schemas::";

    private const string UsageJson = """
        {
          "input_tokens": 12,
          "input_tokens_details": {
            "cached_tokens": 2,
            "cache_write_tokens": 1,
            "orchestration_input_tokens": 4,
            "orchestration_input_cached_tokens": 3,
            "future_input_detail": { "tokens": 5 }
          },
          "output_tokens": 8,
          "output_tokens_details": {
            "reasoning_tokens": 3,
            "orchestration_output_tokens": 2,
            "future_output_detail": ["preserved"]
          },
          "total_tokens": 20,
          "future_usage_field": { "value": true }
        }
        """;

    public static TheoryData<string> Cases() => ["normal response", "completed event", "incomplete event", "failed event"];

    private static string Wrap(string name, string usage) => name switch
    {
        "normal response" => "{\"usage\":" + usage + "}",
        "completed event" => "{\"type\":\"response.completed\",\"response\":{\"usage\":" + usage + "}}",
        "incomplete event" => "{\"type\":\"response.incomplete\",\"response\":{\"usage\":" + usage + "}}",
        _ => "{\"type\":\"response.failed\",\"sequence_number\":1,\"response\":{\"usage\":" + usage + "}}",
    };

    private static bool Validate(string name, string usage)
    {
        var value = OpenAIUpstream.Json(Wrap(name, usage));
        return name == "normal response" ? OpenAIResponsesSchema.IsValidResponse(value) : OpenAIResponsesSchema.IsValidChunk(value);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "preserves complete usage for $name", Coverage = UpstreamCoverage.Partial)]
    public void Accepts_complete_usage_for_(string name) => Assert.True(Validate(name, UsageJson));

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "rejects invalid total_tokens for $name", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_total_tokens_for_(string name) =>
        Assert.False(Validate(name, UsageJson.Replace("\"total_tokens\": 20", "\"total_tokens\": \"20\"")));

    [Theory]
    [InlineData("input_tokens", "\"input_tokens\": 12", "\"input_tokens\": \"12\"")]
    [InlineData("input_tokens_details.cached_tokens", "\"cached_tokens\": 2", "\"cached_tokens\": \"2\"")]
    [InlineData("output_tokens", "\"output_tokens\": 8", "\"output_tokens\": \"8\"")]
    [InlineData("output_tokens_details.reasoning_tokens", "\"reasoning_tokens\": 3", "\"reasoning_tokens\": \"3\"")]
    [UpstreamTest(Prefix + "rejects invalid $name values", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_values(string name, string original, string replacement)
    {
        Assert.Contains(original, UsageJson);
        Assert.False(OpenAIResponsesSchema.IsValidResponse(OpenAIUpstream.Json(Wrap("normal response", UsageJson.Replace(original, replacement)))), name);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts failed events with null usage", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_failed_events_with_null_usage() =>
        Assert.True(OpenAIResponsesSchema.IsValidChunk(OpenAIUpstream.Json("""{"type":"response.failed","sequence_number":1,"response":{"usage":null}}""")));
}
