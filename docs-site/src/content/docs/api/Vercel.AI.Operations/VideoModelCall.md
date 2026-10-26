---
title: "VideoModelCall"
description: "Arguments shared by generate, start, and status."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoModelCall
```


Arguments shared by generate, start, and status.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L185)

## Constructors

<a id="Vercel_AI_Operations_VideoModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_VideoModelCall__ctor_System_String_System_Int32_System_String_System_String_System_Nullable_System_Double__System_Nullable_System_Int32__System_Nullable_System_Double__Vercel_AI_Operations_VideoModelFile_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoFrameFile__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoModelFile__System_Nullable_System_Boolean__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_System_String_System_Object_"></a>

### `VideoModelCall(string?, int, string?, string?, double?, int?, double?, VideoModelFile?, IReadOnlyList<VideoFrameFile>?, IReadOnlyList<VideoModelFile>?, bool?, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken, string?, object?)`


```csharp
public VideoModelCall(string? prompt, int n, string? aspectRatio, string? resolution, double? duration, int? fps, double? seed, VideoModelFile? image, IReadOnlyList<VideoFrameFile>? frameImages, IReadOnlyList<VideoModelFile>? inputReferences, bool? generateAudio, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, string? webhookUrl = null, object? operation = null)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `n` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `aspectRatio` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `resolution` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `duration` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `fps` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `seed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `image` | [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/) |  |
| `frameImages` | [IReadOnlyList&lt;VideoFrameFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `inputReferences` | [IReadOnlyList&lt;VideoModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `generateAudio` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `webhookUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operation` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L188)

## Properties

<a id="Vercel_AI_Operations_VideoModelCall_AspectRatio_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; }
```


Aspect ratio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L215)

<a id="Vercel_AI_Operations_VideoModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L248)

<a id="Vercel_AI_Operations_VideoModelCall_Duration_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Duration"></a>

### Duration


```csharp
public double? Duration { get; }
```


Duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L221)

<a id="Vercel_AI_Operations_VideoModelCall_Fps_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Fps"></a>

### Fps


```csharp
public int? Fps { get; }
```


Frames per second.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L224)

<a id="Vercel_AI_Operations_VideoModelCall_FrameImages_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_FrameImages"></a>

### FrameImages


```csharp
public IReadOnlyList<VideoFrameFile>? FrameImages { get; }
```


Normalized frames.

**Value:** [IReadOnlyList&lt;VideoFrameFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L233)

<a id="Vercel_AI_Operations_VideoModelCall_GenerateAudio_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_GenerateAudio"></a>

### GenerateAudio


```csharp
public bool? GenerateAudio { get; }
```


Whether to generate audio.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L239)

<a id="Vercel_AI_Operations_VideoModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L245)

<a id="Vercel_AI_Operations_VideoModelCall_Image_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Image"></a>

### Image


```csharp
public VideoModelFile? Image { get; }
```


Resolved start image.

**Value:** [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L230)

<a id="Vercel_AI_Operations_VideoModelCall_InputReferences_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_InputReferences"></a>

### InputReferences


```csharp
public IReadOnlyList<VideoModelFile>? InputReferences { get; }
```


Normalized references. Null when frames were provided.

**Value:** [IReadOnlyList&lt;VideoModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L236)

<a id="Vercel_AI_Operations_VideoModelCall_N_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_N"></a>

### N


```csharp
public int N { get; }
```


Video count for this call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L212)

<a id="Vercel_AI_Operations_VideoModelCall_Operation_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Operation"></a>

### Operation


```csharp
public object? Operation { get; }
```


Opaque operation for a status call.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L254)

<a id="Vercel_AI_Operations_VideoModelCall_Prompt_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L209)

<a id="Vercel_AI_Operations_VideoModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L242)

<a id="Vercel_AI_Operations_VideoModelCall_Resolution_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Resolution"></a>

### Resolution


```csharp
public string? Resolution { get; }
```


Resolution.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L218)

<a id="Vercel_AI_Operations_VideoModelCall_Seed_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_Seed"></a>

### Seed


```csharp
public double? Seed { get; }
```


Seed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L227)

<a id="Vercel_AI_Operations_VideoModelCall_WebhookUrl_"></a>
<a id="Vercel_AI_Operations_VideoModelCall_WebhookUrl"></a>

### WebhookUrl


```csharp
public string? WebhookUrl { get; }
```


Webhook URL for a start call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L251)
