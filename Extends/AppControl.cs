// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MSM.Data;
using MSM.UIElements;

namespace MSM.Extends
{
    public class AppControl : ControlOptimized
    {
        // This is a simplified/butchered app control, Chrome doesn't like being pushed around
        public AppControl(TerminalControl terminal, String executable, IEnumerable<String> arguments, Dictionary<String, String> environmentVariables, IntPtr windowHandle)
        {
            _terminal = terminal;
            _windowHandle = windowHandle;
            _environment = environmentVariables;

            if (File.Exists(executable))
            {
                FileInfo fileInfo = new(executable);
                if (fileInfo.Directory != null)
                {
                    _path = fileInfo.Directory.ToString();
                    _executable = executable;
                    _parameters = arguments.Aggregate("", (current, argument) => current + " " + argument).Trim();
                }
            }

            Padding = new Padding(0);

            SizeChanged += OnSizeChanged;
        }
        ~AppControl()
        {
            Stop();
            Dispose();
        }

        protected void OnSizeChanged(Object s, EventArgs eventArgs)
        {
            if (ChildHandle != IntPtr.Zero && Width != 0 && Height != 0)
            {
                NativeMethods.MoveWindow(ChildHandle, 0, 0, Width, Height, true);
            }

            Invalidate();
        }

        public void Stop(Object sender, CancelEventArgs e)
        {
            Stop();
        }
        internal void Stop()
        {
            if (!_iscreated || _isdisposed || ChildHandle == IntPtr.Zero || _childProcess.HasExited)
            {
                return;
            }

            _iscreated = false;
            _isdisposed = true;
            NativeMethods.SetParent(ChildHandle, IntPtr.Zero);
            ChildHandle = IntPtr.Zero;

            _childProcess.Exited -= ChildProcessExited;
            _childProcess.Refresh();
            _childProcess.Kill();
            _childProcess?.Dispose();
        }

        private readonly TerminalControl _terminal;
        private Boolean _iscreated;
        private Boolean _isdisposed;
        internal IntPtr ChildHandle = IntPtr.Zero;
        private Process _childProcess;
        private static IntPtr _windowHandle = IntPtr.Zero;
        private readonly String _executable;
        private readonly String _parameters;
        private readonly Dictionary<String, String> _environment;
        private readonly String _path;
        public Boolean LoadSuccess;

        internal void Load()
        {
            try
            {
                ProcessStartInfo procInfo = new(_executable)
                {
                    WorkingDirectory = _path,
                    Arguments = _parameters,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Maximized,
                    UseShellExecute = false,
                    ErrorDialogParentHandle = Variables.MainForm.Handle, 
                };
                foreach (KeyValuePair<String, String> keyValuePair in _environment)
                {
                    procInfo.EnvironmentVariables[keyValuePair.Key] = keyValuePair.Value;
                }

                _childProcess = Process.Start(procInfo);
                if (_childProcess == null)
                {
                    LoadSuccess = false;
                    return;
                }

                _childProcess.WaitForInputIdle();

                while (_childProcess.MainWindowHandle == IntPtr.Zero)
                {
                    Thread.Sleep(25);
                }

                // Set window handle to us
                NativeMethods.SetParent(_childProcess.MainWindowHandle, _windowHandle);
                // Maximize the window
                NativeMethods.ShowWindow(_childProcess.MainWindowHandle, NativeMethods.ShowWindowCommands.Maximize);

                // Remove UI elements
                IntPtr windowStyle = NativeMethods.GetWindowLongArchitectureInvariant(_childProcess.MainWindowHandle, (Int32)NativeMethods.GWL.GWL_STYLE);
                IntPtr windowStyleOriginal = windowStyle;

                Int32 style = windowStyle.ToInt32();
                style &= ~((Int32)NativeMethods.WindowStyles.WS_CAPTION | (Int32)NativeMethods.WindowStyles.WS_SIZEBOX);
                windowStyle = new IntPtr(style);

                // Attach handle to our form and apply the window style
                IntPtr result = NativeMethods.SetWindowLongArchitectureInvariant(new HandleRef(this, _childProcess.MainWindowHandle), (Int32)NativeMethods.GWL.GWL_STYLE, windowStyle);
                if (result == IntPtr.Zero || windowStyleOriginal != result)
                {
                    Service.Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not set window style!", null);
                }

                //OnSizeChanged(null, null);

                _childProcess.EnableRaisingEvents = true;
                _childProcess.Exited += ChildProcessExited;
                ChildHandle = _childProcess.MainWindowHandle;
            }
            catch (Exception exception)
            {
                Service.Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not load AppControl!", exception);
                LoadSuccess = false;
                return;
            }

            OnSizeChanged(null, null);

            _iscreated = true;
            _isdisposed = false;
            LoadSuccess = true;
        }
        
        private void ChildProcessExited(Object sender, EventArgs e)
        {
            Service.Events.OnProcessExited(_terminal);
        }
    }
}