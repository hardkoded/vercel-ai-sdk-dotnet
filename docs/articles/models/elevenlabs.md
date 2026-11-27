# ElevenLabs

`ElevenLabsProvider` in the `Vercel.AI.ElevenLabs` namespace creates the ElevenLabs models that this port implements. The API reference lists every option: [ElevenLabsProvider](/api/Vercel.AI.ElevenLabs/ElevenLabsProvider/).

Browse the upstream catalog of ElevenLabs models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `SpeechModel` | `eleven_multilingual_v2` |
| Transcription | `TranscriptionModel` | `scribe_v1` |
| Streaming transcription | `TranscriptionModel` | `scribe_v2_realtime` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Speech model:

```csharp
using Vercel.AI.ElevenLabs;

var model = (ISpeechCaller)ElevenLabsProvider.Create().SpeechModel("eleven_multilingual_v2");
var result = await GenerateSpeech.GenerateSpeechAsync(
    new GenerateSpeechRequest { Model = model, Text = "Hello from the Vercel AI SDK for .NET." }
);

await File.WriteAllBytesAsync("speech.mp3", result.Audio.Data);
```

Transcription model:

```csharp
using Vercel.AI.ElevenLabs;

var model = (ElevenLabsProvider.ElevenLabsTranscriptionModel)ElevenLabsProvider.Create().TranscriptionModel("scribe_v1");
var result = await model.TranscribeAsync(
    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),
    new ElevenLabsTranscriptionRequest(),
    CancellationToken.None
);

Console.WriteLine(result.Text);
```

Streaming transcription with Scribe v2 Realtime:

`ElevenLabsTranscriptionModel` implements `ITranscriptionCaller`, so `scribe_v2_realtime` works with `StreamTranscribe`. The model streams over the WebSocket `/v1/speech-to-text/realtime` and takes raw `audio/pcm` at 8000, 16000 (the default), 22050, 24000, 44100 or 48000 Hz, or `audio/pcmu` at 8000 Hz. Calling `DoGenerateAsync` or `TranscribeAsync` on a realtime model, or `DoStreamAsync` on a batch model, throws `UnsupportedFunctionalityException`.

The stream emits `transcript-partial` parts, a `transcript-final` for each committed transcript, and one `finish`. Set `providerOptions.elevenlabs.streaming.includeTimestamps` to get word `Segments` and the duration on `finish`. `includeLanguageDetection` makes `Language` the detected language (it is the requested `languageCode` until then) and also requests timestamps from the API, but `Segments` stay empty unless you set `includeTimestamps`. `filterBackgroundAudio` cannot be combined with `includeTimestamps` or `includeLanguageDetection` and throws `InvalidArgumentException`. The `streaming` options are `commitStrategy` (`manual` or `vad`), `enableLogging`, `filterBackgroundAudio`, `includeLanguageDetection`, `includeTimestamps`, `keyterms`, `minSilenceDurationMs`, `minSpeechDurationMs`, `noVerbatim`, `previousText`, `secondaryLanguages`, `vadSilenceThresholdSecs` and `vadThreshold`. Explicit `null` values count as absent. Batch-only options (`diarize`, `fileFormat`, `numSpeakers`, `tagAudioEvents`, `timestampsGranularity`) add an `unsupported` warning to `stream-start`, and `streaming` on a batch model adds one too.

The realtime endpoint authenticates with the `xi-api-key` header. `ClientWebSocket` can send it on current .NET runtimes. If a socket error occurs while you use the default transport, the message says to pass a header-capable implementation through `ElevenLabsOptions.WebSocket`.

`DoGenerateAsync` returns the raw word items, including spacing and audio events, in `ProviderMetadata` under `elevenlabs.words`. `providerOptions.elevenlabs` is validated for batch and streaming calls (`InvalidArgumentError`), for example `numSpeakers` from 1 to 32 or `vadThreshold` from 0.1 to 0.9.

See the [Providers](providers.md) guide for authentication and provider options.
