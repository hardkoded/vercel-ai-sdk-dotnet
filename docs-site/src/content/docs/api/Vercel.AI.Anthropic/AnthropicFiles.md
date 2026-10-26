---
title: "AnthropicFiles"
description: "Anthropic Files API upload."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicFiles"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicFiles"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicFiles : IFileStore
```


Anthropic Files API upload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L13)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicFiles__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFiles__ctor_Vercel_AI_ProviderUtils_ProviderHttp_System_Func_System_String__System_Func_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String___System_String_"></a>

### `AnthropicFiles(ProviderHttp, Func<string>, Func<IReadOnlyDictionary<string, string?>>, string)`


```csharp
public AnthropicFiles(ProviderHttp http, Func<string> baseUrl, Func<IReadOnlyDictionary<string, string?>> headers, string provider)
```


Creates a files client.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `http` | [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/) |  |
| `baseUrl` | [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `headers` | [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L20)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicFiles_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFiles_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L32)

<a id="Vercel_AI_Anthropic_AnthropicFiles_SpecificationVersion_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFiles_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L29)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicFiles_UploadAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFiles_UploadAsync_System_String_System_Byte___System_String_System_Nullable_System_Boolean__System_Threading_CancellationToken_"></a>

### UploadAsync(string, byte[], string, bool?, CancellationToken)


```csharp
public Task<AnthropicUploadedFile> UploadAsync(string fileName, byte[] data, string mediaType, bool? downloadable, CancellationToken cancellationToken)
```


Uploads a file and returns the parsed Files API body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `downloadable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AnthropicUploadedFile&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L42)

<a id="Vercel_AI_Anthropic_AnthropicFiles_UploadFileAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFiles_UploadFileAsync_System_String_System_Byte___System_String_System_Threading_CancellationToken_"></a>

### UploadFileAsync(string, byte[], string, CancellationToken)


```csharp
public Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
```


Uploads a file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadedFile&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L35)
