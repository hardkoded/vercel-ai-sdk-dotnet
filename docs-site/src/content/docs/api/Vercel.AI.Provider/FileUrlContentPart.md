---
title: "FileUrlContentPart"
description: "A deprecated file-url part inside a tool result. Use FileContentPart with a URL instead. Maps to { type: 'file-url', url, mediaType?, providerOptions } in th..."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileUrlContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FileUrlContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FileUrlContentPart : UserContentPart
```


A deprecated `file-url` part inside a tool result. Use [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/) with a URL instead.
Maps to `{ type: 'file-url', url, mediaType?, providerOptions }` in the tool result content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L924)

## Constructors

<a id="Vercel_AI_Provider_FileUrlContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_FileUrlContentPart__ctor_System_String_System_String_"></a>

### FileUrlContentPart(string, string?)


```csharp
public FileUrlContentPart(string url, string? mediaType = null)
```


Creates a file-url part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L927)

## Properties

<a id="Vercel_AI_Provider_FileUrlContentPart_MediaType_"></a>
<a id="Vercel_AI_Provider_FileUrlContentPart_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


IANA media type. When it is null, it is inferred from the URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L938)

<a id="Vercel_AI_Provider_FileUrlContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_FileUrlContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L941)

<a id="Vercel_AI_Provider_FileUrlContentPart_Url_"></a>
<a id="Vercel_AI_Provider_FileUrlContentPart_Url"></a>

### Url


```csharp
public string Url { get; }
```


Remote URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L935)
