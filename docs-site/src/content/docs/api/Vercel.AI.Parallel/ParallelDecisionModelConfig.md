---
title: "ParallelDecisionModelConfig"
description: "Settings for a DecisionParallelModel. Maps to ParallelDecisionModelConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelDecisionModelConfig"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public class ParallelDecisionModelConfig
```


Settings for a [DecisionParallelModel](/api/Vercel.AI.Parallel/DecisionParallelModel/). Maps to `ParallelDecisionModelConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/DecisionParallelModel.cs#L15)

## Properties

<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API base URL, without a trailing slash.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/DecisionParallelModel.cs#L21)

<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_Headers_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_Headers"></a>

### Headers


```csharp
public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }
```


Provider headers, resolved on every call. When null, the model sends the `x-api-key` read from
`PARALLEL_API_KEY` at call time.

**Value:** [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/DecisionParallelModel.cs#L27)

<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_HeadersAsync_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_HeadersAsync"></a>

### HeadersAsync


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string>>>? HeadersAsync { get; set; }
```


Provider headers that need an asynchronous step, such as an async API key. Used when [Headers](#Vercel_AI_Parallel_ParallelDecisionModelConfig_Headers) is null.
Not serializable: `Serialize` throws [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/), as upstream does for an async `headers`.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/DecisionParallelModel.cs#L33)

<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_HttpClient_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient? HttpClient { get; set; }
```


HTTP client. Not serialized.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/DecisionParallelModel.cs#L39)

<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_Provider_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; set; }
```


Provider id, such as `parallel.decision`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/DecisionParallelModel.cs#L18)
