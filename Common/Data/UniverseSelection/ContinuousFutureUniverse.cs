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
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Securities.Future;

namespace QuantConnect.Data.UniverseSelection
{
    public class ContinuousFutureUniverse : Universe, ITimeTriggeredUniverse
    {
        private IFutureChainProvider _chainProvider;
        private Future _future;
        public override UniverseSettings UniverseSettings { get; }

        public ContinuousFutureUniverse(Future future, UniverseSettings universeSettings, IFutureChainProvider chainProvider)
            : base(new SubscriptionDataConfig(future.SubscriptionDataConfig, symbol: Symbol.CreateFuture($"{future.Symbol.ID.Symbol}-CONTINUOUS-UNIVERSE", future.Symbol.ID.Market, SecurityIdentifier.DefaultDate)))
        {
            _chainProvider = chainProvider;
            UniverseSettings = universeSettings;
            _future = future;
        }

        public override IEnumerable<Symbol> SelectSymbols(DateTime utcTime, BaseDataCollection data)
        {
            var chain = _chainProvider.GetFutureContractList(_future.Symbol, utcTime);
            // 'FutureFilterUniverse' could be provided by the user
            // volume roll style? history?
            var selectedFutureContract = new FutureFilterUniverse(chain, data).Expiration(5, 100).FrontMonth().SingleOrDefault();
            if (selectedFutureContract == null)
            {
                return Unchanged;
            }

            return new[] { selectedFutureContract };
        }

        public override IEnumerable<SubscriptionRequest> GetSubscriptionRequests(
            Security security,
            DateTime currentTimeUtc,
            DateTime maximumEndTimeUtc,
            ISubscriptionDataConfigService subscriptionService
            )
        {
            var result = subscriptionService.Add(security.Symbol,
                UniverseSettings.Resolution,
                UniverseSettings.FillForward,
                UniverseSettings.ExtendedMarketHours,
                dataNormalizationMode: UniverseSettings.DataNormalizationMode);

            // check if this is a new addition and not a remove call
            if (_future.Underlying == null || security.Symbol.ID.Date > _future.Underlying.Symbol.ID.Date)
            {
                // we keep the underlying rolling, allow the user to access the current real data contract symbol
                _future.Underlying = security;
                // we update our continuous future price cache to the new securities
                //_future.Cache = security.Cache;
            }

            return result.SelectMany(config =>
            {
                return new[] { security, _future }.Select(targetSecurity => new SubscriptionRequest(
                      isUniverseSubscription: false,
                      universe: this,
                      // This is where the magic happens, the symbol in config is the real actual contract but we push the update to the canonical security
                      security: targetSecurity,
                      configuration: config,
                      startTimeUtc: currentTimeUtc,
                      endTimeUtc: maximumEndTimeUtc));
            });
        }

        /// <summary>
        /// Each tradeable day of the future we trigger a new selection. Allows use to update the current contract
        /// </summary>
        public IEnumerable<DateTime> GetTriggerTimes(DateTime startTimeUtc, DateTime endTimeUtc, MarketHoursDatabase marketHoursDatabase)
        {
            var startTimeLocal = startTimeUtc.ConvertFromUtc(Configuration.ExchangeTimeZone);
            var endTimeLocal = endTimeUtc.ConvertFromUtc(Configuration.ExchangeTimeZone);

            return Time.EachTradeableDay(_future, startTimeLocal, endTimeLocal);
        }
    }
}
