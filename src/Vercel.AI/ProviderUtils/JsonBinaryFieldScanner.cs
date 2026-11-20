// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// The compiled paths, a trie that is walked while scanning. The edges of a node are the bytes of an object key
/// (a key ends with its closing quote), the items of an array, and "a path ends here".
/// </summary>
public sealed class BinaryFieldPaths
{
    internal Dictionary<byte, BinaryFieldPaths> Keys { get; } = new();

    internal BinaryFieldPaths? Items { get; set; }

    internal bool Ends { get; set; }
}

/// <summary>The body with extracted strings replaced by placeholders, and the extracted bytes.</summary>
public sealed class JsonBinaryFieldScanResult
{
    /// <summary>Creates a scan result.</summary>
    public JsonBinaryFieldScanResult(string text, IReadOnlyDictionary<string, byte[]> binaries)
    {
        Text = text;
        Binaries = binaries;
    }

    /// <summary>The body with extracted strings replaced by placeholders.</summary>
    public string Text { get; }

    /// <summary>The extracted bytes by the placeholder text that stands in for them.</summary>
    public IReadOnlyDictionary<string, byte[]> Binaries { get; }
}

/// <summary>
/// Streaming extraction of large base64 strings from a JSON response body.
/// A JSON response that carries an image or a file is mostly one very long base64 string. The scanner reads the body
/// chunk by chunk: strings at the configured paths are decoded to bytes while they stream in, and the text that is
/// handed to the JSON parser (the "skeleton") holds a short placeholder in their place.
/// A string is extracted only if it is canonical standard-alphabet base64 (length divisible by 4, <c>=</c> only as the
/// final padding, zero trailing bits, no escape sequences or whitespace). Everything else stays a string.
/// Maps to <c>json-binary-field-scanner.ts</c>.
/// </summary>
public sealed class JsonBinaryFieldScanner
{
    /// <summary>Strings that decode to fewer bytes stay strings. Maps to <c>DEFAULT_BINARY_FIELD_MIN_BYTES</c>.</summary>
    public const int DefaultBinaryFieldMinBytes = 64 * 1024;

    private const byte Quote = 0x22;
    private const byte Backslash = 0x5c;
    private const byte EqualsSign = 0x3d;
    private const byte Comma = 0x2c;
    private const byte Colon = 0x3a;
    private const byte LeftBrace = 0x7b;
    private const byte RightBrace = 0x7d;
    private const byte LeftBracket = 0x5b;
    private const byte RightBracket = 0x5d;

    // Where the scanner is.
    private const int Outside = 0; // between tokens
    private const int Key = 1; // in an object key
    private const int Skip = 2; // in a string value that stays as it is
    private const int Blob = 3; // in a string value that is decoded while it streams in

    private const int PieceSize = 64 * 1024;

    private static readonly Regex PathPattern = new(
        @"\A(?:[A-Za-z0-9_-]+|\[\*])(?:\.[A-Za-z0-9_-]+|\[\*])*\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private static readonly Regex SegmentPattern = new(
        @"\[\*]|[A-Za-z0-9_-]+",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    // Six-bit value of each base64 character, 64 for everything else.
    private static readonly byte[] Sextet = CreateSextet();

    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    private readonly BinaryFieldPaths _paths;
    private readonly int _minBytes;

    // The skeleton, as bytes: it is decoded once at the end, like the regular path decodes the whole body.
    private readonly List<byte[]> _pieces = [];
    private readonly Dictionary<string, byte[]> _binaries = new(StringComparer.Ordinal);

    // JSON structure. Only the containers on a path get a frame; the ones that are not are counted in _skipped.
    private readonly List<Frame> _frames = [];

    private byte[] _piece = new byte[PieceSize];
    private int _pieceLength;
    private int _size;
    private int _skipped;
    private int _mode = Outside;
    private bool _escaped;
    private byte _last; // the last `{`, `,` or `:` outside of strings
    private BinaryFieldPaths? _key;

    // The candidate string: whole groups of four characters are decoded into segments,
    // the characters of an unfinished group are kept in _bits.
    private List<(byte[] Buffer, int Length)> _segments = [];
    private byte[] _segment = [];
    private int _at; // bytes used in _segment
    private int _total; // bytes decoded
    private int _bits;
    private int _chars; // characters in _bits
    private int _padding; // `=` characters seen

    /// <summary>Creates a scanner for the compiled <paramref name="paths"/>. Maps to <c>createJsonBinaryFieldScanner</c>.</summary>
    public JsonBinaryFieldScanner(BinaryFieldPaths paths, int minBytes = DefaultBinaryFieldMinBytes)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _minBytes = minBytes;
    }

