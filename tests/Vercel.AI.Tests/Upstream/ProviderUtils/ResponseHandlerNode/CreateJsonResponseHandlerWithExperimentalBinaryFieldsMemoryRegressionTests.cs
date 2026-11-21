// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Buffers.Text;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandlerNode;

/// <summary>
/// Port of <c>response-handler.node.test.ts</c> &gt; <c>createJsonResponseHandler with experimental_binaryFields memory regression</c>.
/// Upstream runs a child Node process with <c>--max-old-space-size=48</c>. The port measures the bytes the handler allocates
/// (<see cref="GC.GetTotalAllocatedBytes(bool)"/>) while it reads a body that is generated lazily, and checks that they stay
/// well below what reading the body into text and parsing it needs. The collection does not run in parallel, so no other test
/// allocates in the window.
/// </summary>
[Collection("BinaryFieldsMemory")]
public sealed class CreateJsonResponseHandlerWithExperimentalBinaryFieldsMemoryRegressionTests
{
    private const string Prefix =
        "packages/provider-utils/src/response-handler.node.test.ts::createJsonResponseHandler with experimental_binaryFields memory regression::";

    private const int ImageMiB = 24;
    private const int ImageCount = 2;
    private const int ImageBytes = ImageMiB * 1024 * 1024;
    private const int PieceBytes = 48 * 1024;
    private const long MiB = 1024 * 1024;

    // Two 24 MiB images are 64 MiB of base64 text, 128 MiB as a string: reading the body into text and parsing it allocates
    // more than 192 MiB. The decoded bytes are 48 MiB. The scanner joins its decoded segments once, so the budget is three
    // times the decoded bytes, and rejects reading the body into text.
    private const long DecodeBudget = 3L * ImageCount * ImageBytes;

    private static readonly string[] BinaryFields = ["images[*]"];

    private static void Piece(int imageIndex, int pieceIndex, Span<byte> bytes)
    {
        var state = unchecked((uint)((imageIndex + 1) * -1640531535 + pieceIndex * 40503));
        if (state == 0)
        {
            state = 1;
        }

        for (var index = 0; index < bytes.Length; index++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            bytes[index] = (byte)(state & 0xff);
        }
    }

