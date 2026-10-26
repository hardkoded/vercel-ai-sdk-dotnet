---
title: "GoogleInteractionsAgent"
description: "An agent to call through the Interactions API: { agent: name } or { managedAgent: name } upstream."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleInteractionsAgent"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleInteractionsAgent"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleInteractionsAgent
```


An agent to call through the Interactions API: `{ agent: name }` or `{ managedAgent: name }` upstream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L169)

## Constructors

<a id="Vercel_AI_Google_GoogleInteractionsAgent__ctor_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsAgent__ctor_System_String_"></a>

### GoogleInteractionsAgent(string)


```csharp
public GoogleInteractionsAgent(string agent)
```


Creates an agent input for a Gemini agent preset such as `deep-research-pro-preview-12-2025`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `agent` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L172)

## Properties

<a id="Vercel_AI_Google_GoogleInteractionsAgent_Agent_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsAgent_Agent"></a>

### Agent


```csharp
public string Agent { get; }
```


The agent name, sent as `agent` in the request body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L178)

## Methods

<a id="Vercel_AI_Google_GoogleInteractionsAgent_Managed_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsAgent_Managed_System_String_"></a>

### Managed(string)


```csharp
public static GoogleInteractionsAgent Managed(string managedAgent)
```


Creates an agent input for a managed agent you created on Google's side.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `managedAgent` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GoogleInteractionsAgent](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L181)
