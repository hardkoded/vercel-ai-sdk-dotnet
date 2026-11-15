---
title: "ExtractionModelCall"
description: "Arguments for doExtract. Maps to ExtractionModelV4CallOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionModelCall
```


Arguments for `doExtract`. Maps to `ExtractionModelV4CallOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L194)

## Constructors

<a id="Vercel_AI_Operations_ExtractionModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionField__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_System_String_System_Nullable_System_Double__System_String_"></a>

### `ExtractionModelCall(IReadOnlyList<DecisionModelStatePart>, IReadOnlyDictionary<string, ExtractionField>, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken, string?, double?, string?)`


```csharp
public ExtractionModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, ExtractionField> fields, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, string? referenceDate = null, double? maxOutputTokens = null, string? reasoning = null)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `fields` | [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `referenceDate` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxOutputTokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L197)

## Properties

<a id="Vercel_AI_Operations_ExtractionModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L225)

<a id="Vercel_AI_Operations_ExtractionModelCall_Fields_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_Fields"></a>

### Fields


```csharp
public IReadOnlyDictionary<string, ExtractionField> Fields { get; }
```


Fields to extract.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L213)

<a id="Vercel_AI_Operations_ExtractionModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L222)

<a id="Vercel_AI_Operations_ExtractionModelCall_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; }
```


Maximum output tokens, an integer of at least 1. Null when the caller set none.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L228)

<a id="Vercel_AI_Operations_ExtractionModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L219)

<a id="Vercel_AI_Operations_ExtractionModelCall_Reasoning_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning effort. Language-model adapters default to `none` when it is null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L231)

<a id="Vercel_AI_Operations_ExtractionModelCall_ReferenceDate_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_ReferenceDate"></a>

### ReferenceDate


```csharp
public string? ReferenceDate { get; }
```


`YYYY-MM-DD` date that anchors relative dates and dates without a year.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L216)

<a id="Vercel_AI_Operations_ExtractionModelCall_State_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCall_State"></a>

### State


```csharp
public IReadOnlyList<DecisionModelStatePart> State { get; }
```


Ordered text, file, and JSON parts. Answer sources index into this list.

**Value:** [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L210)
