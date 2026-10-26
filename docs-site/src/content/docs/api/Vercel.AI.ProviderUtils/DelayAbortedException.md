---
title: "DelayAbortedException"
description: "Raised when Delay is cancelled. Name is AbortError."
editUrl: false
prev: false
next: false
sidebar:
  label: "DelayAbortedException"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_DelayAbortedException"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class DelayAbortedException : OperationCanceledException, ISerializable
```


Raised when [Delay](/api/Vercel.AI.ProviderUtils/Delay/) is cancelled. [Name](#Vercel_AI_ProviderUtils_DelayAbortedException_Name) is `AbortError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [SystemException](https://learn.microsoft.com/dotnet/api/system.systemexception) → [OperationCanceledException](https://learn.microsoft.com/dotnet/api/system.operationcanceledexception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L8)

## Constructors

<a id="Vercel_AI_ProviderUtils_DelayAbortedException__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_DelayAbortedException__ctor"></a>

### DelayAbortedException()


```csharp
public DelayAbortedException()
```


Creates the exception.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L11)

## Properties

<a id="Vercel_AI_ProviderUtils_DelayAbortedException_Name_"></a>
<a id="Vercel_AI_ProviderUtils_DelayAbortedException_Name"></a>

### Name


```csharp
public string Name { get; }
```


Abort error name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L17)
