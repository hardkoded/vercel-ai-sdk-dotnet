// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>tool calling (multi-step flow)</c>.
/// </summary>
public sealed class DoGenerateToolCallingMultiStepFlowTests
{
    private const string Prefix = DoGenerateSupport.Id + "tool calling (multi-step flow)::";

    private const string ExpectedTool =
        "{\"description\":\"Get the current weather in a location\",\"name\":\"getWeather\",\"parameters\":{\"properties\":{\"location\":{\"description\":\"The location to get the weather for\",\"type\":\"string\"}},\"required\":[\"location\"],\"type\":\"object\"},\"type\":\"function\"}";

    private static LanguageModelTool WeatherTool()
    {
        return GoogleUpstream.Function(
            "getWeather",
            "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\",\"description\":\"The location to get the weather for\"}},\"required\":[\"location\"]}",
            "Get the current weather in a location"
        );
    }

    private static LanguageModelCallOptions Options()
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.Tools = [WeatherTool()];
        return options;
    }

    private static SequencedHandler Fixture(string name)
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture(name));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits tools and (no toolChoice) on the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_tools_and_no_toolChoice_on_the_request_body()
    {
        var handler = Fixture("tool-call-step1");

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Body(handler),
            "{\"input\":[{\"content\":[{\"text\":\"Hello, how are you?\",\"type\":\"text\"}],\"type\":\"user_input\"}],\"model\":\"gemini-2.5-flash\",\"tools\":["
                + ExpectedTool
                + "]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits tool_choice in generation_config when toolChoice is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_tool_choice_in_generation_config_when_toolChoice_is_set()
    {
        var handler = Fixture("tool-call-step1");
        var options = Options();
        options.ToolChoice = ToolChoice.Required;

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "any",
            (string?)DoGenerateSupport.Body(handler)["generation_config"]!["tool_choice"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "parses a function_call output as a tool-call content part with finishReason \"tool-calls\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Parses_a_function_call_output_as_a_tool_call_content_part_with_finishReason_tool_calls()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Fixture("tool-call-step1"))
            .DoGenerateAsync(Options(), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.FinishReason(result),
            "{\"raw\":\"requires_action\",\"unified\":\"tool-calls\"}"
        );
        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Content(result.Content.OfType<GeneratedToolCall>()),
            "[{\"input\":\"{\\\"location\\\":\\\"San Francisco\\\"}\",\"providerMetadata\":{\"google\":{\"interactionId\":\"v1_ChdUMnNIYXVxU0lJX2lxdHNQX2FicXVBWRIXVDJzSGF1cVNJSV9pcXRzUF9hYnF1QVk\"}},\"toolCallId\":\"zggxzq8r\",\"toolName\":\"getWeather\",\"type\":\"tool-call\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "round-trips an assistant tool-call + tool-result message pair through the request body (step 2)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Round_trips_an_assistant_tool_call_plus_tool_result_message_pair_through_the_request_body_step_2()
    {
        var handler = Fixture("tool-call-step2");
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("What is the weather in San Francisco right now?"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart(
                        "",
                        GoogleUpstream.ProviderOptions("{\"google\":{\"signature\":\"sig-abc\"}}")
                    ),
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "r7b1dyif",
                            "getWeather",
                            "{\"location\":\"San Francisco\"}"
                        )
                    ),
                }
            ),
            new ToolModelMessage(
                "r7b1dyif",
                "getWeather",
                "{\"location\":\"San Francisco\",\"condition\":\"sunny\",\"temperature\":16}",
                false,
                "json"
            )
        );
        options.Tools = [WeatherTool()];

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Body(handler),
            "{\"input\":[{\"content\":[{\"text\":\"What is the weather in San Francisco right now?\",\"type\":\"text\"}],\"type\":\"user_input\"},{\"signature\":\"sig-abc\",\"type\":\"thought\"},{\"arguments\":{\"location\":\"San Francisco\"},\"id\":\"r7b1dyif\",\"name\":\"getWeather\",\"type\":\"function_call\"},{\"call_id\":\"r7b1dyif\",\"name\":\"getWeather\",\"result\":\"{\\\"location\\\":\\\"San Francisco\\\",\\\"condition\\\":\\\"sunny\\\",\\\"temperature\\\":16}\",\"type\":\"function_result\"}],\"model\":\"gemini-2.5-flash\",\"tools\":["
                + ExpectedTool
                + "]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "parses the step-2 final text response after the tool result was provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Parses_the_step_2_final_text_response_after_the_tool_result_was_provided()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Fixture("tool-call-step2"))
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Contains("San Francisco", DoGenerateSupport.Text(result));
    }
}
