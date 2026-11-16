---
title: "XaiVideoModelOptions"
description: "Provider options for xAI video generation, read from providerOptions.xai. Upstream XaiVideoModelOptions and XaiParsedVideoModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiVideoModelOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiVideoModelOptions
```


Provider options for xAI video generation, read from `providerOptions.xai`. Upstream `XaiVideoModelOptions` and `XaiParsedVideoModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

Use [Mode](#Vercel_AI_Xai_XaiVideoModelOptions_Mode) to select the operation:

&lt;ul&gt;&lt;li&gt;`edit-video` with [VideoUrl](#Vercel_AI_Xai_XaiVideoModelOptions_VideoUrl): video editing (`POST /v1/videos/edits`).&lt;/li&gt;&lt;li&gt;`extend-video` with [VideoUrl](#Vercel_AI_Xai_XaiVideoModelOptions_VideoUrl): video extension (`POST /v1/videos/extensions`).&lt;/li&gt;&lt;li&gt;`reference-to-video` with [ReferenceImageUrls](#Vercel_AI_Xai_XaiVideoModelOptions_ReferenceImageUrls): R2V generation (`POST /v1/videos/generations`).&lt;/li&gt;&lt;li&gt;No mode: standard generation from text prompts or image input.&lt;/li&gt;&lt;/ul&gt;

Omitting the mode while providing [VideoUrl](#Vercel_AI_Xai_XaiVideoModelOptions_VideoUrl) behaves like `edit-video`. Omitting it while providing [ReferenceImageUrls](#Vercel_AI_Xai_XaiVideoModelOptions_ReferenceImageUrls) behaves like `reference-to-video`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L50)

## Fields

<a id="Vercel_AI_Xai_XaiVideoModelOptions_EditVideo"></a>

### EditVideo


```csharp
public const string EditVideo = "edit-video"
```


The `edit-video` mode.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L53)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_ExtendVideo"></a>

### ExtendVideo


```csharp
public const string ExtendVideo = "extend-video"
```


The `extend-video` mode.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L56)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_ReferenceToVideo"></a>

### ReferenceToVideo


```csharp
public const string ReferenceToVideo = "reference-to-video"
```


The `reference-to-video` mode.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L59)

## Properties

<a id="Vercel_AI_Xai_XaiVideoModelOptions_AdditionalProperties_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; }
```


Options this class does not model. They are sent to the API as they are. Upstream `z.looseObject`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L99)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_Keyframes_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_Keyframes"></a>

### Keyframes


```csharp
public IReadOnlyList<XaiVideoKeyframe>? Keyframes { get; set; }
```


Mid-video image anchors (up to 4).

**Value:** [IReadOnlyList&lt;XaiVideoKeyframe&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L78)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_Mode_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_Mode"></a>

### Mode


```csharp
public string? Mode { get; set; }
```


`edit-video`, `extend-video`, `reference-to-video`, or null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L66)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_OutputUploadUrl_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_OutputUploadUrl"></a>

### OutputUploadUrl


```csharp
public string? OutputUploadUrl { get; set; }
```


Deliver directly to caller-managed storage using an HTTPS signed PUT URL. Upstream `output.uploadUrl`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L87)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_PollIntervalMs_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public double? PollIntervalMs { get; set; }
```


Poll interval in milliseconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L90)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_PollTimeoutMs_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public double? PollTimeoutMs { get; set; }
```


Poll timeout in milliseconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L93)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_ReferenceImageUrls_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_ReferenceImageUrls"></a>

### ReferenceImageUrls


```csharp
public IReadOnlyList<string>? ReferenceImageUrls { get; set; }
```


Reference image URLs (1-7) for R2V generation.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L72)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_ReferenceVoiceIds_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_ReferenceVoiceIds"></a>

### ReferenceVoiceIds


```csharp
public IReadOnlyList<string>? ReferenceVoiceIds { get; set; }
```


Preset voice ids (up to 3) that give the subject a voice.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L75)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_Resolution_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_Resolution"></a>

### Resolution


```csharp
public string? Resolution { get; set; }
```


`480p`, `720p`, or `1080p`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L96)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_StorageOptions_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_StorageOptions"></a>

### StorageOptions


```csharp
public XaiVideoStorageOptions? StorageOptions { get; set; }
```


Store the generated video in the xAI Files API.

**Value:** [XaiVideoStorageOptions](/api/Vercel.AI.Xai/XaiVideoStorageOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L81)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_User_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_User"></a>

### User


```csharp
public string? User { get; set; }
```


A unique identifier representing the end user, for abuse monitoring.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L84)

<a id="Vercel_AI_Xai_XaiVideoModelOptions_VideoUrl_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_VideoUrl"></a>

### VideoUrl


```csharp
public string? VideoUrl { get; set; }
```


Source video URL to edit or extend.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L69)

## Methods

<a id="Vercel_AI_Xai_XaiVideoModelOptions_Parse_"></a>
<a id="Vercel_AI_Xai_XaiVideoModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static XaiVideoModelOptions? Parse(JsonElement providerOptions)
```


Reads `providerOptions.xai`. Returns null when absent. An invalid value throws [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/).
When `output` is present the error carries no details, so a signed upload URL never reaches a message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [XaiVideoModelOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L105)
