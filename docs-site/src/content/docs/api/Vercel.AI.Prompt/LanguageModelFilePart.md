---
title: "LanguageModelFilePart"
description: "A file part in the v4 provider prompt. Maps to the result of convertToLanguageModelV4FilePart."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelFilePart"
  hidden: true
---

<a id="Vercel_AI_Prompt_LanguageModelFilePart"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class LanguageModelFilePart
```


A file part in the v4 provider prompt. Maps to the result of `convertToLanguageModelV4FilePart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L108)

## Constructors

<a id="Vercel_AI_Prompt_LanguageModelFilePart__ctor_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart__ctor_System_String_System_String_"></a>

### LanguageModelFilePart(string, string?)


```csharp
public LanguageModelFilePart(string dataType, string? mediaType)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `dataType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L111)

## Properties

<a id="Vercel_AI_Prompt_LanguageModelFilePart_Base64_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_Base64"></a>

### Base64


```csharp
public string? Base64 { get; set; }
```


Inline base64 text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L124)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_Bytes_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_Bytes"></a>

### Bytes


```csharp
public byte[]? Bytes { get; set; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L121)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_DataType_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_DataType"></a>

### DataType


```csharp
public string DataType { get; }
```


`data`, `url`, `reference`, or `text`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L118)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_MediaType_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type extracted from a data URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L139)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_OriginalUrl_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_OriginalUrl"></a>

### OriginalUrl


```csharp
public string? OriginalUrl { get; set; }
```


Original URL string when parsing changed it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L130)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_Reference_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_Reference"></a>

### Reference


```csharp
public IReadOnlyDictionary<string, string>? Reference { get; set; }
```


Provider reference.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L133)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_Text_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text payload.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L136)

<a id="Vercel_AI_Prompt_LanguageModelFilePart_Url_"></a>
<a id="Vercel_AI_Prompt_LanguageModelFilePart_Url"></a>

### Url


```csharp
public Uri? Url { get; set; }
```


URL.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L127)
