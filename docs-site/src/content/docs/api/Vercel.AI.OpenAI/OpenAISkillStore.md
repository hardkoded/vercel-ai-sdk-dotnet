---
title: "OpenAISkillStore"
description: "OpenAI skill store."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISkillStore"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISkillStore"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISkillStore : ISkillStore
```


OpenAI skill store.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L71)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAISkillStore__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillStore__ctor_Vercel_AI_OpenAI_OpenAIProvider_"></a>

### OpenAISkillStore(OpenAIProvider)


```csharp
public OpenAISkillStore(OpenAIProvider provider)
```


Creates a skill store.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L76)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISkillStore_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillStore_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L82)

## Methods

<a id="Vercel_AI_OpenAI_OpenAISkillStore_UploadAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillStore_UploadAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_OpenAI_OpenAISkillFile__System_String_System_Threading_CancellationToken_"></a>

### `UploadAsync(IReadOnlyList<OpenAISkillFile>, string?, CancellationToken)`


```csharp
public Task<OpenAISkillUpload> UploadAsync(IReadOnlyList<OpenAISkillFile> files, string? displayTitle, CancellationToken cancellationToken)
```


Uploads skill files as `files[]`. &lt;code class="paramref"&gt;displayTitle&lt;/code&gt; is warned and omitted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `files` | [IReadOnlyList&lt;OpenAISkillFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `displayTitle` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAISkillUpload&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L93)

<a id="Vercel_AI_OpenAI_OpenAISkillStore_UploadSkillAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillStore_UploadSkillAsync_System_String_System_String_System_Threading_CancellationToken_"></a>

### UploadSkillAsync(string, string, CancellationToken)


```csharp
public Task<UploadedSkill> UploadSkillAsync(string name, string instructions, CancellationToken cancellationToken)
```


Uploads a skill document.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadedSkill&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L85)
