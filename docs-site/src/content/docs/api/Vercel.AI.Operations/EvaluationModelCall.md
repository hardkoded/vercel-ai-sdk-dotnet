---
title: "EvaluationModelCall"
description: "Deprecated alias of DecisionModelCall."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluationModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionModelCall instead.")]
public sealed class EvaluationModelCall : DecisionModelCall
```


Deprecated alias of [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L44)

## Constructors

<a id="Vercel_AI_Operations_EvaluationModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluationModelCall__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_EvaluationQuestion__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_System_Nullable_System_Double__System_String_"></a>

### `EvaluationModelCall(IReadOnlyList<DecisionModelStatePart>, IReadOnlyDictionary<string, EvaluationQuestion>, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken, double?, string?)`


```csharp
public EvaluationModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, EvaluationQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, double? maxOutputTokens = null, string? reasoning = null)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `questions` | [IReadOnlyDictionary&lt;string, EvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `maxOutputTokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L48)
