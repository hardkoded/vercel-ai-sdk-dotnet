---
title: "ImageModelCall"
description: "Arguments for one image model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_ImageModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImageModelCall
```


Arguments for one image model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L67)

## Constructors

<a id="Vercel_AI_Operations_ImageModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_ImageModelCall__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ImageModelFile__Vercel_AI_Operations_ImageModelFile_System_Int32_System_String_System_String_System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `ImageModelCall(string?, IReadOnlyList<ImageModelFile>?, ImageModelFile?, int, string?, string?, int?, JsonElement?, IReadOnlyDictionary<string, string>, CancellationToken)`


```csharp
public ImageModelCall(string? prompt, IReadOnlyList<ImageModelFile>? files, ImageModelFile? mask, int n, string? size, string? aspectRatio, int? seed, JsonElement? providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `files` | [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `mask` | [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/) |  |
| `n` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `size` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `aspectRatio` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `seed` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L70)

## Properties

<a id="Vercel_AI_Operations_ImageModelCall_AspectRatio_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; }
```


Aspect ratio such as `16:9`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L100)

<a id="Vercel_AI_Operations_ImageModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L112)

<a id="Vercel_AI_Operations_ImageModelCall_Files_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_Files"></a>

### Files


```csharp
public IReadOnlyList<ImageModelFile>? Files { get; }
```


Reference images.

**Value:** [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L88)

<a id="Vercel_AI_Operations_ImageModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L109)

<a id="Vercel_AI_Operations_ImageModelCall_Mask_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_Mask"></a>

### Mask


```csharp
public ImageModelFile? Mask { get; }
```


Mask image.

**Value:** [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L91)

<a id="Vercel_AI_Operations_ImageModelCall_N_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_N"></a>

### N


```csharp
public int N { get; }
```


Images requested from this call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L94)

<a id="Vercel_AI_Operations_ImageModelCall_Prompt_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L85)

<a id="Vercel_AI_Operations_ImageModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options. Empty object when the caller omitted them.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L106)

<a id="Vercel_AI_Operations_ImageModelCall_Seed_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_Seed"></a>

### Seed


```csharp
public int? Seed { get; }
```


Seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L103)

<a id="Vercel_AI_Operations_ImageModelCall_Size_"></a>
<a id="Vercel_AI_Operations_ImageModelCall_Size"></a>

### Size


```csharp
public string? Size { get; }
```


Size such as `1024x1024`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L97)
