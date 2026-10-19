# Quickstart

Install `Vercel.AI` and the provider package you call. `OpenAIProvider.Create()` reads `OPENAI_API_KEY`. `GatewayProvider.Create()` reads `AI_GATEWAY_API_KEY`.

<!-- snippet: generate-text -->
<a id='snippet-generate-text'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra", // use Vercel AI Gateway
    Prompt = "What is an agent?",
});

Console.WriteLine(result.Text);
```
<sup><a href='/examples/GenerateText/Program.cs#L14-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-generate-text' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`AiClient` is only required for the high-level helpers. Pass `Model` when you already have an `ILanguageModel`. Pass `ModelId` (`openai/gpt-4.1-mini`) when the Gateway should resolve it.

In ASP.NET Core, `services.AddAiSdk()` registers `IAiClient` and a named `HttpClient`.

## Model call callbacks

`GenerateTextOptions.OnLanguageModelCallEnd` runs after each provider call returns, before tools run and before `OnStepEnd`. Its `LanguageModelCallEndEvent` carries `Provider`, `ModelId`, `FinishReason`, `Usage`, `Content`, `ResponseId`, `ProviderMetadata` and `ResponseTime`. It fires even when a tool then cancels the call, so the provider metadata of that call is not lost. `StreamTextOptions` calls it too, after the stream of a step ends.

`OnToolExecutionStart` runs before a tool's `Execute` and `OnToolExecutionEnd` after it returns or throws, on `GenerateTextOptions` and `StreamTextOptions`. They do not run for a tool without `Execute`, a provider-executed call, or an invalid or unapproved call. `ToolExecutionStartEvent` carries `ToolCall` and `Messages` (what the model received in that step). `ToolExecutionEndEvent` adds `ToolOutput` (an `ExecutedTool`, with `IsError` set when the tool threw) and `ToolExecutionTime`. An exception a callback throws is ignored. The callbacks run before `OnStepEnd`.
