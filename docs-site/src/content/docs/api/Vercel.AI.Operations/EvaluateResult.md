---
title: "EvaluateResult"
description: "Deprecated alias of DecisionResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluateResult"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluateResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionResult instead.")]
public sealed class EvaluateResult : DecisionResult
```


Deprecated alias of [DecisionResult](/api/Vercel.AI.Operations/DecisionResult/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionResult](/api/Vercel.AI.Operations/DecisionResult/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L176)

## Constructors

<a id="Vercel_AI_Operations_EvaluateResult__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluateResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_DecisionRounding_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `EvaluateResult(IReadOnlyDictionary<string, DecisionAnswer>, OperationUsage, IReadOnlyList<OperationWarning>, DecisionRounding?, JsonElement?, ProviderResponse)`


```csharp
public EvaluateResult(IReadOnlyDictionary<string, DecisionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, DecisionRounding? rounding, JsonElement? providerMetadata, ProviderResponse response)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L180)
