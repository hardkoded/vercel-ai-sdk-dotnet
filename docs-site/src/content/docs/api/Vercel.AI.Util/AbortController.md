---
title: "AbortController"
description: "Controller for an AbortSignal."
editUrl: false
prev: false
next: false
sidebar:
  label: "AbortController"
  hidden: true
---

<a id="Vercel_AI_Util_AbortController"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class AbortController
```


Controller for an [AbortSignal](/api/Vercel.AI.Util/AbortSignal/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L138)

## Constructors

<a id="Vercel_AI_Util_AbortController__ctor_"></a>
<a id="Vercel_AI_Util_AbortController__ctor"></a>

### AbortController()


```csharp
public AbortController()
```


Creates a controller whose signal is not aborted.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L141)

## Properties

<a id="Vercel_AI_Util_AbortController_Signal_"></a>
<a id="Vercel_AI_Util_AbortController_Signal"></a>

### Signal


```csharp
public AbortSignal Signal { get; }
```


Signal aborted by [Abort(object)](#Vercel_AI_Util_AbortController_Abort_System_Object_).

**Value:** [AbortSignal](/api/Vercel.AI.Util/AbortSignal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L147)

## Methods

<a id="Vercel_AI_Util_AbortController_Abort_"></a>
<a id="Vercel_AI_Util_AbortController_Abort_System_Object_"></a>

### Abort(object?)


```csharp
public void Abort(object? reason = null)
```


Aborts [Signal](#Vercel_AI_Util_AbortController_Signal) with &lt;code class="paramref"&gt;reason&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reason` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L150)
