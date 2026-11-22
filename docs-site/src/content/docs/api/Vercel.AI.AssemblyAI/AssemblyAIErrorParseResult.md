---
title: "AssemblyAIErrorParseResult"
description: "Result of parsing an AssemblyAI error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIErrorParseResult"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAIErrorParseResult
```


Result of parsing an AssemblyAI error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L55)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult__ctor_System_Boolean_Vercel_AI_AssemblyAI_AssemblyAIErrorData_"></a>

### AssemblyAIErrorParseResult(bool, AssemblyAIErrorData?)


```csharp
public AssemblyAIErrorParseResult(bool success, AssemblyAIErrorData? value)
```


Creates a parse result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `value` | [AssemblyAIErrorData](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorData/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L58)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_RawValue_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_RawValue"></a>

### RawValue


```csharp
public AssemblyAIErrorData? RawValue { get; }
```


Same payload as [Value](#Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_Value).

**Value:** [AssemblyAIErrorData](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L72)

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_Success_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when the body had a flat `error` string, or a nested `error.message` and `error.code`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L66)

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_Value_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorParseResult_Value"></a>

### Value


```csharp
public AssemblyAIErrorData? Value { get; }
```


Parsed error. Keys other than `error`, such as `status`, are dropped.

**Value:** [AssemblyAIErrorData](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L69)
