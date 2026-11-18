---
title: "ZaiThinking"
description: "Controls model thinking. Maps to the thinking option of ZaiLanguageModelChatOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ZaiThinking"
  hidden: true
---

<a id="Vercel_AI_Zai_ZaiThinking"></a>

Namespace: [Vercel.AI.Zai](/api/Vercel.AI.Zai/)


```csharp
public sealed class ZaiThinking
```


Controls model thinking. Maps to the `thinking` option of `ZaiLanguageModelChatOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L47)

## Properties

<a id="Vercel_AI_Zai_ZaiThinking_ClearThinking_"></a>
<a id="Vercel_AI_Zai_ZaiThinking_ClearThinking"></a>

### ClearThinking


```csharp
public bool? ClearThinking { get; set; }
```


Whether reasoning from earlier turns is cleared.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L53)

<a id="Vercel_AI_Zai_ZaiThinking_Type_"></a>
<a id="Vercel_AI_Zai_ZaiThinking_Type"></a>

### Type


```csharp
public ZaiThinkingType? Type { get; set; }
```


Enables or disables thinking.

**Value:** `ZaiThinkingType?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L50)
