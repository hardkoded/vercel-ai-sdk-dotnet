// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>Tool context failed schema validation. Maps to <c>TypeValidationError</c>.</summary>
public sealed class TypeValidationException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public TypeValidationException(object? value, string field, string entityName)
        : base("Type validation failed for " + field + " of " + entityName + ".")
    {
        Value = value;
        Context = new ToolValidationContext(field, entityName);
    }

    /// <summary>Value that failed validation.</summary>
    public object? Value { get; }

    /// <summary>Field and entity that failed.</summary>
    public ToolValidationContext Context { get; }
}

/// <summary>Where tool-context validation failed.</summary>
public sealed class ToolValidationContext
{
    /// <summary>Creates context.</summary>
    public ToolValidationContext(string field, string entityName)
    {
        Field = field ?? string.Empty;
        EntityName = entityName ?? string.Empty;
    }

    /// <summary>Field name. Tool context uses <c>tool context</c>.</summary>
    public string Field { get; }

    /// <summary>Tool name.</summary>
    public string EntityName { get; }
}

/// <summary>Validates tool context. Maps to <c>validateToolContext</c>.</summary>
public static class ToolContexts
{
    private static readonly JsonElement NullElement = JsonDocument
        .Parse("null")
        .RootElement.Clone();

    /// <summary>
    /// Validates a tool context with the context schema of a tool. Returns <paramref name="context"/> as it is when
    /// <paramref name="contextSchema"/> is null. Otherwise returns the context that the schema produces, with its defaults applied.
    /// A missing context is validated as a JSON <c>null</c>.
    /// </summary>
    /// <exception cref="TypeValidationException">The schema rejected the context.</exception>
    public static JsonElement? ValidateToolContext(
        string toolName,
        JsonElement? context,
        Func<JsonElement, JsonElement>? contextSchema
    )
    {
        if (contextSchema is null)
        {
            return context;
        }

        try
        {
            return contextSchema(context ?? NullElement);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            throw new TypeValidationException(context, "tool context", toolName);
        }
    }

    /// <summary>
    /// Returns <paramref name="context"/> when <paramref name="contextSchema"/> is null.
    /// Otherwise returns it when the schema accepts it, and throws <see cref="TypeValidationException"/> when it does not.
    /// </summary>
    public static T ValidateToolContext<T>(string toolName, T context, Func<T, bool>? contextSchema)
    {
        if (contextSchema is null)
        {
            return context;
        }

        if (!contextSchema(context))
        {
            throw new TypeValidationException(context, "tool context", toolName);
        }

        return context;
    }
}
