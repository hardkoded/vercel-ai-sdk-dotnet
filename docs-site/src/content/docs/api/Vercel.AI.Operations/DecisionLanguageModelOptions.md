---
title: "DecisionLanguageModelOptions"
description: "The wrapped model and provider id that cross a workflow boundary for DecisionLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionLanguageModelOptions"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionLanguageModelOptions"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionLanguageModelOptions
```


The wrapped model and provider id that cross a workflow boundary for [DecisionLanguageModel](/api/Vercel.AI.Operations/DecisionLanguageModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L17)

## Constructors

<a id="Vercel_AI_Operations_DecisionLanguageModelOptions__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModelOptions__ctor_Vercel_AI_Provider_ILanguageModel_System_String_"></a>

### DecisionLanguageModelOptions(ILanguageModel, string)


```csharp
public DecisionLanguageModelOptions(ILanguageModel model, string provider)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L20)

## Properties

<a id="Vercel_AI_Operations_DecisionLanguageModelOptions_Model_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModelOptions_Model"></a>

### Model


```csharp
public ILanguageModel Model { get; }
```


The wrapped language model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L27)

<a id="Vercel_AI_Operations_DecisionLanguageModelOptions_Provider_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModelOptions_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The decision provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L30)
