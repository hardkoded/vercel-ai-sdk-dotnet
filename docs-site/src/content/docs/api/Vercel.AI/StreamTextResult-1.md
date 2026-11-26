---
title: "StreamTextResult<TOutput>"
description: "Result of streamText whose parsed output has the type of the OutputSpec%601 of the call. Maps to StreamTextResult<TOOLS, OUTPUT>."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextResult<TOutput>"
  hidden: true
---

<a id="Vercel_AI_StreamTextResult_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextResult<TOutput> : StreamTextResult
```


Result of `streamText` whose parsed output has the type of the `OutputSpec%601` of the call.
Maps to `StreamTextResult<TOOLS, OUTPUT>`.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` | The type of the parsed output. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5777)

## Properties

<a id="Vercel_AI_StreamTextResult_1_Output_"></a>
<a id="Vercel_AI_StreamTextResult_1_Output"></a>

### Output


```csharp
public Task<TOutput?> Output { get; }
```


The parsed output once the stream finished, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/default"&gt;default&lt;/a&gt; when none was generated. A mismatch with
&lt;code class="typeparamref"&gt;TOutput&lt;/code&gt; faults the task with [JsonException](https://learn.microsoft.com/dotnet/api/system.text.json.jsonexception). Maps to `output`.

**Value:** [Task&lt;TOutput&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5792)
