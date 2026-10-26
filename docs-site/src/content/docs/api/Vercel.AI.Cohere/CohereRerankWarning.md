---
title: "CohereRerankWarning"
description: "A compatibility warning from a Cohere rerank call."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereRerankWarning"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereRerankWarning"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereRerankWarning
```


A compatibility warning from a Cohere rerank call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L299)

## Constructors

<a id="Vercel_AI_Cohere_CohereRerankWarning__ctor_"></a>
<a id="Vercel_AI_Cohere_CohereRerankWarning__ctor_System_String_System_String_System_String_"></a>

### CohereRerankWarning(string, string, string)


```csharp
public CohereRerankWarning(string type, string feature, string details)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L302)

## Properties

<a id="Vercel_AI_Cohere_CohereRerankWarning_Details_"></a>
<a id="Vercel_AI_Cohere_CohereRerankWarning_Details"></a>

### Details


```csharp
public string Details { get; }
```


Explanation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L316)

<a id="Vercel_AI_Cohere_CohereRerankWarning_Feature_"></a>
<a id="Vercel_AI_Cohere_CohereRerankWarning_Feature"></a>

### Feature


```csharp
public string Feature { get; }
```


Feature name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L313)

<a id="Vercel_AI_Cohere_CohereRerankWarning_Type_"></a>
<a id="Vercel_AI_Cohere_CohereRerankWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L310)
