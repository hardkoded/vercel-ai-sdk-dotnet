---
title: "FileReferenceContentPart"
description: "A deprecated file-reference part inside a tool result. Use JsonElement%7d) instead. Maps to { type: 'file-reference', providerReference, providerOptions } in..."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileReferenceContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FileReferenceContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FileReferenceContentPart : UserContentPart
```


A deprecated `file-reference` part inside a tool result. Use `JsonElement%7d)` instead.
Maps to `{ type: 'file-reference', providerReference, providerOptions }` in the tool result content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1026)

## Constructors

<a id="Vercel_AI_Provider_FileReferenceContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_FileReferenceContentPart__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `FileReferenceContentPart(IReadOnlyDictionary<string, string>)`


```csharp
public FileReferenceContentPart(IReadOnlyDictionary<string, string> providerReference)
```


Creates a file-reference part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1029)

## Properties

<a id="Vercel_AI_Provider_FileReferenceContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_FileReferenceContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1040)

<a id="Vercel_AI_Provider_FileReferenceContentPart_ProviderReference_"></a>
<a id="Vercel_AI_Provider_FileReferenceContentPart_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Map from provider name to the provider's file id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1037)
