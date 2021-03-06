﻿#region Licence
/*
   Copyright 2016 Miha Strehar

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
#endregion

namespace Feri.MS.Http
{
    /// <summary>
    /// This is data class for IPFilter class.
    /// </summary>
    internal class IpNumber
    {
        internal byte[] IPAddress { get; set; }
        internal byte[] IPAddressUpper { get; set; }
        internal byte[] IPAddressLower { get; set; }
    }
}
