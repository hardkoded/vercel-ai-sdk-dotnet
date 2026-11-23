---
title: "DecisionCapabilities"
description: "Upstream ModelCapabilities.decision."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionCapabilities"
  hidden: true
---

<a id="Vercel_AI_DecisionCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DecisionCapabilities
```


Upstream `ModelCapabilities.decision`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L262)

## Properties

<a id="Vercel_AI_DecisionCapabilities_Inputs_"></a>
<a id="Vercel_AI_DecisionCapabilities_Inputs"></a>

### Inputs


```csharp
public DecisionInputCapabilities Inputs { get; init; }
```


Upstream `inputs`.

**Value:** [DecisionInputCapabilities](/api/Vercel.AI/DecisionInputCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L268)

<a id="Vercel_AI_DecisionCapabilities_MaxOutputTokens_"></a>
<a id="Vercel_AI_DecisionCapabilities_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public ModelCapability MaxOutputTokens { get; init; }
```


Upstream `maxOutputTokens`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L274)

<a id="Vercel_AI_DecisionCapabilities_QuestionTypes_"></a>
<a id="Vercel_AI_DecisionCapabilities_QuestionTypes"></a>

### QuestionTypes


```csharp
public DecisionQuestionTypeCapabilities QuestionTypes { get; init; }
```


Upstream `questionTypes`.

**Value:** [DecisionQuestionTypeCapabilities](/api/Vercel.AI/DecisionQuestionTypeCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L265)

<a id="Vercel_AI_DecisionCapabilities_Reasoning_"></a>
<a id="Vercel_AI_DecisionCapabilities_Reasoning"></a>

### Reasoning


```csharp
public ModelCapability Reasoning { get; init; }
```


Upstream `reasoning`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L271)
