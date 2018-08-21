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

using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Data
{
    /// <summary>
    /// Extensions used to improve usability, and reduce code duplication, of the SubscriptionDataConfig
    /// </summary>
    public static class SubscriptionDataConfigExtensions
    {
        /// <summary>
        /// Determines if all subscriptions for the security are internal feeds
        /// </summary>
        /// <returns>True, all subscriptions matching symbol are internal</returns>
        public static bool IsInternalFeed(this IEnumerable<SubscriptionDataConfig> subscriptions, Symbol symbol)
        {
            return subscriptions.Where(x => x.Symbol == symbol).IsInternalFeed();
        }

        /// <summary>
        /// Determines if all subscriptions are internal feeds
        /// </summary>
        /// <returns>True, all subscriptions are internal feeds</returns>
        public static bool IsInternalFeed(this IEnumerable<SubscriptionDataConfig> subscriptions)
        {
            return subscriptions.All(x => x.IsInternalFeed);
        }

        /// <summary>
        /// Determines the highest resolution from all of the symbols data config subscriptions
        /// </summary>
        /// <returns>Highest subscription resolution. Defaults to Daily if no subscription</returns>
        public static Resolution GetHighestSubscriptionResolution(this IEnumerable<SubscriptionDataConfig> subscriptions, Symbol symbol)
        {
            return subscriptions.Where(x => x.Symbol == symbol).GetHighestSubscriptionResolution();
        }

        /// <summary>
        /// Determines the highest resolution from all data config subscriptions
        /// </summary>
        /// <returns>Highest subscription resolution. Defaults to Daily if no subscription</returns>
        public static Resolution GetHighestSubscriptionResolution(this IEnumerable<SubscriptionDataConfig> subscriptions, Resolution defaultResolution = Resolution.Daily)
        {
            return subscriptions.Select(x => x.Resolution)
                                .DefaultIfEmpty(defaultResolution)
                                .Min();
        }

        /// <summary>
        /// Determines base on symbol subscriptions if the security will continue feeding data after the primary market hours have closed.
        /// </summary>
        /// <returns>True, indicates the symbol will continue feeding data after the primary market hours have closed.</returns>
        public static bool IsExtendedMarketHours(this IEnumerable<SubscriptionDataConfig> subscriptions, Symbol symbol)
        {
            return subscriptions.Where(x => x.Symbol == symbol).IsExtendedMarketHours();
        }

        /// <summary>
        /// Determines base on subscriptions if the security will continue feeding data after the primary market hours have closed.
        /// </summary>
        /// <returns>True, indicates the security will continue feeding data after the primary market hours have closed.</returns>
        public static bool IsExtendedMarketHours(this IEnumerable<SubscriptionDataConfig> subscriptions)
        {
            return subscriptions.Any(x => x.ExtendedMarketHours);
        }

        /// <summary>
        /// Determines base on security subscriptions the data normalization mode used for this security.
        /// </summary>
        /// <returns>The DataNormalizationMode of the first subscription found. Defaults to Adjusted if no subscription</returns>
        public static DataNormalizationMode DataNormalizationMode(this IEnumerable<SubscriptionDataConfig> subscriptions, Symbol symbol)
        {
            return subscriptions.Where(x => x.Symbol == symbol)
                                .DataNormalizationMode();
        }

        /// <summary>
        /// Determines base on subscriptions the data normalization mode used for these SubscriptionDataConfig.
        /// </summary>
        /// <returns>The DataNormalizationMode of the first subscription found. Defaults to Adjusted if no subscription</returns>
        public static DataNormalizationMode DataNormalizationMode(this IEnumerable<SubscriptionDataConfig> subscriptions)
        {
            return subscriptions.Select(x => x.DataNormalizationMode)
                                .DefaultIfEmpty(QuantConnect.DataNormalizationMode.Adjusted)
                                .FirstOrDefault();
        }
    }
}
