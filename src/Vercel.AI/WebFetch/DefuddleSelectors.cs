// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace Vercel.AI.WebFetch;

/// <summary>Removes clutter by selector. Maps to <c>defuddle/src/removals/selectors.ts</c>.</summary>
internal static class DefuddleSelectors
{
    private static readonly Regex ResponsiveShow = new(
        @"^(sm|md|lg|xl|2xl|min-\[|max-\[):(?:block|flex|grid|inline|table|contents)",
        RegexOptions.None,
        TimeSpan.FromSeconds(2)
    );

    private static readonly Regex Heading = new("^H[1-6]$", RegexOptions.None);

    private static readonly Regex Delimiter = new(@"[\s_\-:.]", RegexOptions.None);

    public static void RemoveBySelector(
        IDocument document,
        bool removeExact,
        bool removePartial,
        IElement? mainContent,
        bool skipHiddenExactSelectors
    )
    {
        // Elements to remove, in document order, with the way they matched.
        var toRemove = new List<IElement>();
        var seen = new HashSet<IElement>();

        if (removeExact)
        {
            foreach (
                var element in document.QuerySelectorAll(DefuddleConstants.ExactSelectorsJoined)
            )
            {
                if (element.ParentElement == null && element.Parent == null)
                {
                    continue;
                }

                if (skipHiddenExactSelectors)
                {
                    var hiddenAncestor = element.Closest(DefuddleConstants.HiddenExactSkipSelector);
                    var role = (element.GetAttribute("role") ?? string.Empty).ToLowerInvariant();
                    if (
                        element.Matches(DefuddleConstants.HiddenExactSelector)
                        || (hiddenAncestor != null && role == "dialog")
                    )
                    {
                        continue;
                    }
                }

                // Skip elements inside code blocks (e.g. syntax highlighting spans).
                if (element.Closest("pre, code") != null)
                {
                    continue;
                }

                // Skip elements with responsive show classes (e.g. "hidden sm:flex").
                if (
                    element.Matches(DefuddleConstants.HiddenExactSelector)
                    && HasResponsiveShowClass(element.GetAttribute("class") ?? string.Empty)
                )
                {
                    continue;
                }

                if (seen.Add(element))
                {
                    toRemove.Add(element);
                }
            }
        }

        if (removePartial)
        {
            foreach (
                var element in document.QuerySelectorAll(DefuddleConstants.TestAttributesSelector)
            )
            {
                if (seen.Contains(element))
                {
                    continue;
                }

                // Skip code elements and elements containing code blocks, where class names
                // indicate language or syntax, not page structure.
                var tag = element.TagName;
                if (
                    tag == "CODE"
                    || tag == "PRE"
                    || element.QuerySelector("pre") != null
                    || element.Closest("code, pre") != null
                )
                {
                    continue;
                }

                // For headings, only the class is checked. IDs are auto-slugs and data-testid
                // values (e.g. "article-header") cause false positives.
                var isHeading = Heading.IsMatch(tag);
                var className = element.GetAttribute("class") ?? string.Empty;
                var attributes = (
                    isHeading
                        ? className
                        : className
                            + " "
                            + (element.GetAttribute("data-component") ?? string.Empty)
                            + " "
                            + (element.GetAttribute("data-test") ?? string.Empty)
                            + " "
                            + (element.GetAttribute("data-testid") ?? string.Empty)
                            + " "
                            + (element.GetAttribute("data-test-id") ?? string.Empty)
                            + " "
                            + (element.GetAttribute("data-qa") ?? string.Empty)
                            + " "
                            + (element.GetAttribute("data-cy") ?? string.Empty)
                ).ToLowerInvariant();

                // A delimited id is substring-matched like the other attributes. A delimiter-less id
                // is usually a content anchor concatenated from heading words, so it must equal a
                // selector token outright.
                var id = isHeading ? string.Empty : (element.Id ?? string.Empty).ToLowerInvariant();

                var hasAttributes = attributes.Trim().Length > 0;
                if (!hasAttributes && id.Length == 0)
                {
                    continue;
                }

                var attributesMatch =
                    hasAttributes && DefuddleConstants.PartialSelectorsRegex.IsMatch(attributes);
                var idMatch =
                    id.Length > 0
                    && (
                        Delimiter.IsMatch(id)
                            ? DefuddleConstants.PartialSelectorsRegex.IsMatch(id)
                            : DefuddleConstants.PartialSelectorsAnchoredRegex.IsMatch(id)
                    );

                if ((attributesMatch || idMatch) && seen.Add(element))
                {
                    toRemove.Add(element);
                }
            }
        }

        foreach (var element in toRemove)
        {
            // Skip elements that are ancestors of the main content to avoid disconnecting it.
            if (mainContent != null && element.Contains(mainContent))
            {
                continue;
            }

            // Anchor links inside headings are handled by the heading transform.
            if (element.TagName == "A" && element.Closest("h1, h2, h3, h4, h5, h6") != null)
            {
                continue;
            }

            // Skip footnote list containers, their parents, and immediate children.
            if (
                element.Matches(DefuddleConstants.FootnoteListSelectorsJoined)
                || element.QuerySelector(DefuddleConstants.FootnoteListSelectorsJoined) != null
                || element.ParentElement?.Matches(DefuddleConstants.FootnoteListSelectorsJoined)
                    == true
            )
            {
                continue;
            }

            // Buttons that contain media: keep only the media and discard the button chrome.
            if (element.TagName == "BUTTON" && element.QuerySelector("img, picture, video") != null)
            {
                var parent = element.ParentElement;
                if (parent != null)
                {
                    foreach (var media in element.QuerySelectorAll("img, picture, video").ToList())
                    {
                        parent.InsertBefore(media, element);
                    }

                    element.Remove();
                }

                continue;
            }

            // Unwrap buttons in inline context to preserve text.
            if (
                element.TagName == "BUTTON"
                && element.Closest("p, li, td, th, span, h1, h2, h3, h4, h5, h6") != null
            )
            {
                var parent = element.ParentElement;
                if (parent != null)
                {
                    foreach (var child in element.ChildNodes.ToList())
                    {
                        parent.InsertBefore(child, element);
                    }

                    element.Remove();
                }

                continue;
            }

            element.Remove();
        }
    }

    private static bool HasResponsiveShowClass(string className)
    {
        foreach (var token in className.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (ResponsiveShow.IsMatch(token))
            {
                return true;
            }
        }

        return false;
    }
}
