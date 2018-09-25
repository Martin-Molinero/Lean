/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Util;

namespace QuantConnect.Data.Auxiliary
{
    public static class DataTypes
    {
        /// <summary>
        /// Hard code the set of default available data feeds
        /// </summary>
        public static Dictionary<SecurityType, List<TickType>> Default()
        {
            return new Dictionary<SecurityType, List<TickType>>
            {
                {SecurityType.Base, new List<TickType>() { TickType.Trade } },
                {SecurityType.Forex, new List<TickType>() { TickType.Quote } },
                {SecurityType.Equity, new List<TickType>() { TickType.Trade } },
                {SecurityType.Option, new List<TickType>() { TickType.Quote, TickType.Trade, TickType.OpenInterest } },
                {SecurityType.Cfd, new List<TickType>() { TickType.Quote } },
                {SecurityType.Future, new List<TickType>() { TickType.Quote, TickType.Trade, TickType.OpenInterest } },
                {SecurityType.Commodity, new List<TickType>() { TickType.Trade } },
                {SecurityType.Crypto, new List<TickType>() { TickType.Trade, TickType.Quote } },
            };
        }

        /// <summary>
        /// Get the data feed types for a given <see cref="SecurityType"/> <see cref="Resolution"/>
        /// </summary>
        /// <param name="symbolSecurityType">The <see cref="SecurityType"/> used to determine the types</param>
        /// <param name="resolution">The resolution of the data requested</param>
        /// <param name="isCanonical">Indicates whether the security is Canonical (future and options)</param>
        /// <param name="availableDataTypes"></param>
        /// <returns>Types that should be added to the <see cref="SubscriptionDataConfig"/></returns>
        public static List<Tuple<Type, TickType>> LookupSubscriptionConfigDataTypes(Dictionary<SecurityType, List<TickType>> availableDataTypes,
                                                                                    SecurityType symbolSecurityType, Resolution resolution, bool isCanonical)
        {
            if (isCanonical)
            {
                return new List<Tuple<Type, TickType>> { new Tuple<Type, TickType>(typeof(ZipEntryName), TickType.Quote) };
            }

            return availableDataTypes[symbolSecurityType].Select(tickType => new Tuple<Type, TickType>(LeanData.GetDataType(resolution, tickType), tickType)).ToList();
        }
    }
}
