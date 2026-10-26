---
title: "DeepgramErrorParseResult"
description: "Result of parsing a Deepgram error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramErrorParseResult"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramErrorParseResult
```


Result of parsing a Deepgram error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L47)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult__ctor_System_Boolean_Vercel_AI_Deepgram_DeepgramErrorData_"></a>

### DeepgramErrorParseResult(bool, DeepgramErrorData?)


```csharp
public DeepgramErrorParseResult(bool success, DeepgramErrorData? value)
```


Creates a parse result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `value` | [DeepgramErrorData](/api/Vercel.AI.Deepgram/DeepgramErrorData/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L50)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult_RawValue_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult_RawValue"></a>

### RawValue


```csharp
public DeepgramErrorData? RawValue { get; }
```


Same payload as [Value](#Vercel_AI_Deepgram_DeepgramErrorParseResult_Value).

**Value:** [DeepgramErrorData](/api/Vercel.AI.Deepgram/DeepgramErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L64)

<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult_Success_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when `err_code` and `err_msg` were present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L58)

<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult_Value_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorParseResult_Value"></a>

### Value


```csharp
public DeepgramErrorData? Value { get; }
```


Parsed error.

**Value:** [DeepgramErrorData](/api/Vercel.AI.Deepgram/DeepgramErrorData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L61)
