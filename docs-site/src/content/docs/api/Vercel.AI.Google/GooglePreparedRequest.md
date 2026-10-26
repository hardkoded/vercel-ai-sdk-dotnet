---
title: "GooglePreparedRequest"
description: "A generateContent request body plus the warnings and extra headers that go with it."
editUrl: false
prev: false
next: false
sidebar:
  label: "GooglePreparedRequest"
  hidden: true
---

<a id="Vercel_AI_Google_GooglePreparedRequest"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GooglePreparedRequest
```


A generateContent request body plus the warnings and extra headers that go with it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L14)

## Constructors

<a id="Vercel_AI_Google_GooglePreparedRequest__ctor_"></a>
<a id="Vercel_AI_Google_GooglePreparedRequest__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleWarning__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__Vercel_AI_Google_GoogleToolPreparation_System_String_"></a>

### `GooglePreparedRequest(JsonObject, IReadOnlyList<GoogleWarning>, IReadOnlyDictionary<string, string?>, GoogleToolPreparation, string?)`


```csharp
public GooglePreparedRequest(JsonObject body, IReadOnlyList<GoogleWarning> warnings, IReadOnlyDictionary<string, string?> extraHeaders, GoogleToolPreparation tools, string? jsonResponseToolName = null)
```


Creates a prepared request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `extraHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `tools` | [GoogleToolPreparation](/api/Vercel.AI.Google/GoogleToolPreparation/) |  |
| `jsonResponseToolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L17)

## Properties

<a id="Vercel_AI_Google_GooglePreparedRequest_Body_"></a>
<a id="Vercel_AI_Google_GooglePreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


Request JSON.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L27)

<a id="Vercel_AI_Google_GooglePreparedRequest_ExtraHeaders_"></a>
<a id="Vercel_AI_Google_GooglePreparedRequest_ExtraHeaders"></a>

### ExtraHeaders


```csharp
public IReadOnlyDictionary<string, string?> ExtraHeaders { get; }
```


Headers added for this call, such as Vertex pay-as-you-go request types.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L33)

<a id="Vercel_AI_Google_GooglePreparedRequest_JsonResponseToolName_"></a>
<a id="Vercel_AI_Google_GooglePreparedRequest_JsonResponseToolName"></a>

### JsonResponseToolName


```csharp
public string? JsonResponseToolName { get; }
```


Name of the internal tool that carries structured output, or null when the request uses native JSON formatting.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L39)

<a id="Vercel_AI_Google_GooglePreparedRequest_Tools_"></a>
<a id="Vercel_AI_Google_GooglePreparedRequest_Tools"></a>

### Tools


```csharp
public GoogleToolPreparation Tools { get; }
```


Prepared tools, including built-in name mapping.

**Value:** [GoogleToolPreparation](/api/Vercel.AI.Google/GoogleToolPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L36)

<a id="Vercel_AI_Google_GooglePreparedRequest_Warnings_"></a>
<a id="Vercel_AI_Google_GooglePreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Preparation warnings.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L30)
