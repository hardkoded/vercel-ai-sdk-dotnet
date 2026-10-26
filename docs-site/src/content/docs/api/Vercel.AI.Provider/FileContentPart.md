---
title: "FileContentPart"
description: "A file or image in a user message."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FileContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FileContentPart : UserContentPart
```


A file or image in a user message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L671)

## Constructors

<a id="Vercel_AI_Provider_FileContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_FileContentPart__ctor_System_String_System_String_System_Byte___System_String_"></a>

### FileContentPart(string, string?, byte[]?, string?)


```csharp
public FileContentPart(string mediaType, string? url, byte[]? data, string? fileName)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L674)

## Properties

<a id="Vercel_AI_Provider_FileContentPart_Data_"></a>
<a id="Vercel_AI_Provider_FileContentPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L723)

<a id="Vercel_AI_Provider_FileContentPart_FileName_"></a>
<a id="Vercel_AI_Provider_FileContentPart_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L726)

<a id="Vercel_AI_Provider_FileContentPart_MediaType_"></a>
<a id="Vercel_AI_Provider_FileContentPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L717)

<a id="Vercel_AI_Provider_FileContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_FileContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part, such as the xAI `imageDetail`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L714)

<a id="Vercel_AI_Provider_FileContentPart_ProviderReference_"></a>
<a id="Vercel_AI_Provider_FileContentPart_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string>? ProviderReference { get; }
```


Map from provider name to an uploaded file id, used when there is no URL or inline data.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L711)

<a id="Vercel_AI_Provider_FileContentPart_Url_"></a>
<a id="Vercel_AI_Provider_FileContentPart_Url"></a>

### Url


```csharp
public string? Url { get; }
```


Remote URL, when the file is not inlined.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L720)

## Methods

<a id="Vercel_AI_Provider_FileContentPart_FromProviderReference_"></a>
<a id="Vercel_AI_Provider_FileContentPart_FromProviderReference_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `FromProviderReference(string, IReadOnlyDictionary<string, string>, string?, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public static FileContentPart FromProviderReference(string mediaType, IReadOnlyDictionary<string, string> providerReference, string? fileName = null, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a file part that refers to an uploaded file. Maps to `data: { type: "reference", reference }`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) | IANA media type. |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Map from provider name to the provider's file id. |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Optional file name. |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Provider-specific options for this part. |

**Returns:** [FileContentPart](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L701)
