---
title: "DecisionModelResult"
description: "Model response for doDecide."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionModelResult
```


Model response for `doDecide`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluationModelResult](/api/Vercel.AI.Operations/EvaluationModelResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L126)

## Constructors

<a id="Vercel_AI_Operations_DecisionModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_OperationUsage_Vercel_AI_Operations_DecisionRounding_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `DecisionModelResult(IReadOnlyDictionary<string, DecisionAnswer>, IReadOnlyList<OperationWarning>?, OperationUsage?, DecisionRounding?, JsonElement?, ProviderResponse?)`


```csharp
public DecisionModelResult(IReadOnlyDictionary<string, DecisionAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, DecisionRounding? rounding = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a model result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `answers` | [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `rounding` | [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L129)

## Properties

<a id="Vercel_AI_Operations_DecisionModelResult_Answers_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, DecisionAnswer> Answers { get; }
```


Answers keyed by question id.

**Value:** [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L140)

<a id="Vercel_AI_Operations_DecisionModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L152)

<a id="Vercel_AI_Operations_DecisionModelResult_Response_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Response metadata. Timestamp and model id are filled when omitted.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L155)

<a id="Vercel_AI_Operations_DecisionModelResult_Rounding_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult_Rounding"></a>

### Rounding


```csharp
public DecisionRounding? Rounding { get; }
```


Rounding declaration.

**Value:** [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L149)

<a id="Vercel_AI_Operations_DecisionModelResult_Usage_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Token usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L146)

<a id="Vercel_AI_Operations_DecisionModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_DecisionModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L143)
