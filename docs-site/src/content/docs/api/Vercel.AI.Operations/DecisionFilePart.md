---
title: "DecisionFilePart"
description: "A file part of decision state. A URL is downloaded before the model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionFilePart"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionFilePart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionFilePart : DecisionStatePart
```


A file part of decision state. A URL is downloaded before the model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionStatePart](/api/Vercel.AI.Operations/DecisionStatePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L40)

## Constructors

<a id="Vercel_AI_Operations_DecisionFilePart__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionFilePart__ctor_System_String_Vercel_AI_Prompt_FilePartInput_"></a>

### DecisionFilePart(string, FilePartInput)


```csharp
public DecisionFilePart(string mediaType, FilePartInput data)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [FilePartInput](/api/Vercel.AI.Prompt/FilePartInput/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L43)

## Properties

<a id="Vercel_AI_Operations_DecisionFilePart_Data_"></a>
<a id="Vercel_AI_Operations_DecisionFilePart_Data"></a>

### Data


```csharp
public FilePartInput Data { get; }
```


File data.

**Value:** [FilePartInput](/api/Vercel.AI.Prompt/FilePartInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L54)

<a id="Vercel_AI_Operations_DecisionFilePart_FileName_"></a>
<a id="Vercel_AI_Operations_DecisionFilePart_FileName"></a>

### FileName


```csharp
public string? FileName { get; init; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L57)

<a id="Vercel_AI_Operations_DecisionFilePart_MediaType_"></a>
<a id="Vercel_AI_Operations_DecisionFilePart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type. A top-level type such as `image` is resolved to a full type from the data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L51)

<a id="Vercel_AI_Operations_DecisionFilePart_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_DecisionFilePart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L60)