    /// <summary>
    /// Compiles paths such as <c>images[*]</c> or <c>content[*].data.data</c>: plain object keys (letters, digits, <c>_</c> and <c>-</c>)
    /// joined by <c>.</c>, and <c>[*]</c> for every element of an array. Keys are compared with the raw bytes of the key in the body,
    /// so a key written with an escape sequence never matches. Maps to <c>compileBinaryFieldPaths</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentError">A path is not valid.</exception>
    public static BinaryFieldPaths CompileBinaryFieldPaths(IEnumerable<string> paths)
    {
        if (paths is null)
        {
            throw new ArgumentNullException(nameof(paths));
        }

        var root = new BinaryFieldPaths();
        foreach (var path in paths)
        {
            if (!PathPattern.IsMatch(path))
            {
                throw new InvalidArgumentError(
                    "experimental_binaryFields",
                    path,
                    "Invalid binary field path \"" + path + "\""
                );
            }

            var node = root;
            foreach (Match segment in SegmentPattern.Matches(path))
            {
                if (segment.Value == "[*]")
                {
                    node = node.Items ??= new BinaryFieldPaths();
                    continue;
                }

                foreach (var edge in Encoding.UTF8.GetBytes(segment.Value + "\""))
                {
                    if (!node.Keys.TryGetValue(edge, out var next))
                    {
                        next = new BinaryFieldPaths();
                        node.Keys[edge] = next;
                    }

                    node = next;
                }
            }

            node.Ends = true;
        }

        return root;
    }

    /// <summary>Reads the next chunk of the body. Maps to <c>push</c>.</summary>
    public void Push(byte[] chunk, int count)
    {
        if (chunk is null)
        {
            throw new ArgumentNullException(nameof(chunk));
        }

        // The first byte of the chunk that is not in the skeleton yet.
        var from = 0;
        var length = count;

        for (var index = 0; index < length; index++)
        {
            var value = chunk[index];

            if (_mode == Blob)
            {
                // The characters of the string, with the unfinished group in local variables.
                // The loop ends at the first byte that is not one.
                if (_padding == 0)
                {
                    var pending = _bits;
                    var counted = _chars;
                    int sextet;
                    for (; index < length && (sextet = Sextet[chunk[index]]) < 64; index++)
                    {
                        pending = (pending << 6) | sextet;
                        if (++counted == 4)
                        {
                            if (_at + 3 > _segment.Length)
                            {
                                Grow();
                            }

                            _segment[_at++] = (byte)(pending >> 16);
                            _segment[_at++] = (byte)(pending >> 8);
                            _segment[_at++] = (byte)pending;
                            _total += 3;
                            pending = 0;
                            counted = 0;
                        }
                    }

                    _bits = pending;
                    _chars = counted;
                }

                if (index >= length)
                {
                    break;
                }

                value = chunk[index];
                if (value == Quote)
                {
                    CloseBlob();
                    from = index;
                }
                else if (value == EqualsSign && _chars > 1 && _padding < 4 - _chars)
                {
                    // Padding completes a group of two or three characters.
                    _padding++;
                }
                else
                {
                    // Not canonical. The string goes on as a regular string, from this byte.
                    Restore();
                    _mode = Skip;
                    from = index--;
                }
            }
            else if (_mode != Outside)
            {
                if (_mode == Key)
                {
                    _key =
                        _key is not null && _key.Keys.TryGetValue(value, out var next)
                            ? next
                            : null;
                }

                if (_escaped)
                {
                    _escaped = false;
                }
                else if (value == Backslash)
                {
                    _escaped = true;
                }
                else if (value == Quote)
                {
                    if (_mode == Key)
                    {
                        _frames[_frames.Count - 1].Next = _key;
                    }

                    _mode = Outside;
                }
            }
            else if (value == Quote)
            {
                var frame = _frames.Count > 0 ? _frames[_frames.Count - 1] : null;
                if (_skipped > 0)
                {
                    _mode = Skip;
                }
                else if (frame?.Keys is not null && (_last == LeftBrace || _last == Comma))
                {
                    _mode = Key;
                    _key = frame.Keys;
                }
                else if ((frame is not null ? frame.Next : _paths)?.Ends == true)
                {
                    _mode = Blob;

                    // The opening quote still belongs to the skeleton.
                    Emit(chunk, from, index + 1);
                }
                else
                {
                    _mode = Skip;
                }
            }
            else if (value == LeftBrace || value == LeftBracket)
            {
                _last = value;
                var frame = _frames.Count > 0 ? _frames[_frames.Count - 1] : null;
                var node =
                    _skipped > 0 ? null
                    : frame is not null ? frame.Next
                    : _paths;
                var items = node?.Items;
                if (value == LeftBrace && node is not null)
                {
                    _frames.Add(new Frame { Keys = node });
                }
                else if (value == LeftBracket && items is not null)
                {
                    _frames.Add(new Frame { Keys = null, Next = items });
                }
                else
                {
                    _skipped++;
                }
            }
            else if (value == RightBrace || value == RightBracket)
            {
                if (_skipped > 0)
                {
                    _skipped--;
                }
                else if (_frames.Count > 0)
                {
                    _frames.RemoveAt(_frames.Count - 1);
                }
            }
            else if (value == Comma || value == Colon)
            {
                _last = value;
            }
        }

        if (_mode != Blob)
        {
            Emit(chunk, from, length);
        }
    }

