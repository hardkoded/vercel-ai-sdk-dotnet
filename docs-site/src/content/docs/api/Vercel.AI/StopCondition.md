---
title: "StopCondition"
description: "Stops the tool loop. The default for generateText is IsStepCount(int) of 1."
editUrl: false
prev: false
next: false
sidebar:
  label: "StopCondition"
  hidden: true
---

<a id="Vercel_AI_StopCondition"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public abstract class StopCondition
```


Stops the tool loop. The default for `generateText` is [IsStepCount(int)](/api/Vercel.AI/StopWhen/#Vercel_AI_StopWhen_IsStepCount_System_Int32_) of 1.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L327)

## Methods

<a id="Vercel_AI_StopCondition_ShouldStop_"></a>
<a id="Vercel_AI_StopCondition_ShouldStop_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__"></a>

### `ShouldStop(IReadOnlyList<StepResult>)`


```csharp
public abstract bool ShouldStop(IReadOnlyList<StepResult> steps)
```


Returns true when the loop should stop after &lt;code class="paramref"&gt;steps&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L330)
