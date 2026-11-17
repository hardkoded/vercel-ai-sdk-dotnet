---
title: "OpenAIRealtimeModelLiveOptions"
description: "Experimental Live options under sessionConfig.providerOptions.openai. Maps to OpenAIRealtimeModelLiveOptions. ToJsonObject() builds the provider options value."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModelLiveOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModelLiveOptions
```


Experimental Live options under `sessionConfig.providerOptions.openai`. Maps to
`OpenAIRealtimeModelLiveOptions`. [ToJsonObject()](#Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_ToJsonObject) builds the provider options value.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L41)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_Client_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_Client"></a>

### Client


```csharp
public OpenAIRealtimeModelLiveDataChannel? Client { get; set; }
```


Client permissions; valid for WebRTC startup only. Maps to `client.dataChannel`.

**Value:** [OpenAIRealtimeModelLiveDataChannel](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveDataChannel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L44)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_ClientDelegation_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_ClientDelegation"></a>

### ClientDelegation


```csharp
public bool ClientDelegation { get; set; }
```


True for `delegation: { type: 'client' }`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L47)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_DisableDelegation_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_DisableDelegation"></a>

### DisableDelegation


```csharp
public bool DisableDelegation { get; set; }
```


True for an explicit `delegation: null`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L50)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_Input_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_Input"></a>

### Input


```csharp
public IReadOnlyList<JsonObject>? Input { get; set; }
```


Startup history messages (at most 128).

**Value:** [IReadOnlyList&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L53)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_Store_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_Store"></a>

### Store


```csharp
public bool? Store { get; set; }
```


Whether the provider stores the session.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L56)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_VoiceId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_VoiceId"></a>

### VoiceId


```csharp
public string? VoiceId { get; set; }
```


Custom voice id. Maps to `voice: { id }`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L59)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_ToJsonObject_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveOptions_ToJsonObject"></a>

### ToJsonObject()


```csharp
public JsonObject ToJsonObject()
```


Builds the JSON value for `providerOptions.openai`.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L62)
