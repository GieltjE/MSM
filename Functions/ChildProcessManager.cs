using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using MSM.Data;
using MSM.Service;

namespace MSM.Functions
{
    public sealed class ChildProcessManager : IDisposable
    {
        private SafeJobHandle _handle;
        private Boolean _disposed;

        public ChildProcessManager()
        {
            _handle = new SafeJobHandle(NativeMethods.CreateJobObject(IntPtr.Zero, null));

            NativeMethods.JOBOBJECT_BASIC_LIMIT_INFORMATION info = new() {
                LimitFlags = (UInt32)NativeMethods.JobObjectLimitFlags.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE,
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
}