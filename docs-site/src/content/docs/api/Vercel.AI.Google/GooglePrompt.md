---
title: "GooglePrompt"
description: "Gemini contents plus an optional system instruction."
editUrl: false
prev: false
next: false
sidebar:
  label: "GooglePrompt"
  hidden: true
---

<a id="Vercel_AI_Google_GooglePrompt"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GooglePrompt
```


Gemini `contents` plus an optional system instruction.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L39)

## Constructors

<a id="Vercel_AI_Google_GooglePrompt__ctor_"></a>
<a id="Vercel_AI_Google_GooglePrompt__ctor_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonObject_"></a>

### GooglePrompt(JsonArray, JsonObject?)


```csharp
public GooglePrompt(JsonArray contents, JsonObject? systemInstruction)
```


Creates a prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `contents` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `systemInstruction` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L42)

## Properties

<a id="Vercel_AI_Google_GooglePrompt_Contents_"></a>
<a id="Vercel_AI_Google_GooglePrompt_Contents"></a>

### Contents


```csharp
public JsonArray Contents { get; }
```


Conversation contents.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L49)

<a id="Vercel_AI_Google_GooglePrompt_SystemInstruction_"></a>
<a id="Vercel_AI_Google_GooglePrompt_SystemInstruction"></a>

### SystemInstruction


```csharp
public JsonObject? SystemInstruction { get; }
```


System instruction, omitted for Gemma.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L52)
