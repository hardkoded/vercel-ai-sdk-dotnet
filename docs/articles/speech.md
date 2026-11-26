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

`AudioStream` and `FullStream` share one live stream, so only one of them can be accessed, once. Access the stream before awaiting `Usage`, `Warnings`, `Responses`, or `ProviderMetadata`; accessing a result promise first consumes the stream internally. Both streams expose `Cancel()`. Cancelling, or disposing an enumerator early, aborts the model call through `SpeechModelStreamCall.CancellationToken` and rejects the result promises with an `OperationCanceledException`. A stream that is not read stalls under backpressure and keeps the provider call open: the output holds one part, so a slow consumer pauses reads from the model.

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

### ElevenLabs streaming speech

`ElevenLabsSpeechModel` implements `IStreamingSpeechCaller`, so `StreamSpeech.Start` uses the ElevenLabs text-to-speech WebSocket (`stream-input`). Text deltas are forwarded as they arrive, so you can pipe a language model's text stream straight into speech, and audio chunks stream back as they are generated. Music and sound-effects models do not stream; `DoStreamAsync` throws an `UnsupportedFunctionalityException`.

```csharp
var result = StreamSpeech.Start(new StreamSpeechRequest
{
    Model = (IStreamingSpeechCaller)ElevenLabsProvider.Create().SpeechModel("eleven_flash_v2_5"),
    TextStream = textDeltas, // an IAsyncEnumerable<SpeechTextPart>
    Voice = "21m00Tcm4TlvDq8ikWAM",
    OutputFormat = new SpeechAudioFormat("audio/pcm", 24000),
    ProviderOptions = JsonDocument.Parse("""{"elevenlabs":{"streaming":{"chunkLengthSchedule":[50,120,160],"syncAlignment":true}}}""").RootElement.Clone(),
});
```

The API key is sent in the WebSocket's first message, not in a header. Run streaming speech on the server: in browser code the API key would be exposed to the client. Set `ElevenLabsOptions.WebSocket` to a `WebSocketFactory` to replace `ClientWebSocket`.

`OutputFormat` maps to an ElevenLabs `output_format`: `audio/mpeg` (44.1 kHz, or 22.05 kHz), `audio/pcm` (8, 16, 22.05, 24 (default), 44.1, or 48 kHz), `audio/pcmu` and `audio/pcma` (8 kHz), and `audio/opus` or `audio/ogg` (48 kHz Opus, delivered in an Ogg container and reported as `audio/ogg`). Other formats throw an `InvalidArgumentError`. Set `providerOptions.elevenlabs.streaming.outputFormat` to a native ElevenLabs format (for example `mp3_44100_192`) to choose one directly. Without either, audio is `mp3_44100_128`.

The text-to-speech WebSocket speaks one voice per stream. A text part whose `Voice` differs from the call's `Voice` fails the stream with an `InvalidArgumentError`; use dialogue (below) for several voices.

Text deltas that end mid-word are held back until the next whitespace before they are sent, and the rest is flushed when the text stream ends. CJK sentence punctuation also counts as a boundary, and held text is sent after 100 characters regardless. While the text stream is idle, the provider sends keepalive messages every 15 seconds so the connection outlives the server's default inactivity timeout. Setting `streaming.inactivityTimeout` turns keepalives off, so the timeout you set applies.

`Usage.InputCharacters` is the number of Unicode code points sent for synthesis. Audio parts carry ElevenLabs `alignment` and `normalizedAlignment` data in `ProviderMetadata.elevenlabs` when the server returns it.

The `streaming` provider options mirror the WebSocket settings: `autoMode`, `chunkLengthSchedule`, `enableSsmlParsing`, `inactivityTimeout` (seconds, up to 180), `outputFormat`, and `syncAlignment`. `GenerateAsync` ignores them with a warning. `voiceSettings`, `pronunciationDictionaryLocators` (each needs a `versionId`), `seed`, `applyTextNormalization`, `languageCode`, and `enableLogging` apply as for `GenerateAsync`. `previousText`, `nextText`, `previousRequestIds`, `nextRequestIds`, `applyLanguageTextNormalization`, `withTimestamps`, `inputs`, and the music and sound-effects options are not supported over the WebSocket and produce warnings. The `eleven_v3` and `eleven_v4` model families are not served by the text-to-speech WebSocket; stream them as dialogue.

#### Streaming dialogue

Set `providerOptions.elevenlabs.dialogue` to stream multi-speaker dialogue over the Text-to-Dialogue WebSocket with an `eleven_v3` or `eleven_v4` model. List the voices in `dialogue.voices` (the socket needs them up front), then give each text part its speaker with `Voice`. Parts without a `Voice` use the call's `Voice`, or else the first dialogue voice; a voice that is not listed fails the stream. A change of voice starts a new turn. To start a new turn with the same voice, set `providerOptions` of the part to `{"elevenlabs":{"newTurn":true}}`.

