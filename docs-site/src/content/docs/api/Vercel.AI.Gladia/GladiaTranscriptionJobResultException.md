---
title: "GladiaTranscriptionJobResultException"
description: "Cause of the TranscriptionJobFailed error. Carries the Gladia result that upstream passes as cause. .NET-only: a JavaScript cause can be any value, but an Ex..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaTranscriptionJobResultException"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaTranscriptionJobResultException : Exception, ISerializable
```


Cause of the `TranscriptionJobFailed` error. Carries the Gladia result that upstream passes as `cause`.
.NET-only: a JavaScript `cause` can be any value, but an [Exception](https://learn.microsoft.com/dotnet/api/system.exception) cause must be an exception.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L21)

## Constructors

<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException__ctor_System_String_System_Text_Json_Nodes_JsonObject_System_Nullable_System_Int32__"></a>

### GladiaTranscriptionJobResultException(string, JsonObject, int?)


```csharp
public GladiaTranscriptionJobResultException(string message, JsonObject result, int? errorCode)
```


Creates the cause for a failed job.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `result` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `errorCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L24)

## Properties

<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException_ErrorCode_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException_ErrorCode"></a>

### ErrorCode


```csharp
public int? ErrorCode { get; }
```


The `error_code` of the result, when Gladia sent one.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L35)

<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException_Result_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionJobResultException_Result"></a>

### Result


```csharp
public JsonObject Result { get; }
```


The parsed Gladia result of the failed job.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L32)
