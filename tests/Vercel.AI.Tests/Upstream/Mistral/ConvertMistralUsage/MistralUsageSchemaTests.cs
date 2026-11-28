// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertMistralUsage;

/// <summary>Port of <c>convert-mistral-usage.test.ts</c> &gt; <c>mistralUsageSchema</c>.</summary>
public sealed class MistralUsageSchemaTests
{
    private const string Prefix =
        "packages/mistral/src/convert-mistral-usage.test.ts::mistralUsageSchema::";

    private static JsonObject BasicUsage() =>
        new()
        {
            ["prompt_tokens"] = 100,
            ["completion_tokens"] = 50,
            ["total_tokens"] = 150,
        };

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve additional usage and token detail fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_additional_usage_and_token_detail_fields()
    {
        var usage = BasicUsage();
        usage["additional_usage_field"] = new JsonObject { ["value"] = true };
        usage["prompt_tokens_details"] = new JsonObject
        {
            ["cached_tokens"] = 20,
            ["additional_prompt_field"] = new JsonArray("value"),
        };
        usage["prompt_token_details"] = new JsonObject
        {
            ["audio_tokens"] = 5,
            ["additional_prompt_token_field"] = 1,
        };
        usage["completion_tokens_details"] = new JsonObject
        {
            ["reasoning_tokens"] = 10,
            ["additional_completion_field"] = null,
        };
        var element = JsonDocument.Parse(usage.ToJsonString()).RootElement;

        var parsed = MistralUsageSchema.Parse(element);

        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(parsed.GetRawText()), usage),
            parsed.GetRawText()
        );
    }

    [Theory]
    [InlineData("prompt_tokens", "{\"prompt_tokens\":\"100\"}")]
    [InlineData("completion_tokens", "{\"completion_tokens\":\"50\"}")]
    [InlineData("total_tokens", "{\"total_tokens\":\"150\"}")]
    [InlineData("prompt_audio_seconds", "{\"prompt_audio_seconds\":\"10\"}")]
    [InlineData("request_count", "{\"request_count\":\"1\"}")]
    [InlineData("service_tier", "{\"service_tier\":1}")]
    [InlineData("num_cached_tokens", "{\"num_cached_tokens\":\"20\"}")]
    [InlineData(
        "prompt_tokens_details.cached_tokens",
        "{\"prompt_tokens_details\":{\"cached_tokens\":\"20\"}}"
    )]
    [InlineData(
        "prompt_tokens_details.audio_tokens",
        "{\"prompt_tokens_details\":{\"audio_tokens\":\"5\"}}"
    )]
    [InlineData("prompt_tokens_details.messages", "{\"prompt_tokens_details\":{\"messages\":{}}}")]
    [InlineData(
        "prompt_token_details.cached_tokens",
        "{\"prompt_token_details\":{\"cached_tokens\":\"20\"}}"
    )]
    [InlineData(
        "prompt_token_details.audio_tokens",
        "{\"prompt_token_details\":{\"audio_tokens\":\"5\"}}"
    )]
    [InlineData("prompt_token_details.messages", "{\"prompt_token_details\":{\"messages\":{}}}")]
    [InlineData(
        "completion_tokens_details.reasoning_tokens",
        "{\"completion_tokens_details\":{\"reasoning_tokens\":\"10\"}}"
    )]
    [UpstreamTest(
        Prefix + "should reject an invalid %s value",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_an_invalid_field_value(string field, string overrides)
    {
        var usage = BasicUsage();
        foreach (var pair in JsonNode.Parse(overrides)!.AsObject().ToList())
        {
            usage.Remove(pair.Key);
            usage[pair.Key] = pair.Value?.DeepClone();
        }

        var element = JsonDocument.Parse(usage.ToJsonString()).RootElement;

        Assert.Throws<TypeValidationException>(() => MistralUsageSchema.Parse(element));
        Assert.NotEmpty(field);
    }
}
