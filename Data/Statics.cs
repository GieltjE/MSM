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
using MSM.Extends;
using MSM.Service;

namespace MSM.Data;

public static class Statics
{
    public static readonly Random Random = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
    public static readonly NewtonsoftJsonSerializer NewtonsoftJsonSerializer = new();
    public static InformationObjectManager InformationObjectManager;
    public static Double FormOpacityFade = .93;
}