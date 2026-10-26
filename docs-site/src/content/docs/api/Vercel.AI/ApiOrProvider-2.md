---
title: "ApiOrProvider<TApi, TProvider>"
description: "An API or a provider that exposes one. Maps to the upstream Api | ProviderV4 unions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiOrProvider<TApi, TProvider>"
  hidden: true
---

<a id="Vercel_AI_ApiOrProvider_2"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ApiOrProvider<TApi, TProvider> where TApi : class where TProvider : class
```


An API or a provider that exposes one. Maps to the upstream `Api | ProviderV4` unions.

**Type parameters**

| Name | Description |
| --- | --- |
| `TApi` | The API type. |
| `TProvider` | The provider type that exposes the API. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L10)

## Properties

<a id="Vercel_AI_ApiOrProvider_2_Api_"></a>
<a id="Vercel_AI_ApiOrProvider_2_Api"></a>

### Api


```csharp
public TApi? Api { get; }
```


The API, when this wraps one.

**Value:** `TApi`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L21)

<a id="Vercel_AI_ApiOrProvider_2_Provider_"></a>
<a id="Vercel_AI_ApiOrProvider_2_Provider"></a>

### Provider


```csharp
public TProvider? Provider { get; }
```


The provider, when this wraps one.

**Value:** `TProvider`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L24)

## Methods

<a id="Vercel_AI_ApiOrProvider_2_FromApi_"></a>
<a id="Vercel_AI_ApiOrProvider_2_FromApi__0_"></a>

### FromApi(TApi)


```csharp
public static ApiOrProvider<TApi, TProvider> FromApi(TApi api)
```


Wraps an API.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `api` | `TApi` |  |

**Returns:** [ApiOrProvider&lt;TApi, TProvider&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L27)

<a id="Vercel_AI_ApiOrProvider_2_FromProvider_"></a>
<a id="Vercel_AI_ApiOrProvider_2_FromProvider__1_"></a>

### FromProvider(TProvider)


```csharp
public static ApiOrProvider<TApi, TProvider> FromProvider(TProvider provider)
```


Wraps a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | `TProvider` |  |

**Returns:** [ApiOrProvider&lt;TApi, TProvider&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L33)
