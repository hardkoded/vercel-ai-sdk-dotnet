---
title: "AmazonBedrockEndpoints"
description: "Builds Bedrock service hosts. Partition prefixes select the DNS suffix."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockEndpoints"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockEndpoints
```


Builds Bedrock service hosts. Partition prefixes select the DNS suffix.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L10)

## Fields

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_AgentRuntimeEndpointEnvironmentVariable"></a>

### AgentRuntimeEndpointEnvironmentVariable


```csharp
public const string AgentRuntimeEndpointEnvironmentVariable = "AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME"
```


Environment variable that overrides the Bedrock Agent Runtime endpoint.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L16)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_GlobalEndpointEnvironmentVariable"></a>

### GlobalEndpointEnvironmentVariable


```csharp
public const string GlobalEndpointEnvironmentVariable = "AWS_ENDPOINT_URL"
```


Environment variable that overrides every AWS service endpoint.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L19)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_RegionEnvironmentVariable"></a>

### RegionEnvironmentVariable


```csharp
public const string RegionEnvironmentVariable = "AWS_REGION"
```


Environment variable that supplies the AWS region.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L22)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_RuntimeEndpointEnvironmentVariable"></a>

### RuntimeEndpointEnvironmentVariable


```csharp
public const string RuntimeEndpointEnvironmentVariable = "AWS_ENDPOINT_URL_BEDROCK_RUNTIME"
```


Environment variable that overrides the Bedrock Runtime endpoint.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L13)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_DnsSuffix_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_DnsSuffix_System_String_"></a>

### DnsSuffix(string)


```csharp
public static string DnsSuffix(string region)
```


DNS suffix for an AWS partition. The first matching prefix wins, matching the upstream list order.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `region` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L65)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_Resolve_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_Resolve_System_String_System_Func_System_String__System_String_"></a>

### `Resolve(string?, Func<string>, string)`


```csharp
public static string Resolve(string? baseUrl, Func<string> getRegion, string service)
```


Same as `String)`, but &lt;code class="paramref"&gt;getRegion&lt;/code&gt; only runs when no
explicit endpoint is configured.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `getRegion` | [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `service` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L40)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEndpoints_Resolve_System_String_System_String_System_String_"></a>

### Resolve(string?, string, string)


```csharp
public static string Resolve(string? baseUrl, string region, string service)
```


Returns &lt;code class="paramref"&gt;baseUrl&lt;/code&gt; when it is set, then the service endpoint environment variable, then
`AWS_ENDPOINT_URL`. Otherwise returns `https://{service}.{region}.{suffix}`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `region` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `service` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEndpoints.cs#L31)
