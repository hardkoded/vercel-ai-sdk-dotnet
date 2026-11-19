---
title: "RequestOrigin"
description: "Removes credentials, paths, queries, and fragments of a request URL before telemetry. Maps to telemetry/get-request-origin.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "RequestOrigin"
  hidden: true
---

<a id="Vercel_AI_RequestOrigin"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class RequestOrigin
```


Removes credentials, paths, queries, and fragments of a request URL before telemetry. Maps to `telemetry/get-request-origin.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/RequestOrigin.cs#L10)

## Methods

<a id="Vercel_AI_RequestOrigin_GetRequestOrigin_"></a>
<a id="Vercel_AI_RequestOrigin_GetRequestOrigin_System_String_"></a>

### GetRequestOrigin(string?)


```csharp
public static string? GetRequestOrigin(string? url)
```


Returns the origin of an HTTP or HTTPS URL, or null for anything else.
Maps to `getRequestOrigin`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/RequestOrigin.cs#L16)

<a id="Vercel_AI_RequestOrigin_SanitizeRequestMetadata_"></a>
<a id="Vercel_AI_RequestOrigin_SanitizeRequestMetadata_Vercel_AI_Provider_LanguageModelStreamRequest_"></a>

### SanitizeRequestMetadata(LanguageModelStreamRequest?)


```csharp
public static LanguageModelStreamRequest? SanitizeRequestMetadata(LanguageModelStreamRequest? request)
```


Keeps the request metadata but reduces its URL to the origin, and drops an unusable URL.
Maps to `sanitizeRequestMetadata`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/) |  |

**Returns:** [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/RequestOrigin.cs#L35)
