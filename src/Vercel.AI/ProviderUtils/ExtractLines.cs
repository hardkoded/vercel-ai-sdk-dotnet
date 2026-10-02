// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Extracts a 1-based inclusive line range. Maps to <c>extractLines</c>.</summary>
public static class Lines
{
    /// <summary>
    /// Returns <paramref name="text"/> unchanged when both bounds are omitted.
    /// Line endings are detected in the order CR LF, LF, CR.
    /// </summary>
    public static string ExtractLines(string text, int? startLine = null, int? endLine = null)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (startLine is null && endLine is null)
        {
            return text;
        }

        var lineEnding = "\n";
        if (text.IndexOf("\r\n") >= 0)
        {
            lineEnding = "\r\n";
        }
        else if (text.IndexOf('\n') >= 0)
        {
            lineEnding = "\n";
        }
        else if (text.IndexOf('\r') >= 0)
        {
            lineEnding = "\r";
        }

        var lines = text.Split(new[] { lineEnding }, StringSplitOptions.None);
        var start = Math.Max(1, startLine ?? 1) - 1;
        var end = Math.Min(lines.Length, endLine ?? lines.Length);
        if (end < start)
        {
            return string.Empty;
        }

        var slice = new string[end - start];
        Array.Copy(lines, start, slice, 0, slice.Length);
        return string.Join(lineEnding, slice);
    }
}
