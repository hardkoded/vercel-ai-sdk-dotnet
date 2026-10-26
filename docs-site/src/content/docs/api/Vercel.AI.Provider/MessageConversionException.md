---
title: "MessageConversionException"
description: "A UI message could not be converted to a model message. Maps to MessageConversionError."
editUrl: false
prev: false
next: false
sidebar:
  label: "MessageConversionException"
  hidden: true
---

<a id="Vercel_AI_Provider_MessageConversionException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class MessageConversionException : AiSdkException, ISerializable
```


A UI message could not be converted to a model message. Maps to `MessageConversionError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L183)

## Constructors

<a id="Vercel_AI_Provider_MessageConversionException__ctor_"></a>
<a id="Vercel_AI_Provider_MessageConversionException__ctor_Vercel_AI_Ui_UIMessage_System_String_"></a>

### MessageConversionException(UIMessage, string)


```csharp
public MessageConversionException(UIMessage originalMessage, string message)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `originalMessage` | [UIMessage](/api/Vercel.AI.Ui/UIMessage/) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L186)

## Properties

<a id="Vercel_AI_Provider_MessageConversionException_OriginalMessage_"></a>
<a id="Vercel_AI_Provider_MessageConversionException_OriginalMessage"></a>

### OriginalMessage


```csharp
public UIMessage OriginalMessage { get; }
```


The message that could not be converted.

**Value:** [UIMessage](/api/Vercel.AI.Ui/UIMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L193)
