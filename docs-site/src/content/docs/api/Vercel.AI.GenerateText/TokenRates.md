---
title: "TokenRates"
description: "Token-rate helpers. Maps to calculateTokensPerSecond."
editUrl: false
prev: false
next: false
sidebar:
  label: "TokenRates"
  hidden: true
---

<a id="Vercel_AI_GenerateText_TokenRates"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class TokenRates
```


Token-rate helpers. Maps to `calculateTokensPerSecond`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CalculateTokensPerSecond.cs#L8)

## Methods

<a id="Vercel_AI_GenerateText_TokenRates_CalculateTokensPerSecond_"></a>
<a id="Vercel_AI_GenerateText_TokenRates_CalculateTokensPerSecond_System_Nullable_System_Double__System_Nullable_System_Double__"></a>

### CalculateTokensPerSecond(double?, double?)


```csharp
public static double CalculateTokensPerSecond(double? tokens, double? durationMs)
```


Calculates tokens per second. Returns 0 when the count or duration is unknown,
the duration is 0, or the rate is not a finite number.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `durationMs` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

**Returns:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CalculateTokensPerSecond.cs#L14)
