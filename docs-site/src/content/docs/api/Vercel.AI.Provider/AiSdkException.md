---
title: "AiSdkException"
description: "Base exception for the community .NET port of the Vercel AI SDK."
editUrl: false
prev: false
next: false
sidebar:
  label: "AiSdkException"
  hidden: true
---

<a id="Vercel_AI_Provider_AiSdkException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class AiSdkException : Exception, ISerializable
```


Base exception for the community .NET port of the Vercel AI SDK.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

**Derived:** [GatewayError](/api/Vercel.AI.Gateway/GatewayError/), [DownloadException](/api/Vercel.AI.GenerateText/DownloadException/), [InvalidToolApprovalException](/api/Vercel.AI.GenerateText/InvalidToolApprovalException/), [NoOutputGeneratedException](/api/Vercel.AI.GenerateText/NoOutputGeneratedException/), [ToolCallNotFoundForApprovalException](/api/Vercel.AI.GenerateText/ToolCallNotFoundForApprovalException/), [ToolChoiceViolationException](/api/Vercel.AI.GenerateText/ToolChoiceViolationException/), [TypeValidationException](/api/Vercel.AI.GenerateText/TypeValidationException/), [MCPClientError](/api/Vercel.AI.Mcp/MCPClientError/), [DecisionUnsupportedQuestionTypeException](/api/Vercel.AI.Operations/DecisionUnsupportedQuestionTypeException/), [DownloadException](/api/Vercel.AI.Operations/DownloadException/), [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/), [InvalidResponseDataException](/api/Vercel.AI.Operations/InvalidResponseDataException/), [NoImageGeneratedException](/api/Vercel.AI.Operations/NoImageGeneratedException/), [NoSpeechGeneratedException](/api/Vercel.AI.Operations/NoSpeechGeneratedException/), [NoTranscriptGeneratedException](/api/Vercel.AI.Operations/NoTranscriptGeneratedException/), [NoTranslationGeneratedException](/api/Vercel.AI.Operations/NoTranslationGeneratedException/), [NoVideoGeneratedException](/api/Vercel.AI.Operations/NoVideoGeneratedException/), [OperationRetryException](/api/Vercel.AI.Operations/OperationRetryException/), [RetryableCallException](/api/Vercel.AI.Operations/RetryableCallException/), [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/), [UnsupportedModelVersionException](/api/Vercel.AI.Operations/UnsupportedModelVersionException/), [InvalidDataContentException](/api/Vercel.AI.Prompt/InvalidDataContentException/), [InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/), [ApiConnectionException](/api/Vercel.AI.Provider/ApiConnectionException/), [ApiException](/api/Vercel.AI.Provider/ApiException/), [MessageConversionException](/api/Vercel.AI.Provider/MessageConversionException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L8)

## Constructors

<a id="Vercel_AI_Provider_AiSdkException__ctor_"></a>
<a id="Vercel_AI_Provider_AiSdkException__ctor_System_String_"></a>

### AiSdkException(string)


```csharp
public AiSdkException(string message)
```


Creates an exception with a message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L11)

<a id="Vercel_AI_Provider_AiSdkException__ctor_System_String_System_Exception_"></a>

### AiSdkException(string, Exception)


```csharp
public AiSdkException(string message, Exception innerException)
```


Creates an exception with a message and an inner exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L17)
