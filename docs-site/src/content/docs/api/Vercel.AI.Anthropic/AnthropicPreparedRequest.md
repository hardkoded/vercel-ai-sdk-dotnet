---
title: "AnthropicPreparedRequest"
description: "A Messages request body plus the betas and warnings that go with it."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicPreparedRequest"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicPreparedRequest
```


A Messages request body plus the betas and warnings that go with it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L15)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicWarning__System_Collections_Generic_IReadOnlyList_System_String__System_Boolean_System_String_System_Boolean_System_Boolean_System_Text_Json_Nodes_JsonArray_System_String_System_Boolean_"></a>

### `AnthropicPreparedRequest(JsonObject, IReadOnlyList<AnthropicWarning>, IReadOnlyList<string>, bool, string, bool, bool, JsonArray?, string?, bool)`


```csharp
public AnthropicPreparedRequest(JsonObject body, IReadOnlyList<AnthropicWarning> warnings, IReadOnlyList<string> betas, bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey, bool markCodeExecutionDynamic = false, JsonArray? preparedTools = null, string? computerToolsetName = null, bool includePings = false)
```


Creates a prepared request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usesJsonResponseTool` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usedCustomProviderKey` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `markCodeExecutionDynamic` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `preparedTools` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `computerToolsetName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `includePings` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L18)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Betas_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Betas"></a>

### Betas


```csharp
public IReadOnlyList<string> Betas { get; }
```


Beta names for the `anthropic-beta` header.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L62)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Body_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


JSON body.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L47)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_ComputerToolsetName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_ComputerToolsetName"></a>

### ComputerToolsetName


```csharp
public string? ComputerToolsetName { get; }
```


The caller's name for the declared computer toolset, or null when none is declared.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L56)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_IncludePings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_IncludePings"></a>

### IncludePings


```csharp
public bool IncludePings { get; }
```


True when typed `ping` stream parts are emitted. Only set for streaming requests. Never sent to the API.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L44)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_MarkCodeExecutionDynamic_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_MarkCodeExecutionDynamic"></a>

### MarkCodeExecutionDynamic


```csharp
public bool MarkCodeExecutionDynamic { get; }
```


True when a web tool that dynamic filtering runs through code execution is sent without the code execution tool.
The API then allows `code_execution` calls implicitly, so the response marks them dynamic.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L81)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_PreparedTools_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_PreparedTools"></a>

### PreparedTools


```csharp
public JsonArray? PreparedTools { get; }
```


The effective prepared tool definitions, kept when the body omits them (message thread continuation).
Computer toolset member calls are checked against them.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L53)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_ProviderOptionsName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_ProviderOptionsName"></a>

### ProviderOptionsName


```csharp
public string ProviderOptionsName { get; }
```


Provider-options key derived from the provider name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L68)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsedCustomProviderKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsedCustomProviderKey"></a>

### UsedCustomProviderKey


```csharp
public bool UsedCustomProviderKey { get; }
```


True when a custom provider-options key was present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L71)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsesJsonResponseTool_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsesJsonResponseTool"></a>

### UsesJsonResponseTool


```csharp
public bool UsesJsonResponseTool { get; }
```


True when structured output is sent as the `json` tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L65)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<AnthropicWarning> Warnings { get; }
```


Preparation warnings.

**Value:** [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L59)
