---
title: "GoogleRealtimeModelOptions"
description: "Google realtime provider options, read from providerOptions.google. Maps to GoogleRealtimeModelOptions of google-realtime-model-options.ts. Put ToJsonObject(..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeModelOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeModelOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeModelOptions
```


Google realtime provider options, read from `providerOptions.google`. Maps to
`GoogleRealtimeModelOptions` of `google-realtime-model-options.ts`. Put [ToJsonObject()](#Vercel_AI_Google_GoogleRealtimeModelOptions_ToJsonObject) under the
`google` key of [ProviderOptions](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/#Vercel_AI_Provider_RealtimeModelV4SessionConfig_ProviderOptions).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L41)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_DefaultToolBehavior_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_DefaultToolBehavior"></a>

### DefaultToolBehavior


```csharp
public string? DefaultToolBehavior { get; set; }
```


Default `behavior` stamped onto every function declaration in the session setup: `BLOCKING` or
`NON_BLOCKING`. Background-reasoning models accept only `NON_BLOCKING`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L59)

<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_ThinkingConfig_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_ThinkingConfig"></a>

### ThinkingConfig


```csharp
public GoogleRealtimeThinkingConfig? ThinkingConfig { get; set; }
```


Gemini Live thinking configuration. Supported by Live models with background reasoning (for example
`gemini-3.8-live-extended-thinking`). Not supported by latency-optimized models (for example `gemini-3.8-live`).

**Value:** [GoogleRealtimeThinkingConfig](/api/Vercel.AI.Google/GoogleRealtimeThinkingConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L53)

<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_TranslationConfig_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_TranslationConfig"></a>

### TranslationConfig


```csharp
public GoogleRealtimeTranslationConfig? TranslationConfig { get; set; }
```


Gemini Live Translation configuration. Required for `gemini-3.5-live-translate-preview` when translating
speech to a target language.

**Value:** [GoogleRealtimeTranslationConfig](/api/Vercel.AI.Google/GoogleRealtimeTranslationConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L47)

## Methods

<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_ToJsonObject_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelOptions_ToJsonObject"></a>

### ToJsonObject()


```csharp
public JsonObject ToJsonObject()
```


Converts the options to the JSON shape the session builder reads from `providerOptions.google`.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L62)