```csharp
var result = StreamSpeech.Start(new StreamSpeechRequest
{
    Model = (IStreamingSpeechCaller)ElevenLabsProvider.Create().SpeechModel("eleven_v4"),
    TextParts = new[]
    {
        new SpeechTextPart("Did you hear we can stream dialogue now? ", voice: alice),
        new SpeechTextPart("Over one socket? That is neat.", voice: bob),
    },
    ProviderOptions = JsonDocument.Parse($$"""{"elevenlabs":{"dialogue":{"voices":["{{alice}}","{{bob}}"],"voiceSettings":{"stability":0.5}}}}""").RootElement.Clone(),
});
```

Text is held back to word boundaries within each speaker's text and flushed when the speaker changes. Text-to-Dialogue ignores `Speed`, top-level `voiceSettings` (use `dialogue.voiceSettings.stability`), and the `autoMode`, `chunkLengthSchedule`, `enableSsmlParsing`, and `inactivityTimeout` streaming options; each produces a warning. `eleven_v4` models receive the text incrementally. `eleven_v3` models accept a single message per dialogue session, so the provider reads the whole text stream before sending it. `eleven_v3_conversational` supports a single voice. Text-to-Dialogue has a fixed inactivity timeout, so keepalives are always sent while the text stream is idle. Audio parts carry `alignment` (camelCase) and `isFinalAudioForTurn` in `ProviderMetadata.elevenlabs` when the server returns them; a turn boundary without audio arrives as an empty audio part carrying only `isFinalAudioForTurn`.

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

Read the `assemblyai` provider options with `AssemblyAISpeechModelOptions.Parse(request.ProviderOptions)`. It validates `Encoding`, `Container`, `SampleRate` and `BitRate`.

## Speech telemetry

`GenerateSpeechRequest.Telemetry` (a `TelemetryOptions`) delivers a `GenerateSpeechStartEvent` to `ITelemetry.OnStart` and a `GenerateSpeechEndEvent` to `ITelemetry.OnEnd` (`ai.generateSpeech`). A failure calls `OnError`. The end event carries the text, the audio metadata (`Audio.ByteLength`, `Audio.MediaType`, `Audio.Format`), the provider `Usage` (`SpeechModelResult.Usage`, in the provider's native shape), the warnings, the provider metadata, and a `Response` with only the timestamp, model id and headers. Raw audio and the raw response body are never part of an event. `RecordInputs = false` sends `Text` as `""` and empty `ProviderOptions`. Either flag set to `false` hides `ProviderMetadata`.

## Transcription telemetry

`Transcribe.TranscribeAsync` accepts a `TranscribeRequest.Telemetry`, a `TelemetryOptions`. Its `Integrations` are `ITelemetry` objects. Their `OnStart`, `OnEnd` and `OnError` callbacks receive start, end and error events. `OnStart` receives an `IOperationStartEvent` and `OnEnd` an `IOperationEndEvent`, which are a `TranscriptionStartEvent` and a `TranscriptionEndEvent` here. `TranscribeTelemetry` is gone. They carry the provider and model, the audio byte length and media type, the transcript, warnings, response metadata, provider metadata and the provider's usage when it reports one. The start event also carries the `TimestampGranularities` you asked for. The end event carries the provider's `Words`, whether you requested them or the provider returned them by default.

```csharp
var transcript = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = model,
    Audio = await File.ReadAllBytesAsync("audio.mp3"),
    Telemetry = new TelemetryOptions
    {
        FunctionId = "transcribe-customer-call",
        Integrations = [new MyTelemetry()], // MyTelemetry.OnEnd prints ((TranscriptionEndEvent)e).Audio.ByteLength
    },
});
```

Raw audio is never part of an event. Set `TelemetryOptions.RecordInputs = false` or `RecordOutputs = false` to hide sensitive fields. With `RecordOutputs = false`, the end event sends `Text` as `""`, and `Segments` and `Words` as empty lists. `ProviderMetadata` is hidden when either flag is `false`. The result you get back keeps the full transcript.

For URL audio that the SDK downloads, the downloaded media type is passed to the provider and reported on the events. When the provider fetches the URL itself, `Audio.ByteLength` and `Audio.MediaType` are `null`, because the SDK never inspects the audio. If downloading the audio fails, the start event is still emitted before the error event, with `null` audio metadata.

`ITelemetry` also has `OnStreamTranscriptionStart` and `OnStreamTranscriptionEnd` (upstream's `experimental_onStreamTranscriptionStart` and `experimental_onStreamTranscriptionEnd`, with `StreamTranscriptionStartEvent` and `StreamTranscriptionEndEvent`). The dispatcher filters and delivers them, but `StreamTranscribe.Start` does not emit them yet.
