---
title: "LanguageModelCallOptionsInput"
description: "Model call options validated by LanguageModelCallPreparation."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelCallOptionsInput"
  hidden: true
---

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class LanguageModelCallOptionsInput
```


Model call options validated by [LanguageModelCallPreparation](/api/Vercel.AI.Util/LanguageModelCallPreparation/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L8)

## Properties

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_FrequencyPenalty_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L26)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_MaxOutputTokens_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; set; }
```


Maximum output tokens. Must be an integer greater than or equal to 1 when set.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L11)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_PresencePenalty_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L23)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_Reasoning_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Provider reasoning options.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L35)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_Seed_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_Seed"></a>

### Seed


```csharp
public double? Seed { get; set; }
```


Random seed. Must be an integer when set.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L29)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_StopSequences_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; set; }
```


Stop sequences.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L32)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_Temperature_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Sampling temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L14)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_TopK_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_TopK"></a>

### TopK


```csharp
public double? TopK { get; set; }
```


Top-k cutoff.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L20)

<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_TopP_"></a>
<a id="Vercel_AI_Util_LanguageModelCallOptionsInput_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Nucleus sampling probability.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LanguageModelCallPreparation.cs#L17)
