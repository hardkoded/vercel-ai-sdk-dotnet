---
title: "AudioUsage"
description: "Numeric usage from a speech or transcription call. A count stays unset when the provider does not report that unit."
editUrl: false
prev: false
next: false
sidebar:
  label: "AudioUsage"
  hidden: true
---

<a id="Vercel_AI_Provider_AudioUsage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AudioUsage
```


Numeric usage from a speech or transcription call.
A count stays unset when the provider does not report that unit.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L234)

## Constructors

<a id="Vercel_AI_Provider_AudioUsage__ctor_"></a>
<a id="Vercel_AI_Provider_AudioUsage__ctor_System_Nullable_System_Int32__System_Nullable_System_Double__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### AudioUsage(int?, double?, int?, int?, int?)


```csharp
public AudioUsage(int? characters = null, double? seconds = null, int? inputTokens = null, int? outputTokens = null, int? totalTokens = null)
```


Creates usage. Counts that the provider did not report stay null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `characters` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `seconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `inputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `outputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `totalTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L237)

## Properties

<a id="Vercel_AI_Provider_AudioUsage_Characters_"></a>
<a id="Vercel_AI_Provider_AudioUsage_Characters"></a>

### Characters


```csharp
public int? Characters { get; }
```


Character count, when the provider reports one.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L247)

<a id="Vercel_AI_Provider_AudioUsage_InputTokens_"></a>
<a id="Vercel_AI_Provider_AudioUsage_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; }
```


Prompt tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L253)

<a id="Vercel_AI_Provider_AudioUsage_OutputTokens_"></a>
<a id="Vercel_AI_Provider_AudioUsage_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; }
```


Generated tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L256)

<a id="Vercel_AI_Provider_AudioUsage_Seconds_"></a>
<a id="Vercel_AI_Provider_AudioUsage_Seconds"></a>

### Seconds


```csharp
public double? Seconds { get; }
```


Duration in seconds, when the provider reports one.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L250)

<a id="Vercel_AI_Provider_AudioUsage_TotalTokens_"></a>
<a id="Vercel_AI_Provider_AudioUsage_TotalTokens"></a>

### TotalTokens


```csharp
public int? TotalTokens { get; }
```


Input plus output tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L259)
