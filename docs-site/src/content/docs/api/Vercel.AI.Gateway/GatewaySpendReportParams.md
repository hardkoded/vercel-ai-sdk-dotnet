---
title: "GatewaySpendReportParams"
description: "Parameters of CancellationToken). Maps to GatewaySpendReportParams."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewaySpendReportParams"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewaySpendReportParams"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewaySpendReportParams
```


Parameters of `CancellationToken)`. Maps to `GatewaySpendReportParams`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L54)

## Properties

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_CredentialType_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_CredentialType"></a>

### CredentialType


```csharp
public GatewayCredentialType? CredentialType { get; set; }
```


Gets or sets the credential type to filter the spend to.

**Value:** `GatewayCredentialType?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L78)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_DatePart_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_DatePart"></a>

### DatePart


```csharp
public GatewaySpendReportDatePart? DatePart { get; set; }
```


Gets or sets the time granularity when [GroupBy](#Vercel_AI_Gateway_GatewaySpendReportParams_GroupBy) is `day`.

**Value:** `GatewaySpendReportDatePart?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L66)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_EndDate_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_EndDate"></a>

### EndDate


```csharp
public string EndDate { get; set; }
```


Gets or sets the end date in YYYY-MM-DD format (inclusive).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L60)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_GroupBy_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_GroupBy"></a>

### GroupBy


```csharp
public GatewaySpendReportGroupBy? GroupBy { get; set; }
```


Gets or sets the primary aggregation dimension. The Gateway defaults to `day`.

**Value:** `GatewaySpendReportGroupBy?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L63)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_Model_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_Model"></a>

### Model


```csharp
public string? Model { get; set; }
```


Gets or sets the model to filter the spend to (for example `anthropic/claude-sonnet-4.5`).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L72)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_Provider"></a>

### Provider


```csharp
public string? Provider { get; set; }
```


Gets or sets the provider to filter the spend to (for example `anthropic`).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L75)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_StartDate_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_StartDate"></a>

### StartDate


```csharp
public string StartDate { get; set; }
```


Gets or sets the start date in YYYY-MM-DD format (inclusive).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L57)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_Tags_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_Tags"></a>

### Tags


```csharp
public IReadOnlyList<string>? Tags { get; set; }
```


Gets or sets the tags to filter the spend to.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L81)

<a id="Vercel_AI_Gateway_GatewaySpendReportParams_UserId_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportParams_UserId"></a>

### UserId


```csharp
public string? UserId { get; set; }
```


Gets or sets the user to filter the spend to.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L69)
