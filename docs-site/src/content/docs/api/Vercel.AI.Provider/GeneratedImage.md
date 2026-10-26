---
title: "GeneratedImage"
description: "One generated image."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedImage"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedImage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedImage
```


One generated image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L95)

## Constructors

<a id="Vercel_AI_Provider_GeneratedImage__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedImage__ctor_System_String_System_Byte___System_String_"></a>

### GeneratedImage(string, byte[]?, string?)


```csharp
public GeneratedImage(string mediaType, byte[]? data, string? url)
```


Creates an image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L98)

## Properties

<a id="Vercel_AI_Provider_GeneratedImage_Data_"></a>
<a id="Vercel_AI_Provider_GeneratedImage_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Inline image bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L109)

<a id="Vercel_AI_Provider_GeneratedImage_MediaType_"></a>
<a id="Vercel_AI_Provider_GeneratedImage_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L106)

<a id="Vercel_AI_Provider_GeneratedImage_Url_"></a>
<a id="Vercel_AI_Provider_GeneratedImage_Url"></a>

### Url


```csharp
public string? Url { get; }
```


Remote URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L112)
