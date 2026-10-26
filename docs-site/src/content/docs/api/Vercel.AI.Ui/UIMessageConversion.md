---
title: "UIMessageConversion"
description: "Conversion of UI messages to model messages. Maps to convertToModelMessages."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageConversion"
  hidden: true
---

<a id="Vercel_AI_Ui_UIMessageConversion"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public static class UIMessageConversion
```


Conversion of UI messages to model messages. Maps to `convertToModelMessages`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ConvertToModelMessages.cs#L25)

## Methods

<a id="Vercel_AI_Ui_UIMessageConversion_ConvertToModelMessages_"></a>
<a id="Vercel_AI_Ui_UIMessageConversion_ConvertToModelMessages_System_Collections_Generic_IReadOnlyList_Vercel_AI_Ui_UIMessage__Vercel_AI_Ui_ConvertToModelMessagesOptions_"></a>

### `ConvertToModelMessages(IReadOnlyList<UIMessage>, ConvertToModelMessagesOptions?)`


```csharp
public static IReadOnlyList<ModelMessage> ConvertToModelMessages(IReadOnlyList<UIMessage> messages, ConvertToModelMessagesOptions? options = null)
```


Converts UI messages from a chat UI into model messages that can be used with the AI functions.
A tool message upstream holds several parts. Here its parts are consecutive tool messages.
Assistant file and reasoning-file parts are dropped, as assistant messages here cannot hold files.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | The UI messages to convert. |
| `options` | [ConvertToModelMessagesOptions](/api/Vercel.AI.Ui/ConvertToModelMessagesOptions/) | Conversion options. |

**Returns:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) — The model messages.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ConvertToModelMessages.cs#L35)
