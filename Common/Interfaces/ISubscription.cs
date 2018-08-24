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

using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;

namespace QuantConnect.Interfaces
{
    /// <summary>
    /// Interface that allows a read view of a Subscription
    /// </summary>
    public interface ISubscription
    {
        /// <summary>
        /// Gets the security this subscription points to
        /// </summary>
        Security Security { get; }

        /// <summary>
        /// Gets the universe for this subscription
        /// </summary>
        Universe Universe { get; }

        /// <summary>
        /// Gets the configuration for this subscription
        /// </summary>
        SubscriptionDataConfig Configuration { get; }
    }
}
