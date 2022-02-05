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
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using MSM.Data;
using MSM.Functions;

namespace MSM.Extends;

public class FileStreamOptimized : FileStream
{
    public FileStreamOptimized(String path, FileMode mode) : base(path, mode) {}
    public FileStreamOptimized(String path, FileMode mode, FileAccess access) : base(path, mode, access) {}
    public FileStreamOptimized(String path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share) {}
    public FileStreamOptimized(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize) : base(path, mode, access, share, bufferSize) {}
    public FileStreamOptimized(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize, FileOptions options) : base(path, mode, access, share, bufferSize, options) {}
    public FileStreamOptimized(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize, Boolean useAsync) : base(path, mode, access, share, bufferSize, useAsync) {}
    public FileStreamOptimized(String path, FileMode mode, FileSystemRights rights, FileShare share, Int32 bufferSize, FileOptions options, FileSecurity fileSecurity) : base(path, mode, rights, share, bufferSize, options, fileSecurity) {}
    public FileStreamOptimized(String path, FileMode mode, FileSystemRights rights, FileShare share, Int32 bufferSize, FileOptions options) : base(path, mode, rights, share, bufferSize, options) {}
    public FileStreamOptimized(SafeFileHandle handle, FileAccess access) : base(handle, access) {}
    public FileStreamOptimized(SafeFileHandle handle, FileAccess access, Int32 bufferSize) : base(handle, access, bufferSize) {}
    public FileStreamOptimized(SafeFileHandle handle, FileAccess access, Int32 bufferSize, Boolean isAsync) : base(handle, access, bufferSize, isAsync) {}

    private Boolean _disposed;
    protected override void Dispose(Boolean disposing)
    {
        if (!_disposed)
        {
            Flush();
            Flush(true);

            Int32 retries = 0;
            retry:
            if (SafeFileHandle != null)
            {
                if (!NativeMethods.FlushFileBuffers(SafeFileHandle))
                {
                    Thread.Sleep(25);
                    if (retries < 9)
                    {
                        retries++;
                        goto retry;
                    }
                }
            }
        }

        _disposed = true;
        base.Dispose(disposing);

        retry2:
        if (!File.Exists(Name) || FileOperations.IsFileLocked(Name))
        {
            Thread.Sleep(25);
            goto retry2;
        }
    }
    public new void Dispose()
    {
        if (!_disposed)
        {
            Flush();
            Flush(true);

            retry:
            if (SafeFileHandle != null)
            {
                if (!NativeMethods.FlushFileBuffers(SafeFileHandle))
                {
                    Thread.Sleep(25);
                    goto retry;
                }
            }
        }

        _disposed = true;
        base.Dispose();

        retry2:
        if (!File.Exists(Name) || FileOperations.IsFileLocked(Name))
        {
            Thread.Sleep(25);
            goto retry2;
        }
    }
}