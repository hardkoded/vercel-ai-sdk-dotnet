---
title: "ReasoningFileStreamPart"
description: "A file the model produced as part of its reasoning, in a model stream. The data is inline bytes or a URL the SDK downloads before it emits the file. Maps to..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningFileStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ReasoningFileStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ReasoningFileStreamPart : LanguageModelStreamPart
```


A file the model produced as part of its reasoning, in a model stream. The data is inline bytes or a URL the SDK downloads
before it emits the file. Maps to the `reasoning-file` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2685)

## Constructors

<a id="Vercel_AI_Provider_ReasoningFileStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ReasoningFileStreamPart__ctor_System_Byte___System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningFileStreamPart(byte[], string, JsonElement?)


```csharp
public ReasoningFileStreamPart(byte[] data, string mediaType, JsonElement? providerMetadata = null)
```


Creates a reasoning-file part with inline bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2688)

<a id="Vercel_AI_Provider_ReasoningFileStreamPart__ctor_System_Uri_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningFileStreamPart(Uri, string, JsonElement?)


```csharp
public ReasoningFileStreamPart(Uri url, string mediaType, JsonElement? providerMetadata = null)
```


Creates a reasoning-file part that points at a URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2701)

## Properties

<a id="Vercel_AI_Provider_ReasoningFileStreamPart_Data_"></a>
<a id="Vercel_AI_Provider_ReasoningFileStreamPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Inline bytes, or null when the file is a URL.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2710)

<a id="Vercel_AI_Provider_ReasoningFileStreamPart_MediaType_"></a>
<a id="Vercel_AI_Provider_ReasoningFileStreamPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2716)

<a id="Vercel_AI_Provider_ReasoningFileStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ReasoningFileStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2719)

<a id="Vercel_AI_Provider_ReasoningFileStreamPart_Url_"></a>
<a id="Vercel_AI_Provider_ReasoningFileStreamPart_Url"></a>

### Url


```csharp
public Uri? Url { get; }
```


File URL, or null when the bytes are inline.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2713)
