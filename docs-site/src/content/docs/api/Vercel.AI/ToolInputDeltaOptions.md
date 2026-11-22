---
title: "ToolInputDeltaOptions"
description: "The options of OnInputDelta. Maps to { inputTextDelta } & ToolExecutionOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputDeltaOptions"
  hidden: true
---

<a id="Vercel_AI_ToolInputDeltaOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolInputDeltaOptions : ToolExecutionOptions
```


The options of [OnInputDelta](/api/Vercel.AI/Tool/#Vercel_AI_Tool_OnInputDelta). Maps to `{ inputTextDelta } & ToolExecutionOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolExecutionOptions](/api/Vercel.AI/ToolExecutionOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L263)

## Constructors

<a id="Vercel_AI_ToolInputDeltaOptions__ctor_"></a>
<a id="Vercel_AI_ToolInputDeltaOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Threading_CancellationToken_System_String_"></a>

### `ToolInputDeltaOptions(string, IReadOnlyList<ModelMessage>, CancellationToken, string)`


```csharp
public ToolInputDeltaOptions(string toolCallId, IReadOnlyList<ModelMessage> messages, CancellationToken abortSignal, string inputTextDelta)
```


The options of [OnInputDelta](/api/Vercel.AI/Tool/#Vercel_AI_Tool_OnInputDelta). Maps to `{ inputTextDelta } & ToolExecutionOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `inputTextDelta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L263)

## Properties

<a id="Vercel_AI_ToolInputDeltaOptions_InputTextDelta_"></a>
<a id="Vercel_AI_ToolInputDeltaOptions_InputTextDelta"></a>

### InputTextDelta


```csharp
public string InputTextDelta { get; }
```


The fragment of the tool input text. Maps to `inputTextDelta`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L271)
