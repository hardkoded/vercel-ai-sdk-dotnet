---
title: "PrepareResponsesToolsOptions"
description: "Options of PrepareResponsesToolsOptions) besides the tools and the tool choice."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareResponsesToolsOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class PrepareResponsesToolsOptions
```


Options of `PrepareResponsesToolsOptions)` besides the tools and the tool choice.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L36)

## Properties

<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_AllowedTools_"></a>
<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_AllowedTools"></a>

### AllowedTools


```csharp
public OpenAIResponsesAllowedTools? AllowedTools { get; init; }
```


When set, replaces `toolChoice` with an `allowed_tools` choice.

**Value:** [OpenAIResponsesAllowedTools](/api/Vercel.AI.OpenAI/OpenAIResponsesAllowedTools/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L39)

<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_CustomProviderToolNames_"></a>
<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_CustomProviderToolNames"></a>

### CustomProviderToolNames


```csharp
public ISet<string>? CustomProviderToolNames { get; init; }
```


Receives the names of the `openai.custom` tools.

**Value:** [ISet&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L45)

<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_OutputSchemaToolNames_"></a>
<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_OutputSchemaToolNames"></a>

### OutputSchemaToolNames


```csharp
public ISet<string>? OutputSchemaToolNames { get; init; }
```


Receives the names of the function tools that declare an output schema.

**Value:** [ISet&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L48)

<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_SupportsAsyncToolCalling_"></a>
<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_SupportsAsyncToolCalling"></a>

### SupportsAsyncToolCalling


```csharp
public bool SupportsAsyncToolCalling { get; init; }
```


Whether the model accepts `async` tools. When false, the option is dropped with a warning. Default true.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L51)

<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_ToolNameMapping_"></a>
<a id="Vercel_AI_OpenAI_PrepareResponsesToolsOptions_ToolNameMapping"></a>

### ToolNameMapping


```csharp
public ToolNameMapping? ToolNameMapping { get; init; }
```


Maps tool names of the request to provider tool names such as `web_search`.

**Value:** [ToolNameMapping](/api/Vercel.AI.ProviderUtils/ToolNameMapping/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L42)
