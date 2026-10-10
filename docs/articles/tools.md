# Tools

A tool has a name, a JSON Schema, and an `Execute` delegate. The model receives the schema. The SDK runs `Execute` when the model calls the tool, then feeds the JSON result back as a tool message.

<!-- snippet: tools -->
<a id='snippet-tools'></a>
```cs
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
```
<sup><a href='/samples/Vercel.AI.Examples/ToolsExample.cs#L29-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-tools' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The default stop condition is one step. That step’s tool calls still run, and the loop stops before a second model call. Raise `IsStepCount`, or use `HasToolCall` / `IsLoopFinished`, when the model should see the tool result.

`Agent` stops after 10 steps when `AgentOptions.StopWhen` is null. When that default limit is what stopped the loop, the agent logs one warning through `LogWarnings` (type `other`, with the provider and model of the last step). Set `StopWhen` explicitly, with `IsStepCount` or a custom condition, to raise the limit and silence it. `GenerateTextAsync` and `StreamTextAsync` do not log. `LogWarnings.Logger = false` suppresses the warning. `StepResult.Provider` and `StepResult.ModelId` name the model that produced each step.

A call to a tool that has no `Execute` ends the loop at that step. The stop condition is not evaluated.

Set `ApproveTool` to veto a call before it runs. A denied call is stored as an error tool result.

## Tool errors

A tool result carries an output type: `text`, `json`, `error-text`, `error-json`, or `execution-denied`. `ToolModelMessage.OutputType` holds it. When you do not set it, the type comes from `isError` and from whether the output is valid JSON.

Providers keep the error status:

- OpenAI Chat Completions and Responses send `error-text` and `error-json` as `{"error": <value>}`. `execution-denied` stays the reason, or `Tool call execution denied.` when there is none.
- OpenAI Responses honors `promptCacheBreakpoint` on a scalar tool result (`text`, `json`, `error-text`, `error-json`, `execution-denied`). Set it in the provider options of the `ToolModelMessage` (`ProviderMetadata`) or of its output (`OutputProviderOptions`); the output's wins. The result is then sent as an `input_text` array with `prompt_cache_breakpoint`.
- A `LanguageModelTool` with `OutputSchema` is sent to OpenAI Responses as `output_schema`. Its `text` and `execution-denied` results are then sent as JSON string literals, because OpenAI parses the result as JSON.
- Google sends `error-text`, `error-json`, and `execution-denied` on `functionResponse.response.error`. Successful results stay on `response.content`.
