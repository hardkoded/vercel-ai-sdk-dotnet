---
title: "AssistantFilePart"
description: "A file in an assistant message. Maps to the assistant file part."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantFilePart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantFilePart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class AssistantFilePart : AssistantContentPart
```


A file in an assistant message. Maps to the assistant `file` part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

**Derived:** [AssistantReasoningFilePart](/api/Vercel.AI.Provider/AssistantReasoningFilePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L798)

## Constructors

<a id="Vercel_AI_Provider_AssistantFilePart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantFilePart__ctor_System_String_System_String_System_Byte___System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantFilePart(string, string?, byte[]?, string?, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantFilePart(string mediaType, string? url, byte[]? data, string? fileName = null, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) | IANA media type. |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Remote URL, when the file is not inlined. |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) | Inline bytes. |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Optional file name. |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Provider-specific options for this part. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L806)

## Properties

<a id="Vercel_AI_Provider_AssistantFilePart_Data_"></a>
<a id="Vercel_AI_Provider_AssistantFilePart_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L838)

<a id="Vercel_AI_Provider_AssistantFilePart_FileName_"></a>
<a id="Vercel_AI_Provider_AssistantFilePart_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L841)

<a id="Vercel_AI_Provider_AssistantFilePart_MediaType_"></a>
<a id="Vercel_AI_Provider_AssistantFilePart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L832)

<a id="Vercel_AI_Provider_AssistantFilePart_Url_"></a>
<a id="Vercel_AI_Provider_AssistantFilePart_Url"></a>

### Url


```csharp
public string? Url { get; }
```


Remote URL, when the file is not inlined.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L835)
