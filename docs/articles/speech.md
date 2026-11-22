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

### Per-call provider options and headers

`GenerateSpeechOptions` and `SpeechCallOptions` take `ProviderOptions` (keyed by provider name) and `Headers` (per-call headers that override the provider headers; a `null` value removes nothing and is ignored by providers that read them). `GoogleSpeechModel.DoGenerateAsync(SpeechCallOptions, ...)` forwards both to `GoogleSpeechCall`, so `GenerateSpeech` reaches the same behavior as `GoogleSpeechModel.GenerateAsync`.

### Implementing a streaming speech model

Implement `IStreamingSpeechCaller.DoStreamAsync`, which receives a `SpeechModelStreamCall` and returns a `SpeechModelStreamResult` over an `IAsyncEnumerable<SpeechModelStreamPart>`. The call's `Text` is owned by the model once `DoStreamAsync` is called: enumerate it to the end or dispose its enumerator on every terminal path.

### Gemini speech language

`GoogleSpeechCall.Language` takes a supported BCP-47 code such as `en-US` or `de-DE`. `GoogleSpeechModel` forwards an explicit code unchanged as `speechConfig.languageCode`, for single-speaker and multi-speaker (`multiSpeakerVoiceConfig`) requests, including through the Vertex Gemini adapter. Omit `Language` or set it to `"auto"` to let Gemini detect the language from the input text. Gemini TTS does not support `Speed`: passing it adds an `unsupported` warning.

```csharp
var model = (GoogleSpeechModel)GoogleProvider.Create().SpeechModel("gemini-3.8-flash-tts");
var result = await model.GenerateAsync(
    new GoogleSpeechCall("Guten Morgen! Willkommen beim AI SDK.") { Language = "de-DE" },
    CancellationToken.None);
```

### Gemini speech output

Gemini 3.8 and newer TTS models (and custom model IDs) take structured speech. Set `instructions` to give a style, or pass `speechMetadata` (`speaker`, `style`) and `turns` under the `google` provider options. `turns` replace the top-level text (pass an empty string), and each turn may carry its own `speechMetadata`. Per-turn styles override `instructions`, and an empty style clears it. With `multiSpeakerVoiceConfig`, every turn must name a configured `speechMetadata.speaker`. Global `speechMetadata` together with `turns`, custom voices (`voice_*`, `voicekey_*`, or a `voice` in a speaker config) and `turns` on older models throw an `ArgumentException`. These models also accept `OutputFormat` values `mulaw` and `alaw` (and the `audio/*` forms). Raw formats return the bytes unchanged, and `wav` adds no extra header.

`GoogleSpeechModel.GenerateAsync` returns a `GoogleSpeechResult`. Older Gemini TTS models return raw PCM, so `Audio` wraps it in a 44-byte WAV header unless you request `OutputFormat = "pcm"`. In that case you get the raw bytes and an `unsupported` `outputFormat` warning. WAV responses pass through unchanged. The result also carries the raw `Usage` (`usageMetadata`, including cached tokens), `SampleRate` and `MimeType` (the `providerMetadata.google` values), `Timestamp`, `ModelId`, `Headers`, `RequestBody` and `ResponseBody`. A response without inline audio gives an empty `Audio`. Through `GenerateSpeech`, the sample rate and mime type arrive as `providerMetadata.google`.

## AssemblyAI speech

`AssemblyAIProvider.SpeechModel("assemblyai-tts-1")` synthesizes buffered speech; see the AssemblyAI entries in [Providers](providers.md) for the output formats and `assemblyai` provider options.

```csharp
var model = AssemblyAIProvider.Create().Speech("assemblyai-tts-1");
var result = await model.GenerateAsync(
    new AssemblyAISpeechRequest("Hola, mundo!") { Voice = "aai-kyla", Language = "es", OutputFormat = "wav" },
    CancellationToken.None);
Console.WriteLine(result.Usage?.Characters);
```

## Transcription telemetry

`Transcribe.TranscribeAsync` accepts a `TranscribeRequest.Telemetry` object. Its `OnStart`, `OnEnd` and `OnError` callbacks receive start, end and error events. They carry the provider and model, the audio byte length and media type, the transcript, warnings, response metadata, provider metadata and the provider's usage when it reports one. The start event also carries the `TimestampGranularities` you asked for. The end event carries the provider's `Words`, whether you requested them or the provider returned them by default.

```csharp
var transcript = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = model,
    Audio = await File.ReadAllBytesAsync("audio.mp3"),
    Telemetry = new TranscribeTelemetry
    {
        FunctionId = "transcribe-customer-call",
        OnEnd = e => { Console.WriteLine(e.Audio.ByteLength); return Task.CompletedTask; },
    },
});
```

Raw audio is never part of an event. Set `RecordInputs = false` or `RecordOutputs = false` to hide sensitive fields. With `RecordOutputs = false`, the end event sends `Text` as `""`, and `Segments` and `Words` as empty lists. `ProviderMetadata` is hidden when either flag is `false`. The result you get back keeps the full transcript.

For URL audio that the SDK downloads, the downloaded media type is passed to the provider and reported on the events. When the provider fetches the URL itself, `Audio.ByteLength` and `Audio.MediaType` are `null`, because the SDK never inspects the audio. If downloading the audio fails, the start event is still emitted before the error event, with `null` audio metadata.
