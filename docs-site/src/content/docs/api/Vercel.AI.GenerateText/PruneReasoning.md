---
title: "PruneReasoning"
description: "How PruneMessages(PruneMessagesOptions) removes reasoning from assistant messages."
editUrl: false
prev: false
next: false
sidebar:
  label: "PruneReasoning"
  hidden: true
---

<a id="Vercel_AI_GenerateText_PruneReasoning"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public enum PruneReasoning
```


How [PruneMessages(PruneMessagesOptions)](/api/Vercel.AI.GenerateText/MessagePruning/#Vercel_AI_GenerateText_MessagePruning_PruneMessages_Vercel_AI_GenerateText_PruneMessagesOptions_) removes reasoning from assistant messages.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L10)

## Fields

<a id="Vercel_AI_GenerateText_PruneReasoning_All"></a>

### All


```csharp
All = 1
```


Remove reasoning from every assistant message.

**Value:** [PruneReasoning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L16)

<a id="Vercel_AI_GenerateText_PruneReasoning_BeforeLastMessage"></a>

### BeforeLastMessage


```csharp
BeforeLastMessage = 2
```


Remove reasoning from every assistant message except the last message.

**Value:** [PruneReasoning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L19)

<a id="Vercel_AI_GenerateText_PruneReasoning_None"></a>

### None


```csharp
None = 0
```


Keep all reasoning.

**Value:** [PruneReasoning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L13)
