---
title: "GoogleSpeechPreparation"
description: "A prepared Gemini speech request."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechPreparation"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechPreparation"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSpeechPreparation
```


A prepared Gemini speech request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L273)

## Constructors

<a id="Vercel_AI_Google_GoogleSpeechPreparation__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleWarning__"></a>

### `GoogleSpeechPreparation(JsonObject, IReadOnlyList<GoogleWarning>)`


```csharp
public GoogleSpeechPreparation(JsonObject body, IReadOnlyList<GoogleWarning> warnings)
```


Creates a preparation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L276)

## Properties

<a id="Vercel_AI_Google_GoogleSpeechPreparation_Body_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


Request JSON.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L283)

<a id="Vercel_AI_Google_GoogleSpeechPreparation_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L286)
