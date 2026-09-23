# Streaming

`StreamTextAsync` returns immediately. Enumerate `TextStream` for text deltas, or `Stream` for tool calls, tool results, sources, and the finish part.

```csharp
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

## UI message stream

`Vercel.AI.Sdk.AspNetCore` writes the same Server-Sent Events a JavaScript `useChat` client reads:

```csharp
app.MapPost("/chat", (ChatRequest request) =>
{
    var stream = client.StreamTextAsync(new StreamTextOptions { Model = model, Prompt = request.Prompt });
    return stream.ToUIMessageStreamResult();
});
```

The response is `text/event-stream` with `x-vercel-ai-ui-message-stream: v1`. Chunks include text, reasoning, tool input and output, sources, step boundaries, finish, and errors. See COMPATIBILITY.md for chunk types that are not emitted yet.
