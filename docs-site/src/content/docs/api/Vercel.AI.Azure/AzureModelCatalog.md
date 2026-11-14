---
title: "AzureModelCatalog"
description: "Azure-owned models. Maps to azureModelCatalog in azure-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureModelCatalog"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public static class AzureModelCatalog
```


Azure-owned models. Maps to `azureModelCatalog` in `azure-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L36)

## Methods

<a id="Vercel_AI_Azure_AzureModelCatalog_GetAzureModelEntry_"></a>
<a id="Vercel_AI_Azure_AzureModelCatalog_GetAzureModelEntry_System_String_"></a>

### GetAzureModelEntry(string)


```csharp
public static AzureModelRequestConfig? GetAzureModelEntry(string modelId)
```


Returns the entry for &lt;code class="paramref"&gt;modelId&lt;/code&gt;, case-insensitively, or null. Maps to `getAzureModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AzureModelRequestConfig](/api/Vercel.AI.Azure/AzureModelRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L55)
