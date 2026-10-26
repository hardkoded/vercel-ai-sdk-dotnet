---
title: "SourceDocumentUIPart"
description: "A source that is a document. Maps to SourceDocumentUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "SourceDocumentUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_SourceDocumentUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class SourceDocumentUIPart : UIMessagePart
```


A source that is a document. Maps to `SourceDocumentUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L146)

## Constructors

<a id="Vercel_AI_Ui_SourceDocumentUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_SourceDocumentUIPart__ctor_System_String_System_String_System_String_"></a>

### SourceDocumentUIPart(string, string, string)


```csharp
public SourceDocumentUIPart(string sourceId, string mediaType, string title)
```


Creates a source document part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sourceId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L149)

## Properties

<a id="Vercel_AI_Ui_SourceDocumentUIPart_Filename_"></a>
<a id="Vercel_AI_Ui_SourceDocumentUIPart_Filename"></a>

### Filename


```csharp
public string? Filename { get; init; }
```


The file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L167)

<a id="Vercel_AI_Ui_SourceDocumentUIPart_MediaType_"></a>
<a id="Vercel_AI_Ui_SourceDocumentUIPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L161)

<a id="Vercel_AI_Ui_SourceDocumentUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_SourceDocumentUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L170)

<a id="Vercel_AI_Ui_SourceDocumentUIPart_SourceId_"></a>
<a id="Vercel_AI_Ui_SourceDocumentUIPart_SourceId"></a>

### SourceId


```csharp
public string SourceId { get; }
```


The source id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L158)

<a id="Vercel_AI_Ui_SourceDocumentUIPart_Title_"></a>
<a id="Vercel_AI_Ui_SourceDocumentUIPart_Title"></a>

### Title


```csharp
public string Title { get; }
```


The title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L164)
