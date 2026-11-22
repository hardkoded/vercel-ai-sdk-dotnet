---
title: "DocumentSourcePart"
description: "A cited document. Maps to the source stream part of source type document."
editUrl: false
prev: false
next: false
sidebar:
  label: "DocumentSourcePart"
  hidden: true
---

<a id="Vercel_AI_DocumentSourcePart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DocumentSourcePart : TextStreamPart
```


A cited document. Maps to the `source` stream part of source type `document`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L3004)

## Constructors

<a id="Vercel_AI_DocumentSourcePart__ctor_"></a>
<a id="Vercel_AI_DocumentSourcePart__ctor_Vercel_AI_Provider_GeneratedDocumentSource_"></a>

### DocumentSourcePart(GeneratedDocumentSource)


```csharp
public DocumentSourcePart(GeneratedDocumentSource source)
```


Creates a document source part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [GeneratedDocumentSource](/api/Vercel.AI.Provider/GeneratedDocumentSource/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L3007)

## Properties

<a id="Vercel_AI_DocumentSourcePart_Source_"></a>
<a id="Vercel_AI_DocumentSourcePart_Source"></a>

### Source


```csharp
public GeneratedDocumentSource Source { get; }
```


Source.

**Value:** [GeneratedDocumentSource](/api/Vercel.AI.Provider/GeneratedDocumentSource/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L3014)
