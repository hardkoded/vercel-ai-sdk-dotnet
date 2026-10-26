---
title: "EvaluationModelResult"
description: "Deprecated alias of DecisionModelResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluationModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionModelResult instead.")]
public sealed class EvaluationModelResult : DecisionModelResult
```


Deprecated alias of [DecisionModelResult](/api/Vercel.AI.Operations/DecisionModelResult/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionModelResult](/api/Vercel.AI.Operations/DecisionModelResult/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L65)

## Constructors

<a id="Vercel_AI_Operations_EvaluationModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluationModelResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_EvaluationAnswer__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_OperationUsage_Vercel_AI_Operations_EvaluationRounding_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `EvaluationModelResult(IReadOnlyDictionary<string, EvaluationAnswer>, IReadOnlyList<OperationWarning>?, OperationUsage?, EvaluationRounding?, JsonElement?, ProviderResponse?)`


```csharp
public EvaluationModelResult(IReadOnlyDictionary<string, EvaluationAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, EvaluationRounding? rounding = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a model result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `answers` | [IReadOnlyDictionary&lt;string, EvaluationAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `rounding` | [EvaluationRounding](/api/Vercel.AI.Operations/EvaluationRounding/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L69)