    private static HttpResponseMessage Respond(Stream body)
    {
        var content = new StreamContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static async Task<(T Result, long Allocated)> Measure<T>(Func<Task<T>> run)
    {
        GC.Collect();
        var before = GC.GetTotalAllocatedBytes(true);
        var result = await run();
        return (result, GC.GetTotalAllocatedBytes(true) - before);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "decodes 2 x 24 MiB images within a 48 MiB heap",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Decodes_2_x_24_MiB_images_within_a_48_MiB_heap()
    {
        var handler = ResponseHandlers.CreateJsonResponseHandler<object?>(
            validate: null,
            experimentalBinaryFields: BinaryFields
        );

        var (result, allocated) = await Measure(async () =>
        {
            using var response = Respond(new ImagesStream(ImageCount));
            return await handler(response, "https://example.com", new { }, default);
        });

        var images = (List<object?>)((Dictionary<string, object?>)result.Value!)["images"]!;
        Assert.Equal(ImageCount, images.Count);
        var expected = new byte[PieceBytes];
        for (var imageIndex = 0; imageIndex < ImageCount; imageIndex++)
        {
            var decoded = Assert.IsType<byte[]>(images[imageIndex]);
            Assert.Equal(ImageBytes, decoded.Length);
            for (var at = 0; at * PieceBytes < ImageBytes; at++)
            {
                Piece(imageIndex, at, expected);
                Assert.True(
                    decoded.AsSpan(at * PieceBytes, PieceBytes).SequenceEqual(expected),
                    "image " + imageIndex + " differs at piece " + at
                );
            }
        }

        Assert.True(
            allocated < DecodeBudget,
            "allocated " + allocated / MiB + " MiB, budget " + DecodeBudget / MiB + " MiB"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "reports a Standard Schema validator that rejects a 24 MiB image within a 48 MiB heap",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_a_Standard_Schema_validator_that_rejects_a_24_MiB_image_within_a_48_MiB_heap()
    {
        // The validator's error is made from the value it is given, and holds the decoded bytes. They must not end up in
        // the message: serialized byte by byte they are more than 300 MB of text.
        var handler = ResponseHandlers.CreateJsonResponseHandler<object?>(
            validate: value =>
            {
                var images = (List<object?>)((Dictionary<string, object?>)value!)["images"]!;
                var error = new FormatException("images[0] must be a string");
                error.Data["data"] = images[0];
                throw error;
            },
            experimentalBinaryFields: BinaryFields
        );

        var (error, allocated) = await Measure(async () =>
        {
            using var response = Respond(new ImagesStream(1));
            return await Record.ExceptionAsync(() =>
                handler(response, "https://example.com", new { }, default)
            );
        });

        var message = Assert.IsType<ApiException>(error).InnerException?.Message ?? "";
        Assert.Contains("images[0] must be a string", message);
        Assert.True(
            message.Length < 2000,
            "unexpected: " + message[..Math.Min(200, message.Length)]
        );
        Assert.True(allocated < 3L * ImageBytes, "allocated " + allocated / MiB + " MiB");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves many empty and short matched strings within a 48 MiB heap",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_many_empty_and_short_matched_strings_within_a_48_MiB_heap()
    {
        // Ordinary-sized chunks with many small values must keep skeleton bookkeeping proportional to bytes,
        // rather than allocating retained buffers per value.
        const int blockCount = 24;
        var handler = ResponseHandlers.CreateJsonResponseHandler<object?>(
            validate: null,
            experimentalBinaryFields: BinaryFields
        );

        var (result, allocated) = await Measure(async () =>
        {
            using var response = Respond(new SmallValuesStream(blockCount));
            return await handler(response, "https://example.com", new { }, default);
        });

        var images = (List<object?>)((Dictionary<string, object?>)result.Value!)["images"]!;
        Assert.Equal(blockCount * 1024 * 3, images.Count);
        string[] expected = ["", "AA==", "YQ=="];
        for (var index = 0; index < images.Count; index++)
        {
            Assert.Equal(expected[index % 3], images[index]);
        }

        Assert.True(allocated < 48 * MiB, "allocated " + allocated / MiB + " MiB");
    }

    /// <summary>
    /// A read-only stream that generates <c>{"images":["...","..."]}</c> as it is read, so it holds no more than a piece of it.
    /// </summary>
    private sealed class ImagesStream(int imageCount) : GeneratedStream
    {
        private readonly byte[] _piece = new byte[PieceBytes];
        private int _image;
        private int _pieceIndex;
        private string _phase = "open";

        protected override bool Next(byte[] chunk, out int length)
        {
            switch (_phase)
            {
                case "open":
                    _phase = "data";
                    return Text("{\"images\":[\"", chunk, out length);
                case "data":
                    var size = Math.Min(PieceBytes, ImageBytes - _pieceIndex * PieceBytes);
                    Piece(_image, _pieceIndex, _piece.AsSpan(0, size));
                    Base64.EncodeToUtf8(_piece.AsSpan(0, size), chunk, out _, out length);
                    _pieceIndex++;
                    if (_pieceIndex * PieceBytes >= ImageBytes)
                    {
                        _pieceIndex = 0;
                        _image++;
                        _phase = _image == imageCount ? "close" : "next";
                    }

                    return true;
                case "next":
                    _phase = "data";
                    return Text("\",\"", chunk, out length);
                case "close":
                    _phase = "done";
                    return Text("\"]}", chunk, out length);
                default:
                    length = 0;
                    return false;
            }
        }
    }

    private sealed class SmallValuesStream(int blockCount) : GeneratedStream
    {
        private static readonly string Block = string.Join(
            ",",
            Enumerable.Repeat("\"\",\"AA==\",\"YQ==\"", 1024)
        );

        private int _index = -1;

        protected override bool Next(byte[] chunk, out int length)
        {
            var index = _index++;
            if (index < 0)
            {
                return Text("{\"images\":[", chunk, out length);
            }

            if (index < blockCount)
            {
                return Text((index > 0 ? "," : "") + Block, chunk, out length);
            }

            if (index == blockCount)
            {
                return Text("]}", chunk, out length);
            }

            length = 0;
            return false;
        }
    }

    private abstract class GeneratedStream : Stream
    {
        private readonly byte[] _chunk = new byte[PieceBytes * 2];
        private int _length;
        private int _offset;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_offset == _length)
            {
                if (!Next(_chunk, out _length))
                {
                    return 0;
                }

                _offset = 0;
            }

            var read = Math.Min(count, _length - _offset);
            Array.Copy(_chunk, _offset, buffer, offset, read);
            _offset += read;
            return read;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken
        ) => Task.FromResult(Read(buffer, offset, count));

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        protected abstract bool Next(byte[] chunk, out int length);

        protected static bool Text(string text, byte[] chunk, out int length)
        {
            length = Encoding.UTF8.GetBytes(text, 0, text.Length, chunk, 0);
            return true;
        }
    }
}

[CollectionDefinition("BinaryFieldsMemory", DisableParallelization = true)]
public sealed class BinaryFieldsMemoryCollection;
