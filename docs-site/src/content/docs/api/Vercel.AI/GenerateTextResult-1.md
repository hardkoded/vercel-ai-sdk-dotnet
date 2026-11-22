---
title: "GenerateTextResult<TOutput>"
description: "Result of generateText whose parsed output has the type of the OutputSpec%601 of the call. Maps to GenerateTextResult<TOOLS, OUTPUT>."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextResult<TOutput>"
  hidden: true
---

<a id="Vercel_AI_GenerateTextResult_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class GenerateTextResult<TOutput> : GenerateTextResult, IOperationEndEvent
```


Result of `generateText` whose parsed output has the type of the `OutputSpec%601` of the call.
Maps to `GenerateTextResult<TOOLS, OUTPUT>`.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` | The type of the parsed output. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateTextResult](/api/Vercel.AI/GenerateTextResult/)

**Implements:** [IOperationEndEvent](/api/Vercel.AI/IOperationEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2458)

## Properties

<a id="Vercel_AI_GenerateTextResult_1_Output_"></a>
<a id="Vercel_AI_GenerateTextResult_1_Output"></a>

### Output


```csharp
public TOutput? Output { get; }
```


The parsed output, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/default"&gt;default&lt;/a&gt; when the call generated none, as [Output](/api/Vercel.AI/GenerateTextResult/#Vercel_AI_GenerateTextResult_Output).
A mismatch with &lt;code class="typeparamref"&gt;TOutput&lt;/code&gt; throws [JsonException](https://learn.microsoft.com/dotnet/api/system.text.json.jsonexception). Maps to `output`.

**Value:** `TOutput`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2475)
