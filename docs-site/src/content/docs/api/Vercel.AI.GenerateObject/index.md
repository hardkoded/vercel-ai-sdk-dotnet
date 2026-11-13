---
title: "Vercel.AI.GenerateObject"
description: "Public types in the `Vercel.AI.GenerateObject` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.GenerateObject"
---

[API reference](/api/)

Public types in the `Vercel.AI.GenerateObject` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [GenerateObjectEndEvent&lt;T&gt;](/api/Vercel.AI.GenerateObject/GenerateObjectEndEvent-1/) | Event sent after the generated object has been parsed and validated. |
| [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/) | Base information shared by generate-object callback events. |
| [GenerateObjectOptions&lt;T&gt;](/api/Vercel.AI.GenerateObject/GenerateObjectOptions-1/) | Options for CancellationToken). Maps to generateObject. |
| [GenerateObjectRepairContext](/api/Vercel.AI.GenerateObject/GenerateObjectRepairContext/) | Input passed to RepairText. |
| [GenerateObjectResult&lt;T&gt;](/api/Vercel.AI.GenerateObject/GenerateObjectResult-1/) | Result of a generateObject call. |
| [GenerateObjectStartEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStartEvent/) | Event sent before object generation begins. |
| [GenerateObjectStepEndEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStepEndEvent/) | The raw result of the one model step made by generateObject. |
| [GenerateObjectStepStartEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStepStartEvent/) | Event sent immediately before the generate-object model call. |
| [NoObjectGeneratedException](/api/Vercel.AI.GenerateObject/NoObjectGeneratedException/) | Thrown when a model response cannot produce the requested object. Maps to NoObjectGeneratedError. |
