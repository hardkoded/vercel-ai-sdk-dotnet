---
title: "GoogleVertexVideoModelOptions"
description: "Port of GoogleVertexVideoModelOptions (google-vertex-video-model-options.ts). Read from providerOptions.googleVertex, or the legacy providerOptions.vertex."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexVideoModelOptions
```


Port of `GoogleVertexVideoModelOptions` (`google-vertex-video-model-options.ts`). Read from
`providerOptions.googleVertex`, or the legacy `providerOptions.vertex`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L14)

## Properties

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_AdditionalProperties_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; }
```


Options this class does not model, sent to the API as they are. Upstream `z.looseObject`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L45)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_GcsOutputDirectory_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_GcsOutputDirectory"></a>

### GcsOutputDirectory


```csharp
public string? GcsOutputDirectory { get; set; }
```


Output directory in Google Cloud Storage.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L39)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_GenerateAudio_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_GenerateAudio"></a>

### GenerateAudio


```csharp
public bool? GenerateAudio { get; set; }
```


Whether to generate audio.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L36)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_NegativePrompt_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_NegativePrompt"></a>

### NegativePrompt


```csharp
public string? NegativePrompt { get; set; }
```


Negative prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L33)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_PersonGeneration_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_PersonGeneration"></a>

### PersonGeneration


```csharp
public string? PersonGeneration { get; set; }
```


`dont_allow`, `allow_adult`, or `allow_all`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L30)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_PollIntervalMs_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public double? PollIntervalMs { get; set; }
```


Polling interval in milliseconds. Positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L24)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_PollTimeoutMs_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public double? PollTimeoutMs { get; set; }
```


Polling timeout in milliseconds. Positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L27)

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_ReferenceImages_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_ReferenceImages"></a>

### ReferenceImages


```csharp
public JsonElement? ReferenceImages { get; set; }
```


Reference images, each with `bytesBase64Encoded` or `gcsUri`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L42)

## Methods

<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_Parse_"></a>
<a id="Vercel_AI_Google_GoogleVertexVideoModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static GoogleVertexVideoModelOptions? Parse(JsonElement providerOptions)
```


Maps `googleVertexVideoModelOptionsSchema`. Reads `providerOptions.googleVertex` first, then
`providerOptions.vertex`. Returns null when neither is present. An invalid value throws [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [GoogleVertexVideoModelOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexVideoModelOptions.cs#L52)
