// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2022 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MSM.Functions;

public static class NumericalOperations
{
    [DebuggerStepThrough]
    public static Boolean IsNumeric(this String input, Boolean allowNegative, Boolean performBoundsChecking = true)
    {
        if (String.IsNullOrWhiteSpace(input)) return true;
        // Linq doesn't seem to impact this
        if (!allowNegative)
        {
            Boolean firstTest = input.All(c => c is >= '0' and <= '9');
            if (!firstTest)
            {
                return false;
            }

            if (input.Length <= 1)
            {
                return true;
            }

            if (performBoundsChecking && input.Trim('0').Length > 0)
            {
                return ToUInt64(input) != 0;
            }
            return true;
        }

        if (input.Length == 1 && input[0] == '-')
        {
            return false;
        }
        if (input.IndexOf('-') != input.LastIndexOf('-'))
        {
            return false;
        }
        input = input.TrimStart('-');
        Boolean firstTest2 = input.All(c => c is >= '0' and <= '9');
        if (!firstTest2)
        {
            return false;
        }
        if (input.Length <= 1)
        {
            return true;
        }

        if (performBoundsChecking && input.Trim('0').Length > 0)
        {
            return ToInt64(input) != 0;
        }
        return true;
    }
    [DebuggerStepThrough]
    public static Boolean IsNumeric(this Char input)
    {
        return input is >= '0' and <= '9';
    }

    // Do not input non numerical characters, that wil yield very strange results
    [DebuggerStepThrough]
    public static SByte ToSbyte(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            SByte result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            Byte start = 0;
            if (input[0] == '-')
            {
                start = 1;
            }

            for (Int32 i = start; i < input.Length; i++)
            {
                result = (SByte)((10 * result) + (input[i] - 48));
            }
            if (start == 1)
            {
                return (SByte)(-result);
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static Int16 ToInt16(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            Int16 result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            Byte start = 0;
            if (input[0] == '-')
            {
                start = 1;
            }

            for (Int32 i = start; i < input.Length; i++)
            {
                result = (Int16)((10 * result) + (input[i] - 48));
            }
            if (start == 1)
            {
                return (Int16)(-result);
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static Int32 ToInt32(this Char c)
    {
        return Convert.ToInt32(c) - Convert.ToInt32('0');
    }
    [DebuggerStepThrough]
    public static Int32 ToInt32(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            Int32 result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            Byte start = 0;
            if (input[0] == '-')
            {
                start = 1;
            }

            for (Int32 i = start; i < input.Length; i++)
            {
                result = (10 * result) + (input[i] - 48);
            }
            if (start == 1)
            {
                return -result;
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static Int64 ToInt64(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            Int64 result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            Byte start = 0;
            if (input[0] == '-')
            {
                start = 1;
            }

            for (Int32 i = start; i < input.Length; i++)
            {
                result = (10 * result) + (input[i] - 48);
            }
            if (start == 1)
            {
                return -result;
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }

    [DebuggerStepThrough]
    public static Byte ToByte(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            Byte result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (Int32 i = 0; i < input.Length; i++)
            {
                result = (Byte)(((10 * result) + input[i]) - 48);
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static UInt16 ToUInt16(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            UInt16 result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (Int32 i = 0; i < input.Length; i++)
            {
                result = (UInt16)(((10 * result) + input[i]) - 48);
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static UInt32 ToUInt32(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            UInt32 result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (Int32 i = 0; i < input.Length; i++)
            {
                result = (10 * result) + (UInt32)(input[i] - 48);
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static UInt64 ToUInt64(this String input)
    {
        // Warning some functions rely on the stupid conversion of text to a number... (e.g. BME import)
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            UInt64 result = 0;
            // DO NOT convert to LINQ, much slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (Int32 i = 0; i < input.Length; i++)
            {
                result = (10 * result) + (UInt64)(input[i] - 48);
            }
            return result;
        }
        catch
        {
            return 0;
        }
    }

    [DebuggerStepThrough]
    public static Decimal ToDecimal(this String input)
    {
        if (String.IsNullOrEmpty(input)) return 0;
        try
        {
            return Convert.ToDecimal(input, CultureInfo.CurrentUICulture);
        }
        catch
        {
            return 0;
        }
    }
    [DebuggerStepThrough]
    public static Decimal ToDecimal(this Double input)
    {
        try
        {
            return Convert.ToDecimal(input, CultureInfo.CurrentUICulture);
        }
        catch
        {
            return 0;
        }
    }
    private static Regex _decimalMatch;
    [DebuggerStepThrough]
    public static Boolean IsDecimal(this String testDecimal, UInt32 maxLength, Byte maxDecimalCount, Boolean allowNegative, Boolean performBoundsChecking = true, Boolean performDatabaseStyle = true)
    {
        if (performDatabaseStyle)
        {
            // Calculate values the MariaDB/MySQL way (7,2 is 5 length with 2 decimals!)
            if (maxLength != UInt32.MaxValue && maxDecimalCount != Byte.MaxValue)
            {
                maxLength -= maxDecimalCount;
            }
        }

        _decimalMatch ??= new Regex("^[0-9" + CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator + "]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout.InfiniteTimeSpan);

        if (!_decimalMatch.Match(testDecimal).Success) return false;

        if (testDecimal.IndexOf(CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal) != testDecimal.LastIndexOf(CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal))
        {
            return false;
        }
        if (allowNegative && testDecimal.IndexOf('-') != testDecimal.LastIndexOf('-'))
        {
            return false;
        }
        if (testDecimal.Length == 1 && testDecimal[0] == '-')
        {
            return false;
        }

        if (testDecimal.IndexOf(',') != -1)
        {
            if (performBoundsChecking)
            {
                String[] results = testDecimal.Split(',');
                String firstTest = results[0];
                if (allowNegative && firstTest.StartsWith("-"))
                {
                    // Only remove the first character
                    firstTest = firstTest.Substring(1);
                }
                if (firstTest.Length > maxLength || results[1].Length > maxDecimalCount)
                {
                    return false;
                }

                if (!IsNumeric(results[0], allowNegative))
                {
                    return false;
                }
                if (!IsNumeric(results[1], false))
                {
                    return false;
                }
            }
        }
        else
        {
            if (testDecimal.Length > maxLength)
            {
                if (allowNegative && testDecimal.StartsWith("-"))
                {
                    if (testDecimal.Length > maxLength + 1)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!IsNumeric(testDecimal, allowNegative, performBoundsChecking))
            {
                return false;
            }
        }

        if (!Decimal.TryParse(testDecimal, NumberStyles.Number, CultureInfo.CurrentUICulture, out Decimal result))
        {
            return false;
        }
        return result >= 0 || allowNegative;
    }
}