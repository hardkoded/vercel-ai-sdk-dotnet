// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > assistant messages::should round-trip async mode on custom tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_round_trip_async_mode_on_custom_tool_calls()
    {
        using var options = JsonDocument.Parse("{\"itemId\":\"ctc_async\",\"async\":true}");
        var call = new GeneratedToolCall("call_custom_async", "write_sql", "\"SELECT 1\"");
        var message = new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantToolCallPart(call, new Dictionary<string, JsonElement> { ["openai"] = options.RootElement.Clone() }),
        });

        var result = OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[] { message }, "system", false, null, "openai", false, false,
            customToolNames: new HashSet<string> { "write_sql" });

        var expected = new JsonArray(new JsonObject
        {
            ["type"] = "custom_tool_call",
            ["call_id"] = "call_custom_async",
            ["name"] = "write_sql",
            ["input"] = "SELECT 1",
            ["async"] = true,
            ["id"] = "ctc_async",
        });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }
}
