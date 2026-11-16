---
title: "GoogleRealtimeThinkingConfig"
description: "Gemini Live thinking configuration. Maps to thinkingConfig of GoogleRealtimeModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeThinkingConfig"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeThinkingConfig
```


Gemini Live thinking configuration. Maps to `thinkingConfig` of `GoogleRealtimeModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L20)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig_IncludeThoughts_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig_IncludeThoughts"></a>

### IncludeThoughts


```csharp
public bool? IncludeThoughts { get; set; }
```


Whether thought summaries should be included in the response.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L33)

<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingBudget_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingBudget"></a>

### ThinkingBudget


```csharp
public int? ThinkingBudget { get; set; }
```


Token budget for background thinking. Mutually exclusive with [ThinkingLevel](#Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingLevel).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L30)

<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingLevel_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingLevel"></a>

### ThinkingLevel


```csharp
public string? ThinkingLevel { get; set; }
```


Thinking effort level: `low`, `medium` or `high`. Background-reasoning Live models require exactly
one of [ThinkingLevel](#Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingLevel) or [ThinkingBudget](#Vercel_AI_Google_GoogleRealtimeThinkingConfig_ThinkingBudget); when neither is set, the provider sends
`thinkingLevel: "low"` on those models.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L27)
