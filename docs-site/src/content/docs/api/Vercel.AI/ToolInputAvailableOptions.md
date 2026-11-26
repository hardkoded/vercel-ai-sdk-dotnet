---
title: "ToolInputAvailableOptions"
description: "The options of OnInputAvailable. Maps to { input } & ToolExecutionOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputAvailableOptions"
  hidden: true
---

<a id="Vercel_AI_ToolInputAvailableOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolInputAvailableOptions : ToolExecutionOptions
```


The options of [OnInputAvailable](/api/Vercel.AI/Tool/#Vercel_AI_Tool_OnInputAvailable). Maps to `{ input } & ToolExecutionOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolExecutionOptions](/api/Vercel.AI/ToolExecutionOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L292)

## Constructors

<a id="Vercel_AI_ToolInputAvailableOptions__ctor_"></a>
<a id="Vercel_AI_ToolInputAvailableOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Threading_CancellationToken_System_Text_Json_JsonElement_"></a>

### `ToolInputAvailableOptions(string, IReadOnlyList<ModelMessage>, CancellationToken, JsonElement)`


```csharp
public ToolInputAvailableOptions(string toolCallId, IReadOnlyList<ModelMessage> messages, CancellationToken abortSignal, JsonElement input)
```


The options of [OnInputAvailable](/api/Vercel.AI/Tool/#Vercel_AI_Tool_OnInputAvailable). Maps to `{ input } & ToolExecutionOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L292)

## Properties

<a id="Vercel_AI_ToolInputAvailableOptions_Input_"></a>
<a id="Vercel_AI_ToolInputAvailableOptions_Input"></a>

### Input


```csharp
public JsonElement Input { get; }
```


The input of the tool call.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L300)
