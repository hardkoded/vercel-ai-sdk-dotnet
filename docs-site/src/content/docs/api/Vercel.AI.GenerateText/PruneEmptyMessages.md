---
title: "PruneEmptyMessages"
description: "Whether PruneMessages(PruneMessagesOptions) keeps messages whose content is empty after pruning."
editUrl: false
prev: false
next: false
sidebar:
  label: "PruneEmptyMessages"
  hidden: true
---

<a id="Vercel_AI_GenerateText_PruneEmptyMessages"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public enum PruneEmptyMessages
```


Whether [PruneMessages(PruneMessagesOptions)](/api/Vercel.AI.GenerateText/MessagePruning/#Vercel_AI_GenerateText_MessagePruning_PruneMessages_Vercel_AI_GenerateText_PruneMessagesOptions_) keeps messages whose content is empty after pruning.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L23)

## Fields

<a id="Vercel_AI_GenerateText_PruneEmptyMessages_Keep"></a>

### Keep


```csharp
Keep = 1
```


Keep messages whose content is empty.

**Value:** [PruneEmptyMessages](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L29)

<a id="Vercel_AI_GenerateText_PruneEmptyMessages_Remove"></a>

### Remove


```csharp
Remove = 0
```


Remove messages whose content is empty.

**Value:** [PruneEmptyMessages](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L26)
