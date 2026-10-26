---
title: "AnthropicCacheControlValidator"
description: "Reads cachecontrol and enforces Anthropic's four-breakpoint limit."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicCacheControlValidator"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicCacheControlValidator
```


Reads `cache_control` and enforces Anthropic's four-breakpoint limit.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicCacheControl.cs#L11)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator__ctor_System_String_"></a>

### AnthropicCacheControlValidator(string)


```csharp
public AnthropicCacheControlValidator(string providerOptionsName = "anthropic")
```


Creates a validator. &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt; is read before `anthropic`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicCacheControl.cs#L19)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<AnthropicWarning> Warnings { get; }
```


Warnings collected while reading cache controls.

**Value:** [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicCacheControl.cs#L25)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Get_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Get_System_Nullable_System_Text_Json_JsonElement__System_String_System_Boolean_"></a>

### Get(JsonElement?, string, bool)


```csharp
public JsonObject? Get(JsonElement? providerMetadata, string contextType, bool canCache)
```


Returns the cache control object when this context may cache it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `contextType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `canCache` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicCacheControl.cs#L31)

<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Read_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Read_System_Nullable_System_Text_Json_JsonElement__"></a>

### Read(JsonElement?)


```csharp
public static JsonObject? Read(JsonElement? providerMetadata)
```


Reads `cacheControl` or `cache_control` from Anthropic provider metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicCacheControl.cs#L62)

<a id="Vercel_AI_Anthropic_AnthropicCacheControlValidator_Read_System_Nullable_System_Text_Json_JsonElement__System_String_"></a>

### Read(JsonElement?, string)


```csharp
public static JsonObject? Read(JsonElement? providerMetadata, string providerOptionsName)
```


Reads the cache control from &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;, falling back to the `anthropic` namespace.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicCacheControl.cs#L68)
