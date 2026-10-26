---
title: "UIMessageChunks"
description: "Reads AI SDK UI message chunks, including fields added by a newer server."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageChunks"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageChunks"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public static class UIMessageChunks
```


Reads AI SDK UI message chunks, including fields added by a newer server.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunks.cs#L21)

## Methods

<a id="Vercel_AI_AspNetCore_UIMessageChunks_Parse_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageChunks_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static JsonObject Parse(JsonElement value)
```


Parses one chunk object. Extra properties on a known type are kept.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) | JSON object for one chunk. |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunks.cs#L68)

<a id="Vercel_AI_AspNetCore_UIMessageChunks_ParseSse_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageChunks_ParseSse_System_String_"></a>

### ParseSse(string)


```csharp
public static IReadOnlyList<JsonObject> ParseSse(string sse)
```


Parses `data:` SSE frames into chunks. `data: [DONE]` ends the stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sse` | [string](https://learn.microsoft.com/dotnet/api/system.string) | SSE payload. |

**Returns:** [IReadOnlyList&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunks.cs#L92)
