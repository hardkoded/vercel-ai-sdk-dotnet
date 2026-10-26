---
title: "AnthropicOptions"
description: "Anthropic Messages API settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicOptions"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicOptions"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public class AnthropicOptions
```


Anthropic Messages API settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L15)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicOptions_AccessKeyId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_AccessKeyId"></a>

### AccessKeyId


```csharp
public string? AccessKeyId { get; set; }
```


AWS access key. Used when no Anthropic API key is configured.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L54)

<a id="Vercel_AI_Anthropic_AnthropicOptions_ApiKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit API key. Mutually exclusive with [AuthToken](#Vercel_AI_Anthropic_AnthropicOptions_AuthToken).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L24)

<a id="Vercel_AI_Anthropic_AnthropicOptions_ApiKeyEnvironmentVariable_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public string ApiKeyEnvironmentVariable { get; set; }
```


Environment variable. Defaults to `ANTHROPIC_API_KEY`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L30)

<a id="Vercel_AI_Anthropic_AnthropicOptions_AuthToken_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_AuthToken"></a>

### AuthToken


```csharp
public string? AuthToken { get; set; }
```


Bearer token. Mutually exclusive with [ApiKey](#Vercel_AI_Anthropic_AnthropicOptions_ApiKey).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L27)

<a id="Vercel_AI_Anthropic_AnthropicOptions_BaseUrl_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API origin. The official host is normalized to `/v1`. Message calls append `/messages`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L21)

<a id="Vercel_AI_Anthropic_AnthropicOptions_BatchResultDownloads_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_BatchResultDownloads"></a>

### BatchResultDownloads


```csharp
public BatchResultDownloads? BatchResultDownloads { get; set; }
```


Settings for downloading JSON Lines batch results.

**Value:** [BatchResultDownloads](/api/Vercel.AI.ProviderUtils/BatchResultDownloads/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L18)

<a id="Vercel_AI_Anthropic_AnthropicOptions_CredentialProvider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_CredentialProvider"></a>

### CredentialProvider


```csharp
public Func<CancellationToken, Task<AnthropicAwsCredentials>>? CredentialProvider { get; set; }
```


Supplies SigV4 credentials for one request.

**Value:** [Func&lt;CancellationToken, Task&lt;AnthropicAwsCredentials&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L63)

<a id="Vercel_AI_Anthropic_AnthropicOptions_Headers_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Headers merged into every request.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L39)

<a id="Vercel_AI_Anthropic_AnthropicOptions_Name_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_Name"></a>

### Name


```csharp
public string? Name { get; set; }
```


Model provider id. Defaults to `anthropic.messages`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L36)

<a id="Vercel_AI_Anthropic_AnthropicOptions_Region_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_Region"></a>

### Region


```csharp
public string? Region { get; set; }
```


AWS region. Used when the AWS base URL is not set explicitly.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L48)

<a id="Vercel_AI_Anthropic_AnthropicOptions_SecretAccessKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_SecretAccessKey"></a>

### SecretAccessKey


```csharp
public string? SecretAccessKey { get; set; }
```


AWS secret key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L57)

<a id="Vercel_AI_Anthropic_AnthropicOptions_SessionToken_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; set; }
```


AWS session token.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L60)

<a id="Vercel_AI_Anthropic_AnthropicOptions_TransformRequestBody_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_TransformRequestBody"></a>

### TransformRequestBody


```csharp
public Func<JsonObject, JsonObject>? TransformRequestBody { get; set; }
```


Optional rewrite of the JSON body before it is sent.

**Value:** [Func&lt;JsonObject, JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L42)

<a id="Vercel_AI_Anthropic_AnthropicOptions_TransformRequestHeaders_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_TransformRequestHeaders"></a>

### TransformRequestHeaders


```csharp
public Func<Dictionary<string, string?>, IReadOnlyDictionary<string, JsonElement>?, Dictionary<string, string?>>? TransformRequestHeaders { get; set; }
```


Optional rewrite of the combined request headers. Receives the call's provider options and returns the headers to send.

**Value:** [Func&lt;Dictionary&lt;string, string&gt;, IReadOnlyDictionary&lt;string, JsonElement&gt;, Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L45)

<a id="Vercel_AI_Anthropic_AnthropicOptions_UtcNow_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_UtcNow"></a>

### UtcNow


```csharp
public Func<DateTimeOffset>? UtcNow { get; set; }
```


Clock used when signing. Tests pin this.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L66)

<a id="Vercel_AI_Anthropic_AnthropicOptions_Version_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_Version"></a>

### Version


```csharp
public string Version { get; set; }
```


`anthropic-version` header.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L33)

<a id="Vercel_AI_Anthropic_AnthropicOptions_WorkspaceId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicOptions_WorkspaceId"></a>

### WorkspaceId


```csharp
public string? WorkspaceId { get; set; }
```


Claude Platform workspace id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L51)
