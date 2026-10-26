---
title: "AmazonBedrockPreparedCall"
description: "A request after the Bedrock fetch wrapper has applied user-agent and authentication."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockPreparedCall"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockPreparedCall
```


A request after the Bedrock fetch wrapper has applied user-agent and authentication.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L38)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall__ctor_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Boolean_"></a>

### `AmazonBedrockPreparedCall(string, string, string?, IReadOnlyDictionary<string, string>, bool)`


```csharp
public AmazonBedrockPreparedCall(string method, string url, string? body, IReadOnlyDictionary<string, string> headers, bool signed)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L41)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Body_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Body"></a>

### Body


```csharp
public string? Body { get; }
```


Body. Unchanged when the caller passed a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L57)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Headers_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Lower-case headers, including signing headers when [Signed](#Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Signed) is true.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L60)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Method_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Method"></a>

### Method


```csharp
public string Method { get; }
```


HTTP method.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L51)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Signed_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Signed"></a>

### Signed


```csharp
public bool Signed { get; }
```


True when the body was signed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L63)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Url_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockPreparedCall_Url"></a>

### Url


```csharp
public string Url { get; }
```


Request URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L54)
