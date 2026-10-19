// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>How <see cref="MessagePruning.PruneMessages"/> removes reasoning from assistant messages.</summary>
public enum PruneReasoning
{
    /// <summary>Keep all reasoning.</summary>
    None,

    /// <summary>Remove reasoning from every assistant message.</summary>
    All,

    /// <summary>Remove reasoning from every assistant message except the last message.</summary>
    BeforeLastMessage,
}

/// <summary>Whether <see cref="MessagePruning.PruneMessages"/> keeps messages whose content is empty after pruning.</summary>
public enum PruneEmptyMessages
{
    /// <summary>Remove messages whose content is empty.</summary>
    Remove,

    /// <summary>Keep messages whose content is empty.</summary>
    Keep,
}

/// <summary>
/// One rule for pruning tool calls, results, errors, and approvals. Maps to an entry of the <c>toolCalls</c> array,
/// <c>{ type: 'all' | 'before-last-message' | 'before-last-N-messages', tools }</c>.
/// </summary>
public sealed class PruneToolCallsRule
{
    private PruneToolCallsRule(int? keepLastMessagesCount, IReadOnlyList<string>? tools)
    {
        KeepLastMessagesCount = keepLastMessagesCount;
        Tools = tools;
    }

    /// <summary>Prunes in all messages. Maps to <c>type: 'all'</c>.</summary>
    /// <param name="tools">Names of the tools to prune. Null prunes every tool.</param>
    public static PruneToolCallsRule All(IReadOnlyList<string>? tools = null) => new PruneToolCallsRule(null, tools);

    /// <summary>Prunes before the last message. Maps to <c>type: 'before-last-message'</c>.</summary>
    /// <param name="tools">Names of the tools to prune. Null prunes every tool.</param>
    public static PruneToolCallsRule BeforeLastMessage(IReadOnlyList<string>? tools = null) => new PruneToolCallsRule(1, tools);

    /// <summary>Prunes before the last <paramref name="count"/> messages. Maps to <c>type: 'before-last-N-messages'</c>.</summary>
    /// <param name="count">Number of trailing messages to keep unpruned.</param>
    /// <param name="tools">Names of the tools to prune. Null prunes every tool.</param>
    public static PruneToolCallsRule BeforeLastMessages(int count, IReadOnlyList<string>? tools = null)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        return new PruneToolCallsRule(count, tools);
    }

    /// <summary>Number of trailing messages to keep, or null for none (<c>all</c>).</summary>
    public int? KeepLastMessagesCount { get; }

    /// <summary>Names of the tools to prune, or null for every tool.</summary>
    public IReadOnlyList<string>? Tools { get; }
}

/// <summary>Options of <see cref="MessagePruning.PruneMessages"/>.</summary>
public sealed class PruneMessagesOptions
{
    /// <summary>The messages to prune.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; set; } = Array.Empty<ModelMessage>();

    /// <summary>How to remove reasoning from assistant messages. Default is <see cref="PruneReasoning.None"/>.</summary>
    public PruneReasoning Reasoning { get; set; } = PruneReasoning.None;

    /// <summary>How to prune tool calls, results, errors, and approvals. Default is no rules.</summary>
    public IReadOnlyList<PruneToolCallsRule> ToolCalls { get; set; } = Array.Empty<PruneToolCallsRule>();

    /// <summary>Whether to keep or remove messages whose content is empty after pruning. Default is <see cref="PruneEmptyMessages.Remove"/>.</summary>
    public PruneEmptyMessages EmptyMessages { get; set; } = PruneEmptyMessages.Remove;
}

/// <summary>Message pruning. Maps to <c>pruneMessages</c>.</summary>
public static class MessagePruning
{
    private enum EntryKind
    {
        Other,
        ToolCall,
        ToolResult,
        ApprovalRequest,
        ApprovalResponse,
    }

    private readonly struct Entry
    {
        public Entry(object source, EntryKind kind, string? toolCallId, string? toolName, string? approvalId)
        {
            Source = source;
            Kind = kind;
            ToolCallId = toolCallId;
            ToolName = toolName;
            ApprovalId = approvalId;
        }

        public object Source { get; }

        public EntryKind Kind { get; }

        public string? ToolCallId { get; }

        public string? ToolName { get; }

        public string? ApprovalId { get; }
    }

