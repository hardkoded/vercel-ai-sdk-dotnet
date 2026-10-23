// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Vercel.AI.WebFetch;

/// <summary>
/// Options for the HTML content extractor. Options that control the output format, logging, or network access are set by the tool.
/// Maps to <c>WebFetchDefuddleOptions</c>.
/// </summary>
public sealed class WebFetchDefuddleOptions
{
    /// <summary>CSS selector of the element that holds the main content. By default the extractor picks it.</summary>
    public string? ContentSelector { get; set; }

    /// <summary>Remove images from the result. Defaults to <c>false</c>.</summary>
    public bool RemoveImages { get; set; }

    /// <summary>Remove elements that are hidden from view. Defaults to <c>true</c>.</summary>
    public bool RemoveHiddenElements { get; set; } = true;
}

internal static class WebFetchHtml
{
    /// <summary>
    /// Larger HTML is truncated before conversion. Extraction time and memory grow quickly with document size.
    /// </summary>
    private const int MaxHtmlLength = 2_000_000;

    /// <summary>
    /// Extraction cost grows quickly with nesting depth. Real pages rarely exceed a depth of 100; deeper documents fall back to plain text.
    /// </summary>
    private const int MaxDomDepth = 512;

    private static readonly string[] AlwaysRemoved = { "script", "style", "noscript", "template", "iframe", "object", "embed", "svg", "canvas", "link", "meta" };

    private static readonly string[] BoilerplateSelectors =
    {
        "nav", "footer", "aside", "form", "dialog", "[role=navigation]", "[role=banner]", "[role=contentinfo]", "[role=complementary]", "body > header",
    };

    private static readonly string[] HiddenSelectors = { "[hidden]", "[aria-hidden=true]", "[style*='display:none']", "[style*='display: none']" };

    private static readonly string[] ContentCandidates = { "article", "main", "[role=main]", "#content", ".post-content", ".entry-content", ".article-body", ".content" };

    public static (string? Title, string Content) ToMarkdown(string html, string url, WebFetchDefuddleOptions? options)
    {
        var truncated = false;
        if (html.Length > MaxHtmlLength)
        {
            html = html.Substring(0, MaxHtmlLength);
            truncated = true;
        }

        var document = new HtmlParser().ParseDocument(html);

        string? title;
        string content;

        if (GetMaxDepth(document.DocumentElement) > MaxDomDepth)
        {
            title = NullIfEmpty(document.Title);
            content = GetPlainText(document);
        }
        else
        {
            options ??= new WebFetchDefuddleOptions();
            title = ExtractTitle(document);
            content = ExtractContent(document, url, options);
        }

        if (truncated)
        {
            content += "\n\n[The page exceeded " + MaxHtmlLength.ToString(CultureInfo.InvariantCulture) + " characters of HTML and was truncated before conversion.]";
        }

        return (title, content.Trim());
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }

    // Depth of the element tree, found without recursion so a hostile document cannot overflow the stack.
    private static int GetMaxDepth(IElement root)
    {
        var maxDepth = 0;
        var stack = new Stack<(IElement Element, int Depth)>();
        stack.Push((root, 1));

        while (stack.Count > 0)
        {
            var (element, depth) = stack.Pop();
            if (depth > maxDepth)
            {
                maxDepth = depth;
                if (maxDepth > MaxDomDepth)
                {
                    return maxDepth;
                }
            }

            for (var child = element.FirstElementChild; child != null; child = child.NextElementSibling)
            {
                stack.Push((child, depth + 1));
            }
        }

        return maxDepth;
    }

    private static string GetPlainText(IDocument document)
    {
        RemoveAll(document, string.Join(",", AlwaysRemoved));

        var root = (IElement?)document.Body ?? document.DocumentElement;
        var text = new StringBuilder();
        var stack = new Stack<INode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.NodeType == NodeType.Text)
            {
                text.Append(node.TextContent);
                continue;
            }

