---
title: "ImageCallOptions"
description: "Image call settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageCallOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_ImageCallOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ImageCallOptions
```


Image call settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L183)

## Constructors

<a id="Vercel_AI_Provider_ImageCallOptions__ctor_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions__ctor_System_String_"></a>

### ImageCallOptions(string)


```csharp
public ImageCallOptions(string prompt)
```


Creates image call options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L186)

## Properties

<a id="Vercel_AI_Provider_ImageCallOptions_AspectRatio_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio such as `16:9`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L201)

<a id="Vercel_AI_Provider_ImageCallOptions_Count_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Count"></a>

### Count


```csharp
public int Count { get; set; }
```


How many images to generate.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L195)

<a id="Vercel_AI_Provider_ImageCallOptions_Files_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Files"></a>

### Files


```csharp
public IReadOnlyList<ImageModelFile>? Files { get; set; }
```


Input images for an edit. Maps to `files`.

**Value:** [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L207)

<a id="Vercel_AI_Provider_ImageCallOptions_Headers_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L216)

<a id="Vercel_AI_Provider_ImageCallOptions_Mask_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Mask"></a>

### Mask


```csharp
public ImageModelFile? Mask { get; set; }
```


Edit mask.

**Value:** [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L210)

<a id="Vercel_AI_Provider_ImageCallOptions_Prompt_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Prompt"></a>

### Prompt


```csharp
public string Prompt { get; }
```


Image prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L192)

<a id="Vercel_AI_Provider_ImageCallOptions_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options, keyed by provider id.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L213)

<a id="Vercel_AI_Provider_ImageCallOptions_Seed_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed. Models that do not support it add a warning.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L204)

<a id="Vercel_AI_Provider_ImageCallOptions_Size_"></a>
<a id="Vercel_AI_Provider_ImageCallOptions_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Size string such as `1024x1024`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L198)
