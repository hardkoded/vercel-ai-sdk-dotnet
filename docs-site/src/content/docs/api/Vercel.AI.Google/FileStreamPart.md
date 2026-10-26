---
title: "FileStreamPart"
description: "A streamed file part. The shared stream model has no file delta, so this carries the bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileStreamPart"
  hidden: true
---

<a id="Vercel_AI_Google_FileStreamPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class FileStreamPart : LanguageModelStreamPart
```


A streamed file part. The shared stream model has no file delta, so this carries the bytes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L702)

## Constructors

<a id="Vercel_AI_Google_FileStreamPart__ctor_"></a>
<a id="Vercel_AI_Google_FileStreamPart__ctor_System_Byte___System_String_"></a>

### FileStreamPart(byte[], string)


```csharp
public FileStreamPart(byte[] data, string mediaType)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L705)

## Properties

<a id="Vercel_AI_Google_FileStreamPart_Data_"></a>
<a id="Vercel_AI_Google_FileStreamPart_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


File bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L713)

<a id="Vercel_AI_Google_FileStreamPart_MediaType_"></a>
<a id="Vercel_AI_Google_FileStreamPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L716)
