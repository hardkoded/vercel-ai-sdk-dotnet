---
title: "OperationUsage"
description: "Token counts for an image or evaluation call."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationUsage"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationUsage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class OperationUsage
```


Token counts for an image or evaluation call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L412)

## Constructors

<a id="Vercel_AI_Operations_OperationUsage__ctor_"></a>
<a id="Vercel_AI_Operations_OperationUsage__ctor_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Double__System_Nullable_System_Double__"></a>

### OperationUsage(int?, int?, int?, double?, double?)


```csharp
public OperationUsage(int? inputTokens = null, int? outputTokens = null, int? totalTokens = null, double? tokens = null, double? seconds = null)
```


Creates usage. Unset counts stay null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `outputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `totalTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `tokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `seconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L415)

## Properties

<a id="Vercel_AI_Operations_OperationUsage_InputTokens_"></a>
<a id="Vercel_AI_Operations_OperationUsage_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; }
```


Prompt tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L425)

<a id="Vercel_AI_Operations_OperationUsage_OutputTokens_"></a>
<a id="Vercel_AI_Operations_OperationUsage_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; }
```


Generated tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L428)

<a id="Vercel_AI_Operations_OperationUsage_Seconds_"></a>
<a id="Vercel_AI_Operations_OperationUsage_Seconds"></a>

### Seconds


```csharp
public double? Seconds { get; }
```


Seconds of audio processed. Reported by streaming transcription.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L437)

<a id="Vercel_AI_Operations_OperationUsage_Tokens_"></a>
<a id="Vercel_AI_Operations_OperationUsage_Tokens"></a>

### Tokens


```csharp
public double? Tokens { get; }
```


Embedding token count. [NaN](https://learn.microsoft.com/dotnet/api/system.double.nan) when the provider omitted usage.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L434)

<a id="Vercel_AI_Operations_OperationUsage_TotalTokens_"></a>
<a id="Vercel_AI_Operations_OperationUsage_TotalTokens"></a>

### TotalTokens


```csharp
public int? TotalTokens { get; }
```


Input plus output, when both are known.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L431)

## Methods

<a id="Vercel_AI_Operations_OperationUsage_AddImage_"></a>
<a id="Vercel_AI_Operations_OperationUsage_AddImage_Vercel_AI_Operations_OperationUsage_Vercel_AI_Operations_OperationUsage_"></a>

### AddImage(OperationUsage, OperationUsage)


```csharp
public static OperationUsage AddImage(OperationUsage left, OperationUsage right)
```


Adds two image usage values. A missing count stays missing until one side reports it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `left` | [OperationUsage](./) |  |
| `right` | [OperationUsage](./) |  |

**Returns:** [OperationUsage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L440)
