// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class ToolCallsTests
{
    private const string CalculatorSchema = "{\"type\":\"object\",\"properties\":{\"expression\":{\"type\":\"string\",\"description\":\"The mathematical expression to evaluate\"}},\"required\":[\"expression\"]}";

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_text_with_tool_calls(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Prompt = "What is 2+2? Use the calculator tool to compute this.",
            Tools = new[] { Tool.Function("calculator", null, CalculatorSchema, (_, _) => Task.FromResult("\"4\"")) },
        });

        Assert.Equal("calculator", result.ToolCalls[0].ToolName);
        using var input = JsonDocument.Parse(result.ToolCalls[0].ArgumentsJson);
        Assert.Equal("2+2", input.RootElement.GetProperty("expression").GetString());
        Assert.Equal("\"4\"", result.ToolResults[0].OutputJson);
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_stream_text_with_tool_calls(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var toolCallCount = 0;
        var result = AnthropicFeatureSuite.Client().StreamTextAsync(new StreamTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Prompt = "What is 2+2? Use the calculator tool to compute this.",
            Tools = new[]
            {
                Tool.Function("calculator", null, "{\"type\":\"object\",\"properties\":{\"expression\":{\"type\":\"string\"}},\"required\":[\"expression\"]}", (_, _) =>
                {
                    toolCallCount++;
                    return Task.FromResult("\"4\"");
                }),
            },
        });

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Contains(parts, part => part.Type == "tool-call");
        Assert.Equal(1, toolCallCount);
        Assert.True((await result.Usage).TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_handle_multiple_sequential_tool_calls(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var weatherCalls = 0;
        var musicCalls = 0;
        const int sfTemp = 15;
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Prompt = "Check the temperature in San Francisco and play music that matches the weather. Be sure to report the chosen song name.",
            Tools = new[]
            {
                Tool.Function("getTemperature", null, "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\",\"description\":\"The city to check temperature for\"}},\"required\":[\"city\"]}", (_, _) =>
                {
                    weatherCalls++;
                    return Task.FromResult("\"" + sfTemp + "\"");
                }),
                Tool.Function("playWeatherMusic", null, "{\"type\":\"object\",\"properties\":{\"temperature\":{\"type\":\"number\",\"description\":\"Temperature in Celsius\"}},\"required\":[\"temperature\"]}", (args, _) =>
                {
                    musicCalls++;
                    var temperature = args.GetProperty("temperature").GetDouble();
                    var song = temperature <= 10 ? "Playing \"Winter Winds\" by Mumford & Sons"
                        : temperature <= 20 ? "Playing \"Foggy Day\" by Frank Sinatra"
                        : temperature <= 30 ? "Playing \"Here Comes the Sun\" by The Beatles"
                        : "Playing \"Hot Hot Hot\" by Buster Poindexter";
                    return Task.FromResult(JsonSerializer.Serialize(song));
                }),
            },
            StopWhen = StopWhen.IsStepCount(10),
        });

        Assert.Equal(1, weatherCalls);
        Assert.Equal(1, musicCalls);
        Assert.Contains("Foggy Day", result.Text);
    }
}
