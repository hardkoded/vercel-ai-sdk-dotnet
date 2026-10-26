---
title: "DecisionModels"
description: "Resolves decision models, including string model ids. Maps to resolveDecisionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModels"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModels"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class DecisionModels
```


Resolves decision models, including string model ids. Maps to `resolveDecisionModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionModels.cs#L12)

## Fields

<a id="Vercel_AI_Operations_DecisionModels_MissingProviderMessage"></a>

### MissingProviderMessage


```csharp
public const string MissingProviderMessage = "The default provider does not support decision models."
```


Message thrown when the default provider has no decision models.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionModels.cs#L15)

## Properties

<a id="Vercel_AI_Operations_DecisionModels_DefaultProvider_"></a>
<a id="Vercel_AI_Operations_DecisionModels_DefaultProvider"></a>

### DefaultProvider


```csharp
public static ProviderBase? DefaultProvider { get; set; }
```


The global default provider (`AI_SDK_DEFAULT_PROVIDER`). A string model id is resolved through it. It must
implement [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/) or the deprecated [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/). When null, string ids go through Gateway.

**Value:** [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionModels.cs#L23)

<a id="Vercel_AI_Operations_DecisionModels_GatewayFactory_"></a>
<a id="Vercel_AI_Operations_DecisionModels_GatewayFactory"></a>

### GatewayFactory


```csharp
public static Func<GatewayProvider>? GatewayFactory { get; set; }
```


Creates the provider used when [DefaultProvider](#Vercel_AI_Operations_DecisionModels_DefaultProvider) is null. Null uses a shared [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/).

**Value:** [Func&lt;GatewayProvider&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionModels.cs#L26)

## Methods

<a id="Vercel_AI_Operations_DecisionModels_Resolve_"></a>
<a id="Vercel_AI_Operations_DecisionModels_Resolve_Vercel_AI_Operations_IDecisionCaller_System_String_"></a>

### Resolve(IDecisionCaller?, string?)


```csharp
public static IDecisionCaller Resolve(IDecisionCaller? model, string? modelId)
```


Resolves a decision model. &lt;code class="paramref"&gt;model&lt;/code&gt; wins when set. Otherwise &lt;code class="paramref"&gt;modelId&lt;/code&gt; goes through
[DefaultProvider](#Vercel_AI_Operations_DecisionModels_DefaultProvider), or Gateway when none is set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionModels.cs#L32)
