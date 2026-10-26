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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L541)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L544)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L551)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L559)

## Properties

<a id="Vercel_AI_PrepareStepContext_InitialInstructions_"></a>
<a id="Vercel_AI_PrepareStepContext_InitialInstructions"></a>

### InitialInstructions


```csharp
public Instructions? InitialInstructions { get; }
```


The instructions the call started with. Null when there are none.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L586)

<a id="Vercel_AI_PrepareStepContext_InitialMessages_"></a>
<a id="Vercel_AI_PrepareStepContext_InitialMessages"></a>

### InitialMessages


```csharp
public IReadOnlyList<ModelMessage> InitialMessages { get; }
```


The messages the call started with, without the instructions. Empty when the caller built the context without them.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L589)

<a id="Vercel_AI_PrepareStepContext_Instructions_"></a>
<a id="Vercel_AI_PrepareStepContext_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; }
```


The instructions the step is about to send. A [Instructions](/api/Vercel.AI/PrepareStepUpdate/#Vercel_AI_PrepareStepUpdate_Instructions) override carries forward to later steps. Null when there are none.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L583)

<a id="Vercel_AI_PrepareStepContext_Messages_"></a>
<a id="Vercel_AI_PrepareStepContext_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


The messages the step is about to send, without the instructions: the prompt plus the responses of the earlier steps. A [Messages](/api/Vercel.AI/PrepareStepUpdate/#Vercel_AI_PrepareStepUpdate_Messages) override carries forward to later steps. Empty when the caller built the context without them.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L580)

<a id="Vercel_AI_PrepareStepContext_Model_"></a>
<a id="Vercel_AI_PrepareStepContext_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; }
```


The model the step is about to use, or null when the caller built the context without it.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L595)

<a id="Vercel_AI_PrepareStepContext_ResponseMessages_"></a>
<a id="Vercel_AI_PrepareStepContext_ResponseMessages"></a>

### ResponseMessages


```csharp
public IReadOnlyList<ModelMessage> ResponseMessages { get; }
```


The response messages the earlier steps added. Empty when the caller built the context without them.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L592)

<a id="Vercel_AI_PrepareStepContext_StepNumber_"></a>
<a id="Vercel_AI_PrepareStepContext_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


Zero-based step index about to run.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L577)

<a id="Vercel_AI_PrepareStepContext_Steps_"></a>
<a id="Vercel_AI_PrepareStepContext_Steps"></a>

### Steps


```csharp
public IReadOnlyList<StepResult> Steps { get; }
```


Steps already completed.

**Value:** [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L598)
