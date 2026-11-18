---
title: "GatewayAccessPolicyDeniedError"
description: "An AI Gateway access policy that applies to the request does not allow the requested model or provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayAccessPolicyDeniedError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayAccessPolicyDeniedError : GatewayError, ISerializable
```


An AI Gateway access policy that applies to the request does not allow the requested model or provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L277)

## Constructors

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError__ctor_System_String_System_Int32_System_Exception_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `GatewayAccessPolicyDeniedError(string?, int, Exception?, string?, IReadOnlyList<string>?, IReadOnlyList<string>?, IReadOnlyList<string>?)`


```csharp
public GatewayAccessPolicyDeniedError(string? message = null, int statusCode = 403, Exception? cause = null, string? generationId = null, IReadOnlyList<string>? policyIds = null, IReadOnlyList<string>? removedModels = null, IReadOnlyList<string>? removedProviders = null)
```


Creates an access policy denied error. The default message is `Access policy denied`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `policyIds` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `removedModels` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `removedProviders` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L280)

## Properties

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_PolicyIds_"></a>
<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_PolicyIds"></a>

### PolicyIds


```csharp
public IReadOnlyList<string>? PolicyIds { get; }
```


IDs of the access policies that denied the request, when the gateway includes them.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L300)

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_RemovedModels_"></a>
<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_RemovedModels"></a>

### RemovedModels


```csharp
public IReadOnlyList<string>? RemovedModels { get; }
```


Models the access policies removed, when the gateway includes them.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L303)

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_RemovedProviders_"></a>
<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_RemovedProviders"></a>

### RemovedProviders


```csharp
public IReadOnlyList<string>? RemovedProviders { get; }
```


Providers the access policies removed, when the gateway includes them.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L306)

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L297)

## Methods

<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayAccessPolicyDeniedError_IsInstance_System_Exception_"></a>

### IsInstance(Exception?)


```csharp
public static bool IsInstance(Exception? error)
```


True when &lt;code class="paramref"&gt;error&lt;/code&gt; is this error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L309)
