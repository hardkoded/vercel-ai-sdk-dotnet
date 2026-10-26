---
title: "OpenResponsesToolContext"
description: "Input of EncodeTool and EncodeToolChoice."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesToolContext"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesToolContext
```


Input of [EncodeTool](/api/Vercel.AI.OpenResponses/OpenResponsesExtension/#Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeTool) and [EncodeToolChoice](/api/Vercel.AI.OpenResponses/OpenResponsesExtension/#Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeToolChoice).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L13)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext__ctor_System_String_System_Text_Json_JsonElement_"></a>

### OpenResponsesToolContext(string, JsonElement)


```csharp
public OpenResponsesToolContext(string name, JsonElement args)
```


Creates the context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `args` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L16)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext_Args_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext_Args"></a>

### Args


```csharp
public JsonElement Args { get; }
```


Provider-tool arguments, a JSON object.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L26)

<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext_Name_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesToolContext_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L23)
