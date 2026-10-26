---
title: "OpenAIImageCall"
description: "Image generation settings beyond the shared prompt and size."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIImageCall"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIImageCall"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIImageCall
```


Image generation settings beyond the shared prompt and size.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L13)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIImageCall__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall__ctor_System_String_"></a>

### OpenAIImageCall(string)


```csharp
public OpenAIImageCall(string prompt)
```


Creates an image call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L16)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIImageCall_AspectRatio_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio. OpenAI image models do not accept it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L31)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Count_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Count"></a>

### Count


```csharp
public int Count { get; set; }
```


How many images to generate.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L25)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Files_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Files"></a>

### Files


```csharp
public IReadOnlyList<OpenAIImageFile>? Files { get; set; }
```


Source images for an edit.

**Value:** [IReadOnlyList&lt;OpenAIImageFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L40)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra HTTP headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L46)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Mask_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Mask"></a>

### Mask


```csharp
public OpenAIImageFile? Mask { get; set; }
```


Optional edit mask.

**Value:** [OpenAIImageFile](/api/Vercel.AI.OpenAI/OpenAIImageFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L43)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Prompt_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Prompt"></a>

### Prompt


```csharp
public string Prompt { get; }
```


Image prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L22)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_ProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `openai` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L37)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Seed_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed. OpenAI image models do not accept it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L34)

<a id="Vercel_AI_OpenAI_OpenAIImageCall_Size_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageCall_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Size such as `1024x1024`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L28)
