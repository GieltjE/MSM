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
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MSM.Data
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean FlushFileBuffers(SafeFileHandle hFile);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TVITEM lParam);

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        internal struct TVITEM
        {
            public Int32 mask;
            public IntPtr hItem;
            public Int32 state;
            public Int32 stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpszText;
            public Int32 cchTextMax;
            public Int32 iImage;
            public Int32 iSelectedImage;
            public Int32 cChildren;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            public Int32 Left, Top, Right, Bottom;

            private Rect(Int32 left, Int32 top, Int32 right, Int32 bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public Rect(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }
        }
    }
}