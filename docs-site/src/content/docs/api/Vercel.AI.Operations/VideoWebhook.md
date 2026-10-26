---
title: "VideoWebhook"
description: "Webhook notification."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoWebhook"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoWebhook"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoWebhook
```


Webhook notification.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L341)

## Constructors

<a id="Vercel_AI_Operations_VideoWebhook__ctor_"></a>
<a id="Vercel_AI_Operations_VideoWebhook__ctor_System_String_System_Threading_Tasks_Task_"></a>

### VideoWebhook(string, Task)


```csharp
public VideoWebhook(string url, Task received)
```


Creates a webhook.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `received` | [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L344)

## Properties

<a id="Vercel_AI_Operations_VideoWebhook_Received_"></a>
<a id="Vercel_AI_Operations_VideoWebhook_Received"></a>

### Received


```csharp
public Task Received { get; }
```


Completes when the notification arrives.

**Value:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L354)

<a id="Vercel_AI_Operations_VideoWebhook_Url_"></a>
<a id="Vercel_AI_Operations_VideoWebhook_Url"></a>

### Url


```csharp
public string Url { get; }
```


URL the provider should call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L351)
