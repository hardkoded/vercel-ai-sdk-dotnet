---
title: "EvaluateEvent"
description: "Deprecated alias of DecideEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluateEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluateEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecideEvent instead.")]
public sealed class EvaluateEvent : DecideEvent
```


Deprecated alias of [DecideEvent](/api/Vercel.AI.Operations/DecideEvent/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecideEvent](/api/Vercel.AI.Operations/DecideEvent/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L138)

## Constructors

<a id="Vercel_AI_Operations_EvaluateEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluateEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_Vercel_AI_Operations_DecisionState_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_System_Nullable_System_Double__System_String_"></a>

### `EvaluateEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, DecisionState?, IReadOnlyDictionary<string, DecisionQuestion>, int, IReadOnlyDictionary<string, string>?, JsonElement, IReadOnlyDictionary<string, DecisionAnswer>?, OperationUsage?, IReadOnlyList<OperationWarning>?, bool?, bool?, string?, double?, string?)`


```csharp
public EvaluateEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, DecisionState? state, IReadOnlyDictionary<string, DecisionQuestion> questions, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, IReadOnlyList<OperationWarning>? warnings = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null, double? maxOutputTokens = null, string? reasoning = null)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `state` | [DecisionState](/api/Vercel.AI.Operations/DecisionState/) |  |
| `questions` | [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `answers` | [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxOutputTokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L142)
