// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>
/// The approval status of a tool call: <c>not-applicable</c> (no approval needed), <c>approved</c> (approved automatically),
/// <c>denied</c> (denied automatically), or <c>user-approval</c> (the user must approve). Maps to <c>ToolApprovalStatus</c>.
/// For <c>approved</c> and <c>denied</c>, the reason is emitted on the approval response. For <c>user-approval</c>, it is
/// emitted on the approval request so it can be shown to the approver. A function that returns <see langword="null"/>
/// has the effect of <c>not-applicable</c>.
/// </summary>
public sealed class ToolApprovalStatus
{
    /// <summary>The tool does not require approval.</summary>
    public const string NotApplicableType = "not-applicable";

    /// <summary>The tool is approved automatically.</summary>
    public const string ApprovedType = "approved";

    /// <summary>The tool is denied automatically.</summary>
    public const string DeniedType = "denied";

    /// <summary>The tool requires user approval.</summary>
    public const string UserApprovalType = "user-approval";

    /// <summary>Creates a status. <paramref name="type"/> is one of the four type constants of this class.</summary>
    public ToolApprovalStatus(string type, string? reason = null)
    {
        if (type != NotApplicableType && type != ApprovedType && type != DeniedType && type != UserApprovalType)
        {
            throw new ArgumentException("Unknown tool approval status '" + type + "'.", nameof(type));
        }

        if (type == NotApplicableType && reason != null)
        {
            throw new ArgumentException("A not-applicable status has no reason.", nameof(reason));
        }

        Type = type;
        Reason = reason;
    }

    /// <summary>The status without a reason: <c>not-applicable</c>.</summary>
    public static ToolApprovalStatus NotApplicable { get; } = new(NotApplicableType);

    /// <summary>The status without a reason: <c>approved</c>.</summary>
    public static ToolApprovalStatus Approved { get; } = new(ApprovedType);

    /// <summary>The status without a reason: <c>denied</c>.</summary>
    public static ToolApprovalStatus Denied { get; } = new(DeniedType);

    /// <summary>The status without a reason: <c>user-approval</c>.</summary>
    public static ToolApprovalStatus UserApproval { get; } = new(UserApprovalType);

    /// <summary>One of <c>not-applicable</c>, <c>approved</c>, <c>denied</c>, and <c>user-approval</c>.</summary>
    public string Type { get; }

    /// <summary>Why the call was approved, denied, or needs approval.</summary>
    public string? Reason { get; }

    /// <summary>Converts a status string, such as <c>user-approval</c>, to a status without a reason.</summary>
    public static implicit operator ToolApprovalStatus(string type) => new(type);
}

/// <summary>The options a <see cref="SingleToolApprovalFunction"/> receives. Maps to the second argument of <c>SingleToolApprovalFunction</c>.</summary>
public sealed class SingleToolApprovalOptions
{
    /// <summary>Creates the options.</summary>
    public SingleToolApprovalOptions(string toolCallId, IReadOnlyList<ModelMessage> messages)
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    /// <summary>Id of the tool call.</summary>
    public string ToolCallId { get; }

    /// <summary>
    /// Messages that were sent to the language model to initiate the response that contained the tool call.
    /// They do not include the instructions nor the assistant response that contained the tool call.
    /// </summary>
    public IReadOnlyList<ModelMessage> Messages { get; }
}

/// <summary>
/// Determines the approval status of a call to one tool, from the input of the call. Returning <see langword="null"/> has the
/// effect of <c>not-applicable</c>. Maps to <c>SingleToolApprovalFunction</c>.
/// </summary>
public delegate Task<ToolApprovalStatus?> SingleToolApprovalFunction(JsonElement input, SingleToolApprovalOptions options);

