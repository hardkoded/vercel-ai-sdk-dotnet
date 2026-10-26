---
title: "GeneratedImage"
description: "A generated image file."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedImage"
  hidden: true
---

<a id="Vercel_AI_Operations_GeneratedImage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GeneratedImage
```


A generated image file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L168)

## Constructors

<a id="Vercel_AI_Operations_GeneratedImage__ctor_"></a>
<a id="Vercel_AI_Operations_GeneratedImage__ctor_System_Byte___System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedImage(byte[], string, JsonElement?)


```csharp
public GeneratedImage(byte[] data, string mediaType, JsonElement? providerMetadata = null)
```


Creates a generated image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L171)

## Properties

<a id="Vercel_AI_Operations_GeneratedImage_Base64_"></a>
<a id="Vercel_AI_Operations_GeneratedImage_Base64"></a>

### Base64


```csharp
public string Base64 { get; }
```


Base64 form of [Data](#Vercel_AI_Operations_GeneratedImage_Data).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L182)

<a id="Vercel_AI_Operations_GeneratedImage_Data_"></a>
<a id="Vercel_AI_Operations_GeneratedImage_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Image bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L179)

<a id="Vercel_AI_Operations_GeneratedImage_MediaType_"></a>
<a id="Vercel_AI_Operations_GeneratedImage_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Detected or fallback media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L188)

<a id="Vercel_AI_Operations_GeneratedImage_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_GeneratedImage_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Per-image provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L191)