    /// <summary>
    /// Ends the body. A body that ends inside a candidate string is returned as it was received, so that the regular parser reports the truncation.
    /// Maps to <c>finish</c>.
    /// </summary>
    public JsonBinaryFieldScanResult Finish()
    {
        if (_mode == Blob)
        {
            Restore();
        }

        var bytes = Join(_pieces, _piece, _pieceLength, _size);
        return new JsonBinaryFieldScanResult(Encoding.UTF8.GetString(bytes), _binaries);
    }

    /// <summary>
    /// Replaces the placeholders in a parsed value with the bytes they stand for. Objects and arrays are copied, the parsed value is not modified.
    /// The result holds <c>null</c>, <see cref="bool"/>, <see cref="double"/>, <see cref="string"/>, <see cref="byte"/> arrays,
    /// <see cref="List{T}"/> of objects, and <see cref="Dictionary{TKey, TValue}"/> of objects. A placeholder holds a random number,
    /// so a string that equals one is ours. Maps to <c>replaceBinaryFieldPlaceholders</c>.
    /// </summary>
    public static object? ReplaceBinaryFieldPlaceholders(
        JsonElement value,
        IReadOnlyDictionary<string, byte[]> binaries
    )
    {
        if (binaries is null)
        {
            throw new ArgumentNullException(nameof(binaries));
        }

        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                var text = value.GetString()!;
                return binaries.TryGetValue(text, out var bytes) ? bytes : text;
            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in value.EnumerateArray())
                {
                    list.Add(ReplaceBinaryFieldPlaceholders(item, binaries));
                }

