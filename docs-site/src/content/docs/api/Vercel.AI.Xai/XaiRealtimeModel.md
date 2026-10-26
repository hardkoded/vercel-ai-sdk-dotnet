---
title: "XaiRealtimeModel"
description: "xAI realtime voice model."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiRealtimeModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiRealtimeModel : IRealtimeModel
```


xAI realtime voice model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L34)

## Constructors

<a id="Vercel_AI_Xai_XaiRealtimeModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel__ctor_System_String_System_Net_Http_HttpClient_System_String_System_Func_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String___"></a>

### `XaiRealtimeModel(string, HttpClient, string, Func<IReadOnlyDictionary<string, string?>>?)`


```csharp
public XaiRealtimeModel(string modelId, HttpClient httpClient, string baseUrl, Func<IReadOnlyDictionary<string, string?>>? headers)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L41)

## Properties

<a id="Vercel_AI_Xai_XaiRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L56)

<a id="Vercel_AI_Xai_XaiRealtimeModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L50)

## Methods

<a id="Vercel_AI_Xai_XaiRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L59)

<a id="Vercel_AI_Xai_XaiRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

### ConnectAsync(CancellationToken)


```csharp
public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
```


WebSocket sessions are opened by the caller with [Url](/api/Vercel.AI.Xai/XaiClientSecret/#Vercel_AI_Xai_XaiClientSecret_Url).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IRealtimeSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L65)

<a id="Vercel_AI_Xai_XaiRealtimeModel_CreateClientSecretAsync_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_CreateClientSecretAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(int?, CancellationToken)


```csharp
public Task<XaiClientSecret> CreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Posts to `/realtime/client_secrets` and returns a WebSocket URL that includes the model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;XaiClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L71)

<a id="Vercel_AI_Xai_XaiRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_SerializeClientEvent_System_String_"></a>

### SerializeClientEvent(string)


```csharp
public string? SerializeClientEvent(string type)
```


Serializes a client event. `conversation-item-truncate` is dropped because xAI ignores it over WebSocket.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L104)
