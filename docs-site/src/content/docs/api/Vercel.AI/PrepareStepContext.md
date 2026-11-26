---
title: "PrepareStepContext"
description: "Context passed to prepareStep."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareStepContext"
  hidden: true
---

<a id="Vercel_AI_PrepareStepContext"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class PrepareStepContext
```


Context passed to `prepareStep`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1306)

## Constructors

<a id="Vercel_AI_PrepareStepContext__ctor_"></a>
<a id="Vercel_AI_PrepareStepContext__ctor_System_Int32_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__"></a>

### `PrepareStepContext(int, IReadOnlyList<StepResult>)`


```csharp
public PrepareStepContext(int stepNumber, IReadOnlyList<StepResult> steps)
```


Creates context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stepNumber` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1309)

<a id="Vercel_AI_PrepareStepContext__ctor_System_Int32_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__Vercel_AI_Provider_ILanguageModel_"></a>

### `PrepareStepContext(int, IReadOnlyList<StepResult>, IReadOnlyList<ModelMessage>, ILanguageModel)`


```csharp
public PrepareStepContext(int stepNumber, IReadOnlyList<StepResult> steps, IReadOnlyList<ModelMessage> messages, ILanguageModel model)
```


Creates context with the prompt and model the step is about to use.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stepNumber` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1316)

<a id="Vercel_AI_PrepareStepContext__ctor_System_Int32_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__Vercel_AI_Provider_ILanguageModel_Vercel_AI_Prompt_Instructions_Vercel_AI_Prompt_Instructions_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `PrepareStepContext(int, IReadOnlyList<StepResult>, IReadOnlyList<ModelMessage>, ILanguageModel, Instructions?, Instructions?, IReadOnlyList<ModelMessage>, IReadOnlyList<ModelMessage>)`


```csharp
public PrepareStepContext(int stepNumber, IReadOnlyList<StepResult> steps, IReadOnlyList<ModelMessage> messages, ILanguageModel model, Instructions? instructions, Instructions? initialInstructions, IReadOnlyList<ModelMessage> initialMessages, IReadOnlyList<ModelMessage> responseMessages)
```


Creates context with the instructions and the initial and response messages of the call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stepNumber` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |
| `instructions` | [Instructions](/api/Vercel.AI.Prompt/Instructions/) |  |
| `initialInstructions` | [Instructions](/api/Vercel.AI.Prompt/Instructions/) |  |
| `initialMessages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responseMessages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1329)

## Properties

<a id="Vercel_AI_PrepareStepContext_ExperimentalSandbox_"></a>
<a id="Vercel_AI_PrepareStepContext_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; init; }
```


The sandbox environment that the step is operating in. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1380)

<a id="Vercel_AI_PrepareStepContext_ExperimentalSteeringMessages_"></a>
<a id="Vercel_AI_PrepareStepContext_ExperimentalSteeringMessages"></a>

### ExperimentalSteeringMessages


```csharp
public IReadOnlyList<UserModelMessage>? ExperimentalSteeringMessages { get; init; }
```


Experimental. The user messages that were sent with [ExperimentalSteering](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalSteering) and added to [Messages](#Vercel_AI_PrepareStepContext_Messages) for the current step.
Null when the step received no steering messages. [Messages](#Vercel_AI_PrepareStepContext_Messages) already contains them. If you return a [Messages](/api/Vercel.AI/PrepareStepUpdate/#Vercel_AI_PrepareStepUpdate_Messages)
override that is rebuilt from other fields, e.g. [InitialMessages](#Vercel_AI_PrepareStepContext_InitialMessages) and [ResponseMessages](#Vercel_AI_PrepareStepContext_ResponseMessages), add them back together with the
steering messages of previous steps from [Steps](#Vercel_AI_PrepareStepContext_Steps). Maps to `experimental_steeringMessages`.

**Value:** [IReadOnlyList&lt;UserModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1377)

<a id="Vercel_AI_PrepareStepContext_InitialInstructions_"></a>
<a id="Vercel_AI_PrepareStepContext_InitialInstructions"></a>

### InitialInstructions


```csharp
public Instructions? InitialInstructions { get; }
```


The instructions the call started with. Null when there are none.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1357)

<a id="Vercel_AI_PrepareStepContext_InitialMessages_"></a>
<a id="Vercel_AI_PrepareStepContext_InitialMessages"></a>

### InitialMessages


```csharp
public IReadOnlyList<ModelMessage> InitialMessages { get; }
```


The messages the call started with, without the instructions. Empty when the caller built the context without them.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1360)

<a id="Vercel_AI_PrepareStepContext_Instructions_"></a>
<a id="Vercel_AI_PrepareStepContext_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; }
```


The instructions the step is about to send. A [Instructions](/api/Vercel.AI/PrepareStepUpdate/#Vercel_AI_PrepareStepUpdate_Instructions) override carries forward to later steps. Null when there are none.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1354)

<a id="Vercel_AI_PrepareStepContext_Messages_"></a>
<a id="Vercel_AI_PrepareStepContext_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


The messages the step is about to send, without the instructions: the prompt plus the responses of the earlier steps. A [Messages](/api/Vercel.AI/PrepareStepUpdate/#Vercel_AI_PrepareStepUpdate_Messages) override carries forward to later steps. Empty when the caller built the context without them.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1351)

<a id="Vercel_AI_PrepareStepContext_Model_"></a>
<a id="Vercel_AI_PrepareStepContext_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; }
```


The model the step is about to use, or null when the caller built the context without it.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1366)

<a id="Vercel_AI_PrepareStepContext_ResponseMessages_"></a>
<a id="Vercel_AI_PrepareStepContext_ResponseMessages"></a>

### ResponseMessages


```csharp
public IReadOnlyList<ModelMessage> ResponseMessages { get; }
```


The response messages the earlier steps added. Empty when the caller built the context without them.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1363)

<a id="Vercel_AI_PrepareStepContext_RuntimeContext_"></a>
<a id="Vercel_AI_PrepareStepContext_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; init; }
```


The user-defined runtime context. See [RuntimeContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RuntimeContext). Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1387)

<a id="Vercel_AI_PrepareStepContext_StepNumber_"></a>
<a id="Vercel_AI_PrepareStepContext_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


Zero-based step index about to run.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1348)

<a id="Vercel_AI_PrepareStepContext_Steps_"></a>
<a id="Vercel_AI_PrepareStepContext_Steps"></a>

### Steps


```csharp
public IReadOnlyList<StepResult> Steps { get; }
```


Steps already completed.

**Value:** [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1369)

<a id="Vercel_AI_PrepareStepContext_ToolsContext_"></a>
<a id="Vercel_AI_PrepareStepContext_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement> ToolsContext { get; init; }
```


The context of each tool, by tool name. See [ToolsContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolsContext). Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1383)
