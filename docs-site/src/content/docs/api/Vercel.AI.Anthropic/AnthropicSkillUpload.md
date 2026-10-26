---
title: "AnthropicSkillUpload"
description: "Result of uploading a skill."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicSkillUpload"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicSkillUpload"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicSkillUpload
```


Result of uploading a skill.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L30)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicSkillUpload__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillUpload__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `AnthropicSkillUpload(string, string?, IReadOnlyDictionary<string, string>)`


```csharp
public AnthropicSkillUpload(string id, string? name, IReadOnlyDictionary<string, string> providerReference)
```


Creates an upload result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L33)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicSkillUpload_Id_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillUpload_Id"></a>

### Id


```csharp
public string Id { get; }
```


Skill id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L41)

<a id="Vercel_AI_Anthropic_AnthropicSkillUpload_Name_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillUpload_Name"></a>

### Name


```csharp
public string? Name { get; }
```


Skill name from the version metadata, when present.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L44)

<a id="Vercel_AI_Anthropic_AnthropicSkillUpload_ProviderReference_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillUpload_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Map from provider name to the skill id. `anthropic` is set to the skill id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L47)
