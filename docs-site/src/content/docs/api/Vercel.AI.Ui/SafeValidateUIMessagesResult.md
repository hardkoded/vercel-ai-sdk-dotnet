---
title: "SafeValidateUIMessagesResult"
description: "The result of SafeValidateUIMessagesAsync(ValidateUIMessagesOptions). Maps to SafeValidateUIMessagesResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "SafeValidateUIMessagesResult"
  hidden: true
---

<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class SafeValidateUIMessagesResult
```


The result of [SafeValidateUIMessagesAsync(ValidateUIMessagesOptions)](/api/Vercel.AI/Ai/#Vercel_AI_Ai_SafeValidateUIMessagesAsync_Vercel_AI_Ui_ValidateUIMessagesOptions_). Maps to `SafeValidateUIMessagesResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L46)

## Properties

<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult_Data_"></a>
<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult_Data"></a>

### Data


```csharp
public IReadOnlyList<UIMessage>? Data { get; }
```


The validated messages. Null on failure.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L63)

<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult_Error_"></a>
<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult_Error"></a>

### Error


```csharp
public Exception? Error { get; }
```


The validation error. Null on success.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L66)

<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult_Success_"></a>
<a id="Vercel_AI_Ui_SafeValidateUIMessagesResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when the messages are valid.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L60)
