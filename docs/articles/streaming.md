# Streaming

`StreamTextAsync` returns immediately. Enumerate `TextStream` for text deltas, or `Stream` for tool calls, tool results, sources, and the finish part.

<!-- snippet: streaming -->
<a id='snippet-streaming'></a>
```cs
var stream = client.StreamTextAsync(new StreamTextOptions
{
    Model = model,
    Prompt = "Draft a changelog entry.",
});

await foreach (var delta in stream.TextStream())
{
    Console.Write(delta);
}

Console.WriteLine();
Console.WriteLine(await stream.FinishReason);
```
<sup><a href='/samples/Vercel.AI.Examples/StreamingExample.cs#L18-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-streaming' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## UI message stream

`Vercel.AI.AspNetCore` writes the same Server-Sent Events a JavaScript `useChat` client reads:

```csharp
app.MapPost("/chat", (ChatRequest request) =>
{
    var stream = client.StreamTextAsync(new StreamTextOptions { Model = model, Prompt = request.Prompt });
    return stream.ToUIMessageStreamResult(keepAliveMs: 15_000);
});
```

The response is `text/event-stream` with `x-vercel-ai-ui-message-stream: v1`. Chunks include text, reasoning, tool input and output, sources, step boundaries, finish, and errors. See COMPATIBILITY.md for chunk types that are not emitted yet.

`keepAliveMs` is optional. When it is set, the response writes `: stream-open` before the start event and `: keep-alive` whenever the next part is still pending after that many milliseconds. Omit it, or pass null, and the stream contains no comment lines.
