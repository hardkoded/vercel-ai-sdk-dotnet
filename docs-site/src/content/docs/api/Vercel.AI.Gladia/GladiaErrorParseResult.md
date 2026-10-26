---
title: "GladiaErrorParseResult"
description: "Result of parsing a Gladia error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaErrorParseResult"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaErrorParseResult"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaErrorParseResult
```


Result of parsing a Gladia error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L43)

## Constructors

<a id="Vercel_AI_Gladia_GladiaErrorParseResult__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorParseResult__ctor_System_Boolean_Vercel_AI_Gladia_GladiaErrorData_"></a>

### GladiaErrorParseResult(bool, GladiaErrorData?)


```csharp
public GladiaErrorParseResult(bool success, GladiaErrorData? value)
```


Creates a parse result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `value` | [GladiaErrorData](/api/Vercel.AI.Gladia/GladiaErrorData/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L46)

## Properties

<a id="Vercel_AI_Gladia_GladiaErrorParseResult_RawValue_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorParseResult_RawValue"></a>

### RawValue


```csharp
public GladiaErrorData? RawValue { get; }
```


Same payload as [Value](#Vercel_AI_Gladia_GladiaErrorParseResult_Value).

**Value:** [GladiaErrorData](/api/Vercel.AI.Gladia/GladiaErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L60)

<a id="Vercel_AI_Gladia_GladiaErrorParseResult_Success_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when the error object was present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L54)

<a id="Vercel_AI_Gladia_GladiaErrorParseResult_Value_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorParseResult_Value"></a>

### Value


```csharp
public GladiaErrorData? Value { get; }
```


Parsed error.

**Value:** [GladiaErrorData](/api/Vercel.AI.Gladia/GladiaErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L57)
