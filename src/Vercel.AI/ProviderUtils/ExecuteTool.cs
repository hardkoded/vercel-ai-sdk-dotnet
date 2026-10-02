// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;

namespace Vercel.AI.ProviderUtils;

/// <summary>Options passed to a tool execute function. Maps to <c>ToolExecutionOptions</c>.</summary>
public sealed class ToolExecutionOptions
{
    /// <summary>Tool call id.</summary>
    public string? ToolCallId { get; set; }

    /// <summary>Messages available to the tool.</summary>
    public IList<object?>? Messages { get; set; }

    /// <summary>Caller context.</summary>
    public IReadOnlyDictionary<string, object?>? Context { get; set; }
}

/// <summary>One streamed or final tool output. Maps to the objects yielded by <c>executeTool</c>.</summary>
public sealed class ToolExecution
{
    /// <summary>Creates an output. <paramref name="type"/> is <c>preliminary</c> or <c>final</c>.</summary>
    public ToolExecution(string type, object? output)
    {
        Type = type;
        Output = output;
    }

    /// <summary><c>preliminary</c> or <c>final</c>.</summary>
    public string Type { get; }

    /// <summary>Tool output.</summary>
    public object? Output { get; }
}

/// <summary>A tool that may expose an execute function. Maps to <c>Tool</c>.</summary>
public sealed class ExecutableToolDefinition
{
    /// <summary>Execute callback. Invoked with the tool as the receiver when it is an instance method.</summary>
    public Delegate? Execute { get; set; }
}

/// <summary>Runs tools. Maps to <c>isExecutableTool</c> and <c>executeTool</c>.</summary>
public static class ToolExecutionRunner
{
    /// <summary>True when <paramref name="tool"/> has an execute function.</summary>
    public static bool IsExecutableTool(ExecutableToolDefinition? tool)
    {
        return tool != null && tool.Execute != null;
    }

    /// <summary>
    /// Invokes <paramref name="tool"/> and yields preliminary values for an async sequence,
    /// then repeats the last value as final. A single result is yielded as final.
    /// </summary>
    public static async IAsyncEnumerable<ToolExecution> ExecuteTool(
        ExecutableToolDefinition tool,
        object? input,
        ToolExecutionOptions options)
    {
        if (tool is null)
        {
            throw new ArgumentNullException(nameof(tool));
        }

        if (tool.Execute is null)
        {
            throw new InvalidOperationException("Tool has no execute function.");
        }

        var result = tool.Execute.DynamicInvoke(input, options);
        if (IsAsyncIterable(result))
        {
            object? last = null;
            var enumerator = GetAsyncEnumerator(result!);
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    last = enumerator.Current;
                    yield return new ToolExecution("preliminary", last);
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            yield return new ToolExecution("final", last);
        }
        else
        {
            yield return new ToolExecution("final", await UnwrapAsync(result).ConfigureAwait(false));
        }
    }

    private static bool IsAsyncIterable(object? value)
    {
        return value is IAsyncEnumerable<object?>;
    }

    private static IAsyncEnumerator<object?> GetAsyncEnumerator(object value)
    {
        return ((IAsyncEnumerable<object?>)value).GetAsyncEnumerator();
    }

    private static async Task<object?> UnwrapAsync(object? result)
    {
        if (result is Task<object?> objectTask)
        {
            return await objectTask.ConfigureAwait(false);
        }

        if (result is Task task)
        {
            await task.ConfigureAwait(false);
            var type = task.GetType();
            if (type.IsGenericType)
            {
                var property = type.GetProperty("Result");
                return property?.GetValue(task);
            }

            return null;
        }

        return result;
    }
}
