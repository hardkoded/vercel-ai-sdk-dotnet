---
title: "RubricModels"
description: "Helpers for IRubricModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "RubricModels"
  hidden: true
---

<a id="Vercel_AI_Provider_RubricModels"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public static class RubricModels
```


Helpers for [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L547)

## Methods

<a id="Vercel_AI_Provider_RubricModels_FromLegacy_"></a>
<a id="Vercel_AI_Provider_RubricModels_FromLegacy_Vercel_AI_Provider_IEvaluationModel_"></a>

### FromLegacy(IEvaluationModel)


```csharp
[Obsolete("Implement IRubricModel on the model instead.")]
public static IRubricModel FromLegacy(IEvaluationModel model)
```


Wraps a model that implements only the deprecated [IEvaluationModel](/api/Vercel.AI.Provider/IEvaluationModel/). A model that already implements
[IRubricModel](/api/Vercel.AI.Provider/IRubricModel/) is returned unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [IEvaluationModel](/api/Vercel.AI.Provider/IEvaluationModel/) |  |

**Returns:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L553)
