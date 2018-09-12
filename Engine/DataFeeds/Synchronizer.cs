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
 *
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Logging;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    ///
    /// </summary>
    public class Synchronizer : IEnumerable<TimeSlice>, ITimeProvider
    {
        private readonly SubscriptionSynchronizer _subscriptionSynchronizer;
        private readonly DataManager _subscriptionManager;
        private readonly IAlgorithm _algorithm;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        ///
        /// </summary>
        public ITimeProvider TimeProvider;

        /// <summary>
        ///
        /// </summary>
        public Synchronizer(UniverseSelection universeSelection,
                            IAlgorithm algorithm,
                            DataManager subscriptionManager,
                            IDataFeed dataFeed,
                            CancellationTokenSource cancellationTokenSource)

        {
            _cancellationTokenSource = cancellationTokenSource;
            _subscriptionManager = subscriptionManager;
            _algorithm = algorithm;

            var frontierUtc = DateTime.MaxValue; // TODO GetInitialFrontierTime()
            if (algorithm.LiveMode)
            {
                TimeProvider = new RealTimeProvider();
            }
            else
            {
                TimeProvider = new SubscriptionFrontierTimeProvider(frontierUtc, subscriptionManager);
            }
            _subscriptionSynchronizer = new SubscriptionSynchronizer(universeSelection, algorithm.TimeZone, algorithm.Portfolio.CashBook, TimeProvider);
            _subscriptionSynchronizer.SubscriptionFinished += (sender, subscription) =>
            {
                dataFeed.RemoveSubscription(subscription.Configuration);
                Log.Debug($"FileSystemDataFeed.SubscriptionFinished(): Finished subscription: {subscription.Configuration} at {_algorithm.UtcTime} UTC");
            };
        }



        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TimeSlice> GetEnumerator()
        {
            var previousDateTime = DateTime.MaxValue;
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                TimeSlice timeSlice;

                try
                {
                    timeSlice = _subscriptionSynchronizer.Sync(_subscriptionManager.DataFeedSubscriptions);
                }
                catch (Exception err)
                {
                    Log.Error(err);

                    // notify the algorithm about the error, so it can be reported to the user
                    _algorithm.RunTimeError = err;
                    _algorithm.Status = AlgorithmStatus.RuntimeError;

                    break;
                }

                if(_algorithm.LiveMode) // TODO

                // SubscriptionFrontierTimeProvider will return twice the same time if there are no more subscriptions or if Subscription.Current is null
                if (timeSlice.Time != previousDateTime)
                {
                    previousDateTime = timeSlice.Time;
                    yield return timeSlice;
                }
                else if (timeSlice.SecurityChanges == SecurityChanges.None)
                {
                    // there's no more data to pull off, we're done (frontier is max value and no security changes)
                    break;
                }
            }

            //Close up all streams:
            foreach (var subscription in _subscriptionManager.DataFeedSubscriptions)
            {
                subscription.Dispose();
            }

            Log.Trace(string.Format("FileSystemDataFeed.Run(): Data Feed Completed at {0} UTC", _algorithm.UtcTime));
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public DateTime GetUtcNow()
        {
            return _subscriptionSynchronizer.GetUtcNow();
        }
    }
}
