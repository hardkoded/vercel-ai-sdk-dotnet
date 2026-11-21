---
title: "GatewayDeepSeekSearchConfig"
description: "Developer-only configuration of the Gateway DeepSeek Search tool. Maps to DeepSeekSearchConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayDeepSeekSearchConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayDeepSeekSearchConfig
```


Developer-only configuration of the Gateway DeepSeek Search tool. Maps to `DeepSeekSearchConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L89)

## Properties

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_Count_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_Count"></a>

### Count


```csharp
public int? Count { get; set; }
```


Maximum number of results, 1–50 (default: 10). Maps to `count`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L95)

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_Localization_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_Localization"></a>

### Localization


```csharp
public string? Localization { get; set; }
```


Developer-configured IP address for result localization. Maps to `localization`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L107)

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_NeedContent_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_NeedContent"></a>

### NeedContent


```csharp
public bool? NeedContent { get; set; }
```


Include available page content (default: false). Maps to `needContent`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L98)

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_OriQuestion_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_OriQuestion"></a>

### OriQuestion


```csharp
public string? OriQuestion { get; set; }
```


Original question, 1–128 Unicode characters. Maps to `oriQuestion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L101)

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_OriQuestionNull_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_OriQuestionNull"></a>

### OriQuestionNull


```csharp
public bool OriQuestionNull { get; set; }
```


Sends `oriQuestion: null` (an absent question). Takes precedence over [OriQuestion](#Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_OriQuestion).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L104)

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_Query_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_Query"></a>

### Query


```csharp
public string? Query { get; set; }
```


Pin the query instead of allowing the model to choose it. Maps to `query`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L92)
