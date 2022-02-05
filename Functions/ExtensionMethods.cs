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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSM.Data;
using MSM.Service;

namespace MSM.Functions;

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

    public static void AutoEndInvoke(this IAsyncResult result, ISynchronizeInvoke control)
    {
        // http://blogs.msdn.com/b/pfxteam/archive/2012/03/25/10287435.aspx
        Task endInvokeTask = Task.Factory.StartNew(() => { AutoEndInvokeTask(result, control); }, TaskCreationOptions.PreferFairness);
        endInvokeTask.ContinueWith(t => HandleException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
    }
    private static void AutoEndInvokeTask(IAsyncResult result, ISynchronizeInvoke control)
    {
        try
        {
            control.EndInvoke(result);
        }
        catch {}
    }
    private static void HandleException(AggregateException exception)
    {
        foreach (Exception exception1 in exception.InnerExceptions)
        {
            Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Error, "Could not end invoke", exception1);
        }
    }

    public static Boolean Equals(this String[] inputArray, String[] compareToArray)
    {
        if (inputArray.Length != compareToArray.Length) return false;

        return inputArray.All(input => compareToArray.Contains(input, StringComparer.Ordinal));
    }
    public static String Replace(this String source, String oldString, String newString, StringComparison stringComparison)
    {
        List<Int32> indexes = source.GetAllIndexes(oldString, stringComparison, false);
        indexes.Reverse();
        foreach (Int32 index in indexes)
        {
            source = source.Remove(index, oldString.Length).Insert(index, newString);
        }
        return source;
    }
    public static List<Int32> GetAllIndexes(this String input, String search, StringComparison comparison = StringComparison.Ordinal, Boolean allowOverlap = true)
    {
        List<Int32> indexes = new();

        Int32 lastIndex = 0;
        while (true)
        {
            lastIndex = input.IndexOf(search, lastIndex, comparison);
            if (lastIndex == -1) return indexes;

            indexes.Add(lastIndex);
            if (!allowOverlap)
            {
                lastIndex += search.Length;
            }
            else
            {
                lastIndex++;
            }
        }
    }

    private static void WaitHelper(Object semaphoreSlim) => ((SemaphoreSlim)semaphoreSlim).Wait();
    public static void WaitUIFriendly(this SemaphoreSlim semaphoreSlim)
    {
        //if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        if (Thread.CurrentThread.ManagedThreadId == Variables.MainSTAThreadID)
        {
            // See if we can get in a reasonable amount of time
            if (semaphoreSlim.Wait(Variables.ThreadAfterDoEventsSleep))
            {
                return;
            }

            // Start a wait thread instead of looping and doing something like Application.DoEvents();
            ThreadHelpers thread = new();
            thread.ExecuteThreadParameter(WaitHelper, semaphoreSlim);
        }
        else
        {
            semaphoreSlim.Wait();
        }
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