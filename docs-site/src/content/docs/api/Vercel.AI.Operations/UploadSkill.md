---
title: "UploadSkill"
description: "Uploads a skill. Maps to uploadSkill."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadSkill"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadSkill"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class UploadSkill
```


Uploads a skill. Maps to `uploadSkill`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L450)

## Methods

<a id="Vercel_AI_Operations_UploadSkill_UploadSkillAsync_"></a>
<a id="Vercel_AI_Operations_UploadSkill_UploadSkillAsync_Vercel_AI_Operations_UploadSkillRequest_System_Threading_CancellationToken_"></a>

### UploadSkillAsync(UploadSkillRequest, CancellationToken)


```csharp
public static Task<UploadSkillResult> UploadSkillAsync(UploadSkillRequest request, CancellationToken cancellationToken = default)
```


Uploads the skill files.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [UploadSkillRequest](/api/Vercel.AI.Operations/UploadSkillRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadSkillResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L453)
