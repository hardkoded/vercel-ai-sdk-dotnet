---
title: "GoogleInteractionsOutput"
description: "Reads text and function calls from an Interactions response."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleInteractionsOutput"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleInteractionsOutput"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleInteractionsOutput
```


Reads text and function calls from an Interactions response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L118)

## Methods

<a id="Vercel_AI_Google_GoogleInteractionsOutput_Parse_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsOutput_Parse_System_Text_Json_Nodes_JsonObject_"></a>

### Parse(JsonObject)


```csharp
public static (IReadOnlyList<GeneratedContent> Content, bool HasFunctionCall) Parse(JsonObject response)
```


Parses `steps` into generated content and reports whether a client function call is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [(IReadOnlyList&lt;GeneratedContent&gt; Content, bool HasFunctionCall)](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L121)
