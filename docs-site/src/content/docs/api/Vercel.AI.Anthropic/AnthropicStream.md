---
title: "AnthropicStream"
description: "Turns Anthropic SSE JSON events into SDK stream parts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicStream"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicStream"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicStream
```


Turns Anthropic SSE JSON events into SDK stream parts.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L14)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicStream__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream__ctor_System_Boolean_System_Boolean_"></a>

### AnthropicStream(bool, bool)


```csharp
public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool)
```


Creates a stream reader.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `includeRawChunks` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `usesJsonResponseTool` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L34)

<a id="Vercel_AI_Anthropic_AnthropicStream__ctor_System_Boolean_System_Boolean_System_String_System_Boolean_"></a>

### AnthropicStream(bool, bool, string, bool)


```csharp
public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey)
```


Creates a stream reader that mirrors the finish metadata under a custom provider key.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `includeRawChunks` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `usesJsonResponseTool` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usedCustomProviderKey` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L40)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicStream_Complete_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream_Complete"></a>

### Complete()


```csharp
public IEnumerable<LanguageModelStreamPart> Complete()
```


Ends the stream. Returns an interrupted finish part when the events ended cleanly after output
without a `message_stop` for the last message. A stream without output, or with an error,
gets no finish part so the existing no-output and error paths apply.

**Returns:** [IEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L393)

<a id="Vercel_AI_Anthropic_AnthropicStream_Push_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream_Push_System_String_"></a>

### Push(string)


```csharp
public IEnumerable<LanguageModelStreamPart> Push(string json)
```


Reads one SSE data payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L49)
