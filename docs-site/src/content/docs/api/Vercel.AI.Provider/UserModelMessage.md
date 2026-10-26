---
title: "UserModelMessage"
description: "A user message."
editUrl: false
prev: false
next: false
sidebar:
  label: "UserModelMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_UserModelMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class UserModelMessage : ModelMessage
```


A user message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L730)

## Constructors

<a id="Vercel_AI_Provider_UserModelMessage__ctor_"></a>
<a id="Vercel_AI_Provider_UserModelMessage__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_UserContentPart__"></a>

### `UserModelMessage(IReadOnlyList<UserContentPart>)`


```csharp
public UserModelMessage(IReadOnlyList<UserContentPart> content)
```


Creates a user message from parts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [IReadOnlyList&lt;UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L739)

<a id="Vercel_AI_Provider_UserModelMessage__ctor_System_String_"></a>

### UserModelMessage(string)


```csharp
public UserModelMessage(string text)
```


Creates a user message from text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L733)

## Properties

<a id="Vercel_AI_Provider_UserModelMessage_Content_"></a>
<a id="Vercel_AI_Provider_UserModelMessage_Content"></a>

### Content


```csharp
public IReadOnlyList<UserContentPart> Content { get; }
```


User content parts.

**Value:** [IReadOnlyList&lt;UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L746)

<a id="Vercel_AI_Provider_UserModelMessage_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_UserModelMessage_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this message, such as an Amazon Bedrock `cachePoint`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L749)
