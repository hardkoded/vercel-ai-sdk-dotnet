---
title: "GatewaySpendReport"
description: "Fetches a spend report (GET /v1/report) from the Gateway."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewaySpendReport"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewaySpendReport"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewaySpendReport
```


Fetches a spend report (`GET /v1/report`) from the Gateway.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L141)

## Constructors

<a id="Vercel_AI_Gateway_GatewaySpendReport__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReport__ctor_Vercel_AI_Gateway_GatewayConfig_"></a>

### GatewaySpendReport(GatewayConfig)


```csharp
public GatewaySpendReport(GatewayConfig config)
```


Initializes a new instance of the [GatewaySpendReport](./) class.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [GatewayConfig](/api/Vercel.AI.Gateway/GatewayConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L146)

## Methods

<a id="Vercel_AI_Gateway_GatewaySpendReport_GetSpendReportAsync_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReport_GetSpendReportAsync_Vercel_AI_Gateway_GatewaySpendReportParams_System_Threading_CancellationToken_"></a>

### GetSpendReportAsync(GatewaySpendReportParams, CancellationToken)


```csharp
public Task<GatewaySpendReportResponse> GetSpendReportAsync(GatewaySpendReportParams parameters, CancellationToken cancellationToken = default)
```


Gets the spend report of the account.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parameters` | [GatewaySpendReportParams](/api/Vercel.AI.Gateway/GatewaySpendReportParams/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewaySpendReportResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L152)
