// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Examples.Lib;

/// <summary>Port of examples/ai-functions/src/lib/apply-diff.ts.</summary>
public static class ApplyDiffHelper
{
    private const string EndPatch = "*** End Patch";
    private const string EndFile = "*** End of File";

    private static readonly string[] EndSectionMarkers =
    [
        EndPatch,
        "*** Update File:",
        "*** Delete File:",
        "*** Add File:",
        EndFile,
    ];

    private static readonly string[] SectionTerminators =
    [
        EndPatch,
        "*** Update File:",
        "*** Delete File:",
        "*** Add File:",
    ];

    /// <summary>
    /// Applies a headerless V4A diff to the provided file content.
    /// Mode "default" patches an existing file using V4A sections ("@@" + +/-/space lines).
    /// Mode "create" is the create-file syntax that requires every line to start with "+".
    /// Trailing newlines of the original file are preserved. Throws when the diff cannot be applied cleanly.
    /// </summary>
    public static string ApplyDiff(
        string input,
        string diff,
        ApplyDiffMode mode = ApplyDiffMode.Default
    )
    {
        var diffLines = NormalizeDiffLines(diff);

        if (mode == ApplyDiffMode.Create)
        {
            return ParseCreateDiff(diffLines);
        }

        var (chunks, _) = ParseUpdateDiff(diffLines, input);
        return ApplyChunks(input, chunks);
    }

    private static List<string> NormalizeDiffLines(string diff)
    {
        var lines = Regex
            .Split(diff, "\r?\n", RegexOptions.None, TimeSpan.FromSeconds(5))
            .Select(line => line.EndsWith('\r') ? line[..^1] : line)
            .ToList();
        if (lines.Count > 0 && lines[^1] == string.Empty)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }

    private static bool IsDone(ParserState state, string[] prefixes)
    {
        if (state.Index >= state.Lines.Count)
        {
            return true;
        }

        var current = state.Lines[state.Index];
        return prefixes.Any(p => current.StartsWith(p, StringComparison.Ordinal));
    }

    private static string ReadStr(ParserState state, string prefix)
    {
        if (
            state.Index < state.Lines.Count
            && state.Lines[state.Index].StartsWith(prefix, StringComparison.Ordinal)
        )
        {
            var current = state.Lines[state.Index];
            state.Index += 1;
            return current[prefix.Length..];
        }

        return string.Empty;
    }

    private static string ParseCreateDiff(List<string> lines)
    {
        var parser = new ParserState([.. lines, EndPatch]);
        var output = new List<string>();

        while (!IsDone(parser, SectionTerminators))
        {
            var line = parser.Lines[parser.Index];
            parser.Index += 1;
            if (!line.StartsWith('+'))
            {
                throw new InvalidOperationException($"Invalid Add File Line: {line}");
            }

            output.Add(line[1..]);
        }

        return string.Join("\n", output);
    }

    private static (List<Chunk> Chunks, int Fuzz) ParseUpdateDiff(List<string> lines, string input)
    {
        var parser = new ParserState([.. lines, EndPatch]);
        var inputLines = input.Split('\n');
        var chunks = new List<Chunk>();
        var cursor = 0;

        while (!IsDone(parser, EndSectionMarkers))
        {
            var anchor = ReadStr(parser, "@@ ");
            var hasBareAnchor =
                anchor.Length == 0
                && parser.Index < parser.Lines.Count
                && parser.Lines[parser.Index] == "@@";
            if (hasBareAnchor)
            {
                parser.Index += 1;
            }

            if (!(anchor.Length > 0 || hasBareAnchor || cursor == 0))
            {
                throw new InvalidOperationException(
                    $"Invalid Line:\n{(parser.Index < parser.Lines.Count ? parser.Lines[parser.Index] : "undefined")}"
                );
            }

            if (anchor.Trim().Length > 0)
            {
                cursor = AdvanceCursorToAnchor(anchor, inputLines, cursor, parser);
            }

            var (nextContext, sectionChunks, endIndex, eof) = ReadSection(
                parser.Lines,
                parser.Index
            );
            var nextContextText = string.Join("\n", nextContext);
            var (newIndex, fuzz) = FindContext(inputLines, nextContext, cursor, eof);

            if (newIndex == -1)
            {
                if (eof)
                {
                    throw new InvalidOperationException(
                        $"Invalid EOF Context {cursor}:\n{nextContextText}"
                    );
                }

                throw new InvalidOperationException(
                    $"Invalid Context {cursor}:\n{nextContextText}"
                );
            }

            parser.Fuzz += fuzz;
            foreach (var ch in sectionChunks)
            {
                chunks.Add(ch with { OrigIndex = ch.OrigIndex + newIndex });
            }

            cursor = newIndex + nextContext.Count;
            parser.Index = endIndex;
        }

        return (chunks, parser.Fuzz);
    }