                return list;
            case JsonValueKind.Object:
                var map = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var property in value.EnumerateObject())
                {
                    map[property.Name] = ReplaceBinaryFieldPlaceholders(property.Value, binaries);
                }

                return map;
            case JsonValueKind.Number:
                return value.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            default:
                return null;
        }
    }

    private static byte[] CreateSextet()
    {
        var table = new byte[256];
        for (var index = 0; index < table.Length; index++)
        {
            table[index] = 64;
        }

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        for (var index = 0; index < 64; index++)
        {
            table[alphabet[index]] = (byte)index;
        }

        return table;
    }

    private static byte[] Join(List<byte[]> parts, byte[] last, int lastLength, int length)
    {
        var bytes = new byte[length];
        var at = 0;
        foreach (var part in parts)
        {
            Buffer.BlockCopy(part, 0, bytes, at, part.Length);
            at += part.Length;
        }

        Buffer.BlockCopy(last, 0, bytes, at, lastLength);
        return bytes;
    }

    private void Add(byte[] bytes, int start, int end)
    {
        // Coalesce small fragments: retaining a buffer for each matched string would make metadata grow
        // with the number of values, even empty ones.
        var offset = start;
        while (offset < end)
        {
            var length = Math.Min(_piece.Length - _pieceLength, end - offset);
            Buffer.BlockCopy(bytes, offset, _piece, _pieceLength, length);
            _pieceLength += length;
            offset += length;
            _size += length;
            if (_pieceLength == _piece.Length)
            {
                _pieces.Add(_piece);
                _piece = new byte[PieceSize];
                _pieceLength = 0;
            }
        }
    }

    // Adds the part of a chunk to the skeleton. It is copied: a view on the chunk would retain all of it.
    private void Emit(byte[] chunk, int start, int end) => Add(chunk, start, end);

    private void Reset()
    {
        _segments = [];
        _segment = [];
        _at = 0;
        _total = 0;
        _bits = 0;
        _chars = 0;
        _padding = 0;
    }

    // Segments grow with the string, so that a short string does not pin a large buffer.
    private void Grow()
    {
        if (_at > 0)
        {
            _segments.Add((_segment, _at));
        }

        _segment = new byte[Math.Min(Math.Max(_total, 4096), 4 << 20)];
        _at = 0;
    }

    // The unfinished group, as bytes of a group of three padded with zeros.
    private int Rest() => _bits << ((4 - _chars) * 6);

    // The string was not extracted: its characters go back into the skeleton.
    private void Restore()
    {
        foreach (var (buffer, length) in _segments)
        {
            AddBase64(buffer, length);
        }

        AddBase64(_segment, _at);
        var group = Rest();
        var text =
            Convert
                .ToBase64String([(byte)(group >> 16), (byte)(group >> 8), (byte)group])
                .Substring(0, _chars) + new string('=', _padding);
        var encoded = Encoding.ASCII.GetBytes(text);
        Add(encoded, 0, encoded.Length);
        Reset();
    }

    private void AddBase64(byte[] buffer, int length)
    {
        var encoded = Encoding.ASCII.GetBytes(Convert.ToBase64String(buffer, 0, length));
        Add(encoded, 0, encoded.Length);
    }

    // The closing quote of a candidate string was reached.
    private void CloseBlob()
    {
        _mode = Outside;
        var tail = _chars > 0 ? _chars - 1 : 0;

        if (
            _padding == (4 - _chars) % 4
            && (_bits & ((1 << ((4 - _chars) * 2)) - 1)) == 0
            && _total + tail >= _minBytes
        )
        {
            var group = Rest();
            var bytes = new byte[_total + tail];
            var at = 0;
            foreach (var (buffer, length) in _segments)
            {
                Buffer.BlockCopy(buffer, 0, bytes, at, length);
                at += length;
            }

            Buffer.BlockCopy(_segment, 0, bytes, at, _at);
            for (var index = 0; index < tail; index++)
            {
                bytes[_total + index] = (byte)(group >> (16 - 8 * index));
            }

            var placeholder = string.Format(
                CultureInfo.InvariantCulture,
                "[binary data: {0} bytes; ref {1}-{2}]",
                bytes.Length,
                RandomNumber(),
                RandomNumber()
            );
            _binaries[placeholder] = bytes;
            var encoded = Encoding.UTF8.GetBytes(placeholder);
            Add(encoded, 0, encoded.Length);
            Reset();
        }
        else
        {
            Restore();
        }
    }

    private static uint RandomNumber()
    {
        var bytes = new byte[4];
        Rng.GetBytes(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private sealed class Frame
    {
        // The node whose edges match the next key; null in an array.
        public BinaryFieldPaths? Keys { get; set; }

        // The node of the value that is read next.
        public BinaryFieldPaths? Next { get; set; }
    }
}