            for (var child = node.LastChild; child != null; child = child.PreviousSibling)
            {
                stack.Push(child);
            }
        }

        return NormalizeWhitespace(text.ToString());
    }

    // Fetched content is untrusted, so this runs in linear time: a run of whitespace without a newline becomes one space and
    // a run with a newline becomes one newline.
    private static string NormalizeWhitespace(string text)
    {
        var result = new StringBuilder(text.Length);
        var pendingSpace = false;
        var pendingNewline = false;

        foreach (var character in text)
        {
            if (character == '\n')
            {
                pendingNewline = true;
            }
            else if (char.IsWhiteSpace(character))
            {
                pendingSpace = true;
            }
            else
            {
                if (result.Length > 0)
                {
                    if (pendingNewline)
                    {
                        result.Append('\n');
                    }
                    else if (pendingSpace)
                    {
                        result.Append(' ');
                    }
                }

                pendingSpace = false;
                pendingNewline = false;
                result.Append(character);
            }
        }

        return result.ToString();
    }

    private static void RemoveAll(IParentNode root, string selector)
    {
        foreach (var element in root.QuerySelectorAll(selector).ToList())
        {
            element.Remove();
        }
    }

    // Drops a site name from the title, as in "Test Page | Example", when the first heading names the page.
    private static string? ExtractTitle(IDocument document)
    {
        var title = NullIfEmpty(document.Title?.Trim());
        if (title == null)
        {
            return null;
        }

        var heading = document.QuerySelector("h1")?.TextContent;
        heading = heading == null ? null : string.Join(" ", heading.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrEmpty(heading))
        {
            return title;
        }

        foreach (var separator in new[] { " | ", " - ", " – ", " — ", " · ", " :: ", " » " })
        {
            if (title.IndexOf(separator, StringComparison.Ordinal) < 0)
            {
                continue;
            }

            foreach (var segment in title.Split(new[] { separator }, StringSplitOptions.None))
            {
                if (string.Equals(segment.Trim(), heading, StringComparison.OrdinalIgnoreCase))
                {
                    return heading;
                }
            }
        }

        return title;
    }

    private static string ExtractContent(IDocument document, string url, WebFetchDefuddleOptions options)
    {
        RemoveAll(document, string.Join(",", AlwaysRemoved));
        if (options.RemoveHiddenElements)
        {
            RemoveAll(document, string.Join(",", HiddenSelectors));
        }

        IElement? main = null;
        if (!string.IsNullOrEmpty(options.ContentSelector))
        {
            main = document.QuerySelector(options.ContentSelector!);
        }

        if (main == null)
        {
            RemoveAll(document, string.Join(",", BoilerplateSelectors));
            main = FindMainContent(document);
        }

        if (main == null)
        {
            return string.Empty;
        }

        var writer = new MarkdownWriter(new Uri(url), options.RemoveImages);
        writer.RenderChildren(main);
        return writer.ToString();
    }

    private static IElement? FindMainContent(IDocument document)
    {
        foreach (var selector in ContentCandidates)
        {
            var matches = document.QuerySelectorAll(selector);
            if (matches.Length == 1 && matches[0].TextContent.Trim().Length > 0)
            {
                return matches[0];
            }
        }

        return document.Body;
    }

    // Converts the DOM to markdown. Documents deeper than MaxDomDepth never get here, so recursion stays bounded.
    private sealed class MarkdownWriter
    {
        private readonly StringBuilder _output = new();
        private readonly Uri _baseUri;
        private readonly bool _removeImages;
        private int _listDepth;
        private bool _inPre;

        public MarkdownWriter(Uri baseUri, bool removeImages)
        {
            _baseUri = baseUri;
            _removeImages = removeImages;
        }

        public override string ToString()
        {
            return Tidy(_output.ToString());
        }

        public void RenderChildren(INode parent)
        {
            for (var child = parent.FirstChild; child != null; child = child.NextSibling)
            {
                Render(child);
            }
        }

        private void Render(INode node)
        {
            if (node.NodeType == NodeType.Text)
            {
                AppendText(node.TextContent);
                return;
            }

            if (node is not IElement element)
            {
                return;
            }

            switch (element.LocalName)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    Block(() =>
                    {
                        _output.Append(new string('#', element.LocalName[1] - '0')).Append(' ');
                        RenderChildren(element);
                    });
                    break;
                case "p":
                case "div":
                case "section":
                case "article":
                case "main":
                case "header":
                case "figure":
                case "figcaption":
                case "details":
                case "summary":
                case "dl":
                case "dt":
                case "dd":
                case "address":
                    Block(() => RenderChildren(element));
                    break;
                case "br":
                    _output.Append("  \n");
                    break;
                case "hr":
                    Block(() => _output.Append("---"));
                    break;
                case "strong":
                case "b":
                    Wrap(element, "**");
                    break;
                case "em":
                case "i":
                    Wrap(element, "*");
                    break;
                case "del":
                case "s":
                    Wrap(element, "~~");
                    break;
                case "code":
                    Wrap(element, _inPre ? string.Empty : "`");
                    break;
                case "pre":
                    Block(() =>
                    {
                        _output.Append("```\n");
                        _inPre = true;
                        _output.Append(element.TextContent.TrimEnd('\n'));
                        _inPre = false;
                        _output.Append("\n```");
                    });
                    break;
                case "blockquote":
                    RenderBlockquote(element);
                    break;
                case "a":
                    RenderLink(element);
                    break;
                case "img":
                    RenderImage(element);
                    break;
                case "ul":
                case "ol":
                    RenderList(element);
                    break;
                case "table":
                    RenderTable(element);
                    break;
                default:
                    RenderChildren(element);
                    break;
            }
        }

        private void Block(Action render)
        {
            _output.Append("\n\n");
            render();
            _output.Append("\n\n");
        }

        private void Wrap(IElement element, string marker)
        {
            if (string.IsNullOrWhiteSpace(element.TextContent) && element.QuerySelector("img") == null)
            {
                return;
            }

            _output.Append(marker);
            RenderChildren(element);
            _output.Append(marker);
        }

        private void AppendText(string text)
        {
            if (_inPre)
            {
                _output.Append(text);
                return;
            }

            // Collapse whitespace like a browser does. Linear: one pass, no regular expression.
            var previousWasSpace = _output.Length > 0 && char.IsWhiteSpace(_output[_output.Length - 1]);
            foreach (var character in text)
            {
                if (char.IsWhiteSpace(character) && character != ' ')
                {
                    if (!previousWasSpace)
                    {
                        _output.Append(' ');
                    }

                    previousWasSpace = true;
                }
                else
                {
                    _output.Append(character);
                    previousWasSpace = false;
                }
            }
        }

        private string? Resolve(string? reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            Uri resolved;
            if (!Uri.TryCreate(_baseUri, reference!.Trim(), out resolved!))
            {
                return null;
            }

            return resolved.Scheme == "http" || resolved.Scheme == "https" ? resolved.AbsoluteUri : null;
        }

        private void RenderLink(IElement element)
        {
            var href = Resolve(element.GetAttribute("href"));
            if (href == null)
            {
                RenderChildren(element);
                return;
            }

            _output.Append('[');
            RenderChildren(element);
            _output.Append("](").Append(href).Append(')');
        }

        private void RenderImage(IElement element)
        {
            var source = Resolve(element.GetAttribute("src"));
            if (_removeImages || source == null)
            {
                return;
            }

            _output.Append("![").Append(element.GetAttribute("alt") ?? string.Empty).Append("](").Append(source).Append(')');
        }

        private void RenderList(IElement list)
        {
            var ordered = list.LocalName == "ol";
            var number = 1;
            if (ordered && int.TryParse(list.GetAttribute("start"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var start))
            {
                number = start;
            }

            _output.Append(_listDepth == 0 ? "\n\n" : "\n");
            _listDepth++;
            for (var child = list.FirstElementChild; child != null; child = child.NextElementSibling)
            {
                if (child.LocalName != "li")
                {
                    continue;
                }

                _output.Append(new string(' ', (_listDepth - 1) * 2)).Append(ordered ? number++.ToString(CultureInfo.InvariantCulture) + ". " : "- ");
                RenderChildren(child);
                _output.Append('\n');
            }

            _listDepth--;
            if (_listDepth == 0)
            {
                _output.Append('\n');
            }
        }

        private void RenderBlockquote(IElement element)
        {
            var inner = new MarkdownWriter(_baseUri, _removeImages);
            inner.RenderChildren(element);
            var lines = inner.ToString().Split('\n');
            Block(() => _output.Append(string.Join("\n", lines.Select(line => line.Length == 0 ? ">" : "> " + line))));
        }

        private void RenderTable(IElement table)
        {
            var rows = table.QuerySelectorAll("tr").Where(row => row.Closest("table") == table).ToList();
            if (rows.Count == 0)
            {
                return;
            }

            var cells = rows
                .Select(row => row.Children.Where(cell => cell.LocalName == "th" || cell.LocalName == "td")
                    .Select(cell =>
                    {
                        var inner = new MarkdownWriter(_baseUri, _removeImages);
                        inner.RenderChildren(cell);
                        return inner.ToString().Replace("\n", " ").Replace("|", "\\|");
                    })
                    .ToList())
                .Where(row => row.Count > 0)
                .ToList();
            if (cells.Count == 0)
            {
                return;
            }

            var width = cells.Max(row => row.Count);
            Block(() =>
            {
                for (var index = 0; index < cells.Count; index++)
                {
                    var row = cells[index];
                    while (row.Count < width)
                    {
                        row.Add(string.Empty);
                    }

                    _output.Append("| ").Append(string.Join(" | ", row)).Append(" |\n");
                    if (index == 0)
                    {
                        _output.Append("| ").Append(string.Join(" | ", Enumerable.Repeat("---", width))).Append(" |\n");
                    }
                }
            });
        }

        // Trims the line ends, drops runs of blank lines, and leaves fenced code as it is.
        private static string Tidy(string markdown)
        {
            var result = new StringBuilder(markdown.Length);
            var inFence = false;
            var blank = 0;
            foreach (var raw in markdown.Split('\n'))
            {
                var isFence = raw.StartsWith("```", StringComparison.Ordinal);
                var line = inFence && !isFence ? raw : raw.TrimEnd(' ', '\t', '\r');
                if (isFence)
                {
                    inFence = !inFence;
                }

                if (line.Length == 0 && !inFence)
                {
                    blank++;
                    if (blank > 1)
                    {
                        continue;
                    }
                }
                else
                {
                    blank = 0;
                }

                result.Append(line).Append('\n');
            }

            return result.ToString().Trim();
        }
    }
}
