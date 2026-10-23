// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Upstream.Google.BuildGoogleInteractionsStreamTransform;

/// <summary>Port of <c>build-google-interactions-stream-transform.test.ts</c> &gt; <c>buildGoogleInteractionsStreamTransform — tool call IDs</c>.</summary>
public sealed class ToolCallIdsTests
{
    [Fact]
    [UpstreamTest("packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::buildGoogleInteractionsStreamTransform — tool call IDs::uses the generated block ID when a function call ID is empty", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_generated_block_ID_when_a_function_call_ID_is_empty()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            "{\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"interaction-1\",\"status\":\"in_progress\"}}",
            "{\"event_type\":\"step.start\",\"index\":0,\"step\":{\"type\":\"function_call\",\"id\":\"\",\"name\":\"get_weather\",\"arguments\":{}}}",
            "{\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"arguments_delta\",\"id\":\"\",\"arguments\":\"{}\"}}",
            "{\"event_type\":\"step.stop\",\"index\":0}",
            "{\"event_type\":\"interaction.completed\",\"interaction\":{\"id\":\"interaction-1\",\"status\":\"completed\"}}");

        var tools = GoogleInteractionsStreamSupport.Snapshot(parts.Where(part => part.Type.StartsWith("tool-", StringComparison.Ordinal)));

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("[{\"type\":\"tool-input-start\",\"id\":\"interaction-1:0\",\"toolName\":\"get_weather\"},{\"type\":\"tool-input-delta\",\"id\":\"interaction-1:0\",\"delta\":\"{}\"},{\"type\":\"tool-input-end\",\"id\":\"interaction-1:0\"},{\"type\":\"tool-call\",\"toolCallId\":\"interaction-1:0\",\"toolName\":\"get_weather\",\"input\":\"{}\",\"providerMetadata\":{\"google\":{\"interactionId\":\"interaction-1\"}}}]"),
                tools),
            tools.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::buildGoogleInteractionsStreamTransform — tool call IDs::preserves generated block IDs when built-in tool deltas contain empty IDs", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_generated_block_IDs_when_built_in_tool_deltas_contain_empty_IDs()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            "{\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"interaction-1\",\"status\":\"in_progress\"}}",
            "{\"event_type\":\"step.start\",\"index\":0,\"step\":{\"type\":\"google_search_call\",\"id\":\"\",\"arguments\":{}}}",
            "{\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"google_search_call\",\"id\":\"\",\"arguments\":{\"queries\":[\"weather\"]}}}",
            "{\"event_type\":\"step.stop\",\"index\":0}",
            "{\"event_type\":\"step.start\",\"index\":1,\"step\":{\"type\":\"google_search_result\",\"call_id\":\"\",\"result\":null}}",
            "{\"event_type\":\"step.delta\",\"index\":1,\"delta\":{\"type\":\"google_search_result\",\"call_id\":\"\",\"result\":[]}}",
            "{\"event_type\":\"step.stop\",\"index\":1}",
            "{\"event_type\":\"interaction.completed\",\"interaction\":{\"id\":\"interaction-1\",\"status\":\"completed\"}}");

        var tools = GoogleInteractionsStreamSupport.Snapshot(parts.Where(part => part.Type.StartsWith("tool-", StringComparison.Ordinal)));

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("[{\"type\":\"tool-call\",\"toolCallId\":\"interaction-1:0\",\"toolName\":\"google_search\",\"input\":\"{\\\"queries\\\":[\\\"weather\\\"]}\",\"providerExecuted\":true},{\"type\":\"tool-result\",\"toolCallId\":\"interaction-1:1\",\"toolName\":\"google_search\",\"result\":[]}]"),
                tools),
            tools.ToJsonString());
    }
}
