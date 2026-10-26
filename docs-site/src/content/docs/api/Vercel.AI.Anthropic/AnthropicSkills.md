---
title: "AnthropicSkills"
description: "Anthropic Skills API."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicSkills"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicSkills"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicSkills
```


Anthropic Skills API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L51)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicSkills__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkills__ctor_Vercel_AI_ProviderUtils_ProviderHttp_System_Func_System_String__System_Func_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String___System_String_"></a>

### `AnthropicSkills(ProviderHttp, Func<string>, Func<IReadOnlyDictionary<string, string?>>, string)`


```csharp
public AnthropicSkills(ProviderHttp http, Func<string> baseUrl, Func<IReadOnlyDictionary<string, string?>> headers, string provider)
```


Creates a skills client.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `http` | [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/) |  |
| `baseUrl` | [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `headers` | [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L58)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicSkills_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkills_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L70)

<a id="Vercel_AI_Anthropic_AnthropicSkills_SpecificationVersion_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkills_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L67)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicSkills_UploadSkillAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkills_UploadSkillAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicSkillFile__System_String_System_Threading_CancellationToken_"></a>

### `UploadSkillAsync(IReadOnlyList<AnthropicSkillFile>, string?, CancellationToken)`


```csharp
public Task<AnthropicSkillUpload> UploadSkillAsync(IReadOnlyList<AnthropicSkillFile> files, string? displayTitle, CancellationToken cancellationToken)
```


Uploads skill files, then reads the latest version metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `files` | [IReadOnlyList&lt;AnthropicSkillFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `displayTitle` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AnthropicSkillUpload&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L73)
