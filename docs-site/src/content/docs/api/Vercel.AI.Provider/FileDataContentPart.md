---
title: "FileDataContentPart"
description: "A deprecated file-data part inside a tool result. Use FileContentPart with inline data instead. Maps to { type: 'file-data', data, mediaType, filename?, prov..."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileDataContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FileDataContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FileDataContentPart : UserContentPart
```


A deprecated `file-data` part inside a tool result. Use [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/) with inline data instead.
Maps to `{ type: 'file-data', data, mediaType, filename?, providerOptions }` in the tool result content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L969)

## Constructors

<a id="Vercel_AI_Provider_FileDataContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_FileDataContentPart__ctor_System_String_System_String_System_String_"></a>

### FileDataContentPart(string, string, string?)


```csharp
public FileDataContentPart(string data, string mediaType, string? fileName = null)
```


Creates a file-data part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L972)

## Properties

<a id="Vercel_AI_Provider_FileDataContentPart_Data_"></a>
<a id="Vercel_AI_Provider_FileDataContentPart_Data"></a>

### Data


```csharp
public string Data { get; }
```


Base64-encoded file data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L981)

<a id="Vercel_AI_Provider_FileDataContentPart_FileName_"></a>
<a id="Vercel_AI_Provider_FileDataContentPart_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L987)

<a id="Vercel_AI_Provider_FileDataContentPart_MediaType_"></a>
<a id="Vercel_AI_Provider_FileDataContentPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L984)

<a id="Vercel_AI_Provider_FileDataContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_FileDataContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L990)
