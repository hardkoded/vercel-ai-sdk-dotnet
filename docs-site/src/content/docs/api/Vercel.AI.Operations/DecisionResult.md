---
title: "DecisionResult"
description: "Decision result."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionResult"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionResult
```


Decision result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluateResult](/api/Vercel.AI.Operations/EvaluateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L422)

## Constructors

<a id="Vercel_AI_Operations_DecisionResult__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_DecisionRounding_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `DecisionResult(IReadOnlyDictionary<string, DecisionAnswer>, OperationUsage, IReadOnlyList<OperationWarning>, DecisionRounding?, JsonElement?, ProviderResponse)`


```csharp
public DecisionResult(IReadOnlyDictionary<string, DecisionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, DecisionRounding? rounding, JsonElement? providerMetadata, ProviderResponse response)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `answers` | [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `rounding` | [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L425)

## Properties

<a id="Vercel_AI_Operations_DecisionResult_Answers_"></a>
<a id="Vercel_AI_Operations_DecisionResult_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, DecisionAnswer> Answers { get; }
```


Answers, with provider numbers unchanged.

**Value:** [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L436)

<a id="Vercel_AI_Operations_DecisionResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_DecisionResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L448)

<a id="Vercel_AI_Operations_DecisionResult_Response_"></a>
<a id="Vercel_AI_Operations_DecisionResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response. Timestamp and model id are filled when the provider omitted them.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L451)

<a id="Vercel_AI_Operations_DecisionResult_Rounding_"></a>
<a id="Vercel_AI_Operations_DecisionResult_Rounding"></a>

### Rounding


```csharp
public DecisionRounding? Rounding { get; }
```


Rounding declaration.

**Value:** [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L445)

<a id="Vercel_AI_Operations_DecisionResult_Usage_"></a>
<a id="Vercel_AI_Operations_DecisionResult_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Usage. [TotalTokens](/api/Vercel.AI.Operations/OperationUsage/#Vercel_AI_Operations_OperationUsage_TotalTokens) is input plus output when both are set.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L439)

<a id="Vercel_AI_Operations_DecisionResult_Warnings_"></a>
<a id="Vercel_AI_Operations_DecisionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L442)
