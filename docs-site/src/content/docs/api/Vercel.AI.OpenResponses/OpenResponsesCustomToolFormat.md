---
title: "OpenResponsesCustomToolFormat"
description: "The raw text format of a custom tool. Maps to the format member of OpenResponsesCustomToolOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesCustomToolFormat"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesCustomToolFormat
```


The raw text format of a custom tool. Maps to the `format` member of `OpenResponsesCustomToolOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L14)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat_Definition_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat_Definition"></a>

### Definition


```csharp
public string? Definition { get; set; }
```


Grammar definition. Used with the `grammar` type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L23)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat_Syntax_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat_Syntax"></a>

### Syntax


```csharp
public string? Syntax { get; set; }
```


Grammar syntax: `regex` or `lark`. Used with the `grammar` type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L20)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat_Type_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolFormat_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


The format type: `grammar` or `text`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L17)
