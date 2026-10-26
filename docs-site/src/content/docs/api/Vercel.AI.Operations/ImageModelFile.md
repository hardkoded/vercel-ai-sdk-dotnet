---
title: "ImageModelFile"
description: "An image sent to doGenerate as a file or URL."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageModelFile"
  hidden: true
---

<a id="Vercel_AI_Operations_ImageModelFile"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImageModelFile
```


An image sent to `doGenerate` as a file or URL.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L14)

## Properties

<a id="Vercel_AI_Operations_ImageModelFile_Base64_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_Base64"></a>

### Base64


```csharp
public string? Base64 { get; }
```


Base64 text when the file was created with `String)`; otherwise null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L54)

<a id="Vercel_AI_Operations_ImageModelFile_Data_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Bytes when [Type](#Vercel_AI_Operations_ImageModelFile_Type) is `file`. Decoded from [Base64](#Vercel_AI_Operations_ImageModelFile_Base64) when the file was created from base64 text.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L51)

<a id="Vercel_AI_Operations_ImageModelFile_FileName_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_FileName"></a>

### FileName


```csharp
public string? FileName { get; set; }
```


File name used when a provider uploads the file as multipart form data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L60)

<a id="Vercel_AI_Operations_ImageModelFile_MediaType_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type of a file image.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L57)

<a id="Vercel_AI_Operations_ImageModelFile_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options for this file, keyed by provider id.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L63)

<a id="Vercel_AI_Operations_ImageModelFile_Type_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_Type"></a>

### Type


```csharp
public string Type { get; }
```


`url` or `file`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L45)

<a id="Vercel_AI_Operations_ImageModelFile_Url_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_Url"></a>

### Url


```csharp
public string? Url { get; }
```


URL when [Type](#Vercel_AI_Operations_ImageModelFile_Type) is `url`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L48)

## Methods

<a id="Vercel_AI_Operations_ImageModelFile_FromBase64_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_FromBase64_System_String_System_String_System_String_"></a>

### FromBase64(string, string, string?)


```csharp
public static ImageModelFile FromBase64(string data, string mediaType, string? fileName = null)
```


Creates an image from base64 text. Providers that send base64 pass the text unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ImageModelFile](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L29)

<a id="Vercel_AI_Operations_ImageModelFile_FromFile_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_FromFile_System_Byte___System_String_System_String_"></a>

### FromFile(byte[], string, string?)


```csharp
public static ImageModelFile FromFile(byte[] data, string mediaType, string? fileName = null)
```


Creates a binary image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ImageModelFile](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L23)

<a id="Vercel_AI_Operations_ImageModelFile_FromUrl_"></a>
<a id="Vercel_AI_Operations_ImageModelFile_FromUrl_System_String_"></a>

### FromUrl(string)


```csharp
public static ImageModelFile FromUrl(string url)
```


Creates a URL image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ImageModelFile](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L17)
