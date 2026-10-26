---
title: "RevAIErrorParseResult"
description: "Result of parsing a Rev.ai error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAIErrorParseResult"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAIErrorParseResult"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAIErrorParseResult
```


Result of parsing a Rev.ai error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L44)

## Constructors

<a id="Vercel_AI_RevAI_RevAIErrorParseResult__ctor_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorParseResult__ctor_System_Boolean_Vercel_AI_RevAI_RevAIErrorData_"></a>

### RevAIErrorParseResult(bool, RevAIErrorData?)


```csharp
public RevAIErrorParseResult(bool success, RevAIErrorData? value)
```


Creates a parse result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `value` | [RevAIErrorData](/api/Vercel.AI.RevAI/RevAIErrorData/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L47)

## Properties

<a id="Vercel_AI_RevAI_RevAIErrorParseResult_RawValue_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorParseResult_RawValue"></a>

### RawValue


```csharp
public RevAIErrorData? RawValue { get; }
```


Same payload as [Value](#Vercel_AI_RevAI_RevAIErrorParseResult_Value).

**Value:** [RevAIErrorData](/api/Vercel.AI.RevAI/RevAIErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L61)

<a id="Vercel_AI_RevAI_RevAIErrorParseResult_Success_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when `error.message` and `error.code` were present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L55)

<a id="Vercel_AI_RevAI_RevAIErrorParseResult_Value_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorParseResult_Value"></a>

### Value


```csharp
public RevAIErrorData? Value { get; }
```


Parsed error.

**Value:** [RevAIErrorData](/api/Vercel.AI.RevAI/RevAIErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L58)
