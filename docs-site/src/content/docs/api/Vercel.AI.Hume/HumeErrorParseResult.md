---
title: "HumeErrorParseResult"
description: "Result of parsing a Hume error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeErrorParseResult"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeErrorParseResult"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeErrorParseResult
```


Result of parsing a Hume error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L38)

## Constructors

<a id="Vercel_AI_Hume_HumeErrorParseResult__ctor_"></a>
<a id="Vercel_AI_Hume_HumeErrorParseResult__ctor_System_Boolean_Vercel_AI_Hume_HumeErrorData_"></a>

### HumeErrorParseResult(bool, HumeErrorData?)


```csharp
public HumeErrorParseResult(bool success, HumeErrorData? value)
```


Creates a parse result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `value` | [HumeErrorData](/api/Vercel.AI.Hume/HumeErrorData/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L41)

## Properties

<a id="Vercel_AI_Hume_HumeErrorParseResult_RawValue_"></a>
<a id="Vercel_AI_Hume_HumeErrorParseResult_RawValue"></a>

### RawValue


```csharp
public HumeErrorData? RawValue { get; }
```


Same payload as [Value](#Vercel_AI_Hume_HumeErrorParseResult_Value).

**Value:** [HumeErrorData](/api/Vercel.AI.Hume/HumeErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L55)

<a id="Vercel_AI_Hume_HumeErrorParseResult_Success_"></a>
<a id="Vercel_AI_Hume_HumeErrorParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when the error object was present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L49)

<a id="Vercel_AI_Hume_HumeErrorParseResult_Value_"></a>
<a id="Vercel_AI_Hume_HumeErrorParseResult_Value"></a>

### Value


```csharp
public HumeErrorData? Value { get; }
```


Parsed error.

**Value:** [HumeErrorData](/api/Vercel.AI.Hume/HumeErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L52)
