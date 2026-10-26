---
title: "CartesiaSpeechModel"
description: "Cartesia text-to-speech model. Posts to /tts/bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaSpeechModel"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public sealed class CartesiaSpeechModel : ISpeechModel, ISpeechCaller
```


Cartesia text-to-speech model. Posts to `/tts/bytes`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/), [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L126)

## Constructors

<a id="Vercel_AI_Cartesia_CartesiaSpeechModel__ctor_"></a>
<a id="Vercel_AI_Cartesia_CartesiaSpeechModel__ctor_Vercel_AI_Cartesia_CartesiaProvider_System_String_System_Func_System_DateTime__"></a>

### `CartesiaSpeechModel(CartesiaProvider, string, Func<DateTime>?)`


```csharp
public CartesiaSpeechModel(CartesiaProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a speech model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [CartesiaProvider](/api/Vercel.AI.Cartesia/CartesiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L134)

## Properties

<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L145)

<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_Provider_"></a>
<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L142)

## Methods

<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_DoGenerateAsync_Vercel_AI_Operations_SpeechModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechModelCall, CancellationToken)


```csharp
public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
```


Generates speech audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [SpeechModelCall](/api/Vercel.AI.Operations/SpeechModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L156)

<a id="Vercel_AI_Cartesia_CartesiaSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechCallOptions, CancellationToken)


```csharp
public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
```


Synthesizes speech.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SpeechCallOptions](/api/Vercel.AI.Provider/SpeechCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L148)
