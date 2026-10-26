---
title: "GoogleMessages"
description: "Converts prompts into Gemini contents."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleMessages"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleMessages"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleMessages
```


Converts prompts into Gemini contents.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L308)

## Fields

<a id="Vercel_AI_Google_GoogleMessages_SkipThoughtSignatureValidator"></a>

### SkipThoughtSignatureValidator


```csharp
public const string SkipThoughtSignatureValidator = "skip_thought_signature_validator"
```


Sentinel Gemini documents for replaying a function call whose thought signature was dropped.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L311)

## Methods

<a id="Vercel_AI_Google_GoogleMessages_Convert_"></a>
<a id="Vercel_AI_Google_GoogleMessages_Convert_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleTurn__Vercel_AI_Google_GoogleMessageOptions_"></a>

### `Convert(IReadOnlyList<GoogleTurn>, GoogleMessageOptions?)`


```csharp
public static GooglePrompt Convert(IReadOnlyList<GoogleTurn> turns, GoogleMessageOptions? options = null)
```


Converts rich turns.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `turns` | [IReadOnlyList&lt;GoogleTurn&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `options` | [GoogleMessageOptions](/api/Vercel.AI.Google/GoogleMessageOptions/) |  |

**Returns:** [GooglePrompt](/api/Vercel.AI.Google/GooglePrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L320)

<a id="Vercel_AI_Google_GoogleMessages_Convert_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__Vercel_AI_Google_GoogleMessageOptions_"></a>

### `Convert(IReadOnlyList<ModelMessage>, GoogleMessageOptions?)`


```csharp
public static GooglePrompt Convert(IReadOnlyList<ModelMessage> prompt, GoogleMessageOptions? options = null)
```


Converts shared prompt messages.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `options` | [GoogleMessageOptions](/api/Vercel.AI.Google/GoogleMessageOptions/) |  |

**Returns:** [GooglePrompt](/api/Vercel.AI.Google/GooglePrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L314)
