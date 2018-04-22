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
using System.IO;
using Newtonsoft.Json;

namespace MSM.Extends
{
    public class NewtonsoftJsonSerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        public NewtonsoftJsonSerializer()
        {
            _jsonSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Include };
        }

        public String Serialize(Object obj)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _jsonSerializer.Serialize(jsonTextWriter, obj);
                    return stringWriter.ToString();
                }
            }
        }
        public T Deserialize<T>(String input)
        {
            using (StringReader stringReader = new StringReader(input))
            {
                using (JsonTextReader jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }
}