---
title: "OpenResponsesDecodeItemContext"
description: "Input of DecodeItem."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesDecodeItemContext"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesDecodeItemContext
```


Input of [DecodeItem](/api/Vercel.AI.OpenResponses/OpenResponsesExtension/#Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeItem).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L40)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext__ctor_System_Text_Json_Nodes_JsonObject_Vercel_AI_OpenResponses_OpenResponsesItemMode_"></a>

### OpenResponsesDecodeItemContext(JsonObject, OpenResponsesItemMode)


```csharp
public OpenResponsesDecodeItemContext(JsonObject item, OpenResponsesItemMode mode)
```


Creates the context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `item` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `mode` | [OpenResponsesItemMode](/api/Vercel.AI.OpenResponses/OpenResponsesItemMode/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L43)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext_Item_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext_Item"></a>

### Item


```csharp
public JsonObject Item { get; }
```


The completed item. It has a namespaced `type`, a string `id`, and a string `status`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L50)

<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext_Mode_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesDecodeItemContext_Mode"></a>

### Mode


```csharp
public OpenResponsesItemMode Mode { get; }
```


Where the item came from.

**Value:** [OpenResponsesItemMode](/api/Vercel.AI.OpenResponses/OpenResponsesItemMode/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L53)
