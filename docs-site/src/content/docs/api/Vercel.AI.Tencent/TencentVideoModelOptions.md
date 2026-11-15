---
title: "TencentVideoModelOptions"
description: "Provider-specific options for Tencent video generation, passed as providerOptions.tencent. Options apply to TokenHub Kling models (kling-video-), VOD AIGC mo..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentVideoModelOptions"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentVideoModelOptions
```


Provider-specific options for Tencent video generation, passed as `providerOptions.tencent`.
Options apply to TokenHub Kling models (`kling-video-*`), VOD AIGC models (`vod/<ModelName>/<ModelVersion>`), or both.
An option that does not apply to the selected model is ignored with a warning.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L48)

## Properties

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Assets_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Assets"></a>

### Assets


```csharp
public IReadOnlyList<TencentVideoAsset>? Assets { get; set; }
```


VOD: registered materials, referenced by their `asset://` URL. Sent after the other file inputs.

**Value:** [IReadOnlyList&lt;TencentVideoAsset&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L117)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Audio_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Audio"></a>

### Audio


```csharp
public string? Audio { get; set; }
```


TokenHub Kling: `native`, `original` or `off`. Takes precedence over the top-level generateAudio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L54)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_CharacterOrientation_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_CharacterOrientation"></a>

### CharacterOrientation


```csharp
public string? CharacterOrientation { get; set; }
```


TokenHub Kling motion control: `image` or `video`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L60)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Elements_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Elements"></a>

### Elements


```csharp
public IReadOnlyList<TencentVideoElement>? Elements { get; set; }
```


TokenHub Kling: custom elements.

**Value:** [IReadOnlyList&lt;TencentVideoElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L63)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_EnhancePrompt_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_EnhancePrompt"></a>

### EnhancePrompt


```csharp
public bool? EnhancePrompt { get; set; }
```


VOD: whether to optimize the prompt automatically.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L81)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_EnhanceSwitch_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_EnhanceSwitch"></a>

### EnhanceSwitch


```csharp
public bool? EnhanceSwitch { get; set; }
```


VOD: whether to upscale output with video enhancement.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L99)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_ExtInfo_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_ExtInfo"></a>

### ExtInfo


```csharp
public string? ExtInfo { get; set; }
```


VOD: reserved field for model-specific parameters, sent as is.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L114)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_ExternalTaskId_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_ExternalTaskId"></a>

### ExternalTaskId


```csharp
public string? ExternalTaskId { get; set; }
```


TokenHub Kling: custom task id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L75)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_InputComplianceCheck_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_InputComplianceCheck"></a>

### InputComplianceCheck


```csharp
public bool? InputComplianceCheck { get; set; }
```


VOD: whether to run a compliance check on the input.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L93)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_InputRegion_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_InputRegion"></a>

### InputRegion


```csharp
public string? InputRegion { get; set; }
```


VOD: where input files are stored: `Oversea`, `OverseaUSWest` or `Mainland`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L84)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_MultiShot_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_MultiShot"></a>

### MultiShot


```csharp
public bool? MultiShot { get; set; }
```


TokenHub Kling: whether a multi-shot prompt produces multi-shot output.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L57)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_NegativePrompt_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_NegativePrompt"></a>

### NegativePrompt


```csharp
public string? NegativePrompt { get; set; }
```


VOD: prompt describing what the model should avoid.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L78)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_OffPeak_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_OffPeak"></a>

### OffPeak


```csharp
public bool? OffPeak { get; set; }
```


VOD: whether to run the task in off-peak mode.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L102)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_OutputComplianceCheck_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_OutputComplianceCheck"></a>

### OutputComplianceCheck


```csharp
public bool? OutputComplianceCheck { get; set; }
```


VOD: whether to run a compliance check on the output.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L96)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_PersonGeneration_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_PersonGeneration"></a>

### PersonGeneration


```csharp
public string? PersonGeneration { get; set; }
```


VOD: `AllowAdult` or `Disallowed`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L90)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_ReferenceVideoType_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_ReferenceVideoType"></a>

### ReferenceVideoType


```csharp
public string? ReferenceVideoType { get; set; }
```


How reference videos are used: `feature` (default) or `base`. TokenHub Kling omni and VOD Kling.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L69)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Resolution_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Resolution"></a>

### Resolution


```csharp
public string? Resolution { get; set; }
```


Output resolution tier, sent as is. Takes precedence over the top-level resolution. Both backends.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L51)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_SceneType_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_SceneType"></a>

### SceneType


```csharp
public string? SceneType { get; set; }
```


VOD: scenario type, for example `motion_control`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L87)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_SessionContext_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_SessionContext"></a>

### SessionContext


```csharp
public string? SessionContext { get; set; }
```


VOD: context string passed through to callbacks.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L108)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_SessionId_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_SessionId"></a>

### SessionId


```csharp
public string? SessionId { get; set; }
```


VOD: deduplication identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L105)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_TasksPriority_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_TasksPriority"></a>

### TasksPriority


```csharp
public int? TasksPriority { get; set; }
```


VOD: task priority from -10 to 10.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L111)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Voices_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Voices"></a>

### Voices


```csharp
public IReadOnlyList<TencentVideoVoice>? Voices { get; set; }
```


TokenHub Kling omni: voices, referenced in the prompt as `@id`.

**Value:** [IReadOnlyList&lt;TencentVideoVoice&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L66)

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Watermark_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Watermark"></a>

### Watermark


```csharp
public bool? Watermark { get; set; }
```


Whether to add a watermark (TokenHub `watermark_info`, VOD `LogoAdd`). Both backends.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L72)

## Methods

<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Parse_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static TencentVideoModelOptions? Parse(JsonElement providerOptions)
```


Reads `providerOptions.tencent`. Returns null when absent. An invalid value throws [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [TencentVideoModelOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L120)
