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

Set `ApproveTool` to veto a call before it runs. A denied call is stored as an error tool result.
