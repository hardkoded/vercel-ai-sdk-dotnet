// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Sdk;
using Vercel.AI.Sdk.Gateway;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.Testing;

var gatewayKey = Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY");
ILanguageModel model = string.IsNullOrEmpty(gatewayKey)
    ? Scripted()
    : GatewayProvider.Create().LanguageModel("openai/gpt-4.1-mini");

if (string.IsNullOrEmpty(gatewayKey))
{
    Console.WriteLine("AI_GATEWAY_API_KEY is not set. The sample is using a scripted model.");
}

var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = gatewayKey ?? "local" }));

var text = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "Say hello from the Vercel AI SDK for .NET.",
});
Console.WriteLine("text: " + text.Text);

var tools = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "What is the weather in Paris?",
    StopWhen = StopWhen.IsStepCount(2),
    Tools = new[]
    {
        Tool.Function("weather", "Local weather", "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}", (arguments, _) =>
        {
            var city = arguments.TryGetProperty("city", out var value) ? value.GetString() : "there";
            return Task.FromResult("{\"city\":\"" + city + "\",\"forecast\":\"sunny\"}");
        }),
    },
});
Console.WriteLine("tool: " + tools.Text);

var structured = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "Name a city.",
    Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}", "city"),
});
Console.WriteLine("city: " + structured.Output?.GetProperty("city").GetString());

static ILanguageModel Scripted()
{
    var model = new TestLanguageModel();
    model.OnGenerate = options =>
    {
        var sawToolResult = false;
        foreach (var message in options.Prompt)
        {
            if (message is ToolModelMessage)
            {
                sawToolResult = true;
            }
        }

        if (options.Tools is { Count: > 0 } && !sawToolResult)
        {
            return new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call_1", "weather", "{\"city\":\"Paris\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls");
        }

        if (sawToolResult)
        {
            return TestLanguageModel.Text("Sunny in Paris.");
        }

        if (options.JsonSchema != null)
        {
            return TestLanguageModel.Text("{\"city\":\"Lisbon\"}");
        }

        return TestLanguageModel.Text("Hello from the Vercel AI SDK for .NET.");
    };
    return model;
}
