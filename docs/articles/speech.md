# Speech

## Streaming speech

> Streaming speech is an experimental feature.

Use `StreamSpeech.Start` (`experimental_streamSpeech`) when you want audio before the full text is synthesized, or when the text itself arrives incrementally. The model must implement `IStreamingSpeechCaller`; other speech models throw an `UnsupportedFunctionalityException`.

```csharp
using Vercel.AI.Operations;

var result = StreamSpeech.Start(new StreamSpeechRequest
{
    Model = speechModel, // an IStreamingSpeechCaller
    TextStream = textDeltas, // an IAsyncEnumerable<SpeechTextPart>
    Voice = "21m00Tcm4TlvDq8ikWAM",
    OutputFormat = new SpeechAudioFormat("audio/mpeg"),
});

await foreach (var chunk in result.AudioStream)
{
    // chunk is a byte[] of audio in generation order
}

Console.WriteLine((await result.Usage).InputCharacters); // e.g. 512
```

The text is one of `Text` (a complete string), `TextParts` (a sequence of text parts) or `TextStream` (an `IAsyncEnumerable<SpeechTextPart>` synthesized as it arrives). A string converts implicitly to a `SpeechTextPart`. Providers with an incremental text transport synthesize deltas as they arrive; others read the text stream to completion first. The end of the text stream ends the input. A request with none of them throws an `InvalidArgumentException`.

A `SpeechTextPart` can carry its own `Voice` and `ProviderOptions` to give parts of the text their own voice (for example for dialogue) on providers that support several voices per stream. Single-voice providers fail the stream when a part's `Voice` differs from the call's `Voice`. A script that is known up front can be passed as `TextParts`:

```csharp
var result = StreamSpeech.Start(new StreamSpeechRequest
{
    Model = speechModel,
    TextParts = new[]
    {
        new SpeechTextPart("Did you hear? ", voice: alice),
        new SpeechTextPart("No, what?", voice: bob),
    },
});
```

`error` parts from the provider are not fatal: `FullStream` includes them, `AudioStream` skips them, and each one is also passed to `OnError` (which defaults to writing the error to `Console.Error`). The callback is not awaited, and exceptions it throws are ignored.

Use `FullStream` instead of `AudioStream` when you need per-chunk metadata: each `audio` part carries the chunk `AudioFormat` (for example `audio/pcm` at 24000 Hz) and optional `ProviderMetadata`. Audio chunks are not standalone files; concatenate them in order.

`AudioStream` and `FullStream` share one live stream, so only one of them can be accessed, once. Access the stream before awaiting `Usage`, `Warnings`, `Responses`, or `ProviderMetadata`; accessing a result promise first consumes the stream internally. Both streams expose `Cancel()`. Cancelling, or disposing an enumerator early, aborts the model call through `SpeechModelStreamCall.CancellationToken` and rejects the result promises with an `OperationCanceledException`.

### Result

| Member | Type | Meaning |
| --- | --- | --- |
| `AudioStream` | `OperationFullStream<byte[]>` | Audio bytes in generation order. |
| `FullStream` | `OperationFullStream<SpeechStreamPart>` | `audio`, `raw`, and `error` parts. |
| `Usage` | `Task<SpeechStreamUsage>` | `InputCharacters`, `InputTokens`, `OutputTokens`, `Raw`. |
| `Warnings` | `Task<IReadOnlyList<OperationWarning>>` | Warnings for the call. |
| `Responses` | `Task<IReadOnlyList<ProviderResponse>>` | Response metadata. |
| `ProviderMetadata` | `Task<JsonElement>` | Provider-specific metadata. |

A stream that finishes without audio fails with a `NoSpeechGeneratedException` whose `InnerException` is the last provider error part. A stream that ends after audio without a `finish` part fails with the last provider error, or a `StreamProviderError`.

### Implementing a streaming speech model

Implement `IStreamingSpeechCaller.DoStreamAsync`, which receives a `SpeechModelStreamCall` and returns a `SpeechModelStreamResult` over an `IAsyncEnumerable<SpeechModelStreamPart>`. The call's `Text` is owned by the model once `DoStreamAsync` is called: enumerate it to the end or dispose its enumerator on every terminal path.
