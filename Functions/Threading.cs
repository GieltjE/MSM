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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using MSM.Data;
using MSM.Service;

namespace MSM.Functions
{
    public class Threading
    {
        internal Thread MasterThread;
        public ThreadStart ThreadStartFunction;
        public ParameterizedThreadStart ParameterizedThreadStartFunction;

        internal void StartThread(Boolean staThread, Object threadObject = null, ThreadPriority priority = ThreadPriority.Normal)
        {
            MasterThread = threadObject == null ? new Thread(ThreadStartFunction) { Priority = priority } : new Thread(ParameterizedThreadStartFunction) { Priority = priority };
#if DEBUG
            if (String.IsNullOrEmpty(MasterThread.Name))
            {
                try
                {
                    if (threadObject == null)
                    {
                        String name = ThreadStartFunction.Method.Name;
                        if (ThreadStartFunction.Target != null)
                        {
                            name += "|" + ThreadStartFunction.Target;
                        }
                        MasterThread.Name = name;
                    }
                    else
                    {
                        String name = ParameterizedThreadStartFunction.Method.Name;
                        if (ParameterizedThreadStartFunction.Target != null)
                        {
                            name += "|" + ParameterizedThreadStartFunction.Target;
                        }
                        MasterThread.Name = name;
                    }
                }
                catch (Exception exception)
                {
                    Logging.LogErrorItem(exception);
                }
            }
#endif
            if (staThread)
            {
                MasterThread.SetApartmentState(ApartmentState.STA);
            }

            Boolean retried = false;
retry:
            try
            {
                if (threadObject != null)
                {
                    MasterThread.Start(threadObject);
                }
                else
                {
                    MasterThread.Start();
                }
            }
            catch (OutOfMemoryException)
            {
                if (retried) throw;

                retried = true;
                Thread.Sleep(250);
                goto retry;
            }
        }
    }

    public class ThreadHelpers
    {
        private Threading _thread;
        private Threading _waitThread;
        public event ExtensionMethods.CustomDelegate EventComplete;

        private void FireEventComplete()
        {
            EventComplete?.Invoke();
        }

#if DEBUG
        public void ExecuteThread(ThreadStart threadStart, Boolean waitForCompletion = true, Boolean doEvents = true, Boolean staThread = false, ThreadPriority priority = ThreadPriority.Normal, [CallerMemberName] String caller = null, [CallerLineNumber] Int32 lineNumber = 0)
#else
        public void ExecuteThread(ThreadStart threadStart, Boolean waitForCompletion = true, Boolean doEvents = true, Boolean staThread = false, ThreadPriority priority = ThreadPriority.Normal)
#endif
        {
            _thread = new Threading { ThreadStartFunction = threadStart };
            _thread.StartThread(staThread, null, priority);

            if (waitForCompletion)
            {
                if (staThread)
                {
                    _thread.MasterThread.Join();
                }
                else
                {
                    while (_thread.MasterThread != null && _thread.MasterThread.IsAlive && !Variables.ShutDownFired)
                    {
                        if (doEvents)
                        {
                            // Produces NPE when disposing
                            try
                            {
                                Application.DoEvents();
                            }
                            catch {}
                        }
                        Thread.Sleep(Variables.ThreadAfterDoEventsSleep);
                    }
                }

                FireEventComplete();
                return;
            }

            if (EventComplete == null) return;
            _waitThread = new Threading { ThreadStartFunction = WaitForCompletion };
            _waitThread.StartThread(false);
        }
        public void ExecuteThreadParameter(ParameterizedThreadStart threadStart, Object threadObject, Boolean waitForCompletion = true, Boolean doEvents = true, Boolean staThread = false, ThreadPriority priority = ThreadPriority.Normal)
        {
            _thread = new Threading { ParameterizedThreadStartFunction = threadStart };
            _thread.StartThread(staThread, threadObject, priority);

            if (waitForCompletion)
            {
                while (_thread.MasterThread != null && _thread.MasterThread.IsAlive && !Variables.ShutDownFired)
                {
                    if (doEvents) Application.DoEvents();
                    Thread.Sleep(Variables.ThreadAfterDoEventsSleep);
                }

                FireEventComplete();
                return;
            }

            if (EventComplete == null) return;
            _waitThread = new Threading { ThreadStartFunction = WaitForCompletion };
            _waitThread.StartThread(false);
        }

        private void WaitForCompletion()
        {
            while (_thread.MasterThread != null && _thread.MasterThread.IsAlive && !Variables.ShutDownFired)
            {
                Thread.Sleep(Variables.ThreadAfterDoEventsSleep);
            }

            FireEventComplete();
        }

        public void Abort()
        {
            try
            {
                _thread.MasterThread.Abort();
            }
            catch {}
        }
        public Boolean Completed()
        {
            if (_thread?.MasterThread == null)
            {
                return true;
            }
            return !_thread.MasterThread.IsAlive;
        }
    }
}