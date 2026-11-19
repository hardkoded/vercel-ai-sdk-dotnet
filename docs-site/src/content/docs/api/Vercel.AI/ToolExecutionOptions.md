---
title: "ToolExecutionOptions"
description: "The options a tool receives when it runs. Maps to the second argument of execute."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecutionOptions"
  hidden: true
---

<a id="Vercel_AI_ToolExecutionOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolExecutionOptions
```


The options a tool receives when it runs. Maps to the second argument of `execute`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L205)

## Constructors

<a id="Vercel_AI_ToolExecutionOptions__ctor_"></a>
<a id="Vercel_AI_ToolExecutionOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Threading_CancellationToken_"></a>

### `ToolExecutionOptions(string, IReadOnlyList<ModelMessage>, CancellationToken)`


```csharp
public ToolExecutionOptions(string toolCallId, IReadOnlyList<ModelMessage> messages, CancellationToken abortSignal)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L208)

## Properties

<a id="Vercel_AI_ToolExecutionOptions_AbortSignal_"></a>
<a id="Vercel_AI_ToolExecutionOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; }
```


Cancellation signal of the call.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L226)

<a id="Vercel_AI_ToolExecutionOptions_Context_"></a>
<a id="Vercel_AI_ToolExecutionOptions_Context"></a>

### Context


```csharp
public JsonElement? Context { get; init; }
```


The context of this tool from [ToolsContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolsContext), validated with [ContextSchema](/api/Vercel.AI/Tool/#Vercel_AI_Tool_ContextSchema).
Maps to `context`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L235)

<a id="Vercel_AI_ToolExecutionOptions_ExperimentalSandbox_"></a>
<a id="Vercel_AI_ToolExecutionOptions_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; init; }
```


The sandbox environment that the tool is operating in. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L229)

<a id="Vercel_AI_ToolExecutionOptions_Messages_"></a>
<a id="Vercel_AI_ToolExecutionOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


Messages sent to the model in the step that made the call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L223)

<a id="Vercel_AI_ToolExecutionOptions_ToolCallId_"></a>
<a id="Vercel_AI_ToolExecutionOptions_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L220)
