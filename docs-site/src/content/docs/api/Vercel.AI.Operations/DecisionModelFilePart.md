---
title: "DecisionModelFilePart"
description: "A file part the model receives. Maps to LanguageModelV4FilePart."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModelFilePart"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModelFilePart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionModelFilePart : DecisionModelStatePart
```


A file part the model receives. Maps to `LanguageModelV4FilePart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionModelStatePart](/api/Vercel.AI.Operations/DecisionModelStatePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L109)

## Constructors

<a id="Vercel_AI_Operations_DecisionModelFilePart__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionModelFilePart__ctor_System_String_Vercel_AI_Prompt_LanguageModelFilePart_"></a>

### DecisionModelFilePart(string, LanguageModelFilePart)


```csharp
public DecisionModelFilePart(string mediaType, LanguageModelFilePart data)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [LanguageModelFilePart](/api/Vercel.AI.Prompt/LanguageModelFilePart/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L112)

## Properties

<a id="Vercel_AI_Operations_DecisionModelFilePart_Data_"></a>
<a id="Vercel_AI_Operations_DecisionModelFilePart_Data"></a>

### Data


```csharp
public LanguageModelFilePart Data { get; }
```


File data: inline bytes or base64, a URL, a provider reference, or text.

**Value:** [LanguageModelFilePart](/api/Vercel.AI.Prompt/LanguageModelFilePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L123)

<a id="Vercel_AI_Operations_DecisionModelFilePart_FileName_"></a>
<a id="Vercel_AI_Operations_DecisionModelFilePart_FileName"></a>

### FileName


```csharp
public string? FileName { get; init; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L126)

<a id="Vercel_AI_Operations_DecisionModelFilePart_MediaType_"></a>
<a id="Vercel_AI_Operations_DecisionModelFilePart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type. It is a full type after `CancellationToken)`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L120)

<a id="Vercel_AI_Operations_DecisionModelFilePart_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_DecisionModelFilePart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L129)
