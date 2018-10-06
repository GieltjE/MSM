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

using System.Windows.Forms;
using MSM.Data;
using MSM.Functions;

namespace MSM.Service
{
    public static class Events
    {
        public static event ExtensionMethods.CustomDelegate ShutDownFired;
        public static void ShutDown()
        {
            Variables.ShutDownFired = true;
            ShutDownFired?.Invoke();

            Application.Exit();
        }
    }
}