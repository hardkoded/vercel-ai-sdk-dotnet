---
title: "ExtractionLanguageModelOptions"
description: "The wrapped model, provider id, and field limit that cross a workflow boundary for ExtractionLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionLanguageModelOptions"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionLanguageModelOptions
```


The wrapped model, provider id, and field limit that cross a workflow boundary for [ExtractionLanguageModel](/api/Vercel.AI.Operations/ExtractionLanguageModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L18)

## Constructors

<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions__ctor_Vercel_AI_Provider_ILanguageModel_System_String_System_Double_"></a>

### ExtractionLanguageModelOptions(ILanguageModel, string, double)


```csharp
public ExtractionLanguageModelOptions(ILanguageModel model, string provider, double maxFieldsPerCall = Infinity)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxFieldsPerCall` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L21)

## Properties

<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions_MaxFieldsPerCall_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions_MaxFieldsPerCall"></a>

### MaxFieldsPerCall


```csharp
public double MaxFieldsPerCall { get; }
```


Most fields per language-model call. [PositiveInfinity](https://learn.microsoft.com/dotnet/api/system.double.positiveinfinity) means no limit.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L39)

<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions_Model_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions_Model"></a>

### Model


```csharp
public ILanguageModel Model { get; }
```


The wrapped language model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L33)

<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions_Provider_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModelOptions_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The extraction provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L36)
