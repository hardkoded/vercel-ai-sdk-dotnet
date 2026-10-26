---
title: "OpenResponsesOptions"
description: "Open Responses settings. The default origin is the AI Gateway Open Responses route."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesOptions"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesOptions
```


Open Responses settings. The default origin is the AI Gateway Open Responses route.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L17)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_ApiKey_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L23)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_ApiKeyEnvironmentVariable_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public string ApiKeyEnvironmentVariable { get; set; }
```


Environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L26)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_BaseUrl_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API origin. Requests are posted to `{BaseUrl}/responses`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L20)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_Extensions_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_Extensions"></a>

### Extensions


```csharp
public IReadOnlyList<OpenResponsesExtension>? Extensions { get; set; }
```


Codecs for Open Responses extension tools, items, and streaming events. Experimental.

**Value:** [IReadOnlyList&lt;OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L47)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_GetResponseErrorMetadata_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_GetResponseErrorMetadata"></a>

### GetResponseErrorMetadata


```csharp
public Func<JsonElement, OpenResponsesErrorMetadata?>? GetResponseErrorMetadata { get; set; }
```


Reads HTTP status and retry metadata from an endpoint-specific response `error` object.
Applies to both generation and streaming errors. Maps to `getResponseErrorMetadata`.

**Value:** [Func&lt;JsonElement, OpenResponsesErrorMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L44)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_Headers_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Extra headers sent with every request.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L38)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_Name_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


Provider name. It is the key for provider options and provider metadata. Maps to `name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L29)

<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_StrictResponseInput_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesOptions_StrictResponseInput"></a>

### StrictResponseInput


```csharp
public bool StrictResponseInput { get; set; }
```


Whether to serialize assistant history using the strict OpenAI Responses input schemas. Assistant messages
without an item ID are sent as easy input messages. Maps to `strictResponseInput`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L35)
