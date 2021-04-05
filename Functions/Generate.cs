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
using System.Text;

namespace MSM.Functions
{
    public static class Generate
    {
        static Generate()
        {
            Int32 itterations = StaticRandom.Next(100, 1000);
            for (Int32 i = 0; i < itterations; i++)
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                StaticRandom.Next(1, 10);
            }
        }

        public static readonly Random StaticRandom = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
        public static String RandomPassword(UInt32 length)
        {
            StringBuilder builder = new();
            for (UInt32 i = 0; i < length; i++)
            {
                builder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(26 * StaticRandom.NextDouble() + 65))));
            }
            return builder.ToString();
        }
        public static String RandomUniqueID(UInt32 maxLength)
        {
            String uniqueID = RandomUniqueID();
            if (uniqueID.Length > maxLength)
            {
                uniqueID = uniqueID.Substring(0, (Int32)maxLength);
            }
            return uniqueID;
        }
        public static String RandomUniqueID()
        {
            return Guid.NewGuid().ToString("N");
        }
        public static Guid RandomGuid()
        {
            return Guid.NewGuid();
        }
    }
}