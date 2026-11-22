# Vercel.AI.Mistral.LiveKit

LiveKit adapter for the Mistral realtime voice transport of [Vercel.AI](https://www.nuget.org/packages/Vercel.AI), over the
[Livekit.Rtc.Dotnet](https://www.nuget.org/packages/Livekit.Rtc.Dotnet) client. It targets `net10.0` and brings the native
LiveKit binaries, so `Vercel.AI` stays free of native dependencies. Community package, not an official Vercel product.

```csharp
using Vercel.AI.Mistral.LiveKit;

using var liveKit = new LiveKitRtcClient(
    new LiveKitRtcClientOptions
    {
        // Receives each frame of the agent's voice while the transport plays it.
        OnRemoteAudioFrame = frame => PlayAsync(frame),
    }
);

var model = provider.ExperimentalRealtime.Model("mistral-voice", liveKit);

// Capture microphone frames into liveKit.MicrophoneSource (AudioSource.CaptureFrameAsync).
```
