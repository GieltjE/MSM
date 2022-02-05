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
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using MSM.Data;

namespace MSM.Service;

public class InformationObjectManager : IDisposable
{
    private SafeJobHandle _handle;
    private Boolean _disposed;

    public InformationObjectManager()
    {
        _handle = new SafeJobHandle(NativeMethods.CreateJobObject(IntPtr.Zero, null));
        SetHandle(NativeMethods.JobObjectLimitFlags.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE);
    }

    private void SetHandle(NativeMethods.JobObjectLimitFlags flags)
    {
        NativeMethods.JOBOBJECT_BASIC_LIMIT_INFORMATION info = new()
        {
            LimitFlags = (UInt32)flags,
        };
        NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new() { BasicLimitInformation = info };

        Int32 length = Marshal.SizeOf(typeof(NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
        IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

        if (!NativeMethods.SetInformationJobObject(_handle, NativeMethods.JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (UInt32)length))
        {
            Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not initialise child process manager: " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture), null);
        }
    }
    public void Dispose()
    {
        if (_disposed) return;

        SetHandle(NativeMethods.JobObjectLimitFlags.JOB_NONE);

        _handle.Close();
        _handle.Dispose();
        _handle = null;
        _disposed = true;
    }
        
    public void AddProcess(SafeProcessHandle processHandle)
    {
        if (!NativeMethods.AssignProcessToJobObject(_handle, processHandle))
        {
            Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not add the process to the child process manager", null);
        }
    }
}