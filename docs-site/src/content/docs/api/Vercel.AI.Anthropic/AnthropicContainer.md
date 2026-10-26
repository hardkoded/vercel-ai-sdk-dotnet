---
title: "AnthropicContainer"
description: "Copies the latest Anthropic container id into the next call's provider options."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicContainer"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicContainer"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicContainer
```


Copies the latest Anthropic container id into the next call's provider options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContainer.cs#L10)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicContainer_ForwardFromLastStep_"></a>
<a id="Vercel_AI_Anthropic_AnthropicContainer_ForwardFromLastStep_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__"></a>

### `ForwardFromLastStep(IReadOnlyList<StepResult>)`


```csharp
public static PrepareStepUpdate? ForwardFromLastStep(IReadOnlyList<StepResult> steps)
```


Searches &lt;code class="paramref"&gt;steps&lt;/code&gt; from the end and returns a `prepareStep` update whose provider options set
`anthropic.container.id`. Returns null when no step has a container id.
Maps to `forwardAnthropicContainerIdFromLastStep`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [PrepareStepUpdate](/api/Vercel.AI/PrepareStepUpdate/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContainer.cs#L17)
