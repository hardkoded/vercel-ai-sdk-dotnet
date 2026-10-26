---
title: "GoogleParseContext"
description: "Ids and metadata keys used while parsing a Gemini response."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleParseContext"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleParseContext"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleParseContext
```


Ids and metadata keys used while parsing a Gemini response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L406)

## Constructors

<a id="Vercel_AI_Google_GoogleParseContext__ctor_"></a>
<a id="Vercel_AI_Google_GoogleParseContext__ctor_System_String_System_Func_System_String__Vercel_AI_Google_GoogleToolPreparation_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleWarning__System_String_"></a>

### `GoogleParseContext(string?, Func<string>?, GoogleToolPreparation?, IReadOnlyList<GoogleWarning>?, string?)`


```csharp
public GoogleParseContext(string? provider = null, Func<string>? generateId = null, GoogleToolPreparation? tools = null, IReadOnlyList<GoogleWarning>? warnings = null, string? jsonResponseToolName = null)
```


Creates a parse context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `generateId` | [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `tools` | [GoogleToolPreparation](/api/Vercel.AI.Google/GoogleToolPreparation/) |  |
| `warnings` | [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `jsonResponseToolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L411)

## Properties

<a id="Vercel_AI_Google_GoogleParseContext_GenerateId_"></a>
<a id="Vercel_AI_Google_GoogleParseContext_GenerateId"></a>

### GenerateId


```csharp
public Func<string> GenerateId { get; set; }
```


Id factory. The default uses an increasing counter.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L425)

<a id="Vercel_AI_Google_GoogleParseContext_JsonResponseToolName_"></a>
<a id="Vercel_AI_Google_GoogleParseContext_JsonResponseToolName"></a>

### JsonResponseToolName


```csharp
public string? JsonResponseToolName { get; }
```


Name of the internal tool that carries structured output. Its calls become text and ordinary text is omitted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L434)

<a id="Vercel_AI_Google_GoogleParseContext_MetadataNames_"></a>
<a id="Vercel_AI_Google_GoogleParseContext_MetadataNames"></a>

### MetadataNames


```csharp
public IReadOnlyList<string> MetadataNames { get; }
```


Metadata namespaces written onto the result.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L422)

<a id="Vercel_AI_Google_GoogleParseContext_Tools_"></a>
<a id="Vercel_AI_Google_GoogleParseContext_Tools"></a>

### Tools


```csharp
public GoogleToolPreparation Tools { get; }
```


Tool name mapping.

**Value:** [GoogleToolPreparation](/api/Vercel.AI.Google/GoogleToolPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L428)

<a id="Vercel_AI_Google_GoogleParseContext_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleParseContext_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings copied onto the result.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L431)

## Methods

<a id="Vercel_AI_Google_GoogleParseContext_NextId_"></a>
<a id="Vercel_AI_Google_GoogleParseContext_NextId"></a>

### NextId()


```csharp
public string NextId()
```


Returns the next synthetic id.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L437)
