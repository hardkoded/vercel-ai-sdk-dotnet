---
title: "GoogleToolPreparation"
description: "Tools and toolConfig prepared for a generateContent request."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolPreparation"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolPreparation"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolPreparation
```


Tools and `toolConfig` prepared for a generateContent request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L33)

## Constructors

<a id="Vercel_AI_Google_GoogleToolPreparation__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolPreparation__ctor_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleWarning__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `GoogleToolPreparation(JsonArray?, JsonObject?, IReadOnlyList<GoogleWarning>, IReadOnlyDictionary<string, string>)`


```csharp
public GoogleToolPreparation(JsonArray? tools, JsonObject? toolConfig, IReadOnlyList<GoogleWarning> warnings, IReadOnlyDictionary<string, string> names)
```


Creates a preparation result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `toolConfig` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `names` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L36)

## Properties

<a id="Vercel_AI_Google_GoogleToolPreparation_Names_"></a>
<a id="Vercel_AI_Google_GoogleToolPreparation_Names"></a>

### Names


```csharp
public IReadOnlyDictionary<string, string> Names { get; }
```


Maps a built-in wire name such as `code_execution` onto the caller's tool name.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L54)

<a id="Vercel_AI_Google_GoogleToolPreparation_ToolConfig_"></a>
<a id="Vercel_AI_Google_GoogleToolPreparation_ToolConfig"></a>

### ToolConfig


```csharp
public JsonObject? ToolConfig { get; }
```


Function-calling configuration.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L48)

<a id="Vercel_AI_Google_GoogleToolPreparation_Tools_"></a>
<a id="Vercel_AI_Google_GoogleToolPreparation_Tools"></a>

### Tools


```csharp
public JsonArray? Tools { get; }
```


Gemini tools array.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L45)

<a id="Vercel_AI_Google_GoogleToolPreparation_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleToolPreparation_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings for unsupported combinations.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L51)

## Methods

<a id="Vercel_AI_Google_GoogleToolPreparation_CustomName_"></a>
<a id="Vercel_AI_Google_GoogleToolPreparation_CustomName_System_String_"></a>

### CustomName(string)


```csharp
public string CustomName(string wireName)
```


Returns the caller name for a built-in tool, or the wire name.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `wireName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L57)
