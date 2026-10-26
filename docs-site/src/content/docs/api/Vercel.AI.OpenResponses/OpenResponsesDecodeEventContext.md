---
title: "OpenResponsesDecodeEventContext"
description: "Input of DecodeEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesDecodeEventContext"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesDecodeEventContext
```


Input of [DecodeEvent](/api/Vercel.AI.OpenResponses/OpenResponsesExtension/#Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeEvent).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L74)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IDictionary_System_String_System_Object__"></a>

### `OpenResponsesDecodeEventContext(JsonObject, IDictionary<string, object?>)`


```csharp
public OpenResponsesDecodeEventContext(JsonObject @event, IDictionary<string, object?> state)
```


Creates the context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `event` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `state` | [IDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L77)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext_Event_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext_Event"></a>

### Event


```csharp
public JsonObject Event { get; }
```


The event. It has a namespaced `type` and a numeric `sequence_number`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L84)

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext_State_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeEventContext_State"></a>

### State


```csharp
public IDictionary<string, object?> State { get; }
```


State that lasts for one response stream.

**Value:** [IDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L87)
