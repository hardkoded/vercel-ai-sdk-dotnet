---
title: "CreateAgentUIStreamOptionsBase"
description: "The options of createAgentUIStream that do not depend on the agent. The .NET split of the generic CALLOPTIONS parameter."
editUrl: false
prev: false
next: false
sidebar:
  label: "CreateAgentUIStreamOptionsBase"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public abstract class CreateAgentUIStreamOptionsBase
```


The options of `createAgentUIStream` that do not depend on the agent. The .NET split of the generic `CALL_OPTIONS` parameter.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [CreateAgentUIStreamOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptions/), [CreateAgentUIStreamOptions&lt;TCallOptions&gt;](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptions-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L18)

## Properties

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_AbortSignal_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_AbortSignal"></a>

### AbortSignal


```csharp
public AbortSignal? AbortSignal { get; set; }
```


Aborts the stream. Maps to `abortSignal`.

**Value:** [AbortSignal](/api/Vercel.AI.Util/AbortSignal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L32)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_ConvertDataPart_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_ConvertDataPart"></a>

### ConvertDataPart


```csharp
public Func<DataUIPart, UserContentPart?>? ConvertDataPart { get; set; }
```


Converts custom UI data parts to text or file model message parts. Maps to `convertDataPart`.

**Value:** [Func&lt;DataUIPart, UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L29)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_ExperimentalSandbox_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; set; }
```


The sandbox environment that is passed through to tool execution. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L44)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_ExperimentalSteering_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_ExperimentalSteering"></a>

### ExperimentalSteering


```csharp
public ISteering? ExperimentalSteering { get; set; }
```


Adds user messages to the run at the next step boundary. Only [Agent](/api/Vercel.AI/Agent/) supports it, any other
[IAgent](/api/Vercel.AI/IAgent/) makes the call throw [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/). Maps to `experimental_steering`.

**Value:** [ISteering](/api/Vercel.AI.GenerateText/ISteering/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L41)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnStepEnd_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called when each generation step ends. Maps to `onStepEnd`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L47)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnStepFinish_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnStepFinish"></a>

### OnStepFinish


```csharp
[Obsolete("Use OnStepEnd instead.")]
public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }
```


Deprecated alias for [OnStepEnd](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnStepEnd). Maps to `onStepFinish`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L50)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnUIMessageStepEnd_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnUIMessageStepEnd"></a>

### OnUIMessageStepEnd


```csharp
public Func<UIMessageStreamOnStepEndEvent, Task>? OnUIMessageStepEnd { get; set; }
```


Called with the accumulated UI message after each step. Maps to `onUIMessageStepEnd`.

**Value:** [Func&lt;UIMessageStreamOnStepEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L54)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_Timeout_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


The timeout of the call. Maps to `timeout`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L35)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessageStreamOptions_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessageStreamOptions"></a>

### UIMessageStreamOptions


```csharp
public UIMessageStreamOptions? UIMessageStreamOptions { get; set; }
```


The UI message stream options. The tools of the agent and [OnUIMessageStepEnd](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_OnUIMessageStepEnd) replace its
[Tools](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_Tools) and [OnStepEnd](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepEnd).
Its [OriginalMessages](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_OriginalMessages) default to the validated [UIMessages](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessages).

**Value:** [UIMessageStreamOptions](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L61)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessages_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessages"></a>

### UIMessages


```csharp
public IReadOnlyList<UIMessage>? UIMessages { get; set; }
```


The input UI messages. Maps to `uiMessages`. Set [UIMessagesJson](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessagesJson) instead for messages that are not typed yet.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L23)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessagesJson_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessagesJson"></a>

### UIMessagesJson


```csharp
public JsonElement? UIMessagesJson { get; set; }
```


The input UI messages as a JSON array that the call validates, like the `unknown[]` of `uiMessages`. Set either this or [UIMessages](#Vercel_AI_AspNetCore_CreateAgentUIStreamOptionsBase_UIMessages).

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L26)