    /// <summary>
    /// Prunes reasoning, tool calls, results, errors, and approvals from a list of messages. The input is not changed.
    /// A tool message upstream holds several parts. Here its parts are separate consecutive tool messages, and
    /// they count as one message for <see cref="PruneReasoning.BeforeLastMessage"/> and the trailing-message rules.
    /// </summary>
    /// <param name="options">The messages and the pruning settings.</param>
    /// <returns>The pruned messages.</returns>
    public static IReadOnlyList<ModelMessage> PruneMessages(PruneMessagesOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var groups = Group(options.Messages ?? Array.Empty<ModelMessage>());

        if (options.Reasoning != PruneReasoning.None)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                if (groups[index].Count == 1
                    && groups[index][0] is AssistantModelMessage assistant
                    && !(options.Reasoning == PruneReasoning.BeforeLastMessage && index == groups.Count - 1))
                {
                    groups[index] = new List<ModelMessage> { Filter(assistant, part => part is not (AssistantReasoningPart or AssistantReasoningFilePart)) };
                }
            }
        }

        foreach (var rule in options.ToolCalls ?? Array.Empty<PruneToolCallsRule>())
        {
            groups = ApplyRule(groups, rule);
        }

        var result = new List<ModelMessage>();
        foreach (var group in groups)
        {
            if (options.EmptyMessages == PruneEmptyMessages.Remove && IsEmpty(group))
            {
                continue;
            }

            result.AddRange(group);
        }

        return result;
    }

    private static List<List<ModelMessage>> ApplyRule(List<List<ModelMessage>> groups, PruneToolCallsRule rule)
    {
        var keepLast = rule.KeepLastMessagesCount;

        // scan kept messages to identify tool calls and approvals that need to be kept:
        var keptToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        var keptApprovalIds = new HashSet<string>(StringComparer.Ordinal);

        if (keepLast is > 0)
        {
            for (var index = Math.Max(0, groups.Count - keepLast.Value); index < groups.Count; index++)
            {
                foreach (var entry in EntriesOf(groups[index]))
                {
                    if (entry.Kind is EntryKind.ToolCall or EntryKind.ToolResult)
                    {
                        keptToolCallIds.Add(entry.ToolCallId!);
                    }
                    else if (entry.Kind is EntryKind.ApprovalRequest or EntryKind.ApprovalResponse)
                    {
                        keptApprovalIds.Add(entry.ApprovalId!);
                    }
                }
            }
        }

        // Global maps, because an approval response lives in a tool message apart from its request.
        var toolCallIdToToolName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in groups)
        {
            foreach (var entry in EntriesOf(group))
            {
                if (entry.Kind is EntryKind.ToolCall or EntryKind.ToolResult)
                {
                    toolCallIdToToolName[entry.ToolCallId!] = entry.ToolName!;
                }
            }
        }

        var approvalIdToToolCallId = new Dictionary<string, string>(StringComparer.Ordinal);
        var approvalIdToToolName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in groups)
        {
            foreach (var entry in EntriesOf(group))
            {
                if (entry.Kind == EntryKind.ApprovalRequest)
                {
                    approvalIdToToolCallId[entry.ApprovalId!] = entry.ToolCallId!;
                    if (toolCallIdToToolName.TryGetValue(entry.ToolCallId!, out var name))
                    {
                        approvalIdToToolName[entry.ApprovalId!] = name;
                    }
                }
            }
        }

        // Approval requests depend on their originating tool call. Trace through the request of every kept approval.
        foreach (var approvalId in keptApprovalIds)
        {
            if (approvalIdToToolCallId.TryGetValue(approvalId, out var toolCallId))
            {
                keptToolCallIds.Add(toolCallId);
            }
        }

        bool Keep(Entry entry)
        {
            // keep non-tool parts:
            if (entry.Kind == EntryKind.Other)
            {
                return true;
            }

            // keep parts that are associated with a tool call or approval that needs to be kept:
            if (entry.Kind is EntryKind.ToolCall or EntryKind.ToolResult)
            {
                if (keptToolCallIds.Contains(entry.ToolCallId!))
                {
                    return true;
                }
            }
            else if (keptApprovalIds.Contains(entry.ApprovalId!))
            {
                return true;
            }

            // keep parts that are not associated with a tool that should be removed:
            string? partToolName;
            if (entry.Kind is EntryKind.ToolCall or EntryKind.ToolResult)
            {
                partToolName = entry.ToolName;
            }
            else
            {
                partToolName = approvalIdToToolName.TryGetValue(entry.ApprovalId!, out var name) ? name : null;
            }

            return rule.Tools != null && partToolName != null && !ContainsName(rule.Tools, partToolName);
        }

        var pruned = new List<List<ModelMessage>>(groups.Count);
        for (var index = 0; index < groups.Count; index++)
        {
            var group = groups[index];
            if (keepLast is > 0 && index >= groups.Count - keepLast.Value)
            {
                pruned.Add(group);
                continue;
            }

            pruned.Add(FilterGroup(group, Keep));
        }

        return pruned;
    }

    private static List<ModelMessage> FilterGroup(List<ModelMessage> group, Func<Entry, bool> keep)
    {
        if (group.Count == 1 && group[0] is AssistantModelMessage assistant)
        {
            var kept = new List<AssistantContentPart>();
            foreach (var entry in EntriesOf(group))
            {
                if (keep(entry))
                {
                    kept.Add((AssistantContentPart)entry.Source);
                }
            }

            return new List<ModelMessage> { Rebuild(assistant, kept) };
        }

        if (group.Count > 0 && group[0].Role == "tool")
        {
            var entries = EntriesOf(group);
            var kept = new List<ModelMessage>();
            foreach (var entry in entries)
            {
                if (keep(entry))
                {
                    kept.Add((ModelMessage)entry.Source);
                }
            }

            return kept.Count == entries.Count ? group : kept;
        }

        return group;
    }

    private static AssistantModelMessage Filter(AssistantModelMessage message, Func<AssistantContentPart, bool> keep)
    {
        var kept = new List<AssistantContentPart>();
        foreach (var part in message.Parts)
        {
            if (keep(part))
            {
                kept.Add(part);
            }
        }

        return Rebuild(message, kept);
    }

    private static AssistantModelMessage Rebuild(AssistantModelMessage message, List<AssistantContentPart> kept)
    {
        if (kept.Count == message.Parts.Count)
        {
            return message;
        }

        return new AssistantModelMessage(kept) { ProviderOptions = message.ProviderOptions };
    }

    private static List<Entry> EntriesOf(List<ModelMessage> group)
    {
        var entries = new List<Entry>();
        foreach (var message in group)
        {
            switch (message)
            {
                case AssistantModelMessage assistant:
                    foreach (var part in assistant.Parts)
                    {
                        switch (part)
                        {
                            case AssistantToolCallPart call:
                                entries.Add(new Entry(part, EntryKind.ToolCall, call.ToolCall.ToolCallId, call.ToolCall.ToolName, null));
                                break;
                            case AssistantToolResultPart result:
                                entries.Add(new Entry(part, EntryKind.ToolResult, result.ToolCallId, result.ToolName, null));
                                break;
                            case AssistantToolApprovalRequestPart request:
                                entries.Add(new Entry(part, EntryKind.ApprovalRequest, request.ToolCallId, null, request.ApprovalId));
                                break;
                            default:
                                entries.Add(new Entry(part, EntryKind.Other, null, null, null));
                                break;
                        }
                    }

                    break;
                case ToolApprovalResponseModelMessage response:
                    entries.Add(new Entry(response, EntryKind.ApprovalResponse, null, null, response.ApprovalId));
                    break;
                case ToolModelMessage tool:
                    foreach (var result in tool.Results)
                    {
                        entries.Add(new Entry(result, EntryKind.ToolResult, result.ToolCallId, result.ToolName, null));
                    }

                    break;
            }
        }

        return entries;
    }

    // One logical message per input message, except that consecutive tool messages are one message, as upstream's tool message holds several parts.
    private static List<List<ModelMessage>> Group(IReadOnlyList<ModelMessage> messages)
    {
        var groups = new List<List<ModelMessage>>();
        foreach (var message in messages)
        {
            if (message.Role == "tool" && groups.Count > 0 && groups[groups.Count - 1].Count > 0 && groups[groups.Count - 1][0].Role == "tool")
            {
                groups[groups.Count - 1].Add(message);
            }
            else
            {
                groups.Add(new List<ModelMessage> { message });
            }
        }

        return groups;
    }

    private static bool IsEmpty(List<ModelMessage> group)
    {
        if (group.Count == 0)
        {
            return true;
        }

        return group[0] switch
        {
            AssistantModelMessage assistant => assistant.Parts.Count == 0,
            UserModelMessage user => user.Content.Count == 0,
            SystemModelMessage system => system.Content.Length == 0,
            _ => false,
        };
    }

    private static bool ContainsName(IReadOnlyList<string> names, string name)
    {
        foreach (var candidate in names)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
