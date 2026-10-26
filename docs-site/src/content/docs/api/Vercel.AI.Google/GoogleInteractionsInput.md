---
title: "GoogleInteractionsInput"
description: "Converts a prompt into an Interactions input array."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleInteractionsInput"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleInteractionsInput"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleInteractionsInput
```


Converts a prompt into an Interactions `input` array.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L37)

## Methods

<a id="Vercel_AI_Google_GoogleInteractionsInput_Convert_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsInput_Convert_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `Convert(IReadOnlyList<ModelMessage>)`


```csharp
public static JsonObject Convert(IReadOnlyList<ModelMessage> prompt)
```


Converts shared messages. System text is joined into `systemInstruction`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L40)
