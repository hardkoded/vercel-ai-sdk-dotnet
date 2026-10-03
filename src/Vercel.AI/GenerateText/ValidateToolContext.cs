// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

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