/// <summary>The options a <see cref="GenericToolApprovalFunction"/> receives. Maps to the options of <c>GenericToolApprovalFunction</c>.</summary>
public sealed class GenericToolApprovalOptions
{
    /// <summary>Creates the options.</summary>
    public GenericToolApprovalOptions(GeneratedToolCall toolCall, IReadOnlyList<Tool>? tools, IReadOnlyList<ModelMessage> messages)
    {
        ToolCall = toolCall ?? throw new ArgumentNullException(nameof(toolCall));
        Tools = tools;
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    /// <summary>The tool call that needs approval.</summary>
    public GeneratedToolCall ToolCall { get; }

    /// <summary>All tools that are available for the model to call.</summary>
    public IReadOnlyList<Tool>? Tools { get; }

    /// <summary>
    /// Messages that were sent to the language model to initiate the response that contained the tool call.
    /// They do not include the instructions nor the assistant response that contained the tool call.
    /// </summary>
    public IReadOnlyList<ModelMessage> Messages { get; }
}

/// <summary>
/// Determines the approval status of any tool call. Returning <see langword="null"/> has the effect of <c>not-applicable</c>.
/// Maps to <c>GenericToolApprovalFunction</c>.
/// </summary>
public delegate Task<ToolApprovalStatus?> GenericToolApprovalFunction(GenericToolApprovalOptions options);

/// <summary>The approval setting of one tool: a status or a function that produces a status at runtime.</summary>
public sealed class ToolApprovalEntry
{
    /// <summary>Creates an entry with a fixed status.</summary>
    public ToolApprovalEntry(ToolApprovalStatus status)
    {
        Status = status ?? throw new ArgumentNullException(nameof(status));
    }

    /// <summary>Creates an entry whose status is produced for each call.</summary>
    public ToolApprovalEntry(SingleToolApprovalFunction function)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The fixed status, or null when <see cref="Function"/> is set.</summary>
    public ToolApprovalStatus? Status { get; }

    /// <summary>The function that produces the status, or null when <see cref="Status"/> is set.</summary>
    public SingleToolApprovalFunction? Function { get; }

    /// <summary>Converts a status to an entry.</summary>
    public static implicit operator ToolApprovalEntry(ToolApprovalStatus status) => new(status);
}

/// <summary>
/// Configures whether individual tools require approval before they can run. Either use a generic function that is
/// called for all tool calls, or set an approval status (or a function that produces one) for each tool.
/// It takes precedence over the approval settings of the tools. Maps to <c>ToolApprovalConfiguration</c>.
/// </summary>
/// <example>
/// <code>
/// var toolApproval = new ToolApprovalConfiguration
/// {
///     { "deleteFile", "user-approval" },
///     { "readFile", ToolApprovalStatus.Approved },
/// };
/// </code>
/// </example>
public sealed class ToolApprovalConfiguration : IEnumerable<KeyValuePair<string, ToolApprovalEntry>>
{
    private readonly Dictionary<string, ToolApprovalEntry> _tools = new(StringComparer.Ordinal);

    /// <summary>Creates a configuration with per-tool settings. Add them with <c>Add</c> or the indexer.</summary>
    public ToolApprovalConfiguration()
    {
    }

    /// <summary>Creates a configuration with a generic function that is called for all tool calls.</summary>
    public ToolApprovalConfiguration(GenericToolApprovalFunction function)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The generic function, or null for a per-tool configuration.</summary>
    public GenericToolApprovalFunction? Function { get; }

    /// <summary>Gets or sets the approval setting of a tool.</summary>
    public ToolApprovalEntry this[string toolName]
    {
        get => _tools[toolName];
        set => _tools[toolName] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Converts a generic function to a configuration.</summary>
    public static implicit operator ToolApprovalConfiguration(GenericToolApprovalFunction function) => new(function);

    /// <summary>Sets the status of a tool.</summary>
    public void Add(string toolName, ToolApprovalStatus status) => this[toolName] = new ToolApprovalEntry(status);

    /// <summary>Sets the function that produces the status of a tool.</summary>
    public void Add(string toolName, SingleToolApprovalFunction function) => this[toolName] = new ToolApprovalEntry(function);

    /// <summary>Returns the setting of a tool. It is not found for a tool that has none, whatever its name.</summary>
    public bool TryGetValue(string toolName, out ToolApprovalEntry? entry)
    {
        return _tools.TryGetValue(toolName, out entry);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, ToolApprovalEntry>> GetEnumerator() => _tools.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
