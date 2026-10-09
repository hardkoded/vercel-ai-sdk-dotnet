// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Examples;

public static class ToolsExample
{
    public static async Task<string> RunAsync()
    {
        var client = new AiClient(Vercel.AI.Gateway.GatewayProvider.Create(new() { ApiKey = "unused" }));
        var model = new TestLanguageModel();
        model.OnGenerate = options =>
        {
            var sawToolResult = options.Prompt.Any(message => message is ToolModelMessage);
            return sawToolResult
                ? TestLanguageModel.Text("Sunny in Lisbon.")
                : new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedToolCall("call_1", "weather", "{\"city\":\"Lisbon\"}") },
                    FinishReason.ToolCalls,
                    LanguageModelUsage.Empty,
                    "tool_calls");
        };

        #region tools
        var weather = Tool.Function(
            "weather",
            "Returns a short forecast.",
            "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}",
            (arguments, cancellationToken) =>
            {
                var city = arguments.GetProperty("city").GetString();
                return Task.FromResult("{\"city\":\"" + city + "\",\"forecast\":\"sunny\"}");
            });

        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "Weather in Lisbon?",
            Tools = new[] { weather },
            StopWhen = StopWhen.IsStepCount(4),
        });
        #endregion

        return result.Text;
    }
}
