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
using QuantConnect.Data;
using QuantConnect.Interfaces;
using System.Collections.Generic;
using QuantConnect.Data.Auxiliary;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    /// 
    /// </summary>
    public class ZipCollectionSubscriptionDataSourceReader : ISubscriptionDataSourceReader
    {

        private readonly DateTime _date;
        private readonly bool _isLiveMode;
        private readonly BaseData _factory;
        private readonly SubscriptionDataConfig _config;
        private readonly IDataCacheProvider _dataCacheProvider;

        /// <summary>
        /// Event fired when the specified source is considered invalid, this may
        /// be from a missing file or failure to download a remote source
        /// </summary>
        public event EventHandler<InvalidSourceEventArgs> InvalidSource;

        /// <summary>
        /// Event fired when an exception is thrown during a call to
        /// <see cref="BaseData.Reader(SubscriptionDataConfig, string, DateTime, bool)"/>
        /// </summary>
        public event EventHandler<ReaderErrorEventArgs> ReaderError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipCollectionSubscriptionDataSourceReader"/> class
        /// </summary>
        /// <param name="dataCacheProvider">Used to cache data for requested from the IDataProvider</param>
        /// <param name="config">The subscription's configuration</param>
        /// <param name="date">The date this factory was produced to read data for</param>
        /// <param name="isLiveMode">True if we're in live mode, false for backtesting</param>
        public ZipCollectionSubscriptionDataSourceReader(IDataCacheProvider dataCacheProvider, SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            _dataCacheProvider = dataCacheProvider;
            _date = date;
            _config = config;
            _isLiveMode = isLiveMode;
            _factory = _config.GetBaseDataInstance();
        }

        /// <summary>
        /// Reads the specified <paramref name="source"/>
        /// </summary>
        /// <param name="source">The source to be read</param>
        /// <returns>An <see cref="IEnumerable{BaseData}"/> that contains the data in the source</returns>
        public IEnumerable<BaseData> Read(SubscriptionDataSource source)
        {
            // TODO: 'ZipEntryNameSubscriptionDataSourceReader' should use the cache provider
            var zipEntryNameReader = new ZipEntryNameSubscriptionDataSourceReader(new SubscriptionDataConfig(_config, typeof(ZipEntryName)), _date, _isLiveMode);
            foreach (var entryName in zipEntryNameReader.Read(source))
            {
                var innerConfig = new SubscriptionDataConfig(_config, symbol: entryName.Symbol);
                var innerReader = new TextSubscriptionDataSourceReader(_dataCacheProvider, innerConfig, _date, _isLiveMode);
                foreach (var entryDataPoint in innerReader.Read(_factory.GetSource(innerConfig, _date, _isLiveMode)))
                {
                    // TODO: the different open interest dpts have different time
                    entryDataPoint.Time = entryDataPoint.Time.RoundDown(TimeSpan.FromDays(1));
                    // TODO: there can be more that 1 open interest dpt per symbol
                    yield return entryDataPoint;
                }
            }
        }
    }
}
