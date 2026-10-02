// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GetErrorMessageTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > null and undefined::should return \"unknown error\" for null",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_unknown_error_for_null()
    {
        Assert.Equal("unknown error", ErrorMessages.GetErrorMessage(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > null and undefined::should return \"unknown error\" for undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_unknown_error_for_undefined()
    {
        Assert.Equal("unknown error", ErrorMessages.GetErrorMessage(Vercel.AI.ProviderUtils.JsUndefined.Value));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > string errors::should return the string as-is",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_a_string_as_is()
    {
        Assert.Equal("something went wrong", ErrorMessages.GetErrorMessage("something went wrong"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > string errors::should return an empty string as-is",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_string_as_is()
    {
        Assert.Equal(string.Empty, ErrorMessages.GetErrorMessage(string.Empty));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should include the Error type prefix for a basic Error",
        Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_error_type_prefix()
    {
        Assert.Equal("Error: API crashed", ErrorMessages.GetErrorMessage(new Error("API crashed")));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should include the TypeError prefix",
        Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_type_error_prefix()
    {
        Assert.Equal("TypeError: invalid argument", ErrorMessages.GetErrorMessage(new TypeError("invalid argument")));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should include the RangeError prefix",
        Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_range_error_prefix()
    {
        Assert.Equal("RangeError: out of bounds", ErrorMessages.GetErrorMessage(new RangeError("out of bounds")));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should return just the error name when message is empty",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_error_name_when_the_message_is_empty()
    {
        Assert.Equal("Error", ErrorMessages.GetErrorMessage(new Error(string.Empty)));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should return just the type name when TypeError message is empty",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_type_name_when_the_type_error_message_is_empty()
    {
        Assert.Equal("TypeError", ErrorMessages.GetErrorMessage(new TypeError(string.Empty)));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should handle custom error subclasses",
        Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_custom_exception_name()
    {
        Assert.Equal("CustomError: custom failure", ErrorMessages.GetErrorMessage(new CustomError("custom failure")));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should respect custom toString() overrides",
        Coverage = UpstreamCoverage.Covered)]
    public void Respects_a_custom_to_string_override()
    {
        Assert.Equal("API Error 429: rate limited", ErrorMessages.GetErrorMessage(new MyApiError("rate limited", 429)));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > Error instances::should handle custom error subclass with empty message",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_custom_name_when_the_message_is_empty()
    {
        Assert.Equal("CustomError", ErrorMessages.GetErrorMessage(new CustomError()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify plain objects",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializes_plain_objects()
    {
        var error = new Dictionary<string, string>
        {
            ["code"] = "FAIL",
            ["detail"] = "oops",
        };

        Assert.Equal("{\"code\":\"FAIL\",\"detail\":\"oops\"}", ErrorMessages.GetErrorMessage(error));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify numbers",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializes_numbers()
    {
        Assert.Equal("42", ErrorMessages.GetErrorMessage(42));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify booleans",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializes_booleans()
    {
        Assert.Equal("false", ErrorMessages.GetErrorMessage(false));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider/src/errors/get-error-message.test.ts::getErrorMessage > other types::should JSON.stringify arrays",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializes_arrays()
    {
        Assert.Equal("[\"a\",\"b\"]", ErrorMessages.GetErrorMessage(new[] { "a", "b" }));
    }

    private sealed class Error : Exception
    {
        public Error(string message)
            : base(message)
        {
        }
    }

    private sealed class TypeError : Exception
    {
        public TypeError(string message)
            : base(message)
        {
        }
    }

    private sealed class RangeError : Exception
    {
        public RangeError(string message)
            : base(message)
        {
        }
    }

    private sealed class CustomError : Exception
    {
        public CustomError()
            : base(string.Empty)
        {
        }

        public CustomError(string message)
            : base(message)
        {
        }
    }

    private sealed class MyApiError : Exception
    {
        public MyApiError(string message, int code)
            : base(message)
        {
            Code = code;
        }

        public int Code { get; }

        public override string ToString()
        {
            return "API Error " + Code + ": " + Message;
        }
    }
}
