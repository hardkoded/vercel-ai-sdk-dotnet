---
title: "AnthropicAwsPreparedCall"
description: "A request after the Claude Platform fetch wrapper has applied user-agent and SigV4."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicAwsPreparedCall"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicAwsPreparedCall
```


A request after the Claude Platform fetch wrapper has applied user-agent and SigV4.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L39)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall__ctor_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Boolean_"></a>

### `AnthropicAwsPreparedCall(string, string, string?, IReadOnlyDictionary<string, string>, bool)`


```csharp
public AnthropicAwsPreparedCall(string method, string url, string? body, IReadOnlyDictionary<string, string> headers, bool signed)
```


Creates a prepared call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `signed` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L42)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Body_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Body"></a>

### Body


```csharp
public string? Body { get; }
```


Body. Unchanged when the caller passed a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L58)

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Headers_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Lower-case headers, including signing headers when [Signed](#Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Signed) is true.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L61)

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Method_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Method"></a>

### Method


```csharp
public string Method { get; }
```


HTTP method.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L52)

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Signed_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Signed"></a>

### Signed


```csharp
public bool Signed { get; }
```


True when the body was signed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L64)

<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Url_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsPreparedCall_Url"></a>

### Url


```csharp
public string Url { get; }
```


Request URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L55)
