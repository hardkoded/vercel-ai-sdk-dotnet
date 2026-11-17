---
title: "ScaleDownExtractionModelConfig"
description: "Settings for a ScaleDownExtractionModel. Maps to ScaleDownExtractionModelConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownExtractionModelConfig"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public class ScaleDownExtractionModelConfig
```


Settings for a [ScaleDownExtractionModel](/api/Vercel.AI.ScaleDown/ScaleDownExtractionModel/). Maps to `ScaleDownExtractionModelConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L14)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API base URL, without a trailing slash.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L20)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_Headers_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_Headers"></a>

### Headers


```csharp
public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }
```


Provider headers, resolved on every call. When null, the model sends the `x-api-key` read from
`SCALEDOWN_API_KEY` at call time.

**Value:** [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L26)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_HeadersAsync_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_HeadersAsync"></a>

### HeadersAsync


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string>>>? HeadersAsync { get; set; }
```


Provider headers that need an asynchronous step, such as an async API key. Used when [Headers](#Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_Headers) is null.
Not serializable: `Serialize` throws [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/), as upstream does for an async `headers`.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L32)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_HttpClient_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient? HttpClient { get; set; }
```


HTTP client. Not serialized.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L38)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_Provider_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; set; }
```


Provider id, such as `scaledown.extraction`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L17)
