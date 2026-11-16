// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_API_KEY"))
    || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_RESOURCE_NAME")))
{
    Console.WriteLine("Set AZURE_API_KEY and AZURE_RESOURCE_NAME to run this example.");
    return;
}

var azure = AzureOpenAIProvider.Create();

// generate-text/azure/responses-custom-tool.ts
#region azure-responses-custom-tool-generate
var customTool = await Ai.GenerateTextAsync(new GenerateTextOptions
{
    // Replace with your Azure deployment name that supports custom tools.
    Model = azure.ResponsesModel("gpt-5.3-codex"),
    Tools = new[]
    {
        ToTool(AzureOpenAITools.CustomTool(new OpenAICustomToolOptions
        {
            Name = "write_sql",
            Description = "Write a SQL SELECT query to answer the user question.",
            GrammarSyntax = "regex",
            GrammarDefinition = "SELECT .+",
        })),
    },
    ToolChoice = ToolChoice.Required,
    Prompt = "Write a SQL query to get all users older than 25.",
});
#endregion

Console.WriteLine("Tool calls: " + string.Join(", ", customTool.ToolCalls.Select(call => call.ToolName + " " + call.ArgumentsJson)));
Console.WriteLine("Finish reason: " + customTool.FinishReason);
Console.WriteLine("Usage: " + customTool.Usage);

// stream-text/azure/responses-custom-tool.ts
var customToolStream = Ai.StreamTextAsync(new StreamTextOptions
{
    Model = azure.ResponsesModel("gpt-5.3-codex"),
    Tools = new[]
    {
        ToTool(AzureOpenAITools.CustomTool(new OpenAICustomToolOptions
        {
            Name = "write_sql",
            Description = "Write a SQL SELECT query to answer the user question.",
            GrammarSyntax = "regex",
            GrammarDefinition = "SELECT .+",
        })),
    },
    ToolChoice = ToolChoice.Required,
    Prompt = "Write a SQL query to get all users older than 25.",
});

await PrintStream(customToolStream);

// generate-text/azure/responses-shell-container.ts
#region azure-responses-shell-container-generate
var shell = await Ai.GenerateTextAsync(new GenerateTextOptions
{
    // Replace with your Azure deployment name that supports this shell mode.
    Model = azure.ResponsesModel("gpt-5.4-mini"),
    Tools = new[]
    {
        ToTool(AzureOpenAITools.Shell(new OpenAIShellToolOptions
        {
            Environment = new OpenAIShellEnvironment { Type = "containerAuto" },
        })),
    },
    StopWhen = StopWhen.IsStepCount(20),
    Prompt = "Print \"Hello from container!\" and show the system info using uname -a",
});
#endregion

Console.WriteLine("Result: " + shell.Text);

// stream-text/azure/responses-shell-container.ts
var shellStream = Ai.StreamTextAsync(new StreamTextOptions
{
    Model = azure.ResponsesModel("gpt-5.4-mini"),
    Tools = new[]
    {
        ToTool(AzureOpenAITools.Shell(new OpenAIShellToolOptions
        {
            Environment = new OpenAIShellEnvironment { Type = "containerAuto" },
        })),
    },
    StopWhen = StopWhen.IsStepCount(20),
    Prompt = "Print \"Hello from container!\" and show the system info using uname -a",
});

await PrintStream(shellStream);

static Tool ToTool(LanguageModelTool tool) => Tool.ProviderDefined(tool.ProviderToolId!, tool.Name, tool.Args);

static async Task PrintStream(StreamTextResult stream)
{
    await foreach (var chunk in stream.Stream())
    {
        switch (chunk)
        {
            case TextDeltaPart delta:
                Console.Write(delta.Text);
                break;
            case ToolCallPart call:
                Console.WriteLine($"\nTool call: {call.ToolCall.ToolName} {call.ToolCall.ArgumentsJson}");
                break;
            case ToolResultPart result:
                Console.WriteLine($"\nTool result: {result.Result.ToolName} {result.Result.OutputJson}");
                break;
            case ErrorPart error:
                Console.Error.WriteLine("Error: " + error.Message);
                break;
        }
    }

    Console.WriteLine();
}
