---
title: "ElevenLabsErrorParseResult"
description: "Result of parsing an ElevenLabs error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsErrorParseResult"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsErrorParseResult
```


Result of parsing an ElevenLabs error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L43)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult__ctor_System_Boolean_Vercel_AI_ElevenLabs_ElevenLabsErrorData_"></a>

### ElevenLabsErrorParseResult(bool, ElevenLabsErrorData?)


```csharp
public ElevenLabsErrorParseResult(bool success, ElevenLabsErrorData? value)
```


Creates a parse result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `value` | [ElevenLabsErrorData](/api/Vercel.AI.ElevenLabs/ElevenLabsErrorData/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L46)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_RawValue_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_RawValue"></a>

### RawValue


```csharp
public ElevenLabsErrorData? RawValue { get; }
```


Same payload as [Value](#Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_Value).

**Value:** [ElevenLabsErrorData](/api/Vercel.AI.ElevenLabs/ElevenLabsErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L60)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_Success_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when `error.message` and `error.code` were present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L54)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_Value_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorParseResult_Value"></a>

### Value


```csharp
public ElevenLabsErrorData? Value { get; }
```


Parsed error.

**Value:** [ElevenLabsErrorData](/api/Vercel.AI.ElevenLabs/ElevenLabsErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L57)
