﻿/*
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

using Newtonsoft.Json;

namespace QuantConnect.Orders
{
    /// <summary>
    /// The purpose of this class is to store time and price information
    /// available at the time an order was submitted.
    /// </summary>
    public class OrderSubmissionData
    {
        /// <summary>
        /// The bid price at order submission time
        /// </summary>
        [JsonProperty(PropertyName = "bidPrice")]
        public decimal BidPrice { get; }

        /// <summary>
        /// The ask price at order submission time
        /// </summary>
        [JsonProperty(PropertyName = "askPrice")]
        public decimal AskPrice { get; }

        /// <summary>
        /// The current price at order submission time
        /// </summary>
        [JsonProperty(PropertyName = "lastPrice")]
        public decimal LastPrice { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSubmissionData"/> class
        /// </summary>
        /// <remarks>This method is currently only used for testing.</remarks>
        public OrderSubmissionData(decimal bidPrice, decimal askPrice, decimal lastPrice)
        {
            BidPrice = bidPrice;
            AskPrice = askPrice;
            LastPrice = lastPrice;
        }

        /// <summary>
        /// Return a new instance clone of this object
        /// </summary>
        public OrderSubmissionData Clone()
        {
            return (OrderSubmissionData)MemberwiseClone();
        }
    }
}
