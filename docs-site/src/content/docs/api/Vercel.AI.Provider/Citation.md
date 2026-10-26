---
title: "Citation"
description: "A reference that supports generated text: the source it points at, plus the optional range of the text it supports and the cited passage. It is not a second..."
editUrl: false
prev: false
next: false
sidebar:
  label: "Citation"
  hidden: true
---

<a id="Vercel_AI_Provider_Citation"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class Citation
```


A reference that supports generated text: the source it points at, plus the optional range of the text it supports
and the cited passage. It is not a second copy of the retrieved sources.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L361)

## Constructors

<a id="Vercel_AI_Provider_Citation__ctor_"></a>
<a id="Vercel_AI_Provider_Citation__ctor_Vercel_AI_Provider_GeneratedDocumentSource_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_"></a>

### Citation(GeneratedDocumentSource, int?, int?, string?)


```csharp
public Citation(GeneratedDocumentSource source, int? startIndex = null, int? endIndex = null, string? citedText = null)
```


Creates a citation for a document source.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [GeneratedDocumentSource](/api/Vercel.AI.Provider/GeneratedDocumentSource/) |  |
| `startIndex` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `endIndex` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `citedText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L370)

<a id="Vercel_AI_Provider_Citation__ctor_Vercel_AI_Provider_GeneratedSource_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_"></a>

### Citation(GeneratedSource, int?, int?, string?)


```csharp
public Citation(GeneratedSource source, int? startIndex = null, int? endIndex = null, string? citedText = null)
```


Creates a citation for a URL source.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/) |  |
| `startIndex` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `endIndex` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `citedText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L364)

## Properties

<a id="Vercel_AI_Provider_Citation_CitedText_"></a>
<a id="Vercel_AI_Provider_Citation_CitedText"></a>

### CitedText


```csharp
public string? CitedText { get; }
```


The supporting passage from the source, when the provider supplied one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L393)

<a id="Vercel_AI_Provider_Citation_EndIndex_"></a>
<a id="Vercel_AI_Provider_Citation_EndIndex"></a>

### EndIndex


```csharp
public int? EndIndex { get; }
```


Exclusive end offset in the containing generated text, when the provider supplied one.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L390)

<a id="Vercel_AI_Provider_Citation_Source_"></a>
<a id="Vercel_AI_Provider_Citation_Source"></a>

### Source


```csharp
public GeneratedContent Source { get; }
```


The referenced source: a [GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/) (URL) or a [GeneratedDocumentSource](/api/Vercel.AI.Provider/GeneratedDocumentSource/).

**Value:** [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L384)

<a id="Vercel_AI_Provider_Citation_StartIndex_"></a>
<a id="Vercel_AI_Provider_Citation_StartIndex"></a>

### StartIndex


```csharp
public int? StartIndex { get; }
```


Start offset in the containing generated text, when the provider supplied one.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L387)
