# Vercel AI SDK for .NET

[![NuGet](https://img.shields.io/nuget/v/Vercel.AI.svg)](https://www.nuget.org/packages/Vercel.AI)

The Vercel AI SDK for .NET is a provider-agnostic C# toolkit to build AI-powered applications and agents on .NET.

It is an unofficial community port of the [AI SDK](https://github.com/vercel/ai). It is an independent reimplementation of the public AI SDK behavior and provider HTTP APIs. It is not a copy of Vercel’s TypeScript, not an official Vercel product, and not endorsed by Vercel. The parity target is AI SDK 7 / Language Model specification V4. See [COMPATIBILITY.md](COMPATIBILITY.md) for the upstream commit and the packages that are intentionally not ported.

To learn more, read the [documentation](https://hardkoded.github.io/vercel-ai-sdk-dotnet/).

## Installation

You need a project that targets `net10.0` or `netstandard2.0`. Run `dotnet add package Vercel.AI`.

Every provider ships in this one package.

## Skill for Coding Agents

If you use coding agents such as Claude Code or Cursor, we highly recommend adding the Vercel AI SDK for .NET skill to your repository:

```shell
npx skills add hardkoded/vercel-ai-sdk-dotnet
```

## Unified Provider Architecture

The SDK gives you one API to call model providers like OpenAI, Anthropic, Google, and [more](docs/articles/providers.md).

By default, string model ids go to the [Vercel AI Gateway](https://vercel.com/docs/ai-gateway), which gives you access to all major providers. Pass a model string for any supported model:

<!-- snippet: gateway-model -->
<a id='snippet-gateway-model'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "anthropic/claude-opus-5.5", // or "openai/gpt-6-astra", "google/gemini-3.8-flash", etc.
    Prompt = "Hello!",
});
```
<sup><a href='/examples/Providers/Program.cs#L19-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-gateway-model' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can also connect to a provider directly. Each provider has a `Create()` method that reads its API key from the same environment variable as the JavaScript provider:

<!-- snippet: provider-model -->
<a id='snippet-provider-model'></a>
```cs
var client = new AiClient(GatewayProvider.Create());
var anthropic = AnthropicProvider.Create(); // or OpenAIProvider.Create(), GoogleProvider.Create(), etc.

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = anthropic.LanguageModel("claude-opus-5-5"),
    Prompt = "Hello!",
});
```
<sup><a href='/examples/Providers/Program.cs#L34-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-provider-model' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Usage

### Generating Text

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

### Generating Structured Data

<!-- snippet: generate-structured-data -->
<a id='snippet-generate-structured-data'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Output = OutputSpec.Object("""
        {
          "type": "object",
          "properties": {
            "recipe": {
              "type": "object",
              "properties": {
                "name": { "type": "string" },
                "ingredients": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" },
                      "amount": { "type": "string" }
                    },
                    "required": ["name", "amount"],
                    "additionalProperties": false
                  }
                },
                "steps": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["name", "ingredients", "steps"],
              "additionalProperties": false
            }
          },
          "required": ["recipe"],
          "additionalProperties": false
        }
        """),
    Prompt = "Generate a lasagna recipe.",
});

Console.WriteLine(result.Output);
```
<sup><a href='/examples/GenerateStructuredData/Program.cs#L14-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-generate-structured-data' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Agents

<!-- snippet: agent -->
<a id='snippet-agent'></a>
```cs
var weatherAgent = new Agent(new AgentOptions
{
    ModelId = "openai/gpt-6-astra",
    Instructions = "You are an agent that answers questions about the weather.",
    Tools = new[]
    {
        Tool.Function(
            "weather",
            "Returns the weather in a city.",
            """{ "type": "object", "properties": { "city": { "type": "string" } }, "required": ["city"] }""",
            (arguments, cancellationToken) =>
            {
                var city = arguments.GetProperty("city").GetString();
                return Task.FromResult(JsonSerializer.Serialize(new { city, forecast = "sunny", temperature = 22 }));
            }),
    },
});

var result = await weatherAgent.GenerateAsync("What is the weather in Lisbon?");
Console.WriteLine(result.Text);
```
<sup><a href='/examples/Agent/Program.cs#L14-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-agent' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Examples

Every code block in this README comes from a project in [examples](examples). Each project is one example, and CI builds all of them.

| Example | Shows |
| --- | --- |
| [GenerateText](examples/GenerateText) | Generate text with a Gateway model string. |
| [GenerateStructuredData](examples/GenerateStructuredData) | Generate a JSON object that matches a schema. |
| [Agent](examples/Agent) | Run an agent that calls a tool. |
| [Providers](examples/Providers) | Call a model through the Gateway and through a provider directly. |
| [StreamText](examples/StreamText) | Stream text as the model writes it. |
| [Tools](examples/Tools) | Let the model call a tool and read its result. |
| [UIMessageStream](examples/UIMessageStream) | Stream a UI message response from ASP.NET Core to a JavaScript `useChat` client. |

Run an example with `dotnet run --project examples/GenerateText`. The examples call the AI Gateway when `AI_GATEWAY_API_KEY` is set. Without the key, they print a message and exit. `Providers` also calls Anthropic directly when `ANTHROPIC_API_KEY` is set. `UIMessageStream` listens on `http://127.0.0.1:43123` and uses a scripted model when no key is set.

This port has no UI packages. `ToUIMessageStreamResult` writes the AI SDK UI message stream (`text/event-stream`) that a JavaScript `useChat` client already reads.

## Tests

Run `dotnet test Vercel.AI.slnx -c Release`.

Unit tests mock HTTP and do not need keys. Integration tests call a live provider only when `AI_GATEWAY_API_KEY` or `OPENAI_API_KEY` is set.

## Packages

The library targets `net10.0` and `netstandard2.0`. The ASP.NET UI stream helper and the examples are `net10.0` only. Versions come from MinVer. Pushing a `vMAJOR.MINOR.PATCH` tag packs `Vercel.AI`, publishes that package to nuget.org with Trusted Publishing, and the docs workflow publishes the DocFX site.

## Community

Ask questions, report bugs, and share ideas in [GitHub issues](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues).

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before you start.

## Authors

This port is maintained by [Darío Kondratiuk](https://github.com/kblok). The AI SDK it ports is created by [Vercel](https://vercel.com) and [Next.js](https://nextjs.org) team members, with contributions from the [Open Source Community](https://github.com/vercel/ai/graphs/contributors).

## License

Apache License 2.0. Copyright 2023 Vercel, Inc. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
