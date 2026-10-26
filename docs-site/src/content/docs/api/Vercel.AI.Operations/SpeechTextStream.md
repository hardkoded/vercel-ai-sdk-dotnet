---
title: "SpeechTextStream"
description: "The text a speech model reads. The provider owns it once doStream is called: it must enumerate it to the end or dispose its enumerator on every terminal path."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechTextStream"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechTextStream"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechTextStream : IAsyncEnumerable<SpeechTextPart>
```


The text a speech model reads. The provider owns it once `doStream` is called: it must enumerate it to the end
or dispose its enumerator on every terminal path.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAsyncEnumerable&lt;SpeechTextPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L65)

## Constructors

<a id="Vercel_AI_Operations_SpeechTextStream__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechTextStream__ctor_System_Collections_Generic_IAsyncEnumerable_Vercel_AI_Operations_SpeechTextPart__"></a>

### `SpeechTextStream(IAsyncEnumerable<SpeechTextPart>)`


```csharp
public SpeechTextStream(IAsyncEnumerable<SpeechTextPart> source)
```


Creates a stream over &lt;code class="paramref"&gt;source&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [IAsyncEnumerable&lt;SpeechTextPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L70)

## Properties

<a id="Vercel_AI_Operations_SpeechTextStream_Locked_"></a>
<a id="Vercel_AI_Operations_SpeechTextStream_Locked"></a>

### Locked


```csharp
public bool Locked { get; }
```


True after the model takes the enumerator.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L76)

## Methods

<a id="Vercel_AI_Operations_SpeechTextStream_CancelAsync_"></a>
<a id="Vercel_AI_Operations_SpeechTextStream_CancelAsync"></a>

### CancelAsync()


```csharp
public Task CancelAsync()
```


Releases a stream no model took. A locked stream stays with the model.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L91)

<a id="Vercel_AI_Operations_SpeechTextStream_GetAsyncEnumerator_"></a>
<a id="Vercel_AI_Operations_SpeechTextStream_GetAsyncEnumerator_System_Threading_CancellationToken_"></a>

### GetAsyncEnumerator(CancellationToken)


```csharp
public IAsyncEnumerator<SpeechTextPart> GetAsyncEnumerator(CancellationToken cancellationToken = default)
```


Takes the single enumerator.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerator&lt;SpeechTextPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerator-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L79)
