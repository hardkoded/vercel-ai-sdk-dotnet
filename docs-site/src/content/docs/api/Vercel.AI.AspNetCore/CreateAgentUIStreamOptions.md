---
title: "CreateAgentUIStreamOptions"
description: "Options of CancellationToken). Maps to the options of createAgentUIStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "CreateAgentUIStreamOptions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public class CreateAgentUIStreamOptions
```


Options of `CancellationToken)`. Maps to the options of `createAgentUIStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [CreateAgentUIStreamResponseOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamResponseOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L18)

## Properties

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_AbortSignal_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_AbortSignal"></a>

### AbortSignal


```csharp
public AbortSignal? AbortSignal { get; set; }
```


Aborts the stream. Maps to `abortSignal`.

**Value:** [AbortSignal](/api/Vercel.AI.Util/AbortSignal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L33)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_Agent_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_Agent"></a>

### Agent


```csharp
public IAgent? Agent { get; set; }
```


The agent to run. Maps to `agent`.

**Value:** [IAgent](/api/Vercel.AI/IAgent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L21)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_ConvertDataPart_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_ConvertDataPart"></a>

### ConvertDataPart


```csharp
public Func<DataUIPart, UserContentPart?>? ConvertDataPart { get; set; }
```


Converts custom UI data parts to text or file model message parts. Maps to `convertDataPart`.

**Value:** [Func&lt;DataUIPart, UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L30)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_ExperimentalSandbox_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; set; }
```


The sandbox environment that is passed through to tool execution. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L45)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_ExperimentalSteering_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_ExperimentalSteering"></a>

### ExperimentalSteering


```csharp
public ISteering? ExperimentalSteering { get; set; }
```


Adds user messages to the run at the next step boundary. Only [Agent](/api/Vercel.AI/Agent/) supports it, any other
[IAgent](/api/Vercel.AI/IAgent/) makes the call throw [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/). Maps to `experimental_steering`.

**Value:** [ISteering](/api/Vercel.AI.GenerateText/ISteering/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L42)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnStepEnd_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called when each generation step ends. Maps to `onStepEnd`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L48)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnStepFinish_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnStepFinish"></a>

### OnStepFinish


```csharp
[Obsolete("Use OnStepEnd instead.")]
public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }
```


Deprecated alias for [OnStepEnd](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnStepEnd). Maps to `onStepFinish`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L51)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnUIMessageStepEnd_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnUIMessageStepEnd"></a>

### OnUIMessageStepEnd


```csharp
public Func<UIMessageStreamOnStepEndEvent, Task>? OnUIMessageStepEnd { get; set; }
```


Called with the accumulated UI message after each step. Maps to `onUIMessageStepEnd`.

**Value:** [Func&lt;UIMessageStreamOnStepEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L55)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_Timeout_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


The timeout of the call. Maps to `timeout`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L36)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessageStreamOptions_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessageStreamOptions"></a>

### UIMessageStreamOptions


```csharp
public UIMessageStreamOptions? UIMessageStreamOptions { get; set; }
```


The UI message stream options. The tools of the agent and [OnUIMessageStepEnd](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_OnUIMessageStepEnd) replace its
[Tools](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_Tools) and [OnStepEnd](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepEnd).
Its [OriginalMessages](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_OriginalMessages) default to the validated [UIMessages](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessages).

**Value:** [UIMessageStreamOptions](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L62)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessages_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessages"></a>

### UIMessages


```csharp
public IReadOnlyList<UIMessage>? UIMessages { get; set; }
```


The input UI messages. Maps to `uiMessages`. Set [UIMessagesJson](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessagesJson) instead for messages that are not typed yet.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L24)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessagesJson_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessagesJson"></a>

### UIMessagesJson


```csharp
public JsonElement? UIMessagesJson { get; set; }
```


The input UI messages as a JSON array that the call validates, like the `unknown[]` of `uiMessages`. Set either this or [UIMessages](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_UIMessages).

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L27)
