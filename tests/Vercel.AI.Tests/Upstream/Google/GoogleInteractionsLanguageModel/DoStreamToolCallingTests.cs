// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>tool calling (multi-step flow)</c>.
/// </summary>
public sealed class DoStreamToolCallingTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > tool calling (multi-step flow)::";

    private const string InteractionId = "v1_ChdVbXNIYXVEUkVacmpxdHNQb3JQeXlBRRIXVW1zSGF1RFJFWnJqcXRzUG9yUHl5QUU";

    private static LanguageModelCallOptions Options()
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.Tools = new[]
        {
            GoogleUpstream.Function(
                "getWeather",
                "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\",\"description\":\"The location to get the weather for\"}},\"required\":[\"location\"]}",
                "Get the current weather in a location"),
        };
        options.IncludeRawChunks = false;
        return options;
    }

    [Fact]
    [UpstreamTest(Prefix + "emits tool-input-start, tool-input-delta, tool-input-end, tool-call, finish for a function_call stream (step 1)", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_tool_input_start_tool_input_delta_tool_input_end_tool_call_finish_for_a_function_call_stream_step_1()
    {
        var model = GoogleInteractionsStreamSupport.Model(GoogleInteractionsStreamSupport.FixtureHandler("tool-call-step1"));

        var parts = await GoogleInteractionsStreamSupport.Stream(model, Options());

        var interesting = parts.Where(part => part.Type is "tool-input-start" or "tool-input-delta" or "tool-input-end" or "tool-call" or "finish");
        var expected = JsonNode.Parse(
            "[{\"type\":\"tool-input-start\",\"id\":\"61nzpsv4\",\"toolName\":\"getWeather\"},"
            + "{\"type\":\"tool-input-delta\",\"id\":\"61nzpsv4\",\"delta\":\"{\\\"location\\\":\\\"San Francisco\\\"}\"},"
            + "{\"type\":\"tool-input-end\",\"id\":\"61nzpsv4\"},"
            + "{\"type\":\"tool-call\",\"toolCallId\":\"61nzpsv4\",\"toolName\":\"getWeather\",\"input\":\"{\\\"location\\\":\\\"San Francisco\\\"}\",\"providerMetadata\":{\"google\":{\"interactionId\":\"" + InteractionId + "\",\"signature\":\"\"}}},"
            + "{\"type\":\"finish\",\"finishReason\":{\"raw\":\"requires_action\",\"unified\":\"tool-calls\"},\"providerMetadata\":{\"google\":{\"interactionId\":\"" + InteractionId + "\",\"serviceTier\":\"standard\"}},\"usage\":{\"inputTokens\":{\"cacheRead\":0,\"noCache\":53,\"total\":53},\"outputTokens\":{\"reasoning\":65,\"text\":15,\"total\":80},\"raw\":{\"input_tokens_by_modality\":[{\"modality\":\"text\",\"tokens\":53}],\"total_cached_tokens\":0,\"total_input_tokens\":53,\"total_output_tokens\":15,\"total_thought_tokens\":65,\"total_tokens\":133,\"total_tool_use_tokens\":0}}}]");
        var actual = GoogleInteractionsStreamSupport.Snapshot(interesting);
        Assert.True(JsonNode.DeepEquals(expected, actual), actual.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "streams the final text response and finish reason \"stop\" on step 2 (after tool result)", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_the_final_text_response_and_finish_reason_stop_on_step_2_after_tool_result()
    {
        var model = GoogleInteractionsStreamSupport.Model(GoogleInteractionsStreamSupport.FixtureHandler("tool-call-step2"));

        var parts = await GoogleInteractionsStreamSupport.Stream(model, Options());

        var finish = Assert.IsType<FinishStreamPart>(parts.First(part => part is FinishStreamPart));
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        var text = string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.Contains("San Francisco", text);
    }
}
