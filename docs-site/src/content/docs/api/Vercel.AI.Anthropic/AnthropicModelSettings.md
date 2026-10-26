---
title: "AnthropicModelSettings"
description: "Optional settings that let a Messages-compatible provider adjust the shared Anthropic adapter."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicModelSettings"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicModelSettings"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicModelSettings
```


Optional settings that let a Messages-compatible provider adjust the shared Anthropic adapter.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelSettings.cs#L17)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicModelSettings_CacheControlProviderOptionsName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelSettings_CacheControlProviderOptionsName"></a>

### CacheControlProviderOptionsName


```csharp
public string? CacheControlProviderOptionsName { get; init; }
```


Provider-options namespace read for cache control before `anthropic`. Defaults to `anthropic`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelSettings.cs#L29)

<a id="Vercel_AI_Anthropic_AnthropicModelSettings_ConvertMediaPart_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelSettings_ConvertMediaPart"></a>

### ConvertMediaPart


```csharp
public AnthropicMediaPartConverter? ConvertMediaPart { get; init; }
```


Custom media conversion. &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; uses the default.

**Value:** [AnthropicMediaPartConverter](/api/Vercel.AI.Anthropic/AnthropicMediaPartConverter/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelSettings.cs#L20)

<a id="Vercel_AI_Anthropic_AnthropicModelSettings_MaxTemperature_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelSettings_MaxTemperature"></a>

### MaxTemperature


```csharp
public double? MaxTemperature { get; init; }
```


Maximum temperature. Defaults to 1.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelSettings.cs#L23)

<a id="Vercel_AI_Anthropic_AnthropicModelSettings_SupportsTemperatureAndTopPWithThinking_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelSettings_SupportsTemperatureAndTopPWithThinking"></a>

### SupportsTemperatureAndTopPWithThinking


```csharp
public bool SupportsTemperatureAndTopPWithThinking { get; init; }
```


Keeps `temperature` and `topP` when thinking is on.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelSettings.cs#L26)
