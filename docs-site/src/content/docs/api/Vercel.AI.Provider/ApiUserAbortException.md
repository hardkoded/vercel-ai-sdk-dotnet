---
title: "ApiUserAbortException"
description: "The call was cancelled through a CancellationToken. It derives from OperationCanceledException, so catch (OperationCanceledException) handles every caller ca..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiUserAbortException"
  hidden: true
---

<a id="Vercel_AI_Provider_ApiUserAbortException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ApiUserAbortException : OperationCanceledException, ISerializable
```


The call was cancelled through a [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken). It derives from [OperationCanceledException](https://learn.microsoft.com/dotnet/api/system.operationcanceledexception),
so `catch (OperationCanceledException)` handles every caller cancel. It is not an [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [SystemException](https://learn.microsoft.com/dotnet/api/system.systemexception) → [OperationCanceledException](https://learn.microsoft.com/dotnet/api/system.operationcanceledexception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L167)

## Constructors

<a id="Vercel_AI_Provider_ApiUserAbortException__ctor_"></a>
<a id="Vercel_AI_Provider_ApiUserAbortException__ctor"></a>

### ApiUserAbortException()


```csharp
public ApiUserAbortException()
```


Creates the exception.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L170)

<a id="Vercel_AI_Provider_ApiUserAbortException__ctor_System_Exception_System_Threading_CancellationToken_"></a>

### ApiUserAbortException(Exception?, CancellationToken)


```csharp
public ApiUserAbortException(Exception? innerException, CancellationToken cancellationToken)
```


Creates the exception for a cancelled &lt;code class="paramref"&gt;cancellationToken&lt;/code&gt;, with the original cancel as the cause.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L176)
