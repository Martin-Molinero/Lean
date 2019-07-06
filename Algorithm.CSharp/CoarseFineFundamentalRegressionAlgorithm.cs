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
using QuantConnect.Data.Fundamental;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Demonstration of how to define a universe
    /// as a combination of use the coarse fundamental data and fine fundamental data
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="universes" />
    /// <meta name="tag" content="coarse universes" />
    /// <meta name="tag" content="regression test" />
    public class CoarseFineFundamentalRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private const int NumberOfSymbolsFine = 1;
        private List<Symbol> _symbols;
        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2017, 01, 01);
            SetEndDate(2018, 01, 01);
            SetCash(50000);

            // this add universe method accepts two parameters:
            // - coarse selection function: accepts an IEnumerable<CoarseFundamental> and returns an IEnumerable<Symbol>
            // - fine selection function: accepts an IEnumerable<FineFundamental> and returns an IEnumerable<Symbol>
            AddUniverse(CoarseSelectionFunction, FineSelectionFunction);

            _symbols = new List<Symbol>();
            foreach (var symbol in new[] {
                "AAAP","AMZN","IBM","AIG",
                "CSCO","MSFT","ADBE","ABTLE",
                "ABXA","ACA","ABY", "ACFC",
                "ABTX","ACAW","ABUS", "A",
                "AC","ACB","WMT","ACCOB",
                "ACBI","ABT","ABX","ACER",
                "ACFC", "ACET", "ABTL", "ACAD",
                "ACE", "ACGLO", "AA", "ACCO",
                "ACGLP", "ACH", "ACHV", "ACL",
                "ACC", "ACLY", "AABA", "ACIU", // 40
                "ACIA", "ACHC", "AAC", "ACGL",
                "ACMGP", "ACMR", "ACHN", "ACM",
                "ACLS", "AAL", //"ACP", "ACIW",
                //"AAMC", "ACRS", "ACNB", "ACSF",
                //"ACTA", "ACT", "ACN", "ABFS",
                //"ACTYD", "ACTY", "ACRE", "ABG",
                //"ACV", "ACOR", "ACRX", "ACST",
                //"AAME", "AAN", "AANA","AAOI",
                //"AAON", "ACXM", "ACU", "AAP",
                //"AAS", "ACTG", "AAT", "AATK",
                //"ACY", "AAU", "AAV", "AAVL",
                //"AAWW", "AAXN", "AB", "ABAC",
                //"ABAX", "ABB", "ABBV", "ABC",
                //"ABCB", "ABCD", "ABCO", "ABD",
                //"ABDC", "ABE", "ABEO", "ABEV"
            })
            {
                _symbols.Add(
                    QuantConnect.Symbol.Create(symbol, SecurityType.Equity, Market.USA));
            }
        }

        // return a list of three fixed symbol objects
        public IEnumerable<Symbol> CoarseSelectionFunction(IEnumerable<CoarseFundamental> coarse)
        {
            return _symbols;
        }

        // sort the data by P/E ratio and take the top 'NumberOfSymbolsFine'
        public IEnumerable<Symbol> FineSelectionFunction(IEnumerable<FineFundamental> fine)
        {
            // sort descending by P/E ratio
            var sortedByPeRatio = fine.OrderByDescending(x => x.ValuationRatios.PERatio);

            // take the top entries from our sorted collection
            var topFine = sortedByPeRatio.Take(NumberOfSymbolsFine);

            // we need to return only the symbol objects
            return topFine.Select(x => x.Symbol);
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "2"},
            {"Average Win", "1.39%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "40.038%"},
            {"Drawdown", "1.400%"},
            {"Expectancy", "0"},
            {"Net Profit", "1.394%"},
            {"Sharpe Ratio", "3.081"},
            {"Loss Rate", "0%"},
            {"Win Rate", "100%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.033"},
            {"Beta", "19.023"},
            {"Annual Standard Deviation", "0.096"},
            {"Annual Variance", "0.009"},
            {"Information Ratio", "2.904"},
            {"Tracking Error", "0.096"},
            {"Treynor Ratio", "0.016"},
            {"Total Fees", "$2.00"}
        };
    }
}
