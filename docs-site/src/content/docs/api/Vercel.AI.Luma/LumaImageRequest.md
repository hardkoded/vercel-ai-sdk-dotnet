---
title: "LumaImageRequest"
description: "Luma image generation settings beyond the shared prompt and size."
editUrl: false
prev: false
next: false
sidebar:
  label: "LumaImageRequest"
  hidden: true
---

<a id="Vercel_AI_Luma_LumaImageRequest"></a>

Namespace: [Vercel.AI.Luma](/api/Vercel.AI.Luma/)


```csharp
public sealed class LumaImageRequest
```


Luma image generation settings beyond the shared prompt and size.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L17)

## Constructors

<a id="Vercel_AI_Luma_LumaImageRequest__ctor_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest__ctor_System_String_"></a>

### LumaImageRequest(string)


```csharp
public LumaImageRequest(string prompt)
```


Creates a request for &lt;code class="paramref"&gt;prompt&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L20)

## Properties

<a id="Vercel_AI_Luma_LumaImageRequest_AspectRatio_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio such as `16:9`, sent as `aspect_ratio`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L29)

<a id="Vercel_AI_Luma_LumaImageRequest_Files_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_Files"></a>

### Files


```csharp
public IReadOnlyList<ImageModelFile>? Files { get; set; }
```


Reference images. Each one must be a URL.

**Value:** [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L38)

<a id="Vercel_AI_Luma_LumaImageRequest_Headers_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L50)

<a id="Vercel_AI_Luma_LumaImageRequest_Mask_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_Mask"></a>

### Mask


```csharp
public ImageModelFile? Mask { get; set; }
```


Edit mask. Luma does not accept one; setting it throws.

**Value:** [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L41)

<a id="Vercel_AI_Luma_LumaImageRequest_Prompt_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_Prompt"></a>

### Prompt


```csharp
public string Prompt { get; }
```


Image prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L26)

<a id="Vercel_AI_Luma_LumaImageRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `luma` object is read: `referenceType`, `images`,
`pollIntervalMillis`, and `maxPollAttempts` configure the call, and other fields are sent in the body.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L47)

<a id="Vercel_AI_Luma_LumaImageRequest_Seed_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed. Luma does not accept it; a warning is returned.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L35)

<a id="Vercel_AI_Luma_LumaImageRequest_Size_"></a>
<a id="Vercel_AI_Luma_LumaImageRequest_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Size. Luma does not accept it; a warning is returned.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L32)
