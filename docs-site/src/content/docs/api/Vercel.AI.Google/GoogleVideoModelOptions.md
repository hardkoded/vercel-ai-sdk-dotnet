---
title: "GoogleVideoModelOptions"
description: "Provider options for Google video generation, read from providerOptions.google. Upstream GoogleVideoModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVideoModelOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVideoModelOptions
```


Provider options for Google video generation, read from `providerOptions.google`. Upstream `GoogleVideoModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L24)

## Properties

<a id="Vercel_AI_Google_GoogleVideoModelOptions_AdditionalProperties_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public Dictionary<string, JsonElement> AdditionalProperties { get; }
```


Any other option. It is copied into the request `parameters`. Upstream `[key: string]: unknown`.

**Value:** [Dictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L44)

<a id="Vercel_AI_Google_GoogleVideoModelOptions_NegativePrompt_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_NegativePrompt"></a>

### NegativePrompt


```csharp
public string? NegativePrompt { get; set; }
```


Negative prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L38)

<a id="Vercel_AI_Google_GoogleVideoModelOptions_PersonGeneration_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_PersonGeneration"></a>

### PersonGeneration


```csharp
public string? PersonGeneration { get; set; }
```


`dont_allow`, `allow_adult`, or `allow_all`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L35)

<a id="Vercel_AI_Google_GoogleVideoModelOptions_PollIntervalMs_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public double? PollIntervalMs { get; set; }
```


Polling interval in milliseconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L29)

<a id="Vercel_AI_Google_GoogleVideoModelOptions_PollTimeoutMs_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public double? PollTimeoutMs { get; set; }
```


Polling timeout in milliseconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L32)

<a id="Vercel_AI_Google_GoogleVideoModelOptions_ReferenceImages_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_ReferenceImages"></a>

### ReferenceImages


```csharp
public List<GoogleVideoModelReferenceImage>? ReferenceImages { get; set; }
```


Reference images for style or asset guidance.

**Value:** [List&lt;GoogleVideoModelReferenceImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L41)

## Methods

<a id="Vercel_AI_Google_GoogleVideoModelOptions_Parse_"></a>
<a id="Vercel_AI_Google_GoogleVideoModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static GoogleVideoModelOptions? Parse(JsonElement providerOptions)
```


Reads `providerOptions.google`. Returns null when it is absent. Maps to `parseProviderOptions` with `googleVideoModelOptionsSchema`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [GoogleVideoModelOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVideoModelOptions.cs#L48)
