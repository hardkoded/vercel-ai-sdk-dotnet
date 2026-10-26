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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L14)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicWarning__System_Collections_Generic_IReadOnlyList_System_String__System_Boolean_System_String_System_Boolean_"></a>

### `AnthropicPreparedRequest(JsonObject, IReadOnlyList<AnthropicWarning>, IReadOnlyList<string>, bool, string, bool)`


```csharp
public AnthropicPreparedRequest(JsonObject body, IReadOnlyList<AnthropicWarning> warnings, IReadOnlyList<string> betas, bool usesJsonResponseTool, string providerOptionsName, bool usedCustomProviderKey)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L17)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Betas_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Betas"></a>

### Betas


```csharp
public IReadOnlyList<string> Betas { get; }
```


Beta names for the `anthropic-beta` header.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L40)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Body_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


JSON body.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L34)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_ProviderOptionsName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_ProviderOptionsName"></a>

### ProviderOptionsName


```csharp
public string ProviderOptionsName { get; }
```


Provider-options key derived from the provider name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L46)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsedCustomProviderKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsedCustomProviderKey"></a>

### UsedCustomProviderKey


```csharp
public bool UsedCustomProviderKey { get; }
```


True when a custom provider-options key was present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L49)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsesJsonResponseTool_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_UsesJsonResponseTool"></a>

### UsesJsonResponseTool


```csharp
public bool UsesJsonResponseTool { get; }
```


True when structured output is sent as the `json` tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L43)

<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<AnthropicWarning> Warnings { get; }
```


Preparation warnings.

**Value:** [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L37)
