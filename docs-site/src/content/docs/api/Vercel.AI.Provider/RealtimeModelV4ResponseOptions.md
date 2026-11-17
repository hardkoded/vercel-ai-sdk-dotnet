---
title: "RealtimeModelV4ResponseOptions"
description: "Options of a response-create client event."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4ResponseOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4ResponseOptions
```


Options of a `response-create` client event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L239)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions_Instructions_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Instructions for this response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L245)

<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions_Metadata_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions_Metadata"></a>

### Metadata


```csharp
public JsonObject? Metadata { get; set; }
```


Metadata.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L248)

<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions_Modalities_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ResponseOptions_Modalities"></a>

### Modalities


```csharp
public IReadOnlyList<string>? Modalities { get; set; }
```


Output modalities.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L242)
