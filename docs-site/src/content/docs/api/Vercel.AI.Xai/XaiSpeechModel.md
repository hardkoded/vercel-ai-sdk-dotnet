---
title: "XaiSpeechModel"
description: "xAI text-to-speech model, over /tts. Maps to XaiSpeechModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiSpeechModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiSpeechModel : ISpeechModel, ISpeechCaller
```


xAI text-to-speech model, over `/tts`. Maps to `XaiSpeechModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/), [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L15)

## Constructors

<a id="Vercel_AI_Xai_XaiSpeechModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModel__ctor_Vercel_AI_Xai_XaiProvider_System_String_System_Func_System_DateTime__"></a>

### `XaiSpeechModel(XaiProvider, string, Func<DateTime>?)`


```csharp
public XaiSpeechModel(XaiProvider provider, string modelId = "", Func<DateTime>? clock = null)
```


Creates a model. xAI has one speech model, so the model id is empty. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now. Maps to `_internal.currentDate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L26)

## Properties

<a id="Vercel_AI_Xai_XaiSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L40)

<a id="Vercel_AI_Xai_XaiSpeechModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L37)

<a id="Vercel_AI_Xai_XaiSpeechModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L34)

## Methods

<a id="Vercel_AI_Xai_XaiSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModel_DoGenerateAsync_Vercel_AI_Operations_SpeechModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L84)

<a id="Vercel_AI_Xai_XaiSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModel.cs#L43)
