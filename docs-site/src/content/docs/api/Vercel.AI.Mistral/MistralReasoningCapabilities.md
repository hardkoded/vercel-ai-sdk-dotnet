---
title: "MistralReasoningCapabilities"
description: "Reasoning support of a model. Maps to the catalog's ReasoningCapabilities."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralReasoningCapabilities"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralReasoningCapabilities"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralReasoningCapabilities
```


Reasoning support of a model. Maps to the catalog's `ReasoningCapabilities`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L63)

## Constructors

<a id="Vercel_AI_Mistral_MistralReasoningCapabilities__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralReasoningCapabilities__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `MistralReasoningCapabilities(IReadOnlyList<string>)`


```csharp
public MistralReasoningCapabilities(IReadOnlyList<string> efforts)
```


Creates reasoning capabilities. [Support](#Vercel_AI_Mistral_MistralReasoningCapabilities_Support) is true when any effort is accepted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `efforts` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L66)

## Properties

<a id="Vercel_AI_Mistral_MistralReasoningCapabilities_Efforts_"></a>
<a id="Vercel_AI_Mistral_MistralReasoningCapabilities_Efforts"></a>

### Efforts


```csharp
public IReadOnlyList<string> Efforts { get; }
```


The accepted reasoning efforts.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L76)

<a id="Vercel_AI_Mistral_MistralReasoningCapabilities_Support_"></a>
<a id="Vercel_AI_Mistral_MistralReasoningCapabilities_Support"></a>

### Support


```csharp
public bool Support { get; }
```


Whether the model accepts reasoning configuration.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L73)
