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
using System.Globalization;
using System.IO;
using NodaTime;
using QuantConnect.Util;

namespace QuantConnect.Data.UniverseSelection
{
    /// <summary>
    /// ETF Constituent data
    /// </summary>
    [Obsolete("'ETFConstituentData' was renamed to 'ETFConstituentUniverse'")]
    public class ETFConstituentData : ETFConstituentUniverse { }

    /// <summary>
    /// ETF constituent data
    /// </summary>
    public class ETFConstituentUniverse : BaseDataCollection
    {
        /// <summary>
        /// Time of the previous ETF constituent data update
        /// </summary>
        public DateTime? LastUpdate { get; set; }
        
        /// <summary>
        /// The percentage of the ETF allocated to this constituent
        /// </summary>
        public decimal? Weight { get; set; }
        
        /// <summary>
        /// Number of shares held in the ETF
        /// </summary>
        public decimal? SharesHeld { get; set; }
        
        /// <summary>
        /// Market value of the current asset held in U.S. dollars
        /// </summary>
        public decimal? MarketValue { get; set; }

        /// <summary>
        /// Period of the data
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Time that the data became available to use
        /// </summary>
        public override DateTime EndTime
        {
            get { return Time + Period; }
            set { Time = value - Period; }
        }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    config.SecurityType.SecurityTypeToLower(),
                    config.Market,
                    "universes",
                    "etf",
                    config.Symbol.Underlying.Value.ToLowerInvariant(),
                    $"{date:yyyyMMdd}.csv"),
                SubscriptionTransportMedium.LocalFile,
                FileFormat.FoldingCollection);
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object
        /// each time it is called.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            var split = line.Split(',');

            var symbol = new Symbol(SecurityIdentifier.Parse(split[1]), split[0]);
            var lastUpdateDate = Parse.TryParseExact(split[2], "yyyyMMdd", DateTimeStyles.None, out var lastUpdateDateParsed)
                ? lastUpdateDateParsed
                : (DateTime?)null;
            var weighting = split[3].IsNullOrEmpty()
                ? (decimal?)null
                : Parse.Decimal(split[3], NumberStyles.Any);
            var sharesHeld = split[4].IsNullOrEmpty()
                ? (decimal?)null
                : Parse.Decimal(split[4], NumberStyles.Any);
            var marketValue = split[5].IsNullOrEmpty()
                ? (decimal?)null
                : Parse.Decimal(split[5], NumberStyles.Any);

            return new ETFConstituentUniverse
            {
                LastUpdate = lastUpdateDate,
                Weight = weighting,
                SharesHeld = sharesHeld,
                MarketValue = marketValue,

                Symbol = symbol,
                Time = date
            };
        }

        /// <summary>
        /// Indicates if there is support for mapping
        /// </summary>
        /// <returns>True indicates mapping should be used</returns>
        public override bool RequiresMapping()
        {
            return true;
        }

        /// <summary>
        /// Creates a copy of the instance
        /// </summary>
        /// <returns>Clone of the instance</returns>
        public override BaseData Clone()
        {
            return new ETFConstituentUniverse
            {
                LastUpdate = LastUpdate,
                Weight = Weight,
                SharesHeld = SharesHeld,
                MarketValue = MarketValue,

                Symbol = Symbol,
                Time = Time,
                Data = Data
            };
        }

        /// <summary>
        /// Indicates that the data set is expected to be sparse
        /// </summary>
        /// <remarks>Relies on the <see cref="Symbol"/> property value</remarks>
        /// <remarks>This is a method and not a property so that python
        /// custom data types can override it</remarks>
        /// <returns>True if the data set represented by this type is expected to be sparse</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        /// <remarks>
        /// This is a method and not a property so that python
        /// custom data types can override it.
        /// </remarks>
        public override Resolution DefaultResolution()
        {
            return Resolution.Daily;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        /// <remarks>Relies on the <see cref="Symbol"/> property value</remarks>
        /// <remarks>This is a method and not a property so that python
        /// custom data types can override it</remarks>
        public override List<Resolution> SupportedResolutions()
        {
            return DailyResolution;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <remarks>Will throw <see cref="InvalidOperationException"/> for security types
        /// other than <see cref="SecurityType.Base"/></remarks>
        /// <returns>The <see cref="DateTimeZone"/> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return TimeZones.Utc;
        }
    }
}
