# Quickstart

Install `Vercel.AI` and the provider package you call. `OpenAIProvider.Create()` reads `OPENAI_API_KEY`. `GatewayProvider.Create()` reads `AI_GATEWAY_API_KEY`.

```csharp
using Vercel.AI;
using Vercel.AI.OpenAI;

var model = OpenAIProvider.Create().LanguageModel("gpt-4.1-mini");
var client = new AiClient(Vercel.AI.Gateway.GatewayProvider.Create(new() { ApiKey = "unused" }));
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Instructions = "Answer in one sentence.",
    Prompt = "What is a language model specification?",
});
Console.WriteLine(result.Text);
```

`AiClient` is only required for the high-level helpers. Pass `Model` when you already have an `ILanguageModel`. Pass `ModelId` (`openai/gpt-4.1-mini`) when the Gateway should resolve it.

In ASP.NET Core, `services.AddAiSdk()` registers `IAiClient` and a named `HttpClient`.
