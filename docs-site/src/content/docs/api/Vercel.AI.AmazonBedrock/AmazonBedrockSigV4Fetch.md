---
title: "AmazonBedrockSigV4Fetch"
description: "SigV4 fetch wrapper. The credentials are read when each call is sent."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockSigV4Fetch"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockSigV4Fetch
```


SigV4 fetch wrapper. The credentials are read when each call is sent.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L181)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch__ctor_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_AmazonBedrock_AmazonBedrockCredentials___System_String_"></a>

### `AmazonBedrockSigV4Fetch(Func<CancellationToken, Task<AmazonBedrockCredentials>>, string)`


```csharp
public AmazonBedrockSigV4Fetch(Func<CancellationToken, Task<AmazonBedrockCredentials>> getCredentials, string service = "bedrock")
```


Creates a wrapper that signs for &lt;code class="paramref"&gt;service&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `getCredentials` | [Func&lt;CancellationToken, Task&lt;AmazonBedrockCredentials&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `service` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L187)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_Transport_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_Transport"></a>

### Transport


```csharp
public Func<AmazonBedrockPreparedCall, AmazonBedrockPreparedCall>? Transport { get; set; }
```


Optional transport. When unset, the prepared call is returned directly.

**Value:** [Func&lt;AmazonBedrockPreparedCall, AmazonBedrockPreparedCall&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L194)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_UtcNow_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_UtcNow"></a>

### UtcNow


```csharp
public Func<DateTimeOffset>? UtcNow { get; set; }
```


Clock used for signing. Tests can pin this.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L197)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_SendAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_SendAsync_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SendAsync(string, string?, string?, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<AmazonBedrockPreparedCall> SendAsync(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken = default)
```


Resolves the credentials, signs the call, then invokes [Transport](#Vercel_AI_AmazonBedrock_AmazonBedrockSigV4Fetch_Transport) if one is set.
Calls that are not signed never read the credentials. A credential failure propagates and the transport is not called.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AmazonBedrockPreparedCall&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L203)
