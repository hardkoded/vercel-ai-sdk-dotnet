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
