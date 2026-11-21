---
title: "JsonResponseHandler<T>"
description: "Handles a JSON response. Maps to the ResponseHandler that createJsonResponseHandler returns."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonResponseHandler<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonResponseHandler_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public delegate Task<JsonResponseHandlerResult<T>> JsonResponseHandler<T>(HttpResponseMessage response, string url, object? requestBodyValues, CancellationToken cancellationToken)
```


Handles a JSON response. Maps to the `ResponseHandler` that `createJsonResponseHandler` returns.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;JsonResponseHandlerResult&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L56)
