---
title: "GoogleTokenUsage"
description: "Token usage converted from Gemini usageMetadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleTokenUsage"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleTokenUsage"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleTokenUsage
```


Token usage converted from Gemini `usageMetadata`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L12)

## Constructors

<a id="Vercel_AI_Google_GoogleTokenUsage__ctor_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage__ctor_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_System_Nullable_System_Text_Json_JsonElement__"></a>

### GoogleTokenUsage(int, int, int, int, int, int, JsonElement?)


```csharp
public GoogleTokenUsage(int inputTotal, int noCache, int cacheRead, int outputTotal, int text, int reasoning, JsonElement? raw)
```


Creates converted usage. [CacheWrite](#Vercel_AI_Google_GoogleTokenUsage_CacheWrite) stays null because Gemini does not report cache writes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputTotal` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `noCache` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheRead` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `outputTotal` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `text` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `reasoning` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `raw` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L15)

## Properties

<a id="Vercel_AI_Google_GoogleTokenUsage_CacheRead_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_CacheRead"></a>

### CacheRead


```csharp
public int CacheRead { get; }
```


Cached content tokens.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L33)

<a id="Vercel_AI_Google_GoogleTokenUsage_CacheWrite_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_CacheWrite"></a>

### CacheWrite


```csharp
public int? CacheWrite { get; }
```


Always null. Gemini does not report cache writes.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L36)

<a id="Vercel_AI_Google_GoogleTokenUsage_InputTotal_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_InputTotal"></a>

### InputTotal


```csharp
public int InputTotal { get; }
```


Prompt tokens plus tool-use prompt tokens.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L27)

<a id="Vercel_AI_Google_GoogleTokenUsage_NoCache_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_NoCache"></a>

### NoCache


```csharp
public int NoCache { get; }
```


Input tokens that were not served from cache.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L30)

<a id="Vercel_AI_Google_GoogleTokenUsage_OutputTotal_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_OutputTotal"></a>

### OutputTotal


```csharp
public int OutputTotal { get; }
```


Candidate tokens plus thought tokens.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L42)

<a id="Vercel_AI_Google_GoogleTokenUsage_Raw_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; }
```


The original usage object.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L51)

<a id="Vercel_AI_Google_GoogleTokenUsage_Reasoning_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_Reasoning"></a>

### Reasoning


```csharp
public int Reasoning { get; }
```


Thought tokens.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L48)

<a id="Vercel_AI_Google_GoogleTokenUsage_Text_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_Text"></a>

### Text


```csharp
public int Text { get; }
```


Candidate tokens.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L45)

## Methods

<a id="Vercel_AI_Google_GoogleTokenUsage_ToLanguageModelUsage_"></a>
<a id="Vercel_AI_Google_GoogleTokenUsage_ToLanguageModelUsage"></a>

### ToLanguageModelUsage()


```csharp
public LanguageModelUsage ToLanguageModelUsage()
```


Maps onto the shared usage type. Cache writes stay unset.

**Returns:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L54)
