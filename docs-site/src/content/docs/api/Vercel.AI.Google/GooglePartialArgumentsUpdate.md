---
title: "GooglePartialArgumentsUpdate"
description: "The object and text produced by one GoogleJsonAccumulator update."
editUrl: false
prev: false
next: false
sidebar:
  label: "GooglePartialArgumentsUpdate"
  hidden: true
---

<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GooglePartialArgumentsUpdate
```


The object and text produced by one [GoogleJsonAccumulator](/api/Vercel.AI.Google/GoogleJsonAccumulator/) update.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L38)

## Constructors

<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate__ctor_"></a>
<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate__ctor_System_Text_Json_Nodes_JsonObject_System_String_"></a>

### GooglePartialArgumentsUpdate(JsonObject, string)


```csharp
public GooglePartialArgumentsUpdate(JsonObject current, string textDelta)
```


Creates an update.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `current` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `textDelta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L41)

## Properties

<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate_Current_"></a>
<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate_Current"></a>

### Current


```csharp
public JsonObject Current { get; }
```


Arguments accumulated so far.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L48)

<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate_TextDelta_"></a>
<a id="Vercel_AI_Google_GooglePartialArgumentsUpdate_TextDelta"></a>

### TextDelta


```csharp
public string TextDelta { get; }
```


JSON text emitted for this chunk.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L51)
