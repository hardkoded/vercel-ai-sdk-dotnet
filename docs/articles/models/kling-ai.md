# Kling AI

`KlingAIProvider` in the `Vercel.AI.KlingAI` namespace creates the Kling AI models that this port implements. The API reference lists every option: [KlingAIProvider](/api/Vercel.AI.KlingAI/KlingAIProvider/).

Browse the upstream catalog of Kling AI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Video | `VideoModel` | `kling-v3.0-t2v`, `kling-v3.0-i2v`, `kling-v3.0-motion-control` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Video model:

```csharp
using Vercel.AI.KlingAI;

var model = (IVideoCaller)KlingAIProvider.Create().VideoModel("kling-v3.0-t2v");
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
