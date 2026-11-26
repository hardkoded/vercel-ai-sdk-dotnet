# HeyGen

`HeyGenProvider` in the `Vercel.AI.HeyGen` namespace creates the HeyGen models that this port implements. The API reference lists every option: [HeyGenProvider](/api/Vercel.AI.HeyGen/HeyGenProvider/).

Browse the upstream catalog of HeyGen models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Video | `Video`, `VideoModel` | `heygen-video-1` |
| Decision | `DecisionModel` | `heygen-decision-1`, `heygen-decision-1-turbo` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Video model:

```csharp
using Vercel.AI.HeyGen;

var model = (IVideoCaller)HeyGenProvider.Create().Video("heygen-video-1");
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = model,
        Prompt = new VideoPrompt("A yorkie among dandelions at Crissy Field."),
    }
);

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
```

See the [Providers](providers.md) guide for authentication and provider options.
