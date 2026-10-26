---
title: "RawStreamPart"
description: "One raw provider stream event."
editUrl: false
prev: false
next: false
sidebar:
  label: "RawStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_RawStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RawStreamPart : LanguageModelStreamPart
```


One raw provider stream event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2112)

## Constructors

<a id="Vercel_AI_Provider_RawStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_RawStreamPart__ctor_System_String_"></a>

### RawStreamPart(string)


```csharp
public RawStreamPart(string rawJson)
```


Creates a raw part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `rawJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2115)

## Properties

<a id="Vercel_AI_Provider_RawStreamPart_RawJson_"></a>
<a id="Vercel_AI_Provider_RawStreamPart_RawJson"></a>

### RawJson


```csharp
public string RawJson { get; }
```


Raw event JSON.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2122)
