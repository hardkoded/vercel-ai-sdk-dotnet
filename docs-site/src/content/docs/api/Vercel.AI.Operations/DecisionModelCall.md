---
title: "DecisionModelCall"
description: "Arguments for doDecide."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionModelCall
```


Arguments for `doDecide`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluationModelCall](/api/Vercel.AI.Operations/EvaluationModelCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L159)

## Constructors

<a id="Vercel_AI_Operations_DecisionModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_System_Nullable_System_Double__System_String_"></a>

### `DecisionModelCall(IReadOnlyList<DecisionModelStatePart>, IReadOnlyDictionary<string, DecisionQuestion>, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken, double?, string?)`


```csharp
public DecisionModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, DecisionQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, double? maxOutputTokens = null, string? reasoning = null)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `questions` | [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `maxOutputTokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L162)

## Properties

<a id="Vercel_AI_Operations_DecisionModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L186)

<a id="Vercel_AI_Operations_DecisionModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L183)

<a id="Vercel_AI_Operations_DecisionModelCall_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; }
```


Maximum output tokens, an integer of at least 1. Null when the caller set none.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L189)

<a id="Vercel_AI_Operations_DecisionModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L180)

<a id="Vercel_AI_Operations_DecisionModelCall_Questions_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_Questions"></a>

### Questions


```csharp
public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }
```


Questions.

**Value:** [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L177)

<a id="Vercel_AI_Operations_DecisionModelCall_Reasoning_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning effort: `provider-default`, `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, or `max`.
Language-model adapters default to `none` when it is null. Native decision providers report an unsupported-setting warning.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L195)

<a id="Vercel_AI_Operations_DecisionModelCall_State_"></a>
<a id="Vercel_AI_Operations_DecisionModelCall_State"></a>

### State


```csharp
public IReadOnlyList<DecisionModelStatePart> State { get; }
```


Shared state as an ordered list of text, JSON, and file parts. See `CancellationToken)`.

**Value:** [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L174)
