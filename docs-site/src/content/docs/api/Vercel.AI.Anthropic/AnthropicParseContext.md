---
title: "AnthropicParseContext"
description: "Settings for mapping an Anthropic Messages response onto SDK content."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicParseContext"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicParseContext"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicParseContext
```


Settings for mapping an Anthropic Messages response onto SDK content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L13)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicParseContext__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext__ctor_System_Boolean_System_String_System_Boolean_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__"></a>

### `AnthropicParseContext(bool, string, bool, IReadOnlyList<CallWarning>?)`


```csharp
public AnthropicParseContext(bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey, IReadOnlyList<CallWarning>? warnings = null)
```


Creates parse settings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usesJsonResponseTool` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usedCustomProviderKey` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L16)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicParseContext_ComputerToolsetName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_ComputerToolsetName"></a>

### ComputerToolsetName


```csharp
public string? ComputerToolsetName { get; init; }
```


The caller's name for the declared computer toolset. Defaults to `computer`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L58)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_GenerateId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_GenerateId"></a>

### GenerateId


```csharp
public Func<string>? GenerateId { get; set; }
```


Id factory for citation sources. Defaults to `source-n`.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L61)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_MarkCodeExecutionDynamic_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_MarkCodeExecutionDynamic"></a>

### MarkCodeExecutionDynamic


```csharp
public bool MarkCodeExecutionDynamic { get; init; }
```


Marks `code_execution` server tool calls as dynamic. Set when a web tool that dynamic filtering runs through code execution
is sent without the code execution tool, which the API then allows implicitly.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L49)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_PreparedTools_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_PreparedTools"></a>

### PreparedTools


```csharp
public JsonArray? PreparedTools { get; init; }
```


The effective prepared tool definitions that computer toolset member calls are checked against.
Without them, a toolset member call is rejected as undeclared.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L55)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_ProviderOptionsName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_ProviderOptionsName"></a>

### ProviderOptionsName


```csharp
public string ProviderOptionsName { get; }
```


Provider-options key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L34)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_ToolNames_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_ToolNames"></a>

### ToolNames


```csharp
public Dictionary<string, string> ToolNames { get; }
```


Maps an API tool name back to the caller's tool name.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L43)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_UsedCustomProviderKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_UsedCustomProviderKey"></a>

### UsedCustomProviderKey


```csharp
public bool UsedCustomProviderKey { get; }
```


True when a custom provider-options key was present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L37)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_UsesJsonResponseTool_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_UsesJsonResponseTool"></a>

### UsesJsonResponseTool


```csharp
public bool UsesJsonResponseTool { get; }
```


True when structured output was requested through the json tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L31)

<a id="Vercel_AI_Anthropic_AnthropicParseContext_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicParseContext_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings from request preparation.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L40)
