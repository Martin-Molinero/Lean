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

using System.Collections;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    /// DataManager will manage the data feed
    /// </summary>
    public class DataManager : IDataFeed
    {
        private readonly IDataFeed _dataFeed;

        /// <summary>
        /// Creates a new instance of the DataManager class
        /// </summary>
        public DataManager(IDataFeed dataFeed)
        {
            _dataFeed = dataFeed;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public IEnumerator<TimeSlice> GetEnumerator()
        {
            return _dataFeed.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets all of the current subscriptions this data feed is processing
        /// </summary>
        public IEnumerable<Subscription> Subscriptions => _dataFeed.Subscriptions;

        /// <summary>
        /// Public flag indicator that the thread is still busy.
        /// </summary>
        public bool IsActive => _dataFeed.IsActive;

        /// <summary>
        /// Initializes the data feed for the specified job and algorithm
        /// </summary>
        public void Initialize(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler,
            IMapFileProvider mapFileProvider, IFactorFileProvider factorFileProvider, IDataProvider dataProvider)
        {
            _dataFeed.Initialize(algorithm, job, resultHandler, mapFileProvider, factorFileProvider, dataProvider);
        }

        /// <summary>
        /// Adds a new subscription to provide data for the specified security.
        /// </summary>
        /// <param name="request">Defines the subscription to be added, including start/end times the universe and security</param>
        /// <returns>True if the subscription was created and added successfully, false otherwise</returns>
        public bool AddSubscription(SubscriptionRequest request)
        {
            return _dataFeed.AddSubscription(request);
        }

        /// <summary>
        /// Removes the subscription from the data feed, if it exists
        /// </summary>
        /// <param name="configuration">The configuration of the subscription to remove</param>
        /// <returns>True if the subscription was successfully removed, false otherwise</returns>
        public bool RemoveSubscription(SubscriptionDataConfig configuration)
        {
            return _dataFeed.RemoveSubscription(configuration);
        }

        /// <summary>
        /// Primary entry point.
        /// </summary>
        public void Run()
        {
            _dataFeed.Run();
        }

        /// <summary>
        /// External controller calls to signal a terminate of the thread.
        /// </summary>
        public void Exit()
        {
            _dataFeed.Exit();
        }
    }
}
