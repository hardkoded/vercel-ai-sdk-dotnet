---
title: "EventSourceMessage"
description: "One Server-Sent Event. Field names are compared without case folding."
editUrl: false
prev: false
next: false
sidebar:
  label: "EventSourceMessage"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_EventSourceMessage"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class EventSourceMessage
```


One Server-Sent Event. Field names are compared without case folding.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L11)

## Constructors

<a id="Vercel_AI_ProviderUtils_EventSourceMessage__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_EventSourceMessage__ctor_System_String_System_String_System_String_"></a>

### EventSourceMessage(string, string?, string?)


```csharp
public EventSourceMessage(string data, string? eventType, string? id)
```


Creates a message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `eventType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L14)

## Properties

<a id="Vercel_AI_ProviderUtils_EventSourceMessage_Data_"></a>
<a id="Vercel_AI_ProviderUtils_EventSourceMessage_Data"></a>

### Data


```csharp
public string Data { get; }
```


Joined `data` fields.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L22)

<a id="Vercel_AI_ProviderUtils_EventSourceMessage_Event_"></a>
<a id="Vercel_AI_ProviderUtils_EventSourceMessage_Event"></a>

### Event


```csharp
public string? Event { get; }
```


Event type. Null when the stream did not set one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L25)

<a id="Vercel_AI_ProviderUtils_EventSourceMessage_Id_"></a>
<a id="Vercel_AI_ProviderUtils_EventSourceMessage_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Event id. Null when the stream did not set one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L28)
