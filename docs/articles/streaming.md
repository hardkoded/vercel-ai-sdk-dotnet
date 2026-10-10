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

## Cancel a stream

Cancel the `CancellationToken` you passed to `StreamTextAsync`, or set `StreamTextOptions.AbortSignal` to cancel with a reason. The call rejects `Text`, `Steps`, `FinishReason`, and `Usage` at once. It does not wait for your callbacks or for the provider to close the response. A reason that is an exception is the exception the tasks fault with. A token faults them with an `OperationCanceledException`.

The stream ends with an `AbortPart`. `OnAbort` receives the steps that finished before the cancel and the exception. `OnError` and `OnFinish` are not called.

```csharp
var controller = new AbortController();
var stream = client.StreamTextAsync(new StreamTextOptions
{
    Model = model,
    Prompt = "Draft a changelog entry.",
    AbortSignal = controller.Signal,
    OnAbort = (context, _) =>
    {
        Console.WriteLine($"Aborted after {context.Steps.Count} steps.");
        return Task.CompletedTask;
    },
});

controller.Abort(new JsError("manual abort"));
```

`Agent.Stream` takes the same `abortSignal` argument. Set `AgentOptions.OnAbort` to receive the abort.

## Sources and citations

`Sources` on a step or a `GenerateTextResult` is the retrieved reference set: the URLs a search returned, which can include more references than the model cites in its answer. Inline references hang off the text they support instead. `GeneratedText.Citations` is an optional list of `Citation` values on each text part in `StepResult.Content` (or `GenerateTextResult.Content`, the last step). Use `Citations` to render inline references and `Sources` to list what was retrieved.

A `Citation` has:

- `Source`: a `GeneratedSource` (a URL, with its title when the provider sent one) or a `GeneratedDocumentSource` (an `Id`, `MediaType`, `Title`, optional `Filename`, and `ProviderMetadata`). Its id identifies the cited resource. It does not have to match an id in `Sources`.
- `StartIndex` and `EndIndex`: optional provider-supplied offsets in the containing text. The end is exclusive. The SDK keeps the offsets as the provider sent them, so check the provider's indexing convention before slicing a .NET string, which is indexed by UTF-16 code unit. When offsets are missing, show the references next to the whole text block.
- `CitedText`: an optional supporting passage from the source. It is source text, not a position in the answer.

Preserve query strings and fragments in source URLs. They can identify a version, page, or passage, and several citations can point at one source at different places in the text.

```csharp
var result = await client.GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "Summarize the news." });
foreach (var part in result.Content.OfType<GeneratedText>())
{
    foreach (var citation in part.Citations ?? Array.Empty<Citation>())
    {
        Console.WriteLine($"{citation.Source} {citation.StartIndex}-{citation.EndIndex}");
    }
}
```

In a model stream, citations arrive on `TextEndStreamPart.Citations`, tied to the id of the text block they end. `StreamTextAsync` keeps them on the matching text part in `StepResult.Content`. `SimulateStreamingMiddleware` copies `GeneratedText.Citations` onto the text-end part it emits, and emits a text block that has citations even when its text is empty. Citation-derived sources can arrive at the end of the provider stream, after the SDK knows whether the provider sent retrieval data.

`UIMessageStream` writes the citations of a step on its `text-end` chunk (`citations`) and still writes retrieved URL sources as `source-url`. It does not write a `source-document` chunk, so document sources stay on `StepResult.Content` and the model stream only. Document sources are not in `Sources`, which holds URL sources.

Providers that do not send citations leave `Citations` null. A null value does not mean `Sources` is a complete retrieval set.
