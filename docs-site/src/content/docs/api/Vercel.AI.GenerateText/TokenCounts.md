---
title: "TokenCounts"
description: "Token-count helpers. Maps to sumTokenCounts."
editUrl: false
prev: false
next: false
sidebar:
  label: "TokenCounts"
  hidden: true
---

<a id="Vercel_AI_GenerateText_TokenCounts"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class TokenCounts
```


Token-count helpers. Maps to `sumTokenCounts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SumTokenCounts.cs#L8)

## Methods

<a id="Vercel_AI_GenerateText_TokenCounts_SumTokenCounts_"></a>
<a id="Vercel_AI_GenerateText_TokenCounts_SumTokenCounts_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### SumTokenCounts(int?, int?)


```csharp
public static int? SumTokenCounts(int? tokenCount1, int? tokenCount2)
```


Adds token counts. An unknown count is treated as 0 when the other count is known.
Returns null when both counts are unknown.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tokenCount1` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `tokenCount2` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SumTokenCounts.cs#L14)
