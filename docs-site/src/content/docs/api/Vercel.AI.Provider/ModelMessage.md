---
title: "ModelMessage"
description: "A prompt message."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_ModelMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class ModelMessage
```


A prompt message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AssistantModelMessage](/api/Vercel.AI.Provider/AssistantModelMessage/), [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/), [ToolApprovalResponseModelMessage](/api/Vercel.AI.Provider/ToolApprovalResponseModelMessage/), [ToolModelMessage](/api/Vercel.AI.Provider/ToolModelMessage/), [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L604)

## Constructors

<a id="Vercel_AI_Provider_ModelMessage__ctor_"></a>
<a id="Vercel_AI_Provider_ModelMessage__ctor_System_String_"></a>

### ModelMessage(string)


```csharp
protected ModelMessage(string role)
```


Creates a message with a role.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `role` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L607)

## Properties

<a id="Vercel_AI_Provider_ModelMessage_Role_"></a>
<a id="Vercel_AI_Provider_ModelMessage_Role"></a>

### Role


```csharp
public string Role { get; }
```


Message role.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L613)
