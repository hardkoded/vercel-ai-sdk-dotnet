---
title: "StartStepPart"
description: "A step started. It comes before the parts of that step."
editUrl: false
prev: false
next: false
sidebar:
  label: "StartStepPart"
  hidden: true
---

<a id="Vercel_AI_StartStepPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StartStepPart : TextStreamPart
```


A step started. It comes before the parts of that step.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1115)

## Constructors

<a id="Vercel_AI_StartStepPart__ctor_"></a>
<a id="Vercel_AI_StartStepPart__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__Vercel_AI_LanguageModelRequestMetadata_"></a>

### `StartStepPart(IReadOnlyList<CallWarning>?, LanguageModelRequestMetadata?)`


```csharp
public StartStepPart(IReadOnlyList<CallWarning>? warnings = null, LanguageModelRequestMetadata? request = null)
```


Creates a start-step part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `request` | [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1118)

## Properties

<a id="Vercel_AI_StartStepPart_Request_"></a>
<a id="Vercel_AI_StartStepPart_Request"></a>

### Request


```csharp
public LanguageModelRequestMetadata Request { get; }
```


Request metadata of this step. Body and messages are set only when [Include](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_Include) asks for them.

**Value:** [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1126)

<a id="Vercel_AI_StartStepPart_Warnings_"></a>
<a id="Vercel_AI_StartStepPart_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings from the model call of this step.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1129)
