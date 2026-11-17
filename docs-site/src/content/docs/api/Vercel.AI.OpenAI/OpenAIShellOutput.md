---
title: "OpenAIShellOutput"
description: "The output of the shell tool, what the application sends back."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellOutput"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellOutput"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellOutput
```


The output of the shell tool, what the application sends back.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L55)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellOutput_MaxOutputLength_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutput_MaxOutputLength"></a>

### MaxOutputLength


```csharp
public double? MaxOutputLength { get; set; }
```


Optional maximum number of characters that was applied to each command's output.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L58)

<a id="Vercel_AI_OpenAI_OpenAIShellOutput_Output_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutput_Output"></a>

### Output


```csharp
public IReadOnlyList<OpenAIShellOutputItem> Output { get; set; }
```


An array of shell call output contents.

**Value:** [IReadOnlyList&lt;OpenAIShellOutputItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L61)
