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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L52)

<a id="Vercel_AI_Anthropic_AnthropicStream__ctor_System_Boolean_System_Boolean_System_String_System_Boolean_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `AnthropicStream(bool, bool, string, bool, IReadOnlyDictionary<string, string>?)`


```csharp
public AnthropicStream(bool includeRawChunks, bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey, IReadOnlyDictionary<string, string>? toolNames = null)
```


Creates a stream reader that mirrors the finish metadata under a custom provider key.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `includeRawChunks` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `usesJsonResponseTool` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usedCustomProviderKey` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `toolNames` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L56)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicStream_ComputerToolsetName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream_ComputerToolsetName"></a>

### ComputerToolsetName


```csharp
public string? ComputerToolsetName { get; init; }
```


The caller's name for the declared computer toolset. Defaults to `computer`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L44)

<a id="Vercel_AI_Anthropic_AnthropicStream_IncludePings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream_IncludePings"></a>

### IncludePings


```csharp
public bool IncludePings { get; init; }
```


Emits a [PingStreamPart](/api/Vercel.AI.Provider/PingStreamPart/) for each `ping` event. Maps to `includePings`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L38)

<a id="Vercel_AI_Anthropic_AnthropicStream_MarkCodeExecutionDynamic_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream_MarkCodeExecutionDynamic"></a>

### MarkCodeExecutionDynamic


```csharp
public bool MarkCodeExecutionDynamic { get; init; }
```


Marks `code_execution` server tool calls as dynamic. Set when a web tool that dynamic filtering runs through code execution
is sent without the code execution tool, which the API then allows implicitly.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L35)

<a id="Vercel_AI_Anthropic_AnthropicStream_PreparedTools_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStream_PreparedTools"></a>

### PreparedTools


```csharp
public JsonArray? PreparedTools { get; init; }
```


The effective prepared tool definitions that computer toolset member calls are checked against.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L41)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L545)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStream.cs#L77)
