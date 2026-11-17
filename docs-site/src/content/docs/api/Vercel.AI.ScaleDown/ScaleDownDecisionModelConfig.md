---
title: "ScaleDownDecisionModelConfig"
description: "Settings for a DecisionScaleDownModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownDecisionModelConfig"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public class ScaleDownDecisionModelConfig
```


Settings for a [DecisionScaleDownModel](/api/Vercel.AI.ScaleDown/DecisionScaleDownModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L30)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API base URL, without a trailing slash.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L36)

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_Headers_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_Headers"></a>

### Headers


```csharp
public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }
```


Provider headers, resolved on every call. When null, the model sends the `x-api-key` read from
`SCALEDOWN_API_KEY` at call time.

**Value:** [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L42)

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_HeadersAsync_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_HeadersAsync"></a>

### HeadersAsync


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string>>>? HeadersAsync { get; set; }
```


Provider headers that need an asynchronous step, such as an async API key. Used when [Headers](#Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_Headers) is null.
Not serializable: `Serialize` throws [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/), as upstream does for an async `headers`.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L48)

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_HttpClient_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient? HttpClient { get; set; }
```


HTTP client. Not serialized.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L54)

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_Provider_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; set; }
```


Provider id, such as `scaledown.decision`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L33)
