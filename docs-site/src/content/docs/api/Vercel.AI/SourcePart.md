---
title: "SourcePart"
description: "A cited source."
editUrl: false
prev: false
next: false
sidebar:
  label: "SourcePart"
  hidden: true
---

<a id="Vercel_AI_SourcePart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class SourcePart : TextStreamPart
```


A cited source.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1316)

## Constructors

<a id="Vercel_AI_SourcePart__ctor_"></a>
<a id="Vercel_AI_SourcePart__ctor_Vercel_AI_Provider_GeneratedSource_"></a>

### SourcePart(GeneratedSource)


```csharp
public SourcePart(GeneratedSource source)
```


Creates a source part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1319)

## Properties

<a id="Vercel_AI_SourcePart_Source_"></a>
<a id="Vercel_AI_SourcePart_Source"></a>

### Source


```csharp
public GeneratedSource Source { get; }
```


Source.

**Value:** [GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1326)
