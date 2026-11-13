// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>Validates extraction input and answers. Maps to <c>validate-extraction.ts</c>.</summary>
public static class ValidateExtraction
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

    private static readonly Regex Time = new Regex(@"^([01][0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?\z", RegexOptions.CultureInvariant, MatchTimeout);

    private static readonly Regex Datetime = new Regex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}T([01][0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9](\.[0-9]{1,9})?)?(Z|[+-]([01][0-9]|2[0-3]):[0-5][0-9])?\z", RegexOptions.CultureInvariant, MatchTimeout);

    private static readonly Regex DatePattern = new Regex(@"^([0-9]{4})-([0-9]{2})-([0-9]{2})\z", RegexOptions.CultureInvariant, MatchTimeout);

    /// <summary>Validates state, reference date, and fields before any model call. Maps to <c>validateExtractionInput</c>.</summary>
    public static void ValidateExtractionInput(DecisionState? state, IReadOnlyDictionary<string, ExtractionField>? fields, string? referenceDate = null)
    {
        Decide.ValidateDecisionState(state);

        if (referenceDate != null && !IsDate(referenceDate))
        {
            throw new InvalidArgumentException("referenceDate", referenceDate, "must be a YYYY-MM-DD calendar date");
        }

        if (fields == null || fields.Count == 0)
        {
            throw new InvalidArgumentException("fields", fields, "must be a nonempty field map");
        }

        foreach (var pair in fields)
        {
            var parameter = "fields." + pair.Key;
            var field = pair.Value;
            if (field == null)
            {
                throw new InvalidArgumentException(parameter, field, "field must not be null");
            }

            if (field.Decimals is { } decimals && !(decimals >= 0 && decimals <= 6))
            {
                throw new InvalidArgumentException(parameter, field, "decimals must be an integer from 0 to 6");
            }

            if (field.Type == "duration" && field.Unit != null && !ExtractionField.DurationUnits.Contains(field.Unit))
            {
                throw new InvalidArgumentException(parameter, field, "duration unit must be one of " + string.Join(", ", ExtractionField.DurationUnits));
            }

            if (field.Type == "list" && (field.Items == null || !ExtractionField.ListItems.Contains(field.Items)))
            {
                throw new InvalidArgumentException(parameter, field, "list items must be one of " + string.Join(", ", ExtractionField.ListItems));
            }
        }
    }

    /// <summary>
    /// Validates answers strictly. No partial results. Maps to <c>validateExtractionAnswers</c>.
    /// </summary>
    public static void ValidateExtractionAnswers(IReadOnlyDictionary<string, ExtractionField> fields, IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, ExtractionAnswer>? answers)
    {
        if (answers == null || answers.Count != fields.Count || !fields.Keys.All(answers.ContainsKey))
        {
            throw Invalid(answers, "Extraction must return exactly one answer for every field.");
        }

        foreach (var pair in fields)
        {
            var id = pair.Key;
            var field = pair.Value;
            var answer = answers[id];
            if (answer is { Type: "refusal" })
            {
                continue;
            }

            if (answer == null || answer.Type != field.Type)
            {
                throw Invalid(answers, "Field \"" + id + "\" returned an answer with the wrong type.");
            }

            if ((answer.PNull is { } pNull && !IsProbability(pNull)) || (answer.Confidence is { } confidence && !IsProbability(confidence)))
            {
                throw Invalid(answers, "Field \"" + id + "\" must return pNull and confidence as finite probabilities in [0, 1].");
            }

            if (field.Type == "list")
            {
                ValidateList(answers, id, field, answer, state);
                continue;
            }

            var numeric = field.Type is "number" or "duration";
            if (answer.TextItems != null || answer.NumberItems != null || answer.Items != null || (numeric ? answer.Text != null : answer.Number != null))
            {
                throw Invalid(answers, "Field \"" + id + "\" must return null or a valid " + field.Type + " value.");
            }

            if (numeric ? answer.Number != null && !IsValue(field, answer.Number.Value) : answer.Text != null && !IsValue(field, answer.Text))
            {
                throw Invalid(answers, "Field \"" + id + "\" must return null or a valid " + field.Type + " value.");
            }

            if ((numeric ? answer.GuessText != null || (answer.GuessNumber != null && !IsValue(field, answer.GuessNumber.Value)) : answer.GuessNumber != null || (answer.GuessText != null && !IsValue(field, answer.GuessText))))
            {
                throw Invalid(answers, "Field \"" + id + "\" must return a valid " + field.Type + " guess.");
            }

            if (answer.Source != null && !IsSource(answer.Source, state))
            {
                throw Invalid(answers, "Field \"" + id + "\" returned a source outside the state.");
            }
        }
    }

    private static void ValidateList(IReadOnlyDictionary<string, ExtractionAnswer> answers, string id, ExtractionField field, ExtractionAnswer answer, IReadOnlyList<DecisionModelStatePart> state)
    {
        var number = field.Items == "number";
        var item = number ? new ExtractionField("number") { Integer = field.Integer, Decimals = field.Decimals } : new ExtractionField(field.Items!);
        var misplaced = number ? answer.TextItems != null : answer.NumberItems != null;
        var count = number ? answer.NumberItems?.Count : answer.TextItems?.Count;
        var valid = !misplaced
            && answer.Text == null
            && answer.Number == null
            && answer.Source == null
            && (number ? answer.NumberItems == null || answer.NumberItems.All(entry => IsValue(item, entry)) : answer.TextItems == null || answer.TextItems.All(entry => IsValue(item, entry)));
        if (!valid)
        {
            throw Invalid(answers, "Field \"" + id + "\" must return null or a list of " + field.Items + " values.");
        }

        if (answer.Items != null
            && !(count != null
                && answer.Items.Count == count
                && answer.Items.All(entry => entry != null && (entry.Confidence == null || IsProbability(entry.Confidence.Value)) && (entry.Source == null || IsSource(entry.Source, state)))))
        {
            throw Invalid(answers, "Field \"" + id + "\" must return one item entry per list value, with confidences in [0, 1] and sources inside the state.");
        }
    }

    private static InvalidResponseDataException Invalid(object? answers, string message)
    {
        return new InvalidResponseDataException(answers, message);
    }

    private static bool IsProbability(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0 && value <= 1;
    }

    // A calendar date in YYYY-MM-DD form that exists, including leap days.
    private static bool IsDate(string value)
    {
        var match = DatePattern.Match(value);
        if (!match.Success)
        {
            return false;
        }

        var year = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var month = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var day = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        if (month < 1 || month > 12 || day < 1)
        {
            return false;
        }

        var leap = (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        var days = month switch
        {
            2 => leap ? 29 : 28,
            4 or 6 or 9 or 11 => 30,
            _ => 31,
        };
        return day <= days;
    }

    private static bool IsNumber(ExtractionField field, double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return false;
        }

        if (field.Integer == true && Math.Floor(value) != value)
        {
            return false;
        }

        // Number(value.toFixed(decimals)) === value
        return field.Decimals is not { } decimals
            || double.Parse(value.ToString("F" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) == value;
    }

    private static bool IsValue(ExtractionField field, double value)
    {
        return field.Type switch
        {
            "number" => IsNumber(field, value),
            "duration" => IsNumber(field, value) && value >= 0,
            _ => false,
        };
    }

    private static bool IsValue(ExtractionField field, string? value)
    {
        // A null list entry is not a string, so it is never a valid text, date, time, or datetime value.
        if (value is null)
        {
            return false;
        }

        try
        {
            return field.Type switch
            {
                "text" => true,
                "date" => IsDate(value),
                "time" => Time.IsMatch(value),
                "datetime" => Datetime.IsMatch(value) && IsDate(value.Substring(0, 10)),
                _ => false,
            };
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static bool IsSource(ExtractionSource source, IReadOnlyList<DecisionModelStatePart> state)
    {
        if (source.Part < 0 || source.Part >= state.Count)
        {
            return false;
        }

        var part = state[source.Part];
        switch (source.Type)
        {
            case "text":
                return part is DecisionModelTextPart text
                    && source.Start is { } start
                    && source.End is { } end
                    && start >= 0
                    && start < end
                    && end <= text.Text.Length;
            case "file":
                return part is DecisionModelFilePart;
            default:
                return false;
        }
    }
}
