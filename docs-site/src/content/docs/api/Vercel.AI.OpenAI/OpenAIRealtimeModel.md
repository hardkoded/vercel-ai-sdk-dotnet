---
title: "OpenAIRealtimeModel"
description: "OpenAI Realtime session URI builder and WebSocket client."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModel : IRealtimeModel
```


OpenAI Realtime session URI builder and WebSocket client.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L325)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIRealtimeModel(OpenAIProvider, string)


```csharp
public OpenAIRealtimeModel(OpenAIProvider provider, string modelId)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L330)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L340)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L337)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L343)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

### ConnectAsync(CancellationToken)


```csharp
public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
```


Opens a realtime session.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IRealtimeSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L390)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_CreateClientSecretAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_CreateClientSecretAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(int?, CancellationToken)


```csharp
public Task<OpenAIRealtimeClientSecret> CreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Requests a realtime client secret. &lt;code class="paramref"&gt;expiresAfterSeconds&lt;/code&gt; is omitted when null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L351)
