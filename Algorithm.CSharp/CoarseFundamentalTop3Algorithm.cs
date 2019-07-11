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

using QuantConnect.Interfaces;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// In this algorithm we demonstrate how to use the coarse fundamental data to
    /// define a universe as the top dollar volume
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="universes" />
    /// <meta name="tag" content="coarse universes" />
    /// <meta name="tag" content="regression test" />
    public class CoarseFundamentalTop3Algorithm : QCAlgorithm
    {
        private const int NumberOfSymbols = 3;

        // initialize our changes to nothing
        private SecurityChanges _changes = SecurityChanges.None;

        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2010, 01, 1);
            SetEndDate(2017, 01, 1);
            SetCash(50000);

            // this add universe method accepts a single parameter that is a function that
            // accepts an IEnumerable<CoarseFundamental> and returns IEnumerable<Symbol>
            AddUniverse(CoarseSelectionFunction);
        }

        // sort the data by daily dollar volume and take the top 'NumberOfSymbols'
        public static IEnumerable<Symbol> CoarseSelectionFunction(IEnumerable<CoarseFundamental> coarse)
        {
            // sort descending by daily dollar volume
            var sortedByDollarVolume = coarse.OrderByDescending(x => x.DollarVolume);

            // take the top entries from our sorted collection
            var top = sortedByDollarVolume.Take(NumberOfSymbols);

            // we need to return only the symbol objects
            return top.Select(x => x.Symbol);
        }
    }
}
