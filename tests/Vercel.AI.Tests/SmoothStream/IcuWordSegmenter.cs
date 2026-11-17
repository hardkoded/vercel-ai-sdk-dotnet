// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.InteropServices;
using Vercel.AI.GenerateText;

namespace Vercel.AI.Tests.SmoothStream;

/// <summary>
/// An ICU word <see cref="ISegmenter"/> for the tests. It stands in for <c>new Intl.Segmenter(locale, { granularity: 'word' })</c>.
/// .NET does not expose ICU's break iterator, so this binds to the ICU library the runtime already loads.
/// </summary>
internal sealed class IcuWordSegmenter(string locale) : ISegmenter
{
    private const int UbrkWord = 1;
    private const int UbrkDone = -1;

    private static readonly Lazy<IcuApi> s_api = new(IcuApi.Load);

    public IEnumerable<string> Segment(string input)
    {
        var api = s_api.Value;
        var error = 0;
        var text = Marshal.StringToHGlobalUni(input);
        var iterator = IntPtr.Zero;
        try
        {
            iterator = api.Open(UbrkWord, locale, text, input.Length, ref error);
            if (error > 0)
            {
                throw new InvalidOperationException($"ubrk_open failed with UErrorCode {error}.");
            }

            var start = 0;
            var segments = new List<string>();
            int end;
            while ((end = api.Next(iterator)) != UbrkDone)
            {
                segments.Add(input[start..end]);
                start = end;
            }

            return segments;
        }
        finally
        {
            if (iterator != IntPtr.Zero)
            {
                api.Close(iterator);
            }

            Marshal.FreeHGlobal(text);
        }
    }

    private sealed class IcuApi
    {
        private readonly OpenDelegate _open;
        private readonly NextDelegate _next;
        private readonly CloseDelegate _close;

        private IcuApi(OpenDelegate open, NextDelegate next, CloseDelegate close)
        {
            _open = open;
            _next = next;
            _close = close;
        }

        private delegate IntPtr OpenDelegate(
            int type,
            [MarshalAs(UnmanagedType.LPStr)] string locale,
            IntPtr text,
            int length,
            ref int error
        );

        private delegate int NextDelegate(IntPtr iterator);

        private delegate void CloseDelegate(IntPtr iterator);

        public static IcuApi Load()
        {
            for (var version = 99; version >= 50; version--)
            {
                var libraryName =
                    OperatingSystem.IsWindows() ? $"icuuc{version}.dll"
                    : OperatingSystem.IsMacOS() ? $"libicuuc.{version}.dylib"
                    : $"libicuuc.so.{version}";
                if (!NativeLibrary.TryLoad(libraryName, out var library))
                {
                    continue;
                }

                if (
                    NativeLibrary.TryGetExport(library, $"ubrk_open_{version}", out var open)
                    && NativeLibrary.TryGetExport(library, $"ubrk_next_{version}", out var next)
                    && NativeLibrary.TryGetExport(library, $"ubrk_close_{version}", out var close)
                )
                {
                    return new IcuApi(
                        Marshal.GetDelegateForFunctionPointer<OpenDelegate>(open),
                        Marshal.GetDelegateForFunctionPointer<NextDelegate>(next),
                        Marshal.GetDelegateForFunctionPointer<CloseDelegate>(close)
                    );
                }
            }

            throw new InvalidOperationException("No ICU library with ubrk_* exports was found.");
        }

        public IntPtr Open(int type, string locale, IntPtr text, int length, ref int error) =>
            _open(type, locale, text, length, ref error);

        public int Next(IntPtr iterator) => _next(iterator);

        public void Close(IntPtr iterator) => _close(iterator);
    }
}
