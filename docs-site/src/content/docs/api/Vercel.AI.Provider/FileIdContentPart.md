---
title: "FileIdContentPart"
description: "A deprecated file-id part inside a tool result. Use JsonElement%7d) instead. Maps to { type: 'file-id', fileId, providerOptions } in the tool result content,..."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileIdContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FileIdContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FileIdContentPart : UserContentPart
```


A deprecated `file-id` part inside a tool result. Use `JsonElement%7d)` instead.
Maps to `{ type: 'file-id', fileId, providerOptions }` in the tool result content, where `fileId` is a string or a map from provider name to file id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L997)

## Constructors

<a id="Vercel_AI_Provider_FileIdContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_FileIdContentPart__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `FileIdContentPart(IReadOnlyDictionary<string, string>)`


```csharp
public FileIdContentPart(IReadOnlyDictionary<string, string> fileIds)
```


Creates a file-id part from a map from provider name to file id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileIds` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1007)

<a id="Vercel_AI_Provider_FileIdContentPart__ctor_System_String_"></a>

### FileIdContentPart(string)


```csharp
public FileIdContentPart(string fileId)
```


Creates a file-id part from a single file id. The provider of the model is used as the reference key.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1000)

## Properties

<a id="Vercel_AI_Provider_FileIdContentPart_FileId_"></a>
<a id="Vercel_AI_Provider_FileIdContentPart_FileId"></a>

### FileId


```csharp
public string? FileId { get; }
```


The file id, when it is a string. Exactly one of [FileId](#Vercel_AI_Provider_FileIdContentPart_FileId) and [FileIds](#Vercel_AI_Provider_FileIdContentPart_FileIds) is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1014)

<a id="Vercel_AI_Provider_FileIdContentPart_FileIds_"></a>
<a id="Vercel_AI_Provider_FileIdContentPart_FileIds"></a>

### FileIds


```csharp
public IReadOnlyDictionary<string, string>? FileIds { get; }
```


The map from provider name to file id, when the file id is a record.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1017)

<a id="Vercel_AI_Provider_FileIdContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_FileIdContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1020)
