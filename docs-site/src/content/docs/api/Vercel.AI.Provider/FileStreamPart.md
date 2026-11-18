---
title: "FileStreamPart"
description: "A file in a model stream. The data is inline bytes or a URL the SDK downloads before it emits the file. Maps to the file stream part."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FileStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FileStreamPart : LanguageModelStreamPart
```


A file in a model stream. The data is inline bytes or a URL the SDK downloads before it emits the file. Maps to the `file` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2527)

## Constructors

<a id="Vercel_AI_Provider_FileStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_FileStreamPart__ctor_System_Byte___System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### FileStreamPart(byte[], string, JsonElement?)


```csharp
public FileStreamPart(byte[] data, string mediaType, JsonElement? providerMetadata = null)
```


Creates a file part with inline bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2530)

<a id="Vercel_AI_Provider_FileStreamPart__ctor_System_Uri_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### FileStreamPart(Uri, string, JsonElement?)


```csharp
public FileStreamPart(Uri url, string mediaType, JsonElement? providerMetadata = null)
```


Creates a file part that points at a URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2539)

## Properties

<a id="Vercel_AI_Provider_FileStreamPart_Data_"></a>
<a id="Vercel_AI_Provider_FileStreamPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Inline bytes, or null when the file is a URL.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2548)

<a id="Vercel_AI_Provider_FileStreamPart_MediaType_"></a>
<a id="Vercel_AI_Provider_FileStreamPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2554)

<a id="Vercel_AI_Provider_FileStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_FileStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2557)

<a id="Vercel_AI_Provider_FileStreamPart_Url_"></a>
<a id="Vercel_AI_Provider_FileStreamPart_Url"></a>

### Url


```csharp
public Uri? Url { get; }
```


File URL, or null when the bytes are inline.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2551)
