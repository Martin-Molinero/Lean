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
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using MathNet.Numerics;
using System.Threading.Tasks;
using System.Threading;

namespace QuantConnect.Algorithm.CSharp
{
    public class BasicTemplateOptionsAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        public override void Initialize()
        {
            SetStartDate(2024, 7, 2);

            var etf = AddEquity("IWV").Symbol;
            var universe = AddUniverse(Universe.ETF(etf, universeFilterFunc: (_) => Enumerable.Empty<Symbol>()));
            var eTFConstituentUniverse = History(universe, 1, Resolution.Daily).SelectMany(x => x.Data).Cast<ETFConstituentUniverse>().ToList();
            var start = DateTime.UtcNow;
            var emptyHistory = 0;
            var totalIVCount = 0;
            Parallel.ForEach(eTFConstituentUniverse.OrderBy(x => x.Weight).Select(x => x.Symbol).Take(200),
                //new ParallelOptions { MaxDegreeOfParallelism = 1 },
                (underlying =>
            {
                var dividendYieldModel = new DividendYieldProvider(underlying);
                foreach (var symbol in OptionChainProvider.GetOptionContractList(underlying, Time))
                {
                    var oppositeRight = symbol.ID.OptionRight == OptionRight.Call ? OptionRight.Put : OptionRight.Call;
                    var mirrorOption = QuantConnect.Symbol.CreateOption(underlying, symbol.ID.Market, symbol.ID.OptionStyle, oppositeRight, symbol.ID.StrikePrice, symbol.ID.Date);
                    var impliedVolatility = new ImpliedVolatility(symbol, mirrorOption: mirrorOption, riskFreeRateModel: RiskFreeInterestRateModel, dividendYieldModel: dividendYieldModel, period: 1
                        //, optionModel: OptionPricingModelType.ForwardTree
                        //, optionModel: OptionPricingModelType.BinomialCoxRossRubinstein
                        );
                    var history = IndicatorHistory(impliedVolatility, new[] { symbol, mirrorOption, underlying }, 1, resolution: Resolution.Daily);
                    Interlocked.Increment(ref totalIVCount);
                    if (history.Count == 0)
                    {
                        Interlocked.Increment(ref emptyHistory);
                        //Log($"Empty history for {symbol} & {mirrorOption}");
                    }
                }
            }));

            var percentage = 0m;
            if (ImpliedVolatility.TotalCallCount != 0)
            {
                percentage = ((ImpliedVolatility.SecondExceptionFailure * 1m) / ImpliedVolatility.TotalCallCount) * 100;
            }
            Quit($"Took: {DateTime.UtcNow - start} FirstExceptionFailure {ImpliedVolatility.FirstExceptionFailure} SecondExceptionFailure {ImpliedVolatility.SecondExceptionFailure}. TotalCount: {ImpliedVolatility.TotalCallCount}. Percent: {percentage.Round(2)}%." +
                $" EmptyIndicatorHistory {emptyHistory} out of {totalIVCount}");
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public List<Language> Languages { get; } = new() { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 471124;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// Final status of the algorithm
        /// </summary>
        public AlgorithmStatus AlgorithmStatus => AlgorithmStatus.Completed;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Orders", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "0%"},
            {"Drawdown", "0%"},
            {"Expectancy", "0"},
            {"Start Equity", "100000"},
            {"End Equity", "99718"},
            {"Net Profit", "0%"},
            {"Sharpe Ratio", "0"},
            {"Sortino Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "0"},
            {"Tracking Error", "0"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$2.00"},
            {"Estimated Strategy Capacity", "$1300000.00"},
            {"Lowest Capacity Asset", "GOOCV 30AKMEIPOSS1Y|GOOCV VP83T1ZUHROL"},
            {"Portfolio Turnover", "10.71%"},
            {"OrderListHash", "8a36462ee0349c04d01d464e592dd347"}
        };
    }
}
