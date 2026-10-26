---
title: "OpenAIImageFile"
description: "An image file sent to the edits endpoint."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIImageFile"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIImageFile"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIImageFile
```


An image file sent to the edits endpoint.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L50)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIImageFile__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageFile__ctor_System_Byte___System_String_System_String_"></a>

### OpenAIImageFile(byte[], string, string?)


```csharp
public OpenAIImageFile(byte[] data, string mediaType, string? fileName)
```


Creates an inline image file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L53)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIImageFile_Data_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageFile_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Image bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L61)

<a id="Vercel_AI_OpenAI_OpenAIImageFile_FileName_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageFile_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


File name used in the multipart body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L67)

<a id="Vercel_AI_OpenAI_OpenAIImageFile_MediaType_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageFile_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L64)
