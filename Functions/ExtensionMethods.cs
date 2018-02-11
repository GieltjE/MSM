// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2018 Michiel Hazelhof (michiel@hazelhof.nl)
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSM.Functions
{
    public static class ExtensionMethods
    {
        [DebuggerStepThrough]
        public static IEnumerable<T> GetAllFlags<T>(this T value) where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(flag => value.IsFlagSet(flag));
        }
        [DebuggerStepThrough]
        public static T SetFlags<T>(this T value, T flags, Boolean on) where T : struct
        {
            Int64 lValue = Convert.ToInt64(value);
            Int64 lFlag = Convert.ToInt64(flags);
            if (on)
            {
                lValue |= lFlag;
            }
            else
            {
                lValue &= ~lFlag;
            }
            return (T)Enum.ToObject(typeof(T), lValue);
        }
        [DebuggerStepThrough]
        public static Boolean IsFlagSet<T>(this T value, T flag) where T : struct
        {
            Int64 lValue = Convert.ToInt64(value);
            Int64 lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }
        [DebuggerStepThrough]
        public static T SetFlags<T>(this T value, T flags) where T : struct
        {
            return value.SetFlags(flags, true);
        }
        [DebuggerStepThrough]
        public static T ClearFlags<T>(this T value, T flags) where T : struct
        {
            return value.SetFlags(flags, false);
        }
        [DebuggerStepThrough]
        public static T CombineFlags<T>(this IEnumerable<T> flags) where T : struct
        {
            return (T)Enum.ToObject(typeof(T), flags.Select(flag => Convert.ToInt64(flag)).Aggregate<Int64, Int64>(0, (current, lFlag) => current | lFlag));
        }

        public static Int32 IndexOf(this String[] input, String toFind)
        {
            for (Int32 i = 0; i < input.Length; i++)
            {
                if (String.Equals(input[i], toFind, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        public delegate void CustomDelegate();
        public delegate void CustomDelegate<in TEventArgs>(TEventArgs t);
        public delegate void CustomDelegate<in TEventArgs, in TEventArgs2>(TEventArgs t, TEventArgs2 t2);
        public delegate void CustomDelegate<in TEventArgs, in TEventArgs2, in TEventArgs3>(TEventArgs t, TEventArgs2 t2, TEventArgs3 t3);
        public delegate void CustomDelegate<in TEventArgs, in TEventArgs2, in TEventArgs3, in TEventArgs4>(TEventArgs t, TEventArgs2 t2, TEventArgs3 t3, TEventArgs4 t4);
        public delegate void CustomDelegate<in TEventArgs, in TEventArgs2, in TEventArgs3, in TEventArgs4, in TEventArgs5>(TEventArgs t, TEventArgs2 t2, TEventArgs3 t3, TEventArgs4 t4, TEventArgs5 t5);
    }

    public static class Enum<T>
    {
        public static Int32 Length()
        {
            return Enum.GetNames(typeof(T)).Length;
        }
        public static Int32 IndexOf(T enum1)
        {
            return Enum.GetNames(typeof(T)).IndexOf(enum1.ToString());
        }
        public static T GetByIndex(Int32 index)
        {
            return Enum.GetValues(typeof(T)).OfType<T>().ToArray()[index];
        }
    }
}