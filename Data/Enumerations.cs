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
using System.ComponentModel;

namespace MSM.Data
{
    public static class Enumerations
    {
        [Flags]
        public enum CronMonth
        {
            All = 1,
            January = 2,
            February = 4,
            March = 8,
            April = 16,
            May = 32,
            June = 64,
            Juli = 128,
            Augustus = 256,
            September = 512,
            October = 1024,
            November = 2048,
            December = 4096
        }
        public static readonly String[] CronMonthString =
        {
            "*",
            "jan",
            "feb",
            "mar",
            "apr",
            "may",
            "jun",
            "jul",
            "aug",
            "sep",
            "oct",
            "nov",
            "dec"
        };

        [Flags]
        public enum CronDayOfTheWeek : byte
        {
            All = 1,
            Monday = 2,
            Tuesday = 4,
            Wednesday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64,
            Sunday = 128
        }
        public static readonly String[] CronDayOfTheWeekString =
        {
            "?",
            "mon",
            "tue",
            "wed",
            "thu",
            "fri",
            "sat",
            "sun"
        };

        public enum Themes : byte
        {
            Light,
            Blue,
            Dark
        }

        public enum CloseAction : byte
        {
            [Description("Close normally")]
            Close,
            [Description("Minimize")]
            Minimize,
            [Description("Minimize to the tray")]
            MinimizeToTray,
        }

        public enum InitialSessions : byte
        {
            [Description("None")]
            None,
            [Description("Previous sessions")]
            Previous,
            [Description("Predefined")]
            Predefined,
        }

        public enum CheckedListBoxSetting : byte
        {
            ServerKeywords
        }
    }
}