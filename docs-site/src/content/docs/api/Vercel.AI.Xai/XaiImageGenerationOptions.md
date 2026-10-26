---
title: "XaiImageGenerationOptions"
description: "Arguments of String). Maps to imageGenerationArgsSchema."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiImageGenerationOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiImageGenerationOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiImageGenerationOptions
```


Arguments of `String)`. Maps to `imageGenerationArgsSchema`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L62)

## Properties

<a id="Vercel_AI_Xai_XaiImageGenerationOptions_Action_"></a>
<a id="Vercel_AI_Xai_XaiImageGenerationOptions_Action"></a>

### Action


```csharp
public XaiImageGenerationAction? Action { get; init; }
```


What the tool can do. xAI uses [Auto](/api/Vercel.AI.Xai/XaiImageGenerationAction/#Vercel_AI_Xai_XaiImageGenerationAction_Auto) when this is null.

**Value:** `XaiImageGenerationAction?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L65)
