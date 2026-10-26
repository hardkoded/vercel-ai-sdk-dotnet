---
title: "GoogleVertexEndpoints"
description: "Vertex host and URL construction. Locations are single DNS labels so they cannot rewrite the host."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexEndpoints"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexEndpoints"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleVertexEndpoints
```


Vertex host and URL construction. Locations are single DNS labels so they cannot rewrite the host.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L11)

## Fields

<a id="Vercel_AI_Google_GoogleVertexEndpoints_ExpressModeBaseUrl"></a>

### ExpressModeBaseUrl


```csharp
public const string ExpressModeBaseUrl = "https://aiplatform.googleapis.com/v1/publishers/google"
```


Express Mode origin used when an API key is supplied.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L14)

## Methods

<a id="Vercel_AI_Google_GoogleVertexEndpoints_AnthropicBaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_AnthropicBaseUrl_System_String_System_String_System_String_"></a>

### AnthropicBaseUrl(string?, string?, string?)


```csharp
public static string AnthropicBaseUrl(string? project, string? location, string? baseUrl)
```


Anthropic on Vertex publisher URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L68)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_GeminiBaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_GeminiBaseUrl_System_String_System_String_System_String_System_Boolean_System_Boolean_"></a>

### GeminiBaseUrl(string, string, string?, bool, bool)


```csharp
public static string GeminiBaseUrl(string project, string location, string? baseUrl, bool expressMode, bool endpointModel)
```


Publisher base URL for Gemini on Vertex. Tuned `endpoints/` ids omit `/publishers/google`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expressMode` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `endpointModel` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L44)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_Host_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_Host_System_String_"></a>

### Host(string)


```csharp
public static string Host(string location)
```


Returns the Vertex AI host for &lt;code class="paramref"&gt;location&lt;/code&gt;.
`global` uses the unprefixed host. `eu` and `us` use the multi-region REP host.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L20)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_IsEndpointModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_IsEndpointModel_System_String_"></a>

### IsEndpointModel(string)


```csharp
public static bool IsEndpointModel(string modelId)
```


True when the model id addresses a deployed endpoint.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L114)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_MaasBaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_MaasBaseUrl_System_String_System_String_System_String_"></a>

### MaasBaseUrl(string, string?, string?)


```csharp
public static string MaasBaseUrl(string project, string? location, string? baseUrl)
```


OpenAI-compatible MaaS URL. An empty base URL is rebuilt from the project and location.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L81)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_RecognizeUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_RecognizeUrl_System_String_System_String_"></a>

### RecognizeUrl(string, string)


```csharp
public static string RecognizeUrl(string project, string location)
```


Cloud Speech-to-Text recognize URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L107)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_RequireLabel_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_RequireLabel_System_String_System_String_"></a>

### RequireLabel(string, string)


```csharp
public static string RequireLabel(string location, string name)
```


Rejects a location that is not one DNS label.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L120)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_SpeechHost_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_SpeechHost_System_String_"></a>

### SpeechHost(string)


```csharp
public static string SpeechHost(string location)
```


Speech-to-Text host. `global` is `speech.googleapis.com`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L37)

<a id="Vercel_AI_Google_GoogleVertexEndpoints_XaiBaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexEndpoints_XaiBaseUrl_System_String_System_String_System_String_"></a>

### XaiBaseUrl(string, string?, string?)


```csharp
public static string XaiBaseUrl(string project, string? location, string? baseUrl)
```


xAI on Vertex always uses the global AI Platform host.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L94)
