// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class ValidateTypesTests
{
    private const string File = "packages/provider-utils/src/validate-types.test.ts::";

    [Fact]
    [UpstreamTest(File + "validateTypes::should return validated object for valid input", Coverage = UpstreamCoverage.Covered)]
    public void Validate_types_returns_a_valid_object()
    {
        var value = JsonParsing.ValidateTypes(Parse("{\"name\":\"John\",\"age\":30}"), PersonSchema());
        JsonAssert.Equal(value, "{\"name\":\"John\",\"age\":30}");
    }

    [Fact]
    [UpstreamTest(File + "validateTypes::should throw TypeValidationError for invalid input", Coverage = UpstreamCoverage.Covered)]
    public void Validate_types_throws_for_a_string_age()
    {
        var input = Parse("{\"name\":\"John\",\"age\":\"30\"}");
        var error = Assert.Throws<TypeValidationException>(() => JsonParsing.ValidateTypes(input, PersonSchema()));
        JsonAssert.Equal(error.Value!.Value, "{\"name\":\"John\",\"age\":\"30\"}");
    }

    [Fact]
    [UpstreamTest(File + "safeValidateTypes::should return validated object for valid input", Coverage = UpstreamCoverage.Covered)]
    public void Safe_validate_returns_the_value()
    {
        var result = JsonParsing.SafeValidateTypes(Parse("{\"name\":\"John\",\"age\":30}"), PersonSchema());
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, "{\"name\":\"John\",\"age\":30}");
        JsonAssert.Equal(result.RawValue!.Value, "{\"name\":\"John\",\"age\":30}");
    }

    [Fact]
    [UpstreamTest(File + "safeValidateTypes::should return error object for invalid input", Coverage = UpstreamCoverage.Covered)]
    public void Safe_validate_returns_the_error()
    {
        var result = JsonParsing.SafeValidateTypes(Parse("{\"name\":\"John\",\"age\":\"30\"}"), PersonSchema());
        Assert.False(result.Success);
        var error = Assert.IsType<TypeValidationException>(result.Error);
        JsonAssert.Equal(error.Value!.Value, "{\"name\":\"John\",\"age\":\"30\"}");
        JsonAssert.Equal(result.RawValue!.Value, "{\"name\":\"John\",\"age\":\"30\"}");
    }

    private static JsonElement Parse(string json)
    {
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static JsonNode PersonSchema()
    {
        return JsonSchemas.Object(
            new[]
            {
                new KeyValuePair<string, JsonNode>("name", JsonSchemas.String()),
                new KeyValuePair<string, JsonNode>("age", JsonSchemas.Number()),
            },
            new[] { "name", "age" });
    }
}