    private static int AdvanceCursorToAnchor(
        string anchor,
        string[] inputLines,
        int cursor,
        ParserState parser
    )
    {
        var found = false;

        if (!inputLines.Take(cursor).Any(s => s == anchor))
        {
            for (var i = cursor; i < inputLines.Length; i += 1)
            {
                if (inputLines[i] == anchor)
                {
                    cursor = i + 1;
                    found = true;
                    break;
                }
            }
        }

        if (!found && !inputLines.Take(cursor).Any(s => s.Trim() == anchor.Trim()))
        {
            for (var i = cursor; i < inputLines.Length; i += 1)
            {
                if (inputLines[i].Trim() == anchor.Trim())
                {
                    cursor = i + 1;
                    parser.Fuzz += 1;
                    found = true;
                    break;
                }
            }
        }

        return cursor;
    }

    private static (
        List<string> NextContext,
        List<Chunk> SectionChunks,
        int EndIndex,
        bool Eof
    ) ReadSection(List<string> lines, int startIndex)
    {
        var context = new List<string>();
        var delLines = new List<string>();
        var insLines = new List<string>();
        var sectionChunks = new List<Chunk>();
        var mode = SectionMode.Keep;
        var index = startIndex;
        var origIndex = index;

        while (index < lines.Count)
        {
            var raw = lines[index];
            if (
                raw.StartsWith("@@", StringComparison.Ordinal)
                || raw.StartsWith(EndPatch, StringComparison.Ordinal)
                || raw.StartsWith("*** Update File:", StringComparison.Ordinal)
                || raw.StartsWith("*** Delete File:", StringComparison.Ordinal)
                || raw.StartsWith("*** Add File:", StringComparison.Ordinal)
                || raw.StartsWith(EndFile, StringComparison.Ordinal)
            )
            {
                break;
            }

            if (raw == "***")
            {
                break;
            }

            if (raw.StartsWith("***", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Invalid Line: {raw}");
            }

            index += 1;
            var lastMode = mode;
            var line = raw;
            if (line.Length == 0)
            {
                line = " ";
            }

            switch (line[0])
            {
                case '+':
                    mode = SectionMode.Add;
                    break;
                case '-':
                    mode = SectionMode.Delete;
                    break;
                case ' ':
                    mode = SectionMode.Keep;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Line: {line}");
            }

            line = line[1..];

            var switchingToContext = mode == SectionMode.Keep && lastMode != mode;
            if (switchingToContext && (insLines.Count > 0 || delLines.Count > 0))
            {
                sectionChunks.Add(new Chunk(context.Count - delLines.Count, delLines, insLines));
                delLines = [];
                insLines = [];
            }

            if (mode == SectionMode.Delete)
            {
                delLines.Add(line);
                context.Add(line);
            }
            else if (mode == SectionMode.Add)
            {
                insLines.Add(line);
            }
            else
            {
                context.Add(line);
            }
        }

        if (insLines.Count > 0 || delLines.Count > 0)
        {
            sectionChunks.Add(new Chunk(context.Count - delLines.Count, delLines, insLines));
        }

        if (index < lines.Count && lines[index] == EndFile)
        {
            index += 1;
            return (context, sectionChunks, index, true);
        }

        if (index == origIndex)
        {
            throw new InvalidOperationException(
                $"Nothing in this section - index={index} {(index < lines.Count ? lines[index] : "undefined")}"
            );
        }

        return (context, sectionChunks, index, false);
    }

    private static (int NewIndex, int Fuzz) FindContext(
        string[] lines,
        List<string> context,
        int start,
        bool eof
    )
    {
        if (eof)
        {
            var endStart = Math.Max(0, lines.Length - context.Count);
            var endMatch = FindContextCore(lines, context, endStart);
            if (endMatch.NewIndex != -1)
            {
                return endMatch;
            }

            var fallback = FindContextCore(lines, context, start);
            return (fallback.NewIndex, fallback.Fuzz + 10000);
        }

        return FindContextCore(lines, context, start);
    }

    private static (int NewIndex, int Fuzz) FindContextCore(
        string[] lines,
        List<string> context,
        int start
    )
    {
        if (context.Count == 0)
        {
            return (start, 0);
        }

        for (var i = start; i < lines.Length; i += 1)
        {
            if (EqualsSlice(lines, context, i, s => s))
            {
                return (i, 0);
            }
        }

        for (var i = start; i < lines.Length; i += 1)
        {
            if (EqualsSlice(lines, context, i, s => s.TrimEnd()))
            {
                return (i, 1);
            }
        }

        for (var i = start; i < lines.Length; i += 1)
        {
            if (EqualsSlice(lines, context, i, s => s.Trim()))
            {
                return (i, 100);
            }
        }

        return (-1, 0);
    }

    private static bool EqualsSlice(
        string[] source,
        List<string> target,
        int start,
        Func<string, string> mapFn
    )
    {
        if (start + target.Count > source.Length)
        {
            return false;
        }

        for (var i = 0; i < target.Count; i += 1)
        {
            if (mapFn(source[start + i]) != mapFn(target[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static string ApplyChunks(string input, List<Chunk> chunks)
    {
        var origLines = input.Split('\n');
        var destLines = new List<string>();
        var origIndex = 0;

        foreach (var chunk in chunks)
        {
            if (chunk.OrigIndex > origLines.Length)
            {
                throw new InvalidOperationException(
                    $"applyDiff: chunk.origIndex {chunk.OrigIndex} > input length {origLines.Length}"
                );
            }

            if (origIndex > chunk.OrigIndex)
            {
                throw new InvalidOperationException(
                    $"applyDiff: overlapping chunk at {chunk.OrigIndex} (cursor {origIndex})"
                );
            }

            destLines.AddRange(origLines.Skip(origIndex).Take(chunk.OrigIndex - origIndex));
            origIndex = chunk.OrigIndex;

            if (chunk.InsLines.Count > 0)
            {
                destLines.AddRange(chunk.InsLines);
            }

            origIndex += chunk.DelLines.Count;
        }

        destLines.AddRange(origLines.Skip(origIndex));
        return string.Join("\n", destLines);
    }

    private enum SectionMode
    {
        Keep,
        Add,
        Delete,
    }

    private sealed record Chunk(int OrigIndex, List<string> DelLines, List<string> InsLines);

    private sealed class ParserState(List<string> lines)
    {
        public List<string> Lines { get; } = lines;

        public int Index { get; set; }

        public int Fuzz { get; set; }
    }
}

/// <summary>Mode of <see cref="ApplyDiffHelper.ApplyDiff"/>.</summary>
public enum ApplyDiffMode
{
    /// <summary>Patch an existing file.</summary>
    Default,

    /// <summary>Create a file; every line must start with "+".</summary>
    Create,
}
