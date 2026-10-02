// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;

namespace Vercel.AI.Util;

/// <summary>Deep equality for JSON-like data, including dates and prototype identity.</summary>
public static class DeepEqual
{
    /// <summary>Returns whether <paramref name="left"/> and <paramref name="right"/> are deeply equal.</summary>
    public static bool IsDeepEqualData(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is JsUndefined && right is JsUndefined)
        {
            return true;
        }

        if (left == null || right == null || left is JsUndefined || right is JsUndefined)
        {
            return false;
        }

        if (IsPrimitive(left) && IsPrimitive(right))
        {
            if (IsNumber(left) && IsNumber(right))
            {
                return Convert.ToDouble(left, System.Globalization.CultureInfo.InvariantCulture)
                    == Convert.ToDouble(right, System.Globalization.CultureInfo.InvariantCulture);
            }

            return Equals(left, right);
        }

        if (IsPrimitive(left) || IsPrimitive(right))
        {
            return false;
        }

        if (left is Delegate || right is Delegate)
        {
            return false;
        }

        if (left.GetType() != right.GetType())
        {
            return false;
        }

        if (left is DateTime && right is DateTime)
        {
            return ((DateTime)left).Ticks == ((DateTime)right).Ticks;
        }

        var leftObject = left as DataObject;
        var rightObject = right as DataObject;
        if (leftObject != null || rightObject != null)
        {
            if (leftObject == null || rightObject == null)
            {
                return false;
            }

            if (!Equals(leftObject.Constructor, rightObject.Constructor))
            {
                return false;
            }

            return DictionariesEqual(leftObject.Properties, rightObject.Properties);
        }

        var leftList = left as IList;
        var rightList = right as IList;
        if (leftList != null || rightList != null)
        {
            if (leftList == null || rightList == null || leftList.Count != rightList.Count)
            {
                return false;
            }

            for (var i = 0; i < leftList.Count; i++)
            {
                if (!IsDeepEqualData(leftList[i], rightList[i]))
                {
                    return false;
                }
            }

            return true;
        }

        var leftMap = left as IDictionary;
        var rightMap = right as IDictionary;
        if (leftMap != null && rightMap != null)
        {
            return DictionariesEqual(leftMap, rightMap);
        }

        return false;
    }

    private static bool DictionariesEqual(IDictionary left, IDictionary right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var key in left.Keys)
        {
            if (!right.Contains(key))
            {
                return false;
            }

            if (!IsDeepEqualData(left[key], right[key]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsPrimitive(object value)
    {
        return value is string || value is bool || IsNumber(value);
    }

    private static bool IsNumber(object value)
    {
        return value is byte || value is sbyte || value is short || value is ushort
            || value is int || value is uint || value is long || value is ulong
            || value is float || value is double || value is decimal;
    }
}
