---
title: "TypeSafeAiDecisionModelConfig"
description: "Settings for a TypeSafeAiDecisionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeSafeAiDecisionModelConfig"
  hidden: true
---

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig"></a>

Namespace: [Vercel.AI.TypeSafeAi](/api/Vercel.AI.TypeSafeAi/)


```csharp
public class TypeSafeAiDecisionModelConfig
```


Settings for a [TypeSafeAiDecisionModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [TypeSafeAiEvaluationModelConfig](/api/Vercel.AI.TypeSafeAi/TypeSafeAiEvaluationModelConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L16)

## Properties

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API base URL, without a trailing slash.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L22)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_Headers_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_Headers"></a>

### Headers


```csharp
public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }
```


Provider headers, resolved on every call. When null, the model sends a bearer token read from
`TYPESAFE_AI_API_KEY` at call time.

**Value:** [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L28)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_HttpClient_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient? HttpClient { get; set; }
```


HTTP client. Not serialized.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L31)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_Provider_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; set; }
```


Provider id, such as `typesafe.decision`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L19)
