---
title: "OpenAICodeInterpreterToolOptions"
description: "Options of CodeInterpreter(OpenAICodeInterpreterToolOptions). ContainerId and FileIds are alternatives; with neither the container is created automatically."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICodeInterpreterToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAICodeInterpreterToolOptions
```


Options of [CodeInterpreter(OpenAICodeInterpreterToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_CodeInterpreter_Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_). [ContainerId](#Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_ContainerId) and [FileIds](#Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_FileIds) are alternatives; with neither the container is created automatically.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L131)

## Properties

<a id="Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_ContainerId_"></a>
<a id="Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_ContainerId"></a>

### ContainerId


```csharp
public string? ContainerId { get; set; }
```


Id of an existing container.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L134)

<a id="Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_FileIds_"></a>
<a id="Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_FileIds"></a>

### FileIds


```csharp
public IReadOnlyList<string>? FileIds { get; set; }
```


File ids made available to an automatically created container.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L137)
